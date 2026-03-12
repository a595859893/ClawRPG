using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;
using ClawRPG.Systems;
using ClawRPG.Scripts.Mounts;
using ClawRPG.Scripts.Systems.Pets;
using ClawRPG.Scripts.Systems.Enhancement;
using ClawRPG.Scripts.UI;
using ClawRPG.Scripts.Items;
using ClawRPG.Scripts.Quests;
using ClawRPG.Scripts.Achievements;
using ClawRPG.Scripts.Combat;

namespace ClawRPG.Scripts {
    /// <summary>
    /// Main game manager - handles game initialization, player spawning, and game state
    /// </summary>
    public partial class Main : Node2D
    {
        [Export] public PackedScene PlayerScene;
        [Export] public PackedScene EnemyScene;

        private Player _player;
        private Node2D _enemies;
        private Node2D _items;

        // Game state
        public enum GameState
        {
            TitleScreen,
            Playing,
            Paused,
            GameOver
        }

        private GameState _currentGameState = GameState.Playing;
        private bool _shiftEToggleCooldown = false; 

        public static bool IsPaused { get; private set; }
        public static int CurrentDay { get; private set; } = 1;

        public void SetGameState(GameState state)
        {
            _currentGameState = state;
            GD.Print("Game state changed to: " + state);
        }

        public GameState GetGameState() => _currentGameState;

        public Player GetPlayer() => _player;

        public override void _Ready()
        {
            GD.Print("=== ClawRPG Starting ===");

            // Create node structure
            _enemies = new Node2D();
            _enemies.Name = "Enemies";
            AddChild(_enemies);

            _items = new Node2D();
            _items.Name = "Items";
            AddChild(_items);

            // Initialize weapon mastery system
            var weaponMasterySystem = new WeaponMasterySystem();
            weaponMasterySystem.Name = "WeaponMasterySystem";
            AddChild(weaponMasterySystem);

            // Initialize title system
            var titleSystem = new TitleSystem();
            titleSystem.Name = "TitleSystem";
            AddChild(titleSystem);

            // Initialize pet combat AI
            var petCombatAI = new PetCombatAI();
            petCombatAI.Name = "PetCombatAI";
            AddChild(petCombatAI);
            petCombatAI.Initialize();

            // Initialize enhancement system
            var enhancementSystem = new EnhancementSystem();
            enhancementSystem.Name = "EnhancementSystem";
            AddChild(enhancementSystem);

            // Initialize enhancement database
            var enhancementDb = new EnhancementDatabase();
            enhancementDb.Name = "EnhancementDatabase";
            AddChild(enhancementDb);

            // Initialize auto potion system
            var autoPotionSystem = new AutoPotionSystem();
            autoPotionSystem.Name = "AutoPotionSystem";
            AddChild(autoPotionSystem);

            // Initialize auto bookmark system
            var autoBookmarkSystem = new AutoBookmarkSystem();
            autoBookmarkSystem.Name = "AutoBookmarkSystem";
            AddChild(autoBookmarkSystem);

            // Initialize achievement badge system
            var badgeSystem = new AchievementBadgeSystem();
            badgeSystem.Name = "AchievementBadgeSystem";
            AddChild(badgeSystem);

            // Initialize secret achievement system
            var secretAchievementSystem = new SecretAchievementSystem();
            secretAchievementSystem.Name = "SecretAchievementSystem";
            AddChild(secretAchievementSystem);

            // Initialize faction system
            var factionSystem = new FactionSystem();
            factionSystem.Name = "FactionSystem";
            AddChild(factionSystem);

            // Initialize player profile system
            var playerProfileSystem = new PlayerProfileSystem();
            playerProfileSystem.Name = "PlayerProfileSystem";
            AddChild(playerProfileSystem);

            // Initialize artifact system
            var artifactSystem = new ArtifactSystem();
            artifactSystem.Name = "ArtifactSystem";
            AddChild(artifactSystem);

            // Initialize weather system
            var weatherSystem = new WeatherSystem();
            weatherSystem.Name = "WeatherSystem";
            AddChild(weatherSystem);

            // Initialize counter attack system
            var counterAttackSystem = new CounterAttackSystem();
            counterAttackSystem.Name = "CounterAttackSystem";
            AddChild(counterAttackSystem);

            // Initialize equipment set system
            var equipmentSetSystem = new EquipmentSetSystem();
            equipmentSetSystem.Name = "EquipmentSetSystem";
            AddChild(equipmentSetSystem);

            // Initialize equipment set database
            var equipmentSetDb = new EquipmentSetDatabase();
            equipmentSetDb.Name = "EquipmentSetDatabase";
            AddChild(equipmentSetDb);

            // Initialize enchantment database
            var enchantmentDb = new EnchantmentDatabase();
            enchantmentDb.Name = "EnchantmentDatabase";
            AddChild(enchantmentDb);

            // Connect counter attack system signals to sound effects
            counterAttackSystem.Connect(CounterAttackSystem.SignalName.CounterAttackPerformed,
                this, nameof(_OnCounterAttackPerformed));
            counterAttackSystem.Connect(CounterAttackSystem.SignalName.CounterAttack窗口,
                this, nameof(_OnCounterAttackWindow));
            counterAttackSystem.Connect(CounterAttackSystem.SignalName.CounterAttackReady,
                this, nameof(_OnCounterAttackReady));

            // Initialize enchantment database
            var enchantmentDb = new ClawRPG.Scripts.Systems.Enchantment.EnchantmentDatabase();
            enchantmentDb.Name = "EnchantmentDatabase";
            AddChild(enchantmentDb);

            // Initialize bounty system
            var bountyManager = BountyManager.Instance;
            bountyManager.Initialize();

            // Initialize mail system
            var mailManager = new MailManager();
            mailManager.Name = "MailManager";
            AddChild(mailManager);

            // Initialize daily login reward system
            var dailyLoginRewardSystem = new DailyLoginRewardSystem();
            dailyLoginRewardSystem.Name = "DailyLoginRewardSystem";
            AddChild(dailyLoginRewardSystem);

            // Initialize daily ritual system
            var dailyRitualSystem = new DailyRitualSystem();
            dailyRitualSystem.Name = "DailyRitualSystem";
            AddChild(dailyRitualSystem);

            // Initialize weekly challenge system
            var weeklyChallengeSystem = new WeeklyChallengeSystem();
            weeklyChallengeSystem.Name = "WeeklyChallengeSystem";
            AddChild(weeklyChallengeSystem);

            // Initialize weather system
            var weatherSystem = new WeatherSystem();
            weatherSystem.Name = "WeatherSystem";
            AddChild(weatherSystem);

            // Connect weather system signals to sound effects
            weatherSystem.Connect(WeatherSystem.SignalName.WeatherChanged,
                this, nameof(_OnWeatherChanged));

            // Initialize companion interaction system (pet/mount bonding)
            var companionInteractionSystem = new CompanionInteractionSystem();
            companionInteractionSystem.Name = "CompanionInteractionSystem";
            AddChild(companionInteractionSystem);

            // Initialize camera effect system
            var cameraEffectSystem = new CameraEffectSystem();
            cameraEffectSystem.Name = "CameraEffectSystem";
            AddChild(cameraEffectSystem);

            // Initialize combo system
            var comboSystem = new ComboSystem();
            comboSystem.Name = "ComboSystem";
            AddChild(comboSystem);

            // Initialize momentum system
            var momentumSystem = new MomentumSystem();
            momentumSystem.Name = "MomentumSystem";
            AddChild(momentumSystem);

            // Initialize skill combo system
            var skillComboSystem = new SkillComboSystem();
            skillComboSystem.Name = "SkillComboSystem";
            AddChild(skillComboSystem);

            // Initialize skill tree system
            var skillTreeSystem = new SkillTreeSystem();
            skillTreeSystem.Name = "SkillTreeSystem";
            AddChild(skillTreeSystem);

            // Initialize AOE indicator system
            var aoeIndicatorManager = new Systems.AOEIndicatorManager();
            aoeIndicatorManager.Name = "AOEIndicatorManager";
            AddChild(aoeIndicatorManager);

            // Initialize animation effect system
            var animationEffectManager = new Systems.AnimationEffectManager();
            animationEffectManager.Name = "AnimationEffectManager";
            AddChild(animationEffectManager);

            // Initialize dialogue system
            var dialogueManager = Quests.DialogueManager.Instance;
            dialogueManager.Name = "DialogueManager";
            AddChild(dialogueManager);

            // Initialize story system
            var storyManager = new StorySystem.StoryManager();
            storyManager.Name = "StoryManager";
            AddChild(storyManager);

            // Initialize sealed tower system (roguelike endless dungeon)
            var sealedTowerManager = new Systems.SealedTowerManager();
            sealedTowerManager.Name = "SealedTowerManager";
            AddChild(sealedTowerManager);

            // Initialize crafting mastery system
            var craftingMasterySystem = new CraftingMasterySystem();
            craftingMasterySystem.Name = "CraftingMasterySystem";
            AddChild(craftingMasterySystem);

            // Initialize treasure hunt system
            var treasureHuntManager = new TreasureHuntManager();
            treasureHuntManager.Name = "TreasureHuntManager";
            AddChild(treasureHuntManager);

            // Initialize region manager
            var regionManager = new RegionManager();
            regionManager.Name = "RegionManager";
            AddChild(regionManager);

            // Initialize sound effect system
            var soundEffectSystem = new SoundEffectSystem();
            soundEffectSystem.Name = "SoundEffectSystem";
            soundEffectSystem.AddToGroup("SoundEffectSystem");
            AddChild(soundEffectSystem);

            // Initialize background music system
            var backgroundMusicSystem = new BackgroundMusicSystem();
            backgroundMusicSystem.Name = "BackgroundMusicSystem";
            AddChild(backgroundMusicSystem);

            // Initialize music collection system
            var musicCollectionSystem = new MusicCollectionSystem();
            musicCollectionSystem.Name = "MusicCollectionSystem";
            AddChild(musicCollectionSystem);

            // Initialize gathering system
            var gatheringSystem = new GatheringSystem();
            gatheringSystem.Name = "GatheringSystem";
            AddChild(gatheringSystem);

            // Initialize monster taming system
            var monsterTamingSystem = new MonsterTamingSystem();
            monsterTamingSystem.Name = "MonsterTamingSystem";
            AddChild(monsterTamingSystem);

            // Initialize prestige system
            var prestigeSystem = new Systems.PrestigeSystem();
            prestigeSystem.Name = "PrestigeSystem";
            AddChild(prestigeSystem);

            // Initialize dice master system
            var diceMasterSystem = new DiceMasterSystem();
            diceMasterSystem.Name = "DiceMasterSystem";
            AddChild(diceMasterSystem);

            // Initialize identification system
            var identificationSystem = new IdentificationSystem();
            identificationSystem.Name = "IdentificationSystem";
            AddChild(identificationSystem);

            // Initialize alchemy laboratory system
            var alchemyLaboratorySystem = new AlchemyLaboratorySystem();
            alchemyLaboratorySystem.Name = "AlchemyLaboratorySystem";
            alchemyLaboratorySystem.UnlockLaboratory();
            alchemyLaboratorySystem.GenerateNewResearches();
            AddChild(alchemyLaboratorySystem);

            // Initialize multiplayer leaderboard system
            var leaderboardSystem = new MultiplayerLeaderboard();
            leaderboardSystem.Name = "MultiplayerLeaderboard";
            AddChild(leaderboardSystem);

            // Initialize network quality UI
            var networkQualityUI = new NetworkQualityUI();
            networkQualityUI.Name = "NetworkQualityUI";
            AddChild(networkQualityUI);

            // Initialize equipment set system
            var equipmentSetManager = new EquipmentSetManager();
            equipmentSetManager.Name = "EquipmentSetManager";
            AddChild(equipmentSetManager);

            // Initialize procedural equipment system (affix generation)
            var proceduralEquipmentSystem = ProceduralEquipmentSystem.Instance;

            // Initialize boss ability visualizer
            var bossAbilityVisualizer = new Combat.BossAbilityVisualizer();
            bossAbilityVisualizer.Name = "BossAbilityVisualizer";
            AddChild(bossAbilityVisualizer);

            // Initialize boss ability warning UI system
            var bossAbilityWarningUI = new UI.BossAbilityWarningUI();
            bossAbilityWarningUI.Name = "BossAbilityWarningUI";
            AddChild(bossAbilityWarningUI);

            // Initialize counter attack VFX system
            var counterAttackVFX = new Combat.CounterAttackVFX();
            counterAttackVFX.Name = "CounterAttackVFX";
            AddChild(counterAttackVFX);

            // Initialize balance manager system
            var balanceManager = new BalanceManager();
            balanceManager.Name = "BalanceManager";
            AddChild(balanceManager);

            // Initialize screen effect system (post-processing)
            var screenEffectManager = new ScreenEffectManager();
            screenEffectManager.Name = "ScreenEffectManager";
            AddChild(screenEffectManager);

            // Initialize combat VFX system
            var combatVFXSystem = new Combat.CombatVFXSystem();
            combatVFXSystem.Name = "CombatVFXSystem";
            AddChild(combatVFXSystem);

            // Initialize team skill system
            var teamSkillSystem = new TeamSkillSystem();
            teamSkillSystem.Name = "TeamSkillSystem";
            AddChild(teamSkillSystem);

            // Initialize loot drop system
            LootDropSystem.Instance.Initialize();

            // Initialize mystery treasure system
            var mysteryTreasureSystem = new MysteryTreasureSystem();
            mysteryTreasureSystem.Name = "MysteryTreasureSystem";
            AddChild(mysteryTreasureSystem);

            // Initialize ranked system
            var rankedSystem = new RankedSystem();
            rankedSystem.Name = "RankedSystem";
            AddChild(rankedSystem);

            // Initialize elemental reaction system
            var elementalReactionManager = new Systems.ElementalReactionManager();
            elementalReactionManager.Name = "ElementalReactionManager";
            AddChild(elementalReactionManager);

            // Initialize elemental damage manager
            var elementalDamageManager = new Systems.ElementalDamageManager();
            elementalDamageManager.Name = "ElementalDamageManager";
            AddChild(elementalDamageManager);

            // Initialize elemental skill manager
            var elementalSkillManager = new Systems.ElementalSkillManager();
            elementalSkillManager.Initialize();

            // Initialize particle effect system
            var particleEffectManager = new Systems.ParticleEffectManager();
            particleEffectManager.Name = "ParticleEffectManager";
            AddChild(particleEffectManager);

            // Initialize shop system
            var shopSystem = new ShopSystem();
            shopSystem.Name = "ShopSystem";
            AddChild(shopSystem);

            // Initialize auction house system
            var auctionHouseSystem = new AuctionHouseSystem();
            auctionHouseSystem.Name = "AuctionHouseSystem";
            AddChild(auctionHouseSystem);
            auctionHouseSystem.Initialize(_player);

            // Initialize mount combat system
            var mountCombatSystem = new Mounts.MountCombatSystem();
            mountCombatSystem.Name = "MountCombatSystem";
            AddChild(mountCombatSystem);

            // Initialize mount battle arena system
            var mountBattleArenaSystem = new Systems.MountBattleArenaSystem();
            mountBattleArenaSystem.Name = "MountBattleArenaSystem";
            AddChild(mountBattleArenaSystem);

            // Initialize mount equipment system
            var mountEquipmentSystem = new Systems.MountEquipmentSystem();
            mountEquipmentSystem.Name = "MountEquipmentSystem";
            AddChild(mountEquipmentSystem);

            // Initialize mount evolution system
            var mountEvolutionSystem = MountEvolutionSystem.Instance;
            mountEvolutionSystem.Initialize();

            // Initialize mount weather bonus system
            var mountWeatherBonusSystem = new Systems.MountWeatherBonusSystem();
            mountWeatherBonusSystem.Name = "MountWeatherBonusSystem";
            AddChild(mountWeatherBonusSystem);

            // Initialize random world event system
            var worldEventSystem = new Systems.RandomWorldEventSystem();
            worldEventSystem.Name = "RandomWorldEventSystem";
            AddChild(worldEventSystem);

            // Initialize choice event system (roguelike style)
            var choiceEventSystem = new Systems.ChoiceEvents.ChoiceEventSystem();
            choiceEventSystem.Name = "ChoiceEventSystem";
            AddChild(choiceEventSystem);

            // Initialize world boss system
            var worldBossSystem = new Systems.WorldBoss.WorldBossSystem();
            worldBossSystem.Name = "WorldBossSystem";
            AddChild(worldBossSystem);

            // Initialize game settings system
            var gameSettings = new GameSettings();
            gameSettings.Name = "GameSettings";
            AddChild(gameSettings);

            // Initialize quick slot system
            var quickSlotSystem = new QuickSlotSystem();
            quickSlotSystem.Name = "QuickSlotSystem";
            AddChild(quickSlotSystem);

            // Initialize guild system
            var guildSystem = new GuildSystem();
            guildSystem.Name = "GuildSystem";
            AddChild(guildSystem);

            // Initialize guild quest system
            var guildQuestSystem = new GuildQuestSystem();
            guildQuestSystem.Name = "GuildQuestSystem";
            AddChild(guildQuestSystem);

            // Initialize guild bank system
            var guildBankSystem = new GuildBankSystem();
            guildBankSystem.Name = "GuildBankSystem";
            AddChild(guildBankSystem);

            // Initialize guild technology system (singleton)
            _ = GuildTechnologySystem.Instance;

            // Initialize collectible system
            CollectibleSystem.Instance.Initialize();

            // Initialize pet equipment system
            var petEquipmentSystem = new Systems.Pets.PetEquipmentSystem();
            petEquipmentSystem.Name = "PetEquipmentSystem";
            AddChild(petEquipmentSystem);

            // Initialize pet equipment enhancement system
            var petEquipmentEnhancementSystem = new Systems.PetEquipment.PetEquipmentEnhancementSystem();
            petEquipmentEnhancementSystem.Name = "PetEquipmentEnhancementSystem";
            AddChild(petEquipmentEnhancementSystem);

            // Initialize equipment recycle system
            var equipmentRecycleSystem = new Systems.EquipmentRecycle.EquipmentRecycleSystem();
            equipmentRecycleSystem.Name = "EquipmentRecycleSystem";
            AddChild(equipmentRecycleSystem);

            // Initialize trade system
            var tradeSystem = new TradeSystem();
            tradeSystem.Name = "TradeSystem";
            AddChild(tradeSystem);

            // Initialize arena tournament system
            var arenaTournamentSystem = new ArenaTournamentSystem();
            arenaTournamentSystem.Name = "ArenaTournamentSystem";
            AddChild(arenaTournamentSystem);

            // Initialize arena colosseum system
            var arenaColosseumSystem = new Systems.ArenaColosseumSystem();
            arenaColosseumSystem.Name = "ArenaColosseumSystem";
            AddChild(arenaColosseumSystem);

            // Initialize gem system
            var gemSystem = GemSystem.Instance;

            // Initialize gem fusion system
            var gemFusionSystem = new Systems.GemSystem.GemFusionSystem();
            gemFusionSystem.Name = "GemFusionSystem";
            AddChild(gemFusionSystem);

            // Initialize keybinding system
            var keybindingSystem = new Systems.KeybindingSystem();

            // Initialize accessibility system
            var accessibilitySystem = new Systems.AccessibilityManager();
            accessibilitySystem.Name = "AccessibilityManager";
            AddChild(accessibilitySystem);

            // Initialize reputation system
            var reputationSystem = ReputationSystem.Instance;
            reputationSystem.Initialize();

            // Initialize NPC schedule system
            var npcScheduleSystem = new NPCScheduleSystem();
            npcScheduleSystem.Name = "NPCScheduleSystem";
            AddChild(npcScheduleSystem);

            // Initialize relic system
            var relicSystem = new Systems.RelicSystem();
            relicSystem.Name = "RelicSystem";
            AddChild(relicSystem);

            // Initialize pet evolution system
            var petEvolutionSystem = new Systems.PetEvolution.PetEvolutionSystem();
            petEvolutionSystem.Name = "PetEvolutionSystem";
            AddChild(petEvolutionSystem);

            // Initialize pet talent system
            var petTalentSystem = new Systems.PetTalentSystem();
            petTalentSystem.Name = "PetTalentSystem";
            AddChild(petTalentSystem);

            // Initialize pet affection system
            var petAffectionSystem = new Systems.PetAffectionSystem();
            petAffectionSystem.Name = "PetAffectionSystem";
            AddChild(petAffectionSystem);
            petAffectionSystem.Initialize();

            // Initialize pet foster system
            var petFosterSystem = new Systems.PetFoster.PetFosterSystem();
            petFosterSystem.Name = "PetFosterSystem";
            AddChild(petFosterSystem);

            // Initialize pet skill system
            var petSkillSystem = new Systems.Pets.PetSkillSystem();
            petSkillSystem.Name = "PetSkillSystem";
            AddChild(petSkillSystem);
            petSkillSystem.Initialize();

            // Initialize pet expedition system
            var petExpeditionSystem = new Systems.PetExpeditionSystem();
            petExpeditionSystem.Name = "PetExpeditionSystem";
            AddChild(petExpeditionSystem);
            petExpeditionSystem.Initialize();

            // Initialize pet training system
            var petTrainingSystem = new Systems.Pets.PetTrainingSystem();
            petTrainingSystem.Name = "PetTrainingSystem";
            AddChild(petTrainingSystem);

            // Initialize pet training UI
            var petTrainingUI = new Systems.Pets.PetTrainingUI();
            petTrainingUI.Name = "PetTrainingUI";
            AddChild(petTrainingUI);

            // Initialize pet story system
            var petStorySystem = new PetStorySystem();
            petStorySystem.Name = "PetStorySystem";
            AddChild(petStorySystem);

            // Initialize pet story UI
            var petStoryUI = new PetStoryUI();
            petStoryUI.Name = "PetStoryUI";
            petStoryUI.Visible = false;
            ui.AddChild(petStoryUI);

            // Initialize pet morph system
            var petMorphSystem = PetMorphSystem.Instance;
            petMorphSystem.Initialize();

            // Initialize pet habitat system
            var petHabitatSystem = new PetHabitatSystem();
            petHabitatSystem.Name = "PetHabitatSystem";
            AddChild(petHabitatSystem);
            petHabitatSystem.Initialize();

            // Initialize pet habitat UI
            var petHabitatUI = new PetHabitatUI();
            petHabitatUI.Name = "PetHabitatUI";
            petHabitatUI.Visible = false;
            ui.AddChild(petHabitatUI);

            // Initialize pet egg hatching system
            var petEggSystem = new PetEggSystem();
            petEggSystem.Name = "PetEggSystem";
            AddChild(petEggSystem);

            // Initialize pet egg UI
            var petEggUI = new PetEggUI();
            petEggUI.Name = "PetEggUI";
            petEggUI.Visible = false;
            ui.AddChild(petEggUI);

            // Initialize pet friendship system
            var petFriendshipSystem = new PetFriendshipSystem();
            petFriendshipSystem.Name = "PetFriendshipSystem";
            AddChild(petFriendshipSystem);
            petFriendshipSystem.LoadData();

            // Initialize pet friendship UI
            var petFriendshipUI = new PetFriendshipUI();
            petFriendshipUI.Name = "PetFriendshipUI";
            petFriendshipUI.Visible = false;
            ui.AddChild(petFriendshipUI);

            // Initialize mount expedition system
            var mountExpeditionSystem = new Systems.MountExpeditionSystem();
            mountExpeditionSystem.Name = "MountExpeditionSystem";
            AddChild(mountExpeditionSystem);

            // Initialize seasonal event system
            var seasonalEventSystem = new SeasonalEventSystem();
            seasonalEventSystem.Name = "SeasonalEventSystem";
            AddChild(seasonalEventSystem);
            seasonalEventSystem.Initialize();

            // Initialize combat HUD enhancement system
            var combatHUDSystem = new CombatHUDEnhancementSystem();
            combatHUDSystem.Name = "CombatHUDEnhancementSystem";
            AddChild(combatHUDSystem);

            // Initialize elemental trial system
            var elementalTrialSystem = new Systems.ElementalTrialSystem();
            elementalTrialSystem.Name = "ElementalTrialSystem";
            AddChild(elementalTrialSystem);
            elementalTrialSystem.Initialize();

            // Initialize pet battle arena system
            var petBattleArenaSystem = new Systems.PetBattleArena.PetBattleArenaSystem();
            petBattleArenaSystem.Name = "PetBattleArenaSystem";
            AddChild(petBattleArenaSystem);

            // Initialize daily dungeon system
            var dailyDungeonSystem = new Systems.DailyDungeon.DailyDungeonSystem();
            dailyDungeonSystem.Name = "DailyDungeonSystem";
            AddChild(dailyDungeonSystem);

            // Initialize random boon system
            var randomBoonSystem = new Systems.RandomBoon.RandomBoonSystem();
            randomBoonSystem.Name = "RandomBoonSystem";
            AddChild(randomBoonSystem);

            // Initialize player talent system
            var playerTalentSystem = new Systems.PlayerTalent.PlayerTalentSystem();
            playerTalentSystem.Name = "PlayerTalentSystem";
            AddChild(playerTalentSystem);

            // Initialize mount race system
            var mountRaceSystem = new MountRaceSystem();
            mountRaceSystem.Name = "MountRaceSystem";
            AddChild(mountRaceSystem);

            // Initialize dynamic difficulty system
            var dynamicDifficultySystem = new DynamicDifficultySystem();
            dynamicDifficultySystem.Name = "DynamicDifficultySystem";
            AddChild(dynamicDifficultySystem);

            // Initialize daily quest system
            var dailyQuestSystem = new Systems.DailyQuest.DailyQuestSystem();
            dailyQuestSystem.Name = "DailyQuestSystem";
            AddChild(dailyQuestSystem);
            dailyQuestSystem.Initialize();

            // Initialize procedural challenge system
            var proceduralChallengeSystem = new Systems.ProceduralChallengeSystem();
            proceduralChallengeSystem.Name = "ProceduralChallengeSystem";
            AddChild(proceduralChallengeSystem);

            // Initialize boss mechanics system
            var bossMechanicsSystem = new Systems.BossMechanics.BossMechanicsSystem();
            bossMechanicsSystem.Name = "BossMechanicsSystem";
            AddChild(bossMechanicsSystem);

            // Tutorial System
            var tutorialDb = new TutorialDatabase();
            GD.Print("Tutorial database initialized");
            keybindingSystem.Name = "KeybindingSystem";
            AddChild(keybindingSystem);

            // Connect sound effect signals
            ConnectSoundSignals();

            // Spawn player
            SpawnPlayer();

            // Initialize UI
            InitializeUI();

            // Load game data
            LoadGameData();

            GD.Print("Game initialized successfully!");
        }

