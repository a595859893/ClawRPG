using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Systems.PetMimicry
{
    /// <summary>
    /// 房间环境类型标签 — 支持多标签组合（flags）
    /// 用于宠物行为印记系统：宠物根据房间环境类型记录主人的行为
    /// </summary>
    [Flags]
    public enum RoomEnvironmentType
    {
        /// <summary>无特殊环境（默认普通房间）</summary>
        None = 0,

        /// <summary>火系环境 — 厨房、火源、熔岩等</summary>
        Fire = 1 << 0,

        /// <summary>冰系环境 — 冰霜、寒冷区域</summary>
        Ice = 1 << 1,

        /// <summary>陷阱密集区 — 机关密集的房间</summary>
        TrapDense = 1 << 2,

        /// <summary>Boss房间</summary>
        Boss = 1 << 3,

        /// <summary>撤退/逃生通道区</summary>
        Escape = 1 << 4,

        /// <summary>宝藏房间 — 高价值战利品区</summary>
        Treasure = 1 << 5,

        /// <summary>休息/恢复区 — 篝火、治疗泉</summary>
        Rest = 1 << 6,

        /// <summary>谜题房间</summary>
        Puzzle = 1 << 7,

        /// <summary>精英敌人区域</summary>
        Elite = 1 << 8,

        /// <summary>普通战斗区（无特殊环境）</summary>
        Combat = 1 << 9,

        /// <summary>入口/出生区域</summary>
        Entrance = 1 << 10,

        /// <summary>毒系环境</summary>
        Poison = 1 << 11,

        /// <summary>电系/机械环境</summary>
        Electric = 1 << 12,

        /// <summary>暗系/虚空环境</summary>
        Shadow = 1 << 13,

        /// <summary>神圣环境</summary>
        Holy = 1 << 14,

        /// <summary>自然/藤蔓环境</summary>
        Nature = 1 << 15
    }

    /// <summary>
    /// 玩家行为类型 — 宠物记录的主人在房间内的行为
    /// </summary>
    public enum PlayerBehaviorType
    {
        /// <summary>使用火系技能/法术</summary>
        UseFireSkill,

        /// <summary>使用冰系技能/法术</summary>
        UseIceSkill,

        /// <summary>使用电系技能</summary>
        UseElectricSkill,

        /// <summary>使用暗系技能</summary>
        UseShadowSkill,

        /// <summary>使用神圣技能</summary>
        UseHolySkill,

        /// <summary>使用自然/藤蔓技能</summary>
        UseNatureSkill,

        /// <summary>高频率位移/闪避</summary>
        FrequentDodge,

        /// <summary>积极进攻（高攻击频率）</summary>
        AggressiveAttack,

        /// <summary>防守姿态（高防御/护盾）</summary>
        DefensiveStance,

        /// <summary>低血量时激进出击（背水一战）</summary>
        LowHPAggression,

        /// <summary>快速撤退/脱离战斗</summary>
        QuickRetreat,

        /// <summary>优先击杀精英/Boss</summary>
        FocusElite,

        /// <summary>绕路避开敌人</summary>
        AvoidCombat,

        /// <summary>触发陷阱</summary>
        TriggerTrap,

        /// <summary>解决谜题</summary>
        SolvePuzzle,

        /// <summary>收集战利品</summary>
        CollectLoot,

        /// <summary>使用恢复/治疗</summary>
        UseHealing,

        /// <summary>宠物协战</summary>
        PetSynergy,

        /// <summary>触发特殊互动</summary>
        SpecialInteraction
    }

    /// <summary>
    /// 行为印记记录 — 将环境类型与行为关联
    /// </summary>
    public class BehaviorImprint
    {
        public RoomEnvironmentType EnvironmentType { get; set; }
        public PlayerBehaviorType BehaviorType { get; set; }
        public int ImprintLevel { get; set; }       // 0-5 模仿等级
        public float Xp { get; set; }
        public DateTime LastRecordedAt { get; set; }
        public int TotalTriggers { get; set; }

        public float GetXpForNextLevel()
        {
            // 对数曲线：等级越高所需XP越多
            return 10f * Mathf.Pow(2f, ImprintLevel);
        }

        public bool AddXp(float amount)
        {
            Xp += amount;
            if (Xp >= GetXpForNextLevel() && ImprintLevel < 5)
            {
                ImprintLevel++;
                Xp = 0f;
                return true; // 升级了
            }
            return false;
        }
    }

    /// <summary>
    /// 宠物行为模仿数据 — 全局单例，存储所有行为印记
    /// 跨游戏持久化：存档时保存，进游戏时加载
    /// </summary>
    public class PetMimicryData : Node
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

        public override void _Ready()
        {
            Instance = this;
            GD.Print("[PetMimicryData] Initialized");
        }

        // ── Imprint Access ─────────────────────────────────────────────────

        /// <summary>
        /// 获取指定环境+行为类型的印记
        /// </summary>
        public BehaviorImprint GetImprint(RoomEnvironmentType envType, PlayerBehaviorType behavior)
        {
            return _imprints.Find(i =>
                i.EnvironmentType == envType && i.BehaviorType == behavior);
        }

        /// <summary>
        /// 获取指定行为类型在所有环境中的最高等级
        /// </summary>
        public int GetHighestLevel(PlayerBehaviorType behavior)
        {
            return _highestBehaviorLevel.TryGetValue(behavior, out var level) ? level : 0;
        }

        /// <summary>
        /// 获取某环境类型下的所有印记
        /// </summary>
        public List<BehaviorImprint> GetImprintsForEnvironment(RoomEnvironmentType envType)
        {
            return _imprints.FindAll(i => i.EnvironmentType == envType);
        }

        /// <summary>
        /// 获取所有印记
        /// </summary>
        public List<BehaviorImprint> GetAllImprints()
        {
            return new List<BehaviorImprint>(_imprints);
        }

        /// <summary>
        /// 添加新印记
        /// </summary>
        public void AddImprint(BehaviorImprint imprint)
        {
            _imprints.Add(imprint);
            UpdateHighestLevel(imprint.BehaviorType, imprint.ImprintLevel);
        }

        private void UpdateHighestLevel(PlayerBehaviorType behavior, int level)
        {
            if (!_highestBehaviorLevel.ContainsKey(behavior) ||
                _highestBehaviorLevel[behavior] < level)
            {
                _highestBehaviorLevel[behavior] = level;
            }
        }

        /// <summary>
        /// 获取宠物的个性类型（最高等级的行为类型）
        /// </summary>
        public PlayerBehaviorType? GetDominantBehavior()
        {
            PlayerBehaviorType? dominant = null;
            int maxLevel = 0;
            foreach (var kvp in _highestBehaviorLevel)
            {
                if (kvp.Value > maxLevel)
                {
                    maxLevel = kvp.Value;
                    dominant = kvp.Key;
                }
            }
            return dominant;
        }

        /// <summary>
        /// 获取所有行为类型的等级排名
        /// </summary>
        public List<(PlayerBehaviorType Behavior, int Level)> GetBehaviorRanking()
        {
            var result = new List<(PlayerBehaviorType, int)>();
            foreach (PlayerBehaviorType behavior in Enum.GetValues(typeof(PlayerBehaviorType)))
            {
                int level = GetHighestLevel(behavior);
                if (level > 0)
                    result.Add((behavior, level));
            }
            result.Sort((a, b) => b.Item2.CompareTo(a.Item2));
            return result;
        }

        // ── Persistence ─────────────────────────────────────────────────────

        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            var imprintList = new List<Dictionary>();
            foreach (var imprint in _imprints)
            {
                imprintList.Add(new Dictionary
                {
                    { "envType", (int)imprint.EnvironmentType },
                    { "behaviorType", (int)imprint.BehaviorType },
                    { "level", imprint.ImprintLevel },
                    { "xp", imprint.Xp },
                    { "lastRecorded", imprint.LastRecordedAt.ToString("o") },
                    { "totalTriggers", imprint.TotalTriggers }
                });
            }
            data["imprints"] = imprintList;
            return data;
        }

        public override void ImportSaveData(Dictionary data)
        {
            _imprints.Clear();
            _highestBehaviorLevel.Clear();

            if (data == null || !data.Contains("imprints")) return;

            var imprintList = (Godot.Collections.Array)data["imprints"];
            foreach (Dictionary imprintData in imprintList)
            {
                var imprint = new BehaviorImprint
                {
                    EnvironmentType = (RoomEnvironmentType)(int)imprintData["envType"],
                    BehaviorType = (PlayerBehaviorType)(int)imprintData["behaviorType"],
                    ImprintLevel = (int)imprintData["level"],
                    Xp = (float)(double)imprintData["xp"],
                    LastRecordedAt = DateTime.Parse((string)imprintData["lastRecorded"]),
                    TotalTriggers = (int)imprintData["totalTriggers"]
                };
                _imprints.Add(imprint);
                UpdateHighestLevel(imprint.BehaviorType, imprint.ImprintLevel);
            }

            GD.Print($"[PetMimicryData] Loaded {_imprints.Count} behavior imprints from save");
        }

        /// <summary>
        /// 重置所有印记（用于测试或新游戏）
        /// </summary>
        public void ResetAll()
        {
            _imprints.Clear();
            _highestBehaviorLevel.Clear();
            GD.Print("[PetMimicryData] All imprints reset");
        }
    }
}
