using Godot;
using System;
using System.Collections.Generic;

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
        public static bool IsPaused { get; private set; }
        public static int CurrentDay { get; private set; } = 1;
        
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
                // Would load game state here
            }
        }
        
        public override void _Process(double delta)
        {
            // Update UI
            UpdatePlayerUI();
            
            // Handle runes UI toggle (U key)
            if (Input.IsActionJustPressed("runes"))
            {
                ToggleRunesUI();
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
    }
}