        private void ConnectSoundSignals()
        {
            // Connect achievement unlock sound
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked += (achievement) => {
                    if (SoundEffectSystem.Instance != null)
                        SoundEffectSystem.Instance.PlayAchievementUnlock();
                };
            }

            // Connect title unlock sound
            if (TitleSystem.Instance != null)
            {
                TitleSystem.Instance.OnTitleUnlocked += (title) => {
                    if (SoundEffectSystem.Instance != null)
                        SoundEffectSystem.Instance.PlayTitleUnlock();
                };
            }

            // Connect quest complete sound
            QuestSystem.OnQuestCompleted += (quest) => {
                if (SoundEffectSystem.Instance != null)
                    SoundEffectSystem.Instance.PlayQuestComplete();
            };

            GD.Print("Sound effect signals connected");
        }

        private void SpawnPlayer()
        {
            if (PlayerScene == null)
            {
                GD.PrintErr("PlayerScene not set!");
                return;
            }

            _player = PlayerScene.Instantiate<Player>();
            _player.AddToGroup("player");
            _player.GlobalPosition = new Vector2(640, 360); // Center of screen
            AddChild(_player);

            GD.Print("Player spawned");
        }

        private void InitializeUI()
        {
            // Create UI layer
            var ui = new CanvasLayer();
            ui.Name = "UI";
            AddChild(ui);

            // Health bar
            var healthBar = new ProgressBar();
            healthBar.Name = "HealthBar";
            healthBar.Position = new Vector2(20, 20);
            healthBar.Size = new Vector2(200, 20);
            healthBar.Value = 100;
            healthBar.MaxValue = 100;
            ui.AddChild(healthBar);

            // Mana bar
            var manaBar = new ProgressBar();
            manaBar.Name = "ManaBar";
            manaBar.Position = new Vector2(20, 45);
            manaBar.Size = new Vector2(200, 20);
            manaBar.Value = 50;
            manaBar.MaxValue = 50;
            ui.AddChild(manaBar);

            // Level display
            var levelLabel = new Label();
            levelLabel.Name = "LevelLabel";
            levelLabel.Position = new Vector2(230, 20);
            levelLabel.Text = "Lv.1";
            ui.AddChild(levelLabel);

            // Experience bar
            var expBar = new ProgressBar();
            expBar.Name = "ExpBar";
            expBar.Position = new Vector2(20, 70);
            expBar.Size = new Vector2(200, 10);
            expBar.Value = 0;
            expBar.MaxValue = 100;
            ui.AddChild(expBar);

            // Potion UI
            var potionUI = new PotionUI();
            potionUI.Name = "PotionUI";
            ui.AddChild(potionUI);

            // Enchantment UI
            var enchantmentUI = new EnchantmentUI();
            enchantmentUI.Name = "EnchantmentUI";
            ui.AddChild(enchantmentUI);

            // Dynamic screen effect (for vignette, damage overlays, combo pulses)
            var dynamicScreenEffect = new DynamicScreenEffect();
            dynamicScreenEffect.Name = "DynamicScreenEffect";
            dynamicScreenEffect.AddToGroup("DynamicScreenEffect");
            ui.AddChild(dynamicScreenEffect);

            // Combo display UI
            var comboDisplayUI = new ComboDisplayUI();
            comboDisplayUI.Name = "ComboDisplayUI";
            comboDisplayUI.AddToGroup("ComboDisplay");
            ui.AddChild(comboDisplayUI);

            // Hit Stop UI
            var hitStopUI = new HitStopUI();
            hitStopUI.Name = "HitStopUI";
            hitStopUI.Visible = false;
            ui.AddChild(hitStopUI);

            // Combat Stats Panel
            var combatStatsPanel = new UI.CombatStatsPanel();
            combatStatsPanel.Name = "CombatStatsPanel";
            combatStatsPanel.AddToGroup("CombatStatsPanel");
            ui.AddChild(combatStatsPanel);

            // Momentum UI
            var momentumUI = new MomentumUI();
            momentumUI.Name = "MomentumUI";
            ui.AddChild(momentumUI);

            // Skill Combo UI
            var skillComboUI = new SkillComboUI();
            skillComboUI.Name = "SkillComboUI";
            ui.AddChild(skillComboUI);

            // Skill Tree UI
            var skillTreeUI = new SkillTreeUI();
            skillTreeUI.Name = "SkillTreeUI";
            ui.AddChild(skillTreeUI);

            // Quick Slot UI
            var quickSlotUI = new UI.QuickSlotUI();
            quickSlotUI.Name = "QuickSlotUI";
            ui.AddChild(quickSlotUI);

            // Combat HUD Enhancement UI
            var combatHUDUI = new UI.CombatHUDEnhancementUI();
            combatHUDUI.Name = "CombatHUDEnhancementUI";
            combatHUDUI.AddToGroup("CombatHUDEnhancementUI");
            ui.AddChild(combatHUDUI);

            // Combat Skill Cooldown UI
            var skillCooldownUI = new UI.CombatSkillCooldownUI();
            skillCooldownUI.Name = "CombatSkillCooldownUI";
            skillCooldownUI.AddToGroup("CombatSkillCooldownUI");
            ui.AddChild(skillCooldownUI);

            // Combat Status UI
            var combatStatusUI = new UI.CombatStatusUI();
            combatStatusUI.Name = "CombatStatusUI";
            combatStatusUI.AddToGroup("CombatStatusUI");
            ui.AddChild(combatStatusUI);

            // Combat VFX UI
            var combatVFXUI = new UI.CombatVFXUI();
            combatVFXUI.Name = "CombatVFXUI";
            ui.AddChild(combatVFXUI);

            // Player Profile UI
            var playerProfileUI = new UI.PlayerProfileUI();
            playerProfileUI.Name = "PlayerProfileUI";
            ui.AddChild(playerProfileUI);

            // Artifact UI
            var artifactUI = new ArtifactUI();
            artifactUI.Name = "ArtifactUI";
            ui.AddChild(artifactUI);

            // Weather UI
            var weatherUI = new WeatherUI();
            weatherUI.Name = "WeatherUI";
            ui.AddChild(weatherUI);

            // Dialogue UI
            var dialogueUI = new UI.DialogueUI();
            dialogueUI.Name = "DialogueUI";
            ui.AddChild(dialogueUI);

            // Story UI
            var storyUI = new UI.StoryUI();
            storyUI.Name = "StoryUI";
            ui.AddChild(storyUI);

            // Sealed Tower UI (roguelike endless dungeon)
            var sealedTowerUI = new UI.SealedTowerUI();
            sealedTowerUI.Name = "SealedTowerUI";
            sealedTowerUI.Visible = false;
            ui.AddChild(sealedTowerUI);

            // Crafting Mastery UI
            var craftingMasteryUI = new CraftingMasteryUI();
            craftingMasteryUI.Name = "CraftingMasteryUI";
            craftingMasteryUI.Visible = false;
            ui.AddChild(craftingMasteryUI);

            // Hotkey HUD - 显示所有快捷键
            var hotkeyHUD = new HotkeyHUD();
            hotkeyHUD.Name = "HotkeyHUD";
            hotkeyHUD.Visible = false;
            ui.AddChild(hotkeyHUD);

            // Equipment Set UI
            var equipmentSetUI = new UI.EquipmentSetUI();
            equipmentSetUI.Name = "EquipmentSetUI";
            ui.AddChild(equipmentSetUI);

            // Faction UI
            var factionUI = new FactionUI();
            factionUI.Name = "FactionUI";
            factionUI.Visible = false;
            ui.AddChild(factionUI);

            // Keybinding UI
            var keybindingUI = new UI.KeybindingUI();
            keybindingUI.Name = "KeybindingUI";
            ui.AddChild(keybindingUI);

            // Accessibility UI
            var accessibilityUI = new UI.AccessibilityUI();
            accessibilityUI.Name = "AccessibilityUI";
            ui.AddChild(accessibilityUI);

            // Counter Attack UI
            var counterAttackUI = new UI.CounterAttackUI();
            counterAttackUI.Name = "CounterAttackUI";
            ui.AddChild(counterAttackUI);

            // Boss Health Bar UI
            var bossHealthBarUI = new UI.BossHealthBarUI();
            bossHealthBarUI.Name = "BossHealthBarUI";
            ui.AddChild(bossHealthBarUI);

            // Tutorial UI
            var tutorialUI = new UI.TutorialUI();
            tutorialUI.Name = "TutorialUI";
            ui.AddChild(tutorialUI);

            // Balance UI
            var balanceUI = new UI.BalanceUI();
            balanceUI.Name = "BalanceUI";
            ui.AddChild(balanceUI);

            // Reputation UI
            var reputationUI = new UI.ReputationUI();
            reputationUI.Name = "ReputationUI";
            ui.AddChild(reputationUI);

            // Team Skill UI
            var teamSkillUI = new UI.TeamSkillUI();
            teamSkillUI.Name = "TeamSkillUI";
            ui.AddChild(teamSkillUI);

            // Shop UI
            var shopUI = new UI.ShopUI();
            shopUI.Name = "ShopUI";
            ui.AddChild(shopUI);

            // Fishing System
            var fishingSystem = new Crafting.FishingSystem();
            fishingSystem.Name = "FishingSystem";
            AddChild(fishingSystem);

            // Fishing UI
            var fishingUI = new UI.FishingUI();
            fishingUI.Name = "FishingUI";
            ui.AddChild(fishingUI);

            // Alchemy System
            var alchemySystem = Systems.AlchemySystem.Instance;
            alchemySystem.Initialize();

            // Alchemy UI
            var alchemyUI = new Systems.AlchemyUI();
            alchemyUI.Name = "AlchemyUI";
            ui.AddChild(alchemyUI);

            // Alchemy Laboratory UI
            var alchemyLabUI = new AlchemyLaboratoryUI();
            alchemyLabUI.Name = "AlchemyLaboratoryUI";
            alchemyLabUI.Visible = false;
            ui.AddChild(alchemyLabUI);

            // Cooking System
            var cookingSystem = new Systems.Cooking.CookingSystem();
            cookingSystem.Name = "CookingSystem";
            AddChild(cookingSystem);

            // Cooking UI
            var cookingUI = new Systems.Cooking.CookingUI();
            cookingUI.Name = "CookingUI";
            ui.AddChild(cookingUI);

            // Mount Combat UI
            var mountCombatUI = new UI.MountCombatUI();
            mountCombatUI.Name = "MountCombatUI";
            ui.AddChild(mountCombatUI);

            // Mount Evolution UI
            var mountEvolutionUI = new UI.MountEvolutionUI();
            mountEvolutionUI.Name = "MountEvolutionUI";
            ui.AddChild(mountEvolutionUI);

            // Mount Equipment UI
            var mountEquipmentUI = new Systems.MountEquipmentUI();
            mountEquipmentUI.Name = "MountEquipmentUI";
            ui.AddChild(mountEquipmentUI);

            // Random World Event UI
            var worldEventUI = new Systems.RandomWorldEventUI();
            worldEventUI.Name = "WorldEventUI";
            ui.AddChild(worldEventUI);

            // Title UI
            var titleUI = new Systems.TitleUI();
            titleUI.Name = "TitleUI";
            ui.AddChild(titleUI);

            // Player Talent UI
            var playerTalentUI = new Systems.PlayerTalent.PlayerTalentUI();
            playerTalentUI.Name = "PlayerTalentUI";
            ui.AddChild(playerTalentUI);

            // Mount Race UI
            var mountRaceUI = new MountRaceUI();
            mountRaceUI.Name = "MountRaceUI";
            ui.AddChild(mountRaceUI);

            // Mount Battle Arena UI
            var mountBattleArenaUI = new Systems.MountBattleArenaUI();
            mountBattleArenaUI.Name = "MountBattleArenaUI";
            mountBattleArenaUI.Hide();
            ui.AddChild(mountBattleArenaUI);

            // Mount Weather Bonus UI
            var mountWeatherBonusUI = new UI.MountWeatherBonusUI();
            mountWeatherBonusUI.Name = "MountWeatherBonusUI";
            mountWeatherBonusUI.Hide();
            ui.AddChild(mountWeatherBonusUI);

            // Guild UI
            var guildUI = new GuildUI();
            guildUI.Name = "GuildUI";
            ui.AddChild(guildUI);

            // Guild Quest UI
            var guildQuestUI = new GuildQuestUI();
            guildQuestUI.Name = "GuildQuestUI";
            ui.AddChild(guildQuestUI);

            // Guild Bank UI
            var guildBankUI = new GuildBankUI();
            guildBankUI.Name = "GuildBankUI";
            guildBankUI.Hide();
            ui.AddChild(guildBankUI);

            // Guild Technology UI
            var guildTechnologyUI = new GuildTechnologyUI();
            guildTechnologyUI.Name = "GuildTechnologyUI";
            guildTechnologyUI.Hide();
            ui.AddChild(guildTechnologyUI);

            // Multiplayer Leaderboard UI
            var leaderboardUI = new UI.MultiplayerLeaderboardUI();
            leaderboardUI.Name = "LeaderboardUI";
            ui.AddChild(leaderboardUI);

            // Trade UI
            var tradeUI = new UI.TradeUI();
            tradeUI.Name = "TradeUI";
            ui.AddChild(tradeUI);

            // Daily Login Reward UI
            var dailyLoginRewardUI = new DailyLoginRewardUI();
            dailyLoginRewardUI.Name = "DailyLoginRewardUI";
            ui.AddChild(dailyLoginRewardUI);

            // Gem UI
            var gemUI = new Systems.GemSystem.GemUI();
            gemUI.Name = "GemUI";
            ui.AddChild(gemUI);

            // Gem Fusion UI
            var gemFusionUI = new Systems.GemSystem.GemFusionUI();
            gemFusionUI.Name = "GemFusionUI";
            gemFusionUI.Visible = false; 
            ui.AddChild(gemFusionUI);

            // Costume UI
            var costumeUI = new UI.CostumeUI();
            costumeUI.Name = "CostumeUI";
            ui.AddChild(costumeUI);

            // Relic UI
            var relicUI = new Systems.RelicUI();
            relicUI.Name = "RelicUI";
            relicUI.Visible = false; 
            ui.AddChild(relicUI);

            // Equipment Enhancement System
            var enhancementSystem = Systems.EquipmentEnhancementSystem.Instance;
            enhancementSystem.Initialize();

            // Equipment Enhancement UI
            var enhancementUI = new Systems.EquipmentEnhancementUI();
            enhancementUI.Name = "EquipmentEnhancementUI";
            enhancementUI.Visible = false; 
            ui.AddChild(enhancementUI);

            // Pet Equipment UI
            var petEquipmentUI = new Systems.Pets.PetEquipmentUI();
            petEquipmentUI.Name = "PetEquipmentUI";
            petEquipmentUI.Visible = false; 
            ui.AddChild(petEquipmentUI);

            // Pet Equipment Enhancement UI
            var petEquipmentEnhancementUI = new Systems.PetEquipment.PetEquipmentEnhancementUI();
            petEquipmentEnhancementUI.Name = "PetEquipmentEnhancementUI";
            petEquipmentEnhancementUI.Visible = false; 
            ui.AddChild(petEquipmentEnhancementUI);

            // Pet Evolution UI
            var petEvolutionUI = new Systems.PetEvolution.PetEvolutionUI();
            petEvolutionUI.Name = "PetEvolutionUI";
            petEvolutionUI.Visible = false; 
            ui.AddChild(petEvolutionUI);

            // Pet Talent UI
            var petTalentUI = new Systems.PetTalentUI();
            petTalentUI.Name = "PetTalentUI";
            petTalentUI.Visible = false; 
            ui.AddChild(petTalentUI);

            // Pet Affection UI
            var petAffectionUI = new Systems.PetAffectionUI();
            petAffectionUI.Name = "PetAffectionUI";
            petAffectionUI.Visible = false; 
            ui.AddChild(petAffectionUI);

            // Pet Foster UI
            var petFosterUI = new Systems.PetFoster.PetFosterUI();
            petFosterUI.Name = "PetFosterUI";
            petFosterUI.Visible = false; 
            ui.AddChild(petFosterUI);

            // Pet Skill UI
            var petSkillUI = new PetSkillUI();
            petSkillUI.Name = "PetSkillUI";
            petSkillUI.Visible = false; 
            ui.AddChild(petSkillUI);

            // Pet Expedition UI
            var petExpeditionUI = new PetExpeditionUI();
            petExpeditionUI.Name = "PetExpeditionUI";
            petExpeditionUI.Visible = false; 
            ui.AddChild(petExpeditionUI);

            // Mount Expedition UI
            var mountExpeditionUI = new MountExpeditionUI();
            mountExpeditionUI.Name = "MountExpeditionUI";
            mountExpeditionUI.Visible = false; 
            ui.AddChild(mountExpeditionUI);

            // Mystery Treasure UI
            var mysteryTreasureUI = new MysteryTreasureUI();
            mysteryTreasureUI.Name = "MysteryTreasureUI";
            mysteryTreasureUI.Visible = false; 
            ui.AddChild(mysteryTreasureUI);

            // Ranked UI
            var rankedUI = new RankedUI();
            rankedUI.Name = "RankedUI";
            rankedUI.Visible = false;
            ui.AddChild(rankedUI);

            // Dynamic Difficulty UI
            var dynamicDifficultyUI = new DynamicDifficultyUI();
            dynamicDifficultyUI.Name = "DynamicDifficultyUI";
            dynamicDifficultyUI.Visible = false; 
            ui.AddChild(dynamicDifficultyUI);

            // World Boss UI
            var worldBossUI = new Systems.WorldBoss.WorldBossUI();
            worldBossUI.Name = "WorldBossUI";
            worldBossUI.Visible = false; 
            ui.AddChild(worldBossUI);

            // Choice Event UI
            var choiceEventUI = new Systems.ChoiceEvents.ChoiceEventUI();
            choiceEventUI.Name = "ChoiceEventUI";
            ui.AddChild(choiceEventUI);

            // Elemental Trial UI
            var elementalTrialUI = new Systems.ElementalTrialUI();
            elementalTrialUI.Name = "ElementalTrialUI";
            elementalTrialUI.Visible = false; 
            ui.AddChild(elementalTrialUI);

            // Pet Battle Arena UI
            var petBattleArenaUI = new Systems.PetBattleArena.PetBattleArenaUI();
            petBattleArenaUI.Name = "PetBattleArenaUI";
            petBattleArenaUI.Visible = false; 
            ui.AddChild(petBattleArenaUI);

            // Pet Morph UI
            var petMorphUI = new Systems.PetMorph.PetMorphUI();
            petMorphUI.Name = "PetMorphUI";
            petMorphUI.Visible = false;
            ui.AddChild(petMorphUI);

            // Daily Dungeon UI
            var dailyDungeonUI = new Systems.DailyDungeon.DailyDungeonUI();
            dailyDungeonUI.Name = "DailyDungeonUI";
            dailyDungeonUI.Visible = false; 
            ui.AddChild(dailyDungeonUI);

            // Random Boon UI
            var randomBoonUI = new UI.RandomBoonUI();
            randomBoonUI.Name = "RandomBoonUI";
            randomBoonUI.Visible = false; 
            ui.AddChild(randomBoonUI);

            // Daily Quest UI
            var dailyQuestUI = new Systems.DailyQuest.DailyQuestUI();
            dailyQuestUI.Name = "DailyQuestUI";
            dailyQuestUI.Visible = false; 
            ui.AddChild(dailyQuestUI);

            // Procedural Challenge UI
            var proceduralChallengeUI = new Systems.ProceduralChallengeUI();
            proceduralChallengeUI.Name = "ProceduralChallengeUI";
            proceduralChallengeUI.Visible = false; 
            ui.AddChild(proceduralChallengeUI);

            // Loot Drop UI
            var lootDropUI = new Systems.LootDropUI();
            lootDropUI.Name = "LootDropUI";
            lootDropUI.Visible = false; 
            ui.AddChild(lootDropUI);

            // Equipment Durability System
            var durabilitySystem = new Systems.EquipmentDurability.EquipmentDurabilitySystem();
            durabilitySystem.Name = "EquipmentDurabilitySystem";
            AddChild(durabilitySystem);

            // Equipment Durability UI
            var durabilityUI = new Systems.EquipmentDurability.EquipmentDurabilityUI();
            durabilityUI.Name = "EquipmentDurabilityUI";
            durabilityUI.Visible = false; 
            ui.AddChild(durabilityUI);

            // Equipment Recycle UI
            var equipmentRecycleUI = new Systems.EquipmentRecycle.EquipmentRecycleUI();
            equipmentRecycleUI.Name = "EquipmentRecycleUI";
            equipmentRecycleUI.Visible = false; 
            ui.AddChild(equipmentRecycleUI);

            // Buff System
            var buffSystem = new Systems.BuffSystem.BuffSystem();
            buffSystem.Name = "BuffSystem";
            AddChild(buffSystem);

            // Buff UI
            var buffUI = new Systems.BuffSystem.BuffUI();
            buffUI.Name = "BuffUI";
            buffUI.Visible = false; 
            ui.AddChild(buffUI);

            // Boss Mechanics UI
            var bossMechanicsUI = new Systems.BossMechanics.BossMechanicsUI();
            bossMechanicsUI.Name = "BossMechanicsUI";
            bossMechanicsUI.Visible = false; 
            ui.AddChild(bossMechanicsUI);

            // Rune UI
            var runeUI = new UI.RuneUI();
            runeUI.Name = "RuneUI";
            runeUI.Visible = false; 
            ui.AddChild(runeUI);

            // Music Collection UI
            var musicCollectionUI = new UI.MusicCollectionUI();
            musicCollectionUI.Name = "MusicCollectionUI";
            musicCollectionUI.Visible = false;
            ui.AddChild(musicCollectionUI);

            // Gathering UI
            var gatheringUI = new GatheringUI();
            gatheringUI.Name = "GatheringUI";
            gatheringUI.Visible = false;
            ui.AddChild(gatheringUI);

            // Initialize Monster Taming UI
            var monsterTamingUI = new MonsterTamingUI();
            monsterTamingUI.Name = "MonsterTamingUI";
            monsterTamingUI.Visible = false;
            ui.AddChild(monsterTamingUI);

            // Initialize Prestige UI
            var prestigeUI = new PrestigeUI();
            prestigeUI.Name = "PrestigeUI";
            prestigeUI.Visible = false;
            ui.AddChild(prestigeUI);

            // Initialize Identification UI
            var identificationUI = new IdentificationUI();
            identificationUI.Name = "IdentificationUI";
            identificationUI.Visible = false;
            ui.AddChild(identificationUI);

            GD.Print("UI initialized");

            // Survival Challenge System
            var survivalChallengeSystem = new SurvivalChallengeSystem();
            survivalChallengeSystem.Name = "SurvivalChallengeSystem";
            AddChild(survivalChallengeSystem);
            survivalChallengeSystem.Initialize();

            // Survival Challenge UI
            var survivalChallengeUI = new UI.SurvivalChallengeUI();
            survivalChallengeUI.Name = "SurvivalChallengeUI";
            survivalChallengeUI.Visible = false;
            ui.AddChild(survivalChallengeUI);

            // Arena Colosseum UI
            var arenaColosseumUI = new Systems.ArenaColosseumSystem.ArenaColosseumUI();
            arenaColosseumUI.Name = "ArenaColosseumUI";
            arenaColosseumUI.Visible = false;
            ui.AddChild(arenaColosseumUI);

            // Party System UI
            var partyUI = new Systems.PartySystem.PartyUI();
            partyUI.Name = "PartyUI";
            partyUI.Visible = false;
            ui.AddChild(partyUI);

            // Emote System
            var emoteSystem = new Systems.Emote.EmoteSystem();
            emoteSystem.Name = "EmoteSystem";
            AddChild(emoteSystem);

            // Emote UI
            var emoteUI = new Systems.Emote.EmoteUI();
            emoteUI.Name = "EmoteUI";
            emoteUI.Visible = false;
            ui.AddChild(emoteUI);

            // Economic Dashboard System
            var economicDashboardSystem = new EconomicDashboardSystem();
            economicDashboardSystem.Name = "EconomicDashboardSystem";
            AddChild(economicDashboardSystem);

            // Economic Dashboard UI
            var economicDashboardUI = new EconomicDashboardUI();
            economicDashboardUI.Name = "EconomicDashboardUI";
            economicDashboardUI.Visible = false;
            ui.AddChild(economicDashboardUI);

            // Trigger welcome tutorial
            var tutorialUI = GetNodeOrNull<UI.TutorialUI>("CanvasLayer/TutorialUI");
            if (tutorialUI != null)
            {
                tutorialUI.TriggerTutorial(TutorialTrigger.GameStart);
            }
        }

