using Godot;
using System;
using System.Collections.Generic;

namespace GameSystems
{
    /// <summary>
    /// 商店物品数据结构
    /// </summary>
    public class ShopItem
    {
        public string ItemId { get; set; }
        public int Price { get; set; }
        public int Stock { get; set; } // -1 = 无限
        public int DailyStock { get; set; } // 每日刷新数量
        public float Discount { get; set; } = 1.0f; // 折扣
        public bool IsUnlocked { get; set; } = false; 
        public int UnlockRequirement { get; set; } = 0; // 声望等级需求
    }

    /// <summary>
    /// 商店数据结构
    /// </summary>
    public class ShopData
    {
        public string ShopId { get; set; }
        public string ShopName { get; set; }
        public string Description { get; set; }
        public ShopType ShopType { get; set; }
        public List<ShopItem> Items { get; set; } = new List<ShopItem>();
        public int RefreshCost { get; set; } = 100; // 刷新费用
        public bool CanDiscount { get; set; } = true;
        public string RequiredReputation { get; set; } = "";
        public int RequiredReputationLevel { get; set; } = 0;
    }

    /// <summary>
    /// 商店类型
    /// </summary>
    public enum ShopType
    {
        Weapon,      // 武器店
        Armor,       // 防具店
        Potion,      // 药水店
        Magic,       // 魔法道具店
        General,     // 综合商店
        BlackMarket, // 黑市
        Guild,       // 公会商店
        Specialty    // 特产店
    }

    /// <summary>
    /// 商店系统 - 管理所有商店
    /// </summary>
    public partial class ShopSystem : BaseSystem
    {
        public static ShopSystem Instance { get; private set; }

        // 商店数据库
        private Dictionary<string, ShopData> _shops = new Dictionary<string, ShopData>();
        
        // 玩家购买记录
        private Dictionary<string, int> _purchaseHistory = new Dictionary<string, int>();
        
        // 今日购买记录
        private Dictionary<string, int> _dailyPurchases = new Dictionary<string, int>();
        
        // 商店刷新时间
        private Dictionary<string, DateTime> _lastRefresh = new Dictionary<string, DateTime>();

        // 信号
        [Signal]
        public static readonly SignalPurchaseCompleted PurchaseCompleted;
        [Signal]
        public static readonly SignalShopRefreshed ShopRefreshed;
        [Signal]
        public static readonly SignalItemSold ItemSold;

        public override void _Ready()
        {
            Instance = this;
            InitializeShops();
        }

        private void InitializeShops()
        {
            // 武器商店
            var weaponShop = new ShopData
            {
                ShopId = "weapon_shop",
                ShopName = "武器商店",
                Description = "提供各种武器和攻击道具",
                ShopType = ShopType.Weapon,
                Items = CreateWeaponItems(),
                RefreshCost = 100
            };
            _shops["weapon_shop"] = weaponShop;

            // 防具商店
            var armorShop = new ShopData
            {
                ShopId = "armor_shop",
                ShopName = "防具商店",
                Description = "提供护甲、盾牌和防御装备",
                ShopType = ShopType.Armor,
                Items = CreateArmorItems(),
                RefreshCost = 100
            };
            _shops["armor_shop"] = armorShop;

            // 药水商店
            var potionShop = new ShopData
            {
                ShopId = "potion_shop",
                ShopName = "药水商店",
                Description = "恢复药水和增益药水",
                ShopType = ShopType.Potion,
                Items = CreatePotionItems(),
                RefreshCost = 50
            };
            _shops["potion_shop"] = potionShop;

            // 魔法商店
            var magicShop = new ShopData
            {
                ShopId = "magic_shop",
                ShopName = "魔法商店",
                Description = "魔法卷轴和施法材料",
                ShopType = ShopType.Magic,
                Items = CreateMagicItems(),
                RefreshCost = 150,
                RequiredReputation = "mages_guild",
                RequiredReputationLevel = 2
            };
            _shops["magic_shop"] = magicShop;

            // 综合商店
            var generalShop = new ShopData
            {
                ShopId = "general_shop",
                ShopName = "综合商店",
                Description = "杂货和常用物品",
                ShopType = ShopType.General,
                Items = CreateGeneralItems(),
                RefreshCost = 50
            };
            _shops["general_shop"] = generalShop;

            // 黑市
            var blackMarket = new ShopData
            {
                ShopId = "black_market",
                ShopName = "黑市",
                Description = "珍稀物品和禁忌之物",
                ShopType = ShopType.BlackMarket,
                Items = CreateBlackMarketItems(),
                RefreshCost = 500,
                CanDiscount = false,
                RequiredReputation = "thieves_guild",
                RequiredReputationLevel = 4
            };
            _shops["black_market"] = blackMarket;

            // 公会商店
            var guildShop = new ShopData
            {
                ShopId = "guild_shop",
                ShopName = "公会商店",
                Description = "公会专属物品",
                ShopType = ShopType.Guild,
                Items = CreateGuildItems(),
                RefreshCost = 200,
                RequiredReputation = "warriors_guild",
                RequiredReputationLevel = 3
            };
            _shops["guild_shop"] = guildShop;

            GD.Print($"[ShopSystem] 初始化了 {_shops.Count} 个商店");
        }

