using Godot;
using System;
using System.Collections.Generic;

public class SkillSynergySystem : Node
{
    public static SkillSynergySystem Instance { get; private set; }

    private SkillSynergyData _data;
    private SkillSynergyDatabase _database;

    // 当前技能序列（用于检测协同）
    private List<string> _currentSkillSequence = new List<string>();
    private float _sequenceTimer = 0f;
    private float _sequenceTimeout = 3f;  // 3秒内必须使用下一个技能

    // 连击链
    private int _currentComboChain = 0;

    // 信号
    [Signal] public void SynergyTriggered(string synergyId, string synergyName, string message);
    [Signal] public void SynergyExpired(string synergyId);
    [Signal] public void SynergyStackChanged(string synergyId, int newStacks);

    public override void _Ready()
    {
        Instance = this;
        _database = SkillSynergyDatabase.Instance;
        _data = new SkillSynergyData();
        LoadData();
    }

    public override void _Process(float delta)
    {
        // 更新序列超时
        if (_sequenceTimer > 0)
        {
            _sequenceTimer -= delta;
            if (_sequenceTimer <= 0)
            {
                _currentSkillSequence.Clear();
                _currentComboChain = 0;
            }
        }

        // 更新活跃协同效果持续时间
        var expiredSynergies = new List<string>();
        foreach (var kvp in _data.ActiveSynergies)
        {
            kvp.Value.CurrentDuration -= delta;
            if (kvp.Value.CurrentDuration <= 0)
            {
                expiredSynergies.Add(kvp.Key);
            }
        }

        foreach (var synergyId in expiredSynergies)
        {
            _data.ActiveSynergies.Remove(synergyId);
            EmitSignal(nameof(SynergyExpired), synergyId);
        }
    }

    // 使用技能时调用此方法
    public void OnSkillUsed(string skillId)
    {
        _currentSkillSequence.Add(skillId);
        _sequenceTimer = _sequenceTimeout;

        // 检查是否有匹配的协同
        CheckSynergyTrigger();

        // 更新连击链
        _currentComboChain++;
        if (_currentComboChain > _data.MaxComboChain)
        {
            _data.MaxComboChain = _currentComboChain;
        }
    }

    private void CheckSynergyTrigger()
    {
        var sequenceStr = string.Join(",", _currentSkillSequence);
        
        foreach (var kvp in _database.Synergies)
        {
            var synergy = kvp.Value;
            var requiredStr = string.Join(",", synergy.RequiredSkills);

            // 检查当前序列是否以所需技能序列结尾
            if (sequenceStr.EndsWith(requiredStr) || sequenceStr.Contains(requiredStr))
            {
                // 检查冷却
                if (IsSynergyOnCooldown(synergy.Id))
                    continue;

                // 触发协同
                TriggerSynergy(synergy);

                // 清空序列（协同触发后清空）
                _currentSkillSequence.Clear();
                _currentComboChain = 0;
                break;
            }
        }
    }

    private Dictionary<string, float> _cooldowns = new Dictionary<string, float>();

    private bool IsSynergyOnCooldown(string synergyId)
    {
        if (_cooldowns.ContainsKey(synergyId))
            return _cooldowns[synergyId] > 0;
        return false;
    }