        private void LoadGameData()
        {
            // Load player data if exists
            var saveSystem = new SaveSystem();
            if (saveSystem.HasSave(0))
            {
                GD.Print("Found save file, loading...");
                var data = saveSystem.LoadGame(0);
                if (data != null)
                {
                    // Load statistics
                    var statsData = new Dictionary<string, object>
                    {
                        ["TotalKills"] = data.TotalKills,
                        ["TotalDeaths"] = data.TotalDeaths,
                        ["TotalDamageDealt"] = data.TotalDamageDealt,
                        ["TotalDamageTaken"] = data.TotalDamageTaken,
                        ["TotalHealing"] = data.TotalHealing,
                        ["CriticalHits"] = data.CriticalHits,
                        ["PerfectBlocks"] = data.PerfectBlocks,
                        ["Dodges"] = data.Dodges,
                        ["GoldEarned"] = data.GoldEarned,
                        ["GoldSpent"] = data.GoldSpent,
                        ["ExperienceGained"] = data.ExperienceGained,
                        ["ItemsCollected"] = data.ItemsCollected,
                        ["ItemsCrafted"] = data.ItemsCrafted,
                        ["QuestsCompleted"] = data.QuestsCompleted,
                        ["SkillsLearned"] = data.SkillsLearned,
                        ["SkillsUsed"] = data.SkillsUsed,
                        ["RegionsDiscovered"] = data.RegionsDiscovered,
                        ["EnemiesEncountered"] = data.EnemiesEncountered,
                        ["BossesDefeated"] = data.BossesDefeated,
                        ["TotalPlayTime"] = data.TotalPlayTime,
                        ["HighestLevel"] = data.HighestLevel,
                        ["HighestCombo"] = data.HighestCombo,
                        ["AchievementsUnlocked"] = data.AchievementsUnlocked
                    };
                    StatisticsManager.Instance.LoadStatistics(statsData);
                    GD.Print("Statistics loaded successfully!");

                    // Load combo system data
                    var comboSystem = GetNodeOrNull<ComboSystem>("ComboSystem");
                    if (comboSystem != null && data.ComboData != null)
                    {
                        comboSystem.Deserialize(data.ComboData);
                        GD.Print("Combo data loaded successfully!");
                    }

                    // Load keybinding data
                    var keybindingSystem = GetNodeOrNull<Systems.KeybindingSystem>("KeybindingSystem");
                    if (keybindingSystem != null && data.KeybindingData != null)
                    {
                        keybindingSystem.Deserialize(data.KeybindingData);
                        GD.Print("Keybinding data loaded successfully!");
                    }
                    
                    // Load pet story data
                    var petStorySystem = GetNodeOrNull<PetStorySystem>("PetStorySystem");
                    if (petStorySystem != null && data.PetStoryData != null)
                    {
                        petStorySystem.Deserialize(data.PetStoryData);
                        GD.Print("Pet story data loaded successfully!");
                    }
                }
            }
        }

