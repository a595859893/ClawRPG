using Godot;
using System;
using System.Collections.Generic;

public class TreasureHuntManager : BaseSystem
{
    public static TreasureHuntManager Instance { get; private set; }
    
    // Treasure data structure
    public class Treasure
    {
        public string id;
        public string name;
        public string description;
        public int goldReward;
        public int expReward;
        public string itemId;
        public float dropChance;
    }
    
    // Hunt region data
    public class HuntRegion
    {
        public string id;
        public string name;
        public string description;
        public int requiredLevel;
        public int energyCost;
        public float successRate;
        public List<Treasure> treasures;
    }
    
    // Player hunt data
    private Dictionary<int, PlayerHuntData> playerHuntData = new Dictionary<int, PlayerHuntData>();
    
    public class PlayerHuntData
    {
        public int totalHunts;
        public int successfulHunts;
        public int totalGoldEarned;
        public int totalExpEarned;
        public int currentEnergy;
        public int maxEnergy;
        public DateTime lastEnergyRefresh;
        public List<string> discoveredTreasures = new List<string>();
        public Dictionary<string, int> regionHuntCount = new Dictionary<string, int>();
    }
    
    // Regions
    private List<HuntRegion> regions = new List<HuntRegion>();
    private Dictionary<string, HuntRegion> regionsById = new Dictionary<string, HuntRegion>();
    
    // UI
    private Control ui;
    private bool isUIVisible = false;
    
    public override void _Ready()
    {
        Instance = this;
        InitializeRegions();
    }
    
    private void InitializeRegions()
    {
        // Create hunting regions
        HuntRegion forest = new HuntRegion
        {
            id = "forest",
            name = "Ancient Forest",
            description = "A mystical forest filled with hidden treasures",
            requiredLevel = 1,
            energyCost = 10,
            successRate = 0.7f,
            treasures = new List<Treasure>
            {
                new Treasure { id = "forest_gold_1", name = "Hidden Gold", description = "Gold coins buried under a tree", goldReward = 100, expReward = 20, dropChance = 0.5f },
                new Treasure { id = "forest_herb", name = "Rare Herb", description = "A precious medicinal herb", goldReward = 50, expReward = 30, itemId = "herb_red", dropChance = 0.3f },
                new Treasure { id = "forest_chest", name = "Old Chest", description = "An abandoned treasure chest", goldReward = 200, expReward = 50, dropChance = 0.2f }
            }
        };
        
        HuntRegion mountain = new HuntRegion
        {
            id = "mountain",
            name = "Crystal Mountain",
            description = "A mountain rich with crystal deposits",
            requiredLevel = 10,
            energyCost = 15,
            successRate = 0.6f,
            treasures = new List<Treasure>
            {
                new Treasure { id = "mountain_crystal", name = "Crystal Shard", description = "A fragment of rare crystal", goldReward = 150, expReward = 40, itemId = "crystal_blue", dropChance = 0.4f },
                new Treasure { id = "mountain_gem", name = "Precious Gem", description = "A valuable gemstone", goldReward = 300, expReward = 60, itemId = "gem_ruby", dropChance = 0.25f },
                new Treasure { id = "mountain_ore", name = "Rare Ore", description = "A piece of rare ore", goldReward = 200, expReward = 45, dropChance = 0.35f }
            }
        };
        
        HuntRegion desert = new HuntRegion
        {
            id = "desert",
            name = "Sahara Desert",
            description = "An ancient desert with buried secrets",
            requiredLevel = 20,
            energyCost = 20,
            successRate = 0.55f,
            treasures = new List<Treasure>
            {
                new Treasure { id = "desert_coin", name = "Ancient Coin", description = "A coin from an ancient civilization", goldReward = 250, expReward = 50, itemId = "coin_ancient", dropChance = 0.35f },
                new Treasure { id = "desert_artifact", name = "Desert Artifact", description = "A mysterious artifact", goldReward = 500, expReward = 100, itemId = "artifact_scarab", dropChance = 0.15f },
                new Treasure { id = "desert_gem", name = "Desert Jewel", description = "A beautiful desert jewel", goldReward = 350, expReward = 70, itemId = "gem_sapphire", dropChance = 0.25f }
            }
        };
        
        HuntRegion ocean = new HuntRegion
        {
            id = "ocean",
            name = "Sunken Kingdom",
            description = "An underwater kingdom lost to time",
            requiredLevel = 30,
            energyCost = 25,
            successRate = 0.5f,
            treasures = new List<Treasure>
            {
                new Treasure { id = "ocean_pearl", name = "Pearl of Wisdom", description = "A luminous pearl", goldReward = 400, expReward = 80, itemId = "pearl", dropChance = 0.3f },
                new Treasure { id = "ocean_relic", name = "Ancient Relic", description = "A relic from the sunken kingdom", goldReward = 600, expReward = 120, itemId = "relic_trident", dropChance = 0.2f },
                new Treasure { id = "ocean_treasure", name = "Royal Treasure", description = "Treasure from the king", goldReward = 800, expReward = 150, dropChance = 0.1f }
            }
        };
        
        HuntRegion volcano = new HuntRegion
        {
            id = "volcano",
            name = "Volcanic Core",
            description = "The heart of an active volcano",
            requiredLevel = 40,
            energyCost = 30,
            successRate = 0.45f,
            treasures = new List<Treasure>
            {
                new Treasure { id = "volcano_ingot", name = "Fire Ingot", description = "An ingot of volcanic metal", goldReward = 500, expReward = 100, itemId = "ingot_fire", dropChance = 0.3f },
                new Treasure { id = "volcano_gem", name = "Magma Gem", description = "A gem forged in fire", goldReward = 700, expReward = 140, itemId = "gem_magma", dropChance = 0.2f },
                new Treasure { id = "volcano_artifact", name = "Volcanic Artifact", description = "An artifact of immense power", goldReward = 1000, expReward = 200, itemId = "artifact_ volcanic", dropChance = 0.1f }
            }
        };
        
        HuntRegion ice = new HuntRegion
        {
            id = "ice",
            name = "Frozen Tundra",
            description = "A frozen wilderness with ancient secrets",
            requiredLevel = 50,
            energyCost = 35,
            successRate = 0.4f,
            treasures = new List<Treasure>
            {
                new Treasure { id = "ice_crystal", name = "Ice Crystal", description = "A crystal of eternal winter", goldReward = 600, expReward = 120, itemId = "crystal_ice", dropChance = 0.25f },
                new Treasure { id = "ice_artifact", name = "Frost Artifact", description = "An artifact of ice and snow", goldReward = 1200, expReward = 250, itemId = "artifact_frost", dropChance = 0.1f },
                new Treasure { id = "ice_relic", name = "Ancient Ice Relic", description = "A relic from the ice age", goldReward = 800, expReward = 160, itemId = "relic_ice", dropChance = 0.15f }
            }
        };
        
        regions.Add(forest);
        regions.Add(mountain);
        regions.Add(desert);
        regions.Add(ocean);
        regions.Add(volcano);
        regions.Add(ice);
        
        // Build O(1) lookup dictionary
        foreach (var r in regions)
        {
            regionsById[r.id] = r;
        }
    }
    
