// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using JPEmuRenamer.Services;

namespace JPEmuRenamer.Views;

/// <summary>マニュアル。「ゲームの遊び方」と「リネーム」を切り替え、左の目次から各項目へ移動できる</summary>
public partial class HelpWindow : Window
{
    public HelpWindow(string tab = "Play")
    {
        InitializeComponent();
        if (tab == "Rename") RenameTab.IsChecked = true;
        else PlayTab.IsChecked = true;
    }

    /// <summary>開いている画面に合わせて、遊び方かリネームのどちらかを表示する</summary>
    public void ShowTab(string tab)
    {
        if (tab == "Rename") RenameTab.IsChecked = true;
        else PlayTab.IsChecked = true;
    }

    private void OnTabChecked(object sender, RoutedEventArgs e)
    {
        var lang = Loc.Instance.Language;
        Render(RenameTab.IsChecked == true ? HelpContent.Rename(lang) : HelpContent.Play(lang));
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    // ---------- 表示 ----------

    private Brush B(string key) => (Brush)FindResource(key);

    private void Render(string source)
    {
        var doc = new FlowDocument
        {
            FontFamily = (FontFamily)FindResource("UiFont"),
            FontSize = 14.5,
            Foreground = B("TextBrush"),
            Background = B("BgBrush"),
            PagePadding = new Thickness(36, 24, 36, 40),
            LineHeight = 25,
            TextAlignment = TextAlignment.Left,
            MaxPageWidth = 900,
            ColumnWidth = double.PositiveInfinity
        };
        TocPanel.Children.Clear();

        var lines = source.Replace("\r\n", "\n").Split('\n');
        List? list = null;
        Table? table = null;
        TableRowGroup? rows = null;

        void EndBlocks()
        {
            list = null;
            table = null;
            rows = null;
        }

        foreach (var raw in lines)
        {
            var line = raw.TrimEnd();
            if (line.Length == 0)
            {
                EndBlocks();
                continue;
            }

            if (line.StartsWith("# "))
            {
                EndBlocks();
                var p = new Paragraph(new Run(line[2..]))
                {
                    FontSize = 23,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, doc.Blocks.Count == 0 ? 0 : 30, 0, 8),
                    BorderBrush = B("LineBrush"),
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding = new Thickness(0, 0, 0, 6)
                };
                doc.Blocks.Add(p);
                AddToc(line[2..], p, false);
            }
            else if (line.StartsWith("## "))
            {
                EndBlocks();
                var p = new Paragraph(new Run(line[3..]))
                {
                    FontSize = 17.5,
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 18, 0, 4)
                };
                doc.Blocks.Add(p);
                AddToc(line[3..], p, true);
            }
            else if (line.StartsWith("- "))
            {
                table = null;
                rows = null;
                if (list == null)
                {
                    list = new List { MarkerStyle = TextMarkerStyle.Disc, Margin = new Thickness(0, 4, 0, 8), Padding = new Thickness(22, 0, 0, 0) };
                    doc.Blocks.Add(list);
                }
                var p = new Paragraph { Margin = new Thickness(0, 2, 0, 2) };
                AddInlines(p.Inlines, line[2..]);
                list.ListItems.Add(new ListItem(p));
            }
            else if (line.StartsWith("|"))
            {
                list = null;
                var cells = line.Trim('|').Split('|').Select(c => c.Trim()).ToList();
                bool header = table == null;
                if (table == null)
                {
                    table = new Table
                    {
                        CellSpacing = 0,
                        Margin = new Thickness(0, 8, 0, 12),
                        BorderBrush = B("LineBrush"),
                        BorderThickness = new Thickness(1)
                    };
                    // 最後の列（説明）を広く、最初の列（項目名）は狭めにする
                    for (int i = 0; i < cells.Count; i++)
                    {
                        double w = i == cells.Count - 1 ? (cells.Count == 2 ? 1.6 : 1.2) : (i == 0 ? 0.8 : 0.6);
                        table.Columns.Add(new TableColumn { Width = new GridLength(w, GridUnitType.Star) });
                    }
                    rows = new TableRowGroup();
                    table.RowGroups.Add(rows);
                    doc.Blocks.Add(table);
                }
                var row = new TableRow { Background = header ? B("Panel3Brush") : (rows!.Rows.Count % 2 == 0 ? B("Panel2Brush") : B("PanelBrush")) };
                foreach (var c in cells)
                {
                    var p = new Paragraph { Margin = new Thickness(0), FontWeight = header ? FontWeights.SemiBold : FontWeights.Normal };
                    AddInlines(p.Inlines, c);
                    row.Cells.Add(new TableCell(p)
                    {
                        Padding = new Thickness(10, 6, 10, 6),
                        BorderBrush = B("LineBrush"),
                        BorderThickness = new Thickness(0, 0, 1, 1)
                    });
                }
                rows!.Rows.Add(row);
            }
            else if (line.StartsWith("> "))
            {
                EndBlocks();
                var p = new Paragraph
                {
                    Background = B("Panel2Brush"),
                    BorderBrush = B("AccentBrush"),
                    BorderThickness = new Thickness(3, 0, 0, 0),
                    Padding = new Thickness(14, 8, 14, 8),
                    Margin = new Thickness(0, 8, 0, 10),
                    Foreground = B("SubBrush")
                };
                AddInlines(p.Inlines, line[2..]);
                doc.Blocks.Add(p);
            }
            else
            {
                EndBlocks();
                var p = new Paragraph { Margin = new Thickness(0, 4, 0, 6) };
                AddInlines(p.Inlines, line);
                doc.Blocks.Add(p);
            }
        }

