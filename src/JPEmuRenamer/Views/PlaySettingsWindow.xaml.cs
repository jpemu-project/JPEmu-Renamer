// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using JPEmuRenamer.Emulation;
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;
using Microsoft.Win32;
using InputBinding = JPEmuRenamer.Models.InputBinding;
using InputManager = JPEmuRenamer.Emulation.InputManager;

namespace JPEmuRenamer.Views;

/// <summary>
/// 内蔵コアのプレイ設定（入力・保存先・画面・音声・コア固有）。「基本」「詳細」の2タブ。
/// 「全体→機種→ゲーム」の層を選んで編集し、上書きした項目には●を付ける。変更はその場で保存する。
/// </summary>
public partial class PlaySettingsWindow : Window
{
    private readonly PlayContext context;
    private readonly IReadOnlyList<CoreOption> liveOptions;
    private string scope;
    private int detailPlayer;
    private readonly List<(string tab, Action refresh)> rows = new();

    // 入力の割り当て待ち
    private readonly DispatcherTimer captureTimer = new() { Interval = TimeSpan.FromMilliseconds(20) };
    private InputManager.Capture? capture;
    private Button? captureButton;
    private bool captureKeys;
    private Action<InputBinding?>? captureDone;
    private int captureIgnoreFrames;

    /// <summary>設定が変わったとき（プレイ中ならその場で反映するため）</summary>
    public event Action? SettingsChanged;

    public PlaySettingsWindow(PlayContext context, string initialScope, IReadOnlyList<CoreOption>? liveOptions = null)
    {
        InitializeComponent();
        this.context = context;
        this.liveOptions = liveOptions ?? Array.Empty<CoreOption>();
        scope = initialScope;
        if (scope == "Game" && context.Game == null) scope = "System";
        if (scope == "System" && context.System == null) scope = "Global";

        BasicTab.Header = Loc.T("TabBasic");
        DetailTab.Header = Loc.T("TabDetail");
        ResetTabButton.Content = Loc.T("ResetTab");
        CloseButton.Content = Loc.T("Close");

        InputManager.Instance.Acquire();
        captureTimer.Tick += OnCaptureTick;
        Closed += (_, _) =>
        {
            captureTimer.Stop();
            InputManager.Instance.Release();
        };
        PreviewKeyDown += OnPreviewKeyDown;

        BuildScopeButtons();
        Rebuild();
    }

    private PlaySettings Layer => context.Layer(scope);
    private PlaySettings Inherited => context.Inherited(scope);
    private PlaySettings Effective => PlaySettings.Resolve(Inherited, Layer);

    private void Changed()
    {
        context.Save();
        SettingsChanged?.Invoke();
        foreach (var r in rows) r.refresh();
    }

    // ---------- 層の選択 ----------

    private void BuildScopeButtons()
    {
        ScopePanel.Children.Clear();
        void Add(string key, string text, bool enabled)
        {
            var rb = new RadioButton
            {
                Content = text,
                GroupName = "Scope",
                Style = (Style)FindResource("SegButton"),
                IsChecked = scope == key,
                IsEnabled = enabled,
                Tag = FindResource("AccentBrush"),
                Padding = new Thickness(14, 6, 14, 6)
            };
            rb.Checked += (_, _) =>
            {
                if (scope == key) return;
                scope = key;
                Rebuild();
            };
            ScopePanel.Children.Add(rb);
        }
        Add("Global", Loc.T("ScopeGlobal"), true);
        Add("System", context.System != null ? Loc.F("ScopeSystem", context.System.Name) : Loc.T("ScopeSystemNone"), context.System != null);
        // ゲームを選ばずに開いたときは、押せない「ゲーム」を出さない
        if (context.Game != null)
            Add("Game", context.GameTitle != null ? Loc.F("ScopeGame", Trim(context.GameTitle, 24)) : Loc.T("ScopeGameNone"), true);
    }

    private static string Trim(string s, int max) => s.Length <= max ? s : s[..(max - 1)] + "…";

