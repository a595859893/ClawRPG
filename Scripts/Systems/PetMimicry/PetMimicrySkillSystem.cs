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
    /// </summary>
    public class PetMimicrySkillSystem : Node
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

        // ── Signals ─────────────────────────────────────────────────────────
        [Signal]
        public delegate void MimicrySkillUsedEventHandler(PlayerBehaviorType skillType, MimicrySkillType mimSkill, Vector2 worldPosition);

        [Signal]
        public delegate void MimicrySkillLearnedEventHandler(PlayerBehaviorType behavior, int newLevel);

        [Signal]
        public delegate void MimicrySkillReadyEventHandler(PlayerBehaviorType skillType, bool isReady);

        public override void _Ready()
        {
            Instance = this;

            // 等待依赖系统
            _ProcessDeferred();
        }

        private async void _ProcessDeferred()
        {
            // Wait one frame for other systems to initialize
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

            RefreshSkillInstances();
            SubscribeToEvents();

            GD.Print("[PetMimicrySkillSystem] Initialized");
        }

        private void SubscribeToEvents()
        {
            var bus = EventBusManager.Instance;
            if (bus == null) return;

            // 战斗开始时刷新技能（无数据事件）
            bus.Subscribe(EventBusManager.Events.CombatStarted, OnCombatStarted);

            // 战斗结束时清除激活效果（无数据事件）
            bus.Subscribe(EventBusManager.Events.CombatEnded, OnCombatEnded);

            // 环境变化时重新评估技能
            bus.Subscribe<string>(EventBusManager.Events.SceneChanged, OnSceneChanged);

            // REQ-146: 主人受伤时触发反击类技能
            bus.Subscribe<float>(EventBusManager.Events.PlayerHealthChanged, OnOwnerDamaged);

            // REQ-146: 主人攻击时触发协同技能（通过宠物协战信号间接检测）
            if (PetCombatCompanionSystem.Instance != null)
                PetCombatCompanionSystem.Instance.SynergyAttackTriggered += OnPetSynergyAttack;
        }

        private void OnPetSynergyAttack(string petId, string attackType, float syncLevel)
        {
            // 宠物协战信号说明主人刚刚发起了攻击
            _ownerAttackCooldown = OWNER_ATTACK_COOLDOWN;
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;

            // 更新冷却
            UpdateCooldowns(dt);

            // 更新激活效果持续时间
            UpdateActiveEffects(dt);

            // REQ-146: 更新攻击冷却
            if (_ownerAttackCooldown > 0f)
                _ownerAttackCooldown -= dt;

            // 技能思考逻辑
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

        /// <summary>
        /// 获取当前环境最佳技能实例
        /// </summary>
        private MimicrySkillInstance GetBestSkillForCurrentEnvironment()
        {
            if (_database == null || _mimicryData == null) return null;

            var currentEnv = GetCurrentEnvironment();
            var def = _database.GetBestSkillForEnvironment(currentEnv, _mimicryData);
            if (def == null) return null;

            if (_skillInstances.TryGetValue(def.SourceBehavior, out var instance))
            {
                return instance;
            }
            return null;
        }

        /// <summary>
        /// 获取当前环境的房间类型
        /// </summary>
        private RoomEnvironmentType GetCurrentEnvironment()
        {
            try
            {
                var dungeon = ProceduralDungeonSystem.Instance?.CurrentDungeon;
                var room = dungeon?.CurrentRoom;
                return RoomEnvironmentClassifier.Classify(room);
            }
            catch
            {
                return RoomEnvironmentType.None;
            }
        }

        // ── Cooldown & Effect Updates ───────────────────────────────────────

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
            // Bug 3 fix: 被动技能效果过期时同步 UI 状态
            switch (skillType)
            {
                case MimicrySkillType.DodgeMaster:
                    // 移除闪避buff（宠物闪避率恢复正常）
                    GD.Print($"[PetMimicrySkillSystem] DodgeMaster effect expired, dodge rate restored");
                    break;
                case MimicrySkillType.LootInstinct:
                    // 关闭战利品加成
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

        // ══════════════════════════════════════════════════════════════
        // REQ-146: Skill Activator — 触发评估 + 优先级选择 + 互斥组
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 技能决策循环：评估所有技能触发条件 → 选最佳 → 执行
        /// </summary>
        private void ThinkAndAct()
        {
            if (_mimicryData == null) return;

            var currentEnv = GetCurrentEnvironment();

            // 检测环境变化
            if (currentEnv != _lastEnvironment)
            {
                _lastEnvironment = currentEnv;
                RefreshSkillInstances();
            }

            // REQ-146: 评估所有技能的触发条件
            var candidates = EvaluateTriggerConditions(currentEnv);
            if (candidates.Count == 0) return;

            // REQ-146: 按优先级选择最佳技能（尊重互斥组）
            var best = SelectBestSkill(candidates);
            if (best == null || !best.IsReady) return;

            // 执行技能
            if (ExecuteSkill(best))
            {
                // 标记互斥组为活跃
                if (!string.IsNullOrEmpty(best.Definition.TriggerConfig.MutexGroup))
                    _activeMutexGroups.Add(best.Definition.TriggerConfig.MutexGroup);

                EmitSignal(SignalName.MimicrySkillUsed, best.Definition.SourceBehavior, best.Definition.SkillType, Vector2.Zero);
            }
        }

        /// <summary>
        /// REQ-146: 评估所有技能的触发条件，返回满足条件的技能列表
        /// </summary>
        private List<MimicrySkillInstance> EvaluateTriggerConditions(RoomEnvironmentType currentEnv)
        {
            var candidates = new List<MimicrySkillInstance>();
            var petHp = GetPetHpPercent();
            var ownerHp = GetOwnerHpPercent();
            var nearbyEnemy = GetCurrentTarget();
            float timeSinceOwnerDamage = Time.GetTicksMsec() / 1000f - _lastOwnerDamageTime;

            foreach (var kvp in _skillInstances)
            {
                var inst = kvp.Value;
                var def = inst.Definition;
                var trigger = def.TriggerConfig;

                // 冷却检查
                if (!inst.IsReady) continue;

                // 互斥组检查（同组技能正在执行则跳过）
                if (!string.IsNullOrEmpty(trigger.MutexGroup) && _activeMutexGroups.Contains(trigger.MutexGroup))
                    continue;

                // 被动/持续性技能已经在激活列表则跳过
                if (_activeSkillEffects.ContainsKey(def.SkillType) &&
                    (def.SkillType == MimicrySkillType.DodgeMaster ||
                     def.SkillType == MimicrySkillType.LootInstinct ||
                     def.SkillType == MimicrySkillType.PuzzleInsight ||
                     def.SkillType == MimicrySkillType.SpecialMorph))
                    continue;

                bool triggered = false;
                switch (trigger.Trigger)
                {
                    case MimicryTriggerType.HpBelowThreshold:
                        // 宠物HP低于阈值
                        triggered = petHp < trigger.Threshold;
                        break;

                    case MimicryTriggerType.OnOwnerDamaged:
                        // 主人最近受伤（3秒窗口）
                        triggered = timeSinceOwnerDamage < OWNER_DAMAGE_COOLDOWN;
                        break;

                    case MimicryTriggerType.OnEnemyNearby:
                        // 敌人在技能范围内
                        triggered = nearbyEnemy != null && IsEnemyInRange(nearbyEnemy, trigger.Range);
                        break;

                    case MimicryTriggerType.OnOwnerAttacking:
                        // 需要检测主人是否在攻击（通过 PetSynergyAttackTriggered 信号被动触发检查）
                        triggered = _ownerAttackCooldown > 0f && nearbyEnemy != null;
                        break;

                    case MimicryTriggerType.OnEnvironmentMatch:
                        // 当前环境匹配技能的环境类型
                        triggered = (currentEnv & trigger.EnvironmentType) == trigger.EnvironmentType;
                        break;

                    case MimicryTriggerType.CooldownBased:
                        // 冷却结束自动触发（默认行为）
                        triggered = true;
                        break;

                    case MimicryTriggerType.ManualToggle:
                        // 手动激活（暂不自动触发）
                        triggered = false;
                        break;

                    case MimicryTriggerType.None:
                    default:
                        triggered = false;
                        break;
                }

                if (triggered)
                    candidates.Add(inst);
            }

            return candidates;
        }

        /// <summary>
        /// REQ-146: 从候选技能中选择最佳技能（最高优先级，互斥组去重）
        /// </summary>
        private MimicrySkillInstance SelectBestSkill(List<MimicrySkillInstance> candidates)
        {
            if (candidates.Count == 0) return null;
            if (candidates.Count == 1) return candidates[0];

            // 按优先级降序排序
            candidates.Sort((a, b) =>
                b.Definition.TriggerConfig.Priority.CompareTo(a.Definition.TriggerConfig.Priority));

            return candidates[0];
        }

        /// <summary>
        /// 检查敌人是否在指定范围内
        /// </summary>
        private bool IsEnemyInRange(Node2D enemy, float range)
        {
            if (enemy == null) return false;
            var petPos = GetPetPosition();
            return petPos.DistanceTo(enemy.GlobalPosition) <= range;
        }

        // ══════════════════════════════════════════════════════════════
        // Legacy trigger evaluation (kept for compatibility)
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// 判断是否应该使用该技能（REX-146前向兼容）
        /// </summary>
        private bool ShouldUseSkill(MimicrySkillInstance skill, RoomEnvironmentType env)
        {
            var def = skill.Definition;

            // 检查被动/持续性技能
            switch (def.SkillType)
            {
                case MimicrySkillType.DodgeMaster:
                case MimicrySkillType.LootInstinct:
                case MimicrySkillType.PuzzleInsight:
                    if (_activeSkillEffects.ContainsKey(def.SkillType)) return false;
                    break;
            }

            // 检查是否有敌人目标
            var target = GetCurrentTarget();
            if (def.BaseDamage > 0f && target == null) return false;

            // HP检查（LastStand）
            if (def.SkillType == MimicrySkillType.LastStand)
            {
                float hpPercent = GetPetHpPercent();
                if (hpPercent > 0.3f) return false;
            }

            return true;
        }

        /// <summary>
        /// 执行技能效果
        /// </summary>
        private bool ExecuteSkill(MimicrySkillInstance skill)
        {
            if (!skill.TryUse()) return false;

            var def = skill.Definition;
            var targets = GetSkillTargets(def.SkillType);

            switch (def.SkillType)
            {
                case MimicrySkillType.FireBreath:
                case MimicrySkillType.FrostBreath:
                case MimicrySkillType.ElectricArc:
                case MimicrySkillType.ShadowTear:
                case MimicrySkillType.HolySmite:
                case MimicrySkillType.NatureBind:
                case MimicrySkillType.DashStrike:
                case MimicrySkillType.FrenzyStrike:
                case MimicrySkillType.EliteSlayer:
                case MimicrySkillType.SynergyFangs:
                    // 伤害技能：应用到所有目标
                    ApplyDamageSkill(def.SkillType, def.GetDamage(skill.ImprintLevel), targets);
                    break;

                case MimicrySkillType.IronBulwark:
                    // 护盾技能
                    ApplyShieldSkill(def.GetDuration(skill.ImprintLevel));
                    break;

                case MimicrySkillType.LastStand:
                    // 低血狂暴：提升伤害
                    ApplyBuffSkill(MimicrySkillType.LastStand, def.GetDuration(skill.ImprintLevel));
                    break;

                case MimicrySkillType.Rearguard:
                    // 掩护撤退：提升移速+减伤
                    ApplyBuffSkill(MimicrySkillType.Rearguard, def.GetDuration(skill.ImprintLevel));
                    break;

                case MimicrySkillType.DodgeMaster:
                    // 闪避精通：激活闪避buff
                    ApplyBuffSkill(def.SkillType, def.GetDuration(skill.ImprintLevel));
                    break;

                case MimicrySkillType.HealingLight:
                    // 治疗之光
                    ApplyHealingSkill(def.GetDamage(skill.ImprintLevel)); // 负值=治疗
                    break;

                case MimicrySkillType.TrapSense:
                    // 陷阱感知：短暂无敌
                    ApplyBuffSkill(def.SkillType, def.GetDuration(skill.ImprintLevel));
                    break;

                case MimicrySkillType.LootInstinct:
                case MimicrySkillType.PuzzleInsight:
                case MimicrySkillType.SpecialMorph:
                    // 持续性技能
                    ApplyBuffSkill(def.SkillType, def.GetDuration(skill.ImprintLevel));
                    break;
            }

            // 播放视觉反馈
            PlaySkillVFX(def.SkillType, targets);

            return true;
        }

        // ── Target Acquisition ─────────────────────────────────────────────

        /// <summary>
        /// 获取宠物的当前攻击目标（优先使用 PetCombatAI 的目标）
        /// </summary>
        private Node2D GetCurrentTarget()
        {
            try
            {
                // 优先从 PetCombatAI 获取
                if (PetCombatAI.Instance != null)
                {
                    var aiTarget = PetCombatAI.Instance.GetCurrentTarget();
                    if (aiTarget != null) return aiTarget;
                }

                // 回退：扫描最近的敌人
                return FindNearestEnemy();
            }
            catch
            {
                return FindNearestEnemy();
            }
        }

        /// <summary>
        /// 查找最近的敌人
        /// </summary>
        private Node2D FindNearestEnemy()
        {
            var petPos = GetPetPosition();
            Node2D nearest = null;
            float nearestDist = float.MaxValue;

            try
            {
                var enemies = GetTree().GetNodesInGroup("enemy");
                foreach (Node node in enemies)
                {
                    if (node is Node2D enemy)
                    {
                        float dist = petPos.DistanceTo(enemy.GlobalPosition);
                        if (dist < nearestDist && dist <= ENEMY_SCAN_RANGE)
                        {
                            nearestDist = dist;
                            nearest = enemy;
                        }
                    }
                }
            }
            catch { }

            return nearest;
        }

        /// <summary>
        /// 获取技能目标列表（用于AOE技能）
        /// </summary>
        private List<Node2D> GetSkillTargets(MimicrySkillType skillType)
        {
            var targets = new List<Node2D>();
            var primary = GetCurrentTarget();
            if (primary == null) return targets;

            targets.Add(primary);

            // AOE技能获取额外目标
            if (skillType == MimicrySkillType.ElectricArc ||
                skillType == MimicrySkillType.FireBreath ||
                skillType == MimicrySkillType.NatureBind)
            {
                var petPos = GetPetPosition();
                var enemies = GetTree().GetNodesInGroup("enemy");
                foreach (Node node in enemies)
                {
                    if (node is Node2D enemy && enemy != primary)
                    {
                        float dist = petPos.DistanceTo(enemy.GlobalPosition);
                        if (dist <= ENEMY_SCAN_RANGE && targets.Count < MAX_SKILL_TARGETS)
                        {
                            targets.Add(enemy);
                        }
                    }
                }
            }

            return targets;
        }

        /// <summary>
        /// 获取宠物节点的世界坐标
        /// </summary>
        private Vector2 GetPetPosition()
        {
            try
            {
                if (PetCombatAI.Instance != null)
                {
                    var node = PetCombatAI.Instance.GetPetNode();
                    if (node != null) return node.GlobalPosition;
                }
                // 回退到玩家位置
                var players = GetTree().GetNodesInGroup("player");
                foreach (Node node in players)
                {
                    if (node is Node2D p) return p.GlobalPosition;
                }
            }
            catch { }
            return Vector2.Zero;
        }

        /// <summary>
        /// 获取宠物当前HP百分比
        /// </summary>
        private float GetPetHpPercent()
        {
            try
            {
                if (PetCombatAI.Instance != null)
                {
                    return PetCombatAI.Instance.GetPetHpPercent();
                }
            }
            catch { }
            return 1f; // 默认满血
        }

        /// <summary>
        /// REQ-146: 获取主人当前HP百分比
        /// </summary>
        private float GetOwnerHpPercent()
        {
            try
            {
                var players = GetTree().GetNodesInGroup("player");
                foreach (Node node in players)
                {
                    if (node is Character ch)
                    {
                        float maxHp = ch.MaxHealth;
                        if (maxHp > 0f)
                            return ch.Health / maxHp;
                    }
                }
            }
            catch { }
            return 1f;
        }

        // ── Skill Effects ───────────────────────────────────────────────────

        /// <summary>
        /// 应用伤害技能
        /// </summary>
        private void ApplyDamageSkill(MimicrySkillType skillType, float damage, List<Node2D> targets)
        {
            string dmgType = GetDamageType(skillType);
            var petPos = GetPetPosition();

            foreach (var target in targets)
            {
                if (target == null) continue;

                // 应用伤害
                var enemy = target as Enemy;
                if (enemy != null)
                {
                    int finalDamage = CalculateFinalDamage(damage, skillType, target);
                    enemy.TakeDamage(finalDamage);
                }

                GD.Print($"[PetMimicrySkillSystem] {skillType} dealt {damage:F0} ({dmgType}) to {(target.Name ?? "enemy")}");
            }

            // 发射协同攻击信号（触发宠物协战增益）
            if (PetCombatCompanionSystem.Instance != null)
            {
                PetCombatCompanionSystem.Instance.SynergyAttackTriggered?.Invoke(
                    "mimicry", skillType.ToString(), 1.0f);
            }
        }

        /// <summary>
        /// 计算最终伤害（含Buff加成）
        /// </summary>
        private int CalculateFinalDamage(float baseDamage, MimicrySkillType skillType, Node2D target)
        {
            float multiplier = baseDamage;

            // REQ-150 Bug 1 fix: 应用 fidelity 修正系数
            float fidelity = GetCurrentFidelity();
            if (fidelity < 0.4f)
                multiplier *= 0.7f;
            else if (fidelity >= 0.7f)
                multiplier *= 1.0f;
            else
                multiplier *= (0.7f + (fidelity - 0.4f) / 0.3f * 0.3f); // 线性插值 0.4→0.7

            // LastStand 低血狂暴加成
            if (_activeSkillEffects.ContainsKey(MimicrySkillType.LastStand))
            {
                multiplier *= 1.5f;
            }

            // SynergyFangs 协同加成
            if (PetCombatCompanionSystem.Instance != null)
            {
                float syncLevel = PetCombatCompanionSystem.Instance.GetCurrentSyncLevel();
                multiplier *= (1f + syncLevel * 0.1f);
            }

            return Mathf.RoundToInt(multiplier);
        }

        /// <summary>
        /// REQ-150 Bug 1 fix: 获取当前技能的 fidelity 值
        /// </summary>
        private float GetCurrentFidelity()
        {
            if (_mimicryData == null) return 0.5f;
            var currentEnv = GetCurrentEnvironment();
            // 遍历所有 imprints 找 fidelity 最高的
            float bestFidelity = 0.5f; // 默认值
            foreach (var imprint in _mimicryData.GetAllImprints())
            {
                if (imprint.Fidelity > bestFidelity)
                    bestFidelity = imprint.Fidelity;
            }
            return bestFidelity;
        }

        /// <summary>
        /// 将技能类型映射为伤害类型字符串
        /// </summary>
        private string GetDamageType(MimicrySkillType skillType)
        {
            return skillType switch
            {
                MimicrySkillType.FireBreath => "fire",
                MimicrySkillType.FrostBreath => "ice",
                MimicrySkillType.ElectricArc => "electric",
                MimicrySkillType.ShadowTear => "shadow",
                MimicrySkillType.HolySmite => "holy",
                MimicrySkillType.NatureBind => "nature",
                MimicrySkillType.DashStrike => "physical",
                MimicrySkillType.FrenzyStrike => "physical",
                MimicrySkillType.EliteSlayer => "physical",
                MimicrySkillType.SynergyFangs => "physical",
                _ => "physical"
            };
        }

        /// <summary>
        /// 应用护盾技能
        /// </summary>
        private void ApplyShieldSkill(float duration)
        {
            // 护盾值 = 宠物最大HP * 20%
            float shieldAmount = 50f; // 默认值
            try
            {
                if (PetCombatAI.Instance != null)
                {
                    shieldAmount = PetCombatAI.Instance.GetPetMaxHp() * 0.2f;
                }
            }
            catch { }

            _activeSkillEffects[MimicrySkillType.IronBulwark] = duration;
            GD.Print($"[PetMimicrySkillSystem] IronBulwark: +{shieldAmount:F0} shield for {duration:F1}s");
        }

        /// <summary>
        /// 应用Buff技能（持续性效果）
        /// </summary>
        private void ApplyBuffSkill(MimicrySkillType skillType, float duration)
        {
            _activeSkillEffects[skillType] = duration;
            GD.Print($"[PetMimicrySkillSystem] Buff {skillType} activated for {duration:F1}s");
        }

        /// <summary>
        /// 应用治疗技能
        /// </summary>
        private void ApplyHealingSkill(float healAmount)
        {
            // healAmount 是负值（如 -20），取绝对值
            float heal = Mathf.Abs(healAmount);
            try
            {
                if (PetCombatAI.Instance != null)
                {
                    PetCombatAI.Instance.HealPet(Mathf.RoundToInt(heal));
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[PetMimicrySkillSystem] Failed to heal pet: {ex.Message}");
            }
        }

        // ── Visual & Events ────────────────────────────────────────────────

        /// <summary>
        /// 播放技能视觉特效
        /// </summary>
        private void PlaySkillVFX(MimicrySkillType skillType, List<Node2D> targets)
        {
            // 发射 PetAttacked 信号供 PetAttackVFX 使用（屏幕边缘爪痕）
            if (targets.Count > 0 && targets[0] != null)
            {
                if (PetCombatAI.Instance != null)
                {
                    PetCombatAI.Instance.EmitPetAttackedSignal(targets[0], 0); // 伤害已在 ApplyDamageSkill 结算
                }
            }

            // 发射 MimicrySkillUsed 信号供 UI 使用
            EmitSignal(SignalName.MimicrySkillUsed,
                GetSkillBehaviorType(skillType),
                skillType,
                GetPetPosition());
        }

        /// <summary>
        /// 获取技能对应的行为类型
        /// </summary>
        private PlayerBehaviorType GetSkillBehaviorType(MimicrySkillType skillType)
        {
            if (_database == null) return PlayerBehaviorType.AggressiveAttack;
            var def = _database.GetSkill(skillType);
            return def?.SourceBehavior ?? PlayerBehaviorType.AggressiveAttack;
        }

        // ── Event Handlers ─────────────────────────────────────────────────

        private void OnCombatStarted()
        {
            // 战斗开始：刷新技能实例
            RefreshSkillInstances();
            _thinkAccumulator = 0f;

            // 立即思考一次
            ThinkAndAct();
        }

        private void OnCombatEnded()
        {
            // 战斗结束：清除激活效果和战斗内状态，但保留 skillInstances（避免宠物"失忆"）
            _activeSkillEffects.Clear();
            _activeMutexGroups.Clear();
            _ownerAttackCooldown = 0f;

            // Bug 2 fix: 保留 skillInstances，只重置运行时冷却/充能
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

        /// <summary>
        /// REQ-146: 当主人发起攻击时调用（由 PetSynergyAttackTriggered 或类似信号触发）
        /// </summary>
        public void OnOwnerAttack()
        {
            _ownerAttackCooldown = OWNER_ATTACK_COOLDOWN;
        }

        private void OnSceneChanged(string scenePath)
        {
            // 场景变化：重新检测环境
            var newEnv = GetCurrentEnvironment();
            if (newEnv != _lastEnvironment)
            {
                _lastEnvironment = newEnv;
                RefreshSkillInstances();

                // REQ-143: 尝试刷新匹配的印记
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
