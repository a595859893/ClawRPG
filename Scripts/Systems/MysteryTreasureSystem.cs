using Godot;
using System;
using System.Collections.Generic;

public class MysteryTreasureSystem : Node
{
    private static MysteryTreasureSystem _instance;
    public static MysteryTreasureSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                GD.PrintErr("MysteryTreasureSystem not initialized!");
            }
            return _instance;
        }
    }

    // 信号
    public static void EmitTreasureFound(string treasureId, Vector2 position) { }
    public static void EmitTreasureOpened(string treasureId, Dictionary<string, int> rewards) { }

    private MysteryTreasureDatabase _database;
    private PlayerMysteryTreasureData _playerData;
    private List<TreasureInstance> _activeTreasures = new List<TreasureInstance>();
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private float _spawnTimer = 0f;
    private float _spawnInterval = 30f; // 每30秒尝试生成宝藏
    private int _maxActiveTreasures = 10;
    private int _regionTreasureMultiplier = 1;

    // 信号定义
    public Signal<string, Vector2> TreasureFound { get; }
    public Signal<string, Dictionary<string, int>> TreasureOpened { get; }
    public Signal<string> TreasureDiscovered { get; }
    public Signal<int> GoldEarned { get; }
    public Signal<int> ExpEarned { get; }

    public override void _Ready()
    {
        _instance = this;
        _database = MysteryTreasureDatabase.Instance;
        _playerData = new PlayerMysteryTreasureData
        {
            RarityCount = new Dictionary<string, int>(),
            TypeCount = new Dictionary<string, int>(),
            DiscoveredTreasureIds = new List<string>(),
            TreasureHistory = new Dictionary<string, int>()
        };
        
        // 初始化稀有度和类型统计
        foreach (TreasureRarity rarity in Enum.GetValues(typeof(TreasureRarity)))
            _playerData.RarityCount[rarity.ToString()] = 0;
        foreach (TreasureType type in Enum.GetValues(typeof(TreasureType)))
            _playerData.TypeCount[type.ToString()] = 0;

        // 初始化随机数生成器
        _rng.Randomize();
        
        GD.Print("MysteryTreasureSystem initialized");
    }

    public override void _Process(float delta)
    {
        _spawnTimer += delta;
        if (_spawnTimer >= _spawnInterval)
        {
            _spawnTimer = 0f;
            TrySpawnTreasure();
        }
    }

    // 尝试生成宝藏
    private void TrySpawnTreasure()
    {
        if (_activeTreasures.Count >= _maxActiveTreasures)
            return;

        var treasure = _database.GetRandomTreasure();
        if (treasure == null)
            return;

        // 随机位置（在当前区域）
        Vector2 spawnPos = GetRandomSpawnPosition();
        
        var instance = new TreasureInstance
        {
            InstanceId = Guid.NewGuid().ToString(),
            TreasureId = treasure.TreasureId,
            Position = spawnPos,
            IsOpened = false,
            IsDiscovered = false,
            SpawnTime = Time.GetTicksMsec() / 1000f
        };
        
        _activeTreasures.Add(instance);
        
        // 发送信号
        EmitSignal(nameof(TreasureFound), treasure.TreasureId, spawnPos);
    }

    // 获取随机生成位置
    private Vector2 GetRandomSpawnPosition()
    {
        // 获取玩家位置并在附近生成
        var player = GetTree().GetFirstNodeInGroup("Player");
        if (player != null)
        {
            Node2D playerNode = player as Node2D;
            if (playerNode != null)
            {
                float angle = _rng.Randf() * Mathf.PI * 2;
                float distance = _rng.RandfRange(200f, 500f);
                return playerNode.GlobalPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
            }
        }
        
        return new Vector2(_rng.RandfRange(-1000f, 1000f), _rng.RandfRange(-1000f, 1000f));
    }

    // 打开宝藏
    public bool OpenTreasure(string instanceId)
    {
        var instance = _activeTreasures.Find(t => t.InstanceId == instanceId);
        if (instance == null || instance.IsOpened)
            return false;

        var treasure = _database.GetTreasureById(instance.TreasureId);
        if (treasure == null)
            return false;

        instance.IsOpened = true;
        
        // 生成奖励
        Dictionary<string, int> rewards = new Dictionary<string, int>();
        
        // 金币奖励
        int goldReward = _rng.RandiRange(treasure.MinGold, treasure.MaxGold);
        goldReward = (int)(goldReward * _regionTreasureMultiplier);
        rewards["gold"] = goldReward;
        
        // 添加金币给玩家
        var player = GetTree().GetFirstNodeInGroup("Player");
        if (player != null)
        {
            Player p = player as Player;
            if (p != null)
            {
                p.AddGold(goldReward);
            }
        }
        
        // 经验奖励
        int expReward = (int)(treasure.ExpReward * _regionTreasureMultiplier);
        rewards["exp"] = expReward;
        
        // 物品奖励
        if (treasure.ItemIds != null && treasure.ItemIds.Count > 0)
        {
            for (int i = 0; i < treasure.ItemIds.Count; i++)
            {
                string itemId = treasure.ItemIds[i];
                int count = treasure.ItemCounts != null && i < treasure.ItemCounts.Count ? treasure.ItemCounts[i] : 1;
                
                rewards[itemId] = count;
                
                // 添加物品到背包
                if (player != null)
                {
                    Player p = player as Player;
                    if (p != null && p.Inventory != null)
                    {
                        p.Inventory.AddItem(itemId, count);
                    }
                }
            }
        }

        // 更新玩家数据
        UpdatePlayerData(treasure, rewards);
        
        // 发送信号
        EmitSignal(nameof(TreasureOpened), treasure.TreasureId, rewards);
        
        // 移除已打开的宝藏
        _activeTreasures.Remove(instance);
        
        return true;
    }

    // 发现宝藏（玩家靠近时）
    public void DiscoverTreasure(string instanceId)
    {
        var instance = _activeTreasures.Find(t => t.InstanceId == instanceId);
        if (instance != null && !instance.IsDiscovered)
        {
            instance.IsDiscovered = true;
            var treasure = _database.GetTreasureById(instance.TreasureId);
            if (treasure != null)
            {
                EmitSignal(nameof(TreasureDiscovered), treasure.TreasureId);
            }
        }
    }

    // 更新玩家数据
    private void UpdatePlayerData(MysteryTreasureData treasure, Dictionary<string, int> rewards)
    {
        _playerData.TotalFound++;
        
        // 更新稀有度统计
        string rarityKey = treasure.Rarity.ToString();
        if (_playerData.RarityCount.ContainsKey(rarityKey))
            _playerData.RarityCount[rarityKey]++;
        
        // 更新类型统计
        string typeKey = treasure.Type.ToString();
        if (_playerData.TypeCount.ContainsKey(typeKey))
            _playerData.TypeCount[typeKey]++;
        
        // 更新金币和经验
        if (rewards.ContainsKey("gold"))
            _playerData.TotalGoldEarned += rewards["gold"];
        if (rewards.ContainsKey("exp"))
            _playerData.TotalExpEarned += rewards["exp"];
        
        // 更新发现列表
        if (!_playerData.DiscoveredTreasureIds.Contains(treasure.TreasureId))
            _playerData.DiscoveredTreasureIds.Add(treasure.TreasureId);
        
        // 更新历史记录
        if (_playerData.TreasureHistory.ContainsKey(treasure.TreasureId))
            _playerData.TreasureHistory[treasure.TreasureId]++;
        else
            _playerData.TreasureHistory[treasure.TreasureId] = 1;
    }

    // 获取活跃宝藏列表
    public List<TreasureInstance> GetActiveTreasures() => new List<TreasureInstance>(_activeTreasures);

    // 获取玩家数据
    public PlayerMysteryTreasureData GetPlayerData() => _playerData;

    // 获取统计信息
    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            { "total_found", _playerData.TotalFound },
            { "total_gold", _playerData.TotalGoldEarned },
            { "total_exp", _playerData.TotalExpEarned },
            { "common_count", _playerData.RarityCount.GetValueOrDefault("Common", 0) },
            { "uncommon_count", _playerData.RarityCount.GetValueOrDefault("Uncommon", 0) },
            { "rare_count", _playerData.RarityCount.GetValueOrDefault("Rare", 0) },
            { "epic_count", _playerData.RarityCount.GetValueOrDefault("Epic", 0) },
            { "legendary_count", _playerData.RarityCount.GetValueOrDefault("Legendary", 0) }
        };
    }

    // 手动生成宝藏（用于测试或事件）
    public void SpawnTreasure(string treasureId)
    {
        var treasure = _database.GetTreasureById(treasureId);
        if (treasure == null)
            return;

        Vector2 spawnPos = GetRandomSpawnPosition();
        
        var instance = new TreasureInstance
        {
            InstanceId = Guid.NewGuid().ToString(),
            TreasureId = treasureId,
            Position = spawnPos,
            IsOpened = false,
            IsDiscovered = false,
            SpawnTime = Time.GetTicksMsec() / 1000f
        };
        
        _activeTreasures.Add(instance);
        EmitSignal(nameof(TreasureFound), treasureId, spawnPos);
    }

    // 强制生成随机宝藏
    public void ForceSpawnRandomTreasure(TreasureRarity minRarity = TreasureRarity.Common)
    {
        var treasure = _database.GetRandomTreasureByRarity(minRarity);
        if (treasure == null)
            return;

        SpawnTreasure(treasure.TreasureId);
    }

    // 设置生成间隔
    public void SetSpawnInterval(float seconds)
    {
        _spawnInterval = Mathf.Max(10f, seconds);
    }

    // 设置最大活跃宝藏数
    public void SetMaxActiveTreasures(int max)
    {
        _maxActiveTreasures = Mathf.Max(1, max);
    }

    // 设置区域宝藏倍率
    public void SetRegionTreasureMultiplier(int multiplier)
    {
        _regionTreasureMultiplier = Mathf.Max(1, multiplier);
    }

    // 清除所有活跃宝藏
    public void ClearAllTreasures()
    {
        _activeTreasures.Clear();
    }

    // 存档
    public Dictionary<string, object> Save()
    {
        Dictionary<string, object> data = new Dictionary<string, object>();
        
        data["player_data"] = new Dictionary<string, object>
        {
            { "total_found", _playerData.TotalFound },
            { "total_gold", _playerData.TotalGoldEarned },
            { "total_exp", _playerData.TotalExpEarned },
            { "rarity_count", _playerData.RarityCount },
            { "type_count", _playerData.TypeCount },
            { "discovered_ids", _playerData.DiscoveredTreasureIds },
            { "treasure_history", _playerData.TreasureHistory }
        };
        
        return data;
    }

    // 读档
    public void Load(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("player_data"))
        {
            var playerData = data["player_data"] as Dictionary<string, object>;
            if (playerData != null)
            {
                _playerData.TotalFound = playerData.GetValueOrDefault("total_found", 0);
                _playerData.TotalGoldEarned = playerData.GetValueOrDefault("total_gold", 0);
                _playerData.TotalExpEarned = playerData.GetValueOrDefault("total_exp", 0);
                
                if (playerData.ContainsKey("rarity_count"))
                {
                    var rarityCount = playerData["rarity_count"] as Dictionary<string, object>;
                    foreach (var kvp in rarityCount)
                    {
                        _playerData.RarityCount[kvp.Key] = Convert.ToInt32(kvp.Value);
                    }
                }
                
                if (playerData.ContainsKey("type_count"))
                {
                    var typeCount = playerData["type_count"] as Dictionary<string, object>;
                    foreach (var kvp in typeCount)
                    {
                        _playerData.TypeCount[kvp.Key] = Convert.ToInt32(kvp.Value);
                    }
                }
                
                if (playerData.ContainsKey("discovered_ids"))
                {
                    _playerData.DiscoveredTreasureIds = new List<string>(
                        playerData["discovered_ids"] as System.Collections.IEnumerable
                    );
                }
                
                if (playerData.ContainsKey("treasure_history"))
                {
                    var history = playerData["treasure_history"] as Dictionary<string, object>;
                    foreach (var kvp in history)
                    {
                        _playerData.TreasureHistory[kvp.Key] = Convert.ToInt32(kvp.Value);
                    }
                }
            }
        }
        
        GD.Print("MysteryTreasureSystem loaded");
    }
}
