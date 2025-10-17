using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;

namespace BlindBoxPlugin.Windows
{
    public class ConfigWindow : Window, IDisposable
    {
        private readonly Configuration _configuration;

        private string _text = "";
        private List<string> _result = [];

        // We give this window a constant ID using ###
        // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
        // and the window ID will always be "###XYZ counter window" for ImGui
        public ConfigWindow(Plugin plugin)
            : base("盲盒设置")
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(480, 270),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
            };
            SizeCondition = ImGuiCond.Always;

            _configuration = plugin.Configuration;
        }

        public void Dispose() { }

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
                    var result = string.Join("\n", _result);
                    ImGui.InputTextMultiline(
                        "##result",
                        ref result,
                        ushort.MaxValue,
                        new Vector2(0, 0),
                        ImGuiInputTextFlags.ReadOnly
                    );

                    if (ImGui.Button("获取"))
                    {
                        var items = _text.Split('\n');
                        List<string> itemIds = [];

                        foreach (var item in items)
                        {
                            var sheet = Plugin.DataManager.GetExcelSheet<Item>();
                            var i = sheet.FirstOrDefault(i => i.Name == item);
                            var rowId = i.RowId != 0 ? i.RowId.ToString() : "名称有误";
                            itemIds.Add(rowId);
                        }
                        _result = itemIds;
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("输出到剪贴板"))
                    {
                        ImGui.SetClipboardText(string.Join(",", _result));
                    }

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }
    }
}
