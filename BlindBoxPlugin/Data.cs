using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        var sheet = Plugin.ItemSheet;
        Item = sheet.GetRowOrDefault(id) ?? default;
        Items = itemIds.Select(itemID => sheet.GetRowOrDefault(itemID) ?? default).ToList();
    }

    public BlindBoxInfo(uint id, List<uint> itemIds, List<uint> uniqueItemIds) : this(id, itemIds)
    {
        UniqueItems = new HashSet<uint>(uniqueItemIds);
    }
}

/// <summary>JSON 反序列化用的分组 DTO。</summary>
internal class BlindBoxGroupJSON
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("boxes")]
    public List<BlindBoxEntryJSON> Boxes { get; set; } = [];
}

/// <summary>JSON 反序列化用的盲盒条目 DTO。</summary>
internal class BlindBoxEntryJSON
{
    [JsonPropertyName("id")]
    public uint ID { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<uint> Items { get; set; } = [];

    [JsonPropertyName("uniqueItems")]
    public List<uint> UniqueItems { get; set; } = [];
}

/// <summary>所有盲盒数据的注册表。</summary>
public static class BlindBoxData
{
    /// <summary>盲盒系列分组。</summary>
    public static readonly List<(string Name, HashSet<uint> ItemIds)> SeriesGroups;

    /// <summary>盲盒 ID → 盲盒信息 的查找表。</summary>
    public static readonly Dictionary<uint, BlindBoxInfo> BlindBoxInfoMap;

    static BlindBoxData()
    {
        var assembly = typeof(BlindBoxData).Assembly;
        const string resourceName = "BlindBoxPlugin.blind_boxes.json";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource not found: {resourceName}");

        var groups = JsonSerializer.Deserialize<List<BlindBoxGroupJSON>>(stream) ?? [];

        var infoMap = new Dictionary<uint, BlindBoxInfo>();
        var allIds = new HashSet<uint>();
        var seriesGroups = new List<(string Name, HashSet<uint> ItemIds)> { ("全部", allIds) };

        foreach (var group in groups)
        {
            var groupIds = new HashSet<uint>();
            foreach (var box in group.Boxes)
            {
                var info = box.UniqueItems.Count > 0
                    ? new BlindBoxInfo(box.ID, box.Items, box.UniqueItems)
                    : new BlindBoxInfo(box.ID, box.Items);
                infoMap[box.ID] = info;
                groupIds.Add(box.ID);
            }

            seriesGroups.Add((group.Name, groupIds));
            allIds.UnionWith(groupIds);
        }

        BlindBoxInfoMap = infoMap;
        SeriesGroups = seriesGroups;
    }
}
