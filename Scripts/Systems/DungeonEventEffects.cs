using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 地牢事件效果处理器。负责处理各类事件的具体效果实现。
/// </summary>
public partial class DungeonEventEffects
{
    private RandomDungeonEventData _data;
    private Random _rand = new Random();
    
    public DungeonEventEffects(RandomDungeonEventData data)
    {
        _data = data;
    }
    
    public void SetData(RandomDungeonEventData data)
    {
        _data = data;
    }
    
    public Dictionary<string, object> ProcessCombatEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        int enemyCount = eventData.ContainsKey("enemy_count") ? Convert.ToInt32(eventData["enemy_count"]) : 1;
        float difficulty = eventData.ContainsKey("difficulty") ? Convert.ToSingle(eventData["difficulty"]) : 1.0f;
        int goldReward = eventData.ContainsKey("reward_gold") ? Convert.ToInt32(eventData["reward_gold"]) : 0;
        int expReward = eventData.ContainsKey("reward_exp") ? Convert.ToInt32(eventData["reward_exp"]) : 0;
        
        // Calculate actual rewards based on difficulty
        goldReward = (int)(goldReward * difficulty);
        expReward = (int)(expReward * difficulty);
        
        _data.GoldGainedFromEvents += goldReward;
        _data.ExpGainedFromEvents += expReward;
        
        result["combat"] = true;
        result["enemy_count"] = enemyCount;
        result["difficulty"] = difficulty;
        result["gold_reward"] = goldReward;
        result["exp_reward"] = expReward;
        result["message"] = $"Combat encounter! {enemyCount} enemies (difficulty {difficulty:F1}x).";
        
        _data.EnemiesDefeatedInRoom = 0; // Reset for combat
        