    private void TriggerSynergy(SkillSynergyDatabase.SynergyConfig synergy)
    {
        // 设置冷却
        _cooldowns[synergy.Id] = synergy.Cooldown;

        // 检查并更新活跃协同
        SkillSynergyData.SynergyRecord record;
        if (_data.ActiveSynergies.ContainsKey(synergy.Id))
        {
            record = _data.ActiveSynergies[synergy.Id];
            if (record.TriggerCount < synergy.MaxStacks)
            {
                record.TriggerCount++;
                record.CurrentDuration = synergy.Duration;
            }
            else
            {
                // 已达最大层数，只更新持续时间
                record.CurrentDuration = synergy.Duration;
            }
            EmitSignal(nameof(SynergyStackChanged), synergy.Id, record.TriggerCount);
        }
        else
        {
            // 新协同
            record = new SkillSynergyData.SynergyRecord
            {
                SynergyId = synergy.Id,
                SynergyName = synergy.Name,
                TriggerCount = 1,
                MaxStacks = synergy.MaxStacks,
                Duration = synergy.Duration,
                CurrentDuration = synergy.Duration
            };
            _data.ActiveSynergies[synergy.Id] = record;

            // 发现新协同
            if (!_data.SynergyUnlockProgress.ContainsKey(synergy.Id))
            {
                _data.SynergyUnlockProgress[synergy.Id] = 0;
                _data.UniqueSynergiesDiscovered++;
            }
        }

        // 更新统计
        _data.TotalSynergiesTriggered++;
        if (_data.SynergyTriggerHistory.ContainsKey(synergy.Id))
            _data.SynergyTriggerHistory[synergy.Id]++;
        else
            _data.SynergyTriggerHistory[synergy.Id] = 1;

        // 更新解锁进度
        if (_data.SynergyUnlockProgress.ContainsKey(synergy.Id))
            _data.SynergyUnlockProgress[synergy.Id]++;

        // 记录伤害/治疗加成
        if (synergy.DamageMultiplier > 1f)
            _data.TotalBonusDamage += (synergy.DamageMultiplier - 1f) * 100f;
        if (synergy.HealMultiplier > 1f)
            _data.TotalBonusHealing += (synergy.HealMultiplier - 1f) * 100f;

        // 发送信号
        EmitSignal(nameof(SynergyTriggered), synergy.Id, synergy.Name, synergy.TriggerMessage);

        // 触发屏幕效果（如果有）
        TriggerSynergyEffects(synergy);

        // 保存数据
        SaveData();
    }

    private void TriggerSynergyEffects(SkillSynergyDatabase.SynergyConfig synergy)
    {
        // 根据稀有度触发屏幕效果
        var intensity = 0.3f;
        switch (synergy.Rarity)
        {
            case SkillSynergyDatabase.SynergyRarity.Common:
                intensity = 0.2f;
                break;
            case SkillSynergyDatabase.SynergyRarity.Uncommon:
                intensity = 0.3f;
                break;
            case SkillSynergyDatabase.SynergyRarity.Rare:
                intensity = 0.5f;
                break;
            case SkillSynergyDatabase.SynergyRarity.Epic:
                intensity = 0.7f;
                break;
            case SkillSynergyDatabase.SynergyRarity.Legendary:
                intensity = 1.0f;
                break;
        }

        // 触发屏幕闪金（暴击效果）
        if (synergy.DamageMultiplier > 1.5f || synergy.Rarity >= SkillSynergyDatabase.SynergyRarity.Epic)
        {
            // 简单的屏幕闪烁效果
            // ScreenEffectSystem.Instance?.TriggerCritFlash(intensity, synergy.Duration);
        }

        // 触发屏幕震动
        if (synergy.DamageMultiplier > 2.0f)
        {
            // ScreenEffectSystem.Instance?.TriggerShake(intensity, 0.5f);
        }
    }

    // 更新冷却
    public void _ProcessCooldowns(float delta)
    {
        var expiredCooldowns = new List<string>();
        foreach (var kvp in _cooldowns)
        {
            if (kvp.Value > 0)
            {
                _cooldowns[kvp.Key] = kvp.Value - delta;
                if (_cooldowns[kvp.Key] <= 0)
                {
                    expiredCooldowns.Add(kvp.Key);
                }
            }
        }
        foreach (var id in expiredCooldowns)
        {
            _cooldowns.Remove(id);
        }
    }

    // 获取活跃协同
    public Dictionary<string, SkillSynergyData.SynergyRecord> GetActiveSynergies()
    {
        return _data.ActiveSynergies;
    }

    // 获取协同统计
    public Dictionary<string, int> GetSynergyStats()
    {
        return _data.SynergyTriggerHistory;
    }

    // 获取协同加成
    public Dictionary<string, float> GetCurrentStatBonuses()
    {
        var bonuses = new Dictionary<string, float>();

        foreach (var kvp in _data.ActiveSynergies)
        {
            var synergy = _database.GetSynergy(kvp.Key);
            if (synergy == null) continue;

            var stackMultiplier = (float)kvp.Value.TriggerCount / synergy.MaxStacks;

            if (synergy.StatBonuses != null)
            {
                foreach (var statKvp in synergy.StatBonuses)
                {
                    if (bonuses.ContainsKey(statKvp.Key))
                        bonuses[statKvp.Key] += statKvp.Value * stackMultiplier;
                    else
                        bonuses[statKvp.Key] = statKvp.Value * stackMultiplier;
                }
            }

            // 暴击加成
            if (synergy.CriticalChanceBonus > 0)
            {
                if (bonuses.ContainsKey("critical_chance"))
                    bonuses["critical_chance"] += synergy.CriticalChanceBonus * stackMultiplier;
                else
                    bonuses["critical_chance"] = synergy.CriticalChanceBonus * stackMultiplier;
            }

            if (synergy.CriticalDamageBonus > 0)
            {
                if (bonuses.ContainsKey("critical_damage"))
                    bonuses["critical_damage"] += synergy.CriticalDamageBonus * stackMultiplier;
                else
                    bonuses["critical_damage"] = synergy.CriticalDamageBonus * stackMultiplier;
            }
        }

        return bonuses;
    }

