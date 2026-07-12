using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.Exd;
using Lumina.Excel.Sheets;

namespace BlindBoxPlugin
{
    public static unsafe class GameFunctions
    {
        /// <summary>通过 EXD 模块检查物品是否已解锁（已获得）。</summary>
        /// <param name="item">要检查的物品。</param>
        /// <returns>已解锁返回 true，否则 false。</returns>
        public static bool IsUnlocked(Item item)
        {
            if (item.RowId == 0)
                return false;

            // Generic EXD-based check (works across Lumina versions):
            var row = ExdModule.GetItemRowById(item.RowId);
            if (row == null)
                return false;
            return UIState.Instance()->IsItemActionUnlocked(row) == 1;
        }
    }
}
