using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.Sheets;

namespace BlindBoxPlugin;

/// <summary>单个盲盒的信息。</summary>
public class BlindBoxInfo
{
    /// <summary>该盲盒包含的所有物品。</summary>
    public readonly List<Item> Items;

    /// <summary>盲盒本身对应的道具行。</summary>
    public readonly Item Item;

    /// <summary>标记为"仅可从当前途径获取"的物品 ID 集合。</summary>
    public HashSet<uint> UniqueItems { get; set; } = new();

    public BlindBoxInfo(uint id, List<uint> itemIds)
    {
        var sheet = Plugin.DataManager.GetExcelSheet<Item>();
        Item = sheet.HasRow(id) ? sheet.GetRow(id) : default;
        Items = itemIds.Select(itemId => sheet.HasRow(itemId) ? sheet.GetRow(itemId) : default).ToList();
    }

    public BlindBoxInfo(uint id, List<uint> itemIds, List<uint> uniqueItemIds) : this(id, itemIds)
    {
        UniqueItems = new HashSet<uint>(uniqueItemIds);
    }
}

/// <summary>所有盲盒数据的注册表。</summary>
public static class BlindBoxData
{
    // ===== 特殊配给货箱 =====

    /// <summary>特殊配给货箱（重生/苍穹）</summary>
    private static readonly BlindBoxInfo SpecialSupplyCrateHW = new(36635,
    [
        6003, 6004, 6005, 6168, 6173, 6174, 6175, 6177, 6179, 6184,
        6185, 6187, 6188, 6189, 6190, 6191, 6192, 6195, 6199, 6200,
        6203, 6204, 6205, 6208, 6213, 6214, 7559, 7564, 7566, 7567,
        7568, 8193, 8194, 8195, 8196, 8199, 8200, 8201, 8202, 8205,
        9347, 9348, 9349, 9350, 10071, 12049, 12051, 12052, 12054, 12055,
        12056, 12057, 12058, 12061, 12062, 12069, 13275, 13276, 13279, 13283,
        13284, 14083, 14093, 14094, 14095, 14098, 14100, 14103, 15436, 15437,
        15440, 15441, 15447, 16564, 16568, 16570, 16572, 16573, 17525, 17527,
    ]);

    /// <summary>特殊配给货箱（红莲）</summary>
    private static readonly BlindBoxInfo SpecialSupplyCrateSB = new(36636,
    [
        20524, 20525, 20528, 20529, 20530, 20531, 20533, 20536, 20537, 20538,
        20539, 20541, 20542, 20544, 20545, 20546, 20547, 21052, 21055, 21057,
        21058, 21059, 21060, 21063, 21064, 21065, 21193, 21907, 21911, 21915,
        21916, 21917, 21918, 21919, 21920, 21921, 21922, 23023, 23027, 23028,
        23030, 23032, 23036, 23989, 23998, 24000, 24001, 24002, 24219, 24630,
        24634, 24635, 24639, 24640, 24902, 24903,
    ]);

    /// <summary>庆典礼物箱</summary>
    private static readonly BlindBoxInfo CelebrationGiftBox = new(33441,
    [
        8202, 8570, 9355, 12051, 12052, 12053, 12082, 12083, 14080, 15427,
        16557, 17522, 17861, 17862, 17863, 24291, 24292, 28612, 28615, 28622,
        28628, 29403, 30111, 30112, 30113, 30259, 30260, 30261, 30269, 30270,
        31324, 31325, 31406, 32826, 33039, 33127, 33675, 33847,
    ]);

    /// <summary>无人岛特殊配给货箱</summary>
    private static readonly BlindBoxInfo IslandSupplyCrate = new(41667,
    [
        13279, 13283, 15447, 16572, 20524, 20525, 20528, 20529, 20530, 20531,
        20542, 41649, 41650,
    ],
    [41649]);

    // ===== 上锁的宝箱 =====

    /// <summary>常风地带上锁的宝箱</summary>
    private static readonly BlindBoxInfo ChestWind = new(22508,
        [21918, 21917, 21919, 21907, 21191, 22479, 22480],
        [21191, 21907, 22479, 22480]);

    /// <summary>恒冰地带上锁的宝箱</summary>
    private static readonly BlindBoxInfo ChestIce = new(23142, [23028]);

    /// <summary>涌火地带上锁的宝箱</summary>
    private static readonly BlindBoxInfo ChestFlame = new(24141, [24001]);

    /// <summary>丰水地带上锁的宝箱</summary>
    private static readonly BlindBoxInfo ChestWater = new(24848, [25067, 24640], [25067]);

    /// <summary>南方战线上锁的宝箱</summary>
    private static readonly BlindBoxInfo ChestSouth = new(31357,
    [
        21042, 21057, 21058, 21059, 21063, 21064, 21065, 21193, 21911, 21916,
        21920, 21921, 21924, 23032, 23037, 23989, 23998, 24143, 24634, 24639,
        24799, 24902, 30861, 30876, 31326, 31664,
    ],
    [31326, 31664]);

    /// <summary>高原上锁的宝箱</summary>
    private static readonly BlindBoxInfo ChestHighland = new(33797,
    [
        12082, 14080, 15427, 16557, 17522, 20558, 21058, 21063, 21064, 21065,
        21193, 21911, 21916, 21920, 21921, 21924, 23032, 23037, 23989, 23998,
        24634, 24639, 24902, 26802, 32828, 33672, 33706,
    ],
    [33672]);

    // ===== 九宫幻卡包 =====

