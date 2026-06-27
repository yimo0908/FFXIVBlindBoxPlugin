using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;

namespace BlindBoxPlugin.Windows;

public partial class MainWindow : Window, IDisposable
{
    private readonly Plugin _plugin;

    public MainWindow(Plugin plugin)
        : base("盲盒信息")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(500, 300),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
        SizeCondition = ImGuiCond.FirstUseEver;
        _plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // 首次打开或选中项无效时自动选择第一个盲盒
        if (!BlindBoxData.BlindBoxInfoMap.ContainsKey(_plugin.Configuration.SelectedItem)
            && BlindBoxData.BlindBoxInfoMap.Count > 0)
        {
            _plugin.Configuration.SelectedItem = BlindBoxData.BlindBoxInfoMap.Keys.Min();
            _plugin.Configuration.Save();
        }

        // 选择盲盒显示内容
        ImGui.Text("点击物品名称可复制到剪切板。");
        ImGui.TextColored(new Vector4(0, 1, 0, 1), "已获得为绿色，");
        ImGui.SameLine();
        ImGui.Text("未获得可交易为白色，");
        ImGui.SameLine();
        ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1), "未获得不可交易为灰色，");
        ImGui.SameLine();
        ImGui.Text("带“*”意为仅可从当前途径获取。");
        var displayModeIndex = (int)_plugin.Configuration.DisplayMode;
        ImGui.Text("显示物品的种类：");
        ImGui.SameLine();
        if (ImGui.RadioButton("所有", ref displayModeIndex, 0))
        {
            _plugin.Configuration.DisplayMode = DisplayMode.All;
            _plugin.Configuration.Save();
        }
        ImGui.SameLine();
        if (ImGui.RadioButton("已获得", ref displayModeIndex, 1))
        {
            _plugin.Configuration.DisplayMode = DisplayMode.Acquired;
            _plugin.Configuration.Save();
        }
        ImGui.SameLine();
        if (ImGui.RadioButton("未获得", ref displayModeIndex, 2))
        {
            _plugin.Configuration.DisplayMode = DisplayMode.Missing;
            _plugin.Configuration.Save();
        }

        // 盲盒选择
        var windowsWidth = ImGui.GetWindowWidth();
        if (ImGui.BeginChild("Selectors", new Vector2(windowsWidth * 0.4f, -1), true))
        {
            ImGui.SetNextItemWidth(-1);
            var seriesIndex = Math.Clamp(_plugin.Configuration.SelectedSeriesIndex, 0, BlindBoxData.SeriesGroups.Count - 1);
            var seriesNames = BlindBoxData.SeriesGroups.Select(g => g.Name).ToArray();
            if (ImGui.Combo("##SeriesFilter", ref seriesIndex, seriesNames, seriesNames.Length))
            {
                _plugin.Configuration.SelectedSeriesIndex = seriesIndex;
                _plugin.Configuration.Save();
            }

            var filter = BlindBoxData.SeriesGroups[seriesIndex].ItemIds;
            var filtered = filter != null
                ? BlindBoxData.BlindBoxInfoMap.Where(kvp => filter.Contains(kvp.Key)).OrderBy(kvp => kvp.Key)
                : BlindBoxData.BlindBoxInfoMap.OrderBy(kvp => kvp.Key);

            if (ImGui.BeginTable("SelectorsTable", 1, ImGuiTableFlags.RowBg))
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

                ImGui.EndTable();
            }

            ImGui.EndChild();
        }

        ImGui.SameLine();

        // 盲盒内容
        if (ImGui.BeginChild("Contents", new Vector2(-1, -1), true))
        {
            if (
                BlindBoxData.BlindBoxInfoMap.TryGetValue(
                    _plugin.Configuration.SelectedItem,
                    out var blindBox
                )
            )
            {
                // 每帧计算一次已获得物品 ID 集合，避免 O(n²) native 调用
                var acquiredRowIds = new HashSet<uint>(
                    blindBox.Items.Where(GameFunctions.IsUnlocked).Select(i => i.RowId)
                );

                var items = _plugin.Configuration.DisplayMode switch
                {
                    DisplayMode.All => blindBox.Items.OrderBy(i => i.RowId).ToList(),
                    DisplayMode.Acquired => blindBox.Items.Where(i => acquiredRowIds.Contains(i.RowId)).OrderBy(i => i.RowId).ToList(),
                    DisplayMode.Missing => blindBox.Items.Where(i => !acquiredRowIds.Contains(i.RowId)).OrderBy(i => i.RowId).ToList(),
                    _ => throw new ArgumentOutOfRangeException(),
                };

                if (ImGui.BeginTable("ItemsTable", 1, ImGuiTableFlags.RowBg))
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

                    ImGui.EndTable();
                }
            }
            else
                ImGui.Text("请选择一个盲盒");

            ImGui.EndChild();
        }
    }
}
