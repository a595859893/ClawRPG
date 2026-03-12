using Godot;
using System;
using System.Collections.Generic;
using ElementalReactionData;

public class ElementalReactionSystem : Node
{
    private static ElementalReactionSystem _instance;
    public static ElementalReactionSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ElementalReactionSystem();
            }
            return _instance;
        }
    }

    // 玩家元素状态
    public PlayerElementalState PlayerState { get; private set; }

    // 敌人元素状态字典
    private Dictionary<int, EnemyElementalState> _enemyStates = new Dictionary<int, EnemyElementalState>();

    // 反应冷却字典
    private Dictionary<int, Dictionary<ReactionType, float>> _reactionCooldowns = new Dictionary<int, Dictionary<ReactionType, float>>();

    // 统计数据
    public int TotalReactionsTriggered { get; private set; }
    public float TotalReactionDamage { get; private set; }
    public Dictionary<ReactionType, int> ReactionCountByType { get; private set; } = new Dictionary<ReactionType, int>();

    // 信号
    [Signal]
    public signal void reaction_triggered(ReactionType type, ElementType elem1, ElementType elem2, float damage);
    [Signal]
    public signal void element_applied(NodeId node, ElementType type, float intensity);
    [Signal]
    public signal void stat_changed(NodeId node, string stat, float change);

    public override void _Ready()
    {
        _instance = this;
        PlayerState = new PlayerElementalState();

        // 初始化反应计数
        foreach (ReactionType rt in Enum.GetValues(typeof(ReactionType)))
        {
            ReactionCountByType[rt] = 0;
        }

        GD.Print("[ElementalReactionSystem] Initialized");
    }

    public override void _Process(float delta)
    {
        // 更新元素状态持续时间
        UpdateElementDurations(delta);

        // 更新反应冷却
        UpdateCooldowns(delta);
    }

    // 应用元素到目标
    public void ApplyElement(NodeId targetNode, ElementType element, float intensity)
    {
        if (!_enemyStates.ContainsKey(targetNode.InstanceId))
        {
            _enemyStates[targetNode.InstanceId] = new EnemyElementalState
            {
                Node = targetNode,
                AppliedElements = new Dictionary<ElementType, float>()
            };
        }

        var state = _enemyStates[targetNode.InstanceId];

        // 添加或更新元素
        if (state.AppliedElements.ContainsKey(element))
        {
            state.AppliedElements[element] = Mathf.Min(100f, state.AppliedElements[element] + intensity);
        }
        else
        {
            state.AppliedElements[element] = intensity;
        }

        // 检测反应
        CheckForReactions(targetNode, state);

        emit_signal("element_applied", targetNode, element, intensity);
    }

    // 检测反应
    private void CheckForReactions(NodeId targetNode, EnemyElementalState state)
    {
        if (state.AppliedElements.Count < 2)
            return;

        // 检查所有元素组合
        List<ElementType> elements = new List<ElementType>(state.AppliedElements.Keys);

        for (int i = 0; i < elements.Count; i++)
        {
            for (int j = i + 1; j < elements.Count; j++)
            {
                var elem1 = elements[i];
                var elem2 = elements[j];

                // 检查是否有反应
                var config = ElementalReactionDatabase.Instance.GetReaction(elem1, elem2);
                if (config == null)
                    continue;

                // 检查冷却
                if (IsOnCooldown(targetNode.InstanceId, config.Type, config.Cooldown))
                    continue;

                // 检查元素强度是否足够触发
                float requiredIntensity = 20f;
                if (state.AppliedElements[elem1] < requiredIntensity || 
                    state.AppliedElements[elem2] < requiredIntensity)
                    continue;

                // 触发反应
                TriggerReaction(targetNode, state, config, elem1, elem2);
            }
        }
    }

    // 触发反应
    private void TriggerReaction(NodeId targetNode, EnemyElementalState state, ReactionConfig config, ElementType elem1, ElementType elem2)
    {
        // 计算伤害
        float damage = config.BaseDamage * config.DamageMultiplier;

        // 应用玩家元素亲和加成
        float affinityBonus = 0f;
        if (PlayerState.ElementalAffinity.ContainsKey(elem1))
            affinityBonus += PlayerState.ElementalAffinity[elem1];
        if (PlayerState.ElementalAffinity.ContainsKey(elem2))
            affinityBonus += PlayerState.ElementalAffinity[elem2];

        damage *= (1f + affinityBonus);

        // 更新统计
        TotalReactionsTriggered++;
        TotalReactionDamage += damage;

        if (ReactionCountByType.ContainsKey(config.Type))
            ReactionCountByType[config.Type]++;
        else
            ReactionCountByType[config.Type] = 1;

        state.TotalDamageTaken += damage;
        if (state.ReactionsSuffered.ContainsKey(config.Type))
            state.ReactionsSuffered[config.Type]++;
        else
            state.ReactionsSuffered[config.Type] = 1;

        // 设置冷却
        SetCooldown(targetNode.InstanceId, config.Type, config.Cooldown);

        // 消耗元素
        state.AppliedElements[elem1] -= 30f;
        state.AppliedElements[elem2] -= 30f;

        // 清理强度为0的元素
        List<ElementType> toRemove = new List<ElementType>();
        foreach (var kvp in state.AppliedElements)
        {
            if (kvp.Value <= 0)
                toRemove.Add(kvp.Key);
        }
        foreach (var elem in toRemove)
        {
            state.AppliedElements.Remove(elem);
        }

        // 触发信号
        emit_signal("reaction_triggered", config.Type, elem1, elem2, damage);

        GD.Print($"[ElementalReaction] {config.Type} triggered! Damage: {damage:F1}");
    }

    // 设置反应冷却
    private void SetCooldown(int instanceId, ReactionType type, float cooldown)
    {
        if (!_reactionCooldowns.ContainsKey(instanceId))
        {
            _reactionCooldowns[instanceId] = new Dictionary<ReactionType, float>();
        }
        _reactionCooldowns[instanceId][type] = cooldown;
    }

    // 检查是否在冷却中
    private bool IsOnCooldown(int instanceId, ReactionType type, float cooldown)
    {
        if (!_reactionCooldowns.ContainsKey(instanceId))
            return false;

        if (!_reactionCooldowns[instanceId].ContainsKey(type))
            return false;

        return _reactionCooldowns[instanceId][type] > 0;
    }

    // 更新冷却
    private void UpdateCooldowns(float delta)
    {
        List<int> toRemoveNodes = new List<int>();

        foreach (var nodeKvp in _reactionCooldowns)
        {
            List<ReactionType> toRemove = new List<ReactionType>();

            foreach (var kvp in nodeKvp.Value)
            {
                _reactionCooldowns[nodeKvp.Key][kvp.Key] -= delta;
                if (_reactionCooldowns[nodeKvp.Key][kvp.Key] <= 0)
                    toRemove.Add(kvp.Key);
            }

            foreach (var rt in toRemove)
            {
                _reactionCooldowns[nodeKvp.Key].Remove(rt);
            }

            if (_reactionCooldowns[nodeKvp.Key].Count == 0)
                toRemoveNodes.Add(nodeKvp.Key);
        }

        foreach (var nodeId in toRemoveNodes)
        {
            _reactionCooldowns.Remove(nodeId);
        }
    }

    // 更新元素持续时间
    private void UpdateElementDurations(float delta)
    {
        // 玩家元素状态衰减
        if (PlayerState.ActiveElements != null)
        {
            List<ElementStatus> toRemove = new List<ElementStatus>();

            foreach (var status in PlayerState.ActiveElements)
            {
                status.Duration -= delta;
                if (status.Duration <= 0)
                    toRemove.Add(status);
            }

            foreach (var status in toRemove)
            {
                PlayerState.ActiveElements.Remove(status);
            }
        }

        // 敌人元素状态衰减
        foreach (var kvp in _enemyStates)
        {
            List<ElementType> toRemove = new List<ElementType>();
            foreach (var elemKvp in kvp.Value.AppliedElements)
            {
                // 元素随时间自然衰减
                kvp.Value.AppliedElements[elemKvp.Key] -= delta * 2f; // 每秒2点
                if (kvp.Value.AppliedElements[elemKvp.Key] <= 0)
                    toRemove.Add(elemKvp.Key);
            }

            foreach (var elem in toRemove)
            {
                kvp.Value.AppliedElements.Remove(elem);
            }
        }
    }

    // 增加元素亲和
    public void AddElementalAffinity(ElementType element, float bonus)
    {
        if (PlayerState.ElementalAffinity.ContainsKey(element))
        {
            PlayerState.ElementalAffinity[element] += bonus;
        }
        else
        {
            PlayerState.ElementalAffinity[element] = bonus;
        }

        GD.Print($"[ElementalReaction] {element} affinity increased by {bonus*100}%");
    }

    // 获取敌人当前元素状态
    public Dictionary<ElementType, float> GetEnemyElements(NodeId node)
    {
        if (_enemyStates.ContainsKey(node.InstanceId))
        {
            return _enemyStates[node.InstanceId].AppliedElements;
        }
        return new Dictionary<ElementType, float>();
    }

    // 清除敌人元素状态
    public void ClearEnemyElements(NodeId node)
    {
        if (_enemyStates.ContainsKey(node.InstanceId))
        {
            _enemyStates.Remove(node.InstanceId);
        }
    }

    // 获取反应统计
    public Dictionary<string, int> GetReactionStats()
    {
        var stats = new Dictionary<string, int>();
        stats["TotalReactions"] = TotalReactionsTriggered;
        stats["TotalDamage"] = (int)TotalReactionDamage;

        foreach (var kvp in ReactionCountByType)
        {
            stats[kvp.Key.ToString()] = kvp.Value;
        }

        return stats;
    }

    // 重置统计
    public void ResetStats()
    {
        TotalReactionsTriggered = 0;
        TotalReactionDamage = 0;
        ReactionCountByType.Clear();
        foreach (ReactionType rt in Enum.GetValues(typeof(ReactionType)))
        {
            ReactionCountByType[rt] = 0;
        }
        _enemyStates.Clear();
        _reactionCooldowns.Clear();
        PlayerState = new PlayerElementalState();
    }

    // 存档支持
    public Dictionary<string, object> GetSaveData()
    {
        var data = new Dictionary<string, object>();
        data["TotalReactionsTriggered"] = TotalReactionsTriggered;
        data["TotalReactionDamage"] = TotalReactionDamage;

        var reactionCounts = new Dictionary<string, int>();
        foreach (var kvp in ReactionCountByType)
        {
            reactionCounts[kvp.Key.ToString()] = kvp.Value;
        }
        data["ReactionCountByType"] = reactionCounts;

        return data;
    }

    public void LoadSaveData(Dictionary<string, object> data)
    {
        if (data.ContainsKey("TotalReactionsTriggered"))
            TotalReactionsTriggered = (int)data["TotalReactionsTriggered"];
        if (data.ContainsKey("TotalReactionDamage"))
            TotalReactionDamage = (float)data["TotalReactionDamage"];

        if (data.ContainsKey("ReactionCountByType"))
        {
            var reactionCounts = (Dictionary<string, object>)data["ReactionCountByType"];
            foreach (var kvp in reactionCounts)
            {
                ReactionType rt = (ReactionType)Enum.Parse(typeof(ReactionType), kvp.Key);
                ReactionCountByType[rt] = (int)kvp.Value;
            }
        }
    }
}