    public void ToggleUI()
    {
        isUIVisible = !isUIVisible;
        
        if (ui != null)
        {
            ui.Visible = isUIVisible;
            if (isUIVisible)
            {
                UpdateUI();
            }
        }
    }
    
    public void CreateUI()
    {
        // Create UI if it doesn't exist
        if (ui == null)
        {
            ui = (Control)GD.Load<PackedScene>("res://UI/TreasureHuntUI.tscn").Instance();
            // Use CallDeferred to safely add to scene tree at a safe lifecycle point
            GetTree().CurrentScene.CallDeferred("add_child", ui);
            ui.Visible = isUIVisible;
        }
    }
    
    private void UpdateUI()
    {
        // Update UI elements
        // This would be implemented based on the actual UI scene
    }
    
    public bool StartHunt(int playerId, string regionId)
    {
        // Get player data
        if (!playerHuntData.ContainsKey(playerId))
        {
            InitializePlayerData(playerId);
        }
        
        PlayerHuntData data = playerHuntData[playerId];
        
        // Find region — O(1) dictionary lookup
        if (!regionsById.TryGetValue(regionId, out HuntRegion region))
        {
            return false;
        }
        
        // Check requirements
        int playerLevel = Player.Instance != null ? Player.Instance.level : 1;
        if (playerLevel < region.requiredLevel)
        {
            return false;
        }
        
        if (data.currentEnergy < region.energyCost)
        {
            return false;
        }
        
        // Deduct energy
        data.currentEnergy -= region.energyCost;
        
        // Determine success — use thread-safe shared Random instance
        bool success = Random.Shared.NextDouble() < region.successRate;
        
        data.totalHunts++;
        
        if (!data.regionHuntCount.ContainsKey(regionId))
        {
            data.regionHuntCount[regionId] = 0;
        }
        data.regionHuntCount[regionId]++;
        
        if (success)
        {
            data.successfulHunts++;
            
            // Select treasure
            Treasure treasure = SelectTreasure(region.treasures);
            
            // Award rewards
            data.totalGoldEarned += treasure.goldReward;
            data.totalExpEarned += treasure.expReward;
            
            if (Player.Instance != null)
            {
                Player.Instance.AddGold(treasure.goldReward);
                Player.Instance.AddExp(treasure.expReward);
                
                if (!string.IsNullOrEmpty(treasure.itemId) && Random.Shared.NextDouble() < treasure.dropChance)
                {
                    // Add item to inventory
                    if (InventoryManager.Instance != null)
                    {
                        InventoryManager.Instance.AddItem(treasure.itemId, 1);
                    }
                }
            }
            
            // Track discovered treasure
            if (!data.discoveredTreasures.Contains(treasure.id))
            {
                data.discoveredTreasures.Add(treasure.id);
            }
            
            // Show discovery notification
            ShowTreasureDiscovery(treasure);
        }
        
        return true;
    }
    
