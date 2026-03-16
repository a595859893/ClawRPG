using System;
using System.Collections.Generic;
using Godot;

public class PlayerTalentSystem : BaseSystem
{
    private static PlayerTalentSystem _instance;
    public static PlayerTalentSystem Instance
    {
        get
        {
            if (_instance == null) _instance = new PlayerTalentSystem();
            return _instance;
        }
    }
    
    public PlayerTalentData.PlayerTalentData PlayerData { get; private set; }
    
    // 天赋点数获取事件
    public Action<int> OnPointsChanged;
    public Action<string> OnTalentUnlocked;
    public Action<PlayerTalentData.TalentTree> OnTreeUnlocked;
    
    private int _availablePoints = 0;
    public int AvailablePoints
    {
        get => _availablePoints;
        private set
        {
            if (_availablePoints != value)
            {
                _availablePoints = value;
                OnPointsChanged?.Invoke(value);
            }
        }
    }
    
    public PlayerTalentSystem()
    {
        PlayerData = new PlayerTalentData.PlayerTalentData();
        InitializeDefaults();
    }
    
    private void InitializeDefaults()
    {
        // 初始每个系给1点
        foreach (PlayerTalentData.TalentTree tree in Enum.GetValues(typeof(PlayerTalentData.TalentTree)))
        {
            PlayerData.UnlockedTrees[tree] = 1;  // 初始解锁Tier 1
            PlayerData.TreePoints[tree] = 0;
        }
        AvailablePoints = 3;  // 初始3点可分配
    }
    
    public bool CanUnlockTalent(string talentId)
    {
        var talent = PlayerTalentDatabase.Instance.GetTalent(talentId);
        if (talent == null) return false;
        
        // 检查是否已解锁
        if (PlayerData.UnlockedTalents.Contains(talentId)) return false;
        
        // 检查前置天赋
        foreach (var req in talent.Requires)
        {
            if (!PlayerData.UnlockedTalents.Contains(req)) return false;
        }
        
        // 检查是否有足够点数
        if (PlayerData.TreePoints[talent.Tree] + talent.Cost > GetTreeMaxPoints(talent.Tree))
            return false;
        
        return true;
    }
    
    public bool UnlockTalent(string talentId)
    {
        if (!CanUnlockTalent(talentId)) return false;
        
        var talent = PlayerTalentDatabase.Instance.GetTalent(talentId);
        if (talent == null) return false;
        
        // 解锁天赋
        PlayerData.UnlockedTalents.Add(talentId);
        PlayerData.TreePoints[talent.Tree] += talent.Cost;
        PlayerData.TotalPointsSpent += talent.Cost;
        
        // 更新解锁的Tier
        if (talent.Tier > GetUnlockedTier(talent.Tree))
        {
            PlayerData.UnlockedTrees[talent.Tree] = talent.Tier;
            OnTreeUnlocked?.Invoke(talent.Tree);
        }
        
        OnTalentUnlocked?.Invoke(talentId);
        
        // 应用天赋效果到玩家
        ApplyTalentBonuses(talent);
        
        return true;
    }
    
    public void AddTalentPoints(int points)
    {
        AvailablePoints += points;
    }
    
    private void ApplyTalentBonuses(PlayerTalentData.TalentNode talent)
    {
        var player = GetPlayer();
        if (player == null) return;
        
        // 应用属性加成
        foreach (var bonus in talent.Bonuses)
        {
            switch (bonus.Key)
            {
                case "attack_flat":
                    player.TalentAttackBonus += bonus.Value;
                    break;
                case "attack_percent":
                    player.TalentAttackPercent += bonus.Value;
                    break;
                case "defense_flat":
                    player.TalentDefenseBonus += bonus.Value;
                    break;
                case "defense_percent":
                    player.TalentDefensePercent += bonus.Value;
                    break;
                case "health_flat":
                    player.TalentHealthBonus += bonus.Value;
                    break;
                case "health_percent":
                    player.TalentHealthPercent += bonus.Value;
                    break;
                case "move_speed":
                    player.TalentMoveSpeed += bonus.Value;
                    break;
                case "attack_speed":
                    player.TalentAttackSpeed += bonus.Value;
                    break;
                case "crit_rate":
                    player.TalentCritRate += bonus.Value;
                    break;
                case "crit_damage":
                    player.TalentCritDamage += bonus.Value;
                    break;
                case "dodge":
                    player.TalentDodge += bonus.Value;
                    break;
                case "lifesteal":
                    player.TalentLifeSteal += bonus.Value;
                    break;
                case "exp_bonus":
                    player.TalentExpBonus += bonus.Value;
                    break;
                case "gold_bonus":
                    player.TalentGoldBonus += bonus.Value;
                    break;
                case "drop_rate":
                    player.TalentDropRate += bonus.Value;
                    break;
                case "rare_drop":
                    player.TalentRareDrop += bonus.Value;
                    break;
                case "health_regen":
                    player.TalentHealthRegen += bonus.Value;
                    break;
                case "sell_price":
                    player.TalentSellPrice += bonus.Value;
                    break;
                case "enhance_success":
                    player.TalentEnhanceSuccess += bonus.Value;
                    break;
            }
        }
        
        player.UpdateTotalAttributes();
    }
    
