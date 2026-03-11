using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems;

/// <summary>
/// 遗物系统管理器
/// </summary>
public class RelicSystem : Node
{
    public static RelicSystem Instance { get; private set; }
    
    private PlayerRelicData _playerData = new();
    
    // 信号
    [Signal] public delegate void RelicPurchased(string relicId);
    [Signal] public delegate void RelicEquipped(string relicId);
    [Signal] public delegate void RelicUnequipped(string relicId);
    [Signal] public delegate void RelicSlotUnlocked(int newSlotCount);
    
    public override void _Ready()
    {
        Instance = this;
        RelicDatabase.Initialize();
        GD.Print("[RelicSystem] Initialized");
    }
    
    public void LoadData(PlayerRelicData data)
    {
        if (data != null)
        {
            _playerData = data;
        }
    }
    
    public PlayerRelicData GetData()
    {
        return _playerData;
    }
    
    #region 遗物获取
    
    /// <summary>
    /// 购买遗物
    /// </summary>
    public bool PurchaseRelic(string relicId)
    {
        var relic = RelicDatabase.GetRelic(relicId);
        if (relic == null)
        {
            GD.PrintErr($"[RelicSystem] Relic not found: {relicId}");
            return false;
        }
        
        if (_playerData.OwnedRelicIds.Contains(relicId))
        {
            GD.Print($"[RelicSystem] Already owned: {relicId}");
            return false;
        }
        
        var player = GetTree().CurrentScene.GetNode<Player>("Player");
        if (player == null)
        {
            GD.PrintErr("[RelicSystem] Player not found");
            return false;
        }
        
        if (player.Gold < relic.Price)
        {
            GD.Print($"[RelicSystem] Not enough gold: {player.Gold} < {relic.Price}");
            return false;
        }
        
        player.Gold -= relic.Price;
        _playerData.OwnedRelicIds.Add(relicId);
        
        EmitSignal(nameof(RelicPurchased), relicId);
        GD.Print($"[RelicSystem] Purchased relic: {relic.Name} for {relic.Price} gold");
        return true;
    }
    
    /// <summary>
    /// 随机获取遗物（作为奖励）
    /// </summary>
    public RelicData GrantRandomRelic(RelicRarity rarity)
    {
        var relic = RelicDatabase.GetRandomRelic(rarity);
        if (relic == null) return null;
        
        if (!_playerData.OwnedRelicIds.Contains(relic.Id))
        {
            _playerData.OwnedRelicIds.Add(relic.Id);
            GD.Print($"[RelicSystem] Granted random relic: {relic.Name} ({rarity})");
        }
        return relic;
    }
    
    /// <summary>
    /// 检查是否拥有遗物
    /// </summary>
    public bool HasRelic(string relicId)
    {
        return _playerData.OwnedRelicIds.Contains(relicId);
    }
    
    #endregion
    
    #region 遗物装备
    
    /// <summary>
    /// 装备遗物
    /// </summary>
    public bool EquipRelic(string relicId)
    {
        if (!_playerData.OwnedRelicIds.Contains(relicId))
        {
            GD.Print($"[RelicSystem] Cannot equip - not owned: {relicId}");
            return false;
        }
        
        if (_playerData.EquippedRelicIds.Contains(relicId))
        {
            GD.Print($"[RelicSystem] Already equipped: {relicId}");
            return false;
        }
        
        if (_playerData.EquippedRelicIds.Count >= _playerData.MaxRelicSlots)
        {
            GD.Print($"[RelicSystem] Max relic slots reached: {_playerData.MaxRelicSlots}");
            return false;
        }
        
        _playerData.EquippedRelicIds.Add(relicId);
        ApplyRelicEffects();
        
        EmitSignal(nameof(RelicEquipped), relicId);
        GD.Print($"[RelicSystem] Equipped relic: {relicId}");
        return true;
    }
    
    /// <summary>
    /// 卸下遗物
    /// </summary>
    public bool UnequipRelic(string relicId)
    {
        if (!_playerData.EquippedRelicIds.Contains(relicId))
        {
            GD.Print($"[RelicSystem] Not equipped: {relicId}");
            return false;
        }
        
        _playerData.EquippedRelicIds.Remove(relicId);
        ApplyRelicEffects();
        
        EmitSignal(nameof(RelicUnequipped), relicId);
        GD.Print($"[RelicSystem] Unequipped relic: {relicId}");
        return true;
    }
    
    /// <summary>
    /// 解锁遗物槽位
    /// </summary>
    public bool UnlockRelicSlot(int cost)
    {
        var player = GetTree().CurrentScene.GetNode<Player>("Player");
        if (player == null)
        {
            GD.PrintErr("[RelicSystem] Player not found");
            return false;
        }
        
        if (player.Gold < cost)
        {
            GD.Print($"[RelicSystem] Not enough gold for slot unlock: {player.Gold} < {cost}");
            return false;
        }
        
        player.Gold -= cost;
        _playerData.MaxRelicSlots += 1;
        
        EmitSignal(nameof(RelicSlotUnlocked), _playerData.MaxRelicSlots);
        GD.Print($"[RelicSystem] Unlocked relic slot: {_playerData.MaxRelicSlots}");
        return true;
    }
    