    // 获取伤害乘数
    public float GetDamageMultiplier()
    {
        float multiplier = 1f;

        foreach (var kvp in _data.ActiveSynergies)
        {
            var synergy = _database.GetSynergy(kvp.Key);
            if (synergy == null) continue;

            var stackMultiplier = (float)kvp.Value.TriggerCount / synergy.MaxStacks;
            if (synergy.DamageMultiplier > 1f)
            {
                multiplier += (synergy.DamageMultiplier - 1f) * stackMultiplier;
            }
        }

        return multiplier;
    }

    // 获取治疗乘数
    public float GetHealMultiplier()
    {
        float multiplier = 1f;

        foreach (var kvp in _data.ActiveSynergies)
        {
            var synergy = _database.GetSynergy(kvp.Key);
            if (synergy == null) continue;

            var stackMultiplier = (float)kvp.Value.TriggerCount / synergy.MaxStacks;
            if (synergy.HealMultiplier > 1f)
            {
                multiplier += (synergy.HealMultiplier - 1f) * stackMultiplier;
            }
        }

        return multiplier;
    }

    // 获取资源消耗减少
    public float GetResourceCostReduction()
    {
        float reduction = 0f;

        foreach (var kvp in _data.ActiveSynergies)
        {
            var synergy = _database.GetSynergy(kvp.Key);
            if (synergy == null) continue;

            var stackMultiplier = (float)kvp.Value.TriggerCount / synergy.MaxStacks;
            if (synergy.ResourceCostReduction > 0)
            {
                reduction += synergy.ResourceCostReduction * stackMultiplier;
            }
        }

        return reduction;
    }

    // 获取冷却减少
    public float GetCooldownReduction()
    {
        float reduction = 0f;

        foreach (var kvp in _data.ActiveSynergies)
        {
            var synergy = _database.GetSynergy(kvp.Key);
            if (synergy == null) continue;

            var stackMultiplier = (float)kvp.Value.TriggerCount / synergy.MaxStacks;
            if (synergy.CooldownReduction > 0)
            {
                reduction += synergy.CooldownReduction * stackMultiplier;
            }
        }

        return reduction;
    }

    // 获取协同详情
    public SkillSynergyDatabase.SynergyConfig GetSynergyDetails(string synergyId)
    {
        return _database.GetSynergy(synergyId);
    }

    // 获取当前连击链
    public int GetCurrentComboChain()
    {
        return _currentComboChain;
    }

    // 获取所有协同
    public Dictionary<string, SkillSynergyDatabase.SynergyConfig> GetAllSynergies()
    {
        return _database.Synergies;
    }

    // 获取解锁进度
    public int GetUnlockProgress(string synergyId)
    {
        if (_data.SynergyUnlockProgress.ContainsKey(synergyId))
            return _data.SynergyUnlockProgress[synergyId];
        return 0;
    }

    // 获取统计数据
    public SkillSynergyData GetStatistics()
    {
        return _data;
    }

    // 手动触发测试协同
    public void TestSynergy(string synergyId)
    {
        var synergy = _database.GetSynergy(synergyId);
        if (synergy != null)
        {
            TriggerSynergy(synergy);
        }
    }

    // 强制清除序列
    public void ClearSequence()
    {
        _currentSkillSequence.Clear();
        _sequenceTimer = 0;
        _currentComboChain = 0;
    }

    // 保存数据
    private void SaveData()
    {
        var saveSystem = GetNode("/root/SaveSystem");
        if (saveSystem != null)
        {
            // SaveSystem.SaveGameData("skill_synergy", _data.Serialize());
        }
    }

    // 加载数据
    private void LoadData()
    {
        var saveSystem = GetNode("/root/SaveSystem");
        if (saveSystem != null)
        {
            // var data = SaveSystem.LoadGameData("skill_synergy");
            // if (data != null)
            //     _data.Deserialize(data);
        }
    }

    // 通知 UI 更新
    public void NotifyUIUpdate()
    {
        // 通知 UI 刷新
    }
}