        private List<ShopItem> CreateWeaponItems()
        {
            return new List<ShopItem>
            {
                new ShopItem { ItemId = "sword_basic", Price = 100, Stock = 5 },
                new ShopItem { ItemId = "sword_iron", Price = 300, Stock = 3 },
                new ShopItem { ItemId = "sword_steel", Price = 800, Stock = 2 },
                new ShopItem { ItemId = "axe_basic", Price = 120, Stock = 5 },
                new ShopItem { ItemId = "axe_heavy", Price = 500, Stock = 2 },
                new ShopItem { ItemId = "bow_hunter", Price = 200, Stock = 4 },
                new ShopItem { ItemId = "bow_long", Price = 600, Stock = 2 },
                new ShopItem { ItemId = "dagger_shadow", Price = 400, Stock = 3 },
                new ShopItem { ItemId = "staff_arcane", Price = 350, Stock = 3 },
                new ShopItem { ItemId = "wand_fire", Price = 450, Stock = 2 }
            };
        }

        private List<ShopItem> CreateArmorItems()
        {
            return new List<ShopItem>
            {
                new ShopItem { ItemId = "helmet_leather", Price = 80, Stock = 5 },
                new ShopItem { ItemId = "helmet_iron", Price = 200, Stock = 3 },
                new ShopItem { ItemId = "armor_leather", Price = 100, Stock = 5 },
                new ShopItem { ItemId = "armor_chain", Price = 350, Stock = 3 },
                new ShopItem { ItemId = "armor_plate", Price = 700, Stock = 2 },
                new ShopItem { ItemId = "shield_wooden", Price = 50, Stock = 10 },
                new ShopItem { ItemId = "shield_iron", Price = 180, Stock = 5 },
                new ShopItem { ItemId = "shield_tower", Price = 400, Stock = 2 },
                new ShopItem { ItemId = "boots_leather", Price = 60, Stock = 8 },
                new ShopItem { ItemId = "boots_steel", Price = 250, Stock = 3 }
            };
        }

        private List<ShopItem> CreatePotionItems()
        {
            return new List<ShopItem>
            {
                new ShopItem { ItemId = "potion_health_small", Price = 20, Stock = 20, DailyStock = 10 },
                new ShopItem { ItemId = "potion_health_medium", Price = 50, Stock = 15, DailyStock = 5 },
                new ShopItem { ItemId = "potion_health_large", Price = 100, Stock = 8, DailyStock = 3 },
                new ShopItem { ItemId = "potion_mana_small", Price = 25, Stock = 20, DailyStock = 10 },
                new ShopItem { ItemId = "potion_mana_medium", Price = 60, Stock = 15, DailyStock = 5 },
                new ShopItem { ItemId = "potion_mana_large", Price = 120, Stock = 8, DailyStock = 3 },
                new ShopItem { ItemId = "potion_stamina", Price = 30, Stock = 15, DailyStock = 8 },
                new ShopItem { ItemId = "potion_strength", Price = 150, Stock = 5, DailyStock = 2 },
                new ShopItem { ItemId = "potion_speed", Price = 100, Stock = 5, DailyStock = 2 },
                new ShopItem { ItemId = "antidote", Price = 40, Stock = 10 }
            };
        }

        private List<ShopItem> CreateMagicItems()
        {
            return new List<ShopItem>
            {
                new ShopItem { ItemId = "scroll_fireball", Price = 80, Stock = 10 },
                new ShopItem { ItemId = "scroll_ice_storm", Price = 120, Stock = 8 },
                new ShopItem { ItemId = "scroll_lightning", Price = 100, Stock = 8 },
                new ShopItem { ItemId = "scroll_heal", Price = 60, Stock = 12 },
                new ShopItem { ItemId = "scroll_teleport", Price = 150, Stock = 5 },
                new ShopItem { ItemId = "scroll_invisibility", Price = 200, Stock = 5 },
                new ShopItem { ItemId = "mana_crystal", Price = 50, Stock = 15 },
                new ShopItem { ItemId = "magic_orb", Price = 300, Stock = 3 }
            };
        }

