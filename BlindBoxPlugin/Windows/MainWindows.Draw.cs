using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.Sheets;

namespace BlindBoxPlugin.Windows;

public partial class MainWindow
{
    /// <summary>绘制物品图标。直接使用 Dalamud 的 IDalamudTextureWrap.Handle，无需反射。</summary>
    private void DrawItemIcon(Item item)
    {
        var texture = Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(item.Icon)).GetWrapOrEmpty();
        var iconSize = new Vector2(ImGui.GetTextLineHeight(), ImGui.GetTextLineHeight());
        ImGui.Image(texture.Handle, iconSize, Vector2.Zero, Vector2.One, Vector4.One, Vector4.Zero);
        ImGui.SameLine();
    }

    /// <summary>绘制单个盲盒物品行，包含图标、名称和颜色状态。</summary>
    private void DrawBlindBoxItem(Item item, bool unique, HashSet<uint> acquiredRowIds)
    {
        var isAcquired = acquiredRowIds.Contains(item.RowId);
        Vector4 color = isAcquired
            ? ColorAcquired
            : (item.IsUntradable ? ColorUntradeable : ColorMissing);

        DrawItemIcon(item);

        if (unique)
        {
            ImGui.TextColored(ColorUnique, "*");
            ImGui.SameLine();
        }

        var itemName = item.Name.ExtractText();
        ImGui.TextColored(color, itemName);

        if (ImGui.IsItemClicked())
        {
            LinkItemToChat(item);
            CopyItemNameToClipboard(item);
            ShowGimmickHint(
                $"{itemName} 已复制到剪切板",
                RaptureAtkModule.TextGimmickHintStyle.Info,
                4
            );
        }
    }
}
