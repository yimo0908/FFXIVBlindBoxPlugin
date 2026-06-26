using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;

namespace BlindBoxPlugin.Windows
{
    public class ConfigWindow : Window
    {
        private string _text = "";
        private List<string> _result = [];
        private string _resultText = "";
        private Dictionary<string, uint>? _itemNameIndex;

        // We give this window a constant ID using ###
        // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
        // and the window ID will always be "###XYZ counter window" for ImGui
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
                    var text = _text;
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
                        _text = text;
                    }
                    ImGui.SameLine();
                    ImGui.Text("=>");
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(windowsWidth * 0.5f - 22);
                    ImGui.InputTextMultiline(
                        "##result",
                        ref _resultText,
                        ushort.MaxValue,
                        new Vector2(0, 0),
                        ImGuiInputTextFlags.ReadOnly
                    );

                    if (ImGui.Button("获取"))
                    {
                        var index = GetItemNameIndex();
                        var items = _text.Split('\n');
                        List<string> itemIds = new List<string>();

                        foreach (var item in items)
                        {
                            var rowId = index.TryGetValue(item, out var id) ? id.ToString() : "名称有误";
                            itemIds.Add(rowId);
                        }
                        _result = itemIds;
                        _resultText = string.Join("\n", _result);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("输出到剪贴板"))
                    {
                        ImGui.SetClipboardText(string.Join(",", _result));
                    }
                    ImGui.SameLine();
                    // 新增：清空输入和结果按钮
                    if (ImGui.Button("清空"))
                    {
                        _text = string.Empty;
                        _result = new List<string>();
                        _resultText = string.Empty;
                    }

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }

        /// <summary>构建物品名称 → RowId 的索引（懒加载缓存）。</summary>
        private Dictionary<string, uint> GetItemNameIndex()
        {
            if (_itemNameIndex != null)
                return _itemNameIndex;

            _itemNameIndex = new Dictionary<string, uint>();
            var sheet = Plugin.DataManager.GetExcelSheet<Item>();
            foreach (var row in sheet)
            {
                var name = row.Name.ToString();
                if (!string.IsNullOrEmpty(name))
                    _itemNameIndex.TryAdd(name, row.RowId);
            }
            return _itemNameIndex;
        }
    }
}
