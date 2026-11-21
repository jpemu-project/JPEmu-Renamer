# JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
# libretro 公式の「コア情報ファイル」（*.info）から、アプリに入れる CoreInfo.json を作る。
#
# 使い方:
#   1. https://buildbot.libretro.com/assets/frontend/info.zip を展開したフォルダを用意する
#   2. https://buildbot.libretro.com/nightly/windows/x86_64/latest/.index-extended から
#      Windows 用に配布されているコアの名前（xxx_libretro.dll.zip の xxx）を1行ずつ書いたファイルを用意する
#   3. pwsh tools/gen-coreinfo.ps1 -InfoDir <展開したフォルダ> -WindowsList <名前の一覧>
param(
    [Parameter(Mandatory)] [string] $InfoDir,
    [Parameter(Mandatory)] [string] $WindowsList,
    [string] $Out = (Join-Path $PSScriptRoot '..\src\JPEmuRenamer\Emulation\CoreInfo.json')
)
$ErrorActionPreference = 'Stop'
$keep = 'Emulator', 'Game engine', 'Computer', 'Virtual machine'
$names = Get-Content $WindowsList | ForEach-Object { $_.Trim() } | Where-Object { $_ } | Sort-Object -Unique

function Read-Info([string] $path) {
    $d = @{}
    foreach ($line in [IO.File]::ReadAllLines($path)) {
        if ($line -match '^\s*([A-Za-z0-9_]+)\s*=\s*"?(.*?)"?\s*$') { $d[$Matches[1]] = $Matches[2] }
    }
    $d
}

$cores = foreach ($n in $names) {
    $f = Join-Path $InfoDir "$($n)_libretro.info"
    if (-not (Test-Path $f)) { continue }
    $i = Read-Info $f
    if ($i['categories'] -and -not ($keep | Where-Object { $i['categories'] -like "*$_*" })) { continue }
    $display = $i['display_name']
    if ($display -match '\(([^()]+)\)\s*$') { $display = $Matches[1] }
    if (-not $display) { $display = $i['corename'] }
    if (-not $display) { $display = $n }
    $fw = @()
    $count = 0; [int]::TryParse([string]$i['firmware_count'], [ref]$count) | Out-Null
    for ($k = 0; $k -lt [Math]::Max($count, 64); $k++) {
        $p = $i["firmware$($k)_path"]; if (-not $p) { $p = $i["firmware$($k)"] }  # hyperscanemu などは _path が無い
        if (-not $p) { if ($k -ge $count) { break } else { continue } }
        $fw += , [ordered]@{ p = $p.Replace('\', '/'); d = [string]$i["firmware$($k)_desc"]; o = ($i["firmware$($k)_opt"] -ne 'false') }
    }
    [ordered]@{
        n  = $n
        d  = $display
        s  = [string]$i['systemname']
        x  = @(([string]$i['supported_extensions']).Split('|') | ForEach-Object { $_.Trim().ToLowerInvariant() } | Where-Object { $_ })
        hw = [string]$i['required_hw_api']
        db = @(([string]$i['database']).Split('|') | ForEach-Object { $_.Trim() } | Where-Object { $_ })
        fw = $fw
    }
}
$json = [ordered]@{ source = 'libretro core info (https://github.com/libretro/libretro-core-info)'; cores = @($cores) } | ConvertTo-Json -Depth 6 -Compress
[IO.File]::WriteAllText([IO.Path]::GetFullPath($Out), $json, [Text.UTF8Encoding]::new($false))
"$(@($cores).Count) cores -> $Out"
