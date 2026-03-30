using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.Pets;
using ClawRPG.Scripts.Systems.Pets.AI;

namespace ClawRPG.Systems.Pets.AI
{
    /// <summary>
    /// 宠物战术 AI 系统 (REQ-112)
    /// 职责：战术模式切换、自动技能施放、Readable Failure 日志
    /// 接入点：PetDecisionSystem + PetSkillSystem + CombatEvents
    /// </summary>
    public partial class PetTacticalAI : BaseSystem
    {
        private static PetTacticalAI _instance;
        public static PetTacticalAI Instance => _instance ??= new PetTacticalAI();

        // ===== 战术模式 =====
        
        /// <summary>
        /// 三种战术模式：跟随/保护/进攻
        /// </summary>
        public enum PetTacticalMode
        {
            Follow,   // 跟随：宠物跟随玩家，躲避危险，保持安全距离
            Protect,  // 保护：宠物优先保护玩家，吸引仇恨，嘲讽敌人
            Attack    // 进攻：宠物主动进攻当前敌人
        }

        // ===== 战术状态 =====
        
        /// <summary>
        /// 宠物战术状态快照（用于持久化）
        /// </summary>
        public struct PetTacticalState
        {
            public PetTacticalMode CurrentMode;
            public float ModeSwitchCooldown;   // 模式切换冷却，防止抖动
            public float SkillCheckTimer;       // 技能检查计时器
            public float DecisionLogTimer;      // 决策日志输出计时器（Readable Failure）
            public string LastDecisionReason;   // 上次决策原因描述
        }

        // ===== 配置 =====
        
        private float _modeSwitchCooldownDuration = 2f;  // 模式切换冷却 2 秒
        private float _skillCheckInterval = 0.5f;        // 技能检查间隔 0.5 秒
        private float _decisionLogInterval = 3f;         // 决策日志每 3 秒
        private float _engageRange = 150f;              // 进入战斗的距离阈值
        private float _retreatHealthPercent = 0.2f;     // 血量低于 20% 进入保护模式

        // ===== 状态 =====
        
        private PetTacticalState _tacticalState;
        private Pet _activePet;
        private CharacterBody2D _player;
        private Node2D _currentTarget;
        private List<Node2D> _nearbyEnemies = new List<Node2D>();
        
        // ===== 事件驱动状态 =====
        
        private bool _isInCombat = false;  // 事件驱动标记，非轮询
        private bool _pendingSkillCheck = false;  // 事件触发技能检查
        
        // ===== 子系统引用 =====
        
        private PetDecisionSystem _decisionSystem;
        private PetBehaviorTree _behaviorTree;
        private PetSkillSystem _skillSystem;
        
        // ===== 信号 =====
        
        public Action<PetTacticalMode, PetTacticalMode> OnTacticalModeChanged;
        public Action<string> OnTacticalDecision;  // 决策原因（Readable Failure）

        // ===== 生命周期 =====

