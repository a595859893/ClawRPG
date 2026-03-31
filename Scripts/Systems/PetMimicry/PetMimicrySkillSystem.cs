using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Characters;
using ClawRPG.Scripts.Managers;
using ClawRPG.Scripts.Systems.Pets;
using ClawRPG.Scripts.Systems.ProceduralDungeon;

namespace ClawRPG.Scripts.Systems.PetMimicry
{
    /// <summary>
    /// 宠物模仿技能系统 — 根据当前环境触发已习得的模仿技能
    ///
    /// 职责：
    /// 1. 在 _Process 中检测当前环境并选择最佳技能
    /// 2. 管理技能实例（冷却/充能）
    /// 3. 执行技能效果（伤害/buff/控制）
    /// 4. 与 PetCombatAI 集成（使用宠物当前目标）
    /// 5. 发射信号通知 UI/战斗系统
    ///
    /// Partial class 拆分：
    /// - PetMimicrySkillSystem.cs          : 生命周期 + 技能实例管理 + ThinkAndAct + Public API
    /// - PetMimicrySkillSystem.SkillLogic.cs : 技能执行 + 效果系统 + 目标获取 + VFX
    /// - PetMimicrySkillSystem.Trigger.cs  : 触发评估 + 互斥组 + 环境检测
    /// </summary>
    public partial class PetMimicrySkillSystem : Node
    {
        public static PetMimicrySkillSystem Instance { get; private set; }

        // ── Dependencies ──────────────────────────────────────────────────────
        private MimicryDatabase _database;
        private PetMimicryData _mimicryData;
        private RoomEnvironmentType _lastEnvironment = RoomEnvironmentType.None;

        // ── Skill State ─────────────────────────────────────────────────────
        /// <summary>所有已解锁技能实例（按行为类型索引）</summary>
        private Dictionary<PlayerBehaviorType, MimicrySkillInstance> _skillInstances = new Dictionary<PlayerBehaviorType, MimicrySkillInstance>();

        /// <summary>当前正在生效的被动技能效果（类型→剩余持续时间）</summary>
        private Dictionary<MimicrySkillType, float> _activeSkillEffects = new Dictionary<MimicrySkillType, float>();

        // ── Config ──────────────────────────────────────────────────────────
        private const float SKILL_THINK_INTERVAL = 0.5f;   // 每0.5秒检查一次是否要放技能
        private const float ENEMY_SCAN_RANGE = 200f;         // 敌人扫描范围
        private const int   MAX_SKILL_TARGETS = 3;          // AOE最大目标数
        private float _thinkAccumulator = 0f;

        // REQ-146: 互斥组管理 — 记录当前激活的互斥组，防止同组技能重复触发
        private HashSet<string> _activeMutexGroups = new HashSet<string>();

        // REQ-146: 最后一次主人受伤时间（用于 OnOwnerDamaged 触发检测）
        private float _lastOwnerDamageTime = float.MinValue;
        private const float OWNER_DAMAGE_COOLDOWN = 3f; // 主人受伤后3秒内的触发窗口

        // REQ-146: 主人攻击冷却（用于 OnOwnerAttacking 触发）
        private float _ownerAttackCooldown = 0f;
        private const float OWNER_ATTACK_COOLDOWN = 1f; // 主人攻击后1秒窗口

        // ── Signals (Godot 4 [Signal] delegate pattern) ───────────────────────
        [Signal]
        public delegate void MimicrySkillUsedEventHandler(PlayerBehaviorType skillType, MimicrySkillType mimSkill, Vector2 worldPosition);
        [Signal]
        public delegate void MimicrySkillLearnedEventHandler(PlayerBehaviorType behavior, int newLevel);
        [Signal]
        public delegate void MimicrySkillReadyEventHandler(PlayerBehaviorType skillType, bool isReady);

        public override void _Ready()
        {
            Instance = this;
            _ProcessDeferred();
        }