        private float _autoSaveTimer = 0f;
        private const float AutoSaveInterval = 300f; // 5 minutes

        public override void _Process(double delta)
        {
            float dt = (float)delta;

            // Update boss mechanics system
            var bossMechanicsSystem = GetNode<Systems.BossMechanics.BossMechanicsSystem>("BossMechanicsSystem");
            if (bossMechanicsSystem != null)
            {
                bossMechanicsSystem._Process(dt);
            }

            // Update combat status system
            CombatStatusSystem.Instance._Process(dt);

            // Update cooking system
            var cookingSystem = GetNodeOrNull<Systems.Cooking.CookingSystem>("CookingSystem");
            if (cookingSystem != null)
            {
                cookingSystem.UpdateCooking(dt);
            }

            // Update survival challenge system
            var survivalChallengeSystem = GetNode<SurvivalChallengeSystem>("SurvivalChallengeSystem");
            if (survivalChallengeSystem != null)
            {
                survivalChallengeSystem._Process(dt);
            }

            // Update play time
            StatisticsManager.Instance.AddPlayTime(dt);

            // Auto save every 5 minutes
            _autoSaveTimer += dt;
            if (_autoSaveTimer >= AutoSaveInterval)
            {
                _autoSaveTimer = 0f;
                // Auto save logic would go here
                GD.Print("Auto save triggered...");
            }

            // Update UI
            UpdatePlayerUI();

            // Update potion effects
            if (_player != null)
            {
                PotionManager.Instance.UpdatePotionEffects(dt, _player);
            }

            // Handle runes UI toggle (U key)
            if (Input.IsActionJustPressed("runes"))
            {
                ToggleRunesUI();
            }

            // Handle quest tracker toggle (T key)
            if (Input.IsActionJustPressed("quest_tracker"))
            {
                ToggleQuestTracker();
            }

            // Handle quest guide toggle (G key)
            if (Input.IsActionJustPressed("quest_guide"))
            {
                ToggleQuestGuide();
            }

            // Handle multiplayer UI toggle (M key)
            if (Input.IsActionJustPressed("multiplayer"))
            {
                ToggleMultiplayerUI();
            }

            // Handle weapon mastery UI toggle (W key)
            if (Input.IsActionJustPressed("weapon_mastery"))
            {
                ToggleWeaponMasteryUI();
            }

            // Handle counter attack UI toggle (Shift+C key)
            if (Input.IsActionJustPressed("counter_attack"))
            {
                ToggleCounterAttackUI();
            }

            // Handle mount UI toggle (O key)
            if (Input.IsActionJustPressed("mounts"))
            {
                ToggleMountUI();
            }

            // Handle skill cooldown UI toggle (K key)
            if (Input.IsKeyPressed(Key.K))
            {
                ToggleSkillCooldownUI();
            }

            // Handle momentum UI toggle (M key)
            if (Input.IsKeyPressed(Key.M))
            {
                ToggleMomentumUI();
            }

            // Handle choice event UI toggle (C key)
            if (Input.IsKeyPressed(Key.C))
            {
                ToggleChoiceEventUI();
            }

            // Handle alchemy laboratory UI toggle (Ctrl+L key)
            if (Input.IsKeyPressed(Key.L) && Input.IsKeyPressed(Key.Ctrl))
            {
                ToggleAlchemyLaboratoryUI();
            }

            // Handle artifact UI toggle (K key - when not in combat)
            if (Input.IsKeyPressed(Key.K))
            {
                ToggleArtifactUI();
            }

            // Handle weather UI toggle (Ctrl+W key)
            if (Input.IsKeyPressed(Key.W) && Input.IsKeyPressed(Key.Ctrl))
            {
                ToggleWeatherUI();
            }

            // Handle title UI toggle (Y key)
            if (Input.IsActionJustPressed("titles"))
            {
                ToggleTitleUI();
            }

            // Handle daily ritual UI toggle (Ctrl+Shift+R key)
            if (Input.IsKeyPressed(Key.R) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                ToggleDailyRitualUI();
            }

            // Handle weekly challenge UI toggle (Ctrl+Shift+W key)
            if (Input.IsKeyPressed(Key.W) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                ToggleWeeklyChallengeUI();
            }

            // Handle bookmarks UI toggle (N key)
            if (Input.IsActionJustPressed("bookmarks"))
            {
                ToggleBookmarkUI();
            }

            // Handle auto bookmark settings toggle (Shift+N key)
            if (Input.IsActionJustPressed("auto_bookmark"))
            {
                ToggleAutoBookmarkUI();
            }

            // Handle equipment set UI toggle (Shift+S key)
            if (Input.IsActionJustPressed("enhancement"))
            {
                ToggleEnhancementUI();
            }

            // Handle equipment set UI toggle (J key)
            if (Input.IsActionJustPressed("ui_equipment_set"))
            {
                ToggleEquipmentSetUI();
            }

            // Handle player talent UI toggle (T key)
            if (Input.IsActionJustPressed("ui_talent"))
            {
                TogglePlayerTalentUI();
            }

            // Handle mount race UI toggle (Shift+R key)
            if (Input.IsActionJustPressed("ui_mount_race"))
            {
                ToggleMountRaceUI();
            }

            // Handle mount battle arena UI toggle (Ctrl+M key)
            if (Input.IsActionJustPressed("ui_mount_arena"))
            {
                ToggleMountBattleArenaUI();
            }

            // Handle mount weather bonus UI toggle (Ctrl+Shift+W key)
            if (Input.IsKeyPressed(Key.W) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.Shift))
            {
                ToggleMountWeatherBonusUI();
            }

            // Handle auto potion UI toggle (Shift+X key)
            if (Input.IsActionJustPressed("auto_potion"))
            {
                ToggleAutoPotionUI();
            }

            // Handle potion UI toggle (P key)
            if (Input.IsActionJustPressed("potion"))
            {
                TogglePotionUI();
            }

            // Handle enchantment UI toggle (E key - secondary action)
            if (Input.IsActionJustPressed("enchantment"))
            {
                ToggleEnchantmentUI();
            }

            // Handle bounty UI toggle (B key)
            if (Input.IsActionJustPressed("bounty"))
            {
                ToggleBountyUI();
            }

            // Handle equipment visuals UI toggle (V key)
            if (Input.IsActionJustPressed("equip_visuals"))
            {
                ToggleEquipmentVisualsUI();
            }

            // Handle combat VFX UI toggle (Shift+V key)
            if (Input.IsKeyPressed(Key.Shift) && Input.IsKeyPressed(Key.V))
            {
                ToggleCombatVFXUI();
            }

            // Handle story UI toggle (K key)
            if (Input.IsActionJustPressed("story"))
            {
                ToggleStoryUI();
            }

            // Handle sealed tower UI toggle (Ctrl+T key)
            if (Input.IsKeyPressed(Key.Control) && Input.IsKeyPressed(Key.T))
            {
                ToggleSealedTowerUI();
            }

            // Handle faction UI toggle (F key)
            if (Input.IsKeyPressed(Key.F) && !Input.IsKeyPressed(Key.Control))
            {
                ToggleFactionUI();
            }

            // Handle treasure hunt UI toggle (H key)
            if (Input.IsKeyPressed(Key.H))
            {
                ToggleTreasureHuntUI();
            }

            // Handle crafting mastery UI toggle (Ctrl+M key)
            if (Input.IsKeyPressed(Key.M) && Input.IsKeyPressed(Key.Ctrl))
            {
                ToggleCraftingMasteryUI();
            }

            // Handle dice master UI toggle (D key)
            if (Input.IsKeyPressed(Key.D))
            {
                ToggleDiceMasterUI();
            }

            // Handle ranked UI toggle (Shift+K key)
            if (Input.IsKeyPressed(Key.Shift) && Input.IsKeyPressed(Key.K))
            {
                ToggleRankedUI();
            }

            // Handle music collection UI toggle (Shift+M key)
            if (Input.IsKeyPressed(Key.Shift) && Input.IsKeyPressed(Key.M))
            {
                ToggleMusicCollectionUI();
            }

            // Handle gathering UI toggle (Shift+G key)
            if (Input.IsKeyPressed(Key.Shift) && Input.IsKeyPressed(Key.G))
            {
                ToggleGatheringUI();
            }

            // Handle monster taming UI toggle (T key)
            if (Input.IsKeyPressed(Key.T))
            {
                ToggleMonsterTamingUI();
            }

            // Handle prestige UI toggle (Ctrl+P key)
            if (Input.IsKeyPressed(Key.Control) && Input.IsKeyPressed(Key.P))
            {
                TogglePrestigeUI();
            }

            // Handle identification UI toggle (I key)
            if (Input.IsKeyPressed(Key.I))
            {
                ToggleIdentificationUI();
            }

            // Handle player profile UI toggle (F key)
            if (Input.IsActionJustPressed("player_profile"))
            {
                TogglePlayerProfileUI();
            }

            // Handle hotkey HUD toggle (F1 key) - 显示快捷键指南
            if (Input.IsKeyPressed(Key.F1))
            {
                ToggleHotkeyHUD();
            }

            // Handle keybinding UI toggle (F10 key)
            if (Input.IsActionJustPressed("keybinding"))
            {
                ToggleKeybindingUI();
            }

            // Handle settings UI toggle (F11 key)
            if (Input.IsActionJustPressed("balance"))
            {
                OpenSettingsUI();
            }

            // Handle tutorial UI toggle (F9 key) - 查看快捷键教程
            if (Input.IsActionJustPressed("tutorial"))
            {
                var tutorialUI = GetNodeOrNull<UI.TutorialUI>("CanvasLayer/TutorialUI");
                if (tutorialUI != null)
                {
                    tutorialUI.StartTutorialById("hotkeys");
                }
            }

            // Handle team skill UI toggle (T key)
            if (Input.IsActionJustPressed("team_skill"))
            {
                var teamSkillUI = GetNodeOrNull<UI.TeamSkillUI>("CanvasLayer/TeamSkillUI");
                if (teamSkillUI != null)
                {
                    teamSkillUI.Toggle();
                }
            }

            // Handle team skill hotkeys (1-9, 0, -, =, ])
            HandleTeamSkillInput();

            // Handle combat stats panel toggle (F12 key)
            if (Input.IsActionJustPressed("combat_stats"))
            {
                var combatStatsPanel = GetNodeOrNull<UI.CombatStatsPanel>("CanvasLayer/CombatStatsPanel");
                if (combatStatsPanel != null)
                {
                    combatStatsPanel.Toggle();
                }
            }

            // Handle reputation UI toggle (R key)
            if (Input.IsActionJustPressed("ui_reputation"))
            {
                ToggleReputationUI();
            }

            // Handle achievement badge UI toggle (B key)
            if (Input.IsActionJustPressed("ui_badges"))
            {
                ToggleBadgeUI();
            }

            // Handle secret achievement UI toggle (Ctrl+S key)
            if (Input.IsActionJustPressed("ui_secret_achievements"))
            {
                ToggleSecretAchievementUI();
            }

            // Handle collectible UI toggle (K key)
            if (Input.IsActionJustPressed("ui_collectible"))
            {
                ToggleCollectibleUI();
            }

            // Handle mail UI toggle (M key)
            if (Input.IsActionJustPressed("ui_mail"))
            {
                ToggleMailUI();
            }

            // Handle shop UI toggle (H key)
            if (Input.IsActionJustPressed("ui_shop"))
            {
                ToggleShopUI();
            }

            // Handle guild UI toggle (G key)
            if (Input.IsActionJustPressed("ui_guild"))
            {
                ToggleGuildUI();
            }

            // Handle guild quest UI toggle (Shift+G key)
            if (Input.IsActionJustPressed("ui_guild_quest"))
            {
                ToggleGuildQuestUI();
            }

            // Handle guild bank UI toggle (Shift+B key)
            if (Input.IsActionJustPressed("ui_guild_bank"))
            {
                ToggleGuildBankUI();
            }

            // Handle guild technology UI toggle (Ctrl+Shift+T key)
            if (Input.IsActionJustPressed("ui_guild_tech"))
            {
                ToggleGuildTechnologyUI();
            }

            // Handle trade UI toggle (T key)
            if (Input.IsActionJustPressed("ui_trade"))
            {
                ToggleTradeUI();
            }

            // Handle daily login reward UI toggle (L key - using existing binding, different from Alchemy)
            if (Input.IsActionJustPressed("ui_daily_login"))
            {
                ToggleDailyLoginRewardUI();
            }

            // Handle random boon UI toggle (B key)
            if (Input.IsActionJustPressed("ui_boon"))
            {
                ToggleRandomBoonUI();
            }

            // Handle daily quest UI toggle (Q key)
            if (Input.IsActionJustPressed("ui_daily_quest"))
            {
                ToggleDailyQuestUI();
            }

            // Handle procedural challenge UI toggle (P key - using Shift+P for challenge)
            if (Input.IsActionJustPressed("ui_challenge"))
            {
                ToggleProceduralChallengeUI();
            }

            // Handle loot drop UI toggle (L key)
            if (Input.IsActionJustPressed("ui_loot"))
            {
                ToggleLootDropUI();
            }

            // Handle equipment durability UI toggle (U key - using Shift+U)
            if (Input.IsKeyPressed(KEY_SHIFT) && Input.IsKeyPressed(KEY_U))
            {
                ToggleEquipmentDurabilityUI();
            }

            // Handle equipment recycle UI toggle (R key)
            if (Input.IsKeyPressed(KEY_R))
            {
                ToggleEquipmentRecycleUI();
            }

            // Handle auction house UI toggle (Y key)
            if (Input.IsActionJustPressed("ui_auction"))
            {
                ToggleAuctionHouseUI();
            }

            // Handle enchantment UI toggle (E key)
            if (Input.IsActionJustPressed("ui_enchant"))
            {
                ToggleEnchantmentUI();
            }

            // Handle fishing UI toggle (P key)
            if (Input.IsActionJustPressed("ui_fishing"))
            {
                ToggleFishingUI();
            }

            // Handle alchemy UI toggle (L key)
            if (Input.IsActionJustPressed("ui_alchemy"))
            {
                ToggleAlchemyUI();
            }

            // Handle cooking UI toggle (Shift+C key)
            if (Input.IsKeyPressed(Key.Shift) && Input.IsActionJustPressed("ui_fishing"))
            {
                ToggleCookingUI();
            }

            // Handle mount combat UI toggle (V key)
            if (Input.IsActionJustPressed("ui_mount_combat"))
            {
                ToggleMountCombatUI();
            }

            // Handle mount evolution UI toggle (J key)
            if (Input.IsActionJustPressed("ui_mount_evolution"))
            {
                ToggleMountEvolutionUI();
            }

            // Handle mount equipment UI toggle (K key)
            if (Input.IsActionJustPressed("ui_mount_equipment"))
            {
                ToggleMountEquipmentUI();
            }

