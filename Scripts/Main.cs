using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;
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
            
            // Initialize counter attack system
            var counterAttackSystem = new CounterAttackSystem();
            counterAttackSystem.Name = "CounterAttackSystem";
            AddChild(counterAttackSystem);
            
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
            
            // Initialize weather system
            var weatherSystem = new WeatherSystem();
            weatherSystem.Name = "WeatherSystem";
            AddChild(weatherSystem);
            
            // Connect weather system signals to sound effects
            weatherSystem.Connect(WeatherSystem.SignalName.WeatherChanged, 
                this, nameof(_OnWeatherChanged));
            
            // Initialize camera effect system
            var cameraEffectSystem = new CameraEffectSystem();
            cameraEffectSystem.Name = "CameraEffectSystem";
            AddChild(cameraEffectSystem);
            
            // Initialize combo system
            var comboSystem = new ComboSystem();
            comboSystem.Name = "ComboSystem";
            AddChild(comboSystem);

            // Initialize dialogue system
            var dialogueManager = Quests.DialogueManager.Instance;
            dialogueManager.Name = "DialogueManager";
            AddChild(dialogueManager);
            
            // Initialize story system
            var storyManager = new StorySystem.StoryManager();
            storyManager.Name = "StoryManager";
            AddChild(storyManager);
            
            // Initialize region manager
            var regionManager = new RegionManager();
            regionManager.Name = "RegionManager";
            AddChild(regionManager);
            
            // Initialize sound effect system
            var soundEffectSystem = new SoundEffectSystem();
            soundEffectSystem.Name = "SoundEffectSystem";
            AddChild(soundEffectSystem);
            
            // Initialize background music system
            var backgroundMusicSystem = new BackgroundMusicSystem();
            backgroundMusicSystem.Name = "BackgroundMusicSystem";
            AddChild(backgroundMusicSystem);
            
            // Initialize equipment set system
            var equipmentSetManager = new EquipmentSetManager();
            equipmentSetManager.Name = "EquipmentSetManager";
            AddChild(equipmentSetManager);
            
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
            
            // Initialize keybinding system
            var keybindingSystem = new Systems.KeybindingSystem();
            
            // Initialize reputation system
            var reputationSystem = ReputationSystem.Instance;
            reputationSystem.Initialize();
            
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

            // Combat Stats Panel
            var combatStatsPanel = new UI.CombatStatsPanel();
            combatStatsPanel.Name = "CombatStatsPanel";
            combatStatsPanel.AddToGroup("CombatStatsPanel");
            ui.AddChild(combatStatsPanel);

            // Dialogue UI
            var dialogueUI = new UI.DialogueUI();
            dialogueUI.Name = "DialogueUI";
            ui.AddChild(dialogueUI);

            // Story UI
            var storyUI = new UI.StoryUI();
            storyUI.Name = "StoryUI";
            ui.AddChild(storyUI);
            
            // Equipment Set UI
            var equipmentSetUI = new UI.EquipmentSetUI();
            equipmentSetUI.Name = "EquipmentSetUI";
            ui.AddChild(equipmentSetUI);
            
            // Keybinding UI
            var keybindingUI = new UI.KeybindingUI();
            keybindingUI.Name = "KeybindingUI";
            ui.AddChild(keybindingUI);
            
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
            
            GD.Print("UI initialized");
            
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
                }
            }
        }
        
        private float _autoSaveTimer = 0f;
        private const float AutoSaveInterval = 300f; // 5 minutes
        
        public override void _Process(double delta)
        {
            float dt = (float)delta;
            
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
            
            // Handle title UI toggle (Y key)
            if (Input.IsActionJustPressed("titles"))
            {
                ToggleTitleUI();
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
            
            // Handle equipment set UI toggle (Shift+E key)
            if (Input.IsKeyPressed(KEY_SHIFT) && Input.IsKeyPressed(KEY_E))
            {
                if (!_shiftEToggleCooldown)
                {
                    ToggleEquipmentSetUI();
                    _shiftEToggleCooldown = true;
                }
            }
            else
            {
                _shiftEToggleCooldown = false;
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
            
            // Handle story UI toggle (K key)
            if (Input.IsActionJustPressed("story"))
            {
                ToggleStoryUI();
            }
            
            // Handle player profile UI toggle (F key)
            if (Input.IsActionJustPressed("player_profile"))
            {
                TogglePlayerProfileUI();
            }
            
            // Handle keybinding UI toggle (F10 key)
            if (Input.IsActionJustPressed("keybinding"))
            {
                ToggleKeybindingUI();
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
        
        private void ToggleTitleUI()
        {
            var titleUI = GetNodeOrNull<UI.TitleUI>("CanvasLayer/TitleUI");
            if (titleUI != null)
            {
                if (titleUI.Visible)
                {
                    titleUI.Hide();
                }
                else
                {
                    titleUI.Show();
                }
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
        
        private void ToggleEquipmentSetUI()
        {
            var setUI = GetNodeOrNull<UI.EquipmentSetUI>("CanvasLayer/EquipmentSetUI");
            if (setUI != null)
            {
                setUI.ToggleSetUI();
            }
        }
        
        private void TogglePlayerProfileUI()
        {
            var profileUI = GetNodeOrNull<UI.PlayerProfileUI>("CanvasLayer/PlayerProfileUI");
            if (profileUI != null)
            {
                profileUI.Toggle();
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
        
        private void ToggleReputationUI()
        {
            var reputationUI = GetNodeOrNull<UI.ReputationUI>("CanvasLayer/ReputationUI");
            if (reputationUI != null)
            {
                reputationUI.Toggle();
            }
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