    private void Rebuild()
    {
        StopCapture(null);
        rows.Clear();
        tabProps.Clear();
        BasicPanel.Children.Clear();
        DetailPanel.Children.Clear();
        ScopeHint.Text = scope switch
        {
            "Global" => Loc.T("ScopeHintGlobal"),
            "System" => Loc.T("ScopeHintSystem"),
            _ => Loc.T("ScopeHintGame")
        };

        // ----- 基本 -----
        Section(BasicPanel, Loc.T("SecController"), Loc.T("SecControllerHint"));
        PadList(BasicPanel);
        Mapping(BasicPanel, "Basic", 0, false);

        Section(BasicPanel, Loc.T("SecSave"), null);
        Folder(BasicPanel, "Basic", Loc.T("SaveDir"), nameof(PlaySettings.SaveDir), Storage.DefaultSaveDir);
        Folder(BasicPanel, "Basic", Loc.T("StateDir"), nameof(PlaySettings.StateDir), Storage.DefaultStateDir);
        Folder(BasicPanel, "Basic", Loc.T("ScreenshotDir"), nameof(PlaySettings.ScreenshotDir), Storage.DefaultScreenshotDir);

        Section(BasicPanel, Loc.T("SecVideo"), null);
        Choice(BasicPanel, "Basic", Loc.T("WindowMode"), nameof(PlaySettings.WindowMode),
            ("Window", Loc.T("ModeWindow")), ("Fullscreen", Loc.T("ModeFullscreen")));
        Choice(BasicPanel, "Basic", Loc.T("ScaleMode"), nameof(PlaySettings.ScaleMode),
            ("1", Loc.T("Scale1")), ("2", Loc.T("Scale2")), ("3", Loc.T("Scale3")), ("Fit", Loc.T("ScaleFit")));
        Choice(BasicPanel, "Basic", Loc.T("Aspect"), nameof(PlaySettings.Aspect),
            ("Original", Loc.T("AspectOriginal")), ("4:3", "4:3"), ("Stretch", Loc.T("AspectStretch")));

        Section(BasicPanel, Loc.T("SecAudio"), null);
        Slider(BasicPanel, "Basic", Loc.T("Volume"), nameof(PlaySettings.Volume), 0, 100, 5, "{0}%");
        Toggle(BasicPanel, "Basic", Loc.T("Mute"), nameof(PlaySettings.Mute));
        var devices = AudioOutput.Devices();
        Choice(BasicPanel, "Basic", Loc.T("AudioDevice"), nameof(PlaySettings.AudioDevice),
            new[] { ("", Loc.T("DefaultDevice")) }.Concat(devices.Select(d => (d.id, d.name))).ToArray());

        // ----- 詳細 -----
        Section(DetailPanel, Loc.T("SecController"), Loc.T("SecControllerDetailHint"));
        var playerBox = new ComboBox { Width = 200, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 8) };
        for (int i = 0; i < 4; i++) playerBox.Items.Add(Loc.F("PlayerN", i + 1));
        playerBox.SelectedIndex = detailPlayer;
        DetailPanel.Children.Add(playerBox);
        var mappingHost = new StackPanel();
        DetailPanel.Children.Add(mappingHost);
        playerBox.SelectionChanged += (_, _) =>
        {
            detailPlayer = Math.Max(0, playerBox.SelectedIndex);
            mappingHost.Children.Clear();
            Mapping(mappingHost, "Detail", detailPlayer, true);
        };
        Mapping(mappingHost, "Detail", detailPlayer, true);
        Slider(DetailPanel, "Detail", Loc.T("TurboRate"), nameof(PlaySettings.TurboRate), 1, 10, 1, Loc.T("TurboRateFmt"));
        Slider(DetailPanel, "Detail", Loc.T("Deadzone"), nameof(PlaySettings.Deadzone), 0, 90, 5, "{0}%");
        SubTitle(DetailPanel, Loc.T("Hotkeys"));
        Hotkeys(DetailPanel);
        Note(DetailPanel, Loc.T("KeyboardComboHint"));
        Note(DetailPanel, Loc.T("PerGameInputHint"));

        Section(DetailPanel, Loc.T("SecSave"), null);
        Choice(DetailPanel, "Detail", Loc.T("FolderMode"), nameof(PlaySettings.FolderMode),
            ("Shared", Loc.T("FolderShared")), ("PerSystem", Loc.T("FolderPerSystem")), ("PerGame", Loc.T("FolderPerGame")));
        Toggle(DetailPanel, "Detail", Loc.T("SaveNextToRom"), nameof(PlaySettings.SaveNextToRom));
        Slider(DetailPanel, "Detail", Loc.T("StateSlots"), nameof(PlaySettings.StateSlots), 1, 20, 1, "{0}");
        if (scope == "Global") BiosFolder(DetailPanel);