        private async void _ProcessDeferred()
        {
            await ToSignal(GetTree(), "IdleFrame");
            _database = MimicryDatabase.Instance;
            _mimicryData = PetMimicryData.Instance;

            if (_database == null)
            {
                GD.PrintErr("[PetMimicrySkillSystem] MimicryDatabase not found!");
                return;
            }
            if (_mimicryData == null)
            {
                GD.PrintErr("[PetMimicrySkillSystem] PetMimicryData not found!");
                return;
            }

            SubscribeToEvents();
            RefreshSkillInstances();
        }

        private void SubscribeToEvents()
        {
            // PetCombatAI 战斗状态信号
            if (PetCombatAI.Instance != null)
            {
                // PetCombatAI signals are subscribed here
            }

            // 宠物协战攻击信号
            if (PetCombatCompanionSystem.Instance != null)
            {
                PetCombatCompanionSystem.Instance.PetSynergyAttackTriggered += OnPetSynergyAttack;
            }
        }

        private void OnPetSynergyAttack(string petId, string attackType, float syncLevel)
        {
            _ownerAttackCooldown = OWNER_ATTACK_COOLDOWN;
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;

            UpdateCooldowns(dt);
            UpdateActiveEffects(dt);

            if (_ownerAttackCooldown > 0f)
                _ownerAttackCooldown -= dt;

            _thinkAccumulator += dt;
            if (_thinkAccumulator >= SKILL_THINK_INTERVAL)
            {
                _thinkAccumulator = 0f;
                ThinkAndAct();
            }
        }

        // ── Skill Instance Management ───────────────────────────────────────

        /// <summary>
        /// 根据当前印记数据刷新所有技能实例
        /// </summary>
        public void RefreshSkillInstances()
        {
            if (_database == null || _mimicryData == null) return;

            _skillInstances.Clear();
            var unlocked = _database.GetUnlockedSkills(_mimicryData);
            foreach (var instance in unlocked)
            {
                _skillInstances[instance.Definition.SourceBehavior] = instance;
            }

            GD.Print($"[PetMimicrySkillSystem] Refreshed {_skillInstances.Count} skill instances");
        }

        private void UpdateCooldowns(float dt)
        {
            foreach (var instance in _skillInstances.Values)
            {
                instance.Tick(dt);
            }
        }

        private void UpdateActiveEffects(float dt)
        {
            var expired = new List<MimicrySkillType>();
            foreach (var kvp in _activeSkillEffects)
            {
                kvp.Value -= dt;
                if (kvp.Value <= 0f)
                    expired.Add(kvp.Key);
            }
            foreach (var type in expired)
            {
                _activeSkillEffects.Remove(type);
                OnActiveEffectExpired(type);
            }
        }

        private void OnActiveEffectExpired(MimicrySkillType skillType)
        {
            switch (skillType)
            {
                case MimicrySkillType.DodgeMaster:
                    GD.Print($"[PetMimicrySkillSystem] DodgeMaster effect expired, dodge rate restored");
                    break;
                case MimicrySkillType.LootInstinct:
                    GD.Print($"[PetMimicrySkillSystem] LootInstinct effect expired, bonus deactivated");
                    break;
                case MimicrySkillType.PuzzleInsight:
                    GD.Print($"[PetMimicrySkillSystem] PuzzleInsight effect expired");
                    break;
                case MimicrySkillType.SpecialMorph:
                    GD.Print($"[PetMimicrySkillSystem] SpecialMorph effect expired, form restored");
                    break;
                case MimicrySkillType.LastStand:
                    GD.Print($"[PetMimicrySkillSystem] LastStand effect expired, damage bonus removed");
                    break;
                case MimicrySkillType.Rearguard:
                    GD.Print($"[PetMimicrySkillSystem] Rearguard effect expired, movement bonus removed");
                    break;
                case MimicrySkillType.IronBulwark:
                    GD.Print($"[PetMimicrySkillSystem] IronBulwark shield expired");
                    break;
                default:
                    GD.Print($"[PetMimicrySkillSystem] Active effect {skillType} expired");
                    break;
            }
        }

