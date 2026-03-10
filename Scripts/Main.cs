using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

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
        
        public static bool IsPaused { get; private set; }
        public static int CurrentDay { get; private set; } = 1;
        
        public void SetGameState(GameState state)
        {
            _currentGameState = state;
            GD.Print("Game state changed to: " + state);
        }
        
        public GameState GetGameState() => _currentGameState;
        
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
            
            // Spawn player
            SpawnPlayer();
            
            // Initialize UI
            InitializeUI();
            
            // Load game data
            LoadGameData();
            
            GD.Print("Game initialized successfully!");
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
            
            GD.Print("UI initialized");
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
            
            GD.Print("Enemy spawned: " + enemyType);
        }
        
        public void AdvanceDay()
        {
            CurrentDay++;
            GD.Print("Day " + CurrentDay + " begins!");
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
    }
}
