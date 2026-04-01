using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.InheritanceFragment
{
    /// <summary>
    /// 传承碎片系统 - 管理碎片的解锁、存储和显示
    /// </summary>
    public partial class InheritanceFragmentSystem : BaseSystem
    {
        private static InheritanceFragmentSystem _instance;
        public static InheritanceFragmentSystem Instance => _instance;

        // 已解锁的碎片 ID 集合
        private HashSet<string> _unlockedFragmentIds = new HashSet<string>();

        // 成就 → 碎片映射
        private Dictionary<string, string> _achievementToFragmentMap = new Dictionary<string, string>();

        // Signals
        public delegate void FragmentUnlockedEventHandler(string fragmentId, InheritanceFragment fragment);
        public delegate void FragmentsReadyEventHandler(List<InheritanceFragment> fragments);

        private InheritanceFragmentDatabase _database;

        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            _database = InheritanceFragmentDatabase.Instance;
            InitializeMapping();
            LoadFragments();
        }

        protected override string SystemName => "InheritanceFragmentSystem";

        #region Initialization

        /// <summary>
        /// 初始化成就 → 碎片映射
        /// </summary>
        private void InitializeMapping()
        {
            // 火焰吐息 - Boss 击杀
            _achievementToFragmentMap["fire_breath_boss_kill"] = "fire_breath_boss_kill";
            _achievementToFragmentMap["fire_breath_mastery"] = "fire_breath_mastery";

            // 连击 - 5步/10步 combo
            _achievementToFragmentMap["combo_5_step"] = "combo_glossary_5step";
            _achievementToFragmentMap["combo_10_step"] = "combo_glossary_10step";

            // 宠物 - Boss 战存活
            _achievementToFragmentMap["pet_survived_boss"] = "pet_loyalty_survived_boss";
            _achievementToFragmentMap["pet_boss_5_times"] = "pet_synergy_boss_5";

            // 遗物 - 收集通关
            _achievementToFragmentMap["relic_3_cleared"] = "relic_intuition_3_collected";

            // 狂暴 - 狂暴通关
            _achievementToFragmentMap["enrage_cleared"] = "enrage_awakening_boss_enraged";
        }

        /// <summary>
        /// 从存档加载碎片状态
        /// </summary>
        private void LoadFragments()
        {
            // 碎片状态会在 ImportSaveData 时恢复
        }

        #endregion

        #region Fragment Unlocking

        /// <summary>
        /// 尝试解锁碎片 - 当成就触发时调用
        /// </summary>
        public void TryUnlockFragment(string achievementId)
        {
            if (_achievementToFragmentMap.TryGetValue(achievementId, out string fragmentId))
            {
                if (!_unlockedFragmentIds.Contains(fragmentId))
                {
                    UnlockFragment(fragmentId);
                }
            }
        }

        /// <summary>
        /// 解锁指定碎片
        /// </summary>
        public void UnlockFragment(string fragmentId)
        {
            var fragment = _database.GetFragment(fragmentId);
            if (fragment == null || fragment.IsUnlocked)
                return;

            fragment.IsUnlocked = true;
            _unlockedFragmentIds.Add(fragmentId);

            GD.Print($"[InheritanceFragment] 解锁碎片: {fragment.DisplayName}");

            // 触发信号
            OnFragmentUnlocked?.Invoke(fragmentId, fragment);

            // 触发 UI 提示
            ShowFragmentUnlockNotification(fragment);
        }

        /// <summary>
        /// 显示碎片解锁通知
        /// </summary>
        private void ShowFragmentUnlockNotification(InheritanceFragment fragment)
        {
            // 通知 UI 显示碎片获得提示
            // 延迟一帧确保 UI 系统已就绪
            CallDeferred(nameof(_ShowNotificationDeferred), fragment);
        }

        private void _ShowNotificationDeferred(InheritanceFragment fragment)
        {
            // 通过信号通知 UI 层
            // UI 会在适当的时机（如 run 开始）显示所有已解锁碎片
        }

        #endregion

        #region Fragment Queries

        /// <summary>
        /// 获取所有已解锁碎片
        /// </summary>
        public List<InheritanceFragment> GetUnlockedFragments()
        {
            var result = new List<InheritanceFragment>();
            foreach (var fragmentId in _unlockedFragmentIds)
            {
                var fragment = _database.GetFragment(fragmentId);
                if (fragment != null)
                    result.Add(fragment);
            }
            result.Sort((a, b) => a.UnlockOrder.CompareTo(b.UnlockOrder));
            return result;
        }

        /// <summary>
        /// 获取已解锁碎片的模糊提示（用于 run 开始时显示）
        /// </summary>
        public List<string> GetVagueHintsForNewRun()
        {
            var hints = new List<string>();
            foreach (var fragment in GetUnlockedFragments())
            {
                hints.Add(fragment.VagueHint);
            }
            return hints;
        }

        /// <summary>
        /// 检查是否有未显示的新碎片提示
        /// </summary>
        public bool HasUnseenFragments()
        {
            return _unlockedFragmentIds.Count > 0;
        }

        #endregion

        #region Achievement Integration

        /// <summary>
        /// 从 AchievementSystem 订阅成就解锁事件
        /// </summary>
        public void SubscribeToAchievements()
        {
            // 这个方法应该在游戏初始化时调用，连接到 AchievementSystem 的信号
            // 示例: AchievementSystem.Instance.OnAchievementUnlocked += HandleAchievementUnlocked;
        }

        /// <summary>
        /// 处理成就解锁回调
        /// </summary>
        public void HandleAchievementUnlocked(string achievementId)
        {
            TryUnlockFragment(achievementId);
        }

        #endregion

        #region Persistence

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["unlocked_fragment_ids"] = new List<string>(_unlockedFragmentIds);
            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            _unlockedFragmentIds.Clear();

            if (data.TryGetValue("unlocked_fragment_ids", out var idsObj) && idsObj is List<object> ids)
            {
                foreach (var id in ids)
                {
                    if (id is string fragmentId)
                    {
                        _unlockedFragmentIds.Add(fragmentId);
                        _database.SetFragmentUnlocked(fragmentId, true);
                    }
                }
            }

            GD.Print($"[InheritanceFragment] 从存档加载了 {_unlockedFragmentIds.Count} 个碎片");
        }

        #endregion

        #region Events

        public event FragmentUnlockedEventHandler OnFragmentUnlocked;

        #endregion
    }
}