    private Treasure SelectTreasure(List<Treasure> treasures)
    {
        float roll = (float)Random.Shared.NextDouble();
        float cumulative = 0;
        
        foreach (var treasure in treasures)
        {
            cumulative += treasure.dropChance;
            if (roll <= cumulative)
            {
                return treasure;
            }
        }
        
        return treasures[treasures.Count - 1];
    }
    
    private void ShowTreasureDiscovery(Treasure treasure)
    {
        // Show notification to player
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification($"Found: {treasure.name}!", NotificationManager.NotificationType.Treasure);
        }
    }
    
    public void RefreshEnergy(int playerId)
    {
        if (!playerHuntData.ContainsKey(playerId))
        {
            InitializePlayerData(playerId);
        }
        
        PlayerHuntData data = playerHuntData[playerId];
        
        // Refresh energy (called periodically)
        if (data.currentEnergy < data.maxEnergy)
        {
            data.currentEnergy = Mathf.Min(data.currentEnergy + 5, data.maxEnergy);
        }
    }
    
    private void InitializePlayerData(int playerId)
    {
        playerHuntData[playerId] = new PlayerHuntData
        {
            totalHunts = 0,
            successfulHunts = 0,
            totalGoldEarned = 0,
            totalExpEarned = 0,
            currentEnergy = 100,
            maxEnergy = 100,
            lastEnergyRefresh = DateTime.Now,
            discoveredTreasures = new List<string>(),
            regionHuntCount = new Dictionary<string, int>()
        };
    }
    
    public PlayerHuntData GetPlayerData(int playerId)
    {
        if (!playerHuntData.ContainsKey(playerId))
        {
            InitializePlayerData(playerId);
        }
        return playerHuntData[playerId];
    }
    
    public List<HuntRegion> GetRegions()
    {
        return regions;
    }
    
    public void SaveData(int playerId)
    {
        if (playerHuntData.ContainsKey(playerId))
        {
            // Save to file
            string path = $"user://treasure_hunt_{playerId}.json";
            using (var file = FileAccess.Open(path, FileAccess.ModeFlags.Write))
            {
                var data = playerHuntData[playerId];
                string json = Json.Stringify(new Dictionary<string, object>
                {
                    { "totalHunts", data.totalHunts },
                    { "successfulHunts", data.successfulHunts },
                    { "totalGoldEarned", data.totalGoldEarned },
                    { "totalExpEarned", data.totalExpEarned },
                    { "currentEnergy", data.currentEnergy },
                    { "maxEnergy", data.maxEnergy },
                    { "discoveredTreasures", data.discoveredTreasures },
                    { "regionHuntCount", data.regionHuntCount }
                });
                file.StoreString(json);
            }
        }
    }
    
    public void LoadData(int playerId)
    {
        string path = $"user://treasure_hunt_{playerId}.json";
        if (FileAccess.FileExists(path))
        {
            using (var file = FileAccess.Open(path, FileAccess.ModeFlags.Read))
            {
                string json = file.GetAsString();
                var dict = Json.ParseString(json).AsDict();
                
                PlayerHuntData data = new PlayerHuntData
                {
                    totalHunts = (int)dict["totalHunts"].AsDouble(),
                    successfulHunts = (int)dict["successfulHunts"].AsDouble(),
                    totalGoldEarned = (int)dict["totalGoldEarned"].AsDouble(),
                    totalExpEarned = (int)dict["totalExpEarned"].AsDouble(),
                    currentEnergy = (int)dict["currentEnergy"].AsDouble(),
                    maxEnergy = (int)dict["maxEnergy"].AsDouble(),
                    discoveredTreasures = new List<string>(),
                    regionHuntCount = new Dictionary<string, int>()
                };
                
                var discovered = dict["discoveredTreasures"].AsArray();
                foreach (var d in discovered)
                {
                    data.discoveredTreasures.Add(d.ToString());
                }
                
                var regionCounts = dict["regionHuntCount"].AsDict();
                foreach (var kvp in regionCounts)
                {
                    data.regionHuntCount[kvp.Key] = (int)kvp.Value.AsDouble();
                }
                
                playerHuntData[playerId] = data;
            }
        }
        else
        {
            InitializePlayerData(playerId);
        }
    }

    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary<string, Variant>();

        // 保存玩家寻宝数据
        var allPlayerData = new Dictionary<string, Dictionary<string, Variant>>();
        foreach (var kvp in playerHuntData)
        {
            var playerData = new Dictionary<string, Variant>
            {
                ["total_hunts"] = kvp.Value.totalHunts,
                ["successful_hunts"] = kvp.Value.successfulHunts,
                ["total_gold_earned"] = kvp.Value.totalGoldEarned,
                ["total_exp_earned"] = kvp.Value.totalExpEarned,
                ["current_energy"] = kvp.Value.currentEnergy,
                ["max_energy"] = kvp.Value.maxEnergy,
                ["last_energy_refresh"] = kvp.Value.lastEnergyRefresh.ToString("o"),
                ["discovered_treasures"] = new List<string>(kvp.Value.discoveredTreasures)
            };

            // 保存区域狩猎次数
            var regionCounts = new Dictionary<string, int>();
            foreach (var regionKvp in kvp.Value.regionHuntCount)
            {
                regionCounts[regionKvp.Key] = regionKvp.Value;
            }
            playerData["region_hunt_count"] = regionCounts;

            allPlayerData[kvp.Key.ToString()] = playerData;
        }
        data["player_hunt_data"] = allPlayerData;

        return data;
    }

    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;

        // 加载玩家寻宝数据
        if (data.TryGetValue("player_hunt_data", out var playerDataDict))
        {
            playerHuntData = new Dictionary<int, PlayerHuntData>();
            var allData = (Dictionary<string, Variant>)playerDataDict;
            foreach (var kvp in allData)
            {
                if (int.TryParse(kvp.Key, out var playerId))
                {
                    var pData = (Dictionary<string, Variant>)kvp.Value;
                    var playerData = new PlayerHuntData();

                    if (pData.TryGetValue("total_hunts", out var totalHunts))
                        playerData.totalHunts = (int)totalHunts;
                    if (pData.TryGetValue("successful_hunts", out var successfulHunts))
                        playerData.successfulHunts = (int)successfulHunts;
                    if (pData.TryGetValue("total_gold_earned", out var totalGold))
                        playerData.totalGoldEarned = (int)totalGold;
                    if (pData.TryGetValue("total_exp_earned", out var totalExp))
                        playerData.totalExpEarned = (int)totalExp;
                    if (pData.TryGetValue("current_energy", out var currentEnergy))
                        playerData.currentEnergy = (int)currentEnergy;
                    if (pData.TryGetValue("max_energy", out var maxEnergy))
                        playerData.maxEnergy = (int)maxEnergy;
                    if (pData.TryGetValue("last_energy_refresh", out var lastRefresh) && DateTime.TryParse((string)lastRefresh, out var parsed))
                        playerData.lastEnergyRefresh = parsed;
                    if (pData.TryGetValue("discovered_treasures", out var discovered))
                        playerData.discoveredTreasures = new List<string>((List<string>)discovered);

                    if (pData.TryGetValue("region_hunt_count", out var regionData))
                    {
                        playerData.regionHuntCount = new Dictionary<string, int>();
                        var regionDict = (Dictionary<string, Variant>)regionData;
                        foreach (var regionKvp in regionDict)
                        {
                            playerData.regionHuntCount[regionKvp.Key] = (int)regionKvp.Value;
                        }
                    }

                    playerHuntData[playerId] = playerData;
                }
            }
        }
    }
}
