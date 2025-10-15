using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.Sheets;

namespace BlindBoxPlugin.Windows
{
    public class MainWindow : Window, IDisposable
    {
        private readonly Plugin _plugin;
        public MainWindow(Plugin plugin) : base("盲盒信息")
        {

            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(500, 300),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };
            SizeCondition = ImGuiCond.FirstUseEver;
            _plugin = plugin;
        }

        public void Dispose() { }

        public override void Draw()
        {
            // 选择盲盒显示内容
            var displayModes = Enum.GetNames<DisplayMode>();
            var displayModeIndex = (int)_plugin.Configuration.DisplayMode;
            ImGui.SetNextItemWidth(80);
            ImGui.Text("点击物品名称可复制到剪切板。");
            if (ImGui.Combo("显示物品的种类", ref displayModeIndex, DisplayModeNames.Names(), displayModes.Length))
            {
                _plugin.Configuration.DisplayMode = (DisplayMode)displayModeIndex;
                _plugin.Configuration.Save();
            }

            // 盲盒选择
            var windowsWidth = ImGui.GetWindowWidth();
            if (ImGui.BeginChild("Selectors", new Vector2(windowsWidth * 0.4f, -1), true))
            {
                foreach (var item in BlindBoxData.BlindBoxInfoMap)
                {
                    var blindbox = item.Value;
                    if (ImGui.Selectable(blindbox.Item.Name.ToString(), blindbox.Item.RowId == _plugin.Configuration.SelectedItem))
                    {
                        _plugin.Configuration.SelectedItem = blindbox.Item.RowId;
                        _plugin.Configuration.Save();
                    }
                }
                ImGui.EndChild();
            }
            ImGui.SameLine();

            // 盲盒内容
            if (ImGui.BeginChild("Contents", new Vector2(-1, -1), true))
            {
                if (BlindBoxData.BlindBoxInfoMap.TryGetValue(_plugin.Configuration.SelectedItem, out var blindBox))
                {
                    switch (_plugin.Configuration.DisplayMode)
                    {
                        case DisplayMode.All:
                            foreach (var item in blindBox.Items)
                            {
                                DrawBlindBoxItem(item.Name.ToString(), blindBox.UniqueItems.Contains(item.RowId), item.RowId);
                            }
                            break;
                        case DisplayMode.Acquired:
                            foreach (var item in blindBox.AcquiredItems)
                            {
                                DrawBlindBoxItem(item.Name.ToString(), blindBox.UniqueItems.Contains(item.RowId), item.RowId);
                            }
                            break;
                        case DisplayMode.Missing:
                            foreach (var item in blindBox.MissingItems)
                            {
                                DrawBlindBoxItem(item.Name.ToString(), blindBox.UniqueItems.Contains(item.RowId), item.RowId);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    ImGui.Text("请选择一个盲盒");
                }
                ImGui.EndChild();
            }
        }

        private void LinkItemToChat(uint item)
        {
            var itemSheet = Plugin.DataManager.GetExcelSheet<Item>();
            var id = itemSheet.GetRowOrDefault(item);
            var rarity = (id?.Rarity ?? 0);
            var itemName = (id?.Name.ToString() ?? "");

            var payloadList = new List<Payload>
            {
                new UIForegroundPayload((ushort) (0x223 + rarity * 2)),
                new UIGlowPayload((ushort) (0x224 + rarity * 2)),
                new ItemPayload(item),
                new UIForegroundPayload(500),
                new UIGlowPayload(501),
                new TextPayload($"{(char)SeIconChar.LinkMarker}"),
                new UIForegroundPayload(0),
                new UIGlowPayload(0),
                new TextPayload(itemName),
                new RawPayload([0x02, 0x27, 0x07, 0xCF, 0x01, 0x01, 0x01, 0xFF, 0x01, 0x03]),
                new RawPayload([0x02, 0x13, 0x02, 0xEC, 0x03])
            };
            Plugin.ChatGui.Print(new XivChatEntry { Message = new SeString(payloadList) });
        }

        private void CopyItemNameToClipboard(uint itemId)
        {
            var itemSheet = Plugin.DataManager.GetExcelSheet<Item>();
            var itemName = itemSheet.GetRow(itemId).Name.ExtractText();
            ImGui.SetClipboardText(itemName);
        }

        private unsafe void ShowGimmickHint(string text, RaptureAtkModule.TextGimmickHintStyle style = RaptureAtkModule.TextGimmickHintStyle.Info, int duration = 5)
        {
            var raptureAtkModule = RaptureAtkModule.Instance();
            if (raptureAtkModule == null) return;
            raptureAtkModule->ShowTextGimmickHint(text, style, duration);
        }

        private void DrawBlindBoxItem(string name, bool unique, uint itemId)
        {
            if (unique)
            {
                ImGui.Text("*");
                ImGui.SameLine();
            }
            ImGui.Text(name);

            if (ImGui.IsItemClicked())
            {
                LinkItemToChat(itemId);
                CopyItemNameToClipboard(itemId);
                ShowGimmickHint($"{name} 已复制到剪切板", RaptureAtkModule.TextGimmickHintStyle.Info, 4);
            }
        }
    }
}
