using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 神秘商店系统管理器
    /// </summary>
    public partial class MysteryMerchantSystem : BaseSystem
    {
        public static MysteryMerchantSystem Instance { get; private set; }
        
        // 商店配置
        private const float MERCHANT_SPAWN_INTERVAL = 300f; // 5分钟检查一次
        private const float MERCHANT_DURATION = 600f; // 商店持续10分钟
        private const int MAX_ACTIVE_MERCHANTS = 3; // 最多同时存在3个商人
        
        // 状态
        private List<MysteryMerchant> _activeMerchants = new List<MysteryMerchant>();
        private PlayerMysteryMerchantData _playerData = new PlayerMysteryMerchantData();
        private float _spawnTimer = 0f;
        private Random _random = new Random();
        
        // 信号
        public static Signal<MysteryMerchant> MerchantSpawned { get; } = new Signal<MysteryMerchant>();
        public static Signal<MysteryMerchant> MerchantExpired { get; } = new Signal<MysteryMerchant>();
        public static Signal<string, int, int> ItemPurchased { get; } = new Signal<string, int, int>();
        public static Signal<MysteryMerchant> MerchantRefreshed { get; } = new Signal<MysteryMerchant>();
        
        public override void _Ready()
        {
            Instance = this;
            LoadPlayerData();
        }
        
        public override void _Process(float delta)
        {
            _spawnTimer += delta;
            
            // 检查商店过期
            for (int i = _activeMerchants.Count - 1; i >= 0; i--)
            {
                var merchant = _activeMerchants[i];
                merchant.RemainingTime -= delta;
                
                if (merchant.RemainingTime <= 0)
                {
                    ExpireMerchant(merchant);
                }
            }
            
            // 尝试生成新商店
            if (_spawnTimer >= MERCHANT_SPAWN_INTERVAL && _activeMerchants.Count < MAX_ACTIVE_MERCHANTS)
            {
                TrySpawnMerchant();
                _spawnTimer = 0f;
            }
        }
        
        // 生成商店
        private void TrySpawnMerchant()
        {
            var player = GetPlayer();
            if (player == null) return;
            
            int playerLevel = player.GetLevel();
            
            // 获取可用的商人类型
            var availableTypes = new List<MysteryMerchantType>();
            foreach (MysteryMerchantType type in Enum.GetValues(typeof(MysteryMerchantType)))
            {
                var config = MysteryMerchantDatabase.GetMerchantConfig(type);
                if (config != null && playerLevel >= (int)config["minLevel"])
                {
                    float chance = (float)config["spawnChance"];
                    if (_random.NextDouble() < chance)
                    {
                        availableTypes.Add(type);
                    }
                }
            }
            
            if (availableTypes.Count > 0)
            {
                var selectedType = availableTypes[_random.Next(availableTypes.Count)];
                CreateMerchant(selectedType);
            }
        }
        
        // 创建商店
        private void CreateMerchant(MysteryMerchantType type)
        {
            var config = MysteryMerchantDatabase.GetMerchantConfig(type);
            if (config == null) return;
            
            var merchant = new MysteryMerchant
            {
                MerchantId = Guid.NewGuid().ToString(),
                MerchantType = type,
                MerchantName = MysteryMerchantDatabase.GetMerchantTypeName(type),
                Description = (string)config["description"],
                SpawnChance = (float)config["spawnChance"],
                MinimumPlayerLevel = (int)config["minLevel"],
                RefreshCost = (int)config["refreshCost"],
                ExpireTime = DateTime.Now.AddSeconds(MERCHANT_DURATION),
                IsActive = true
            };
            
            // 生成商品
            var itemCountRange = (Vector2)config["itemCount"];
            int itemCount = _random.Next((int)itemCountRange.X, (int)itemCountRange.Y + 1);
            GenerateMerchantItems(merchant, itemCount, config);
            
            _activeMerchants.Add(merchant);
            MerchantSpawned?.Invoke(merchant);
        }
        
        // 生成商品
        private void GenerateMerchantItems(MysteryMerchant merchant, int count, Dictionary<string, object> config)
        {
            var priceRange = (Vector2)config["priceRange"];
            var discountRange = (Vector2)config["discount"];
            
            for (int i = 0; i < count; i++)
            {
                var item = new MysteryMerchantItem();
                
                // 随机选择商品类型
                string[] itemTypes = { "weapon", "armor", "potion", "material", "charm" };
                string itemType = itemTypes[_random.Next(itemTypes.Length)];
                
                // 从商品池中选择商品
                var typePool = MysteryMerchantDatabase.ItemPool;
                if (typePool.ContainsKey(itemType) && typePool[itemType].Count > 0)
                {
                    var itemData = typePool[itemType][_random.Next(typePool[itemType].Count)];
                    item.ItemId = (string)itemData["id"];
                    item.ItemName = (string)itemData["name"];
                    item.OriginalPrice = (int)itemData["price"];
                }
                else
                {
                    item.ItemId = $"item_{i}";
                    item.ItemName = $"神秘物品 {i + 1}";
                    item.OriginalPrice = priceRange.X + _random.Next((int)(priceRange.Y - priceRange.X));
                }
                
                // 随机稀有度
                item.Rarity = MysteryMerchantDatabase.GetRandomRarity();
                
                // 价格计算（考虑稀有度和折扣）
                float rarityMultiplier = 1.0f + ((int)item.Rarity * 0.5f);
                float discount = (float)(discountRange.X + _random.NextDouble() * (discountRange.Y - discountRange.X));
                item.Price = (int)(item.OriginalPrice * rarityMultiplier * discount);
                item.Discount = discount;
                
                // 库存
                item.MaxStock = _random.Next(1, 4);
                item.Stock = item.MaxStock;
                
                // 限时抢购（高稀有度更可能有）
                item.IsLimited = _random.NextDouble() < (0.1f + (int)item.Rarity * 0.05f);
                
                // 隐藏商品（传奇和神话稀有度）
                item.IsSecret = item.Rarity >= MerchantItemRarity.Legendary && _random.NextDouble() < 0.3f;
                
                // 生成额外属性
                var rarityConfig = MysteryMerchantDatabase.GetRarityConfig(item.Rarity);
                if (rarityConfig != null)
                {
                    int bonusCount = (int)rarityConfig["bonusAttributes"];
                    for (int j = 0; j < bonusCount; j++)
                    {
                        string attr = MysteryMerchantDatabase.AttributePool[_random.Next(MysteryMerchantDatabase.AttributePool.Length)];
                        int value = _random.Next(1, 10) * ((int)item.Rarity + 1);
                        item.BonusAttributes[attr] = value;
                    }
                }
                
                item.Description = GenerateItemDescription(item);
                merchant.Items.Add(item);
            }
        }
        
        // 生成物品描述
        private string GenerateItemDescription(MysteryMerchantItem item)
        {
            string desc = MysteryMerchantDatabase.GetRarityName(item.Rarity) + " ";
            
            if (item.BonusAttributes.Count > 0)
            {
                foreach (var attr in item.BonusAttributes)
                {
                    desc += $"+{attr.Value} {attr.Key} ";
                }
            }
            else
            {
                desc += "珍贵物品";
            }
            
            if (item.IsLimited)
                desc += " [限时]";
            if (item.IsSecret)
                desc += " [隐藏]";
                
            return desc;
        }
        
        // 商店过期
        private void ExpireMerchant(MysteryMerchant merchant)
        {
            merchant.IsActive = false;
            _activeMerchants.Remove(merchant);
            MerchantExpired?.Invoke(merchant);
        }
        
        // 购买商品
        public bool PurchaseItem(string merchantId, int itemIndex)
        {
            var merchant = GetMerchantById(merchantId);
            if (merchant == null || !merchant.IsActive) return false;
            if (itemIndex < 0 || itemIndex >= merchant.Items.Count) return false;
            
            var item = merchant.Items[itemIndex];
            if (item.Stock <= 0) return false;
            
            var player = GetPlayer();
            if (player == null) return false;
            
            int playerGold = player.GetGold();
            if (playerGold < item.Price) return false;
            
            // 扣除金币
            player.ModifyGold(-item.Price);
            
            // 添加物品到背包（简化版本，实际应该调用InventorySystem）
            // AddItemToInventory(item.ItemId, 1);
            
            // 更新库存
            item.Stock--;
            
            // 更新玩家数据
            _playerData.TotalPurchases++;
            _playerData.TotalGoldSpent += item.Price;
            _playerData.TotalItemsBought++;
            
            string rarityKey = item.Rarity.ToString();
            if (!_playerData.PurchasesByRarity.ContainsKey(rarityKey))
                _playerData.PurchasesByRarity[rarityKey] = 0;
            _playerData.PurchasesByRarity[rarityKey]++;
            
            string typeKey = merchant.MerchantType.ToString();
            if (!_playerData.PurchasesByType.ContainsKey(typeKey))
                _playerData.PurchasesByType[typeKey] = 0;
            _playerData.PurchasesByType[typeKey]++;
            
            // 幸运购买检测（隐藏物品）
            if (item.IsSecret)
            {
                _playerData.SecretItemsFound++;
                _playerData.LuckyPurchases++;
            }
            
            // 检查是否发现新商人类型
            if (!_playerData.UnlockedMerchantTypes.Contains(merchant.MerchantType.ToString()))
            {
                _playerData.UnlockedMerchantTypes.Add(merchant.MerchantType.ToString());
            }
            
            SavePlayerData();
            ItemPurchased?.Invoke(item.ItemId, 1, item.Price);
            
            return true;
        }
        
        // 刷新商店
        public bool RefreshMerchant(string merchantId)
        {
            var merchant = GetMerchantById(merchantId);
            if (merchant == null || !merchant.IsActive) return false;
            
            var player = GetPlayer();
            if (player == null) return false;
            
            if (player.GetGold() < merchant.RefreshCost) return false;
            
            // 扣除刷新费用
            player.ModifyGold(-merchant.RefreshCost);
            
            // 重新生成商品
            merchant.Items.Clear();
            var config = MysteryMerchantDatabase.GetMerchantConfig(merchant.MerchantType);
            if (config != null)
            {
                var itemCountRange = (Vector2)config["itemCount"];
                int itemCount = _random.Next((int)itemCountRange.X, (int)itemCountRange.Y + 1);
                GenerateMerchantItems(merchant, itemCount, config);
            }
            
            MerchantRefreshed?.Invoke(merchant);
            return true;
        }
        
        // 访问商店
        public void VisitMerchant(string merchantId)
        {
            var merchant = GetMerchantById(merchantId);
            if (merchant == null) return;
            
            _playerData.TotalVisits++;
            
            if (!_playerData.VisitHistory.ContainsKey(merchant.MerchantType.ToString()))
                _playerData.VisitHistory[merchant.MerchantType.ToString()] = 0;
            _playerData.VisitHistory[merchant.MerchantType.ToString()]++;
            
            SavePlayerData();
        }
        
        // 获取活跃商店列表
        public List<MysteryMerchant> GetActiveMerchants()
        {
            return new List<MysteryMerchant>(_activeMerchants);
        }
        
        // 根据ID获取商店
        public MysteryMerchant GetMerchantById(string merchantId)
        {
            return _activeMerchants.Find(m => m.MerchantId == merchantId);
        }
        
        // 获取玩家数据
        public PlayerMysteryMerchantData GetPlayerData()
        {
            return _playerData;
        }
        
        // 强制生成商店（测试用）
        public MysteryMerchant ForceSpawnMerchant(MysteryMerchantType type)
        {
            if (GetMerchantByType(type) != null) return null;
            
            CreateMerchant(type);
            return _activeMerchants.Find(m => m.MerchantType == type);
        }
        
        // 根据类型获取商店
        public MysteryMerchant GetMerchantByType(MysteryMerchantType type)
        {
            return _activeMerchants.Find(m => m.MerchantType == type);
        }
        
        // 获取玩家实例
        private Player GetPlayer()
        {
            var tree = GetTree();
            if (tree == null) return null;
            
            var players = tree.GetNodesInGroup("player");
            if (players.Count > 0)
                return players[0] as Player;
                
            return null;
        }
        
        // 存档
        private void SavePlayerData()
        {
            // 简化版本：保存到文件
            // 实际应该使用SaveSystem
            var savePath = "user://mystery_merchant_data.json";
            // SaveSystem.SaveData(savePath, _playerData);
        }
        
        // 读档
        private void LoadPlayerData()
        {
            // 简化版本：从文件加载
            var savePath = "user://mystery_merchant_data.json";
            // _playerData = SaveSystem.LoadData<PlayerMysteryMerchantData>(savePath);
            if (_playerData == null)
                _playerData = new PlayerMysteryMerchantData();
        }
        
        // 获取统计信息
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                { "totalVisits", _playerData.TotalVisits },
                { "totalPurchases", _playerData.TotalPurchases },
                { "totalGoldSpent", _playerData.TotalGoldSpent },
                { "totalItemsBought", _playerData.TotalItemsBought },
                { "luckyPurchases", _playerData.LuckyPurchases },
                { "secretItemsFound", _playerData.SecretItemsFound },
                { "unlockedMerchantTypes", _playerData.UnlockedMerchantTypes.Count }
            };
        }
    }

        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary<string, Variant>();
            
            // 保存玩家数据统计
            data["totalVisits"] = _playerData.TotalVisits;
            data["totalPurchases"] = _playerData.TotalPurchases;
            data["totalGoldSpent"] = _playerData.TotalGoldSpent;
            data["totalItemsBought"] = _playerData.TotalItemsBought;
            data["luckyPurchases"] = _playerData.LuckyPurchases;
            data["secretItemsFound"] = _playerData.SecretItemsFound;
            data["unlockedMerchantTypes"] = new List<Variant>(_playerData.UnlockedMerchantTypes);
            
            // 保存购买统计（按稀有度）
            var purchasesByRarity = new Dictionary<string, Variant>();
            foreach (var kvp in _playerData.PurchasesByRarity)
            {
                purchasesByRarity[kvp.Key] = kvp.Value;
            }
            data["purchasesByRarity"] = purchasesByRarity;
            
            // 保存购买统计（按商人类型）
            var purchasesByType = new Dictionary<string, Variant>();
            foreach (var kvp in _playerData.PurchasesByType)
            {
                purchasesByType[kvp.Key] = kvp.Value;
            }
            data["purchasesByType"] = purchasesByType;
            
            // 保存访问历史
            var visitHistory = new Dictionary<string, Variant>();
            foreach (var kvp in _playerData.VisitHistory)
            {
                visitHistory[kvp.Key] = kvp.Value;
            }
            data["visitHistory"] = visitHistory;
            
            // 保存活跃商店状态（用于恢复剩余时间）
            var activeMerchantsData = new List<Dictionary<string, Variant>>();
            foreach (var merchant in _activeMerchants)
            {
                activeMerchantsData.Add(new Dictionary<string, Variant>
                {
                    { "merchantId", merchant.MerchantId },
                    { "merchantType", (int)merchant.MerchantType },
                    { "merchantName", merchant.MerchantName },
                    { "remainingTime", merchant.RemainingTime },
                    { "expireTime", merchant.ExpireTime.Ticks }
                });
            }
            data["activeMerchants"] = activeMerchantsData;
            
            // 保存生成计时器
            data["spawnTimer"] = _spawnTimer;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            // 加载玩家数据统计
            if (data.TryGetValue("totalVisits", out var totalVisits))
                _playerData.TotalVisits = (int)totalVisits;
            if (data.TryGetValue("totalPurchases", out var totalPurchases))
                _playerData.TotalPurchases = (int)totalPurchases;
            if (data.TryGetValue("totalGoldSpent", out var totalGoldSpent))
                _playerData.TotalGoldSpent = (int)totalGoldSpent;
            if (data.TryGetValue("totalItemsBought", out var totalItemsBought))
                _playerData.TotalItemsBought = (int)totalItemsBought;
            if (data.TryGetValue("luckyPurchases", out var luckyPurchases))
                _playerData.LuckyPurchases = (int)luckyPurchases;
            if (data.TryGetValue("secretItemsFound", out var secretItemsFound))
                _playerData.SecretItemsFound = (int)secretItemsFound;
            if (data.TryGetValue("unlockedMerchantTypes", out var unlockedTypes))
                _playerData.UnlockedMerchantTypes = new List<string>((IEnumerable<string>)unlockedTypes);
            
            // 加载购买统计（按稀有度）
            if (data.TryGetValue("purchasesByRarity", out var purchasesByRarity))
            {
                var pbr = (Dictionary<string, Variant>)purchasesByRarity;
                foreach (var kvp in pbr)
                {
                    _playerData.PurchasesByRarity[kvp.Key] = (int)kvp.Value;
                }
            }
            
            // 加载购买统计（按商人类型）
            if (data.TryGetValue("purchasesByType", out var purchasesByType))
            {
                var pbt = (Dictionary<string, Variant>)purchasesByType;
                foreach (var kvp in pbt)
                {
                    _playerData.PurchasesByType[kvp.Key] = (int)kvp.Value;
                }
            }
            
            // 加载访问历史
            if (data.TryGetValue("visitHistory", out var visitHistory))
            {
                var vh = (Dictionary<string, Variant>)visitHistory;
                foreach (var kvp in vh)
                {
                    _playerData.VisitHistory[kvp.Key] = (int)kvp.Value;
                }
            }
            
            // 加载活跃商店状态
            if (data.TryGetValue("activeMerchants", out var activeMerchantsData))
            {
                var merchantsList = (List<Variant>)activeMerchantsData;
                foreach (var merchantVar in merchantsList)
                {
                    var mData = (Dictionary<string, Variant>)merchantVar;
                    var merchant = new MysteryMerchant
                    {
                        MerchantId = (string)mData["merchantId"],
                        MerchantType = (MysteryMerchantType)(int)mData["merchantType"],
                        MerchantName = (string)mData["merchantName"],
                        RemainingTime = (float)mData["remainingTime"],
                        ExpireTime = new DateTime((long)mData["expireTime"]),
                        IsActive = true
                    };
                    _activeMerchants.Add(merchant);
                }
            }
            
            // 加载生成计时器
            if (data.TryGetValue("spawnTimer", out var spawnTimer))
                _spawnTimer = (float)spawnTimer;
        }
}
