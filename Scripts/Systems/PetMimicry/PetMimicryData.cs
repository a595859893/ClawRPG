using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Systems.PetMimicry
{
    /// <summary>
    /// 宠物行为模仿数据 — 全局单例，存储所有行为印记
    /// 跨游戏持久化：存档时保存，进游戏时加载
    ///
    /// REQ-149: 增加条件触发性格机制层
    /// - 不再只返回"历史最多行为"作为性格
    /// - 结合HP状态、环境专精、事件驱动等条件触发器加权计算
    /// </summary>
    public partial class PetMimicryData : Node
    {
        public static PetMimicryData Instance { get; private set; }

        /// <summary>
        /// 所有行为印记列表 (environment × behavior)
        /// </summary>
        private List<BehaviorImprint> _imprints = new List<BehaviorImprint>();

        /// <summary>
        /// 每种行为类型的最高等级（用于宠物个性卡）
        /// </summary>
        private Dictionary<PlayerBehaviorType, int> _highestBehaviorLevel = new Dictionary<PlayerBehaviorType, int>();

        // ══════════════════════════════════════════════════════════════════════
        // REQ-149: 条件触发状态
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>当前玩家HP百分比 (0.0-1.0)，由外部系统更新</summary>
        private float _currentHpPercent = 1.0f;

        /// <summary>当前房间环境类型，由外部系统更新</summary>
        private RoomEnvironmentType _currentEnvironment = RoomEnvironmentType.None;

        /// <summary>近期事件触发的临时性格加成（会随时间衰减）</summary>
        private Dictionary<PlayerBehaviorType, float> _eventDrivenBonus = new Dictionary<PlayerBehaviorType, float>();

        /// <summary>各触发器的默认权重配置</summary>
        private const float HP_TRIGGER_WEIGHT = 2.0f;
        private const float ENV_TRIGGER_WEIGHT = 1.5f;
        private const float EVENT_TRIGGER_WEIGHT = 1.8f;
        private const float RECENT_TRIGGER_WEIGHT = 1.3f;
        private const float SUPPRESSED_TRIGGER_WEIGHT = -0.5f;

        private const float HP_LOW_THRESHOLD = 0.3f;
        private const float HP_CRITICAL_THRESHOLD = 0.15f;
        private const float HP_TRIGGER_WINDOW_SECONDS = 10f;
        private const float EVENT_BONUS_DECAY_SECONDS = 30f;
        private const float SUPPRESSION_THRESHOLD_SECONDS = 120f;

        /// <summary>最近记录的印记时间（用于近因偏好）</summary>
        private DateTime _mostRecentRecordTime = DateTime.MinValue;

        public override void _Ready()
        {
            Instance = this;
            GD.Print("[PetMimicryData] Initialized");
        }

        // ── Imprint Access ────────────────────────────────────────────────────

        public BehaviorImprint GetImprint(RoomEnvironmentType envType, PlayerBehaviorType behavior)
        {
            return _imprints.Find(i => i.EnvironmentType == envType && i.BehaviorType == behavior);
        }

        public int GetHighestLevel(PlayerBehaviorType behavior)
        {
            return _highestBehaviorLevel.TryGetValue(behavior, out var level) ? level : 0;
        }

        public List<BehaviorImprint> GetImprintsForEnvironment(RoomEnvironmentType envType)
        {
            return _imprints.FindAll(i => i.EnvironmentType == envType);
        }

        public List<BehaviorImprint> GetAllImprints() => new List<BehaviorImprint>(_imprints);

        public void AddImprint(BehaviorImprint imprint)
        {
            if (imprint.Fidelity <= 0f)
                imprint.Fidelity = 0.3f + (float)GD.RandDouble() * 0.3f;
            _imprints.Add(imprint);
            UpdateHighestLevel(imprint.BehaviorType, imprint.ImprintLevel);
            if (imprint.LastRecordedAt > _mostRecentRecordTime)
                _mostRecentRecordTime = imprint.LastRecordedAt;
        }

        private void UpdateHighestLevel(PlayerBehaviorType behavior, int level)
        {
            if (!_highestBehaviorLevel.ContainsKey(behavior) || _highestBehaviorLevel[behavior] < level)
                _highestBehaviorLevel[behavior] = level;
        }

        // ── External API ──────────────────────────────────────────────────────

        public void SetCurrentHpPercent(float hpPercent) => _currentHpPercent = Mathf.Clamp(hpPercent, 0f, 1f);
        public void SetCurrentEnvironment(RoomEnvironmentType envType) => _currentEnvironment = envType;

        public void TriggerEventDrivenBonus(PlayerBehaviorType behavior, float bonusAmount = 1.0f)
        {
            if (!_eventDrivenBonus.ContainsKey(behavior)) _eventDrivenBonus[behavior] = 0f;
            _eventDrivenBonus[behavior] += bonusAmount;
        }

        // ── 行为排名 ──────────────────────────────────────────────────────────

        public List<(PlayerBehaviorType Behavior, int Level)> GetBehaviorRanking()
        {
            var result = new List<(PlayerBehaviorType, int)>();
            foreach (PlayerBehaviorType behavior in Enum.GetValues(typeof(PlayerBehaviorType)))
            {
                int level = GetHighestLevel(behavior);
                if (level > 0) result.Add((behavior, level));
            }
            result.Sort((a, b) => b.Item2.CompareTo(a.Item2));
            return result;
        }

        // ── 性格描述 ──────────────────────────────────────────────────────────

        public PlayerBehaviorType? GetDominantBehavior() => GetDominantBehaviorEx().DominantBehavior;

        public string GetPersonalityDescription() => GetDominantBehaviorEx().Description;

        public static string GetBehaviorDisplayName(PlayerBehaviorType behavior)
        {
            switch (behavior)
            {
                case PlayerBehaviorType.UseFireSkill: return "火系";
                case PlayerBehaviorType.UseIceSkill: return "冰系";
                case PlayerBehaviorType.UseElectricSkill: return "电系";
                case PlayerBehaviorType.UseShadowSkill: return "暗系";
                case PlayerBehaviorType.UseHolySkill: return "神圣";
                case PlayerBehaviorType.UseNatureSkill: return "自然";
                case PlayerBehaviorType.FrequentDodge: return "闪避";
                case PlayerBehaviorType.AggressiveAttack: return "进攻";
                case PlayerBehaviorType.DefensiveStance: return "防守";
                case PlayerBehaviorType.LowHPAggression: return "背水一战";
                case PlayerBehaviorType.QuickRetreat: return "撤退";
                case PlayerBehaviorType.FocusElite: return "精英杀手";
                case PlayerBehaviorType.AvoidCombat: return "回避";
                case PlayerBehaviorType.TriggerTrap: return "触发陷阱";
                case PlayerBehaviorType.SolvePuzzle: return "解谜";
                case PlayerBehaviorType.CollectLoot: return "收藏家";
                case PlayerBehaviorType.UseHealing: return "治疗";
                case PlayerBehaviorType.PetSynergy: return "协战";
                case PlayerBehaviorType.SpecialInteraction: return "特殊互动";
                default: return behavior.ToString();
            }
        }

        // ── 重置 ──────────────────────────────────────────────────────────────

        public void ResetAll()
        {
            _imprints.Clear();
            _highestBehaviorLevel.Clear();
            _eventDrivenBonus.Clear();
            _mostRecentRecordTime = DateTime.MinValue;
            GD.Print("[PetMimicryData] All imprints reset");
        }
    }
}