        // ── Core AI Logic ──────────────────────────────────────────────────

        /// <summary>
        /// 主思考循环：检测环境变化 + 评估触发条件 + 选择并执行技能
        /// </summary>
        private void ThinkAndAct()
        {
            if (_mimicryData == null) return;

            var currentEnv = GetCurrentEnvironment();

            if (currentEnv != _lastEnvironment)
            {
                _lastEnvironment = currentEnv;
                RefreshSkillInstances();
            }

            var candidates = EvaluateTriggerConditions(currentEnv);
            if (candidates.Count == 0) return;

            var best = SelectBestSkill(candidates);
            if (best == null || !best.IsReady) return;

            if (ExecuteSkill(best))
            {
                if (!string.IsNullOrEmpty(best.Definition.TriggerConfig.MutexGroup))
                    _activeMutexGroups.Add(best.Definition.TriggerConfig.MutexGroup);

                EmitSignal(SignalName.MimicrySkillUsed, best.Definition.SourceBehavior, best.Definition.SkillType, Vector2.Zero);
            }
        }

        // ── Event Handlers ─────────────────────────────────────────────────

        private void OnCombatStarted()
        {
            RefreshSkillInstances();
            _thinkAccumulator = 0f;
            ThinkAndAct();
        }

        private void OnCombatEnded()
        {
            _activeSkillEffects.Clear();
            _activeMutexGroups.Clear();
            _ownerAttackCooldown = 0f;

            foreach (var instance in _skillInstances.Values)
            {
                instance.CurrentCooldown = 0f;
                instance.ChargeAccumulator = 0f;
                if (instance.Definition.MaxCharges > 0)
                    instance.CurrentCharges = instance.Definition.MaxCharges;
            }
        }

        private void OnOwnerDamaged(float damageAmount)
        {
            _lastOwnerDamageTime = Time.GetTicksMsec() / 1000f;
            GD.Print($"[PetMimicrySkillSystem] Owner damaged by {damageAmount:F0}, setting trigger window");
        }

        public void OnOwnerAttack()
        {
            _ownerAttackCooldown = OWNER_ATTACK_COOLDOWN;
        }

        private void OnSceneChanged(string scenePath)
        {
            var newEnv = GetCurrentEnvironment();
            if (newEnv != _lastEnvironment)
            {
                _lastEnvironment = newEnv;
                RefreshSkillInstances();

                if (MimicryLevelTracker.Instance != null && newEnv != RoomEnvironmentType.None)
                    MimicryLevelTracker.Instance.RefreshImprint(newEnv);
            }
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// 获取当前所有已解锁的技能（用于UI展示）
        /// </summary>
        public List<(PlayerBehaviorType Behavior, MimicrySkillType Skill, bool IsReady, float CooldownRemaining, float MaxCooldown)> GetAllSkillsStatus()
        {
            var result = new List<(PlayerBehaviorType, MimicrySkillType, bool, float, float)>();
            foreach (var kvp in _skillInstances)
            {
                var inst = kvp.Value;
                result.Add((kvp.Key, inst.Definition.SkillType, inst.IsReady, inst.CurrentCooldown, inst.Cooldown));
            }
            return result;
        }

        /// <summary>
        /// 获取当前激活的技能效果
        /// </summary>
        public Dictionary<MimicrySkillType, float> GetActiveEffects()
        {
            return new Dictionary<MimicrySkillType, float>(_activeSkillEffects);
        }

        /// <summary>
        /// 获取指定技能的当前状态（是否就绪）
        /// </summary>
        public bool IsSkillReady(PlayerBehaviorType behavior)
        {
            return _skillInstances.TryGetValue(behavior, out var inst) && inst.IsReady;
        }
    }
}