    private Player GetPlayer()
    {
        var main = GetTree().CurrentScene as Main;
        return main?.GetPlayer();
    }
    
    private Node GetTree()
    {
        return Engine.GetMainLoop() as Node;
    }
    
    public int GetUnlockedTier(PlayerTalentData.TalentTree tree)
    {
        return PlayerData.UnlockedTrees.ContainsKey(tree) ? PlayerData.UnlockedTrees[tree] : 0;
    }
    
    public int GetTreeMaxPoints(PlayerTalentData.TalentTree tree)
    {
        int tier = GetUnlockedTier(tree);
        return tier * 5;  // 每层5点
    }
    
    public int GetTreeSpentPoints(PlayerTalentData.TalentTree tree)
    {
        return PlayerData.TreePoints.ContainsKey(tree) ? PlayerData.TreePoints[tree] : 0;
    }
    
    public bool CanUnlockTree(PlayerTalentData.TalentTree tree)
    {
        return false;  // 暂时不需要升级系统
    }
    
    public List<PlayerTalentData.TalentNode> GetAvailableTalents(PlayerTalentData.TalentTree tree)
    {
        List<PlayerTalentData.TalentNode> available = new List<PlayerTalentData.TalentNode>();
        foreach (var talent in PlayerTalentDatabase.Instance.GetTalentsByTree(tree))
        {
            if (CanUnlockTalent(talent.Id))
                available.Add(talent);
        }
        return available;
    }
    
    public Dictionary<string, object> GetSaveData()
    {
        Dictionary<string, object> data = new Dictionary<string, object>();
        data["unlocked_talents"] = new List<string>(PlayerData.UnlockedTalents);
        data["total_points_spent"] = PlayerData.TotalPointsSpent;
        
        Dictionary<string, int> treePoints = new Dictionary<string, int>();
        foreach (var kvp in PlayerData.TreePoints)
            treePoints[kvp.Key.ToString()] = kvp.Value;
        data["tree_points"] = treePoints;
        
        Dictionary<string, int> unlockedTrees = new Dictionary<string, int>();
        foreach (var kvp in PlayerData.UnlockedTrees)
            unlockedTrees[kvp.Key.ToString()] = kvp.Value;
        data["unlocked_trees"] = unlockedTrees;
        
        return data;
    }
    
    public void LoadSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("unlocked_talents"))
        {
            PlayerData.UnlockedTalents = new HashSet<string>((List<string>)data["unlocked_talents"]);
        }
        
        if (data.ContainsKey("total_points_spent"))
        {
            PlayerData.TotalPointsSpent = (int)data["total_points_spent"];
        }
        
        if (data.ContainsKey("tree_points"))
        {
            var treePoints = (Dictionary<string, object>)data["tree_points"];
            foreach (var kvp in treePoints)
            {
                PlayerTalentData.TalentTree tree = Enum.Parse<PlayerTalentData.TalentTree>(kvp.Key);
                PlayerData.TreePoints[tree] = (int)kvp.Value;
            }
        }
        
        if (data.ContainsKey("unlocked_trees"))
        {
            var unlockedTrees = (Dictionary<string, object>)data["unlocked_trees"];
            foreach (var kvp in unlockedTrees)
            {
                PlayerTalentData.TalentTree tree = Enum.Parse<PlayerTalentData.TalentTree>(kvp.Key);
                PlayerData.UnlockedTrees[tree] = (int)kvp.Value;
            }
        }
        
        // 重新应用所有已解锁天赋
        ReapplyAllTalents();
    }
    
    private void ReapplyAllTalents()
    {
        var player = GetPlayer();
        if (player == null) return;
        
        // 重置天赋加成
        player.TalentAttackBonus = 0;
        player.TalentAttackPercent = 0;
        player.TalentDefenseBonus = 0;
        player.TalentDefensePercent = 0;
        player.TalentHealthBonus = 0;
        player.TalentHealthPercent = 0;
        player.TalentMoveSpeed = 0;
        player.TalentAttackSpeed = 0;
        player.TalentCritRate = 0;
        player.TalentCritDamage = 0;
        player.TalentDodge = 0;
        player.TalentLifeSteal = 0;
        player.TalentExpBonus = 0;
        player.TalentGoldBonus = 0;
        player.TalentDropRate = 0;
        player.TalentRareDrop = 0;
        player.TalentHealthRegen = 0;
        player.TalentSellPrice = 0;
        player.TalentEnhanceSuccess = 0;
        
        // 重新应用所有已解锁天赋
        foreach (string talentId in PlayerData.UnlockedTalents)
        {
            var talent = PlayerTalentDatabase.Instance.GetTalent(talentId);
            if (talent != null)
            {
                ApplyTalentBonuses(talent);
            }
        }
    }

    /// <summary>
    /// 导出保存数据（供 SaveSystem 调用）
    /// </summary>
    public Dictionary ExportSaveData()
    {
        return GetSaveData();
    }

    /// <summary>
    /// 导入保存数据（供 SaveSystem 调用）
    /// </summary>
    public void ImportSaveData(Dictionary data)
    {
        LoadSaveData(data);
    }
}
