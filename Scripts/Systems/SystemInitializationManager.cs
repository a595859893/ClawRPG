using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Managers;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 系统初始化管理器 - 负责游戏启动时所有系统的初始化
    /// 管理器的初始化顺序通过 Priority 属性控制
    /// </summary>
    public class SystemInitializationManager : ManagerBase
    {
        public static SystemInitializationManager Instance { get; private set; }
        
        /// <summary>
        /// 所有需要初始化的系统类型
        /// </summary>
        private List<Type> _systemTypes = new List<Type>();
        
        /// <summary>
        /// 初始化顺序分组
        /// </summary>
        private Dictionary<string, List<Type>> _initializationGroups = new Dictionary<string, List<Type>>();
        
        /// <summary>
        /// 是否初始化完成
        /// </summary>
        public bool IsInitializationComplete { get; private set; } = false;
        
        /// <summary>
        /// 初始化完成事件
        /// </summary>
        public event Action OnAllSystemsInitialized;
        
        public override void _Ready()
        {
            Instance = this;
            base._Ready();
        }
        
        protected override void Initialize()
        {
            GD.Print("[SystemInitializationManager] Starting system initialization...");
            
            var startTime = DateTime.Now;
            
            // 注册所有系统
            RegisterAllSystems();
            
            // 按优先级顺序初始化系统
            InitializeSystemsByPriority();
            
            var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
            GD.Print($"[SystemInitializationManager] All systems initialized in {elapsed}ms");
            
            IsInitializationComplete = true;
            NotifyInitialized();
            OnAllSystemsInitialized?.Invoke();
        }
        
        /// <summary>
        /// 注册所有需要初始化的系统
        /// </summary>
        private void RegisterAllSystems()
        {
            // 核心系统 - 最高优先级
            RegisterSystemInGroup("core", typeof(WeaponMasterySystem));
            RegisterSystemInGroup("core", typeof(TitleSystem));
            RegisterSystemInGroup("core", typeof(TitleCollectionSystem));
            RegisterSystemInGroup("core", typeof(PetCombatAI));
            RegisterSystemInGroup("core", typeof(EnhancementSystem));
            RegisterSystemInGroup("core", typeof(EnhancementDatabase));
            RegisterSystemInGroup("core", typeof(AutoPotionSystem));
            RegisterSystemInGroup("core", typeof(AutoBookmarkSystem));
            RegisterSystemInGroup("core", typeof(AchievementBadgeSystem));
            RegisterSystemInGroup("core", typeof(SecretAchievementSystem));
            RegisterSystemInGroup("core", typeof(FactionSystem));
            RegisterSystemInGroup("core", typeof(PlayerProfileSystem));
            RegisterSystemInGroup("core", typeof(ArtifactSystem));
            RegisterSystemInGroup("core", typeof(WeatherSystem));
            RegisterSystemInGroup("core", typeof(CounterAttackSystem));
            RegisterSystemInGroup("core", typeof(EquipmentSetSystem));
            RegisterSystemInGroup("core", typeof(EquipmentSetDatabase));
            RegisterSystemInGroup("core", typeof(EnchantmentDatabase));
            RegisterSystemInGroup("core", typeof(MountTrainingSystem));
            RegisterSystemInGroup("core", typeof(MountTrainingDatabase));
            
            // 战斗系统
            RegisterSystemInGroup("combat", typeof(ArtifactFusionDatabase));
            RegisterSystemInGroup("combat", typeof(ArtifactFusionSystem));
            RegisterSystemInGroup("combat", typeof(BountyManager));
            RegisterSystemInGroup("combat", typeof(MailManager));
            RegisterSystemInGroup("combat", typeof(DailyLoginRewardSystem));
            RegisterSystemInGroup("combat", typeof(MeditationSystem));
            RegisterSystemInGroup("combat", typeof(MeditationDatabase));
            RegisterSystemInGroup("combat", typeof(EnchantmentSystem));
            RegisterSystemInGroup("combat", typeof(BossMechanicsSystem));
            RegisterSystemInGroup("combat", typeof(ProceduralDungeonSystem));
            RegisterSystemInGroup("combat", typeof(ArenaTournamentSystem));
            RegisterSystemInGroup("combat", typeof(MythicPlusDungeonSystem));
            RegisterSystemInGroup("combat", typeof(GuildWarSystem));
            RegisterSystemInGroup("combat", typeof(GuildHeritageSystem));
            RegisterSystemInGroup("combat", typeof(DreamscapeSystem));
            RegisterSystemInGroup("combat", typeof(DreamscapeDatabase));
            RegisterSystemInGroup("combat", typeof(LeaderboardSystem));
            RegisterSystemInGroup("combat", typeof(LeaderboardDatabase));
            RegisterSystemInGroup("combat", typeof(PartySystem));
            RegisterSystemInGroup("combat", typeof(CombatUISystem));
            RegisterSystemInGroup("combat", typeof(DailyRitualSystem));
            RegisterSystemInGroup("combat", typeof(WeeklyChallengeSystem));
            RegisterSystemInGroup("combat", typeof(CompanionInteractionSystem));
            RegisterSystemInGroup("combat", typeof(CameraEffectSystem));
            RegisterSystemInGroup("combat", typeof(ComboSystem));
            RegisterSystemInGroup("combat", typeof(ComboChainData));
            RegisterSystemInGroup("combat", typeof(ComboChainDatabase));
            RegisterSystemInGroup("combat", typeof(ComboChainSystem));
            RegisterSystemInGroup("combat", typeof(MomentumSystem));
            RegisterSystemInGroup("combat", typeof(EnemyScalingSystem));
            RegisterSystemInGroup("combat", typeof(SkillComboSystem));
            RegisterSystemInGroup("combat", typeof(SkillTreeSystem));
            RegisterSystemInGroup("combat", typeof(SkillTreeResetSystem));
            RegisterSystemInGroup("combat", typeof(SkillSynergySystem));
            RegisterSystemInGroup("combat", typeof(SoulBondSystem));
            RegisterSystemInGroup("combat", typeof(ConstellationSystem));
            RegisterSystemInGroup("combat", typeof(ProceduralStoryData));
            RegisterSystemInGroup("combat", typeof(ProceduralStoryDatabase));
            RegisterSystemInGroup("combat", typeof(ProceduralStorySystem));
            
            // 视觉效果系统
            RegisterSystemInGroup("vfx", typeof(AOEIndicatorManager));
            RegisterSystemInGroup("vfx", typeof(AnimationEffectManager));
            RegisterSystemInGroup("vfx", typeof(DialogueManager));
            RegisterSystemInGroup("vfx", typeof(StoryManager));
            RegisterSystemInGroup("vfx", typeof(SealedTowerManager));
            RegisterSystemInGroup("vfx", typeof(CraftingMasterySystem));
            RegisterSystemInGroup("vfx", typeof(TreasureHuntManager));
            RegisterSystemInGroup("vfx", typeof(RegionManager));
            RegisterSystemInGroup("vfx", typeof(SoundEffectSystem));
            RegisterSystemInGroup("vfx", typeof(BackgroundMusicSystem));
            RegisterSystemInGroup("vfx", typeof(MusicCollectionSystem));
            RegisterSystemInGroup("vfx", typeof(GatheringSystem));
            RegisterSystemInGroup("vfx", typeof(MonsterTamingSystem));
            RegisterSystemInGroup("vfx", typeof(PrestigeSystem));
            RegisterSystemInGroup("vfx", typeof(DiceMasterSystem));
            RegisterSystemInGroup("vfx", typeof(IdentificationSystem));
            RegisterSystemInGroup("vfx", typeof(AlchemyLaboratorySystem));
            RegisterSystemInGroup("vfx", typeof(MultiplayerLeaderboard));
            RegisterSystemInGroup("vfx", typeof(NetworkQualityUI));
            RegisterSystemInGroup("vfx", typeof(EquipmentSetManager));
            RegisterSystemInGroup("vfx", typeof(ProceduralEquipmentSystem));
            RegisterSystemInGroup("vfx", typeof(BossAbilityVisualizer));
            RegisterSystemInGroup("vfx", typeof(BossAbilityWarningUI));
            RegisterSystemInGroup("vfx", typeof(CounterAttackVFX));
            RegisterSystemInGroup("vfx", typeof(BalanceManager));
            RegisterSystemInGroup("vfx", typeof(ScreenEffectManager));
            RegisterSystemInGroup("vfx", typeof(CombatVFXSystem));
            RegisterSystemInGroup("vfx", typeof(TeamSkillSystem));
            RegisterSystemInGroup("vfx", typeof(SkillMasterySystem));
            RegisterSystemInGroup("vfx", typeof(LootDropSystem));
            RegisterSystemInGroup("vfx", typeof(MysteryTreasureSystem));
            RegisterSystemInGroup("vfx", typeof(RankedSystem));
            RegisterSystemInGroup("vfx", typeof(ElementalReactionManager));
            RegisterSystemInGroup("vfx", typeof(ElementalDamageManager));
            RegisterSystemInGroup("vfx", typeof(ElementalSkillManager));
            RegisterSystemInGroup("vfx", typeof(ParticleEffectManager));
            
            // 经济系统
            RegisterSystemInGroup("economy", typeof(ShopSystem));
            RegisterSystemInGroup("economy", typeof(AuctionHouseSystem));
            RegisterSystemInGroup("economy", typeof(DynamicMarketTaxSystem));
            RegisterSystemInGroup("economy", typeof(EconomicWarningSystem));
            
            // 坐骑系统
            RegisterSystemInGroup("mount", typeof(MountCombatSystem));
            RegisterSystemInGroup("mount", typeof(MountBattleArenaSystem));
            RegisterSystemInGroup("mount", typeof(MountEquipmentSystem));
            RegisterSystemInGroup("mount", typeof(MountEvolutionSystem));
            RegisterSystemInGroup("mount", typeof(MountWeatherBonusSystem));
            RegisterSystemInGroup("mount", typeof(MountExpeditionSystem));
            
            // 世界事件系统
            RegisterSystemInGroup("world", typeof(RandomWorldEventSystem));
            RegisterSystemInGroup("world", typeof(ChoiceEventSystem));
            RegisterSystemInGroup("world", typeof(WorldBossSystem));
            RegisterSystemInGroup("world", typeof(BossRushSystem));
            RegisterSystemInGroup("world", typeof(DailyLoginBonusSystem));
            RegisterSystemInGroup("world", typeof(GameSettings));
            RegisterSystemInGroup("world", typeof(QuickSlotSystem));
            
            // 公会系统
            RegisterSystemInGroup("guild", typeof(GuildSystem));
            RegisterSystemInGroup("guild", typeof(GuildQuestSystem));
            RegisterSystemInGroup("guild", typeof(GuildBankSystem));
            RegisterSystemInGroup("guild", typeof(GuildTechnologySystem));
            RegisterSystemInGroup("guild", typeof(GuildWarLeagueSystem));
            RegisterSystemInGroup("guild", typeof(GuildHallDatabase));
            RegisterSystemInGroup("guild", typeof(GuildHallSystem));
            RegisterSystemInGroup("guild", typeof(GuildTournamentBracketSystem));
            
            // 多人系统
            RegisterSystemInGroup("multiplayer", typeof(MultiplayerLobbyData));
            RegisterSystemInGroup("multiplayer", typeof(MultiplayerLobbyDatabase));
            RegisterSystemInGroup("multiplayer", typeof(MultiplayerLobbySystem));
            RegisterSystemInGroup("multiplayer", typeof(MultiplayerLobbyUI));
            RegisterSystemInGroup("multiplayer", typeof(MultiplayerVoteSystem));
            RegisterSystemInGroup("multiplayer", typeof(MultiplayerVoteUI));
            
            // 宠物系统
            RegisterSystemInGroup("pet", typeof(CollectibleSystem));
            RegisterSystemInGroup("pet", typeof(PetEquipmentSystem));
            RegisterSystemInGroup("pet", typeof(PetEquipmentEnhancementSystem));
            RegisterSystemInGroup("pet", typeof(EquipmentRecycleSystem));
            RegisterSystemInGroup("pet", typeof(TradeSystem));
            RegisterSystemInGroup("pet", typeof(ArenaTournamentSystem));
            RegisterSystemInGroup("pet", typeof(ArenaColosseumSystem));
            RegisterSystemInGroup("pet", typeof(GemSystem));
            RegisterSystemInGroup("pet", typeof(GemFusionSystem));
            RegisterSystemInGroup("pet", typeof(KeybindingSystem));
            RegisterSystemInGroup("pet", typeof(AccessibilityManager));
            RegisterSystemInGroup("pet", typeof(ReputationSystem));
            RegisterSystemInGroup("pet", typeof(NPCScheduleSystem));
            RegisterSystemInGroup("pet", typeof(RelicSystem));
            RegisterSystemInGroup("pet", typeof(PetEvolutionSystem));
            RegisterSystemInGroup("pet", typeof(PetBreedingSystem));
            RegisterSystemInGroup("pet", typeof(PetTalentSystem));
            RegisterSystemInGroup("pet", typeof(PetAffectionSystem));
            RegisterSystemInGroup("pet", typeof(PetInteractionSystem));
            RegisterSystemInGroup("pet", typeof(PetAIImprovementsSystem));
            RegisterSystemInGroup("pet", typeof(PetAIImprovementsDatabase));
            RegisterSystemInGroup("pet", typeof(PetRecycleSystem));
            RegisterSystemInGroup("pet", typeof(PetInventorySystem));
            RegisterSystemInGroup("pet", typeof(PetInventoryDatabase));
            RegisterSystemInGroup("pet", typeof(PetLifeCycleSystem));
            RegisterSystemInGroup("pet", typeof(EliteMonsterDatabase));
            RegisterSystemInGroup("pet", typeof(EliteMonsterSystem));
            RegisterSystemInGroup("pet", typeof(PetFosterSystem));
            RegisterSystemInGroup("pet", typeof(PetSkillSystem));
            RegisterSystemInGroup("pet", typeof(PetExpeditionSystem));
            RegisterSystemInGroup("pet", typeof(PetTrainingSystem));
            RegisterSystemInGroup("pet", typeof(PetTrainingUI));
            RegisterSystemInGroup("pet", typeof(PetStorySystem));
            RegisterSystemInGroup("pet", typeof(PetMorphSystem));
            RegisterSystemInGroup("pet", typeof(PetHabitatSystem));
            RegisterSystemInGroup("pet", typeof(PetEggSystem));
            RegisterSystemInGroup("pet", typeof(PetFriendshipSystem));
            RegisterSystemInGroup("pet", typeof(PetSynthesisSystem));
            
            // 活动与挑战系统
            RegisterSystemInGroup("event", typeof(SeasonalEventSystem));
            RegisterSystemInGroup("event", typeof(CombatHUDEnhancementSystem));
            RegisterSystemInGroup("event", typeof(ElementalTrialSystem));
            RegisterSystemInGroup("event", typeof(PetBattleArenaSystem));
            RegisterSystemInGroup("event", typeof(DailyDungeonSystem));
            RegisterSystemInGroup("event", typeof(RandomBoonSystem));
            RegisterSystemInGroup("event", typeof(PlayerTalentSystem));
            RegisterSystemInGroup("event", typeof(MountRaceSystem));
            RegisterSystemInGroup("event", typeof(DynamicDifficultySystem));
            RegisterSystemInGroup("event", typeof(DailyQuestSystem));
            RegisterSystemInGroup("event", typeof(ProceduralChallengeSystem));
            RegisterSystemInGroup("event", typeof(DynamicQuestChallengeSystem));
            
            // 特殊系统
            RegisterSystemInGroup("special", typeof(EnemyWeaknessData));
            RegisterSystemInGroup("special", typeof(EnemyWeaknessDatabase));
            RegisterSystemInGroup("special", typeof(EnemyWeaknessSystem));
            RegisterSystemInGroup("special", typeof(ItemSmeltingData));
            RegisterSystemInGroup("special", typeof(ItemSmeltingDatabase));
            RegisterSystemInGroup("special", typeof(ItemSmeltingSystem));
            RegisterSystemInGroup("special", typeof(CombatEffectOverlaySystem));
            RegisterSystemInGroup("special", typeof(TutorialDatabase));
            RegisterSystemInGroup("special", typeof(SurvivalChallengeSystem));
            RegisterSystemInGroup("special", typeof(EmoteSystem));
            RegisterSystemInGroup("special", typeof(EconomicDashboardSystem));
            
            // 钓鱼与烹饪
            RegisterSystemInGroup("lifestyle", typeof(FishingSystem));
            RegisterSystemInGroup("lifestyle", typeof(ParallelDimensionSystem));
            RegisterSystemInGroup("lifestyle", typeof(AlchemySystem));
            RegisterSystemInGroup("lifestyle", typeof(CookingSystem));
            
            GD.Print($"[SystemInitializationManager] Registered {_systemTypes.Count} systems in {_initializationGroups.Count} groups");
        }
        
        /// <summary>
        /// 注册系统到指定分组
        /// </summary>
        private void RegisterSystemInGroup(string group, Type systemType)
        {
            if (!_initializationGroups.ContainsKey(group))
            {
                _initializationGroups[group] = new List<Type>();
            }
            _initializationGroups[group].Add(systemType);
            _systemTypes.Add(systemType);
        }
        
        /// <summary>
        /// 按优先级顺序初始化所有系统
        /// </summary>
        private void InitializeSystemsByPriority()
        {
            var mainNode = GetParent();
            
            // 按分组顺序初始化
            string[] groupOrder = { "core", "combat", "vfx", "economy", "mount", "world", "guild", "multiplayer", "pet", "event", "special", "lifestyle" };
            
            foreach (var group in groupOrder)
            {
                if (_initializationGroups.ContainsKey(group))
                {
                    InitializeGroup(_initializationGroups[group], mainNode, group);
                }
            }
            
            // 初始化任何未分类的系统
            foreach (var systemType in _systemTypes)
            {
                // 检查是否已初始化
                if (mainNode.HasNode(systemType.Name))
                {
                    continue;
                }
                
                try
                {
                    var system = Activator.CreateInstance(systemType) as Node;
                    if (system != null)
                    {
                        system.Name = systemType.Name;
                        mainNode.AddChild(system);
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[SystemInitializationManager] Failed to initialize {systemType.Name}: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 初始化一组系统
        /// </summary>
        private void InitializeGroup(List<Type> systemTypes, Node parent, string groupName)
        {
            foreach (var systemType in systemTypes)
            {
                try
                {
                    // 检查是否已存在
                    if (parent.HasNode(systemType.Name))
                    {
                        continue;
                    }
                    
                    var system = Activator.CreateInstance(systemType) as Node;
                    if (system != null)
                    {
                        system.Name = systemType.Name;
                        parent.AddChild(system);
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[SystemInitializationManager] Failed to initialize {systemType.Name} in group {groupName}: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 初始化特定系统（供外部调用）
        /// </summary>
        public T InitializeSystem<T>() where T : Node, new()
        {
            var mainNode = GetParent();
            var systemType = typeof(T);
            
            if (mainNode.HasNode(systemType.Name))
            {
                return mainNode.GetNode<T>(systemType.Name);
            }
            
            var system = new T();
            system.Name = systemType.Name;
            mainNode.AddChild(system);
            
            return system;
        }
        
        /// <summary>
        /// 获取已注册的系统数量
        /// </summary>
        public int GetRegisteredSystemCount() => _systemTypes.Count;
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            return new Dictionary
            {
                { "isInitializationComplete", IsInitializationComplete },
                { "registeredSystemCount", _systemTypes.Count }
            };
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            if (data.Contains("isInitializationComplete"))
                IsInitializationComplete = Convert.ToBoolean(data["isInitializationComplete"]);
        }
    }
}
