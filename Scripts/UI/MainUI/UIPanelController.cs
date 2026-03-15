using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.UI
{
    /// <summary>
    /// UI 面板控制器 - 处理单个面板的显示/隐藏逻辑
    /// </summary>
    public partial class UIPanelController : BaseSystem
    {
        /// <summary>
        /// 面板信息
        /// </summary>
        public class PanelInfo
        {
            public string Path { get; set; }
            public string Name { get; set; }
            public bool DefaultVisible { get; set; } = false;
        }
        
        private Node _mainNode;
        private Dictionary<string, PanelInfo> _panelRegistry = new Dictionary<string, PanelInfo>();
        
        public override void _Ready()
        {
            base._Ready();
            InitializePanelRegistry();
        }
        
        /// <summary>
        /// 初始化面板注册表
        /// </summary>
        private void InitializePanelRegistry()
        {
            _panelRegistry["RunesUI"] = new PanelInfo { Path = "CanvasLayer/RunesUI", Name = "Runes UI" };
            _panelRegistry["MeditationUI"] = new PanelInfo { Path = "CanvasLayer/MeditationUI", Name = "Meditation UI" };
            _panelRegistry["QuestTracker"] = new PanelInfo { Path = "CanvasLayer/QuestTracker", Name = "Quest Tracker" };
            _panelRegistry["QuestGuideUI"] = new PanelInfo { Path = "CanvasLayer/QuestGuideUI", Name = "Quest Guide" };
            _panelRegistry["MultiplayerLobbyUI"] = new PanelInfo { Path = "CanvasLayer/MultiplayerLobbyUI", Name = "Multiplayer UI" };
            _panelRegistry["CombatRatingUI"] = new PanelInfo { Path = "CanvasLayer/CombatRatingUI", Name = "Combat Rating UI" };
            _panelRegistry["WeaponMasteryUI"] = new PanelInfo { Path = "CanvasLayer/WeaponMasteryUI", Name = "Weapon Mastery UI" };
            _panelRegistry["CounterAttackUI"] = new PanelInfo { Path = "CanvasLayer/CounterAttackUI", Name = "Counter Attack UI" };
            _panelRegistry["MountUI"] = new PanelInfo { Path = "CanvasLayer/MountUI", Name = "Mount UI" };
            _panelRegistry["MountTrainingUI"] = new PanelInfo { Path = "CanvasLayer/MountTrainingUI", Name = "Mount Training UI" };
            _panelRegistry["SkillCooldownUI"] = new PanelInfo { Path = "CanvasLayer/SkillCooldownUI", Name = "Skill Cooldown UI" };
            _panelRegistry["SkillSynergyUI"] = new PanelInfo { Path = "CanvasLayer/SkillSynergyUI", Name = "Skill Synergy UI" };
            _panelRegistry["SkillTreeResetUI"] = new PanelInfo { Path = "CanvasLayer/SkillTreeResetUI", Name = "Skill Tree Reset UI" };
            _panelRegistry["SkillMasteryUI"] = new PanelInfo { Path = "CanvasLayer/SkillMasteryUI", Name = "Skill Mastery UI" };
            _panelRegistry["ConstellationUI"] = new PanelInfo { Path = "CanvasLayer/ConstellationUI", Name = "Constellation UI" };
            _panelRegistry["ProceduralStoryUI"] = new PanelInfo { Path = "CanvasLayer/ProceduralStoryUI", Name = "Procedural Story UI" };
            _panelRegistry["MomentumUI"] = new PanelInfo { Path = "CanvasLayer/MomentumUI", Name = "Momentum UI" };
            _panelRegistry["EnemyScalingUI"] = new PanelInfo { Path = "CanvasLayer/EnemyScalingUI", Name = "Enemy Scaling UI" };
            _panelRegistry["WeatherUI"] = new PanelInfo { Path = "CanvasLayer/WeatherUI", Name = "Weather UI" };
            _panelRegistry["ChoiceEventUI"] = new PanelInfo { Path = "CanvasLayer/ChoiceEventUI", Name = "Choice Event UI" };
            _panelRegistry["MusicCollectionUI"] = new PanelInfo { Path = "CanvasLayer/MusicCollectionUI", Name = "Music Collection UI" };
            _panelRegistry["GatheringUI"] = new PanelInfo { Path = "CanvasLayer/GatheringUI", Name = "Gathering UI" };
            _panelRegistry["MonsterTamingUI"] = new PanelInfo { Path = "CanvasLayer/MonsterTamingUI", Name = "Monster Taming UI" };
            _panelRegistry["DailyPuzzleUI"] = new PanelInfo { Path = "CanvasLayer/DailyPuzzleUI", Name = "Daily Puzzle UI" };
            _panelRegistry["PrestigeUI"] = new PanelInfo { Path = "CanvasLayer/PrestigeUI", Name = "Prestige UI" };
            _panelRegistry["IdentificationUI"] = new PanelInfo { Path = "CanvasLayer/IdentificationUI", Name = "Identification UI" };
            _panelRegistry["TitleUI"] = new PanelInfo { Path = "CanvasLayer/TitleUI", Name = "Title UI" };
            _panelRegistry["TitleCollectionUI"] = new PanelInfo { Path = "CanvasLayer/TitleCollectionUI", Name = "Title Collection UI" };
            _panelRegistry["BookmarkUI"] = new PanelInfo { Path = "CanvasLayer/BookmarkUI", Name = "Bookmark UI" };
            _panelRegistry["GuildUI"] = new PanelInfo { Path = "CanvasLayer/GuildUI", Name = "Guild UI" };
            _panelRegistry["TradeUI"] = new PanelInfo { Path = "CanvasLayer/TradeUI", Name = "Trade UI" };
            _panelRegistry["AuctionHouseUI"] = new PanelInfo { Path = "CanvasLayer/AuctionHouseUI", Name = "Auction House UI" };
            _panelRegistry["FriendListUI"] = new PanelInfo { Path = "CanvasLayer/FriendListUI", Name = "Friend List UI" };
            _panelRegistry["MailUI"] = new PanelInfo { Path = "CanvasLayer/MailUI", Name = "Mail UI" };
            _panelRegistry["SettingsUI"] = new PanelInfo { Path = "CanvasLayer/SettingsUI", Name = "Settings UI" };
            _panelRegistry["InventoryUI"] = new PanelInfo { Path = "CanvasLayer/InventoryUI", Name = "Inventory UI" };
            _panelRegistry["CharacterUI"] = new PanelInfo { Path = "CanvasLayer/CharacterUI", Name = "Character UI" };
            _panelRegistry["SkillTreeUI"] = new PanelInfo { Path = "CanvasLayer/SkillTreeUI", Name = "Skill Tree UI" };
            _panelRegistry["MapUI"] = new PanelInfo { Path = "CanvasLayer/MapUI", Name = "Map UI" };
            _panelRegistry["QuestLogUI"] = new PanelInfo { Path = "CanvasLayer/QuestLogUI", Name = "Quest Log UI" };
            _panelRegistry["WorldEventUI"] = new PanelInfo { Path = "CanvasLayer/WorldEventUI", Name = "World Event UI" };
            _panelRegistry["WorldBossUI"] = new PanelInfo { Path = "CanvasLayer/WorldBossUI", Name = "World Boss UI" };
            _panelRegistry["SealedTowerUI"] = new PanelInfo { Path = "CanvasLayer/SealedTowerUI", Name = "Sealed Tower UI" };
            _panelRegistry["ArenaTournamentUI"] = new PanelInfo { Path = "CanvasLayer/ArenaTournamentUI", Name = "Arena Tournament UI" };
            _panelRegistry["DailyChallengeUI"] = new PanelInfo { Path = "CanvasLayer/DailyChallengeUI", Name = "Daily Challenge UI" };
            _panelRegistry["RandomDungeonUI"] = new PanelInfo { Path = "CanvasLayer/RandomDungeonUI", Name = "Random Dungeon UI" };
            _panelRegistry["LeaderboardUI"] = new PanelInfo { Path = "CanvasLayer/LeaderboardUI", Name = "Leaderboard UI" };
            _panelRegistry["AchievementUI"] = new PanelInfo { Path = "CanvasLayer/AchievementUI", Name = "Achievement UI" };
            _panelRegistry["PetUI"] = new PanelInfo { Path = "CanvasLayer/PetUI", Name = "Pet UI" };
        }
        
        /// <summary>
        /// 设置主节点引用
        /// </summary>
        public void SetMainNode(Node mainNode)
        {
            _mainNode = mainNode;
        }
        
        /// <summary>
        /// 切换面板显示状态
        /// </summary>
        public void TogglePanel(string panelName)
        {
            if (!_panelRegistry.ContainsKey(panelName))
            {
                GD.Print($"[UIPanelController] Panel not found: {panelName}");
                return;
            }
            
            var panelInfo = _panelRegistry[panelName];
            var panel = _mainNode?.GetNodeOrNull<Control>(panelInfo.Path);
            
            if (panel != null)
            {
                panel.Visible = !panel.Visible;
                GD.Print($"{panelInfo.Name} toggled: " + panel.Visible);
            }
            else
            {
                // 尝试备选路径
                var altPanel = _mainNode?.GetNodeOrNull<Control>($"CanvasLayer/{panelName}");
                if (altPanel != null)
                {
                    altPanel.Visible = !altPanel.Visible;
                    GD.Print($"{panelInfo.Name} toggled: " + altPanel.Visible);
                }
            }
        }
        
        /// <summary>
        /// 显示面板
        /// </summary>
        public void ShowPanel(string panelName)
        {
            if (!_panelRegistry.ContainsKey(panelName))
                return;
            
            var panelInfo = _panelRegistry[panelName];
            var panel = _mainNode?.GetNodeOrNull<Control>(panelInfo.Path);
            
            if (panel != null)
            {
                panel.Visible = true;
            }
        }
        
        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void HidePanel(string panelName)
        {
            if (!_panelRegistry.ContainsKey(panelName))
                return;
            
            var panelInfo = _panelRegistry[panelName];
            var panel = _mainNode?.GetNodeOrNull<Control>(panelInfo.Path);
            
            if (panel != null)
            {
                panel.Visible = false;
            }
        }
        
        /// <summary>
        /// 获取面板可见状态
        /// </summary>
        public bool IsPanelVisible(string panelName)
        {
            if (!_panelRegistry.ContainsKey(panelName))
                return false;
            
            var panelInfo = _panelRegistry[panelName];
            var panel = _mainNode?.GetNodeOrNull<Control>(panelInfo.Path);
            
            return panel != null && panel.Visible;
        }
        
        /// <summary>
        /// 隐藏所有面板
        /// </summary>
        public void HideAllPanels()
        {
            foreach (var kvp in _panelRegistry)
            {
                HidePanel(kvp.Key);
            }
        }
        
        /// <summary>
        /// 注册新面板
        /// </summary>
        public void RegisterPanel(string name, string path, string displayName = "")
        {
            _panelRegistry[name] = new PanelInfo
            {
                Path = path,
                Name = string.IsNullOrEmpty(displayName) ? name : displayName
            };
        }
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            // 加载数据
        }
    }
}
