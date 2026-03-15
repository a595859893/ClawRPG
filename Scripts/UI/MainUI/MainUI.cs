using Godot;
using System;
using ClawRPG.UI;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// MainUI - 主 UI 控制器，协调各 UI 子系统
    /// </summary>
    public partial class MainUI : Node
    {
        private Main _main;
        private UIManager _uiManager;
        private UIPanelController _panelController;
        private UIStyles _uiStyles;
        
        public MainUI()
        {
        }
        
        public void Initialize(Main main)
        {
            _main = main;
            
            // 初始化 UI 管理器
            _uiManager = new UIManager();
            _uiManager.Initialize(main);
            
            // 初始化面板控制器
            _panelController = new UIPanelController();
            _panelController.SetMainNode(main);
            
            // 初始化 UI 样式
            _uiStyles = new UIStyles();
        }
        
        #region UI Toggle Methods
        
        /// <summary>
        /// Toggle Runes UI
        /// </summary>
        public void ToggleRunesUI() => _uiManager.ToggleUI("RunesUI");

        /// <summary>
        /// Toggle Meditation UI
        /// </summary>
        public void ToggleMeditationUI() => _uiManager.ToggleUI("MeditationUI");

        /// <summary>
        /// Toggle Quest Tracker
        /// </summary>
        public void ToggleQuestTracker() => _uiManager.ToggleUI("QuestTracker");

        /// <summary>
        /// Toggle Quest Guide
        /// </summary>
        public void ToggleQuestGuide() => _uiManager.ToggleUI("QuestGuideUI");

        /// <summary>
        /// Toggle Multiplayer UI
        /// </summary>
        public void ToggleMultiplayerUI() => _uiManager.ToggleUI("MultiplayerLobbyUI");

        /// <summary>
        /// Toggle Combat Rating UI
        /// </summary>
        public void ToggleCombatRatingUI() => _uiManager.ToggleUI("CombatRatingUI");

        /// <summary>
        /// Toggle Weapon Mastery UI
        /// </summary>
        public void ToggleWeaponMasteryUI() => _uiManager.ToggleUI("WeaponMasteryUI");

        /// <summary>
        /// Toggle Counter Attack UI
        /// </summary>
        public void ToggleCounterAttackUI() => _uiManager.ToggleUI("CounterAttackUI");

        /// <summary>
        /// Toggle Mount UI
        /// </summary>
        public void ToggleMountUI() => _uiManager.ToggleUI("MountUI");

        /// <summary>
        /// Toggle Mount Training UI
        /// </summary>
        public void ToggleMountTrainingUI() => _uiManager.ToggleUI("MountTrainingUI");

        /// <summary>
        /// Toggle Skill Cooldown UI
        /// </summary>
        public void ToggleSkillCooldownUI() => _uiManager.ToggleUI("SkillCooldownUI");

        /// <summary>
        /// Toggle Skill Synergy UI
        /// </summary>
        public void ToggleSkillSynergyUI() => _uiManager.ToggleUI("SkillSynergyUI");

        /// <summary>
        /// Toggle Skill Tree Reset UI
        /// </summary>
        public void ToggleSkillTreeResetUI() => _uiManager.ToggleUI("SkillTreeResetUI");

        /// <summary>
        /// Toggle Skill Mastery UI
        /// </summary>
        public void ToggleSkillMasteryUI() => _uiManager.ToggleUI("SkillMasteryUI");

        /// <summary>
        /// Toggle Constellation UI
        /// </summary>
        public void ToggleConstellationUI() => _uiManager.ToggleUI("ConstellationUI");

        /// <summary>
        /// Toggle Procedural Story UI
        /// </summary>
        public void ToggleProceduralStoryUI() => _uiManager.ToggleUI("ProceduralStoryUI");

        /// <summary>
        /// Toggle Momentum UI
        /// </summary>
        public void ToggleMomentumUI() => _uiManager.ToggleUI("MomentumUI");

        /// <summary>
        /// Toggle Enemy Scaling UI
        /// </summary>
        public void ToggleEnemyScalingUI() => _uiManager.ToggleUI("EnemyScalingUI");

        /// <summary>
        /// Toggle Weather UI
        /// </summary>
        public void ToggleWeatherUI() => _uiManager.ToggleUI("WeatherUI");

        /// <summary>
        /// Toggle Choice Event UI
        /// </summary>
        public void ToggleChoiceEventUI() => _uiManager.ToggleUI("ChoiceEventUI");

        /// <summary>
        /// Toggle Music Collection UI
        /// </summary>
        public void ToggleMusicCollectionUI() => _uiManager.ToggleUI("MusicCollectionUI");

        /// <summary>
        /// Toggle Gathering UI
        /// </summary>
        public void ToggleGatheringUI() => _uiManager.ToggleUI("GatheringUI");

        /// <summary>
        /// Toggle Monster Taming UI
        /// </summary>
        public void ToggleMonsterTamingUI() => _uiManager.ToggleUI("MonsterTamingUI");

        /// <summary>
        /// Toggle Daily Puzzle UI
        /// </summary>
        public void ToggleDailyPuzzleUI() => _uiManager.ToggleUI("DailyPuzzleUI");

        /// <summary>
        /// Toggle Prestige UI
        /// </summary>
        public void TogglePrestigeUI() => _uiManager.ToggleUI("PrestigeUI");

        /// <summary>
        /// Toggle Identification UI
        /// </summary>
        public void ToggleIdentificationUI() => _uiManager.ToggleUI("IdentificationUI");

        /// <summary>
        /// Toggle Title UI
        /// </summary>
        public void ToggleTitleUI() => _uiManager.ToggleUI("TitleUI");

        /// <summary>
        /// Toggle Title Collection UI
        /// </summary>
        public void ToggleTitleCollectionUI() => _uiManager.ToggleUI("TitleCollectionUI");

        /// <summary>
        /// Toggle Bookmark UI
        /// </summary>
        public void ToggleBookmarkUI() => _uiManager.ToggleUI("BookmarkUI");

        /// <summary>
        /// Toggle Guild UI
        /// </summary>
        public void ToggleGuildUI() => _uiManager.ToggleUI("GuildUI");

        /// <summary>
        /// Toggle Trade UI
        /// </summary>
        public void ToggleTradeUI() => _uiManager.ToggleUI("TradeUI");

        /// <summary>
        /// Toggle Auction House UI
        /// </summary>
        public void ToggleAuctionHouseUI() => _uiManager.ToggleUI("AuctionHouseUI");

        /// <summary>
        /// Toggle Friend List UI
        /// </summary>
        public void ToggleFriendListUI() => _uiManager.ToggleUI("FriendListUI");

        /// <summary>
        /// Toggle Mail UI
        /// </summary>
        public void ToggleMailUI() => _uiManager.ToggleUI("MailUI");

        /// <summary>
        /// Toggle Settings UI
        /// </summary>
        public void ToggleSettingsUI() => _uiManager.ToggleUI("SettingsUI");

        /// <summary>
        /// Toggle Inventory UI
        /// </summary>
        public void ToggleInventoryUI() => _uiManager.ToggleUI("InventoryUI");

        /// <summary>
        /// Toggle Character UI
        /// </summary>
        public void ToggleCharacterUI() => _uiManager.ToggleUI("CharacterUI");

        /// <summary>
        /// Toggle Skill Tree UI
        /// </summary>
        public void ToggleSkillTreeUI() => _uiManager.ToggleUI("SkillTreeUI");

        /// <summary>
        /// Toggle Map UI
        /// </summary>
        public void ToggleMapUI() => _uiManager.ToggleUI("MapUI");

        /// <summary>
        /// Toggle Quest Log UI
        /// </summary>
        public void ToggleQuestLogUI() => _uiManager.ToggleUI("QuestLogUI");

        /// <summary>
        /// Toggle World Event UI
        /// </summary>
        public void ToggleWorldEventUI() => _uiManager.ToggleUI("WorldEventUI");

        /// <summary>
        /// Toggle World Boss UI
        /// </summary>
        public void ToggleWorldBossUI() => _uiManager.ToggleUI("WorldBossUI");

        /// <summary>
        /// Toggle Sealed Tower UI
        /// </summary>
        public void ToggleSealedTowerUI() => _uiManager.ToggleUI("SealedTowerUI");

        /// <summary>
        /// Toggle Arena Tournament UI
        /// </summary>
        public void ToggleArenaTournamentUI() => _uiManager.ToggleUI("ArenaTournamentUI");

        /// <summary>
        /// Toggle Daily Challenge UI
        /// </summary>
        public void ToggleDailyChallengeUI() => _uiManager.ToggleUI("DailyChallengeUI");

        /// <summary>
        /// Toggle Random Dungeon UI
        /// </summary>
        public void ToggleRandomDungeonUI() => _uiManager.ToggleUI("RandomDungeonUI");

        /// <summary>
        /// Toggle Leaderboard UI
        /// </summary>
        public void ToggleLeaderboardUI() => _uiManager.ToggleUI("LeaderboardUI");

        /// <summary>
        /// Toggle Achievement UI
        /// </summary>
        public void ToggleAchievementUI() => _uiManager.ToggleUI("AchievementUI");

        /// <summary>
        /// Toggle Pet UI
        /// </summary>
        public void TogglePetUI() => _uiManager.ToggleUI("PetUI");
        
        #endregion
        
        #region Style Methods
        
        /// <summary>
        /// 设置 UI 主题
        /// </summary>
        public void SetTheme(UIStyles.ThemeType theme)
        {
            _uiStyles.SetTheme(theme);
        }
        
        /// <summary>
        /// 获取当前主题
        /// </summary>
        public UIStyles.ThemeType GetCurrentTheme()
        {
            return _uiStyles.GetCurrentTheme();
        }
        
        /// <summary>
        /// 获取主色
        /// </summary>
        public Color GetPrimaryColor()
        {
            return _uiStyles.GetPrimaryColor();
        }
        
        #endregion
    }
}