        Viewer.Document = doc;
    }

    /// <summary>**太字** と `キー` を読み取って文字を並べる</summary>
    private void AddInlines(InlineCollection target, string text)
    {
        foreach (var part in Regex.Split(text, @"(\*\*[^*]+\*\*|`[^`]+`)"))
        {
            if (part.Length == 0) continue;
            if (part.StartsWith("**") && part.EndsWith("**") && part.Length > 4)
            {
                target.Add(new Bold(new Run(part[2..^2])));
            }
            else if (part.StartsWith('`') && part.EndsWith('`') && part.Length > 2)
            {
                // キーやファイル名は枠で囲んで目立たせる
                var tb = new TextBlock(new Run(part[1..^1]))
                {
                    FontFamily = new FontFamily("Consolas, Yu Gothic UI"),
                    FontSize = 13,
                    Foreground = B("TextBrush")
                };
                var box = new Border
                {
                    Child = tb,
                    Background = B("Panel3Brush"),
                    BorderBrush = B("Line2Brush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(5, 0, 5, 1),
                    Margin = new Thickness(1, 0, 1, 0)
                };
                target.Add(new InlineUIContainer(box) { BaselineAlignment = BaselineAlignment.Center });
            }
            else
            {
                target.Add(new Run(part));
            }
        }
    }

    private void AddToc(string text, Block target, bool sub)
    {
        // 目次は左寄せで並べる（ボタンの見た目は中央寄せなので、文字と枠で作る）
        var item = new Border
        {
            Child = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                FontSize = sub ? 13 : 14,
                FontWeight = sub ? FontWeights.Normal : FontWeights.SemiBold,
                Foreground = sub ? B("SubBrush") : B("TextBrush")
            },
            Background = Brushes.Transparent,
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(sub ? 22 : 10, 5, 8, 5),
            Margin = new Thickness(0, sub ? 0 : 8, 0, 0),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        item.MouseEnter += (_, _) => item.Background = B("Panel3Brush");
        item.MouseLeave += (_, _) => item.Background = Brushes.Transparent;
        item.MouseLeftButtonUp += (_, _) => target.BringIntoView();
        TocPanel.Children.Add(item);
    }
}