            // Handle world event UI toggle (O key)
            if (Input.IsActionJustPressed("ui_world_event"))
            {
                ToggleWorldEventUI();
            }

            // Handle gem UI toggle (Z key)
            if (Input.IsActionJustPressed("ui_gem"))
            {
                ToggleGemUI();
            }

            // Handle gem fusion UI toggle (F key)
            if (Input.IsActionJustPressed("ui_gem_fusion"))
            {
                ToggleGemFusionUI();
            }

            // Handle costume UI toggle (C key)
            if (Input.IsActionJustPressed("ui_costume"))
            {
                ToggleCostumeUI();
            }

            // Handle pet equipment UI toggle (Shift+P key)
            if (Input.IsActionJustPressed("ui_pet") && Input.IsKeyPressed(Key.Shift))
            {
                TogglePetEquipmentUI();
            }

            // Handle relic UI toggle (R key)
            if (Input.IsActionJustPressed("ui_relic"))
            {
                ToggleRelicUI();
            }

            // Handle arena tournament UI toggle (Shift+T key)
            if (Input.IsActionJustPressed("ui_tournament") && Input.IsKeyPressed(Key.Shift))
            {
                ToggleArenaTournamentUI();
            }

            // Handle arena colosseum UI toggle (Ctrl+A)
            if (Input.IsActionJustPressed("ui_colosseum"))
            {
                ToggleArenaColosseumUI();
            }

            // Handle party UI toggle (Ctrl+P)
            if (Input.IsActionJustPressed("ui_party"))
            {
                TogglePartyUI();
            }

            // Handle equipment enhancement UI toggle (E key - separate from enchantment)
            if (Input.IsActionJustPressed("ui_enhancement"))
            {
                ToggleEquipmentEnhancementUI();
            }

            // Handle pet evolution UI toggle (P key - separate from pet equipment)
            if (Input.IsActionJustPressed("ui_pet_evolution"))
            {
                TogglePetEvolutionUI();
            }

            // Handle pet talent UI toggle
            if (Input.IsActionJustPressed("ui_pet_talent"))
            {
                TogglePetTalentUI();
            }

            // Handle pet affection UI toggle (Shift+P)
            if (Input.IsActionJustPressed("ui_pet_affection"))
            {
                TogglePetAffectionUI();
            }

            // Handle pet foster UI toggle (Alt+P)
            if (Input.IsActionJustPressed("ui_pet_foster"))
            {
                TogglePetFosterUI();
            }

            // Handle pet skill UI toggle (Ctrl+P)
            if (Input.IsActionJustPressed("ui_pet_skill"))
            {
                TogglePetSkillUI();
            }

            // Handle pet expedition UI toggle (Ctrl+E)
            if (Input.IsActionJustPressed("ui_pet_expedition"))
            {
                TogglePetExpeditionUI();
            }

            // Handle pet story UI toggle (Ctrl+Shift+P)
            if (Input.IsKeyPressed(Key.Shift) && Input.IsKeyPressed(Key.Ctrl) && Input.IsKeyPressed(Key.P))
            {
                TogglePetStoryUI();
            }

            // Handle pet equipment enhancement UI toggle (Ctrl+Shift+E)
            if (Input.IsActionJustPressed("ui_pet_equipment_enhancement"))
            {
                TogglePetEquipmentEnhancementUI();
            }

            // Handle mount expedition UI toggle (Ctrl+R)
            if (Input.IsActionJustPressed("ui_mount_expedition"))
            {
                ToggleMountExpeditionUI();
            }

            // Handle mystery treasure UI toggle (T key)
            if (Input.IsActionJustPressed("ui_mystery_treasure"))
            {
                ToggleMysteryTreasureUI();
            }

            // Handle dynamic difficulty UI toggle (Shift+D key)
            if (Input.IsActionJustPressed("ui_dynamic_difficulty"))
            {
                ToggleDynamicDifficultyUI();
            }

            // Handle boss mechanics UI toggle (B key)
            if (Input.IsActionJustPressed("boss_mechanics"))
            {
                ToggleBossMechanicsUI();
            }

            // Handle elemental trial UI toggle (E key - separate from enchantment)
            if (Input.IsActionJustPressed("ui_elemental_trial"))
            {
                ToggleElementalTrialUI();
            }

            // Handle pet battle arena UI toggle (P key - separate from pet evolution)
            if (Input.IsActionJustPressed("ui_pet_battle_arena"))
            {
                TogglePetBattleArenaUI();
            }

            // Handle pet morph UI toggle (O key)
            if (Input.IsActionJustPressed("ui_pet_morph"))
            {
                TogglePetMorphUI();
            }

            // Handle pet habitat UI toggle (Ctrl+H key)
            if (Input.IsKeyPressed(Key.Control) && Input.IsKeyPressed(Key.H))
            {
                TogglePetHabitatUI();
            }

            // Handle pet egg UI toggle (Ctrl+E key)
            if (Input.IsKeyPressed(Key.Control) && Input.IsKeyPressed(Key.E))
            {
                TogglePetEggUI();
            }

            // Handle pet friendship UI toggle (Ctrl+Shift+P key)
            if (Input.IsKeyPressed(Key.Control) && Input.IsKeyPressed(Key.Shift) && Input.IsKeyPressed(Key.P))
            {
                TogglePetFriendshipUI();
            }

            // Handle daily dungeon UI toggle (D key)
            if (Input.IsActionJustPressed("ui_daily_dungeon"))
            {
                ToggleDailyDungeonUI();
            }

            // Handle survival challenge UI toggle (X key)
            if (Input.IsActionJustPressed("ui_survival_challenge"))
            {
                ToggleSurvivalChallengeUI();
            }

            // Handle buff UI toggle (V key)
            if (Input.IsActionJustPressed("ui_buff"))
            {
                ToggleBuffUI();
            }

            // Handle seasonal event UI toggle (E key with shift - Shift+E)
            if (Input.IsActionJustPressed("ui_seasonal_event"))
            {
                ToggleSeasonalEventUI();
            }

            // Handle hit stop UI toggle (Ctrl+Shift+H)
            if (Input.IsActionJustPressed("hit_stop_toggle"))
            {
                ToggleHitStopUI();
            }

            // Handle emote UI toggle (E key)
            if (Input.IsKeyPressed(Key.E))
            {
                ToggleEmoteUI();
            }

            // Handle economic dashboard UI toggle (Shift+E key)
            if (Input.IsKeyPressed(Key.Shift) && Input.IsKeyPressed(Key.E))
            {
                ToggleEconomicDashboardUI();
            }

            // Handle title UI toggle (N key)
            if (Input.IsActionJustPressed("ui_title"))
            {
                ToggleTitleUI();
            }

            // Handle special attacks
            if (Input.IsActionJustPressed("spin_attack"))
            {
                TrySpinAttack();
            }

            if (Input.IsActionJustPressed("charge_attack"))
            {
                TryChargeAttack();
            }

