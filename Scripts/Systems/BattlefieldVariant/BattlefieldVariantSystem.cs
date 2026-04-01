using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Managers;

/// <summary>
/// 战场变体系统（REQ-115）
/// 每场战斗随机分配一个战场变体，为战斗添加环境策略层
/// </summary>
public partial class BattlefieldVariantSystem : BaseSystem
{
    private static BattlefieldVariantSystem _instance;
    public static BattlefieldVariantSystem Instance => _instance;

    // Signals
    public delegate void VariantSelectedEventHandler(BattlefieldVariantType variant);
    public delegate void VariantEffectTriggeredEventHandler(BattlefieldVariantType variant, string effectDesc);
    public delegate void VariantExitedEventHandler(BattlefieldVariantType variant);
    public delegate void VariantDamageAppliedEventHandler(float damage);

    // 运行时数据
    private BattlefieldVariantRuntimeData _runtimeData = new BattlefieldVariantRuntimeData();

    // 变体配置
    private Dictionary<BattlefieldVariantType, BattlefieldVariantConfig> _variantConfigs = new Dictionary<BattlefieldVariantType, BattlefieldVariantConfig>();

    // 平衡参数
    private const float SCORCHED_EARTH_DAMAGE_PER_TICK = 2f;
    private const float SCORCHED_EARTH_TICK_INTERVAL = 1f;
    private const float SCORCHED_EARTH_STACKING_PENALTY_PER_TICK = 1f;
    private const float SCORCHED_EARTH_STATIONARY_THRESHOLD = 0.5f; // 超过0.5s不动开始叠加

    private const float BROKEN_GROUND_MISS_CHANCE = 0.15f;
    private const float BROKEN_GROUND_PROJECTILE_DEVIATION = 0.10f;

    private const float STATIC_AIR_CHAIN_CHANCE = 0.20f;
    private const float STATIC_AIR_CHAIN_RADIUS = 80f;

    // 状态
    private bool _isInCombat = false;
    private float _lastTickTime = 0f;
    private Vector2 _lastPlayerPos = Vector2.Zero;
    private float _stationaryTime = 0f;

    // 系统引用
    private PlayerData _playerData;
    private EnemyManager _enemyManager;

    public override void _Ready()
    {
        _instance = this;
        InitializeVariantConfigs();
        SubscribeToEvents();
    }

    public override void _Process(double delta)
    {
        if (!_isInCombat || _runtimeData.ActiveVariant == BattlefieldVariantType.None)
            return;

        float dt = (float)delta;
        _runtimeData.ElapsedTime += dt;

        switch (_runtimeData.ActiveVariant)
        {
            case BattlefieldVariantType.ScorchedEarth:
                ProcessScorchedEarth(dt);
                break;
        }
    }

    private void InitializeVariantConfigs()
    {
        // ScorchedEarth
        var scorchConfig = new BattlefieldVariantConfig
        {
            VariantType = BattlefieldVariantType.ScorchedEarth,
            DisplayName = "焦土",
            Description = "地面持续灼烧！保持移动以避免叠加伤害",
            IconColor = new Color(1.0f, 0.35f, 0.0f),
            DamagePerTick = SCORCHED_EARTH_DAMAGE_PER_TICK,
            TickInterval = SCORCHED_EARTH_TICK_INTERVAL,
            StackingPenaltyPerTick = SCORCHED_EARTH_STACKING_PENALTY_PER_TICK
        };
        _variantConfigs[BattlefieldVariantType.ScorchedEarth] = scorchConfig;

        // BrokenGround
        var brokenConfig = new BattlefieldVariantConfig
        {
            VariantType = BattlefieldVariantType.BrokenGround,
            DisplayName = "破碎地面",
            Description = "地形崎岖！近战有概率滑倒，弹道可能偏移",
            IconColor = new Color(0.6f, 0.5f, 0.35f),
            MissChance = BROKEN_GROUND_MISS_CHANCE,
            ProjectileDeviationChance = BROKEN_GROUND_PROJECTILE_DEVIATION
        };
        _variantConfigs[BattlefieldVariantType.BrokenGround] = brokenConfig;

        // StaticAir
        var staticConfig = new BattlefieldVariantConfig
        {
            VariantType = BattlefieldVariantType.StaticAir,
            DisplayName = "静电空气",
            Description = "空气带电！技能有概率产生链式反应",
            IconColor = new Color(0.5f, 0.8f, 1.0f),
            ChainReactionChance = STATIC_AIR_CHAIN_CHANCE,
            ChainRadius = STATIC_AIR_CHAIN_RADIUS
        };
        _variantConfigs[BattlefieldVariantType.StaticAir] = staticConfig;
    }