        private List<ShopItem> CreateGeneralItems()
        {
            return new List<ShopItem>
            {
                new ShopItem { ItemId = "arrow_basic", Price = 5, Stock = 100, DailyStock = 50 },
                new ShopItem { ItemId = "arrow_silver", Price = 15, Stock = 30, DailyStock = 15 },
                new ShopItem { ItemId = "bomb_basic", Price = 30, Stock = 15 },
                new ShopItem { ItemId = "bomb_fire", Price = 80, Stock = 8 },
                new ShopItem { ItemId = "trap_basic", Price = 40, Stock = 10 },
                new ShopItem { ItemId = "rope", Price = 10, Stock = 20 },
                new ShopItem { ItemId = "torch", Price = 5, Stock = 50 },
                new ShopItem { ItemId = "key_chest", Price = 25, Stock = 10 },
                new ShopItem { ItemId = "food_ration", Price = 10, Stock = 30 },
                new ShopItem { ItemId = "wine_quality", Price = 30, Stock = 15 }
            };
        }

        private List<ShopItem> CreateBlackMarketItems()
        {
            return new List<ShopItem>
            {
                new ShopItem { ItemId = "artifact_demonic", Price = 5000, Stock = 1 },
                new ShopItem { ItemId = "weapon_legendary", Price = 3000, Stock = 1 },
                new ShopItem { ItemId = "armor_legendary", Price = 2500, Stock = 1 },
                new ShopItem { ItemId = "poison_deadly", Price = 500, Stock = 2 },
                new ShopItem { ItemId = "cursed_doll", Price = 800, Stock = 2 },
                new ShopItem { ItemId = "dragon_scale", Price = 2000, Stock = 1 }
            };
        }

        private List<ShopItem> CreateGuildItems()
        {
            return new List<ShopItem>
            {
                new ShopItem { ItemId = "guild_badge_warrior", Price = 500, Stock = 1 },
                new ShopItem { ItemId = "guild_sword_master", Price = 1500, Stock = 1 },
                new ShopItem { ItemId = "guild_armor_elite", Price = 1200, Stock = 1 },
                new ShopItem { ItemId = "guild_potion_special", Price = 200, Stock = 5 },
                new ShopItem { ItemId = "guild_scroll_rare", Price = 300, Stock = 3 }
            };
        }

        /// <summary>
        /// 获取商店数据
        /// </summary>
        public ShopData GetShop(string shopId)
        {
            if (_shops.TryGetValue(shopId, out var shop))
            {
                return shop;
            }
            return null;
        }

        /// <summary>
        /// 获取所有商店列表
        /// </summary>
        public List<ShopData> GetAllShops()
        {
            return new List<ShopData>(_shops.Values);
        }

        /// <summary>
        /// 获取玩家可访问的商店
        /// </summary>
        public List<ShopData> GetAccessibleShops()
        {
            var accessible = new List<ShopData>();
            
            foreach (var shop in _shops.Values)
            {
                if (CanAccessShop(shop))
                {
                    accessible.Add(shop);
                }
            }
            
            return accessible;
        }

        /// <summary>
        /// 检查是否可以访问商店
        /// </summary>
        public bool CanAccessShop(ShopData shop)
        {
            if (string.IsNullOrEmpty(shop.RequiredReputation))
                return true;

            var repSystem = ReputationSystem.Instance;
            if (repSystem == null)
                return false;

            int currentLevel = repSystem.GetReputationLevel(shop.RequiredReputation);
            return currentLevel >= shop.RequiredReputationLevel;
        }