        Section(DetailPanel, Loc.T("SecVideo"), null);
        Choice(DetailPanel, "Detail", Loc.T("FullscreenMode"), nameof(PlaySettings.FullscreenMode),
            new[] { ("", Loc.T("FullscreenDesktop")) }.Concat(DisplayInfo.Modes().Select(m => (m, m.Replace("@", "  ") + "Hz"))).ToArray());
        Toggle(DetailPanel, "Detail", Loc.T("IntegerScale"), nameof(PlaySettings.IntegerScale));
        Choice(DetailPanel, "Detail", Loc.T("Filter"), nameof(PlaySettings.Filter),
            ("Sharp", Loc.T("FilterSharp")), ("Smooth", Loc.T("FilterSmooth")));
        Choice(DetailPanel, "Detail", Loc.T("Shader"), nameof(PlaySettings.Shader),
            ("None", Loc.T("ShaderNone")), ("Scanlines", Loc.T("ShaderScanlines")), ("Crt", Loc.T("ShaderCrt")));
        Toggle(DetailPanel, "Detail", Loc.T("VSync"), nameof(PlaySettings.VSync));
        Note(DetailPanel, Loc.T("InternalResolutionHint"));

        Section(DetailPanel, Loc.T("SecAudio"), null);
        Choice(DetailPanel, "Detail", Loc.T("SampleRate"), nameof(PlaySettings.SampleRate), (44100, "44.1kHz"), (48000, "48kHz"));
        Choice(DetailPanel, "Detail", Loc.T("OutputMode"), nameof(PlaySettings.Exclusive),
            (false, Loc.T("WasapiShared")), (true, Loc.T("WasapiExclusive")));
        Slider(DetailPanel, "Detail", Loc.T("Latency"), nameof(PlaySettings.LatencyMs), 16, 256, 8, "{0}ms");
        Toggle(DetailPanel, "Detail", Loc.T("RateControl"), nameof(PlaySettings.RateControl));

        Section(DetailPanel, Loc.T("SecCore"), null);
        CoreOptions(DetailPanel);

