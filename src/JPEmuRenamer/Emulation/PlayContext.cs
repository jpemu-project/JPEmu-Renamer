// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;

namespace JPEmuRenamer.Emulation;

/// <summary>
/// プレイ設定の3つの層（全体・機種・ゲーム）と、その保存方法をまとめたもの。
/// 設定画面とプレイ画面が同じものを参照し、変更はその場で保存・反映する。
/// </summary>
public sealed class PlayContext
{
    public PlayContext(AppSettings app, SystemConfig? system, string? gameKey, string? gameTitle, Action saveApp)
    {
        App = app;
        System = system;
        GameKey = gameKey;
        GameTitle = gameTitle;
        SaveApp = saveApp;
        Game = gameKey != null ? Storage.LoadGame(gameKey) : null;
    }

    public AppSettings App { get; }
    /// <summary>機種（保存されている設定そのもの。複製ではない）</summary>
    public SystemConfig? System { get; }
    public string? GameKey { get; }
    public string? GameTitle { get; }
    public GameOverrides? Game { get; }
    private Action SaveApp { get; }

    public string CoreName => System != null ? CoreCatalog.CoreNameFor(System) : "";

    /// <summary>"Global" / "System" / "Game" の層</summary>
    public PlaySettings Layer(string scope) => scope switch
    {
        "Game" when Game != null => Game.Play,
        "System" when System != null => System.Play,
        _ => App.Play
    };

    /// <summary>指定した層より下の層だけを重ねた値（上書きしていないときに表示する値）</summary>
    public PlaySettings Inherited(string scope) => scope switch
    {
        "Game" => PlaySettings.Resolve(App.Play, System?.Play),
        "System" => PlaySettings.Resolve(App.Play),
        _ => PlaySettings.Defaults()
    };

    public PlaySettings Resolve() => PlaySettings.Resolve(App.Play, System?.Play, Game?.Play);

    public string BiosDir => string.IsNullOrWhiteSpace(App.BiosDir) ? Storage.DefaultBiosDir : App.BiosDir;

    public void Save()
    {
        SaveApp();
        if (GameKey != null && Game != null) Storage.SaveGame(GameKey, Game);
    }
}
