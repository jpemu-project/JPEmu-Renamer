// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JPEmuRenamer.Emulation;
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;

namespace JPEmuRenamer.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly AppSettings settings;
    private CancellationTokenSource? loadCts;

    // 画面側（MainWindow）が受け持つダイアログ
    public Func<string, bool>? Confirm { get; set; }
    public Action<string>? Info { get; set; }
    public Func<SystemConfig?, (SystemConfig? config, bool deleted)>? EditSystemDialog { get; set; }
    public Action? OpenAppSettingsDialog { get; set; }
    /// <summary>内蔵コアのプレイ画面を開く（既に開いていれば false）</summary>
    public Func<PlayContext, SessionRequest, bool>? OpenPlayWindow { get; set; }
    public Action<PlayContext, string>? OpenPlaySettingsDialog { get; set; }
    public Action<SystemConfig, List<GameItem>>? OpenSnapshotBatch { get; set; }

    public MainViewModel(AppSettings settings)
    {
        this.settings = settings;
        Loc.Instance.Language = settings.Language;
        uiScale = Math.Clamp(settings.UiScale, 0.75, 2.0);
        fontSize = Math.Clamp(settings.FontSize, 11, 28);
        filter = settings.ListFilter is "Owned" or "Missing" ? settings.ListFilter : "All";
        GameItem.HideExtensions = settings.HideExtensions;
        statusText = Loc.T("Ready");

        foreach (var s in settings.Systems) Systems.Add(new SystemItem(s));
        Systems.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoSystems));
        itemsView = CollectionViewSource.GetDefaultView(items);

        Loc.Instance.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(Loc.Language)) return;
            foreach (var s in Systems) s.RefreshTexts();
            OnPropertyChanged(nameof(CountText));
            OnPropertyChanged(nameof(SelectedCountText));
            OnPropertyChanged(nameof(LaunchButtonText));
            _ = ReloadAsync();
        };
    }

    public AppSettings Settings => settings;
    public ObservableCollection<SystemItem> Systems { get; } = new();

    /// <summary>機種が1つも無い（最初の起動時など）</summary>
    public bool HasNoSystems => Systems.Count == 0;

    [ObservableProperty]
    private ObservableCollection<GameItem> items = new();

    [ObservableProperty]
    private ICollectionView itemsView;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LaunchButtonText))]
    private SystemItem? selectedSystem;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    private GameItem? selectedItem;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private string filter;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPlayMode))]
    private bool isManageMode;

    /// <summary>「遊ぶ」側のラジオボタン用（IsManageMode の反対）</summary>
    public bool IsPlayMode
    {
        get => !IsManageMode;
        set => IsManageMode = !value;
    }

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool needsSetup;

    [ObservableProperty]
    private string statusText;

    [ObservableProperty]
    private double uiScale;

    [ObservableProperty]
    private double fontSize;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountText))]
    private int ownedCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountText))]
    private int missingCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedCountText))]
    private int selectedCount;

    public bool HasSelection => SelectedItem != null;
    public string CountText => Loc.F("OwnedMissing", OwnedCount, MissingCount);
    public string SelectedCountText => Loc.F("SelectedCount", SelectedCount);
    public string LaunchButtonText =>
        SelectedSystem != null && UsesExternal(SelectedSystem.Config) ? Loc.T("LaunchExternal") : Loc.T("LaunchCore");

    /// <summary>既定の起動方法が外部exeか（exeが未設定なら内蔵コア）</summary>
    private static bool UsesExternal(SystemConfig sys) =>
        sys.LaunchMode == "External" && !string.IsNullOrWhiteSpace(sys.EmulatorPath);

    // ---------- 初期化・読み込み ----------

    public void Start()
    {
        var first = Systems.FirstOrDefault(s => s.Config.Id == settings.LastSystemId) ?? Systems.FirstOrDefault();
        SelectedSystem = first;
    }

    partial void OnSelectedSystemChanged(SystemItem? value)
    {
        if (value == null) return;
        settings.LastSystemId = value.Config.Id;
        Save();
        _ = LoadAsync(value);
    }

    public Task ReloadAsync() => SelectedSystem != null ? LoadAsync(SelectedSystem) : Task.CompletedTask;

    private async Task LoadAsync(SystemItem sysItem)
    {
        loadCts?.Cancel();
        var cts = new CancellationTokenSource();
        loadCts = cts;
        var ct = cts.Token;

        var sys = sysItem.Config.Clone();
        bool subfolders = settings.ScanSubfolders;
        NeedsSetup = string.IsNullOrWhiteSpace(sys.DatPath) && sys.RomFolders.Count == 0;
        if (NeedsSetup)
        {
            SetItems(new List<GameItem>());
            StatusText = Loc.T("Ready");
            return;
        }

        IsBusy = true;
        var progress = new Progress<(int done, int total)>(p =>
        {
            if (!ct.IsCancellationRequested) StatusText = Loc.F("Scanning", p.done, p.total);
        });

        try
        {
            string? datError = null;
            var list = await Task.Run(() =>
            {
                var dat = new List<DatEntry>();
                if (!string.IsNullOrWhiteSpace(sys.DatPath))
                {
                    try { dat = DatParser.Parse(sys.DatPath); }
                    catch (Exception ex) { datError = ex.Message; }
                }
                var cache = Storage.LoadCache(sys.Id);
                var roms = RomScanner.Scan(sys, subfolders, cache, progress, ct);
                Storage.SaveCache(sys.Id, roms);
                return Matcher.Match(dat, roms, sys);
            }, ct);

            if (ct.IsCancellationRequested) return;
            SetItems(list);
            sysItem.OwnedCount = OwnedCount;
            sysItem.MissingCount = MissingCount;
            sysItem.Counted = true;

            StatusText = datError != null
                ? Loc.F("DatError", datError)
                : Loc.F("Loaded", sys.Name,
                    list.Count(i => i.Status is MatchStatus.Match or MatchStatus.NameDiff or MatchStatus.Duplicate),
                    list.Count(i => i.Status == MatchStatus.NameDiff),
                    list.Count(i => i.Status == MatchStatus.NotInDat));
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            StatusText = ex.Message;
        }
        finally
        {
            if (loadCts == cts) IsBusy = false;
        }
    }

    private void SetItems(List<GameItem> list)
    {
        foreach (var old in Items) old.PropertyChanged -= OnItemPropertyChanged;
        foreach (var it in list) it.PropertyChanged += OnItemPropertyChanged;

        Items = new ObservableCollection<GameItem>(list);
        var view = CollectionViewSource.GetDefaultView(Items);
        view.Filter = FilterItem;
        ItemsView = view;

        OwnedCount = list.Where(i => i.IsOwned && i.Dat != null).Select(i => i.Dat!.Title).Distinct().Count();
        MissingCount = list.Count(i => i.Status == MatchStatus.Missing);
        SelectedItem = list.FirstOrDefault(i => FilterItem(i));
        UpdateSelectedCount();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameItem.IsChecked)) UpdateSelectedCount();
    }

    private void UpdateSelectedCount() => SelectedCount = Items.Count(i => i.IsChecked && i.CanRename);

    private bool FilterItem(object o)
    {
        if (o is not GameItem g) return false;
        if (Filter == "Owned" && !g.IsOwned) return false;
        if (Filter == "Missing" && g.IsOwned) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        var q = SearchText.Trim();
        return g.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
            || g.CurrentFileName.Contains(q, StringComparison.OrdinalIgnoreCase);
    }

    partial void OnSearchTextChanged(string value) => ItemsView.Refresh();

    partial void OnFilterChanged(string value)
    {
        settings.ListFilter = value;
        Save();
        ItemsView.Refresh();
    }

    partial void OnUiScaleChanged(double value)
    {
        settings.UiScale = value;
        Save();
    }

    partial void OnFontSizeChanged(double value)
    {
        settings.FontSize = value;
        Save();
    }

    public void ChangeScale(int wheelDelta)
    {
        var next = Math.Round(UiScale + (wheelDelta > 0 ? 0.05 : -0.05), 2);
        UiScale = Math.Clamp(next, 0.75, 2.0);
    }

    // ---------- コマンド ----------

    [RelayCommand]
    private Task Launch()
    {
        var sys = SelectedSystem?.Config;
        if (sys == null) return Task.CompletedTask;
        return LaunchWith(UsesExternal(sys) ? "External" : "Core");
    }

    /// <summary>起動方法を指定して起動する（右クリックの「別の方法で起動」）</summary>
    [RelayCommand]
    private async Task LaunchWith(string mode)
    {
        var item = SelectedItem;
        var sys = SelectedSystem?.Config;
        if (item == null || sys == null) return;
        if (item.Rom == null)
        {
            Info?.Invoke(Loc.T("MissingCannotLaunch"));
            return;
        }

        if (mode == "External")
        {
            if (string.IsNullOrWhiteSpace(sys.EmulatorPath))
            {
                Info?.Invoke(Loc.T("NoEmulator"));
                return;
            }
            try
            {
                Launcher.LaunchExternal(sys, item.Rom.Path);
            }
            catch (Exception ex)
            {
                Info?.Invoke(Loc.F("LaunchFailed", ex.Message));
            }
            return;
        }

        var coreName = CoreCatalog.CoreNameFor(sys);
        if (string.IsNullOrEmpty(coreName))
        {
            Info?.Invoke(Loc.T("NoCoreForSystem"));
            return;
        }
        if (!CoreCatalog.IsInstalled(coreName))
        {
            var display = CoreCatalog.Get(coreName)?.DisplayName ?? coreName;
            if (Confirm?.Invoke(Loc.F("ConfirmDownloadCore", display)) != true) return;
            if (!await DownloadCoreAsync(coreName)) return;
        }

        var context = new PlayContext(settings, sys, item.Rom.Crc.ToString("X8"), item.Title, Save);
        var request = new SessionRequest(coreName, item.Rom.Path, item.Rom.InnerName, sys, context.GameKey!, item.Title,
            context.Resolve(), context.BiosDir);
        if (OpenPlayWindow?.Invoke(context, request) == false)
            Info?.Invoke(Loc.T("AlreadyPlaying"));
    }

    /// <summary>コアをダウンロードする（進み具合は下の状態表示に出す）</summary>
    public async Task<bool> DownloadCoreAsync(string coreName)
    {
        var display = CoreCatalog.Get(coreName)?.DisplayName ?? coreName;
        var before = StatusText;
        IsBusy = true;
        try
        {
            var progress = new Progress<double>(p => StatusText = Loc.F("CoreDownloading", display, (int)(p * 100)));
            StatusText = Loc.F("CoreDownloading", display, 0);
            await CoreCatalog.DownloadAsync(coreName, progress, CancellationToken.None);
            StatusText = Loc.F("CoreDownloaded", display);
            return true;
        }
        catch (Exception ex)
        {
            StatusText = before;
            Info?.Invoke(Loc.F("CoreDownloadFailed", display, ex.Message));
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>プレイ設定を開く（"Global" / "System" / "Game"）</summary>
    [RelayCommand]
    private void OpenPlaySettings(string scope)
    {
        var sys = SelectedSystem?.Config;
        var item = scope == "Game" ? SelectedItem : null;
        var key = item?.Rom != null ? item.Rom.Crc.ToString("X8") : null;
        var context = new PlayContext(settings, scope == "Global" && sys == null ? null : sys, key, key != null ? item!.Title : null, Save);
        OpenPlaySettingsDialog?.Invoke(context, key != null ? "Game" : scope == "Global" ? "Global" : "System");
    }

    /// <summary>この機種の所有ROMのスナップショットを一括作成する</summary>
    [RelayCommand]
    private void CreateSnapshots()
    {
        var sys = SelectedSystem?.Config;
        if (sys == null) return;
        OpenSnapshotBatch?.Invoke(sys, Items.Where(i => i.Rom != null).ToList());
    }

    [RelayCommand]
    private void OpenFolder()
    {
        var rom = SelectedItem?.Rom;
        if (rom == null) return;
        try { Launcher.OpenFolder(rom.Path); } catch { }
    }

    [RelayCommand]
    private Task Rescan() => ReloadAsync();

    [RelayCommand]
    private void SelectAll(bool? check)
    {
        foreach (var i in Items.Where(i => i.CanRename)) i.IsChecked = check == true;
    }

    [RelayCommand]
    private async Task RunRename()
    {
        var sys = SelectedSystem?.Config;
        if (sys == null || IsBusy) return;
        var targets = Items.Where(i => i.IsChecked && i.CanRename).ToList();
        if (targets.Count == 0) return;
        if (Confirm?.Invoke(Loc.F("ConfirmRename", targets.Count)) != true) return;

        IsBusy = true;
        bool insideZip = settings.RenameInsideZip;
        var result = await Task.Run(() => Renamer.Execute(targets, sys, insideZip));
        if (result.Run.Records.Count > 0)
        {
            var history = Storage.LoadHistory();
            history.Add(result.Run);
            Storage.SaveHistory(history);
        }
        IsBusy = false;

        var msg = Loc.F("RenameDone", result.Run.Records.Count, result.Skipped);
        if (result.Errors.Count > 0) msg += "\n\n" + string.Join("\n", result.Errors.Take(10));
        Info?.Invoke(msg);
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task Undo()
    {
        if (IsBusy) return;
        var history = Storage.LoadHistory();
        var last = history.LastOrDefault();
        if (last == null)
        {
            Info?.Invoke(Loc.T("NothingToUndo"));
            return;
        }
        if (Confirm?.Invoke(Loc.F("ConfirmUndo", last.At.ToString("yyyy/MM/dd HH:mm"), last.Records.Count)) != true) return;

        IsBusy = true;
        var errors = new List<string>();
        int count = await Task.Run(() => Renamer.Undo(last, errors));
        history.RemoveAt(history.Count - 1);
        Storage.SaveHistory(history);
        IsBusy = false;

        var msg = Loc.F("UndoDone", count);
        if (errors.Count > 0) msg += "\n\n" + string.Join("\n", errors.Take(10));
        Info?.Invoke(msg);
        await ReloadAsync();
    }

    [RelayCommand]
    private void AddSystem()
    {
        var result = EditSystemDialog?.Invoke(null);
        if (result?.config == null) return;
        settings.Systems.Add(result.Value.config);
        Save();
        var item = new SystemItem(result.Value.config);
        Systems.Add(item);
        SelectedSystem = item;
    }

    [RelayCommand]
    private void EditSystem()
    {
        var current = SelectedSystem;
        if (current == null) return;
        var result = EditSystemDialog?.Invoke(current.Config.Clone());
        if (result == null) return;

        if (result.Value.deleted)
        {
            settings.Systems.RemoveAll(s => s.Id == current.Config.Id);
            Storage.DeleteCache(current.Config.Id);
            Save();
            Systems.Remove(current);
            SelectedSystem = Systems.FirstOrDefault();
            if (SelectedSystem == null) SetItems(new List<GameItem>());
            return;
        }
        if (result.Value.config == null) return;

        var updated = result.Value.config;
        var idx = settings.Systems.FindIndex(s => s.Id == updated.Id);
        if (idx >= 0) settings.Systems[idx] = updated;
        Save();
        current.Update(updated);
        OnPropertyChanged(nameof(LaunchButtonText));
        _ = LoadAsync(current);
    }

    [RelayCommand]
    private void OpenAppSettings() => OpenAppSettingsDialog?.Invoke();

    public void ApplyLanguage(string lang)
    {
        settings.Language = lang;
        Save();
        Loc.Instance.Language = lang;
    }

    public void Save()
    {
        try { Storage.SaveSettings(settings); } catch { }
    }
}
