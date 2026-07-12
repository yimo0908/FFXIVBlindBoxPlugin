using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;

namespace BlindBoxPlugin.Windows
{
    public class ConfigWindow : Window
    {
        private string text = "";
        private List<string> result = [];
        private string resultText = "";
        private Dictionary<string, uint>? itemNameIndex;

        public ConfigWindow()
            : base("盲盒设置")
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(480, 270),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
            };
            SizeCondition = ImGuiCond.Always;
        }

        public override void Draw()
        {
            if (ImGui.BeginTabBar("BlindBoxTabBar", ImGuiTabBarFlags.AutoSelectNewTabs))
            {
                if (ImGui.BeginTabItem("获取物品Id"))
                {
                    var windowsWidth = ImGui.GetWindowWidth();
                    var text = this.text;
                    ImGui.SetNextItemWidth(windowsWidth * 0.5f - 22);
                    if (
                        ImGui.InputTextMultiline(
                            "##text",
                            ref text,
                            ushort.MaxValue,
                            new Vector2(0, 0)
                        )
                    )
                    {
                        this.text = text;
                    }
                    ImGui.SameLine();
                    ImGui.Text("=>");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(windowsWidth * 0.5f - 22);
                    ImGui.InputTextMultiline(
                        "##result",
                        ref resultText,
                        ushort.MaxValue,
                        new Vector2(0, 0),
                        ImGuiInputTextFlags.ReadOnly
                    );

                    if (ImGui.Button("获取"))
                    {
                        var index = GetItemNameIndex();
                        var items = this.text.Split('\n');
                        List<string> itemIds = new List<string>();

                        foreach (var item in items)
                        {
                            var rowID = index.TryGetValue(item, out var id) ? id.ToString() : "名称有误";
                            itemIds.Add(rowID);
                        }
                        result = itemIds;
                        resultText = string.Join("\n", result);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("输出到剪贴板"))
                    {
                        ImGui.SetClipboardText(string.Join(",", result));
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("清空"))
                    {
                        this.text = string.Empty;
                        result = new List<string>();
                        resultText = string.Empty;
                    }

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }

        /// <summary>构建物品名称 → RowId 的索引（懒加载缓存）。</summary>
        private Dictionary<string, uint> GetItemNameIndex()
        {
            if (itemNameIndex != null)
                return itemNameIndex;

            itemNameIndex = new Dictionary<string, uint>();
            var sheet = Plugin.ItemSheet;
            foreach (var row in sheet)
            {
                var name = row.Name.ToString();
                if (!string.IsNullOrEmpty(name))
                    itemNameIndex.TryAdd(name, row.RowId);
            }
            return itemNameIndex;
        }
    }
}
