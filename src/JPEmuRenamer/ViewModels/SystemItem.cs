// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;
using JPEmuRenamer.Ui;

namespace JPEmuRenamer.ViewModels;

/// <summary>左の機種一覧の1行</summary>
public partial class SystemItem : ObservableObject
{
    public SystemItem(SystemConfig config)
    {
        Config = config;
    }

    public SystemConfig Config { get; private set; }

    public string Name => Config.Name;
    public Geometry Icon => IconFactory.Get(Config.IconKind);
    public Brush IconBrush => BrushUtil.FromHex(Config.Color);
    public ImageSource? CustomIcon => BrushUtil.LoadImage(Config.CustomIconPath);
    public bool HasCustomIcon => CustomIcon != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountText))]
    private int ownedCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountText))]
    private int missingCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountText))]
    private bool counted;

    public string CountText => Counted ? Loc.F("OwnedMissing", OwnedCount, MissingCount) : "";

    public void Update(SystemConfig config)
    {
        Config = config;
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Icon));
        OnPropertyChanged(nameof(IconBrush));
        OnPropertyChanged(nameof(CustomIcon));
        OnPropertyChanged(nameof(HasCustomIcon));
    }

    public void RefreshTexts() => OnPropertyChanged(nameof(CountText));
}
