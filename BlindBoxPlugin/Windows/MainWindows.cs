using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;

namespace BlindBoxPlugin.Windows;

public partial class MainWindow : Window, IDisposable
{
    private readonly Plugin _plugin;

    // ── 像素暗色主题常量 ──
    private static readonly Vector4 PixelWindowBg = new(0.06f, 0.06f, 0.08f, 0.96f);
    private static readonly Vector4 PixelChildBg   = new(0.03f, 0.03f, 0.05f, 1f);
    private static readonly Vector4 PixelBorder    = new(0.22f, 0.22f, 0.28f, 0.5f);
    private static readonly Vector4 PixelDim       = new(0.35f, 0.35f, 0.4f, 1f);
    private static readonly Vector4 PixelAccent    = new(0.4f, 0.85f, 1.0f, 1f);
    private static readonly Vector4 PixelTabActive = new(0.12f, 0.12f, 0.16f, 1f);
    private static readonly Vector4 PixelTabHover  = new(0.08f, 0.08f, 0.12f, 1f);
    private static readonly Vector4 PixelButtonBg  = new(0.1f, 0.1f, 0.14f, 1f);

    // ── 物品状态颜色 ──
    private static readonly Vector4 ColorAcquired   = new(0.2f, 0.9f, 0.25f, 1f);
    private static readonly Vector4 ColorMissing    = new(0.85f, 0.85f, 0.85f, 1f);
    private static readonly Vector4 ColorUntradeable= new(0.45f, 0.45f, 0.5f, 1f);
    private static readonly Vector4 ColorUnique     = new(1.0f, 0.78f, 0.25f, 1f);
    private static readonly Vector4 ColorDefault    = new(0.85f, 0.85f, 0.85f, 1f);

    // ── 进度条颜色 ──
    private static readonly uint ProgressColorComplete = ImGui.GetColorU32(new Vector4(0.2f, 0.9f, 0.25f, 1f));
    private static readonly uint ProgressColorHigh     = ImGui.GetColorU32(new Vector4(0.3f, 0.7f, 1f, 1f));
    private static readonly uint ProgressColorMid      = ImGui.GetColorU32(new Vector4(1f, 0.78f, 0.25f, 1f));
    private static readonly uint ProgressColorLow      = ImGui.GetColorU32(new Vector4(1f, 0.35f, 0.35f, 1f));

    private const int PushedColorCount = 10;
    private const int PushedVarCount = 8;

    private static readonly string[] DisplayModeLabels = { "所有", "已获得", "未获得" };

    private readonly StringBuilder _sb = new();

    public MainWindow(Plugin plugin)
        : base("盲盒信息##BlindBoxMain", ImGuiWindowFlags.NoScrollbar)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(520, 320),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
        SizeCondition = ImGuiCond.FirstUseEver;
        _plugin = plugin;