            // Handle pause
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                TogglePause();
            }

            // Handle world boss UI toggle (W key)
            if (Input.IsActionJustPressed("ui_world_boss"))
            {
                ToggleWorldBossUI();
            }
        }

        private void ToggleRunesUI()
        {
            var runeUI = GetNodeOrNull<UI.RuneUI>("CanvasLayer/RuneUI");
            if (runeUI != null)
            {
                runeUI.Toggle();
            }
        }

        private void ToggleQuestTracker()
        {
            var questTracker = GetNodeOrNull<UI.QuestTrackerUI>("CanvasLayer/QuestTrackerUI");
            if (questTracker != null)
            {
                questTracker.Toggle();
            }
        }

        private void ToggleQuestGuide()
        {
            var questGuide = GetNodeOrNull<UI.QuestGuideArrow>("CanvasLayer/QuestGuideArrow");
            if (questGuide != null)
            {
                questGuide.Toggle();
            }
        }

        private void ToggleMultiplayerUI()
        {
            var multiplayerUI = GetNodeOrNull<UI.MultiplayerUI>("CanvasLayer/MultiplayerUI");
            if (multiplayerUI != null)
            {
                multiplayerUI.Toggle();
            }
        }

        private void ToggleWeaponMasteryUI()
        {
            var weaponMasteryUI = GetNodeOrNull<UI.WeaponMasteryUI>("CanvasLayer/WeaponMasteryUI");
            if (weaponMasteryUI != null)
            {
                weaponMasteryUI.Toggle();
            }
        }

        private void ToggleCounterAttackUI()
        {
            var counterAttackUI = GetNodeOrNull<UI.CounterAttackUI>("CanvasLayer/CounterAttackUI");
            if (counterAttackUI != null)
            {
                counterAttackUI.Toggle();
            }
        }

        private void ToggleMountUI()
        {
            var mountUI = GetNodeOrNull<UI.MountUI>("CanvasLayer/MountUI");
            if (mountUI != null)
            {
                mountUI.ToggleUI();
            }
        }

        private void ToggleSkillCooldownUI()
        {
            var skillCooldownUI = GetNodeOrNull<UI.CombatSkillCooldownUI>("CanvasLayer/CombatSkillCooldownUI");
            if (skillCooldownUI != null)
            {
                skillCooldownUI.Toggle();
            }
        }

        private void ToggleMomentumUI()
        {
            var momentumUI = GetNodeOrNull<MomentumUI>("CanvasLayer/MomentumUI");
            if (momentumUI != null)
            {
                momentumUI.ToggleVisibility();
            }
        }

        private void ToggleChoiceEventUI()
        {
            var choiceEventUI = GetNodeOrNull<Systems.ChoiceEvents.ChoiceEventUI>("CanvasLayer/ChoiceEventUI");
            if (choiceEventUI != null)
            {
                choiceEventUI.ToggleVisibility();
            }
        }

        private void ToggleMusicCollectionUI()
        {
            var musicCollectionUI = GetNodeOrNull<UI.MusicCollectionUI>("CanvasLayer/MusicCollectionUI");
            if (musicCollectionUI != null)
            {
                musicCollectionUI.Visible = !musicCollectionUI.Visible;
            }
        }

        private void ToggleGatheringUI()
        {
            var gatheringUI = GetNodeOrNull<GatheringUI>("CanvasLayer/GatheringUI");
            if (gatheringUI != null)
            {
                gatheringUI.ToggleUI();
            }
        }

        private void ToggleMonsterTamingUI()
        {
            var monsterTamingUI = GetNodeOrNull<MonsterTamingUI>("CanvasLayer/MonsterTamingUI");
            if (monsterTamingUI != null)
            {
                monsterTamingUI.ToggleUI();
            }
        }

        private void TogglePrestigeUI()
        {
            var prestigeUI = GetNodeOrNull<PrestigeUI>("CanvasLayer/PrestigeUI");
            if (prestigeUI != null)
            {
                if (prestigeUI.Visible)
                {
                    prestigeUI.Hide();
                }
                else
                {
                    prestigeUI.Show();
                }
            }
        }

        private void ToggleIdentificationUI()
        {
            var identificationUI = GetNodeOrNull<IdentificationUI>("CanvasLayer/IdentificationUI");
            if (identificationUI != null)
            {
                identificationUI.ToggleUI();
            }
        }

        private void ToggleTitleUI()
        {
            var titleUI = GetNodeOrNull<Systems.TitleUI>("CanvasLayer/TitleUI");
            if (titleUI != null)
            {
                titleUI.ToggleUI();
            }
        }

        private void ToggleBookmarkUI()
        {
            var bookmarkUI = GetNodeOrNull<UI.BookmarkUI>("CanvasLayer/BookmarkUI");
            if (bookmarkUI != null)
            {
                bookmarkUI.ToggleVisibility();
            }
        }

        private void ToggleAutoBookmarkUI()
        {
            var autoBookmarkUI = GetNodeOrNull<UI.AutoBookmarkUI>("CanvasLayer/AutoBookmarkUI");
            if (autoBookmarkUI != null)
            {
                autoBookmarkUI.Toggle();
            }
        }

        private void ToggleEnhancementUI()
        {
            var enhancementUI = GetNodeOrNull<UI.EnhancementUI>("CanvasLayer/EnhancementUI");
            if (enhancementUI != null)
            {
                if (enhancementUI.Visible)
                {
                    enhancementUI.Hide();
                }
                else
                {
                    enhancementUI.Show();
                }
            }
        }

        private void ToggleAutoPotionUI()
        {
            var autoPotionUI = GetNodeOrNull<UI.AutoPotionUI>("CanvasLayer/AutoPotionUI");
            if (autoPotionUI != null)
            {
                autoPotionUI.ToggleVisibility();
            }
        }

        private void ToggleEnchantmentUI()
        {
            var enchantmentUI = GetNodeOrNull<UI.EnchantmentUI>("CanvasLayer/EnchantmentUI");
            if (enchantmentUI != null)
            {
                enchantmentUI.Toggle();
            }
        }

        /// <summary>
        /// 切换声望界面
        /// </summary>
        private void ToggleFactionUI()
        {
            var factionUI = GetNodeOrNull<FactionUI>("UI/FactionUI");
            if (factionUI != null)
            {
                factionUI.Toggle();
            }
        }

        /// <summary>
        /// 切换钓鱼界面
        /// </summary>
        private void ToggleFishingUI()
        {
            var fishingUI = GetNodeOrNull<UI.FishingUI>("UI/FishingUI");
            if (fishingUI != null)
            {
                if (fishingUI.Visible)
                {
                    fishingUI.HideFishingUI();
                }
                else
                {
                    fishingUI.ShowFishingUI();
                }
            }
        }

        /// <summary>
        /// 切换炼金界面
        /// </summary>
        private void ToggleAlchemyUI()
        {
            var alchemyUI = GetNodeOrNull<Systems.AlchemyUI>("UI/AlchemyUI");
            if (alchemyUI != null)
            {
                alchemyUI.Toggle();
            }
        }

        /// <summary>
        /// 切换烹饪界面
        /// </summary>
        private void ToggleCookingUI()
        {
            var cookingUI = GetNodeOrNull<Systems.Cooking.CookingUI>("UI/CookingUI");
            if (cookingUI != null)
            {
                cookingUI.Toggle();
            }
        }

        /// <summary>
        /// Toggle mount combat UI
        /// </summary>
        private void ToggleMountCombatUI()
        {
            var mountCombatUI = GetNodeOrNull<UI.MountCombatUI>("UI/MountCombatUI");
            if (mountCombatUI != null)
            {
                mountCombatUI.Toggle();
            }
        }

        /// <summary>
        /// Toggle mount evolution UI
        /// </summary>
        private void ToggleMountEvolutionUI()
        {
            var mountEvolutionUI = GetNodeOrNull<UI.MountEvolutionUI>("UI/MountEvolutionUI");
            if (mountEvolutionUI != null)
            {
                mountEvolutionUI.ToggleUI();
            }
        }

        /// <summary>
        /// Toggle mount equipment UI
        /// </summary>
        private void ToggleMountEquipmentUI()
        {
            var mountEquipmentUI = GetNodeOrNull<Systems.MountEquipmentUI>("UI/MountEquipmentUI");
            if (mountEquipmentUI != null)
            {
                mountEquipmentUI.ToggleUI();
            }
        }

        private void ToggleWorldEventUI()
        {
            var worldEventUI = GetNodeOrNull<Systems.RandomWorldEventUI>("UI/WorldEventUI");
            if (worldEventUI != null)
            {
                worldEventUI.ToggleVisibility();
            }
        }

        /// <summary>
        /// Toggle gem UI
        /// </summary>
        private void ToggleGemUI()
        {
            var gemUI = GetNodeOrNull<Systems.GemSystem.GemUI>("UI/GemUI");
            if (gemUI != null)
            {
                gemUI.Visible = !gemUI.Visible;
                if (gemUI.Visible)
                {
                    gemUI.RefreshUI();
                }
            }
        }

        /// <summary>
        /// Toggle gem fusion UI
        /// </summary>
        private void ToggleGemFusionUI()
        {
            var gemFusionUI = GetNodeOrNull<Systems.GemSystem.GemFusionUI>("UI/GemFusionUI");
            if (gemFusionUI != null)
            {
                gemFusionUI.Toggle();
            }
        }

        /// <summary>
        /// Toggle collectible UI
        /// </summary>
        private void ToggleCollectibleUI()
        {
            var collectibleUI = GetNodeOrNull<Systems.CollectibleUI>("UI/CollectibleUI");
            if (collectibleUI != null)
            {
                collectibleUI.QueueFree();
            }
            else
            {
                var newUI = new Systems.CollectibleUI();
                newUI.Name = "CollectibleUI";
                GetNode("UI").AddChild(newUI);
            }
        }

        /// <summary>
        /// Toggle costume UI
        /// </summary>
        private void ToggleCostumeUI()
        {
            var costumeUI = GetNodeOrNull<UI.CostumeUI>("UI/CostumeUI");
            if (costumeUI != null)
            {
                costumeUI.Toggle();
            }
        }

        /// <summary>
        /// Toggle pet equipment UI
        /// </summary>
        private void TogglePetEquipmentUI()
        {
            var petEquipmentUI = GetNodeOrNull<Systems.Pets.PetEquipmentUI>("UI/PetEquipmentUI");
            if (petEquipmentUI != null)
            {
                petEquipmentUI.Toggle();
            }
        }

        /// <summary>
        /// Toggle pet equipment enhancement UI
        /// </summary>
        private void TogglePetEquipmentEnhancementUI()
        {
            var enhancementUI = GetNodeOrNull<Systems.PetEquipment.PetEquipmentEnhancementUI>("UI/PetEquipmentEnhancementUI");
            if (enhancementUI != null)
            {
                if (enhancementUI.Visible)
                {
                    enhancementUI.Hide();
                }
                else
                {
                    enhancementUI.Show();
                }
            }
        }

        /// <summary>
        /// Toggle relic UI
        /// </summary>
        private void ToggleRelicUI()
        {
            var relicUI = GetNodeOrNull<Systems.RelicUI>("UI/RelicUI");
            if (relicUI != null)
            {
                relicUI.Toggle();
            }
        }

        /// <summary>
        /// Toggle arena tournament UI
        /// </summary>
        private void ToggleArenaTournamentUI()
        {
            var arenaTournamentUI = GetNodeOrNull<UI.ArenaTournamentUI>("UI/ArenaTournamentUI");
            if (arenaTournamentUI != null)
            {
                arenaTournamentUI.QueueFree();
            }
            else
            {
                var newUI = new UI.ArenaTournamentUI();
                newUI.Name = "ArenaTournamentUI";
                GetNode("UI").AddChild(newUI);
            }
        }

        /// <summary>
        /// Toggle arena colosseum UI
        /// </summary>
        private void ToggleArenaColosseumUI()
        {
            var arenaColosseumUI = GetNodeOrNull<Systems.ArenaColosseumSystem.ArenaColosseumUI>("CanvasLayer/ArenaColosseumUI");
            if (arenaColosseumUI != null)
            {
                arenaColosseumUI.Visible = !arenaColosseumUI.Visible;
            }
            else
            {
                var newUI = GetNodeOrNull<Systems.ArenaColosseumSystem.ArenaColosseumUI>("CanvasLayer/ArenaColosseumUI");
                if (newUI != null)
                {
                    newUI.Show();
                }
            }
        }

        /// <summary>
        /// Toggle party UI
        /// </summary>
        private void TogglePartyUI()
        {
            var partyUI = GetNodeOrNull<Systems.PartySystem.PartyUI>("CanvasLayer/PartyUI");
            if (partyUI != null)
            {
                partyUI.Toggle();
            }
        }

        /// <summary>
        /// Toggle equipment enhancement UI
        /// </summary>
        private void ToggleEquipmentEnhancementUI()
        {
            var enhancementUI = GetNodeOrNull<Systems.EquipmentEnhancementUI>("UI/EquipmentEnhancementUI");
            if (enhancementUI != null)
            {
                enhancementUI.ToggleUI();
            }
        }

        /// <summary>
        /// Toggle pet evolution UI
        /// </summary>
        private void TogglePetEvolutionUI()
        {
            var petEvolutionUI = GetNodeOrNull<Systems.PetEvolution.PetEvolutionUI>("UI/PetEvolutionUI");
            if (petEvolutionUI != null)
            {
                if (petEvolutionUI.Visible)
                {
                    petEvolutionUI.HideUI();
                }
                else
                {
                    petEvolutionUI.ShowUI();
                }
            }
        }

        /// <summary>
        /// Toggle pet talent UI
        /// </summary>
        private void TogglePetTalentUI()
        {
            var petTalentUI = GetNodeOrNull<Systems.PetTalentUI>("UI/PetTalentUI");
            if (petTalentUI != null)
            {
                petTalentUI.ToggleUI();
            }
        }

        /// <summary>
        /// Toggle pet affection UI
        /// </summary>
        private void TogglePetAffectionUI()
        {
            var petAffectionUI = GetNodeOrNull<Systems.PetAffectionUI>("UI/PetAffectionUI");
            if (petAffectionUI != null)
            {
                petAffectionUI.ToggleUI();
            }
        }

        /// <summary>
        /// Toggle pet foster UI
        /// </summary>
        private void TogglePetFosterUI()
        {
            var petFosterUI = GetNodeOrNull<Systems.PetFoster.PetFosterUI>("UI/PetFosterUI");
            if (petFosterUI != null)
            {
                petFosterUI.ToggleUI();
            }
        }

        /// <summary>
        /// Toggle pet skill UI
        /// </summary>
        private void TogglePetSkillUI()
        {
            var petSkillUI = GetNodeOrNull<PetSkillUI>("UI/PetSkillUI");
            if (petSkillUI != null)
            {
                petSkillUI.ToggleUI();
            }
        }

        /// <summary>
        /// Toggle pet expedition UI
        /// </summary>
        private void TogglePetExpeditionUI()
        {
            var petExpeditionUI = GetNodeOrNull<PetExpeditionUI>("UI/PetExpeditionUI");
            if (petExpeditionUI != null)
            {
                petExpeditionUI.Visible = !petExpeditionUI.Visible;
                if (petExpeditionUI.Visible)
                {
                    petExpeditionUI.Refresh();
                }
            }
        }

        /// <summary>
        /// Toggle pet story UI
        /// </summary>
        private void TogglePetStoryUI()
        {
            var petStoryUI = GetNodeOrNull<PetStoryUI>("UI/PetStoryUI");
            if (petStoryUI != null)
            {
                // Get player and their first pet (or selected pet)
                var player = GetPlayer();
                if (player != null && player.Pets != null && player.Pets.Count > 0)
                {
                    // Show stories for the first pet for now
                    var pet = player.Pets[0];
                    if (pet != null)
                    {
                        // Get pet type from pet data
                        int petTypeId = 1; // Default to wolf
                        if (pet.Has("PetTypeId"))
                        {
                            petTypeId = (int)pet.Get("PetTypeId");
                        }
                        else if (pet.Has("pet_type"))
                        {
                            petTypeId = (int)pet.Get("pet_type");
                        }
                        
                        string petName = "宠物";
                        if (pet.Has("name"))
                        {
                            petName = (string)pet.Get("name");
                        }
                        
                        string[] petTypes = { "", "狼", "熊", "鹰", "狐狸", "龙", "马" };
                        string petTypeStr = petTypeId < petTypes.Length ? petTypes[petTypeId] : "未知";
                        
                        PetStoryUI.Toggle(0, petTypeId, petName, petTypeStr);
                    }
                }
            }
        }

        /// <summary>
        /// Toggle mount expedition UI
        /// </summary>
        private void ToggleMountExpeditionUI()
        {
            var mountExpeditionUI = GetNodeOrNull<MountExpeditionUI>("UI/MountExpeditionUI");
            if (mountExpeditionUI != null)
            {
                mountExpeditionUI.Toggle();
            }
        }

        /// <summary>
        /// Toggle mystery treasure UI
        /// </summary>
        private void ToggleMysteryTreasureUI()
        {
            var mysteryTreasureUI = GetNodeOrNull<MysteryTreasureUI>("UI/MysteryTreasureUI");
            if (mysteryTreasureUI != null)
            {
                mysteryTreasureUI.Toggle();
            }
        }

        /// <summary>
        /// Toggle dynamic difficulty UI
        /// </summary>
        private void ToggleDynamicDifficultyUI()
        {
            var dynamicDifficultyUI = GetNodeOrNull<DynamicDifficultyUI>("UI/DynamicDifficultyUI");
            if (dynamicDifficultyUI != null)
            {
                dynamicDifficultyUI.ToggleUI();
            }
        }

        /// <summary>
        /// Toggle boss mechanics UI
        /// </summary>
        private void ToggleBossMechanicsUI()
        {
            var bossMechanicsUI = GetNodeOrNull<Systems.BossMechanics.BossMechanicsUI>("UI/BossMechanicsUI");
            if (bossMechanicsUI != null)
            {
                bossMechanicsUI.Toggle();
            }
        }

        /// <summary>
        /// Toggle elemental trial UI
        /// </summary>
        private void ToggleElementalTrialUI()
        {
            var elementalTrialUI = GetNodeOrNull<Systems.ElementalTrialUI>("UI/ElementalTrialUI");
            if (elementalTrialUI != null)
            {
                elementalTrialUI.Toggle();
            }
        }

        /// <summary>
        /// Toggle hit stop effect UI
        /// </summary>
        private void ToggleHitStopUI()
        {
            var hitStopUI = GetNodeOrNull<HitStopUI>("UI/HitStopUI");
            if (hitStopUI != null)
            {
                hitStopUI.Visible = !hitStopUI.Visible;
            }
        }

        /// <summary>
        /// Toggle pet battle arena UI
        /// </summary>
        private void TogglePetBattleArenaUI()
        {
            var petBattleArenaUI = GetNodeOrNull<Systems.PetBattleArena.PetBattleArenaUI>("UI/PetBattleArenaUI");
            if (petBattleArenaUI != null)
            {
                petBattleArenaUI.Toggle();
            }
        }

        /// <summary>
        /// Toggle pet morph UI
        /// </summary>
        private void TogglePetMorphUI()
        {
            var petMorphUI = GetNodeOrNull<Systems.PetMorph.PetMorphUI>("UI/PetMorphUI");
            if (petMorphUI != null)
            {
                if (petMorphUI.Visible)
                {
                    petMorphUI.HideUI();
                }
                else
                {
                    petMorphUI.ShowUI();
                }
            }
        }

        /// <summary>
        /// Toggle pet habitat UI
        /// </summary>
        private void TogglePetHabitatUI()
        {
            var petHabitatUI = GetNodeOrNull<PetHabitatUI>("UI/PetHabitatUI");
            if (petHabitatUI != null)
            {
                if (petHabitatUI.Visible)
                {
                    petHabitatUI.Hide();
                }
                else
                {
                    petHabitatUI.Show();
                }
            }
        }

        /// <summary>
        /// Toggle pet egg UI
        /// </summary>
        private void TogglePetEggUI()
        {
            var petEggUI = GetNodeOrNull<PetEggUI>("UI/PetEggUI");
            if (petEggUI != null)
            {
                if (petEggUI.Visible)
                {
                    petEggUI.Hide();
                }
                else
                {
                    petEggUI.Show();
                }
            }
        }

        /// <summary>
        /// Toggle pet friendship UI
        /// </summary>
        private void TogglePetFriendshipUI()
        {
            var petFriendshipUI = GetNodeOrNull<PetFriendshipUI>("UI/PetFriendshipUI");
            if (petFriendshipUI != null)
            {
                if (petFriendshipUI.Visible)
                {
                    petFriendshipUI.Hide();
                }
                else
                {
                    petFriendshipUI.Show();
                }
            }
        }

        /// <summary>
        /// Toggle daily dungeon UI
        /// </summary>
        private void ToggleDailyDungeonUI()
        {
            var dailyDungeonUI = GetNodeOrNull<Systems.DailyDungeon.DailyDungeonUI>("UI/DailyDungeonUI");
            if (dailyDungeonUI != null)
            {
                dailyDungeonUI.Toggle();
            }
        }

        /// <summary>
        /// Toggle survival challenge UI
        /// </summary>
        private void ToggleSurvivalChallengeUI()
        {
            var survivalChallengeUI = GetNodeOrNull<UI.SurvivalChallengeUI>("UI/SurvivalChallengeUI");
            if (survivalChallengeUI != null)
            {
                survivalChallengeUI.Toggle();
            }
        }

        private void ToggleBountyUI()
        {
            var bountyUI = GetNodeOrNull<UI.BountyUI>("CanvasLayer/BountyUI");
            if (bountyUI != null)
            {
                bountyUI.Toggle();
            }
        }

        private void ToggleEquipmentVisualsUI()
        {
            var equipVisualsUI = GetNodeOrNull<UI.EquipmentVisualsUI>("CanvasLayer/EquipmentVisualsUI");
            if (equipVisualsUI != null)
            {
                equipVisualsUI.Toggle();
            }
        }

        private void ToggleCombatVFXUI()
        {
            var combatVFXUI = GetNodeOrNull<UI.CombatVFXUI>("CanvasLayer/CombatVFXUI");
            if (combatVFXUI != null)
            {
                combatVFXUI.Toggle();
            }
        }

        private void ToggleStoryUI()
        {
            var storyUI = GetNodeOrNull<UI.StoryUI>("CanvasLayer/StoryUI");
            if (storyUI != null)
            {
                storyUI.Visible = !storyUI.Visible;
                if (storyUI.Visible)
                {
                    storyUI.RefreshChapterList();
                }
            }
        }

        private void ToggleSealedTowerUI()
        {
            var sealedTowerUI = GetNodeOrNull<UI.SealedTowerUI>("CanvasLayer/SealedTowerUI");
            if (sealedTowerUI != null)
            {
                sealedTowerUI.Toggle();
            }
        }

        private void ToggleTreasureHuntUI()
        {
            var treasureHuntUI = GetNodeOrNull<UI.TreasureHuntUI>("CanvasLayer/TreasureHuntUI");
            if (treasureHuntUI != null)
            {
                treasureHuntUI.Visible = !treasureHuntUI.Visible;
                if (treasureHuntUI.Visible)
                {
                    treasureHuntUI.LoadRegions();
                }
            }
            else
            {
                // Create UI if it doesn't exist
                var ui = GD.Load<PackedScene>("res://UI/TreasureHuntUI.tscn");
                if (ui != null)
                {
                    var newUI = ui.Instance();
                    newUI.Name = "TreasureHuntUI";
                    var canvasLayer = GetNodeOrNull<CanvasLayer>("CanvasLayer");
                    if (canvasLayer == null)
                    {
                        canvasLayer = new CanvasLayer();
                        canvasLayer.Name = "CanvasLayer";
                        AddChild(canvasLayer);
                    }
                    canvasLayer.AddChild(newUI);
                    newUI.Visible = true;
                }
            }
        }

        private void ToggleCraftingMasteryUI()
        {
            var craftingMasteryUI = GetNodeOrNull<CraftingMasteryUI>("CanvasLayer/CraftingMasteryUI");
            if (craftingMasteryUI != null)
            {
                craftingMasteryUI.Visible = !craftingMasteryUI.Visible;
                if (craftingMasteryUI.Visible)
                {
                    craftingMasteryUI.Refresh();
                }
            }
        }

        private void ToggleDiceMasterUI()
        {
            var diceMasterUI = GetNodeOrNull<DiceMasterUI>("CanvasLayer/DiceMasterUI");
            if (diceMasterUI != null)
            {
                diceMasterUI.Toggle();
            }
            else
            {
                // Create UI if it doesn't exist
                var diceMasterUI_new = new DiceMasterUI();
                diceMasterUI_new.Name = "DiceMasterUI";
                var canvasLayer = GetNodeOrNull<CanvasLayer>("CanvasLayer");
                if (canvasLayer == null)
                {
                    canvasLayer = new CanvasLayer();
                    canvasLayer.Name = "CanvasLayer";
                    AddChild(canvasLayer);
                }
                canvasLayer.AddChild(diceMasterUI_new);
                diceMasterUI_new.Show();
            }
        }

        private void ToggleRankedUI()
        {
            var rankedUI = GetNodeOrNull<RankedUI>("CanvasLayer/RankedUI");
            if (rankedUI != null)
            {
                rankedUI.Toggle();
            }
            else
            {
                var ui = GetNodeOrNull<RankedUI>("UI/RankedUI");
                if (ui != null)
                {
                    ui.Toggle();
                }
            }
        }

        private void ToggleEquipmentSetUI()
        {
            var setUI = GetNodeOrNull<UI.EquipmentSetUI>("CanvasLayer/EquipmentSetUI");
            if (setUI != null)
            {
                setUI.Toggle();
            }
        }

        private void TogglePlayerTalentUI()
        {
            var talentUI = GetNodeOrNull<Systems.PlayerTalent.PlayerTalentUI>("CanvasLayer/PlayerTalentUI");
            if (talentUI != null)
            {
                talentUI.Toggle();
            }
        }

        private void ToggleMountRaceUI()
        {
            var raceUI = GetNodeOrNull<MountRaceUI>("CanvasLayer/MountRaceUI");
            if (raceUI != null)
            {
                raceUI.Toggle();
            }
        }

        private void ToggleMountBattleArenaUI()
        {
            var arenaUI = GetNodeOrNull<Systems.MountBattleArenaUI>("CanvasLayer/MountBattleArenaUI");
            if (arenaUI != null)
            {
                arenaUI.Toggle();
            }
        }

        private void ToggleMountWeatherBonusUI()
        {
            var weatherUI = GetNodeOrNull<UI.MountWeatherBonusUI>("CanvasLayer/MountWeatherBonusUI");
            if (weatherUI != null)
            {
                if (weatherUI.Visible)
                {
                    weatherUI.Hide();
                }
                else
                {
                    weatherUI.ShowUI();
                }
            }
        }

        private void TogglePlayerProfileUI()
        {
            var profileUI = GetNodeOrNull<UI.PlayerProfileUI>("UI/PlayerProfileUI");
            if (profileUI != null)
            {
                profileUI.Toggle();
            }
        }

        private void ToggleHotkeyHUD()
        {
            var hotkeyHUD = GetNodeOrNull<HotkeyHUD>("UI/HotkeyHUD");
            if (hotkeyHUD != null)
            {
                hotkeyHUD.Toggle();
            }
        }

        private void ToggleArtifactUI()
        {
            var artifactUI = GetNodeOrNull<ArtifactUI>("UI/ArtifactUI");
            if (artifactUI != null)
            {
                artifactUI.ToggleVisible();
            }
        }

        private void ToggleWeatherUI()
        {
            var weatherUI = GetNodeOrNull<WeatherUI>("UI/WeatherUI");
            if (weatherUI != null)
            {
                weatherUI.Visible = !weatherUI.Visible;
            }
        }

        private void ToggleKeybindingUI()
        {
            var keybindingUI = GetNodeOrNull<UI.KeybindingUI>("CanvasLayer/KeybindingUI");
            if (keybindingUI != null)
            {
                keybindingUI.ToggleKeybindingUI();
            }
        }

        private void OpenSettingsUI()
        {
            // Check if any modal UI is open
            var existingSettings = GetNodeOrNull<UI.SettingsUI>("CanvasLayer/SettingsUI");
            if (existingSettings != null)
            {
                existingSettings.QueueFree();
                return;
            }

            var settingsUI = new UI.SettingsUI();
            settingsUI.Name = "SettingsUI";
            GetNode("CanvasLayer").AddChild(settingsUI);
        }

        private void ToggleReputationUI()
        {
            var reputationUI = GetNodeOrNull<UI.ReputationUI>("CanvasLayer/ReputationUI");
            if (reputationUI != null)
            {
                reputationUI.Toggle();
            }
        }

        private void ToggleBadgeUI()
        {
            var badgeUI = GetNodeOrNull<UI.AchievementBadgeUI>("CanvasLayer/AchievementBadgeUI");
            if (badgeUI != null)
            {
                badgeUI.QueueFree();
            }
            else
            {
                var newBadgeUI = new UI.AchievementBadgeUI();
                newBadgeUI.Name = "AchievementBadgeUI";
                var canvasLayer = GetNodeOrNull("CanvasLayer");
                if (canvasLayer != null)
                {
                    canvasLayer.AddChild(newBadgeUI);
                }
            }
        }

        private void ToggleSecretAchievementUI()
        {
            var secretUI = GetNodeOrNull<Systems.SecretAchievementUI>("CanvasLayer/SecretAchievementUI");
            if (secretUI != null)
            {
                secretUI.QueueFree();
            }
            else
            {
                var newSecretUI = new Systems.SecretAchievementUI();
                newSecretUI.Name = "SecretAchievementUI";
                var canvasLayer = GetNodeOrNull("CanvasLayer");
                if (canvasLayer != null)
                {
                    canvasLayer.AddChild(newSecretUI);
                }
            }
        }

        /// <summary>
        /// 切换炼金实验室界面
        /// </summary>
        private void ToggleAlchemyLaboratoryUI()
        {
            var labUI = GetNodeOrNull<AlchemyLaboratoryUI>("CanvasLayer/AlchemyLaboratoryUI");
            if (labUI != null)
            {
                labUI.QueueFree();
            }
            else
            {
                var newLabUI = new AlchemyLaboratoryUI();
                newLabUI.Name = "AlchemyLaboratoryUI";
                var canvasLayer = GetNodeOrNull("CanvasLayer");
                if (canvasLayer != null)
                {
                    canvasLayer.AddChild(newLabUI);
                }
            }
        }

        /// <summary>
        /// Toggle daily ritual UI
        /// </summary>
        private void ToggleDailyRitualUI()
        {
            var ritualUI = GetNodeOrNull<DailyRitualUI>("CanvasLayer/DailyRitualUI");
            if (ritualUI != null)
            {
                ritualUI.QueueFree();
            }
            else
            {
                var newRitualUI = new DailyRitualUI();
                newRitualUI.Name = "DailyRitualUI";
                var canvasLayer = GetNodeOrNull("CanvasLayer");
                if (canvasLayer != null)
                {
                    canvasLayer.AddChild(newRitualUI);
                }
            }
        }

        /// <summary>
        /// Toggle weekly challenge UI
        /// </summary>
        private void ToggleWeeklyChallengeUI()
        {
            var challengeUI = GetNodeOrNull<WeeklyChallengeUI>("CanvasLayer/WeeklyChallengeUI");
            if (challengeUI != null)
            {
                challengeUI.QueueFree();
            }
            else
            {
                var newChallengeUI = new WeeklyChallengeUI();
                newChallengeUI.Name = "WeeklyChallengeUI";
                var canvasLayer = GetNodeOrNull("CanvasLayer");
                if (canvasLayer != null)
                {
                    canvasLayer.AddChild(newChallengeUI);
                }
            }
        }

        /// <summary>
        /// 切换邮件界面
        /// </summary>
        private void ToggleMailUI()
        {
            var mailUI = GetNodeOrNull<UI.MailUI>("CanvasLayer/MailUI");
            if (mailUI != null)
            {
                mailUI.QueueFree();
            }
            else
            {
                var newMailUI = new UI.MailUI();
                newMailUI.Name = "MailUI";
                var canvasLayer = GetNodeOrNull("CanvasLayer");
                if (canvasLayer != null)
                {
                    canvasLayer.AddChild(newMailUI);
                    // 传入玩家ID（单人模式用默认ID）
                    newMailUI.Open(GetMultiplayer().GetUniqueId().ToString());
                }
            }
        }

        /// <summary>
        /// 切换商店界面
        /// </summary>
        private void ToggleShopUI()
        {
            var shopUI = GetNodeOrNull<UI.ShopUI>("CanvasLayer/ShopUI");
            if (shopUI != null && shopUI.Visible)
            {
                shopUI.Hide();
            }
            else
            {
                // ShopUI 是直接添加到 ui 容器的
                var ui = GetNodeOrNull<Control>("UI");
                if (ui != null)
                {
                    var existingShopUI = ui.GetNodeOrNull<UI.ShopUI>("ShopUI");
                    if (existingShopUI != null)
                    {
                        existingShopUI.Toggle();
                    }
                }
            }
        }

        /// <summary>
        /// 切换拍卖行界面
        /// </summary>
        private void ToggleAuctionHouseUI()
        {
            var auctionUI = GetNodeOrNull<UI.AuctionHouseUI>("CanvasLayer/AuctionHouseUI");
            if (auctionUI != null && auctionUI.Visible)
            {
                auctionUI.Hide();
            }
            else
            {
                // AuctionHouseUI 是添加到 canvas layer 的
                var canvasLayer = GetNodeOrNull("CanvasLayer");
                if (canvasLayer != null)
                {
                    if (auctionUI == null)
                    {
                        auctionUI = new UI.AuctionHouseUI();
                        auctionUI.Name = "AuctionHouseUI";
                        canvasLayer.AddChild(auctionUI);
                    }
                    auctionUI.Show();
                    auctionUI.Open();
                }
            }
        }

        /// <summary>
        /// 切换公会界面
        /// </summary>
        private void ToggleGuildUI()
        {
            var ui = GetNodeOrNull<Control>("UI");
            if (ui == null) return;

            var guildUI = ui.GetNodeOrNull<GuildUI>("GuildUI");
            if (guildUI != null && guildUI.Visible)
            {
                guildUI.Hide();
            }
            else
            {
                if (guildUI == null)
                {
                    guildUI = new GuildUI();
                    guildUI.Name = "GuildUI";
                    ui.AddChild(guildUI);
                }
                guildUI.Show();
                guildUI.Toggle();
            }
        }

        /// <summary>
        /// 切换公会任务界面 (Shift+G)
        /// </summary>
        private void ToggleGuildQuestUI()
        {
            var ui = GetNodeOrNull<Control>("UI");
            if (ui == null) return;

            var guildQuestUI = ui.GetNodeOrNull<GuildQuestUI>("GuildQuestUI");
            if (guildQuestUI != null)
            {
                guildQuestUI.ToggleUI();
            }
        }

        /// <summary>
        /// 切换公会银行界面 (Shift+B)
        /// </summary>
        private void ToggleGuildBankUI()
        {
            var ui = GetNodeOrNull<Control>("UI");
            if (ui == null) return;

            var guildBankUI = ui.GetNodeOrNull<GuildBankUI>("GuildBankUI");
            if (guildBankUI != null)
            {
                GuildBankUI.Toggle();
            }
        }

        /// <summary>
        /// 切换公会科技界面 (Ctrl+Shift+T)
        /// </summary>
        private void ToggleGuildTechnologyUI()
        {
            var ui = GetNodeOrNull<Control>("UI");
            if (ui == null) return;

            var guildTechnologyUI = ui.GetNodeOrNull<GuildTechnologyUI>("GuildTechnologyUI");
            if (guildTechnologyUI != null && guildTechnologyUI.Visible)
            {
                guildTechnologyUI.Hide();
            }
            else
            {
                if (guildTechnologyUI == null)
                {
                    guildTechnologyUI = new GuildTechnologyUI();
                    guildTechnologyUI.Name = "GuildTechnologyUI";
                    ui.AddChild(guildTechnologyUI);
                }
                guildTechnologyUI.Show();
                guildTechnologyUI.Refresh();
            }
        }

        /// <summary>
        /// 切换交易界面
        /// </summary>
        private void ToggleTradeUI()
        {
            TradeUI.ToggleTrade();
        }

        /// <summary>
        /// 切换每日登录奖励界面
        /// </summary>
        private void ToggleDailyLoginRewardUI()
        {
            var ui = GetNodeOrNull<Control>("UI");
            if (ui == null) return;

            var dailyLoginRewardUI = ui.GetNodeOrNull<DailyLoginRewardUI>("DailyLoginRewardUI");
            if (dailyLoginRewardUI != null && dailyLoginRewardUI.Visible)
            {
                dailyLoginRewardUI.Hide();
            }
            else
            {
                if (dailyLoginRewardUI == null)
                {
                    dailyLoginRewardUI = new DailyLoginRewardUI();
                    dailyLoginRewardUI.Name = "DailyLoginRewardUI";
                    ui.AddChild(dailyLoginRewardUI);
                }
                dailyLoginRewardUI.Show();
            }
        }

        /// <summary>
        /// 切换随机祝福界面
        /// </summary>
        private void ToggleRandomBoonUI()
        {
            var ui = GetNodeOrNull<Control>("UI");
            if (ui == null) return;

            var randomBoonUI = ui.GetNodeOrNull<UI.RandomBoonUI>("RandomBoonUI");
            if (randomBoonUI != null && randomBoonUI.Visible)
            {
                randomBoonUI.Hide();
            }
            else
            {
                if (randomBoonUI == null)
                {
                    randomBoonUI = new UI.RandomBoonUI();
                    randomBoonUI.Name = "RandomBoonUI";
                    ui.AddChild(randomBoonUI);
                }
                randomBoonUI.Show();
            }
        }

        /// <summary>
        /// 切换每日任务界面
        /// </summary>
        private void ToggleDailyQuestUI()
        {
            var ui = GetNodeOrNull<Control>("UI");
            if (ui == null) return;

            var dailyQuestUI = ui.GetNodeOrNull<Systems.DailyQuest.DailyQuestUI>("DailyQuestUI");
            if (dailyQuestUI != null && dailyQuestUI.Visible)
            {
                dailyQuestUI.Hide();
            }
            else
            {
                if (dailyQuestUI == null)
                {
                    dailyQuestUI = new Systems.DailyQuest.DailyQuestUI();
                    dailyQuestUI.Name = "DailyQuestUI";
                    ui.AddChild(dailyQuestUI);
                }
                dailyQuestUI.Show();
            }
        }

        /// <summary>
        /// 切换程序化挑战界面
        /// </summary>
        private void ToggleProceduralChallengeUI()
        {
            var ui = GetNodeOrNull<Control>("UI");
            if (ui == null) return;

            var challengeUI = ui.GetNodeOrNull<Systems.ProceduralChallengeUI>("ProceduralChallengeUI");
            if (challengeUI != null && challengeUI.Visible)
            {
                challengeUI.Hide();
            }
            else
            {
                if (challengeUI == null)
                {
                    challengeUI = new Systems.ProceduralChallengeUI();
                    challengeUI.Name = "ProceduralChallengeUI";
                    ui.AddChild(challengeUI);
                }
                challengeUI.Toggle();
            }
        }

        /// <summary>
        /// 切换战利品掉落统计界面
        /// </summary>
        private void ToggleLootDropUI()
        {
            var ui = GetNodeOrNull<Control>("UI");
            if (ui == null) return;

            var lootUI = ui.GetNodeOrNull<Systems.LootDropUI>("LootDropUI");
            if (lootUI != null && lootUI.Visible)
            {
                lootUI.Hide();
            }
            else
            {
                if (lootUI == null)
                {
                    lootUI = new Systems.LootDropUI();
                    lootUI.Name = "LootDropUI";
                    ui.AddChild(lootUI);
                }
                lootUI.Toggle();
            }
        }

        /// <summary>
        /// 切换装备耐久度界面
        /// </summary>
        private void ToggleEquipmentDurabilityUI()
        {
            var ui = GetNodeOrNull<Control>("UI");
            if (ui == null) return;

            var durabilityUI = ui.GetNodeOrNull<Systems.EquipmentDurability.EquipmentDurabilityUI>("EquipmentDurabilityUI");
            if (durabilityUI != null && durabilityUI.Visible)
            {
                durabilityUI.ToggleUI();
            }
            else
            {
                if (durabilityUI == null)
                {
                    durabilityUI = new Systems.EquipmentDurability.EquipmentDurabilityUI();
                    durabilityUI.Name = "EquipmentDurabilityUI";
                    ui.AddChild(durabilityUI);
                }
                durabilityUI.ToggleUI();
            }
        }

        /// <summary>
        /// 切换装备回收界面
        /// </summary>
        private void ToggleEquipmentRecycleUI()
        {
            var ui = GetNodeOrNull<Control>("UI");
            if (ui == null) return;

            var recycleUI = ui.GetNodeOrNull<Systems.EquipmentRecycle.EquipmentRecycleUI>("EquipmentRecycleUI");
            if (recycleUI != null && recycleUI.Visible)
            {
                recycleUI.ToggleUI();
            }
            else
            {
                if (recycleUI == null)
                {
                    recycleUI = new Systems.EquipmentRecycle.EquipmentRecycleUI();
                    recycleUI.Name = "EquipmentRecycleUI";
                    ui.AddChild(recycleUI);
                }
                recycleUI.ToggleUI();
            }
        }

        /// <summary>
        /// 切换状态效果界面
        /// </summary>
        private void ToggleBuffUI()
        {
            var buffUI = GetNodeOrNull<Systems.BuffSystem.BuffUI>("UI/BuffUI");
            if (buffUI != null)
            {
                buffUI.ToggleBuffUI();
            }
        }

        /// <summary>
        /// 切换季节性活动界面
        /// </summary>
        private void ToggleSeasonalEventUI()
        {
            var ui = GetNodeOrNull<Control>("UI");
            if (ui == null) return;

            var eventUI = ui.GetNodeOrNull<SeasonalEventUI>("SeasonalEventUI");
            if (eventUI != null && eventUI.Visible)
            {
                eventUI.Hide();
            }
            else
            {
                if (eventUI == null)
                {
                    eventUI = new SeasonalEventUI();
                    eventUI.Name = "SeasonalEventUI";
                    ui.AddChild(eventUI);
                }
                eventUI.Show();
            }
        }

        /// <summary>
        /// 切换表情动作界面
        /// </summary>
        private void ToggleEmoteUI()
        {
            var ui = GetNodeOrNull<Control>("UI");
            if (ui == null) return;

            var emoteUI = ui.GetNodeOrNull<Systems.Emote.EmoteUI>("EmoteUI");
            if (emoteUI != null)
            {
                emoteUI.Toggle();
            }
            else
            {
                emoteUI = new Systems.Emote.EmoteUI();
                emoteUI.Name = "EmoteUI";
                ui.AddChild(emoteUI);
                emoteUI.Toggle();
            }
        }

        /// <summary>
        /// 切换经济监控面板
        /// </summary>
        private void ToggleEconomicDashboardUI()
        {
            var ui = GetNodeOrNull<Control>("UI");
            if (ui == null) return;

            var economicDashboardUI = ui.GetNodeOrNull<EconomicDashboardUI>("EconomicDashboardUI");
            if (economicDashboardUI != null)
            {
                economicDashboardUI.ToggleVisibility();
            }
            else
            {
                economicDashboardUI = new EconomicDashboardUI();
                economicDashboardUI.Name = "EconomicDashboardUI";
                ui.AddChild(economicDashboardUI);
                economicDashboardUI.ToggleVisibility(true);
            }
        }

        /// <summary>
        /// 切换附魔界面
        /// </summary>
        private void ToggleEnchantmentUI()
        {
            var ui = GetNodeOrNull<Control>("UI");
            if (ui == null) return;

            var enchantUI = ui.GetNodeOrNull<Systems.EnchantmentUI>("EnchantmentUI");
            if (enchantUI != null && enchantUI.Visible)
            {
                enchantUI.Hide();
            }
            else
            {
                if (enchantUI == null)
                {
                    enchantUI = new Systems.EnchantmentUI();
                    enchantUI.Name = "EnchantmentUI";
                    ui.AddChild(enchantUI);
                }
                enchantUI.Show();
            }
        }

        /// <summary>
        /// 切换世界首领界面
        /// </summary>
        private void ToggleWorldBossUI()
        {
            var ui = GetNodeOrNull<Control>("UI");
            if (ui == null) return;

            var worldBossUI = ui.GetNodeOrNull<Systems.WorldBoss.WorldBossUI>("WorldBossUI");
            if (worldBossUI != null && worldBossUI.Visible)
            {
                worldBossUI.Hide();
            }
            else
            {
                if (worldBossUI == null)
                {
                    worldBossUI = new Systems.WorldBoss.WorldBossUI();
                    worldBossUI.Name = "WorldBossUI";
                    ui.AddChild(worldBossUI);
                }
                worldBossUI.Show();
            }
        }

        /// <summary>
        /// 处理队伍技能快捷键输入
        /// </summary>
        private void HandleTeamSkillInput()
        {
            if (TeamSkillSystem.Instance == null) return;

            // 数字键 1-9 使用对应技能
            if (Input.IsActionJustPressed("team_skill_1"))
                TeamSkillSystem.Instance.UseSkill(TeamSkillSystem.TeamSkillType.HealingRain);
            else if (Input.IsActionJustPressed("team_skill_2"))
                TeamSkillSystem.Instance.UseSkill(TeamSkillSystem.TeamSkillType.ShieldWall);
            else if (Input.IsActionJustPressed("team_skill_3"))
                TeamSkillSystem.Instance.UseSkill(TeamSkillSystem.TeamSkillType.DamageAura);
            else if (Input.IsActionJustPressed("team_skill_4"))
                TeamSkillSystem.Instance.UseSkill(TeamSkillSystem.TeamSkillType.DefenseAura);
            else if (Input.IsActionJustPressed("team_skill_5"))
                TeamSkillSystem.Instance.UseSkill(TeamSkillSystem.TeamSkillType.SpeedAura);
            else if (Input.IsActionJustPressed("team_skill_6"))
                TeamSkillSystem.Instance.UseSkill(TeamSkillSystem.TeamSkillType.ManaRegen);
            else if (Input.IsActionJustPressed("team_skill_7"))
                TeamSkillSystem.Instance.UseSkill(TeamSkillSystem.TeamSkillType.CritAura);
            else if (Input.IsActionJustPressed("team_skill_8"))
                TeamSkillSystem.Instance.UseSkill(TeamSkillSystem.TeamSkillType.LifeSteal);
            else if (Input.IsActionJustPressed("team_skill_9"))
                TeamSkillSystem.Instance.UseSkill(TeamSkillSystem.TeamSkillType.Invincibility);
            else if (Input.IsActionJustPressed("team_skill_0"))
                TeamSkillSystem.Instance.UseSkill(TeamSkillSystem.TeamSkillType.Resurrection);
        }

        private void ToggleDialogueUI(string npcId)
        {
            if (Quests.DialogueManager.Instance.IsInDialogue)
            {
                Quests.DialogueManager.Instance.EndDialogue();
            }
            else
            {
                Quests.DialogueManager.Instance.StartDialogue(npcId);
            }
        }

        private void TrySpinAttack()
        {
            if (WeaponMasterySystem.Instance != null)
            {
                bool success = WeaponMasterySystem.Instance.TrySpinAttack();
                if (success)
                {
                    GD.Print("Spin attack executed!");
                }
            }
        }

        private void TryChargeAttack()
        {
            if (WeaponMasterySystem.Instance != null && _player != null)
            {
                Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
                if (inputDir.Length() > 0.1f)
                {
                    bool success = WeaponMasterySystem.Instance.TryChargeAttack(inputDir);
                    if (success)
                    {
                        GD.Print("Charge attack executed!");
                    }
                }
            }
        }

        private void UpdatePlayerUI()
        {
            if (_player == null) return;

            var healthBar = GetNodeOrNull<ProgressBar>("UI/HealthBar");
            var manaBar = GetNodeOrNull<ProgressBar>("UI/ManaBar");
            var levelLabel = GetNodeOrNull<Label>("UI/LevelLabel");
            var expBar = GetNodeOrNull<ProgressBar>("UI/ExpBar");

            if (healthBar != null)
            {
                healthBar.MaxValue = _player.MaxHealth;
                healthBar.Value = _player.CurrentHealth;
            }

            if (manaBar != null)
            {
                manaBar.MaxValue = _player.MaxMana;
                manaBar.Value = _player.CurrentMana;
            }

            if (levelLabel != null)
            {
                levelLabel.Text = "Lv." + _player.Level;
            }

            if (expBar != null)
            {
                expBar.MaxValue = _player.Level * 100;
                expBar.Value = _player.Experience;
            }
        }

        private void TogglePause()
        {
            IsPaused = !IsPaused;
            GetTree().Paused = IsPaused;

            if (IsPaused)
            {
                ShowPauseMenu();
            }
            else
            {
                HidePauseMenu();
            }

            GD.Print("Game " + (IsPaused ? "PAUSED" : "RESUMED"));
        }

        private void ShowPauseMenu()
        {
            // Create pause menu
            var pauseMenu = new Control();
            pauseMenu.Name = "PauseMenu";
            pauseMenu.SetAnchorsPreset(Control.LayoutPreset.FullRect);

            var panel = new Panel();
            panel.SetAnchorsPreset(Control.LayoutPreset.Center);
            panel.Size = new Vector2(300, 200);
            pauseMenu.AddChild(panel);

            AddChild(pauseMenu);
        }

        private void HidePauseMenu()
        {
            var pauseMenu = GetNodeOrNull("PauseMenu");
            if (pauseMenu != null)
            {
                pauseMenu.QueueFree();
            }
        }

        public void SpawnEnemy(Vector2 position, string enemyType = "goblin")
        {
            if (EnemyScene == null) return;

            var enemy = EnemyScene.Instantiate<Enemy>();
            enemy.GlobalPosition = position;
            _enemies.AddChild(enemy);

            // Start battle music when enemy spawns (if not already in battle)
            if (BackgroundMusicSystem.Instance != null && !BackgroundMusicSystem.Instance.IsInBattle()) {
                BackgroundMusicSystem.Instance.StartBattleMusic(false);
            }

            // Start combat stats
            var combatStatsPanel = GetNodeOrNull<UI.CombatStatsPanel>("CanvasLayer/CombatStatsPanel");
            if (combatStatsPanel != null) {
                combatStatsPanel.StartCombat();
            }

            // Trigger first combat tutorial
            var tutorialUI = GetNodeOrNull<UI.TutorialUI>("CanvasLayer/TutorialUI");
            if (tutorialUI != null && tutorialUI.IsActive == false) {
                tutorialUI.TriggerTutorial(TutorialTrigger.FirstCombat);
            }

            GD.Print("Enemy spawned: " + enemyType);
        }

        public void AdvanceDay()
        {
            CurrentDay++;
            GD.Print("Day " + CurrentDay + " begins!");
        }

        /// <summary>
        /// Check if battle has ended (no enemies remaining)
        /// </summary>
        public void CheckBattleEnd() {
            if (_enemies.GetChildCount() == 0 && BackgroundMusicSystem.Instance != null) {
                BackgroundMusicSystem.Instance.StopBattleMusic();
                BackgroundMusicSystem.Instance.PlayVictoryMusic();
            }

            // End combat stats if no enemies
            if (_enemies.GetChildCount() == 0) {
                var combatStatsPanel = GetNodeOrNull<UI.CombatStatsPanel>("CanvasLayer/CombatStatsPanel");
                if (combatStatsPanel != null) {
                    combatStatsPanel.EndCombat();
                }
            }
        }

        /// <summary>
        /// 显示通知消息
        /// </summary>
        public void ShowNotification(string message, string detail = "")
        {
            // 尝试找到通知UI系统
            var notificationUI = GetNodeOrNull("CanvasLayer/NotificationUI");
            if (notificationUI != null)
            {
                // 调用通知UI的方法
                GD.Print($"通知: {message} - {detail}");
            }
            else
            {
                GD.Print($"通知: {message} - {detail}");
            }
        }

        /// <summary>
        /// 获取世界事件管理器实例
        /// </summary>
        public WorldEventManager GetWorldEventManager()
        {
            return WorldEventManager.Instance;
        }

        /// <summary>
        /// 开始新游戏
        /// </summary>
        public void StartNewGame()
        {
            GD.Print("Starting new game...");

            // 重置玩家数据
            if (_player != null)
            {
                _player.ResetPlayer();
            }

            // 重置游戏状态
            CurrentDay = 1;
            IsPaused = false; 
            SetGameState(GameState.Playing);

            // 显示游戏UI
            ShowGameUI();

            GD.Print("New game started!");
        }

        /// <summary>
        /// 加载游戏存档
        /// </summary>
        public void LoadGame(int saveSlot)
        {
            GD.Print("Loading game from slot: " + saveSlot);

            var saveSystem = new SaveSystem();
            var saveData = saveSystem.LoadGame(saveSlot);

            if (saveData != null)
            {
                // 加载玩家数据
                if (_player != null && saveData.PlayerData != null)
                {
                    _player.LoadPlayerData(saveData.PlayerData);
                }

                // 加载统计
                var statsData = new Dictionary<string, object>
                {
                    ["TotalKills"] = saveData.TotalKills,
                    ["TotalDeaths"] = saveData.TotalDeaths,
                    ["TotalDamageDealt"] = saveData.TotalDamageDealt,
                    ["TotalDamageTaken"] = saveData.TotalDamageTaken,
                    ["TotalHealing"] = saveData.TotalHealing,
                    ["CriticalHits"] = saveData.CriticalHits,
                    ["PerfectBlocks"] = saveData.PerfectBlocks,
                    ["Dodges"] = saveData.Dodges,
                    ["GoldEarned"] = saveData.GoldEarned,
                    ["GoldSpent"] = saveData.GoldSpent,
                    ["ExperienceGained"] = saveData.ExperienceGained,
                    ["ItemsCollected"] = saveData.ItemsCollected,
                    ["ItemsCrafted"] = saveData.ItemsCrafted,
                    ["QuestsCompleted"] = saveData.QuestsCompleted,
                    ["SkillsLearned"] = saveData.SkillsLearned,
                    ["SkillsUsed"] = saveData.SkillsUsed,
                    ["RegionsDiscovered"] = saveData.RegionsDiscovered,
                    ["EnemiesEncountered"] = saveData.EnemiesEncountered,
                    ["BossesDefeated"] = saveData.BossesDefeated,
                    ["TotalPlayTime"] = saveData.TotalPlayTime,
                    ["HighestLevel"] = saveData.HighestLevel,
                    ["HighestCombo"] = saveData.HighestCombo,
                    ["AchievementsUnlocked"] = saveData.AchievementsUnlocked
                };
                StatisticsManager.Instance.LoadStatistics(statsData);

                // 加载快速槽数据
                if (saveData.QuickSlotItemIds != null && saveData.QuickSlotQuantities != null)
                {
                    for (int i = 0; i < Mathf.Min(saveData.QuickSlotItemIds.Length, 9); i++)
                    {
                        if (QuickSlotSystem.Instance != null && i < 9)
                        {
                            QuickSlotSystem.Instance.SetSlot(i, saveData.QuickSlotItemIds[i], saveData.QuickSlotQuantities[i]);
                        }
                    }
                }

                // 加载坐骑数据
                if (saveData.MountData != null && MountManager.Instance != null)
                {
                    MountManager.Instance.Deserialize(saveData.MountData);
                }

                // 加载收藏点数据
                if (saveData.BookmarkData != null && BookmarkSystem.Instance != null)
                {
                    BookmarkSystem.Instance.Deserialize(saveData.BookmarkData);
                }

                // 加载自动收藏点数据
                if (saveData.AutoBookmarkData != null)
                {
                    var autoBookmarkSystem = GetNodeOrNull<Systems.AutoBookmarkSystem>("AutoBookmarkSystem");
                    if (autoBookmarkSystem != null)
                    {
                        autoBookmarkSystem.Deserialize(saveData.AutoBookmarkData);
                    }
                }

                // 加载强化数据
                if (saveData.EnhancementData != null)
                {
                    var enhancementSystem = GetNodeOrNull<Systems.Enhancement.EnhancementSystem>("EnhancementSystem");
                    if (enhancementSystem != null)
                    {
                        enhancementSystem.Deserialize(saveData.EnhancementData);
                    }
                }

                // 加载自动药水数据
                if (saveData.AutoPotionData != null)
                {
                    var autoPotionSystem = GetNodeOrNull<Systems.AutoPotionSystem>("AutoPotionSystem");
                    if (autoPotionSystem != null)
                    {
                        autoPotionSystem.Deserialize(saveData.AutoPotionData);
                    }
                }

                // 加载附魔数据
                if (saveData.EnchantmentData != null)
                {
                    ClawRPG.Scripts.Systems.Enchantment.EnchantmentSystem.Instance.Deserialize(saveData.EnchantmentData);
                }

                // 加载赏金数据
                if (saveData.BountyData != null)
                {
                    BountyManager.Instance.Deserialize(saveData.BountyData);
                }

                // 加载天气数据
                var weatherSystem = GetNodeOrNull<WeatherSystem>("WeatherSystem");
                if (weatherSystem != null && saveData.WeatherData != null)
                {
                    weatherSystem.Deserialize(saveData.WeatherData);
                }

                // 加载装备外观数据
                var equipVisuals = GetNodeOrNull<UI.EquipmentVisuals>("EquipmentVisuals");
                if (equipVisuals != null && saveData.EquipmentVisualsData != null)
                {
                    equipVisuals.Deserialize(saveData.EquipmentVisualsData);
                }

                // 加载已解锁外观数据
                if (equipVisuals != null && saveData.UnlockedVisuals != null)
                {
                    equipVisuals.LoadUnlockedVisualsData(saveData.UnlockedVisuals);
                }

                // 加载按键绑定数据
                var keybindingSystem = GetNodeOrNull<Systems.KeybindingSystem>("KeybindingSystem");
                if (keybindingSystem != null && saveData.KeybindingData != null)
                {
                    keybindingSystem.Deserialize(saveData.KeybindingData);
                }

                // 加载宠物故事数据
                var petStorySystem = GetNodeOrNull<PetStorySystem>("PetStorySystem");
                if (petStorySystem != null && saveData.PetStoryData != null)
                {
                    petStorySystem.Deserialize(saveData.PetStoryData);
                }

                // 加载表情动作数据
                var emoteSystem = GetNodeOrNull<Systems.Emote.EmoteSystem>("EmoteSystem");
                if (emoteSystem != null && saveData.EmoteData != null)
                {
                    emoteSystem.LoadData(saveData.EmoteData);
                }

                // 加载封印之塔数据
                var sealedTowerManager = GetNodeOrNull<Systems.SealedTowerManager>("SealedTowerManager");
                if (sealedTowerManager != null && saveData.SealedTowerData != null)
                {
                    sealedTowerManager.LoadData(saveData.SealedTowerData);
                }

                // 加载声望转生数据
                var prestigeSystem = GetNodeOrNull<Systems.PrestigeSystem>("PrestigeSystem");
                if (prestigeSystem != null && saveData.PrestigeData != null)
                {
                    prestigeSystem.LoadData(saveData.PrestigeData);
                }

                CurrentDay = saveData.CurrentDay;
                SetGameState(GameState.Playing);

                ShowGameUI();

                GD.Print("Game loaded successfully!");
            }
            else
            {
                GD.PrintErr("Failed to load save file!");
            }
        }

        /// <summary>
        /// 显示游戏UI
        /// </summary>
        private void ShowGameUI()
        {
            // 显示所有游戏UI元素
            var canvasLayer = GetNodeOrNull<CanvasLayer>("CanvasLayer");
            if (canvasLayer != null)
            {
                foreach (var child in canvasLayer.GetChildren())
                {
                    if (child is Control control)
                    {
                        control.Visible = true;
                    }
                }
            }
        }

        /// <summary>
        /// 切换设置界面
        /// </summary>
        public void ToggleSettings()
        {
            var settingsUI = GetNodeOrNull<Control>("CanvasLayer/SettingsUI");
            if (settingsUI != null)
            {
                settingsUI.Visible = !settingsUI.Visible;

                if (settingsUI.Visible)
                {
                    GD.Print("Settings opened");
                }
                else
                {
                    GD.Print("Settings closed");
                }
            }
            else
            {
                GD.Print("Settings UI not found in scene");
            }
        }

        #region Counter Attack Sound Handlers

        /// <summary>
        /// Handle counter attack performed signal
        /// </summary>
        private void _OnCounterAttackPerformed(CounterAttackSystem.CounterType type, float damage)
        {
            if (SoundEffectSystem.Instance != null)
            {
                SoundEffectSystem.Instance.PlayCounterAttackPerformed(type);
            }
        }

        /// <summary>
        /// Handle counter attack window signal
        /// </summary>
        private void _OnCounterAttackWindow(bool isActive)
        {
            if (SoundEffectSystem.Instance != null)
            {
                if (isActive)
                {
                    SoundEffectSystem.Instance.PlayCounterAttackWindow();
                }
            }
        }

        /// <summary>
        /// Handle counter attack ready signal
        /// </summary>
        private void _OnCounterAttackReady()
        {
            if (SoundEffectSystem.Instance != null)
            {
                SoundEffectSystem.Instance.PlayCounterAttackReady();
            }
        }

        #endregion

        #region Weather Sound Handlers

        /// <summary>
        /// Handle weather changed signal
        /// </summary>
        private void _OnWeatherChanged(WeatherData newWeather, WeatherData oldWeather)
        {
            if (SoundEffectSystem.Instance != null && newWeather != null)
            {
                SoundEffectSystem.Instance.PlayWeatherChange(newWeather.Type);
            }
        }

        #endregion
    }
}