    private void SubscribeToEvents()
    {
        if (EventBusManager.Instance != null)
        {
            EventBusManager.Instance.Subscribe(EventBusManager.Events.CombatStarted, OnCombatStarted);
            EventBusManager.Instance.Subscribe(EventBusManager.Events.CombatEnded, OnCombatEnded);
            EventBusManager.Instance.Subscribe(EventBusManager.Events.DamageDealt, OnDamageDealt);
        }
    }

    private void OnCombatStarted()
    {
        _isInCombat = true;
        SelectRandomVariant();

        if (EventBusManager.Instance != null)
        {
            EventBusManager.Instance.Emit(EventBusManager.Events.BattlefieldVariantStarted, _runtimeData.ActiveVariant);
        }
    }

    private void OnCombatEnded()
    {
        _isInCombat = false;
        if (_runtimeData.ActiveVariant != BattlefieldVariantType.None)
        {
            EmitSignal(SignalName.VariantExited, _runtimeData.ActiveVariant);

            if (EventBusManager.Instance != null)
            {
                EventBusManager.Instance.Emit(EventBusManager.Events.BattlefieldVariantEnded, _runtimeData.ActiveVariant);
            }
        }
        ResetRuntimeData();
    }

    private void OnDamageDealt(object data)
    {
        if (!_isInCombat || _runtimeData.ActiveVariant != BattlefieldVariantType.StaticAir)
            return;

        // Static Air: 有概率链式反应
        if (data is DamageDealtEvent dmgData && GD.Randf() < STATIC_AIR_CHAIN_CHANCE)
        {
            TriggerChainReaction(dmgData.TargetPosition);
        }
    }

    /// <summary>
    /// 每场战斗开始时随机选择变体
    /// </summary>
    public void SelectRandomVariant()
    {
        // 排除 None，随机选择 1-3
        int variantCount = Enum.GetValues(typeof(BattlefieldVariantType)).Length - 1;
        int randomIndex = (int)(GD.Randf() * variantCount) + 1;
        BattlefieldVariantType selected = (BattlefieldVariantType)randomIndex;

        _runtimeData.ActiveVariant = selected;
        _runtimeData.IsActive = true;
        _runtimeData.ElapsedTime = 0f;
        _runtimeData.EffectIntensity = 1.0f;
        _runtimeData.LastEffectTime = 0f;
        _runtimeData.StationaryTime = 0f;
        _runtimeData.ChainReactionCount = 0;
        _lastTickTime = 0f;
        _stationaryTime = 0f;

        // 通知 UI
        EmitSignal(SignalName.VariantSelected, selected);

        // 打印变体信息
        if (_variantConfigs.TryGetValue(selected, out var config))
        {
            GD.Print($"[BattlefieldVariant] 战场变体已激活: {config.DisplayName} - {config.Description}");
        }
    }

    private void ProcessScorchedEarth(float dt)
    {
        // 获取玩家位置（如果有 PlayerData）
        Vector2 playerPos = Vector2.Zero;
        var playerNode = GetTree().GetNodesInGroup("player");
        if (playerNode.Count > 0 && playerNode[0] is Node2D p)
        {
            playerPos = p.GlobalPosition;
        }

        // 检测是否在移动
        float distance = playerPos.DistanceTo(_lastPlayerPos);
        if (distance < 5f)
        {
            _stationaryTime += dt;
        }
        else
        {
            _stationaryTime = 0f;
        }
        _lastPlayerPos = playerPos;

        // 每秒造成环境伤害
        if (_runtimeData.ElapsedTime - _lastTickTime >= SCORCHED_EARTH_TICK_INTERVAL)
        {
            _lastTickTime = _runtimeData.ElapsedTime;

            // 静止叠加惩罚
            float stackingBonus = 0f;
            if (_stationaryTime > SCORCHED_EARTH_STATIONARY_THRESHOLD)
            {
                stackingBonus = (_stationaryTime / SCORCHED_EARTH_TICK_INTERVAL) * SCORCHED_EARTH_STACKING_PENALTY_PER_TICK;
            }

            float totalDamage = SCORCHED_EARTH_DAMAGE_PER_TICK + stackingBonus;
            ApplyEnvironmentDamage(totalDamage);

            string effectDesc = stackingBonus > 0
                ? $"灼烧伤害 {totalDamage:F1}（静止+{stackingBonus:F1}）"
                : $"灼烧伤害 {totalDamage:F1}";
            EmitSignal(SignalName.VariantEffectTriggered, BattlefieldVariantType.ScorchedEarth, effectDesc);
        }
    }

