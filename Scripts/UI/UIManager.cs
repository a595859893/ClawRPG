using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// UI 管理器 - 负责所有游戏 UI 的创建、显示和隐藏
    /// </summary>
    public class UIManager : ManagerBase
    {
        public static UIManager Instance { get; private set; }
        
        /// <summary>
        /// 所有 UI 实例的缓存
        /// </summary>
        private Dictionary<string, Control> _uiCache = new Dictionary<string, Control>();
        
        /// <summary>
        /// CanvasLayer 引用
        /// </summary>
        private CanvasLayer _canvasLayer;
        
        /// <summary>
        /// UI 根节点
        /// </summary>
        private Control _uiRoot;
        
        /// <summary>
        /// UI 初始化完成事件
        /// </summary>
        public event Action OnUIInitialized;
        
        public override void _Ready()
        {
            Instance = this;
            base._Ready();
        }
        
        protected override void Initialize()
        {
            GD.Print("[UIManager] Starting UI initialization...");
            
            // 创建 CanvasLayer
            _canvasLayer = new CanvasLayer();
            _canvasLayer.Name = "UI";
            GetParent().AddChild(_canvasLayer);
            
            // 创建 UI 根节点
            _uiRoot = new Control();
            _uiRoot.Name = "UIRoot";
            _uiRoot.Set AnchorsPreset(Control.LayoutPreset.FullRect);
            _canvasLayer.AddChild(_uiRoot);
            
            // 初始化基础 UI
            InitializeBasicUI();
            
            // 初始化所有其他 UI
            InitializeAllUI();
            
            GD.Print("[UIManager] UI initialization complete");
            NotifyInitialized();
            OnUIInitialized?.Invoke();
        }
        
        /// <summary>
        /// 初始化基础 UI 元素
        /// </summary>
        private void InitializeBasicUI()
        {
            // Health bar
            var healthBar = new ProgressBar
            {
                Name = "HealthBar",
                Position = new Vector2(20, 20),
                Size = new Vector2(200, 20),
                Value = 100,
                MaxValue = 100
            };
            _uiRoot.AddChild(healthBar);
            
            // Mana bar
            var manaBar = new ProgressBar
            {
                Name = "ManaBar",
                Position = new Vector2(20, 45),
                Size = new Vector2(200, 20),
                Value = 50,
                MaxValue = 50
            };
            _uiRoot.AddChild(manaBar);
            
            // Level display
            var levelLabel = new Label
            {
                Name = "LevelLabel",
                Position = new Vector2(230, 20),
                Text = "Lv.1"
            };
            _uiRoot.AddChild(levelLabel);
            
            // Experience bar
            var expBar = new ProgressBar
            {
                Name = "ExpBar",
                Position = new Vector2(20, 70),
                Size = new Vector2(200, 10),
                Value = 0,
                MaxValue = 100
            };
            _uiRoot.AddChild(expBar);
        }
        
        /// <summary>
        /// 初始化所有 UI
        /// </summary>
        private void InitializeAllUI()
        {
            // 基础 UI
            AddUI<PotionUI>("PotionUI");
            AddUI<EnchantmentUI>("EnchantmentUI");
            AddUI<ProceduralDungeonUI>("ProceduralDungeonUI");
            AddUI<MythicPlusDungeonUI>("MythicPlusDungeonUI");
            AddUI<MountTrainingUI>("MountTrainingUI");
            AddUI<DynamicScreenEffect>("DynamicScreenEffect");
            AddUI<ComboDisplayUI>("ComboDisplayUI");
            AddUI<HitStopUI>("HitStopUI");
            AddUI<CombatStatsPanel>("CombatStatsPanel");
            AddUI<MomentumUI>("MomentumUI");
            AddUI<SkillComboUI>("SkillComboUI");
            AddUI<SkillTreeUI>("SkillTreeUI");
            AddUI<SkillTreeResetUI>("SkillTreeResetUI");
            AddUI<SkillSynergyUI>("SkillSynergyUI");
            AddUI<TradeRouteUI>("TradeRouteUI");
            AddUI<MarketTrendUI>("MarketTrendUI");
            AddUI<ProceduralStoryUI>("ProceduralStoryUI");
            AddUI<QuickSlotUI>("QuickSlotUI");
            AddUI<CombatHUDEnhancementUI>("CombatHUDEnhancementUI");
            AddUI<CombatSkillCooldownUI>("CombatSkillCooldownUI");
            AddUI<CombatStatusUI>("CombatStatusUI");
            AddUI<CombatVFXUI>("CombatVFXUI");
            AddUI<PlayerProfileUI>("PlayerProfileUI");
            AddUI<ArtifactUI>("ArtifactUI");
            AddUI<ArtifactFusionUI>("ArtifactFusionUI");
            AddUI<SoulBondUI>("SoulBondUI");
            AddUI<WeatherUI>("WeatherUI");
            AddUI<LeaderboardUI>("LeaderboardUI");
            AddUI<DialogueUI>("DialogueUI");
            AddUI<StoryUI>("StoryUI");
            AddUI<SealedTowerUI>("SealedTowerUI");
            AddUI<CraftingMasteryUI>("CraftingMasteryUI");
            AddUI<HotkeyHUD>("HotkeyHUD");
            AddUI<EquipmentSetUI>("EquipmentSetUI");
            AddUI<FactionUI>("FactionUI");
            AddUI<KeybindingUI>("KeybindingUI");
            AddUI<AccessibilityUI>("AccessibilityUI");
            AddUI<CounterAttackUI>("CounterAttackUI");
            AddUI<BossHealthBarUI>("BossHealthBarUI");
            AddUI<TutorialUI>("TutorialUI");
            AddUI<BalanceUI>("BalanceUI");
            AddUI<ReputationUI>("ReputationUI");
            AddUI<TeamSkillUI>("TeamSkillUI");
            AddUI<ShopUI>("ShopUI");
            AddUI<FishingUI>("FishingUI");
            AddUI<ParallelDimensionUI>("ParallelDimensionUI");
            AddUI<AlchemyUI>("AlchemyUI");
            AddUI<AlchemyLaboratoryUI>("AlchemyLaboratoryUI");
            AddUI<CookingUI>("CookingUI");
            AddUI<MountCombatUI>("MountCombatUI");
            AddUI<MountEvolutionUI>("MountEvolutionUI");
            AddUI<MountEquipmentUI>("MountEquipmentUI");
            AddUI<WorldEventUI>("WorldEventUI");
            AddUI<TitleUI>("TitleUI");
            AddUI<PlayerTalentUI>("PlayerTalentUI");
            AddUI<MountRaceUI>("MountRaceUI");
            AddUI<MountBattleArenaUI>("MountBattleArenaUI");
            AddUI<MountWeatherBonusUI>("MountWeatherBonusUI");
            AddUI<GuildUI>("GuildUI");
            AddUI<GuildQuestUI>("GuildQuestUI");
            AddUI<GuildBankUI>("GuildBankUI");
            AddUI<GuildTechnologyUI>("GuildTechnologyUI");
            AddUI<GuildWarLeagueUI>("GuildWarLeagueUI");
            AddUI<MultiplayerLeaderboardUI>("MultiplayerLeaderboardUI");
            AddUI<TradeUI>("TradeUI");
            AddUI<DailyLoginRewardUI>("DailyLoginRewardUI");
            AddUI<GemUI>("GemUI");
            AddUI<GemFusionUI>("GemFusionUI");
            AddUI<CostumeUI>("CostumeUI");
            AddUI<RelicUI>("RelicUI");
            AddUI<EquipmentEnhancementUI>("EquipmentEnhancementUI");
            AddUI<PetEquipmentUI>("PetEquipmentUI");
            AddUI<PetEquipmentEnhancementUI>("PetEquipmentEnhancementUI");
            AddUI<PetEvolutionUI>("PetEvolutionUI");
            AddUI<PetTalentUI>("PetTalentUI");
            AddUI<PetBreedingUI>("PetBreedingUI");
            AddUI<PetAffectionUI>("PetAffectionUI");
            AddUI<PetInteractionUI>("PetInteractionUI");
            AddUI<PetAIImprovementsUI>("PetAIImprovementsUI");
            AddUI<PetRecycleUI>("PetRecycleUI");
            AddUI<PetInventoryUI>("PetInventoryUI");
            AddUI<EliteMonsterUI>("EliteMonsterUI");
            AddUI<PetFosterUI>("PetFosterUI");
            AddUI<PetSkillUI>("PetSkillUI");
            AddUI<PetExpeditionUI>("PetExpeditionUI");
            AddUI<MountExpeditionUI>("MountExpeditionUI");
            AddUI<MysteryTreasureUI>("MysteryTreasureUI");
            AddUI<RankedUI>("RankedUI");
            AddUI<DynamicDifficultyUI>("DynamicDifficultyUI");
            AddUI<WorldBossUI>("WorldBossUI");
            AddUI<BossRushUI>("BossRushUI");
            AddUI<ChoiceEventUI>("ChoiceEventUI");
            AddUI<ElementalTrialUI>("ElementalTrialUI");
            AddUI<PetBattleArenaUI>("PetBattleArenaUI");
            AddUI<PetMorphUI>("PetMorphUI");
            AddUI<DailyDungeonUI>("DailyDungeonUI");
            AddUI<RandomBoonUI>("RandomBoonUI");
            AddUI<DailyQuestUI>("DailyQuestUI");
            AddUI<ProceduralChallengeUI>("ProceduralChallengeUI");
            AddUI<DynamicQuestChallengeUI>("DynamicQuestChallengeUI");
            AddUI<LootDropUI>("LootDropUI");
            AddUI<EquipmentDurabilityUI>("EquipmentDurabilityUI");
            AddUI<EquipmentRecycleUI>("EquipmentRecycleUI");
            AddUI<BuffUI>("BuffUI");
            AddUI<BossMechanicsUI>("BossMechanicsUI");
            AddUI<RuneUI>("RuneUI");
            AddUI<MusicCollectionUI>("MusicCollectionUI");
            AddUI<GatheringUI>("GatheringUI");
            AddUI<MonsterTamingUI>("MonsterTamingUI");
            AddUI<DailyPuzzleUI>("DailyPuzzleUI");
            AddUI<PrestigeUI>("PrestigeUI");
            AddUI<IdentificationUI>("IdentificationUI");
            AddUI<SurvivalChallengeUI>("SurvivalChallengeUI");
            AddUI<ArenaColosseumUI>("ArenaColosseumUI");
            AddUI<PartyUI>("PartyUI");
            AddUI<EmoteUI>("EmoteUI");
            AddUI<EconomicDashboardUI>("EconomicDashboardUI");
            AddUI<SeededRunUI>("SeededRunUI");
            AddUI<CombatEffectOverlayUI>("CombatEffectOverlayUI");
            AddUI<DreamscapeUI>("DreamscapeUI");
            AddUI<EnemyWeaknessUI>("EnemyWeaknessUI");
            AddUI<ItemSmeltingUI>("ItemSmeltingUI");
            AddUI<EnemyWeaknessUI>("EnemyWeaknessUI");
            AddUI<EnemyScalingUI>("EnemyScalingUI");
            AddUI<ConstellationUI>("ConstellationUI");
            AddUI<ProceduralStoryUI>("ProceduralStoryUI");
            AddUI<SkillMasteryUI>("SkillMasteryUI");
            AddUI<AchievementMilestoneUI>("AchievementMilestoneUI");
            AddUI<SealedDungeonUI>("SealedDungeonUI");
            AddUI<GuildTournamentBracketUI>("GuildTournamentBracketUI");
            AddUI<ContractBountyUI>("ContractBountyUI");
            AddUI<QuestTrackerUI>("QuestTrackerUI");
            AddUI<QuestGuideArrow>("QuestGuideArrow");
            AddUI<MultiplayerUI>("MultiplayerUI");
            AddUI<WeaponMasteryUI>("WeaponMasteryUI");
            AddUI<MountUI>("MountUI");
            AddUI<BookmarkUI>("BookmarkUI");
            AddUI<AutoBookmarkUI>("AutoBookmarkUI");
            AddUI<EnhancementUI>("EnhancementUI");
            AddUI<AutoPotionUI>("AutoPotionUI");
            AddUI<PetStoryUI>("PetStoryUI");
            AddUI<PetHabitatUI>("PetHabitatUI");
            AddUI<PetEggUI>("PetEggUI");
            AddUI<PetFriendshipUI>("PetFriendshipUI");
            AddUI<PetSynthesisUI>("PetSynthesisUI");
            AddUI<PetGeneticsUI>("PetGeneticsUI");
            AddUI<PetFusionUI>("PetFusionUI");
            
            // 系统 UI
            AddSystemUI<Systems.SealedTowerManager>("SealedTowerUI");
            AddSystemUI<Systems.AchievementManager>("AchievementUI");
            AddSystemUI<Systems.QuestSystem>("QuestUI");
            
            GD.Print($"[UIManager] Initialized {_uiCache.Count} UI elements");
        }
        
        /// <summary>
        /// 添加 UI 实例
        /// </summary>
        private void AddUI<T>(string uiName) where T : Control, new()
        {
            try
            {
                var ui = new T
                {
                    Name = uiName,
                    Visible = false
                };
                _uiRoot.AddChild(ui);
                _uiCache[uiName] = ui;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[UIManager] Failed to add UI {uiName}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 添加系统自带的 UI
        /// </summary>
        private void AddSystemUI<T>(string uiName)
        {
            // 系统 UI 由系统本身创建，这里只是占位
        }
        
        /// <summary>
        /// 显示 UI
        /// </summary>
        public void ShowUI(string uiName)
        {
            if (_uiCache.TryGetValue(uiName, out var ui))
            {
                ui.Visible = true;
            }
        }
        
        /// <summary>
        /// 隐藏 UI
        /// </summary>
        public void HideUI(string uiName)
        {
            if (_uiCache.TryGetValue(uiName, out var ui))
            {
                ui.Visible = false;
            }
        }
        
        /// <summary>
        /// 切换 UI 显示状态
        /// </summary>
        public void ToggleUI(string uiName)
        {
            if (_uiCache.TryGetValue(uiName, out var ui))
            {
                ui.Visible = !ui.Visible;
            }
        }
        
        /// <summary>
        /// 获取 UI 实例
        /// </summary>
        public T GetUI<T>(string uiName) where T : Control
        {
            if (_uiCache.TryGetValue(uiName, out var ui))
            {
                return ui as T;
            }
            return null;
        }
        
        /// <summary>
        /// 获取 CanvasLayer
        /// </summary>
        public CanvasLayer GetCanvasLayer() => _canvasLayer;
        
        /// <summary>
        /// 获取 UI 根节点
        /// </summary>
        public Control GetUIRoot() => _uiRoot;
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            // 保存可见的 UI 状态
            var visibleUIs = new List<string>();
            foreach (var kvp in _uiCache)
            {
                if (kvp.Value.Visible)
                {
                    visibleUIs.Add(kvp.Key);
                }
            }
            data["visibleUIs"] = visibleUIs;
            
            return data;
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null || !data.Contains("visibleUIs")) return;
            
            var visibleUIs = data["visibleUIs"] as List<string>;
            if (visibleUIs == null) return;
            
            foreach (var uiName in visibleUIs)
            {
                ShowUI(uiName);
            }
        }
    }
}
