// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using JPEmuRenamer.Emulation;
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;

namespace JPEmuRenamer.Ui;

/// <summary>
/// メイン画面右上の自動プレビュー。タイトルを選んで少し止まると、内蔵コアを音・入力なしで動かして映像を流す。
/// 始まるまでと、画面が変化しないゲームでは、保存済みのスナップショットを表示する。
/// 自分で撮ったスクリーンショットがあり、優先する設定なら、それを表示して自動プレビューは始めない（画面のクリックで切り替え）。
/// 同時に動かせるコアは1つなので、遊ぶとき・コアを更新するときなどは Suspend で止める。
/// </summary>
public sealed class PreviewController
{
    private readonly Image image;
    private readonly TextBlock text;
    private readonly Button saveButton;
    private readonly Func<AppSettings> settings;
    private readonly Action saveSettings;
    private readonly DispatcherTimer delay = new();

    private SystemConfig? system;
    private GameItem? item;
    private bool playTab;
    private bool active = true;
    private int suspendCount;

    private EmulatorSession? session;
    private WriteableBitmap? bitmap;
    private int[] frame = Array.Empty<int>();
    private long serial;
    private ImageSource? snapshot;
    private string? snapshotPath;
    private bool rendering;
    private ImageSource? ownShot;
    // 自分のスクショがあるタイトルで、クリックして自動プレビューに切り替えたとき
    private bool showLive;

    public PreviewController(Image image, TextBlock text, Button saveButton, Func<AppSettings> settings, Action saveSettings)
    {
        this.image = image;
        this.text = text;
        this.saveButton = saveButton;
        this.settings = settings;
        this.saveSettings = saveSettings;
        delay.Tick += (_, _) =>
        {
            delay.Stop();
            StartIfAllowed();
        };
    }

    private PreviewSettings Options => settings().Preview;

    /// <summary>選んでいるタイトルが変わったとき</summary>
    public void Select(SystemConfig? sys, GameItem? selected, bool onPlayTab)
    {
        bool same = sys == system && selected == item && onPlayTab == playTab;
        system = sys;
        item = selected;
        playTab = onPlayTab;
        if (same && session != null) return;
        if (!same) showLive = false;

        LoadOwnShot();
        StopSession();
        ShowIdle();
        delay.Stop();
        // カーソルを連打している間は始めない（止まってから待ち時間がたったら始める）
        delay.Interval = TimeSpan.FromMilliseconds(Math.Clamp(Options.DelayMs, 0, 3000));
        if (CanRun()) delay.Start();
    }

    /// <summary>ウィンドウが前面にあるか（裏にあるあいだは止めておく）</summary>
    public void SetActive(bool isActive)
    {
        if (active == isActive) return;
        active = isActive;
        Refresh();
    }

    /// <summary>一時的に止める（遊ぶとき、コアの更新、スクショの一括作成など）。Resume と対で呼ぶ</summary>
    public void Suspend()
    {
        suspendCount++;
        delay.Stop();
        StopSession();
        ShowIdle();
    }

    public void Resume()
    {
        if (suspendCount > 0) suspendCount--;
        Refresh();
    }

    /// <summary>設定を変えたときなどに、いまの状態でやり直す</summary>
    public void Refresh()
    {
        LoadOwnShot();
        if (!CanRun())
        {
            delay.Stop();
            StopSession();
            ShowIdle();
            return;
        }
        if (session == null && !delay.IsEnabled)
        {
            delay.Interval = TimeSpan.FromMilliseconds(Math.Clamp(Options.DelayMs, 0, 3000));
            delay.Start();
        }
    }

    public void Shutdown()
    {
        delay.Stop();
        StopSession();
    }

    private bool CanRun() =>
        Options.Enabled && active && suspendCount == 0 && playTab && system != null && item?.Rom != null && !ShowingOwnShot;

    private bool ShowingOwnShot => ownShot != null && !showLive;

    /// <summary>画面をクリックしたとき：自分のスクショと自動プレビューを切り替える</summary>
    public void ToggleOwnShot()
    {
        if (ownShot == null) return;
        showLive = !showLive;
        delay.Stop();
        StopSession();
        ShowIdle();
        if (showLive && CanRun()) StartIfAllowed();
    }

    private void LoadOwnShot()
    {
        var path = Options.PreferOwnShot && system != null && item != null
            ? Snapshots.OwnShotFor(Options.OwnShotFolder, system, item) : null;
        ownShot = Snapshots.Load(path);
        image.ToolTip = ownShot != null ? Loc.T("OwnShotTip") : null;
        image.Cursor = ownShot != null ? System.Windows.Input.Cursors.Hand : null;
    }