        /// <summary>
        /// 购买物品
        /// </summary>
        public bool PurchaseItem(string shopId, string itemId, int quantity = 1)
        {
            var shop = GetShop(shopId);
            if (shop == null)
            {
                GD.Warning($"[ShopSystem] 商店不存在: {shopId}");
                return false;
            }

            var item = shop.Items.Find(i => i.ItemId == itemId);
            if (item == null)
            {
                GD.Warning($"[ShopSystem] 物品不存在: {itemId}");
                return false;
            }

            // 检查库存
            if (item.Stock >= 0 && item.Stock < quantity)
            {
                GD.Warning($"[ShopSystem] 库存不足: {itemId}");
                return false;
            }

            // 检查每日限购
            string dailyKey = $"{shopId}_{itemId}";
            int todayPurchased = _dailyPurchases.ContainsKey(dailyKey) ? _dailyPurchases[dailyKey] : 0;
            if (item.DailyStock > 0 && todayPurchased + quantity > item.DailyStock)
            {
                GD.Warning($"[ShopSystem] 今日购买次数已用尽: {itemId}");
                return false;
            }

            // 计算价格
            int totalPrice = (int)(item.Price * item.Discount * quantity);

            // 检查玩家金币
            var player = GetTree().GetFirstNodeInGroup("player") as CharacterBody2D;
            if (player == null)
            {
                GD.Warning("[ShopSystem] 找不到玩家");
                return false;
            }

            var inventory = InventoryManager.Instance;
            if (inventory == null || inventory.Gold < totalPrice)
            {
                GD.Warning($"[ShopSystem] 金币不足: 需要 {totalPrice}, 当前 {inventory?.Gold ?? 0}");
                return false;
            }

            // 执行购买
            inventory.Gold -= totalPrice;
            
            // 添加物品
            if (inventory.AddItem(itemId, quantity))
            {
                // 更新库存
                if (item.Stock >= 0)
                    item.Stock -= quantity;
                
                // 记录购买
                _dailyPurchases[dailyKey] = todayPurchased + quantity;
                
                string historyKey = $"{shopId}_{itemId}";
                _purchaseHistory[historyKey] = _purchaseHistory.GetValueOrDefault(historyKey, 0) + quantity;

                EmitSignal(SignalName.PurchaseCompleted, shopId, itemId, quantity, totalPrice);
                
                GD.Print($"[ShopSystem] 购买成功: {itemId} x{quantity}, 花费 {totalPrice} 金币");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 出售物品
        /// </summary>
        public bool SellItem(string itemId, int quantity = 1)
        {
            var inventory = InventoryManager.Instance;
            if (inventory == null)
                return false;

            // 获取物品数据
            var itemData = ItemDatabase.Instance?.GetItem(itemId);
            if (itemData == null)
                return false;

            // 检查玩家是否有足够物品
            if (!inventory.HasItem(itemId, quantity))
                return false;

            // 计算出售价格 (通常是购买价格的50%)
            int sellPrice = (int)(itemData.Value * 0.5f);

            // 移除物品
            if (inventory.RemoveItem(itemId, quantity))
            {
                // 添加金币
                inventory.Gold += sellPrice * quantity;

                EmitSignal(SignalName.ItemSold, itemId, quantity, sellPrice * quantity);
                
                GD.Print($"[ShopSystem] 出售成功: {itemId} x{quantity}, 获得 {sellPrice * quantity} 金币");
                return true;
            }

            return false;
        }

        /// <summary>
        /// 刷新商店
        /// </summary>
        public bool RefreshShop(string shopId)
        {
            var shop = GetShop(shopId);
            if (shop == null)
                return false;

            var inventory = InventoryManager.Instance;
            if (inventory == null || inventory.Gold < shop.RefreshCost)
            {
                GD.Warning($"[ShopSystem] 刷新费用不足: 需要 {shop.RefreshCost}");
                return false;
            }

            // 扣除刷新费用
            inventory.Gold -= shop.RefreshCost;

            // 刷新物品 (重新随机化库存)
            foreach (var item in shop.Items)
            {
                if (item.Stock >= 0)
                {
                    // 恢复到最大库存的随机比例
                    int maxStock = GetMaxStock(item.ItemId);
                    item.Stock = (int)(maxStock * GD.Randf() * 0.5f + maxStock * 0.3f);
                }
                
                // 随机折扣
                if (shop.CanDiscount)
                    item.Discount = 0.8f + (float)GD.Randf() * 0.2f;
            }

            _lastRefresh[shopId] = DateTime.Now;
            EmitSignal(SignalName.ShopRefreshed, shopId);
            
            GD.Print($"[ShopSystem] 商店已刷新: {shopId}");
            return true;
        }

        private int GetMaxStock(string itemId)
        {
            // 根据物品ID返回最大库存
            if (itemId.Contains("potion"))
                return 20;
            if (itemId.Contains("scroll"))
                return 15;
            if (itemId.Contains("legendary") || itemId.Contains("artifact"))
                return 1;
            return 10;
        }

        /// <summary>
        /// 获取购买历史
        /// </summary>
        public int GetPurchaseHistory(string shopId, string itemId)
        {
            string key = $"{shopId}_{itemId}";
            return _purchaseHistory.GetValueOrDefault(key, 0);
        }

        /// <summary>
        /// 每日重置
        /// </summary>
        public void DailyReset()
        {
            _dailyPurchases.Clear();
            
            // 恢复每日限购物品的库存
            foreach (var shop in _shops.Values)
            {
                foreach (var item in shop.Items)
                {
                    if (item.DailyStock > 0 && item.Stock >= 0)
                    {
                        int maxStock = GetMaxStock(item.ItemId);
                        item.Stock = Math.Min(item.Stock + item.DailyStock, maxStock);
                    }
                }
            }
            
            GD.Print("[ShopSystem] 每日重置完成");
        }

        /// <summary>
        /// 获取商店类型名称
        /// </summary>
        public static string GetShopTypeName(ShopType type)
        {
            return type switch
            {
                ShopType.Weapon => "武器店",
                ShopType.Armor => "防具店",
                ShopType.Potion => "药水店",
                ShopType.Magic => "魔法商店",
                ShopType.General => "杂货店",
                ShopType.BlackMarket => "黑市",
                ShopType.Guild => "公会商店",
                ShopType.Specialty => "特产店",
                _ => "商店"
            };
        }
    }
}
