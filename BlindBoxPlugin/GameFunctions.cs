using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.Exd;
using Lumina.Excel.Sheets;

namespace BlindBoxPlugin
{
    public static unsafe class GameFunctions
    {
        /// <summary>
        /// Use the EXD-based check which is stable across Lumina versions.
        /// Falls back to a generic EXD inspection rather than relying on Lumina's ItemAction/Data API.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
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