    // ---------- 表示 ----------

    /// <summary>再生していないときの表示（スナップショットか案内の文字）</summary>
    private void ShowIdle()
    {
        saveButton.Visibility = Visibility.Collapsed;
        if (ShowingOwnShot)
        {
            image.Source = ownShot;
            text.Text = "";
            return;
        }
        snapshotPath = system != null && item != null ? Snapshots.PathFor(system, item) : null;
        snapshot = Snapshots.Load(snapshotPath);
        image.Source = snapshot;
        saveButton.Visibility = Visibility.Collapsed;
        if (snapshot != null)
        {
            text.Text = "";
            return;
        }
        if (item == null || system == null) text.Text = "";
        else if (item.Rom == null) text.Text = Loc.T("PreviewMissing");
        else if (!Options.Enabled) text.Text = Loc.T("PreviewOff");
        else if (!CoreCatalog.IsInstalled(CoreCatalog.CoreNameFor(system))) text.Text = Loc.T("PreviewNoCore");
        else text.Text = Loc.T("PreviewWaiting");
    }

    private void StartIfAllowed()
    {
        if (!CanRun() || session != null) return;
        var sys = system!;
        var target = item!;
        var coreName = CoreCatalog.CoreNameFor(sys);
        if (string.IsNullOrEmpty(coreName) || !CoreCatalog.IsInstalled(coreName))
        {
            ShowIdle();
            return;
        }

        var gameKey = target.Rom!.Crc.ToString("X8");
        var context = new PlayContext(settings(), sys, gameKey, target.Title, saveSettings);
        // 必要なBIOSが無ければ、起動せずに何が足りないかを出す
        if (CoreCatalog.MissingBios(sys, context.BiosDir) is string bios)
        {
            ShowIdle();
            if (snapshot == null) text.Text = Loc.F("BiosMissing", bios);
            return;
        }
        var opt = Options;
        var request = new SessionRequest(coreName, target.Rom.Path, target.Rom.InnerName, sys, gameKey, target.Title,
            context.Resolve(), context.BiosDir,
            new PreviewOptions(Math.Clamp(opt.Speed, 1, 16), Math.Clamp(opt.SkipSeconds, 0, 120), Math.Clamp(opt.LoopSeconds, 0, 600)));

        var s = new EmulatorSession(request);
        s.Failed += _ => image.Dispatcher.BeginInvoke(() =>
        {
            if (session != s) return;
            StopSession();
            ShowIdle();
            if (snapshot == null) text.Text = Loc.T("PreviewFailed");
        });
        session = s;
        serial = 0;
        if (snapshot == null) text.Text = Loc.T("PreviewLoading");
        s.Start();
        if (!rendering)
        {
            CompositionTarget.Rendering += OnRendering;
            rendering = true;
        }
    }

    private void StopSession()
    {
        if (rendering)
        {
            CompositionTarget.Rendering -= OnRendering;
            rendering = false;
        }
        var s = session;
        session = null;
        bitmap = null;
        if (s != null)
        {
            s.Stop();
            s.Dispose();
        }
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        var s = session;
        if (s == null) return;

        // 画面がしばらく変化しない（タイトル画面で止まる）ゲームは、スナップショットがあればそちらを出す
        if (snapshot != null && s.Publishing && s.SecondsSinceChange > 3)
        {
            if (image.Source != snapshot)
            {
                image.Source = snapshot;
                saveButton.Visibility = Visibility.Collapsed;
            }
            return;
        }

        if (!s.TryGetFrame(ref frame, out var w, out var h, ref serial)) return;
        if (bitmap == null || bitmap.PixelWidth != w || bitmap.PixelHeight != h)
            bitmap = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgr32, null);
        bitmap.WritePixels(new Int32Rect(0, 0, w, h), frame, w * 4, 0);
        if (image.Source != bitmap)
        {
            image.Source = bitmap;
            text.Text = "";
            saveButton.Visibility = Visibility.Visible;
        }
    }

    /// <summary>いま流れている画面を、このタイトルのスナップショットとして保存する</summary>
    public bool SaveCurrentFrame()
    {
        if (bitmap == null || snapshotPath == null || image.Source != bitmap) return false;
        int w = bitmap.PixelWidth, h = bitmap.PixelHeight;
        var pixels = new int[w * h];
        bitmap.CopyPixels(pixels, w * 4, 0);
        Snapshots.Save(snapshotPath, pixels, w, h);
        snapshot = Snapshots.Load(snapshotPath);
        return true;
    }
}