    /// <summary>九宫幻卡白金包</summary>
    private static readonly BlindBoxInfo TripleTriadPlatinum = new(10077,
        [9822, 9826, 9827, 9828, 9830, 9834, 9840, 9842, 9848, 9851, 14208],
        [9828, 9840, 9842, 9848, 9851]);

    /// <summary>九宫幻卡铜包</summary>
    private static readonly BlindBoxInfo TripleTriadCopper = new(10128,
    [
        9775, 9776, 9779, 9782, 9783, 9795, 9796, 9797, 9798, 9809,
        15621, 16759, 16760, 16762, 16765,
    ],
    [16759, 16760, 16762]);

    /// <summary>九宫幻卡银包</summary>
    private static readonly BlindBoxInfo TripleTriadSilver = new(10129,
        [9785, 9786, 9787, 9788, 9790, 9792, 9811, 9812, 9813, 9814, 9821, 9827, 9828, 14199],
        [9788, 9790, 9827, 9828]);

    /// <summary>九宫幻卡金包</summary>
    private static readonly BlindBoxInfo TripleTriadGold = new(10130,
    [
        9799, 9800, 9801, 9805, 9822, 9824, 9825, 9826, 9829, 9836,
        9837, 9838, 9839, 9847, 14192,
    ],
    [9839, 9847, 14192]);

    /// <summary>九宫幻卡灵银包</summary>
    private static readonly BlindBoxInfo TripleTriadSilverSp = new(13380,
        [9810, 9823, 9841, 9843, 9844, 13367, 13368, 13372, 14193]);

    /// <summary>帝国九宫幻卡包</summary>
    private static readonly BlindBoxInfo TripleTriadImperial = new(17702,
        [13378, 16774, 16775, 17681, 17682, 17686],
        [16774, 16775, 17681, 17682, 17686]);

    /// <summary>九宫幻卡梦想包</summary>
    private static readonly BlindBoxInfo TripleTriadDream = new(28652,
        [26765, 26766, 26767, 26768, 26772, 28653, 28655, 28657, 28658, 28660, 28661],
        [28653, 28655, 28657, 28658, 28660, 28661]);

    // ===== 能源包 =====

    /// <summary>俄匊斯能源包</summary>
    private static readonly BlindBoxInfo EnergyPackOizys = new(50414,
    [
        6183, 6185, 6190, 6191, 6199, 6200, 6204, 6209, 6214, 7559,
        8200, 10071, 12051, 12055, 12058, 12062, 13275, 13276, 13279, 15441,
        20524, 20528, 20539, 21916, 28612, 28615, 28622, 28628, 28917, 30110,
        30112, 30113, 30269, 30270, 30862, 31324, 31325, 31401, 33039, 33696,
        35984, 50334, 50435,
    ],
    [50435]);

    /// <summary>奥克塞西亚能源包</summary>
    private static readonly BlindBoxInfo EnergyPackAuxesia = new(50415,
    [
        6183, 6191, 6200, 6209, 6214, 8200, 12051, 12058, 13284, 14093,
        14094, 14095, 15436, 15437, 15439, 15441, 20528, 20539, 20545, 20546,
        20547, 21057, 21065, 21915, 21920, 23030, 23989, 24634, 28125, 28628,
        30111, 31324, 31406, 32826, 32829, 32841, 33039, 33674, 33696, 35984,
        39482, 40380, 52257, 52288,
    ],
    [52257, 52288]);

    // ===== 宇宙好运道 =====

    /// <summary>月球信用点</summary>
    private static readonly BlindBoxInfo LunarCredit = new(45691,
        [44505,44509,47966,48154,48160,48210,48220,48221],
        [44509,47966,48154,48160,48210,48220,48221]
    );

    /// <summary>法恩娜信用点</summary>
    private static readonly BlindBoxInfo PhaennaCredit = new(48146,
        [46155,46782,46795,46840,47973],
        [46155,46782,46795,46840]
    );

    /// <summary>俄匊斯信用点</summary>
    private static readonly BlindBoxInfo OizysCredit = new(48147,
        [50323,50441,50450,50455,50458,50803],
        [50323,50450,50455,50458,50803]
    );

    /// <summary>奥克塞西亚信用点</summary>
    private static readonly BlindBoxInfo AuxesiaCredit = new(48148,
        [52267,52275,52359,52449,52648],
        [52275,52359,52449,52648]
    );

    /// <summary>盲盒 ID → 盲盒信息 的查找表。</summary>
    public static readonly Dictionary<uint, BlindBoxInfo> BlindBoxInfoMap = new()
    {
        [36635] = SpecialSupplyCrateHW,
        [36636] = SpecialSupplyCrateSB,
        [33441] = CelebrationGiftBox,
        [41667] = IslandSupplyCrate,
        [22508] = ChestWind,
        [23142] = ChestIce,
        [24141] = ChestFlame,
        [24848] = ChestWater,
        [31357] = ChestSouth,
        [33797] = ChestHighland,
        [10077] = TripleTriadPlatinum,
        [10128] = TripleTriadCopper,
        [10129] = TripleTriadSilver,
        [10130] = TripleTriadGold,
        [13380] = TripleTriadSilverSp,
        [17702] = TripleTriadImperial,
        [28652] = TripleTriadDream,
        [50414] = EnergyPackOizys,
        [50415] = EnergyPackAuxesia,
        [45691] = LunarCredit,
        [48146] = PhaennaCredit,
        [48147] = OizysCredit,
        [48148] = AuxesiaCredit,
    };
}