        public override void _Ready()
        {
            Instance = this;
            _tacticalState = new PetTacticalState
            {
                CurrentMode = PetTacticalMode.Follow,
                ModeSwitchCooldown = 0f,
                SkillCheckTimer = 0f,
                DecisionLogTimer = 0f,
                LastDecisionReason = "System initialized"
            };
            GD.Print("[PetTacticalAI] Initialized");
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(Pet pet, CharacterBody2D player)
        {
            _activePet = pet;
            _player = player;
            
            // 尝试获取子系统引用
            _decisionSystem = PetDecisionSystem.Instance;
            _behaviorTree = PetBehaviorTree.Instance;
            _skillSystem = PetSkillSystem.Instance;
            
            // 订阅事件总线（REQ-112-05: 事件驱动集成）
            SubscribeToEventBus();
            
            GD.Print($"[PetTacticalAI] Initialized for pet: {pet?.PetName ?? "null"}");
        }

        /// <summary>
        /// 主更新循环
        /// </summary>
        public override void _Process(double delta)
        {
            float deltaF = (float)delta;
            
            // 更新冷却计时器
            UpdateTimers(deltaF);
            
            // 更新战术模式
            UpdateTacticalMode(deltaF);
            
            // 自动技能施放
            UpdateSkillCasting(deltaF);
            
            // 输出决策日志（Readable Failure）
            UpdateDecisionLog(deltaF);
        }

        // ===== 公开接口 =====

        /// <summary>
        /// 切换战术模式（玩家主动触发）
        /// </summary>
        public void SetTacticalMode(PetTacticalMode mode)
        {
            if (_tacticalState.ModeSwitchCooldown > 0)
            {
                GD.Print($"[PetTacticalAI] Mode switch blocked by cooldown: {_tacticalState.ModeSwitchCooldown:F1}s remaining");
                return;
            }
            
            if (_tacticalState.CurrentMode == mode)
                return;
            
            var oldMode = _tacticalState.CurrentMode;
            _tacticalState.CurrentMode = mode;
            _tacticalState.ModeSwitchCooldown = _modeSwitchCooldownDuration;
            _tacticalState.LastDecisionReason = $"Mode switched from {oldMode} to {mode} by player";
            
            GD.Print($"[PetTacticalAI] Mode changed: {oldMode} -> {mode}");
            OnTacticalModeChanged?.Invoke(oldMode, mode);
        }

        /// <summary>
        /// 获取当前战术模式
        /// </summary>
        public PetTacticalMode GetCurrentMode() => _tacticalState.CurrentMode;

        /// <summary>
        /// 更新附近敌人列表（由检测系统调用）
        /// </summary>
        public void UpdateNearbyEnemies(List<Node2D> enemies)
        {
            _nearbyEnemies = enemies ?? new List<Node2D>();
        }

        /// <summary>
        /// 设置当前攻击目标
        /// </summary>
        public void SetTarget(Node2D target)
        {
            _currentTarget = target;
        }

        // ===== 事件驱动集成 (REQ-112-05) =====
        
        /// <summary>
        /// 订阅 EventBusManager 事件，替代部分 _Process 轮询
        /// </summary>
        private void SubscribeToEventBus()
        {
            var eventBus = EventBusManager.Instance;
            if (eventBus == null)
            {
                GD.Warn("[PetTacticalAI] EventBusManager not available, event-driven integration disabled");
                return;
            }
            
            eventBus.Subscribe(EventBusManager.Events.CombatStarted, OnCombatStarted);
            eventBus.Subscribe(EventBusManager.Events.CombatEnded, OnCombatEnded);
            eventBus.Subscribe<EnemySpawnedEventData>(EventBusManager.Events.EnemySpawned, OnEnemySpawned);
            eventBus.Subscribe<EnemyDiedEventData>(EventBusManager.Events.EnemyDied, OnEnemyDied);
            
            GD.Print("[PetTacticalAI] Subscribed to EventBusManager events (CombatStarted, CombatEnded, EnemySpawned, EnemyDied)");
        }
        
        /// <summary>
        /// 战斗开始事件处理
        /// </summary>
        private void OnCombatStarted()
        {
            _isInCombat = true;
            _tacticalState.LastDecisionReason = "Combat started via event";
            GD.Print("[PetTacticalAI] Combat started (event-driven)");
        }
        
        /// <summary>
        /// 战斗结束事件处理
        /// </summary>
        private void OnCombatEnded()
        {
            _isInCombat = false;
            _nearbyEnemies.Clear();
            _tacticalState.LastDecisionReason = "Combat ended via event";
            _tacticalState.CurrentMode = PetTacticalMode.Follow;
            GD.Print("[PetTacticalAI] Combat ended (event-driven)");
        }
        
        /// <summary>
        /// 敌人生成事件处理
        /// </summary>
        private void OnEnemySpawned(EnemySpawnedEventData data)
        {
            if (data?.Enemy != null)
            {
                _isInCombat = true;
                
                // 如果敌人不在列表中，添加
                if (!_nearbyEnemies.Contains(data.Enemy))
                {
                    _nearbyEnemies.Add(data.Enemy);
                    _pendingSkillCheck = true;  // 触发立即技能检查
                    GD.Print($"[PetTacticalAI] Enemy spawned and added to tracking: {data.EnemyType}, total enemies: {_nearbyEnemies.Count}");
                }
            }
        }
        
        /// <summary>
        /// 敌人死亡事件处理
        /// </summary>
        private void OnEnemyDied(EnemyDiedEventData data)
        {
            if (data?.Enemy != null)
            {
                _nearbyEnemies.Remove(data.Enemy);
                _pendingSkillCheck = true;
                
                if (_nearbyEnemies.Count == 0)
                {
                    // 触发战斗结束检查（延迟，由 CombatEnded 事件最终确认）
                    _tacticalState.LastDecisionReason = $"Enemy {data.EnemyType} killed, {_nearbyEnemies.Count} enemies remaining";
                }
                else
                {
                    _tacticalState.LastDecisionReason = $"Enemy {data.EnemyType} killed, {_nearbyEnemies.Count} enemies remaining";
                }
                
                GD.Print($"[PetTacticalAI] Enemy removed from tracking: {data.EnemyType}, remaining: {_nearbyEnemies.Count}");
            }
        }
        
        // ===== 内部更新 =====

        private void UpdateTimers(float delta)
        {
            if (_tacticalState.ModeSwitchCooldown > 0)
                _tacticalState.ModeSwitchCooldown -= delta;
            
            _tacticalState.SkillCheckTimer += delta;
            _tacticalState.DecisionLogTimer += delta;
        }

        /// <summary>
        /// 战术模式决策
        /// </summary>
        private void UpdateTacticalMode(float delta)
        {
            if (_activePet == null || _player == null)
                return;

            // 玩家血量检查（保护模式触发）
            bool playerLowHealth = IsPlayerLowHealth();
            
            // 宠物血量检查（撤退判断）
            bool petLowHealth = IsPetLowHealth();
            
            // 敌人检测
            bool hasEnemies = _nearbyEnemies.Count > 0;
            
            // 根据当前模式 + 战场情况决定是否需要切换
            PetTacticalMode suggestedMode = SuggestMode(playerLowHealth, petLowHealth, hasEnemies);
            
            if (suggestedMode != _tacticalState.CurrentMode && _tacticalState.ModeSwitchCooldown <= 0)
            {
                SetTacticalMode(suggestedMode);
            }
        }

        /// <summary>
        /// 建议最优战术模式
        /// </summary>
        private PetTacticalMode SuggestMode(bool playerLowHealth, bool petLowHealth, bool hasEnemies)
        {
            // 保护优先级最高：宠物血量低 或 玩家血量低
            if (petLowHealth || playerLowHealth)
            {
                _tacticalState.LastDecisionReason = $"{(petLowHealth ? "Pet" : "Player")} health critical, switching to Protect";
                return PetTacticalMode.Protect;
            }
            
            // Follow 模式：无敌人时保持跟随
            if (!hasEnemies)
            {
                _tacticalState.LastDecisionReason = "No enemies nearby, staying in Follow mode";
                return PetTacticalMode.Follow;
            }
            
            // 有敌人时根据模式决定
            switch (_tacticalState.CurrentMode)
            {
                case PetTacticalMode.Follow:
                    // Follow 遇到敌人默认切换 Attack
                    _tacticalState.LastDecisionReason = "Enemy detected in Follow mode, engaging Attack";
                    return PetTacticalMode.Attack;
                    
                case PetTacticalMode.Protect:
                    // Protect 遇到敌人保持保护姿态
                    _tacticalState.LastDecisionReason = "Protecting player from enemy threat";
                    return PetTacticalMode.Protect;
                    
                case PetTacticalMode.Attack:
                    // Attack 敌人消失后回退 Follow
                    if (!hasEnemies)
                    {
                        _tacticalState.LastDecisionReason = "Target eliminated, returning to Follow";
                        return PetTacticalMode.Follow;
                    }
                    _tacticalState.LastDecisionReason = "Maintaining Attack on current target";
                    return PetTacticalMode.Attack;
                    
                default:
                    return PetTacticalMode.Follow;
            }
        }

        /// <summary>
        /// 自动技能施放
        /// </summary>
        private void UpdateSkillCasting(float delta)
        {
            if (_skillSystem == null || _activePet == null)
                return;
            
            // 按间隔检查，除非有事件触发的待检查标记
            if (!_pendingSkillCheck && _tacticalState.SkillCheckTimer < _skillCheckInterval)
                return;
            _tacticalState.SkillCheckTimer = 0f;
            _pendingSkillCheck = false;
            
            if (_activePet.Id == null)
                return;
            
            // 获取已学会的技能
            var skills = _skillSystem.GetLearnedSkills(_activePet.Id);
            if (skills.Count == 0)
                return;
            
            // 选择最优可用技能
            foreach (var skill in skills)
            {
                if (_skillSystem.CanUseSkill(_activePet.Id, skill.SkillId))
                {
                    // 技能可用，检查是否符合当前战术模式
                    if (ShouldUseSkill(skill))
                    {
                        ExecuteSkill(skill);
                        break;  // 每帧最多施放一个技能
                    }
                }
            }
        }

        /// <summary>
        /// 判断是否应使用技能
        /// </summary>
        private bool ShouldUseSkill(PetSkill skill)
        {
            switch (_tacticalState.CurrentMode)
            {
                case PetTacticalMode.Attack:
                    // Attack 模式优先使用伤害技能
                    return skill.SkillType == "damage" || skill.SkillType == "debuff";
                    
                case PetTacticalMode.Protect:
                    // Protect 模式优先使用护盾/治疗技能
                    return skill.SkillType == "heal" || skill.SkillType == "shield" || skill.SkillType == "buff";
                    
                case PetTacticalMode.Follow:
                    // Follow 模式：只有紧急情况才用技能
                    return IsPetLowHealth() || IsPlayerLowHealth();
                    
                default:
                    return false;
            }
        }

        /// <summary>
        /// 执行技能
        /// </summary>
        private void ExecuteSkill(PetSkill skill)
        {
            if (_activePet?.Id == null) return;
            
            _skillSystem.UseSkill(_activePet.Id, skill.SkillId);
            
            string reason = $"[{_tacticalState.CurrentMode}] Used {skill.SkillName} ({skill.SkillType})";
            _tacticalState.LastDecisionReason = reason;
            GD.Print($"[PetTacticalAI] {reason}");
            OnTacticalDecision?.Invoke(reason);
        }

        /// <summary>
        /// 决策日志输出（Readable Failure）
        /// </summary>
        private void UpdateDecisionLog(float delta)
        {
            if (_tacticalState.DecisionLogTimer < _decisionLogInterval)
                return;
            _tacticalState.DecisionLogTimer = 0f;
            
            string log = $"[PetTacticalAI Decision] Mode={_tacticalState.CurrentMode} | Reason={_tacticalState.LastDecisionReason} | " +
                        $"Enemies={_nearbyEnemies.Count} | PetHP={GetPetHealthPercent():P0} | PlayerHP={GetPlayerHealthPercent():P0}";
            GD.Print(log);
        }

        // ===== 辅助方法 =====

        private bool IsPlayerLowHealth()
        {
            if (_player == null) return false;
            if (!_player.HasMethod("GetCurrentHealth") || !_player.HasMethod("GetMaxHealth"))
                return false;
            
            int hp = (int)_player.Call("GetCurrentHealth");
            int maxHp = (int)_player.Call("GetMaxHealth");
            return maxHp > 0 && (float)hp / maxHp < 0.3f;
        }

        private bool IsPetLowHealth()
        {
            if (_activePet == null) return false;
            float hpPercent = (float)_activePet.CurrentHealth / Mathf.Max(1, _activePet.MaxHealth);
            return hpPercent < _retreatHealthPercent;
        }

        private float GetPetHealthPercent()
        {
            if (_activePet == null) return 1f;
            return (float)_activePet.CurrentHealth / Mathf.Max(1, _activePet.MaxHealth);
        }

        private float GetPlayerHealthPercent()
        {
            if (_player == null || !_player.HasMethod("GetCurrentHealth") || !_player.HasMethod("GetMaxHealth"))
                return 1f;
            int hp = (int)_player.Call("GetCurrentHealth");
            int maxHp = (int)_player.Call("GetMaxHealth");
            return maxHp > 0 ? (float)hp / maxHp : 1f;
        }

        // ===== 持久化 =====

        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary
            {
                { "tacticalMode", (int)_tacticalState.CurrentMode },
                { "modeSwitchCooldown", _tacticalState.ModeSwitchCooldown }
            };
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.Contains("tacticalMode"))
            {
                var mode = (PetTacticalMode)(int)data["tacticalMode"];
                _tacticalState.CurrentMode = mode;
            }
            if (data.Contains("modeSwitchCooldown"))
                _tacticalState.ModeSwitchCooldown = Convert.ToSingle(data["modeSwitchCooldown"]);
        }
    }
}