    #endregion
    
    #region 效果计算
    
    /// <summary>
    /// 获取已装备遗物的属性加成
    /// </summary>
    public Dictionary<string, float> GetEquippedRelicBonuses()
    {
        var bonuses = new Dictionary<string, float>();
        
        foreach (var relicId in _playerData.EquippedRelicIds)
        {
            var relic = RelicDatabase.GetRelic(relicId);
            if (relic == null) continue;
            
            foreach (var bonus in relic.AttributeBonuses)
            {
                if (bonuses.ContainsKey(bonus.Key))
                    bonuses[bonus.Key] += bonus.Value;
                else
                    bonuses[bonus.Key] = bonus.Value;
            }
        }
        
        return bonuses;
    }
    
    /// <summary>
    /// 获取已装备遗物的特殊效果
    /// </summary>
    public List<string> GetEquippedSpecialEffects()
    {
        var effects = new List<string>();
        
        foreach (var relicId in _playerData.EquippedRelicIds)
        {
            var relic = RelicDatabase.GetRelic(relicId);
            if (relic == null || string.IsNullOrEmpty(relic.SpecialEffect)) continue;
            
            effects.Add(relic.SpecialEffect);
        }
        
        return effects;
    }
    
    /// <summary>
    /// 检查是否拥有特定效果
    /// </summary>
    public bool HasSpecialEffect(string effect)
    {
        return GetEquippedSpecialEffects().Contains(effect);
    }
    
    /// <summary>
    /// 应用遗物效果到玩家
    /// </summary>
    private void ApplyRelicEffects()
    {
        var player = GetTree().CurrentScene.GetNode<Player>("Player");
        if (player == null) return;
        
        // 重置属性加成
        player.RelicAttackBonus = 0;
        player.RelicDefenseBonus = 0;
        player.RelicHealthBonus = 0;
        player.RelicSpeedBonus = 0;
        player.RelicCritRateBonus = 0;
        player.RelicCritDamageBonus = 0;
        player.RelicLifestealBonus = 0;
        player.RelicDodgeBonus = 0;
        
        // 应用属性加成
        var bonuses = GetEquippedRelicBonuses();
        
        if (bonuses.TryGetValue("attack", out var atk))
            player.RelicAttackBonus = atk;
        if (bonuses.TryGetValue("defense", out var def))
            player.RelicDefenseBonus = def;
        if (bonuses.TryGetValue("health", out var hp))
            player.RelicHealthBonus = hp;
        if (bonuses.TryGetValue("speed", out var spd))
            player.RelicSpeedBonus = spd;
        if (bonuses.TryGetValue("crit_rate", out var cr))
            player.RelicCritRateBonus = cr;
        if (bonuses.TryGetValue("crit_damage", out var cd))
            player.RelicCritDamageBonus = cd;
        if (bonuses.TryGetValue("lifesteal", out var ls))
            player.RelicLifestealBonus = ls;
        if (bonuses.TryGetValue("dodge", out var dod))
            player.RelicDodgeBonus = dod;
        
        // 应用特殊效果（如果有）
        var effects = GetEquippedSpecialEffects();
        
        // 可以在这里添加更多特殊效果处理
        
        GD.Print($"[RelicSystem] Applied {bonuses.Count} attribute bonuses and {effects.Count} special effects");
    }
    
    #endregion
    
    #region 获取信息
    
    public List<RelicData> GetOwnedRelics()
    {
        var result = new List<RelicData>();
        foreach (var id in _playerData.OwnedRelicIds)
        {
            var relic = RelicDatabase.GetRelic(id);
            if (relic != null) result.Add(relic);
        }
        return result;
    }
    
    public List<RelicData> GetEquippedRelics()
    {
        var result = new List<RelicData>();
        foreach (var id in _playerData.EquippedRelicIds)
        {
            var relic = RelicDatabase.GetRelic(id);
            if (relic != null) result.Add(relic);
        }
        return result;
    }
    
    public int GetMaxRelicSlots() => _playerData.MaxRelicSlots;
    public int GetCurrentEquippedCount() => _playerData.EquippedRelicIds.Count;
    
    #endregion
    
    #region 存档支持
    
    public Dictionary<string, object> Save()
    {
        return new Dictionary<string, object>
        {
            { "owned_relics", _playerData.OwnedRelicIds },
            { "equipped_relics", _playerData.EquippedRelicIds },
            { "max_slots", _playerData.MaxRelicSlots }
        };
    }
    
    public void Load(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.TryGetValue("owned_relics", out var owned))
            _playerData.OwnedRelicIds = new List<string>((System.Collections.IEnumerable)owned);
        
        if (data.TryGetValue("equipped_relics", out var equipped))
            _playerData.EquippedRelicIds = new List<string>((System.Collections.IEnumerable)equipped);
        
        if (data.TryGetValue("max_slots", out var slots))
            _playerData.MaxRelicSlots = (int)slots;
        
        ApplyRelicEffects();
    }
    
    #endregion
}
