using Godot;
using System.Collections.Generic;

public class RogueLegacySystem : Node
{
    private RogueLegacyData _data;
    private RogueLegacyDatabase _database;
    
    // 继承百分比
    private int _goldInheritancePercent = 25;
    private int _experienceInheritancePercent = 50;
    private int _itemInheritancePercent = 30;
    
    // 当前运行数据
    private int _currentFloor = 1;
    private int _currentGold = 0;
    private int _currentExperience = 0;
    private int _enemiesDefeated = 0;
    private int _bossesDefeated = 0;
    private bool _isRunActive = false;
    
    // 继承加成
    private int _bonusAttack = 0;
    private int _bonusDefense = 0;
    private int _bonusHealth = 0;
    private int _bonusSpeed = 0;
    private int _bonusCritical = 0;
    
    // 特殊加成
    private int _startingGoldBonus = 0;
    private float _enemyScalingModifier = 0;
    private int _luckBonus = 0;
    
    public override void _Ready()
    {
        _database = new RogueLegacyDatabase();
        _data = new RogueLegacyData();
        
        LoadData();
        UpdateBonuses();
    }
    
    private void LoadData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem == null) return;
        
        var data = saveSystem.LoadGame();
        if (data == null) return;
        
        if (data.Contains("RogueLegacy"))
        {
            var legacyData = (Godot.Collections.Dictionary)data["RogueLegacy"];
            _data.LegacyPoints = (int)legacyData.Get("legacyPoints", 0);
            _data.TotalLegacyPointsEarned = (int)legacyData.Get("totalLegacyPointsEarned", 0);
            _data.TotalLegacyPointsSpent = (int)legacyData.Get("totalLegacyPointsSpent", 0);
            _data.TotalDeaths = (int)legacyData.Get("totalDeaths", 0);
            _data.RunsCompleted = (int)legacyData.Get("runsCompleted", 0);
            _data.GoldInheritancePercent = (int)legacyData.Get("goldInheritancePercent", 25);
            _data.ExperienceInheritancePercent = (int)legacyData.Get("experienceInheritancePercent", 50);
            _data.ItemInheritancePercent = (int)legacyData.Get("itemInheritancePercent", 30);
            
            _goldInheritancePercent = _data.GoldInheritancePercent;
            _experienceInheritancePercent = _data.ExperienceInheritancePercent;
            _itemInheritancePercent = _data.ItemInheritancePercent;
            
            if (legacyData.Contains("unlockedUpgrades"))
            {
                var upgrades = (Godot.Collections.Array)legacyData["unlockedUpgrades"];
                _data.UnlockedUpgrades = new List<string>();
                foreach (string upgrade in upgrades)
                {
                    _data.UnlockedUpgrades.Add(upgrade);
                }
            }
            
            // 加载统计
            _data.HighestGoldInherited = (int)legacyData.Get("highestGoldInherited", 0);
            _data.HighestExpInherited = (int)legacyData.Get("highestExpInherited", 0);
            _data.LongestRun = (int)legacyData.Get("longestRun", 0);
            _data.BestFloor = (int)legacyData.Get("bestFloor", 0);
            
            // 加载属性加成
            _data.BonusAttack = (int)legacyData.Get("bonusAttack", 0);
            _data.BonusDefense = (int)legacyData.Get("bonusDefense", 0);
            _data.BonusHealth = (int)legacyData.Get("bonusHealth", 0);
            _data.BonusSpeed = (int)legacyData.Get("bonusSpeed", 0);
            _data.BonusCritical = (int)legacyData.Get("bonusCritical", 0);
        }
    }
    
    public void SaveData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem == null) return;
        
        var data = saveSystem.LoadGame();
        if (data == null) data = new Godot.Collections.Dictionary();
        
        var legacyData = new Godot.Collections.Dictionary
        {
            { "legacyPoints", _data.LegacyPoints },
            { "totalLegacyPointsEarned", _data.TotalLegacyPointsEarned },
            { "totalLegacyPointsSpent", _data.TotalLegacyPointsSpent },
            { "totalDeaths", _data.TotalDeaths },
            { "runsCompleted", _data.RunsCompleted },
            { "goldInheritancePercent", _goldInheritancePercent },
            { "experienceInheritancePercent", _experienceInheritancePercent },
            { "itemInheritancePercent", _itemInheritancePercent },
            { "unlockedUpgrades", new Godot.Collections.Array(_data.UnlockedUpgrades) },
            { "highestGoldInherited", _data.HighestGoldInherited },
            { "highestExpInherited", _data.HighestExpInherited },
            { "longestRun", _data.LongestRun },
            { "bestFloor", _data.BestFloor },
            { "bonusAttack", _data.BonusAttack },
            { "bonusDefense", _data.BonusDefense },
            { "bonusHealth", _data.BonusHealth },
            { "bonusSpeed", _data.BonusSpeed },
            { "bonusCritical", _data.BonusCritical }
        };
        
        data["RogueLegacy"] = legacyData;
        saveSystem.SaveGame(data);
    }
    
    // 开始新的盗贼传承运行
    public void StartRun()
    {
        _isRunActive = true;
        _currentFloor = 1;
        _currentGold = _startingGoldBonus;
        _currentExperience = 0;
        _enemiesDefeated = 0;
        _bossesDefeated = 0;
    }
    
    // 结束运行（死亡或完成）
    public void EndRun(bool completed)
    {
        if (!_isRunActive) return;
        
        _isRunActive = false;
        
        // 计算继承资源
        int goldInherited = (int)(_currentGold * _goldInheritancePercent / 100.0f);
        int expInherited = (int)(_currentExperience * _experienceInheritancePercent / 100.0f);
        
        // 计算传承点数
        int pointsEarned = _database.BasePointsPerRun;
        pointsEarned += (_currentFloor - 1) * _database.PointsPerFloor;
        pointsEarned += _bossesDefeated * _database.PointsPerBoss;
        if (completed) pointsEarned += _database.CompletionBonus;
        
        // 更新数据
        _data.LegacyPoints += pointsEarned;
        _data.TotalLegacyPointsEarned += pointsEarned;
        
        if (completed)
        {
            _data.RunsCompleted++;
        }
        else
        {
            _data.TotalDeaths++;
        }
        
        // 更新记录
        if (goldInherited > _data.HighestGoldInherited)
            _data.HighestGoldInherited = goldInherited;
        if (expInherited > _data.HighestExpInherited)
            _data.HighestExpInherited = expInherited;
        
        var runRecord = new LegacyRunRecord
        {
            RunNumber = _data.TotalDeaths + _data.RunsCompleted,
            FloorReached = _currentFloor,
            GoldEarned = _currentGold,
            ExperienceGained = _currentExperience,
            GoldInherited = goldInherited,
            ExpInherited = expInherited,
            LegacyPointsEarned = pointsEarned,
            Completed = completed,
            Timestamp = OS.GetUnixTime()
        };
        
        if (_data.RunHistory == null) _data.RunHistory = new List<LegacyRunRecord>();
        _data.RunHistory.Insert(0, runRecord);
        
        // 只保留最近20条记录
        if (_data.RunHistory.Count > 20)
            _data.RunHistory.RemoveAt(_data.RunHistory.Count - 1);
        
        // 更新最佳记录
        if (_currentFloor > _data.BestFloor)
            _data.BestFloor = _currentFloor;
        
        SaveData();
    }
    
    // 更新楼层进度
    public void UpdateFloor(int floor)
    {
        _currentFloor = floor;
        if (floor > _data.LongestRun)
            _data.LongestRun = floor;
    }
    
    // 添加金币
    public void AddGold(int amount)
    {
        _currentGold += amount;
    }
    
    // 添加经验
    public void AddExperience(int amount)
    {
        _currentExperience += amount;
    }
    
    // 击败敌人
    public void OnEnemyDefeated()
    {
        _enemiesDefeated++;
    }
    
    // 击败Boss
    public void OnBossDefeated()
    {
        _bossesDefeated++;
    }
    
    // 购买升级
    public bool PurchaseUpgrade(string upgradeId)
    {
        var upgrade = _database.GetUpgrade(upgradeId);
        if (upgrade == null) return false;
        
        int currentLevel = GetUpgradeLevel(upgradeId);
        if (currentLevel >= upgrade.MaxLevel) return false;
        
        int cost = _database.GetUpgradeCost(upgrade, currentLevel);
        if (_data.LegacyPoints < cost) return false;
        
        _data.LegacyPoints -= cost;
        _data.TotalLegacyPointsSpent += cost;
        
        if (_data.UnlockedUpgrades == null)
            _data.UnlockedUpgrades = new List<string>();
        
        _data.UnlockedUpgrades.Add(upgradeId);
        
        UpdateBonuses();
        SaveData();
        
        return true;
    }
    
    // 获取升级等级
    public int GetUpgradeLevel(string upgradeId)
    {
        if (_data.UnlockedUpgrades == null) return 0;
        
        int count = 0;
        foreach (var id in _data.UnlockedUpgrades)
        {
            if (id == upgradeId || id.StartsWith(upgradeId + "_"))
                count++;
        }
        
        // 简化的等级计算
        return _data.UnlockedUpgrades.Contains(upgradeId) ? 1 : 0;
    }
    
    // 更新属性加成
    private void UpdateBonuses()
    {
        _bonusAttack = 0;
        _bonusDefense = 0;
        _bonusHealth = 0;
        _bonusSpeed = 0;
        _bonusCritical = 0;
        
        if (_data.UnlockedUpgrades == null) return;
        
        foreach (var upgradeId in _data.UnlockedUpgrades)
        {
            var upgrade = _database.GetUpgrade(upgradeId);
            if (upgrade == null) continue;
            
            int level = GetUpgradeLevel(upgradeId);
            
            switch (upgrade.BonusType)
            {
                case "Attack":
                    _bonusAttack += (int)(upgrade.BonusValue * level);
                    _data.BonusAttack = _bonusAttack;
                    break;
                case "Defense":
                    _bonusDefense += (int)(upgrade.BonusValue * level);
                    _data.BonusDefense = _bonusDefense;
                    break;
                case "Health":
                    _bonusHealth += (int)(upgrade.BonusValue * level);
                    _data.BonusHealth = _bonusHealth;
                    break;
                case "Speed":
                    _bonusSpeed += (int)(upgrade.BonusValue * level);
                    _data.BonusSpeed = _bonusSpeed;
                    break;
                case "Critical":
                    _bonusCritical += (int)(upgrade.BonusValue * level);
                    _data.BonusCritical = _bonusCritical;
                    break;
                case "StartingGold":
                    _startingGoldBonus += (int)(upgrade.BonusValue * level);
                    break;
                case "EnemyScaling":
                    _enemyScalingModifier += upgrade.BonusValue * level;
                    break;
                case "Luck":
                    _luckBonus += (int)(upgrade.BonusValue * level);
                    break;
            }
        }
    }
    
    // 获取统计数据
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            { "TotalDeaths", _data.TotalDeaths },
            { "RunsCompleted", _data.RunsCompleted },
            { "LegacyPoints", _data.LegacyPoints },
            { "TotalPointsEarned", _data.TotalLegacyPointsEarned },
            { "TotalPointsSpent", _data.TotalLegacyPointsSpent },
            { "HighestGoldInherited", _data.HighestGoldInherited },
            { "HighestExpInherited", _data.HighestExpInherited },
            { "BestFloor", _data.BestFloor },
            { "LongestRun", _data.LongestRun }
        };
    }
    
    // 获取当前运行数据
    public Dictionary<string, object> GetCurrentRunData()
    {
        return new Dictionary<string, object>
        {
            { "IsActive", _isRunActive },
            { "CurrentFloor", _currentFloor },
            { "CurrentGold", _currentGold },
            { "CurrentExperience", _currentExperience },
            { "EnemiesDefeated", _enemiesDefeated },
            { "BossesDefeated", _bossesDefeated },
            { "GoldInheritancePercent", _goldInheritancePercent },
            { "ExperienceInheritancePercent", _experienceInheritancePercent }
        };
    }
    
    // 获取属性加成
    public Dictionary<string, int> GetAttributeBonuses()
    {
        return new Dictionary<string, int>
        {
            { "Attack", _bonusAttack },
            { "Defense", _bonusDefense },
            { "Health", _bonusHealth },
            { "Speed", _bonusSpeed },
            { "Critical", _bonusCritical }
        };
    }
    
    // 获取运行历史
    public List<LegacyRunRecord> GetRunHistory()
    {
        return _data.RunHistory ?? new List<LegacyRunRecord>();
    }
    
    // 获取所有升级
    public List<InheritanceUpgrade> GetAllUpgrades()
    {
        return _database.Upgrades;
    }
    
    // 获取分类升级
    public List<InheritanceUpgrade> GetUpgradesByCategory(string category)
    {
        return _database.GetUpgradesByCategory(category);
    }
    
    // 检查运行是否进行中
    public bool IsRunActive() => _isRunActive;
    
    // 获取敌人缩放修正
    public float GetEnemyScalingModifier() => _enemyScalingModifier;
    
    // 获取幸运加成
    public int GetLuckBonus() => _luckBonus;
}