        foreach (var r in rows) r.refresh();
    }

    // ---------- 部品 ----------

    private void Section(Panel p, string title, string? hint)
    {
        p.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 17,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, p.Children.Count == 0 ? 8 : 26, 0, 4)
        });
        if (hint != null) Note(p, hint);
    }

    private static void SubTitle(Panel p, string title) =>
        p.Children.Add(new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 16, 0, 6) });

    private void Note(Panel p, string text) => p.Children.Add(new TextBlock
    {
        Text = text,
        Foreground = (Brush)FindResource("MutedBrush"),
        FontSize = 12.5,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 2, 0, 8)
    });

    /// <summary>ラベル（上書き中なら●と「戻す」）と、その右に入力部品を置いた1行</summary>
    private Grid Row(Panel p, string label, Func<bool> isOverridden, Action reset, FrameworkElement editor)
    {
        var g = new Grid { Margin = new Thickness(0, 4, 0, 4) };
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(230) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var head = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        var mark = new TextBlock { Text = "●", Foreground = (Brush)FindResource("AccentBrush"), Margin = new Thickness(0, 0, 6, 0), FontSize = 11, VerticalAlignment = VerticalAlignment.Center };
        var text = new TextBlock { Text = label, TextWrapping = TextWrapping.Wrap, MaxWidth = 210, VerticalAlignment = VerticalAlignment.Center };
        head.Children.Add(mark);
        head.Children.Add(text);
        g.Children.Add(head);

        Grid.SetColumn(editor, 1);
        g.Children.Add(editor);

        var resetBtn = new Button
        {
            Content = "↺",
            ToolTip = scope == "Global" ? Loc.T("ResetToDefault") : Loc.T("ResetToInherit"),
            Padding = new Thickness(8, 2, 8, 2),
            MinHeight = 28,
            Margin = new Thickness(8, 0, 0, 0)
        };
        resetBtn.Click += (_, _) => { reset(); Changed(); };
        Grid.SetColumn(resetBtn, 2);
        g.Children.Add(resetBtn);

        rows.Add(("", () =>
        {
            bool o = isOverridden();
            mark.Visibility = o ? Visibility.Visible : Visibility.Hidden;
            resetBtn.Visibility = o ? Visibility.Visible : Visibility.Hidden;
        }));
        p.Children.Add(g);
        return g;
    }

    private object? Get(PlaySettings s, string prop) => typeof(PlaySettings).GetProperty(prop)!.GetValue(s);
    private void Set(string prop, object? value) => typeof(PlaySettings).GetProperty(prop)!.SetValue(Layer, value);

    private void RegisterTab(string tab, params string[] props)
    {
        foreach (var prop in props) tabProps.Add((tab, prop));
    }

    private readonly List<(string tab, string prop)> tabProps = new();

    private void Choice<T>(Panel p, string tab, string label, string prop, params (T value, string text)[] options)
    {
        RegisterTab(tab, prop);
        var box = new ComboBox { MinWidth = 220, HorizontalAlignment = HorizontalAlignment.Left };
        foreach (var (v, t) in options) box.Items.Add(new ComboBoxItem { Content = t, Tag = v });
        bool updating = false;
        box.SelectionChanged += (_, _) =>
        {
            if (updating || box.SelectedItem is not ComboBoxItem item) return;
            Set(prop, item.Tag);
            Changed();
        };
        Row(p, label, () => Get(Layer, prop) != null, () => Set(prop, null), box);
        rows.Add((tab, () =>
        {
            updating = true;
            var cur = Get(Effective, prop);
            box.SelectedItem = box.Items.Cast<ComboBoxItem>().FirstOrDefault(i => Equals(i.Tag, cur)) ?? box.Items.Cast<ComboBoxItem>().FirstOrDefault();
            updating = false;
        }));
    }

    private void Toggle(Panel p, string tab, string label, string prop)
    {
        RegisterTab(tab, prop);
        var cb = new CheckBox { VerticalAlignment = VerticalAlignment.Center };
        bool updating = false;
        cb.Click += (_, _) =>
        {
            if (updating) return;
            Set(prop, cb.IsChecked == true);
            Changed();
        };
        Row(p, label, () => Get(Layer, prop) != null, () => Set(prop, null), cb);
        rows.Add((tab, () =>
        {
            updating = true;
            cb.IsChecked = Get(Effective, prop) as bool? == true;
            updating = false;
        }));
    }

    private void Slider(Panel p, string tab, string label, string prop, int min, int max, int step, string format)
    {
        RegisterTab(tab, prop);
        var panel = new DockPanel();
        var valueText = new TextBlock { Width = 70, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
        DockPanel.SetDock(valueText, Dock.Right);
        var slider = new Slider { Minimum = min, Maximum = max, TickFrequency = step, IsSnapToTickEnabled = true, VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(valueText);
        panel.Children.Add(slider);
        bool updating = false;
        slider.ValueChanged += (_, _) =>
        {
            valueText.Text = string.Format(format, (int)slider.Value);
            if (updating) return;
            Set(prop, (int)slider.Value);
            context.Save();
            SettingsChanged?.Invoke();
        };
        // 離したときだけ印を更新する（つまみを動かすたびに作り直さない）
        slider.PreviewMouseUp += (_, _) => { foreach (var r in rows) r.refresh(); };
        Row(p, label, () => Get(Layer, prop) != null, () => Set(prop, null), panel);
        rows.Add((tab, () =>
        {
            updating = true;
            slider.Value = Get(Effective, prop) as int? ?? min;
            valueText.Text = string.Format(format, (int)slider.Value);
            updating = false;
        }));
    }

    private void Folder(Panel p, string tab, string label, string prop, string defaultPath)
    {
        RegisterTab(tab, prop);
        var panel = new DockPanel();
        var browse = new Button { Content = Loc.T("Browse"), Margin = new Thickness(8, 0, 0, 0) };
        DockPanel.SetDock(browse, Dock.Right);
        var box = new TextBox { VerticalContentAlignment = VerticalAlignment.Center, ToolTip = Loc.F("DefaultFolder", defaultPath) };
        panel.Children.Add(browse);
        panel.Children.Add(box);
        bool updating = false;
        box.LostFocus += (_, _) =>
        {
            if (updating) return;
            var v = box.Text.Trim().Trim('"');
            var cur = Get(Effective, prop) as string ?? "";
            if (v == cur) return;
            Set(prop, v);
            Changed();
        };
        browse.Click += (_, _) =>
        {
            var dlg = new OpenFolderDialog();
            var cur = Get(Effective, prop) as string;
            dlg.InitialDirectory = string.IsNullOrEmpty(cur) ? defaultPath : cur;
            if (dlg.ShowDialog(this) != true) return;
            Set(prop, dlg.FolderName);
            Changed();
        };
        Row(p, label, () => Get(Layer, prop) != null, () => Set(prop, null), panel);
        rows.Add((tab, () =>
        {
            updating = true;
            box.Text = Get(Effective, prop) as string ?? "";
            updating = false;
        }));
        Note(p, Loc.F("EmptyMeansDefault", defaultPath));
    }

    private void BiosFolder(Panel p)
    {
        var panel = new DockPanel();
        var browse = new Button { Content = Loc.T("Browse"), Margin = new Thickness(8, 0, 0, 0) };
        DockPanel.SetDock(browse, Dock.Right);
        var box = new TextBox { Text = context.App.BiosDir, VerticalContentAlignment = VerticalAlignment.Center };
        panel.Children.Add(browse);
        panel.Children.Add(box);
        box.LostFocus += (_, _) =>
        {
            context.App.BiosDir = box.Text.Trim().Trim('"');
            Changed();
        };
        browse.Click += (_, _) =>
        {
            var dlg = new OpenFolderDialog { InitialDirectory = context.BiosDir };
            if (dlg.ShowDialog(this) != true) return;
            box.Text = context.App.BiosDir = dlg.FolderName;
            Changed();
        };
        Row(p, Loc.T("BiosDir"), () => !string.IsNullOrEmpty(context.App.BiosDir), () => { context.App.BiosDir = ""; box.Text = ""; }, panel);
        Note(p, Loc.F("EmptyMeansDefault", Storage.DefaultBiosDir));
    }

    // ---------- 入力 ----------

    private void PadList(Panel p)
    {
        var text = new TextBlock { Foreground = (Brush)FindResource("SubBrush"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 8) };
        p.Children.Add(text);
        void Update()
        {
            var pads = InputManager.Instance.Pads;
            text.Text = pads.Count == 0
                ? Loc.T("NoPads")
                : string.Join("\n", pads.Select((x, i) => Loc.F("PadAssigned", i + 1, x.Name)));
        }
        Update();
        // パッドの抜き差しに追従する
        var t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        t.Tick += (_, _) => Update();
        t.Start();
        Closed += (_, _) => t.Stop();
    }

    /// <summary>この層で入力を編集できるようにする（初めて変えるときに下の層の割り当てを写す）</summary>
    private InputProfile EditableInput()
    {
        if (Layer.Input == null) Layer.Input = (Inherited.Input ?? InputProfile.CreateDefault()).Clone();
        while (Layer.Input.Players.Count < 4) Layer.Input.Players.Add(new PlayerMap());
        return Layer.Input;
    }

    private InputProfile EffectiveInput => Effective.Input ?? InputProfile.CreateDefault();

    private static bool IsKey(InputBinding b) => b.Kind == "Key";

    private void Mapping(Panel p, string tab, int player, bool showTurbo)
    {
        var grid = new Grid { Margin = new Thickness(0, 4, 0, 4) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        if (showTurbo) grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        void Header(int col, string text)
        {
            var tb = new TextBlock { Text = text, Foreground = (Brush)FindResource("MutedBrush"), FontSize = 12.5, Margin = new Thickness(4, 0, 4, 4) };
            Grid.SetColumn(tb, col);
            grid.Children.Add(tb);
        }
        grid.RowDefinitions.Add(new RowDefinition());
        Header(0, Loc.T("ColControl"));
        Header(1, Loc.T("ColPad"));
        Header(2, Loc.T("ColKeyboard"));
        if (showTurbo) Header(3, Loc.T("ColTurbo"));

        int row = 1;
        foreach (var control in InputProfile.Controls)
        {
            grid.RowDefinitions.Add(new RowDefinition());
            var name = new TextBlock { Text = Loc.T("Ctl" + control), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0) };
            Grid.SetRow(name, row);
            grid.Children.Add(name);

            var padBtn = BindingButton();
            var keyBtn = BindingButton();
            Grid.SetRow(padBtn, row); Grid.SetColumn(padBtn, 1);
            Grid.SetRow(keyBtn, row); Grid.SetColumn(keyBtn, 2);
            grid.Children.Add(padBtn);
            grid.Children.Add(keyBtn);

            string c = control;
            padBtn.Click += (_, _) => StartCapture(padBtn, false, b => SetBinding(player, c, false, b));
            keyBtn.Click += (_, _) => StartCapture(keyBtn, true, b => SetBinding(player, c, true, b));

            CheckBox? turbo = null;
            if (showTurbo)
            {
                turbo = new CheckBox { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 4, 0) };
                Grid.SetRow(turbo, row); Grid.SetColumn(turbo, 3);
                grid.Children.Add(turbo);
                turbo.Click += (_, _) =>
                {
                    var map = EditableInput().Players[player];
                    map.Turbo.Remove(c);
                    if (turbo.IsChecked == true) map.Turbo.Add(c);
                    Changed();
                };
            }

            rows.Add((tab, () =>
            {
                var map = EffectiveInput.Players.ElementAtOrDefault(player) ?? new PlayerMap();
                var list = map.Buttons.GetValueOrDefault(c) ?? new List<InputBinding>();
                if (captureButton != padBtn) padBtn.Content = Describe(list.Where(b => !IsKey(b)));
                if (captureButton != keyBtn) keyBtn.Content = Describe(list.Where(IsKey));
                if (turbo != null) turbo.IsChecked = map.Turbo.Contains(c);
            }));
            row++;
        }

        var head = new DockPanel { Margin = new Thickness(0, 6, 0, 0) };
        var mark = new TextBlock { Text = "● " + Loc.T("InputOverridden"), Foreground = (Brush)FindResource("AccentBrush"), FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center };
        var reset = new Button { Content = scope == "Global" ? Loc.T("InputResetDefault") : Loc.T("InputResetInherit"), Padding = new Thickness(10, 3, 10, 3), MinHeight = 28 };
        DockPanel.SetDock(reset, Dock.Right);
        reset.Click += (_, _) => { Layer.Input = null; Changed(); };
        head.Children.Add(reset);
        head.Children.Add(mark);
        rows.Add((tab, () =>
        {
            bool o = Layer.Input != null;
            mark.Visibility = o ? Visibility.Visible : Visibility.Hidden;
            reset.Visibility = o ? Visibility.Visible : Visibility.Collapsed;
        }));
        p.Children.Add(head);
        p.Children.Add(grid);
        Note(p, Loc.T("MappingHint"));
    }

    private void Hotkeys(Panel p)
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(190) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        int row = 0;
        foreach (var hk in InputProfile.HotkeyNames)
        {
            grid.RowDefinitions.Add(new RowDefinition());
            var name = new TextBlock { Text = Loc.T("Hk" + hk), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 4, 0) };
            Grid.SetRow(name, row);
            grid.Children.Add(name);
            var padBtn = BindingButton();
            var keyBtn = BindingButton();
            Grid.SetRow(padBtn, row); Grid.SetColumn(padBtn, 1);
            Grid.SetRow(keyBtn, row); Grid.SetColumn(keyBtn, 2);
            grid.Children.Add(padBtn);
            grid.Children.Add(keyBtn);
            string h = hk;
            padBtn.Click += (_, _) => StartCapture(padBtn, false, b => SetHotkey(h, false, b));
            keyBtn.Click += (_, _) => StartCapture(keyBtn, true, b => SetHotkey(h, true, b));
            rows.Add(("Detail", () =>
            {
                var list = EffectiveInput.Hotkeys.GetValueOrDefault(h) ?? new List<InputBinding>();
                if (captureButton != padBtn) padBtn.Content = Describe(list.Where(b => !IsKey(b)));
                if (captureButton != keyBtn) keyBtn.Content = Describe(list.Where(IsKey));
            }));
            row++;
        }
        p.Children.Add(grid);
    }

    private Button BindingButton() => new()
    {
        Margin = new Thickness(3),
        Padding = new Thickness(8, 3, 8, 3),
        MinHeight = 30,
        HorizontalContentAlignment = HorizontalAlignment.Left
    };

    private static string Describe(IEnumerable<InputBinding> list)
    {
        var s = string.Join(" / ", list.Select(InputManager.Describe));
        return s.Length == 0 ? "—" : s;
    }

    /// <summary>キー（またはパッド）の割り当てを1つに置き換える。null なら消す</summary>
    private void SetBinding(int player, string control, bool keys, InputBinding? b)
    {
        var map = EditableInput().Players[player];
        if (!map.Buttons.TryGetValue(control, out var list)) map.Buttons[control] = list = new List<InputBinding>();
        list.RemoveAll(x => IsKey(x) == keys);
        if (b != null) list.Add(b);
        Changed();
    }

    private void SetHotkey(string name, bool keys, InputBinding? b)
    {
        var hk = EditableInput().Hotkeys;
        if (!hk.TryGetValue(name, out var list)) hk[name] = list = new List<InputBinding>();
        list.RemoveAll(x => IsKey(x) == keys);
        if (b != null) list.Add(b);
        Changed();
    }

    private void StartCapture(Button button, bool keys, Action<InputBinding?> done)
    {
        StopCapture(null);
        captureButton = button;
        captureKeys = keys;
        captureDone = done;
        captureIgnoreFrames = 8; // クリックした直後のマウス・キーを拾わない
        capture = InputManager.Instance.StartCapture();
        button.Content = keys ? Loc.T("PressKey") : Loc.T("PressPad");
        captureTimer.Start();
    }

    private void StopCapture(InputBinding? result, bool clear = false)
    {
        captureTimer.Stop();
        var done = captureDone;
        var had = captureButton != null;
        capture = null;
        captureButton = null;
        captureDone = null;
        if (!had) return;
        if (result != null || clear) done?.Invoke(result);
        else foreach (var r in rows) r.refresh();
    }

    private void OnCaptureTick(object? sender, EventArgs e)
    {
        if (capture == null) return;
        if (captureIgnoreFrames > 0)
        {
            captureIgnoreFrames--;
            capture = InputManager.Instance.StartCapture();
            return;
        }
        var b = capture.Poll();
        if (b == null) return;
        if (IsKey(b) != captureKeys)
        {
            // 種類が違う入力（キー待ちでパッドなど）は無視する。ただし Esc は取り消し
            if (b.Kind == "Key" && b.Code == 0x1B) StopCapture(null);
            return;
        }
        if (captureKeys && b.Code == 0x1B && b.With == 0) { StopCapture(null); return; }
        if (captureKeys && b.Code == 0x2E && b.With == 0) { StopCapture(null, clear: true); return; }
        StopCapture(b);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        // 割り当て待ちのあいだはキー操作で画面が動かないようにする
        if (captureButton == null) return;
        if (!captureKeys)
        {
            if (e.Key == Key.Escape) StopCapture(null);
            else if (e.Key == Key.Delete) StopCapture(null, clear: true);
        }
        e.Handled = true;
    }

    // ---------- コア固有 ----------

    private void CoreOptions(Panel p)
    {
        var coreName = context.CoreName;
        if (string.IsNullOrEmpty(coreName))
        {
            Note(p, Loc.T("CoreOptionsNoSystem"));
            return;
        }
        var options = liveOptions.Count > 0 ? liveOptions.ToList() : Storage.LoadCoreOptions(coreName);
        var info = CoreCatalog.Get(coreName);
        Note(p, Loc.F("CoreOptionsFor", info?.DisplayName ?? coreName));
        if (options.Count == 0)
        {
            Note(p, Loc.T("CoreOptionsUnknown"));
            return;
        }
        foreach (var opt in options)
        {
            var box = new ComboBox { MinWidth = 220, HorizontalAlignment = HorizontalAlignment.Left };
            foreach (var v in opt.Values) box.Items.Add(v);
            bool updating = false;
            var key = opt.Key;
            box.SelectionChanged += (_, _) =>
            {
                if (updating || box.SelectedItem is not string v) return;
                Layer.CoreOptions ??= new Dictionary<string, string>();
                Layer.CoreOptions[key] = v;
                Changed();
            };
            Row(p, opt.Description, () => Layer.CoreOptions?.ContainsKey(key) == true, () => Layer.CoreOptions?.Remove(key), box);
            rows.Add(("Detail", () =>
            {
                updating = true;
                var eff = Effective.CoreOptions;
                box.SelectedItem = eff != null && eff.TryGetValue(key, out var v) && opt.Values.Contains(v) ? v : opt.Default;
                updating = false;
            }));
        }
    }

    // ---------- 下のボタン ----------

    private void OnResetTab(object sender, RoutedEventArgs e)
    {
        bool basic = Tabs.SelectedItem == BasicTab;
        var tab = basic ? "Basic" : "Detail";
        var msg = Loc.F("ConfirmResetTab", basic ? Loc.T("TabBasic") : Loc.T("TabDetail"));
        if (MessageBox.Show(this, msg, "JPEmu Renamer", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        foreach (var (t, prop) in tabProps)
            if (t == tab) Set(prop, null);
        Layer.Input = null;
        if (!basic) Layer.CoreOptions = null;
        Changed();
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();
}