        return result;
    }
    
    public Dictionary<string, object> ProcessTreasureEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        int goldMin = eventData.ContainsKey("gold_min") ? Convert.ToInt32(eventData["gold_min"]) : 0;
        int goldMax = eventData.ContainsKey("gold_max") ? Convert.ToInt32(eventData["gold_max"]) : 10;
        int gold = _rand.Next(goldMin, goldMax + 1);
        bool hasItem = eventData.ContainsKey("item_chance") && (float)_rand.NextDouble() < Convert.ToSingle(eventData["item_chance"]);
        
        _data.GoldGainedFromEvents += gold;
        _data.HasTreasure = true;
        
        result["gold_found"] = gold;
        result["has_item"] = hasItem;
        
        if (hasItem)
        {
            _data.ItemsGained++;
            result["item_rarity"] = GetRandomItemRarity();
        }
        
        string message = gold > 0 ? $"Found {gold} gold!" : "Found nothing...";
        if (hasItem) message += " Also found an item!";
        
        result["message"] = message;
        result["success"] = true;
        
        return result;
    }
    
    public Dictionary<string, object> ProcessHealingEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        int healAmount = eventData.ContainsKey("heal_amount") ? Convert.ToInt32(eventData["heal_amount"]) : 20;
        
        _data.PlayerHealth = Math.Min(_data.PlayerHealth + healAmount, 100);
        _data.IsInjured = _data.PlayerHealth < 50;
        _data.IsFullHealth = _data.PlayerHealth >= 100;
        
        result["healed"] = healAmount;
        result["current_health"] = _data.PlayerHealth;
        result["message"] = $"Restored {healAmount} health!";
        result["success"] = true;
        
        return result;
    }
    
    public Dictionary<string, object> ProcessBuffEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        string buffId = "buff_" + Guid.NewGuid().ToString().Substring(0, 8);
        int duration = eventData.ContainsKey("buff_duration") ? Convert.ToInt32(eventData["buff_duration"]) : 60;
        
        _data.AppliedBuffs.Add(buffId);
        
        result["buff_id"] = buffId;
        result["duration"] = duration;
        
        if (eventData.ContainsKey("attack_bonus"))
            result["attack_bonus"] = eventData["attack_bonus"];
        if (eventData.ContainsKey("defense_bonus"))
            result["defense_bonus"] = eventData["defense_bonus"];
        if (eventData.ContainsKey("gold_multiplier"))
            result["gold_multiplier"] = eventData["gold_multiplier"];
            
        result["message"] = "You received a blessing!";
        result["success"] = true;
        
        return result;
    }
    
    public Dictionary<string, object> ProcessDebuffEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        string debuffId = "debuff_" + Guid.NewGuid().ToString().Substring(0, 8);
        int duration = eventData.ContainsKey("debuff_duration") ? Convert.ToInt32(eventData["debuff_duration"]) : 60;
        
        _data.AppliedDebuffs.Add(debuffId);
        
        result["debuff_id"] = debuffId;
        result["duration"] = duration;
        
        if (eventData.ContainsKey("attack_penalty"))
            result["attack_penalty"] = eventData["attack_penalty"];
        if (eventData.ContainsKey("defense_penalty"))
            result["defense_penalty"] = eventData["defense_penalty"];
            
        result["message"] = "You are cursed!";
        result["success"] = true;
        
        return result;
    }
    
    public Dictionary<string, object> ProcessPoisonEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        int damage = eventData.ContainsKey("damage") ? Convert.ToInt32(eventData["damage"]) : 10;
        int dotDuration = eventData.ContainsKey("dot_duration") ? Convert.ToInt32(eventData["dot_duration"]) : 10;
        
        _data.PlayerHealth = Math.Max(_data.PlayerHealth - damage, 0);
        _data.IsInjured = _data.PlayerHealth < 50;
        
        result["immediate_damage"] = damage;
        result["dot_duration"] = dotDuration;
        result["current_health"] = _data.PlayerHealth;
        result["message"] = $"Poison deals {damage} damage!";
        result["success"] = true;
        
        return result;
    }
    
    public Dictionary<string, object> ProcessDamageEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        int damageMin = eventData.ContainsKey("damage_min") ? Convert.ToInt32(eventData["damage_min"]) : 5;
        int damageMax = eventData.ContainsKey("damage_max") ? Convert.ToInt32(eventData["damage_max"]) : 15;
        int damage = _rand.Next(damageMin, damageMax + 1);
        
        _data.PlayerHealth = Math.Max(_data.PlayerHealth - damage, 0);
        _data.IsInjured = _data.PlayerHealth < 50;
        
        result["damage"] = damage;
        result["current_health"] = _data.PlayerHealth;
        result["message"] = $"Trap deals {damage} damage!";
        result["success"] = true;
        
        return result;
    }
    
    public Dictionary<string, object> ProcessChoiceEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        string choiceA = eventData.ContainsKey("choice_a") ? eventData["choice_a"].ToString() : "Option A";
        string choiceB = eventData.ContainsKey("choice_b") ? eventData["choice_b"].ToString() : "Option B";
        
        result["choice_a"] = choiceA;
        result["choice_b"] = choiceB;
        result["requires_choice"] = true;
        result["message"] = $"A choice appears: {choiceA} or {choiceB}?";
        
        return result;
    }
    
    public Dictionary<string, object> ProcessRewardEvent(Dictionary eventData)
    {
        var result = new Dictionary<string, object>();
        
        int gold = eventData.ContainsKey("gold") ? Convert.ToInt32(eventData["gold"]) : 0;
        int exp = eventData.ContainsKey("exp") ? Convert.ToInt32(eventData["exp"]) : 0;
        
        _data.GoldGainedFromEvents += gold;
        _data.ExpGainedFromEvents += exp;
        
        result["gold_reward"] = gold;
        result["exp_reward"] = exp;
        
        if (eventData.ContainsKey("item_rarity"))
        {
            result["item_rarity"] = eventData["item_rarity"].ToString();
            _data.ItemsGained++;
        }
        
        string message = "";
        if (gold > 0) message += $" +{gold} gold";
        if (exp > 0) message += $" +{exp} exp";
        
        result["message"] = message.Length > 0 ? message : "You received a reward!";
        result["success"] = true;
        
        return result;
    }
    
    public string GetRandomItemRarity()
    {
        float roll = (float)_rand.NextDouble() * 100f;
        
        if (roll < 50) return "Common";
        if (roll < 80) return "Uncommon";
        if (roll < 95) return "Rare";
        if (roll < 99) return "Epic";
        return "Legendary";
    }
}
