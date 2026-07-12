using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.Sheets;

namespace BlindBoxPlugin.Windows;

public partial class MainWindow
{
    /// <summary>将物品链接发送到聊天框。</summary>
    private void LinkItemToChat(Item item)
    {
        var rarity = item.Rarity;
        var itemName = item.Name.ExtractText();

        var payloadList = new List<Payload>
        {
            new UIForegroundPayload((ushort)(0x223 + rarity * 2)),
            new UIGlowPayload((ushort)(0x224 + rarity * 2)),
            new ItemPayload(item.RowId),
            new UIForegroundPayload(500),
            new UIGlowPayload(501),
            new TextPayload($"{(char)SeIconChar.LinkMarker}"),
            new UIForegroundPayload(0),
            new UIGlowPayload(0),
            new TextPayload(itemName),
            new RawPayload([0x02, 0x27, 0x07, 0xCF, 0x01, 0x01, 0x01, 0xFF, 0x01, 0x03]),
            new RawPayload([0x02, 0x13, 0x02, 0xEC, 0x03]),
        };
        Plugin.ChatGUI.Print(new XivChatEntry { Message = new SeString(payloadList) });
    }

    /// <summary>将物品名称复制到剪切板。</summary>
    private void CopyItemNameToClipboard(Item item)
    {
        ImGui.SetClipboardText(item.Name.ExtractText());
    }

    /// <summary>在游戏界面上显示一段提示文本。</summary>
    private unsafe void ShowGimmickHint(
        string text,
        RaptureAtkModule.TextGimmickHintStyle style = RaptureAtkModule.TextGimmickHintStyle.Info,
        int duration = 5
    )
    {
        var raptureAtkModule = RaptureAtkModule.Instance();
        if (raptureAtkModule == null)
            return;
        raptureAtkModule->ShowTextGimmickHint(text, style, duration);
    }
}
