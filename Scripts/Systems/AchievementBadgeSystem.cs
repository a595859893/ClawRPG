using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// Achievement badge resource - represents a visual badge reward for achievements
    /// </summary>
    [GlobalClass]
    public partial class AchievementBadge : Resource
    {
        [Export] public string BadgeId { get; set; }
        [Export] public string DisplayName { get; set; }
        [Export] public string Description { get; set; }
        [Export] public Color BadgeColor { get; set; } = Colors.Gold;
        [Export] public string IconName { get; set; }
        [Export] public int Tier { get; set; } = 1; // 1=铜, 2=银, 3=金, 4=钻石
        [Export] public bool IsSecret { get; set; }= false; 
        [Export] public Vector2 Position { get; set; } = Vector2.Zero;
    }

    /// <summary>
    /// Achievement badge system - manages achievement badge rewards and display
    /// </summary>
    public partial class AchievementBadgeSystem : BaseSystem
    {
        public static AchievementBadgeSystem Instance { get; private set; }

        private Dictionary<string, AchievementBadge> _badges = new();
        private Dictionary<string, string> _achievementToBadgeMap = new();

        protected override void Initialize()
        {
            Instance = this;
            InitializeBadges();
        }

        public override void _Ready()
        {
            base._Ready();
        }

        private void InitializeBadges()
        {
            // 铜色徽章 (Tier 1)
            RegisterBadge(new AchievementBadge {
                BadgeId = "first_blood", DisplayName = "初战告捷", Description = "击败第一个敌人",
                BadgeColor = new Color(0.8f, 0.5f, 0.2f), IconName = "sword", Tier = 1,
            });
            RegisterBadge(new AchievementBadge {
                BadgeId = "collector", DisplayName = "收藏家", Description = "收集10件物品",
                BadgeColor = new Color(0.8f, 0.5f, 0.2f), IconName = "chest", Tier = 1,
            });
            RegisterBadge(new AchievementBadge {
                BadgeId = "explorer", DisplayName = "探索者", Description = "发现5个区域",
                BadgeColor = new Color(0.8f, 0.5f, 0.2f), IconName = "map", Tier = 1,
            });

            // 银色徽章 (Tier 2)
            RegisterBadge(new AchievementBadge {
                BadgeId = "warrior", DisplayName = "战士", Description = "击败100个敌人",
                BadgeColor = new Color(0.75f, 0.75f, 0.75f), IconName = "shield", Tier = 2
            });
            RegisterBadge(new AchievementBadge {
                BadgeId = "wealthy", DisplayName = "富翁", Description = "拥有10000金币",
                BadgeColor = new Color(0.75f, 0.75f, 0.75f), IconName = "coin", Tier = 2
            });
            RegisterBadge(new AchievementBadge {
                BadgeId = "team_player", DisplayName = "团队玩家", Description = "完成10次组队",
                BadgeColor = new Color(0.75f, 0.75f, 0.75f), IconName = "users", Tier = 2
            });

            // 金色徽章 (Tier 3)
            RegisterBadge(new AchievementBadge {
                BadgeId = "boss_slayer", DisplayName = "Boss杀手", Description = "击败10个Boss",
                BadgeColor = new Color(1f, 0.84f, 0f), IconName = "crown", Tier = 3
            });
            RegisterBadge(new AchievementBadge {
                BadgeId = "master_crafter", DisplayName = "大师工匠", Description = "合成50件装备",
                BadgeColor = new Color(1f, 0.84f, 0f), IconName = "hammer", Tier = 3
            });
            RegisterBadge(new AchievementBadge {
                BadgeId = "legend", DisplayName = "传奇", Description = "达到满级",
                BadgeColor = new Color(1f, 0.84f, 0f), IconName = "star", Tier = 3
            });

            // 钻石徽章 (Tier 4)
            RegisterBadge(new AchievementBadge {
                BadgeId = "champion", DisplayName = "冠军", Description = "通关游戏",
                BadgeColor = new Color(0f, 0.8f, 1f), IconName = "trophy", Tier = 4
            });
            RegisterBadge(new AchievementBadge {
                BadgeId = "perfectionist", DisplayName = "完美主义者", Description = "完成所有成就",
                BadgeColor = new Color(0f, 0.8f, 1f), IconName = "gem", Tier = 4, IsSecret = true
            });

            // 建立成就到徽章的映射
            _achievementToBadgeMap["kill_first_enemy"] = "first_blood";
            _achievementToBadgeMap["collect_10_items"] = "collector";
            _achievementToBadgeMap["discover_5_regions"] = "explorer";
            _achievementToBadgeMap["kill_100_enemies"] = "warrior";
            _achievementToBadgeMap["have_10000_gold"] = "wealthy";
            _achievementToBadgeMap["complete_10_teams"] = "team_player";
            _achievementToBadgeMap["kill_10_bosses"] = "boss_slayer";
            _achievementToBadgeMap["craft_50_equipment"] = "master_crafter";
            _achievementToBadgeMap["reach_max_level"] = "legend";
            _achievementToBadgeMap["complete_game"] = "champion";
            _achievementToBadgeMap["all_achievements"] = "perfectionist";
        }

        private void RegisterBadge(AchievementBadge badge)
        {
            _badges[badge.BadgeId] = badge;
        }

        public AchievementBadge GetBadge(string badgeId)
        {
            return _badges.ContainsKey(badgeId) ? _badges[badgeId] : null;
        }

        public AchievementBadge GetBadgeForAchievement(string achievementId)
        {
            if (_achievementToBadgeMap.ContainsKey(achievementId))
                return GetBadge(_achievementToBadgeMap[achievementId]);
            return null;
        }

        public List<AchievementBadge> GetAllBadges() => new List<AchievementBadge>(_badges.Values);

        public List<AchievementBadge> GetBadgesByTier(int tier)
        {
            var result = new List<AchievementBadge>();
            foreach (var badge in _badges.Values)
            {
                if (badge.Tier == tier) result.Add(badge);
            }
            return result;
        }

        // 解锁状态
        private HashSet<string> _unlockedBadges = new HashSet<string>();

        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            var unlocked = new Array();
            foreach (var badgeId in _unlockedBadges)
            {
                unlocked.Add(badgeId);
            }
            data["unlocked_badges"] = unlocked;
            return data;
        }

        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            if (data.Contains("unlocked_badges"))
            {
                _unlockedBadges.Clear();
                var unlocked = (Array)data["unlocked_badges"];
                foreach (var badgeId in unlocked)
                {
                    _unlockedBadges.Add((string)badgeId);
                }
            }
        }

        /// <summary>
        /// 解锁徽章
        /// </summary>
        public void UnlockBadge(string badgeId)
        {
            if (_badges.ContainsKey(badgeId) && !_unlockedBadges.Contains(badgeId))
            {
                _unlockedBadges.Add(badgeId);
                GD.Print($"[AchievementBadgeSystem] Badge unlocked: {badgeId}");
            }
        }

        /// <summary>
        /// 检查徽章是否已解锁
        /// </summary>
        public bool IsBadgeUnlocked(string badgeId)
        {
            return _unlockedBadges.Contains(badgeId);
        }
    }
}
