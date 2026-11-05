// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using JPEmuRenamer.Emulation;
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;
using JPEmuRenamer.Ui;
using InputManager = JPEmuRenamer.Emulation.InputManager;

namespace JPEmuRenamer.Views;

/// <summary>内蔵コアでゲームを表示する画面（ウィンドウ/全画面、メッセージ、プレイ中のメニュー）</summary>
public partial class PlayWindow : Window
{
    private readonly PlayContext context;
    private readonly EmulatorSession session;
    private WriteableBitmap? bitmap;
    private int[] frame = Array.Empty<int>();
    private int frameW, frameH;
    private long serial;
    private PlaySettings settings;
    private RetroShaderEffect? effect;
    private string effectKind = "None";
    private bool fullscreen, modeChanged, started, closingRequested;
    private Rect windowedBounds;
    private readonly DispatcherTimer osdTimer = new() { Interval = TimeSpan.FromSeconds(2.5) };
    private readonly DispatcherTimer navTimer = new() { Interval = TimeSpan.FromMilliseconds(16) };
    private readonly Dictionary<string, (bool down, DateTime next)> navState = new();
    private bool sizedOnce;

    public PlayWindow(PlayContext context, SessionRequest request)
    {
        InitializeComponent();
        this.context = context;
        settings = request.Settings;
        Title = request.Title + " - JPEmu Renamer";
        StatusText.Text = Loc.F("CoreLoading", request.Title);
        StatusClose.Content = Loc.T("Close");

        session = new EmulatorSession(request);
        session.Started += () => Dispatcher.BeginInvoke(OnSessionStarted);
        session.Failed += msg => Dispatcher.BeginInvoke(() => ShowFailure(msg));
        session.Message += msg => Dispatcher.BeginInvoke(() => ShowOsd(msg));
        session.Hotkey += name => Dispatcher.BeginInvoke(() => OnHotkey(name));
        session.GeometryChanged += () => Dispatcher.BeginInvoke(UpdateLayoutSize);
        session.Stopped += () => Dispatcher.BeginInvoke(OnSessionStopped);

        osdTimer.Tick += (_, _) => { OsdBorder.Visibility = Visibility.Collapsed; osdTimer.Stop(); };
        navTimer.Tick += OnNavTick;

        ApplyVideoSettings();
        Loaded += (_, _) =>
        {
            CompositionTarget.Rendering += OnRendering;
            session.Start();
        };
    }

    // ---------- セッションの状態 ----------

    private void OnSessionStarted()
    {
        started = true;
        StatusPanel.Visibility = Visibility.Collapsed;
        if (settings.WindowMode == "Fullscreen") SetFullscreen(true);
        ShowOsd(Loc.F("MsgStarted", session.CoreDisplayName));
    }

    private void ShowFailure(string msg)
    {
        StatusPanel.Visibility = Visibility.Visible;
        StatusText.Text = Loc.F("CoreFailed", msg);
        StatusClose.Visibility = Visibility.Visible;
        if (fullscreen) SetFullscreen(false);
    }

    private void OnSessionStopped()
    {
        // 失敗の表示中でなければ閉じる（コアが自分で終了したとき）
        if (StatusClose.Visibility != Visibility.Visible && !closingRequested) Close();
    }

    private void OnStatusClose(object sender, RoutedEventArgs e) => Close();

    // ---------- 映像 ----------

