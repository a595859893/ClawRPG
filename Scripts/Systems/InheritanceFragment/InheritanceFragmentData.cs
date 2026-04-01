using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.InheritanceFragment
{
    /// <summary>
    /// 传承碎片类型 - 定义碎片的解锁条件
    /// </summary>
    public enum FragmentType
    {
        FireBreathMemory,      // 火焰吐息记忆
        ComboGlossary,         // 连击初窥
        PetLoyalty,            // 宠物忠诚
        RelicIntuition,        // 遗物直觉
        EnrageAwakening       // 狂暴觉醒
    }

    /// <summary>
    /// 传承碎片 - 跨 run 保留的永久提示
    /// </summary>
    [System.Serializable]
    public class InheritanceFragment
    {
        public string FragmentId;
        public FragmentType Type;
        public string DisplayName;        // "火焰记忆"
        public string VagueHint;          // "直觉告诉你：Fury Rush 在特定时机很有效"
        public string DetailedHint;       // 详细提示（解锁后可见）
        public bool IsUnlocked;
        public int UnlockOrder;           // 解锁顺序（用于排序）
    }

    /// <summary>
    /// 成就 → 碎片映射配置
    /// </summary>
    [System.Serializable]
    public class AchievementFragmentMapping
    {
        public string AchievementId;       // 触发碎片的成就ID
        public FragmentType FragmentType; // 对应碎片类型
        public string FragmentId;         // 具体碎片ID
    }

    /// <summary>
    /// 传承碎片保存数据
    /// </summary>
    [System.Serializable]
    public class InheritanceFragmentSaveData
    {
        public List<string> UnlockedFragmentIds;
        public int TotalRunsCompleted;
        public Dictionary<string, int> FragmentUnlockCounts;  // 每个碎片解锁次数
    }

    /// <summary>
    /// 碎片数据库 - 预定义所有碎片
    /// </summary>
    public class InheritanceFragmentDatabase
    {
        private static InheritanceFragmentDatabase _instance;
        public static InheritanceFragmentDatabase Instance => _instance ??= new InheritanceFragmentDatabase();

        private List<InheritanceFragment> _fragments = new List<InheritanceFragment>();
        private Dictionary<FragmentType, InheritanceFragment> _fragmentsByType = new Dictionary<FragmentType, InheritanceFragment>();
        private Dictionary<string, InheritanceFragment> _fragmentsById = new Dictionary<string, InheritanceFragment>();

        public InheritanceFragmentDatabase()
        {
            InitializeFragments();
        }

        private void InitializeFragments()
        {
            // 火焰记忆 - 用 Fire Breath 击杀 Boss
            AddFragment(new InheritanceFragment
            {
                FragmentId = "fire_breath_boss_kill",
                Type = FragmentType.FireBreathMemory,
                DisplayName = "🔥 火焰记忆",
                VagueHint = "直觉告诉你：火焰在特定时机能够造成巨大伤害",
                DetailedHint = "Fire Breath 技能在 Boss 30% HP 以下使用时伤害翻倍",
                IsUnlocked = false,
                UnlockOrder = 1
            });

            // 连击初窥 - 成功执行 5+ 步 combo
            AddFragment(new InheritanceFragment
            {
                FragmentId = "combo_glossary_5step",
                Type = FragmentType.ComboGlossary,
                DisplayName = "⚡ 连击初窥",
                VagueHint = "直觉告诉你：某些技能组合能产生更强大的效果",
                DetailedHint = "连续使用 5 个以上技能会触发连锁加成，步骤越多加成越高",
                IsUnlocked = false,
                UnlockOrder = 2
            });

            // 宠物忠诚 - 宠物存活到 Boss 战结束
            AddFragment(new InheritanceFragment
            {
                FragmentId = "pet_loyalty_survived_boss",
                Type = FragmentType.PetLoyalty,
                DisplayName = "🐾 宠物忠诚",
                VagueHint = "直觉告诉你：你的伙伴还记得上次的战斗",
                DetailedHint = "宠物在主人 Boss 战中存活后，下一次出击会获得忠诚buff",
                IsUnlocked = false,
                UnlockOrder = 3
            });

            // 遗物直觉 - 收集 3+ 遗物通关
            AddFragment(new InheritanceFragment
            {
                FragmentId = "relic_intuition_3_collected",
                Type = FragmentType.RelicIntuition,
                DisplayName = "💎 遗物直觉",
                VagueHint = "直觉告诉你：遗物的组合似乎有某种规律",
                DetailedHint = "拥有 3 个同属性遗物时会触发套装效果",
                IsUnlocked = false,
                UnlockOrder = 4
            });

            // 狂暴觉醒 - Boss 狂暴状态下通关
            AddFragment(new InheritanceFragment
            {
                FragmentId = "enrage_awakening_boss_enraged",
                Type = FragmentType.EnrageAwakening,
                DisplayName = "⚠️ 狂暴觉醒",
                VagueHint = "直觉告诉你：Boss 在某个时刻会变得异常危险",
                DetailedHint = "Boss 在 30% HP 时进入狂暴状态，攻击力翻倍但防御下降",
                IsUnlocked = false,
                UnlockOrder = 5
            });

            // 火焰记忆 II - 多次用 Fire Breath 击杀
            AddFragment(new InheritanceFragment
            {
                FragmentId = "fire_breath_mastery",
                Type = FragmentType.FireBreathMemory,
                DisplayName = "🔥🔥 火焰大师",
                VagueHint = "直觉告诉你：火焰的力量不止于此",
                DetailedHint = "Fire Breath 可以与冰系技能形成蒸发反应，造成额外 50% 伤害",
                IsUnlocked = false,
                UnlockOrder = 6
            });

            // 连击进阶 - 成功执行 10+ 步 combo
            AddFragment(new InheritanceFragment
            {
                FragmentId = "combo_glossary_10step",
                Type = FragmentType.ComboGlossary,
                DisplayName = "⚡⚡ 连击大师",
                VagueHint = "直觉告诉你：连击的艺术远比你想象的深奥",
                DetailedHint = "10 步以上 combo 会触发终结技，伤害基于前面所有步骤的累积",
                IsUnlocked = false,
                UnlockOrder = 7
            });

            // 宠物协同 - 宠物参与多次 Boss 战
            AddFragment(new InheritanceFragment
            {
                FragmentId = "pet_synergy_boss_5",
                Type = FragmentType.PetLoyalty,
                DisplayName = "🐾🐾 宠物默契",
                VagueHint = "直觉告诉你：你和你的伙伴已经建立了某种联系",
                DetailedHint = "宠物参与 5 次 Boss 战后解锁协同技能槽位",
                IsUnlocked = false,
                UnlockOrder = 8
            });
        }

        private void AddFragment(InheritanceFragment fragment)
        {
            _fragments.Add(fragment);
            _fragmentsByType[fragment.Type] = fragment;
            _fragmentsById[fragment.FragmentId] = fragment;
        }

        public InheritanceFragment GetFragment(string fragmentId)
        {
            return _fragmentsById.ContainsKey(fragmentId) ? _fragmentsById[fragmentId] : null;
        }

        public InheritanceFragment GetFragmentByType(FragmentType type)
        {
            return _fragmentsByType.ContainsKey(type) ? _fragmentsByType[type] : null;
        }

        public List<InheritanceFragment> GetAllFragments()
        {
            return new List<InheritanceFragment>(_fragments);
        }

        public List<InheritanceFragment> GetUnlockedFragments()
        {
            return _fragments.FindAll(f => f.IsUnlocked);
        }

        public void SetFragmentUnlocked(string fragmentId, bool unlocked)
        {
            if (_fragmentsById.ContainsKey(fragmentId))
            {
                _fragmentsById[fragmentId].IsUnlocked = unlocked;
            }
        }
    }
}
