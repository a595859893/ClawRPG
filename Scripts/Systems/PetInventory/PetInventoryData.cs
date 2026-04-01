using Godot;
using System;
using System.Collections.Generic;

public class PetInventoryItem {
    public string ItemId { get; set; }
    public string ItemName { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
    public string Rarity { get; set; }
    public string Category { get; set; }
    public int Value { get; set; }
    public Dictionary<string, float> Stats { get; set; }
    public string SpecialEffect { get; set; }
    public DateTime AcquiredAt { get; set; }
}

public partial class PetInventoryData : BaseSystem {
    public Dictionary<string, List<PetInventoryItem>> PetInventories { get; set; }
    public int TotalSlots { get; set; }
    public Dictionary<string, int> GoldByPet { get; set; }
    public List<string> UnlockedCategories { get; set; }
    public Dictionary<string, int> ItemUsageCount { get; set; }
    public int TotalItemsCollected { get; set; }
    public int TotalItemsUsed { get; set; }
    public int TotalGoldSpent { get; set; }
    
    public override void _Ready() {
        base._Ready();
        Initialize();
    }
    
    public void Initialize() {
        if (PetInventories == null) {
            PetInventories = new Dictionary<string, List<PetInventoryItem>>();
        }
        if (GoldByPet == null) {
            GoldByPet = new Dictionary<string, int>();
        }
        if (UnlockedCategories == null) {
            UnlockedCategories = new List<string> { "All" };
        }
        if (ItemUsageCount == null) {
            ItemUsageCount = new Dictionary<string, int>();
        }
        if (TotalSlots == 0) {
            TotalSlots = 50;
        }
    }

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, Variant>();

            // 保存宠物背包数据
            var inventoriesData = new Dictionary<string, List<Dictionary<string, Variant>>>();
            if (PetInventories != null)
            {
                foreach (var kvp in PetInventories)
                {
                    var itemsList = new List<Dictionary<string, Variant>>();
                    foreach (var item in kvp.Value)
                    {
                        itemsList.Add(SerializePetInventoryItem(item));
                    }
                    inventoriesData[kvp.Key] = itemsList;
                }
            }
            data["petInventories"] = inventoriesData;

            data["totalSlots"] = TotalSlots;

            // 保存金币数据
            var goldData = new Dictionary<string, int>();
            if (GoldByPet != null)
            {
                foreach (var kvp in GoldByPet)
                {
                    goldData[kvp.Key] = kvp.Value;
                }
            }
            data["goldByPet"] = goldData;

            // 保存已解锁分类
            if (UnlockedCategories != null)
            {
                data["unlockedCategories"] = new List<string>(UnlockedCategories);
            }

            // 保存物品使用统计
            var usageData = new Dictionary<string, int>();
            if (ItemUsageCount != null)
            {
                foreach (var kvp in ItemUsageCount)
                {
                    usageData[kvp.Key] = kvp.Value;
                }
            }
            data["itemUsageCount"] = usageData;

            data["totalItemsCollected"] = TotalItemsCollected;
            data["totalItemsUsed"] = TotalItemsUsed;
            data["totalGoldSpent"] = TotalGoldSpent;

            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            // 加载宠物背包数据
            if (data.TryGetValue("petInventories", out var inventoriesData))
            {
                PetInventories = new Dictionary<string, List<PetInventoryItem>>();
                var invDict = (Dictionary<string, Variant>)inventoriesData;
                foreach (var kvp in invDict)
                {
                    var itemsList = new List<PetInventoryItem>();
                    var itemsVarList = (List<Variant>)kvp.Value;
                    foreach (var itemVar in itemsVarList)
                    {
                        itemsList.Add(DeserializePetInventoryItem((Dictionary<string, Variant>)itemVar));
                    }
                    PetInventories[kvp.Key] = itemsList;
                }
            }

            if (data.TryGetValue("totalSlots", out var totalSlots))
                TotalSlots = (int)totalSlots;

            if (data.TryGetValue("goldByPet", out var goldData))
            {
                GoldByPet = new Dictionary<string, int>();
                var goldDict = (Dictionary<string, Variant>)goldData;
                foreach (var kvp in goldDict)
                {
                    GoldByPet[kvp.Key] = (int)kvp.Value;
                }
            }

            if (data.TryGetValue("unlockedCategories", out var categoriesData))
                UnlockedCategories = new List<string>((List<string>)categoriesData);

            if (data.TryGetValue("itemUsageCount", out var usageData))
            {
                ItemUsageCount = new Dictionary<string, int>();
                var usageDict = (Dictionary<string, Variant>)usageData;
                foreach (var kvp in usageDict)
                {
                    ItemUsageCount[kvp.Key] = (int)kvp.Value;
                }
            }

            if (data.TryGetValue("totalItemsCollected", out var totalCollected))
                TotalItemsCollected = (int)totalCollected;
            if (data.TryGetValue("totalItemsUsed", out var totalUsed))
                TotalItemsUsed = (int)totalUsed;
            if (data.TryGetValue("totalGoldSpent", out var totalGoldSpent))
                TotalGoldSpent = (int)totalGoldSpent;
        }

        private Dictionary<string, Variant> SerializePetInventoryItem(PetInventoryItem item)
        {
            var itemData = new Dictionary<string, Variant>();
            itemData["itemId"] = item.ItemId ?? "";
            itemData["itemName"] = item.ItemName ?? "";
            itemData["description"] = item.Description ?? "";
            itemData["quantity"] = item.Quantity;
            itemData["rarity"] = item.Rarity ?? "";
            itemData["category"] = item.Category ?? "";
            itemData["value"] = item.Value;
            itemData["specialEffect"] = item.SpecialEffect ?? "";

            // 序列化 Stats 字典
            var statsData = new Dictionary<string, float>();
            if (item.Stats != null)
            {
                foreach (var statKvp in item.Stats)
                {
                    statsData[statKvp.Key] = statKvp.Value;
                }
            }
            itemData["stats"] = statsData;

            // 序列化 DateTime 为 ISO 8601 字符串
            itemData["acquiredAt"] = item.AcquiredAt.ToString("o");
            return itemData;
        }

        private PetInventoryItem DeserializePetInventoryItem(Dictionary<string, Variant> itemData)
        {
            var item = new PetInventoryItem();

            if (itemData.TryGetValue("itemId", out var itemId))
                item.ItemId = (string)itemId;
            if (itemData.TryGetValue("itemName", out var itemName))
                item.ItemName = (string)itemName;
            if (itemData.TryGetValue("description", out var desc))
                item.Description = (string)desc;
            if (itemData.TryGetValue("quantity", out var qty))
                item.Quantity = (int)qty;
            if (itemData.TryGetValue("rarity", out var rarity))
                item.Rarity = (string)rarity;
            if (itemData.TryGetValue("category", out var cat))
                item.Category = (string)cat;
            if (itemData.TryGetValue("value", out var val))
                item.Value = (int)val;
            if (itemData.TryGetValue("specialEffect", out var effect))
                item.SpecialEffect = (string)effect;

            if (itemData.TryGetValue("stats", out var statsData))
            {
                item.Stats = new Dictionary<string, float>();
                var statsDict = (Dictionary<string, Variant>)statsData;
                foreach (var statKvp in statsDict)
                {
                    item.Stats[statKvp.Key] = (float)statKvp.Value;
                }
            }

            if (itemData.TryGetValue("acquiredAt", out var acquiredAtStr))
            {
                if (DateTime.TryParse((string)acquiredAtStr, out var parsedDate))
                    item.AcquiredAt = parsedDate;
            }

            return item;
        }
}
