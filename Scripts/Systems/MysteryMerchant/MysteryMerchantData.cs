using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 神秘商店类型
    /// </summary>
    public enum MysteryMerchantType
    {
        TravelingMerchant,  // 旅行商人
        BlackMarketDealer,  // 黑市商人
        AncientCollector,   // 古代收藏家
        DragonHoardKeeper,  // 龙穴守护者
        CursedItemDealer,   // 诅咒物品商人
        LuckyCharmSeller,   // 幸运护符卖家
        RareMaterialVendor, // 稀有材料 vendor
        SecretArtifactDealer // 秘密神器商人
    }

    /// <summary>
    /// 商品稀有度
    /// </summary>
    public enum MerchantItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythical
    }

    /// <summary>
    /// 神秘商店商品数据
    /// </summary>
    public class MysteryMerchantItem
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public MerchantItemRarity Rarity { get; set; }
        public int Price { get; set; }
        public int OriginalPrice { get; set; }  // 原价
        public float Discount { get; set; }     // 折扣率
        public bool IsLimited { get; set; }     // 限时抢购
        public int Stock { get; set; }          // 库存
        public int MaxStock { get; set; }       // 最大库存
        public bool IsSecret { get; set; }      // 隐藏商品
        public string Description { get; set; }
        public Dictionary<string, int> BonusAttributes { get; set; }  // 额外属性加成
        
        public MysteryMerchantItem()
        {
            BonusAttributes = new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// 神秘商店实例
    /// </summary>
    public class MysteryMerchant
    {
        public string MerchantId { get; set; }
        public MysteryMerchantType MerchantType { get; set; }
        public string MerchantName { get; set; }
        public string Description { get; set; }
        public List<MysteryMerchantItem> Items { get; set; }
        public int RefreshCost { get; set; }       // 刷新费用
        public float SpawnChance { get; set; }      // 出现概率
        public int MinimumPlayerLevel { get; set; } // 最低玩家等级
        public string SpawnRegion { get; set; }     // 出现区域
        public DateTime SpawnTime { get; set; }
        public DateTime ExpireTime { get; set; }
        public bool IsActive { get; set; }
        
        public MysteryMerchant()
        {
            Items = new List<MysteryMerchantItem>();
        }
    }

    /// <summary>
    /// 玩家神秘商店数据
    /// </summary>
    public class PlayerMysteryMerchantData
    {
        public int TotalVisits { get; set; }
        public int TotalPurchases { get; set; }
        public int TotalGoldSpent { get; set; }
        public int TotalItemsBought { get; set; }
        public Dictionary<string, int> PurchasesByType { get; set; }  // 按类型统计购买
        public Dictionary<string, int> PurchasesByRarity { get; set; } // 按稀有度统计购买
        public List<string> UnlockedMerchantTypes { get; set; }     // 已解锁的商店类型
        public Dictionary<string, int> VisitHistory { get; set; }   // 访问历史
        public int LuckyPurchases { get; set; }     // 幸运购买次数
        public int SecretItemsFound { get; set; }   // 发现隐藏物品次数
        
        public PlayerMysteryMerchantData()
        {
            PurchasesByType = new Dictionary<string, int>();
            PurchasesByRarity = new Dictionary<string, int>();
            UnlockedMerchantTypes = new List<string>();
            VisitHistory = new Dictionary<string, int>();
        }
    }
}