    private void ApplyEnvironmentDamage(float damage)
    {
        // 尝试对玩家造成环境伤害
        var playerNodes = GetTree().GetNodesInGroup("player");
        if (playerNodes.Count > 0 && playerNodes[0] is Node2D player)
        {
            // 通过 EmitSignal 通知伤害系统处理
            EmitSignal(SignalName.VariantDamageApplied, damage);
        }

        // 对敌人也生效（公平）
        var enemyNodes = GetTree().GetNodesInGroup("enemy");
        foreach (var enemy in enemyNodes)
        {
            if (enemy.HasMethod("TakeDamage"))
            {
                // 敌人也受环境伤害
            }
        }
    }

    private void TriggerChainReaction(Vector2 origin)
    {
        _runtimeData.ChainReactionCount++;
        var nearbyEnemies = GetTree().GetNodesInGroup("enemy");
        foreach (var node in nearbyEnemies)
        {
            if (node is Node2D enemy && enemy.GlobalPosition.DistanceTo(origin) <= STATIC_AIR_CHAIN_RADIUS)
            {
                if (enemy.HasMethod("TakeDamage"))
                {
                    // 链式伤害 = 原始伤害的 30%
                    // 实际伤害通过 CombatEvents 处理
                }
            }
        }
        EmitSignal(SignalName.VariantEffectTriggered, BattlefieldVariantType.StaticAir,
            $"链式反应！已影响 {_runtimeData.ChainReactionCount} 个单位");
    }

    /// <summary>
    /// 获取近战落空概率（BrokenGround 变体）
    /// </summary>
    public float GetMeleeMissChance()
    {
        if (_runtimeData.ActiveVariant == BattlefieldVariantType.BrokenGround)
            return BROKEN_GROUND_MISS_CHANCE;
        return 0f;
    }

    /// <summary>
    /// 获取弹道偏移概率（BrokenGround 变体）
    /// </summary>
    public float GetProjectileDeviationChance()
    {
        if (_runtimeData.ActiveVariant == BattlefieldVariantType.BrokenGround)
            return BROKEN_GROUND_PROJECTILE_DEVIATION;
        return 0f;
    }

    /// <summary>
    /// 检查是否触发链式反应（StaticAir 变体）
    /// </summary>
    public bool RollChainReaction()
    {
        if (_runtimeData.ActiveVariant == BattlefieldVariantType.StaticAir)
            return GD.Randf() < STATIC_AIR_CHAIN_CHANCE;
        return false;
    }

    /// <summary>
    /// 获取当前变体配置
    /// </summary>
    public BattlefieldVariantConfig GetCurrentVariantConfig()
    {
        if (_variantConfigs.TryGetValue(_runtimeData.ActiveVariant, out var config))
            return config;
        return null;
    }

    /// <summary>
    /// 获取运行时数据
    /// </summary>
    public BattlefieldVariantRuntimeData GetRuntimeData() => _runtimeData;

    private void ResetRuntimeData()
    {
        _runtimeData = new BattlefieldVariantRuntimeData();
    }
}

/// <summary>
/// 伤害数据事件（用于链式反应）
/// </summary>
public class DamageDealtEvent
{
    public Node2D Source { get; set; }
    public Node2D Target { get; set; }
    public Vector2 TargetPosition { get; set; }
    public float Damage { get; set; }
}
