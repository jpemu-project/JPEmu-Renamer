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
        sort = SortOptions.Any(o => o.key == settings.ListSort) ? settings.ListSort : "";
        GameItem.HideExtensions = settings.HideExtensions;
        GameItem.ShowFlags = settings.ShowFlags;
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
            OnPropertyChanged(nameof(SortButtonText));
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
        _ = CountOtherSystemsAsync();
    }

    /// <summary>同じ機種のスキャンとキャッシュ保存が重ならないようにする</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, object> scanLocks = new();
    private static object ScanLock(string id) => scanLocks.GetOrAdd(id, _ => new object());

    /// <summary>起動時に、選んでいない機種も所有／未所有の数を数えて左の一覧に出す</summary>
    private async Task CountOtherSystemsAsync()
    {
        bool subfolders = settings.ScanSubfolders;
        foreach (var item in Systems.ToList())
        {
            if (item.Counted || item == SelectedSystem) continue;
            var sys = item.Config.Clone();
            if (string.IsNullOrWhiteSpace(sys.DatPath) && sys.RomFolders.Count == 0) continue;
            try
            {
                var list = await Task.Run(() =>
                {
                    var dat = string.IsNullOrWhiteSpace(sys.DatPath) ? new List<DatEntry>() : DatParser.Parse(sys.DatPath);
                    List<RomFile> roms;
                    lock (ScanLock(sys.Id))
                    {
                        var cache = Storage.LoadCache(sys.Id);
                        roms = RomScanner.Scan(sys, subfolders, cache, null, CancellationToken.None);
                        Storage.SaveCache(sys.Id, roms);
                    }
                    return Matcher.Match(dat, roms, sys);
                });
                // 数えている間にその機種を開いた場合は、そちらの結果を使う
                if (item.Counted) continue;
                var (owned, missing) = Counts(list);
                item.OwnedCount = owned;
                item.MissingCount = missing;
                item.Counted = true;
            }
            catch { /* DATが読めないなどは、その機種を開いたときに表示する */ }
        }
    }

    private static (int owned, int missing) Counts(List<GameItem> list) =>
        (list.Where(i => i.IsOwned && i.Dat != null).Select(i => i.Dat!.Title).Distinct().Count(),
         list.Count(i => i.Status == MatchStatus.Missing));

    partial void OnSelectedSystemChanged(SystemItem? value)
    {
        if (value == null) return;
        settings.LastSystemId = value.Config.Id;
        Save();
        OnPropertyChanged(nameof(MoveUnknown));
        OnPropertyChanged(nameof(UnknownFolder));
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
                List<RomFile> roms;
                lock (ScanLock(sys.Id))
                {
                    var cache = Storage.LoadCache(sys.Id);
                    roms = RomScanner.Scan(sys, subfolders, cache, progress, ct);
                    Storage.SaveCache(sys.Id, roms);
                }
                return Matcher.Match(dat, roms, sys);
            }, ct);

            if (ct.IsCancellationRequested) return;
            SetItems(list);
            sysItem.OwnedCount = OwnedCount;
            sysItem.MissingCount = MissingCount;
            sysItem.Counted = true;

            StatusText = datError != null
                ? Loc.F("DatError", datError)
                : Loc.F("Loaded", sys.Name, OwnedCount, list.Count(i => i.Status == MatchStatus.NameDiff), MissingCount);
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
        ApplySort();

        (OwnedCount, MissingCount) = Counts(list);
        SelectedItem = list.FirstOrDefault(i => FilterItem(i));
        UpdateSelectedCount();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameItem.IsChecked)) UpdateSelectedCount();
    }

    private void UpdateSelectedCount()
    {
        SelectedCount = Items.Count(i => i.IsChecked && i.CanRename);
        NotifySelection();
    }

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

    // ---------- 並び替え ----------

    /// <summary>並び替えの選択肢（キー、文言のキー）</summary>
    public static readonly (string key, string label)[] SortOptions =
    {
        ("DateOld", "SortDateOld"), ("DateNew", "SortDateNew"), ("Kana", "SortKana"),
        ("Publisher", "SortPublisher"), ("Genre", "SortGenre"), ("Price", "SortPrice"), ("Status", "SortStatus")
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SortButtonText))]
    private string sort = "";

    /// <summary>ボタンの文言（並べ替えていれば、いまの並び順も出す）</summary>
    public string SortButtonText
    {
        get
        {
            var opt = SortOptions.FirstOrDefault(o => o.key == Sort);
            return opt.key == null ? Loc.T("Sort") : Loc.T("Sort") + "：" + Loc.T(opt.label);
        }
    }

    partial void OnSortChanged(string value)
    {
        settings.ListSort = value;
        Save();
        ApplySort();
    }

    private void ApplySort()
    {
        if (ItemsView is not ListCollectionView view) return;
        view.CustomSort = string.IsNullOrEmpty(Sort) ? null : new ItemSorter(Sort);
    }

    /// <summary>
    /// 一覧の並び順。値が無いもの（発売日不明など）は最後。同じ値どうしはタイトルのあいうえお順。
    /// DATに無いファイルは、どの並び順でも最後にまとめる。
    /// </summary>
    private sealed class ItemSorter : System.Collections.IComparer
    {
        private readonly string key;
        public ItemSorter(string key) => this.key = key;

        public int Compare(object? x, object? y)
        {
            if (x is not GameItem a || y is not GameItem b) return 0;
            int c = (a.Status == MatchStatus.NotInDat).CompareTo(b.Status == MatchStatus.NotInDat);
            if (c != 0) return c;
            c = key switch
            {
                "DateOld" => CompareNullable(DateOf(a), DateOf(b), false),
                "DateNew" => CompareNullable(DateOf(a), DateOf(b), true),
                "Publisher" => CompareText(a.Publisher, b.Publisher),
                "Genre" => CompareText(a.Genre, b.Genre),
                "Price" => CompareNullable(PriceOf(a), PriceOf(b), false),
                "Status" => StatusRank(a.Status).CompareTo(StatusRank(b.Status)),
                _ => 0
            };
            if (c != 0) return c;
            return string.CompareOrdinal(JapaneseReading.Key(a.Title), JapaneseReading.Key(b.Title));
        }

        /// <summary>空の値は最後</summary>
        private static int CompareText(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return string.IsNullOrWhiteSpace(a).CompareTo(string.IsNullOrWhiteSpace(b));
            return JapaneseReading.Compare.Compare(a, b, JapaneseReading.Options);
        }

        private static int CompareNullable<T>(T? a, T? b, bool descending) where T : struct, IComparable<T>
        {
            if (a == null || b == null) return (a == null).CompareTo(b == null);
            int c = a.Value.CompareTo(b.Value);
            return descending ? -c : c;
        }

        private static int StatusRank(MatchStatus s) => s switch
        {
            MatchStatus.NameDiff => 0,
            MatchStatus.Duplicate => 1,
            MatchStatus.NotInDat => 2,
            MatchStatus.Match => 3,
            _ => 4
        };

        /// <summary>「1985年9月13日」「1985年9月」「1985年」を日付に（日・月が無ければ月初・年初）</summary>
        private static DateTime? DateOf(GameItem g)
        {
            var m = System.Text.RegularExpressions.Regex.Match(g.ReleaseDate, @"(\d{4})\s*年(?:\s*(\d{1,2})\s*月)?(?:\s*(\d{1,2})\s*日)?");
            if (!m.Success)
            {
                if (DateTime.TryParse(g.ReleaseDate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d)) return d;
                return null;
            }
            int y = int.Parse(m.Groups[1].Value), mo = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 1, da = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 1;
            try { return new DateTime(y, Math.Clamp(mo, 1, 12), Math.Clamp(da, 1, DateTime.DaysInMonth(y, Math.Clamp(mo, 1, 12)))); }
            catch { return null; }
        }

        /// <summary>「4,900円」などから数字だけを取り出す</summary>
        private static int? PriceOf(GameItem g)
        {
            var digits = new string(g.Price.Normalize(System.Text.NormalizationForm.FormKC).TakeWhile(ch => ch != '(' && ch != '（').Where(char.IsDigit).ToArray());
            return digits.Length > 0 && digits.Length < 9 ? int.Parse(digits) : null;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ItemsView.Refresh();
        NotifySelection();
    }

    partial void OnFilterChanged(string value)
    {
        settings.ListFilter = value;
        Save();
        ItemsView.Refresh();
        NotifySelection();
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

    /// <summary>いま表示されているリネーム候補（名前違い）。見出しのチェックはこれをまとめて選ぶ</summary>
    private IEnumerable<GameItem> VisibleCandidates => ItemsView.Cast<GameItem>().Where(i => i.CanRename);

    public bool HasRenameCandidates => VisibleCandidates.Any();

    /// <summary>見出しのチェックの状態（すべて選択＝true、一部＝null、なし＝false）</summary>
    public bool? AllChecked
    {
        get
        {
            var list = VisibleCandidates.ToList();
            if (list.Count == 0) return false;
            int on = list.Count(i => i.IsChecked);
            return on == list.Count ? true : on == 0 ? false : null;
        }
        // 見出しのチェックを押すと、一部選択・未選択からは「すべて選択」、すべて選択からは「すべて解除」になる
        set
        {
            bool check = value == true;
            foreach (var i in VisibleCandidates.ToList()) i.IsChecked = check;
            NotifySelection();
        }
    }

    private void NotifySelection()
    {
        OnPropertyChanged(nameof(AllChecked));
        OnPropertyChanged(nameof(HasRenameCandidates));
    }

    [RelayCommand]
    private async Task RunRename()
    {
        var sys = SelectedSystem?.Config;
        if (sys == null || IsBusy) return;
        var targets = Items.Where(i => i.IsChecked && i.CanRename).ToList();
        // DATに無いファイルと重複ファイル（同じタイトルのファイルが既にあるもの）を移動する設定なら、
        // それも一緒に行う（取り消しで元の場所に戻る）
        bool move = sys.MoveUnknown && !string.IsNullOrWhiteSpace(sys.UnknownFolder);
        List<string> Paths(MatchStatus st) => move
            ? Items.Where(i => i.Status == st && i.Rom != null).Select(i => i.Rom!.Path).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            : new List<string>();
        var notInDat = Paths(MatchStatus.NotInDat);
        var duplicates = Paths(MatchStatus.Duplicate);
        var unknown = notInDat.Concat(duplicates).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (targets.Count == 0 && unknown.Count == 0) return;
        var question = unknown.Count > 0
            ? Loc.F("ConfirmRenameMove", targets.Count, notInDat.Count, sys.UnknownFolder, duplicates.Count)
            : Loc.F("ConfirmRename", targets.Count);
        if (Confirm?.Invoke(question) != true) return;

        IsBusy = true;
        bool insideZip = settings.RenameInsideZip;
        var result = await Task.Run(() =>
        {
            var r = Renamer.Execute(targets, sys, insideZip);
            if (unknown.Count > 0) Renamer.MoveFiles(unknown, sys.UnknownFolder, r);
            return r;
        });
        if (result.Run.Records.Count > 0)
        {
            var history = Storage.LoadHistory();
            history.Add(result.Run);
            Storage.SaveHistory(history);
        }
        IsBusy = false;

        var msg = Loc.F("RenameDone", result.Run.Records.Count - result.Moved.Count, result.Skipped);
        if (result.Moved.Count > 0) msg += "\n" + Loc.F("UnknownMoved", result.Moved.Count);
        if (result.Errors.Count > 0) msg += "\n\n" + string.Join("\n", result.Errors.Take(10));
        Info?.Invoke(msg);
        await ReloadAsync();
    }

    /// <summary>リネームのときに、DATに無いファイルを移動する（機種ごと）</summary>
    public bool MoveUnknown
    {
        get => SelectedSystem?.Config.MoveUnknown ?? false;
        set
        {
            if (SelectedSystem == null || SelectedSystem.Config.MoveUnknown == value) return;
            SelectedSystem.Config.MoveUnknown = value;
            Save();
            OnPropertyChanged();
        }
    }

    /// <summary>DATに無いファイルの移動先（機種ごと）</summary>
    public string UnknownFolder
    {
        get => SelectedSystem?.Config.UnknownFolder ?? "";
        set
        {
            value = (value ?? "").Trim().Trim('"');
            if (SelectedSystem == null || SelectedSystem.Config.UnknownFolder == value) return;
            SelectedSystem.Config.UnknownFolder = value;
            Save();
            OnPropertyChanged();
        }
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

    /// <summary>機種の並び順を変える（ドラッグ＆ドロップ）。保存は SaveSystemOrder でまとめて行う</summary>
    public void MoveSystem(SystemItem item, int newIndex)
    {
        int oldIndex = Systems.IndexOf(item);
        if (oldIndex < 0 || newIndex < 0 || newIndex >= Systems.Count || oldIndex == newIndex) return;
        Systems.Move(oldIndex, newIndex);
    }

    /// <summary>画面の並び順を設定に書き戻して保存する</summary>
    public void SaveSystemOrder()
    {
        var order = Systems.Select(s => s.Config.Id).ToList();
        settings.Systems.Sort((a, b) => order.IndexOf(a.Id).CompareTo(order.IndexOf(b.Id)));
        Save();
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