    private void OnRendering(object? sender, EventArgs e)
    {
        if (!session.TryGetFrame(ref frame, out var w, out var h, ref serial)) return;
        if (bitmap == null || bitmap.PixelWidth != w || bitmap.PixelHeight != h)
        {
            bitmap = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgr32, null);
            Screen.Source = bitmap;
            frameW = w;
            frameH = h;
            UpdateLayoutSize();
        }
        bitmap.WritePixels(new Int32Rect(0, 0, w, h), frame, w * 4, 0);
    }

    /// <summary>表示するときの縦横比（設定と回転を反映）</summary>
    private double DisplayAspect()
    {
        double a = settings.Aspect switch
        {
            "4:3" => 4.0 / 3.0,
            _ => session.AspectRatio > 0 ? session.AspectRatio : (double)Math.Max(1, frameW) / Math.Max(1, frameH)
        };
        return session.Rotation % 2 == 1 ? 1 / a : a;
    }

    private void UpdateLayoutSize()
    {
        if (frameW == 0 || frameH == 0) return;
        bool rotated = session.Rotation % 2 == 1;
        Screen.LayoutTransform = session.Rotation == 0 ? Transform.Identity : new RotateTransform(-90 * session.Rotation);

        double availW = Root.ActualWidth, availH = Root.ActualHeight;
        if (availW <= 0 || availH <= 0) return;

        // ゲーム画面の高さ（回転後）を基準にする
        double baseH = rotated ? frameW : frameH;
        double aspect = DisplayAspect();
        double w, h;
        if (settings.Aspect == "Stretch")
        {
            w = availW;
            h = availH;
        }
        else
        {
            double scale = Math.Min(availW / (baseH * aspect), availH / baseH);
            if (settings.IntegerScale == true && scale >= 1) scale = Math.Floor(scale);
            h = baseH * scale;
            w = h * aspect;
        }
        // 回転しているときは回転前の大きさを指定する
        Screen.Width = Math.Max(1, rotated ? h : w);
        Screen.Height = Math.Max(1, rotated ? w : h);
        UpdateEffectSize();

        if (!sizedOnce && !fullscreen && started)
        {
            sizedOnce = true;
            SizeWindowToScale();
        }
    }

    /// <summary>ウィンドウを「等倍・2倍・3倍」に合わせる（画面に合わせる、のときは何もしない）</summary>
    private void SizeWindowToScale()
    {
        if (fullscreen || !int.TryParse(settings.ScaleMode, out var n) || frameH == 0) return;
        bool rotated = session.Rotation % 2 == 1;
        double baseH = (rotated ? frameW : frameH) * n;
        double contentW = baseH * DisplayAspect(), contentH = baseH;
        var work = SystemParameters.WorkArea;
        double chromeW = ActualWidth - Root.ActualWidth, chromeH = ActualHeight - Root.ActualHeight;
        double winW = Math.Min(contentW + chromeW, work.Width), winH = Math.Min(contentH + chromeH, work.Height);
        Width = winW;
        Height = winH;
        Left = work.Left + (work.Width - winW) / 2;
        Top = work.Top + (work.Height - winH) / 2;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => Dispatcher.BeginInvoke(UpdateLayoutSize, DispatcherPriority.Render);

    private void ApplyVideoSettings()
    {
        RenderOptions.SetBitmapScalingMode(Screen, settings.Filter == "Smooth" ? BitmapScalingMode.HighQuality : BitmapScalingMode.NearestNeighbor);
        var kind = settings.Shader ?? "None";
        if (kind != effectKind)
        {
            effectKind = kind;
            effect = kind == "None" ? null : RetroShaderEffect.Create(kind);
            Screen.Effect = effect;
            if (kind != "None" && effect == null) ShowOsd(Loc.T("ShaderUnavailable"));
        }
        UpdateLayoutSize();
    }

    private void UpdateEffectSize()
    {
        if (effect == null) return;
        var dpi = VisualTreeHelper.GetDpi(this);
        effect.Size = new Point4D(Math.Max(1, frameW), Math.Max(1, frameH),
            Screen.Width * dpi.DpiScaleX, Screen.Height * dpi.DpiScaleY);
    }

    // ---------- マウス（タッチ画面） ----------

    private void OnPointerDown(object sender, MouseButtonEventArgs e)
    {
        if (MenuLayer.Visibility == Visibility.Visible || StatusPanel.Visibility == Visibility.Visible) return;
        Root.CaptureMouse();
        UpdatePointer(e);
    }

    private void OnPointerUp(object sender, MouseButtonEventArgs e)
    {
        if (Root.IsMouseCaptured) Root.ReleaseMouseCapture();
        UpdatePointer(e);
    }

    private void OnPointerMove(object sender, MouseEventArgs e) => UpdatePointer(e);

    /// <summary>ゲーム画面上の位置を libretro の座標（-0x7FFF〜0x7FFF）にしてセッションに渡す</summary>
    private void UpdatePointer(MouseEventArgs e)
    {
        double w = Screen.ActualWidth, h = Screen.ActualHeight;
        if (w <= 0 || h <= 0) return;
        // 回転していても、Screen 自身の座標は回転前のゲーム画面の向き
        var p = e.GetPosition(Screen);
        bool inside = p.X >= 0 && p.Y >= 0 && p.X < w && p.Y < h && Root.IsMouseOver;
        short x = (short)Math.Clamp(p.X / w * 0xFFFE - 0x7FFF, -0x7FFF, 0x7FFF);
        short y = (short)Math.Clamp(p.Y / h * 0xFFFE - 0x7FFF, -0x7FFF, 0x7FFF);
        bool down = e.LeftButton == MouseButtonState.Pressed && MenuLayer.Visibility != Visibility.Visible;
        session.SetPointer(x, y, down, inside || Root.IsMouseCaptured);
    }

    // ---------- 全画面 ----------

    private void SetFullscreen(bool on)
    {
        if (on == fullscreen) return;
        if (on)
        {
            windowedBounds = new Rect(Left, Top, Width, Height);
            if (!string.IsNullOrEmpty(settings.FullscreenMode))
                modeChanged = DisplayInfo.ChangeMode(settings.FullscreenMode);
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Normal;
            WindowState = WindowState.Maximized;
            Topmost = true;
            Cursor = Cursors.None;
        }
        else
        {
            if (modeChanged)
            {
                DisplayInfo.RestoreMode();
                modeChanged = false;
            }
            Topmost = false;
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;
            Cursor = null;
            if (!windowedBounds.IsEmpty)
            {
                Left = windowedBounds.Left;
                Top = windowedBounds.Top;
                Width = windowedBounds.Width;
                Height = windowedBounds.Height;
            }
        }
        fullscreen = on;
        if (MenuLayer.Visibility == Visibility.Visible) Cursor = null;
        Dispatcher.BeginInvoke(UpdateLayoutSize, DispatcherPriority.Render);
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        // 画面モードを変えた全画面で他のアプリに切り替えたら、元の画面モードに戻す
        if (fullscreen && modeChanged) SetFullscreen(false);
    }

    // ---------- メッセージ ----------

    private void ShowOsd(string text)
    {
        OsdText.Text = text;
        OsdBorder.Visibility = Visibility.Visible;
        osdTimer.Stop();
        osdTimer.Start();
    }

    // ---------- ホットキー ----------

    private void OnHotkey(string name)
    {
        switch (name)
        {
            case "Menu":
                if (MenuLayer.Visibility == Visibility.Visible) CloseMenu(); else OpenMenu();
                break;
            case "Fullscreen":
                SetFullscreen(!fullscreen);
                break;
            case "Screenshot":
                TakeScreenshot();
                break;
            case "Exit":
                if (MenuLayer.Visibility == Visibility.Visible) CloseMenu();
                else if (fullscreen && settings.WindowMode != "Fullscreen") SetFullscreen(false);
                else Close();
                break;
        }
    }

    private void TakeScreenshot()
    {
        if (bitmap == null) return;
        try
        {
            Directory.CreateDirectory(session.ScreenshotDir);
            var path = System.IO.Path.Combine(session.ScreenshotDir,
                Matcher.SafeFileName(session.RomBaseName) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bitmap.Clone()));
            using (var fs = File.Create(path)) enc.Save(fs);
            ShowOsd(Loc.T("MsgScreenshot"));
        }
        catch (Exception ex)
        {
            ShowOsd(ex.Message);
        }
    }

    // ---------- プレイ中のメニュー ----------

    private record MenuItem(string Key, string Text)
    {
        public override string ToString() => Text;
    }

    private void OpenMenu()
    {
        if (!started) return;
        session.MenuPaused = true;
        MenuTitle.Text = session.Request.Title;
        MenuSub.Text = session.CoreDisplayName;
        MenuHint.Text = Loc.T("MenuHint");
        BuildMenu();
        MenuLayer.Visibility = Visibility.Visible;
        Cursor = null;
        MenuList.SelectedIndex = 0;
        navState.Clear();
        // 開いた瞬間のボタン（メニューを開いたボタンなど）を押しっぱなし扱いにする
        foreach (var k in new[] { "up", "down", "ok", "back" }) navState[k] = (true, DateTime.MaxValue);
        navTimer.Start();
    }

    private void BuildMenu()
    {
        int keep = Math.Max(0, MenuList.SelectedIndex);
        MenuList.Items.Clear();
        MenuList.Items.Add(new MenuItem("Resume", Loc.T("MenuResume")));
        MenuList.Items.Add(new MenuItem("SaveState", Loc.F("MenuSaveState", session.Slot)));
        MenuList.Items.Add(new MenuItem("LoadState", Loc.F("MenuLoadState", session.Slot)));
        MenuList.Items.Add(new MenuItem("SlotNext", Loc.F("MenuSlot", session.Slot)));
        MenuList.Items.Add(new MenuItem("Screenshot", Loc.T("MenuScreenshot")));
        MenuList.Items.Add(new MenuItem("Fullscreen", fullscreen ? Loc.T("MenuWindowed") : Loc.T("MenuFullscreen")));
        MenuList.Items.Add(new MenuItem("Settings", Loc.T("MenuSettings")));
        MenuList.Items.Add(new MenuItem("Reset", Loc.T("MenuReset")));
        MenuList.Items.Add(new MenuItem("Exit", Loc.T("MenuExit")));
        MenuList.SelectedIndex = Math.Min(keep, MenuList.Items.Count - 1);
    }

    private void CloseMenu()
    {
        navTimer.Stop();
        MenuLayer.Visibility = Visibility.Collapsed;
        if (fullscreen) Cursor = Cursors.None;
        session.MenuPaused = false;
    }

    private void OnMenuMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (MenuList.SelectedItem is MenuItem) ActivateMenuItem();
    }

    private void ActivateMenuItem()
    {
        if (MenuList.SelectedItem is not MenuItem item) return;
        switch (item.Key)
        {
            case "Resume":
                CloseMenu();
                break;
            case "SaveState":
                session.RequestSaveState();
                CloseMenu();
                break;
            case "LoadState":
                session.RequestLoadState();
                CloseMenu();
                break;
            case "SlotNext":
                session.ChangeSlot(1);
                Dispatcher.BeginInvoke(BuildMenu, DispatcherPriority.Background);
                break;
            case "Screenshot":
                CloseMenu();
                // メニューが消えてから撮る（映像はゲームの画面だけなので、すぐでもよい）
                TakeScreenshot();
                break;
            case "Fullscreen":
                SetFullscreen(!fullscreen);
                BuildMenu();
                break;
            case "Settings":
                OpenSettings();
                break;
            case "Reset":
                session.RequestReset();
                CloseMenu();
                break;
            case "Exit":
                Close();
                break;
        }
    }

    private void OpenSettings()
    {
        navTimer.Stop();
        bool wasFull = fullscreen;
        if (wasFull) Topmost = false;
        var dlg = new PlaySettingsWindow(context, context.GameKey != null ? "Game" : "System", session.DeclaredOptions)
        {
            Owner = this
        };
        dlg.SettingsChanged += ApplyContextSettings;
        dlg.ShowDialog();
        if (wasFull) Topmost = true;
        ApplyContextSettings();
        navState.Clear();
        foreach (var k in new[] { "up", "down", "ok", "back" }) navState[k] = (true, DateTime.MaxValue);
        navTimer.Start();
    }

    /// <summary>設定画面で変えた内容をこの画面とセッションに反映する</summary>
    private void ApplyContextSettings()
    {
        var old = settings;
        settings = context.Resolve();
        session.ApplySettings(settings);
        ApplyVideoSettings();
        if (old.ScaleMode != settings.ScaleMode && !fullscreen) SizeWindowToScale();
        if (old.WindowMode != settings.WindowMode) SetFullscreen(settings.WindowMode == "Fullscreen");
    }

    /// <summary>メニューをパッドやキーで操作する（押しっぱなしで連続して動く）</summary>
    private void OnNavTick(object? sender, EventArgs e)
    {
        if (!IsActive) return;
        var input = InputManager.Instance;
        var p1 = settings.Input?.Players.FirstOrDefault();
        bool Active(string control, params int[] keys) =>
            keys.Any(input.KeyDown) || (p1 != null && p1.Buttons.TryGetValue(control, out var l) && input.AnyActive(l, 0));

        bool up = Active("Up", 0x26), down = Active("Down", 0x28);
        bool ok = Active("A", 0x0D, 0x20), back = Active("B", 0x08);

        if (Edge("up", up, true)) Move(-1);
        if (Edge("down", down, true)) Move(1);
        if (Edge("ok", ok, false)) ActivateMenuItem();
        if (Edge("back", back, false)) CloseMenu();
    }

    private bool Edge(string key, bool now, bool repeat)
    {
        var t = DateTime.Now;
        navState.TryGetValue(key, out var st);
        if (!now)
        {
            navState[key] = (false, t);
            return false;
        }
        if (!st.down)
        {
            navState[key] = (true, t.AddMilliseconds(400));
            return true;
        }
        if (repeat && t >= st.next)
        {
            navState[key] = (true, t.AddMilliseconds(90));
            return true;
        }
        return false;
    }

    private void Move(int d)
    {
        int n = MenuList.Items.Count;
        if (n == 0) return;
        MenuList.SelectedIndex = ((MenuList.SelectedIndex + d) % n + n) % n;
        MenuList.ScrollIntoView(MenuList.SelectedItem);
    }

    // ---------- 終了 ----------

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        closingRequested = true;
        CompositionTarget.Rendering -= OnRendering;
        navTimer.Stop();
        if (modeChanged)
        {
            DisplayInfo.RestoreMode();
            modeChanged = false;
        }
        session.Stop();
    }

    private void OnClosed(object? sender, EventArgs e) => session.Dispose();
}