        TitleBarButtons.Add(new()
        {
            Icon = FontAwesomeIcon.Cog,
            IconOffset = new(1),
            Click = _ => _plugin.ToggleConfigUi()
        });
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        ImGui.PushStyleColor(ImGuiCol.WindowBg,       PixelWindowBg);
        ImGui.PushStyleColor(ImGuiCol.ChildBg,        PixelChildBg);
        ImGui.PushStyleColor(ImGuiCol.Border,         PixelBorder);
        ImGui.PushStyleColor(ImGuiCol.Separator,      PixelDim);
        ImGui.PushStyleColor(ImGuiCol.FrameBg,        PixelChildBg);
        ImGui.PushStyleColor(ImGuiCol.Button,         PixelButtonBg);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered,  PixelTabHover);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive,   PixelTabActive);
        ImGui.PushStyleColor(ImGuiCol.Text,           ColorDefault);
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram,  PixelAccent);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding,    0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding,     0f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding,     0f);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding,      0f);
        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding,       0f);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize,   1f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize,  1f);
    }

    public override void PostDraw()
    {
        ImGui.PopStyleVar(PushedVarCount);
        ImGui.PopStyleColor(PushedColorCount);
    }

    public override void Draw()
    {
        using var fontScope = Plugin.PluginInterface.UiBuilder.MonoFontHandle.Push();

        // 首次打开或选中项无效时自动选择第一个盲盒
        if (!BlindBoxData.BlindBoxInfoMap.ContainsKey(_plugin.Configuration.SelectedItem)
            && BlindBoxData.BlindBoxInfoMap.Count > 0)
        {
            _plugin.Configuration.SelectedItem = BlindBoxData.BlindBoxInfoMap.Keys.Min();
            _plugin.Configuration.Save();
        }

        // 左右双栏布局
        var windowWidth = ImGui.GetWindowWidth();
        using (var selectorsChild = ImRaii.Child("##Selectors", new Vector2(windowWidth * 0.4f, -1), true))
        {
            if (selectorsChild.Success)
                DrawSelectors();
        }

        ImGui.SameLine();

        using (var contentsChild = ImRaii.Child("##Contents", new Vector2(-1, -1), true))
        {
            if (contentsChild.Success)
                DrawContents();
        }
    }

    private void DrawDisplayModeTabs()
    {
        var modeIndex = (int)_plugin.Configuration.DisplayMode;
        for (int i = 0; i < DisplayModeLabels.Length; i++)
        {
            if (i > 0) ImGui.SameLine();
            var active = i == modeIndex;

            ImGui.PushStyleColor(ImGuiCol.Button,        active ? PixelTabActive : new Vector4(0, 0, 0, 0));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, active ? PixelTabActive : PixelTabHover);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive,  active ? PixelTabActive : PixelTabHover);
            ImGui.PushStyleColor(ImGuiCol.Text,          active ? PixelAccent    : PixelDim);

            if (ImGui.SmallButton($" {DisplayModeLabels[i]} ##displayMode{i}"))
            {
                _plugin.Configuration.DisplayMode = (DisplayMode)i;
                _plugin.Configuration.Save();
            }

            ImGui.PopStyleColor(4);
        }
    }

    private void DrawSectionHeader(string title)
    {
        ImGui.TextColored(PixelAccent, title);
        ImGuiHelpers.ScaledDummy(2);
        DrawPixelSeparator();
        ImGuiHelpers.ScaledDummy(2);
    }

    private void DrawPixelSeparator()
    {
        var avail = ImGui.GetContentRegionAvail().X;
        var charWidth = ImGui.CalcTextSize("─").X;
        if (charWidth <= 0) return;
        var count = Math.Max(1, (int)(avail / charWidth));

        _sb.Clear();
        _sb.Append('─', count);
        ImGui.TextColored(PixelDim, _sb.ToString());
    }

    private void DrawPixelProgress(float ratio, int current, int max)
    {
        const int barWidth = 16;
        var filled = Math.Clamp((int)Math.Round(ratio * barWidth), 0, barWidth);
        var barColor = ImGui.ColorConvertU32ToFloat4(GetProgressColor(ratio));

        _sb.Clear();
        _sb.Append('[');
        _sb.Append('█', filled);
        _sb.Append('░', barWidth - filled);
        _sb.Append("] ");
        _sb.Append(current);
        _sb.Append(" / ");
        _sb.Append(max);
        _sb.Append(" (");
        _sb.Append((int)(ratio * 100));
        _sb.Append("%)");

        ImGui.TextColored(barColor, _sb.ToString());
    }

    private static uint GetProgressColor(float progress) => progress switch
    {
        >= 1f    => ProgressColorComplete,
        >= 0.5f  => ProgressColorHigh,
        >= 0.25f => ProgressColorMid,
        _        => ProgressColorLow,
    };

    private void DrawSelectors()
    {
        DrawSectionHeader("▌ 盲盒列表");

        ImGui.SetNextItemWidth(-1);
        var seriesIndex = Math.Clamp(_plugin.Configuration.SelectedSeriesIndex, 0, BlindBoxData.SeriesGroups.Count - 1);
        var seriesNames = BlindBoxData.SeriesGroups.Select(g => g.Name).ToArray();
        if (ImGui.Combo("##SeriesFilter", ref seriesIndex, seriesNames, seriesNames.Length))
        {
            _plugin.Configuration.SelectedSeriesIndex = seriesIndex;
            _plugin.Configuration.Save();
        }

        ImGuiHelpers.ScaledDummy(2);

        var filter = BlindBoxData.SeriesGroups[seriesIndex].ItemIds;
        var filtered = filter != null
            ? BlindBoxData.BlindBoxInfoMap.Where(kvp => filter.Contains(kvp.Key)).OrderBy(kvp => kvp.Key)
            : BlindBoxData.SeriesGroups
                .Skip(1) // 跳过"全部"
                .SelectMany(g => BlindBoxData.BlindBoxInfoMap
                    .Where(kvp => g.ItemIds.Contains(kvp.Key))
                    .OrderBy(kvp => kvp.Key));

        using var table = ImRaii.Table("##SelectorsTable", 1, ImGuiTableFlags.RowBg);
        if (table.Success)
        {
            foreach (var item in filtered)
            {
                var blindbox = item.Value;
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                DrawItemIcon(blindbox.Item, new Vector4(1, 1, 1, 1));
                ImGui.SameLine();
                if (
                    ImGui.Selectable(
                        $" {blindbox.Item.Name.ToString()}",
                        blindbox.Item.RowId == _plugin.Configuration.SelectedItem,
                        ImGuiSelectableFlags.SpanAllColumns
                    )
                )
                {
                    _plugin.Configuration.SelectedItem = blindbox.Item.RowId;
                    _plugin.Configuration.Save();
                }
            }
        }
    }

    private void DrawContents()
    {
        if (
            !BlindBoxData.BlindBoxInfoMap.TryGetValue(
                _plugin.Configuration.SelectedItem,
                out var blindBox
            )
        )
        {
            ImGui.TextColored(PixelDim, "请选择一个盲盒");
            return;
        }

        // 每帧计算一次已获得物品 ID 集合
        var acquiredRowIds = new HashSet<uint>(
            blindBox.Items.Where(GameFunctions.IsUnlocked).Select(i => i.RowId)
        );

        // 顶部：盲盒名称 + 获取进度
        DrawSectionHeader($"▌ {blindBox.Item.Name.ToString()}");

        var totalCount = blindBox.Items.Count;
        var acquiredCount = blindBox.Items.Count(i => acquiredRowIds.Contains(i.RowId));
        var ratio = totalCount > 0 ? (float)acquiredCount / totalCount : 0f;
        DrawPixelProgress(ratio, acquiredCount, totalCount);

        ImGuiHelpers.ScaledDummy(2);
        DrawPixelSeparator();
        ImGuiHelpers.ScaledDummy(2);

        // 显示模式切换
        DrawDisplayModeTabs();
        ImGuiHelpers.ScaledDummy(3);

        // 图例
        ImGui.TextColored(PixelDim, "点击物品名称可复制到剪切板");
        ImGui.TextColored(ColorAcquired, "■ 已获得");
        ImGui.SameLine();
        ImGui.TextColored(ColorMissing, "  ■ 未获得可交易");
        ImGui.SameLine();
        ImGui.TextColored(ColorUntradeable, "  ■ 未获得不可交易");
        ImGui.SameLine();
        ImGui.TextColored(ColorUnique, "  * 仅当前途径");

        ImGuiHelpers.ScaledDummy(3);

        // 物品列表
        var items = _plugin.Configuration.DisplayMode switch
        {
            DisplayMode.All => blindBox.Items.OrderBy(i => i.RowId).ToList(),
            DisplayMode.Acquired => blindBox.Items.Where(i => acquiredRowIds.Contains(i.RowId)).OrderBy(i => i.RowId).ToList(),
            DisplayMode.Missing => blindBox.Items.Where(i => !acquiredRowIds.Contains(i.RowId)).OrderBy(i => i.RowId).ToList(),
            _ => throw new ArgumentOutOfRangeException(),
        };

        using var table = ImRaii.Table("##ItemsTable", 1, ImGuiTableFlags.RowBg);
        if (table.Success)
        {
            foreach (var item in items)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                DrawBlindBoxItem(
                    item,
                    blindBox.UniqueItems.Contains(item.RowId),
                    acquiredRowIds
                );
            }
        }
    }
}
