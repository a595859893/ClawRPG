using Godot;
using System;
using ClawRPG.Scripts.Systems;
using ClawRPG.Scripts.Managers;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// MainGame - Handles game main loop, player spawning, and scene management
    /// </summary>
    public partial class MainGame : Node
    {
        private Main _main;
        private Player _player;
        private Node2D _enemies;
        private Node2D _items;
        
        private float _autoSaveTimer = 0f;
        private const float AutoSaveInterval = 300f; // 5 minutes
        
        // Managers
        private PlayerSpawnManager _playerSpawnManager;
        private EnemySpawnManager _enemySpawnManager;
        
        public MainGame()
        {
        }
        
        public void Initialize(Main main)
        {
            _main = main;
        }
        
        /// <summary>
        /// Create node structure for game objects
        /// </summary>
        public void CreateNodeStructure(Node2D mainNode)
        {
            _enemies = new Node2D();
            _enemies.Name = "Enemies";
            mainNode.AddChild(_enemies);

            _items = new Node2D();
            _items.Name = "Items";
            mainNode.AddChild(_items);
            
            GD.Print("Game node structure created");
        }
        
        /// <summary>
        /// Spawn the player
        /// </summary>
        public void SpawnPlayer(PackedScene playerScene)
        {
            if (_playerSpawnManager != null)
            {
                _player = _playerSpawnManager.SpawnPlayer(null, true);
                return;
            }
            
            // Fallback to direct spawning
            if (playerScene == null)
            {
                GD.PrintErr("PlayerScene not set!");
                return;
            }

            _player = playerScene.Instantiate<Player>();
            _player.AddToGroup("player");
            _player.GlobalPosition = new Vector2(640, 360); // Center of screen
            _main?.AddChild(_player);

            GD.Print("Player spawned");
        }
        
        /// <summary>
        /// Get the current player
        /// </summary>
        public Player GetPlayer()
        {
            return _player ?? _playerSpawnManager?.GetPlayer();
        }
        
        /// <summary>
        /// Set player spawn manager
        /// </summary>
        public void SetPlayerSpawnManager(PlayerSpawnManager manager)
        {
            _playerSpawnManager = manager;
        }
        
        /// <summary>
        /// Set enemy spawn manager
        /// </summary>
        public void SetEnemySpawnManager(EnemySpawnManager manager)
        {
            _enemySpawnManager = manager;
        }
        
        /// <summary>
        /// Process game logic every frame
        /// </summary>
        public void ProcessGame(double delta)
        {
            float dt = (float)delta;

            // Update boss mechanics system
            var bossMechanicsSystem = _main?.GetNode<Systems.BossMechanics.BossMechanicsSystem>("BossMechanicsSystem");
            if (bossMechanicsSystem != null)
            {
                bossMechanicsSystem._Process(dt);
            }

            // Update combat status system
            CombatStatusSystem.Instance._Process(dt);

            // Update cooking system
            var cookingSystem = _main?.GetNodeOrNull<Systems.Cooking.CookingSystem>("CookingSystem");
            if (cookingSystem != null)
            {
                cookingSystem.UpdateCooking(dt);
            }

            // Update survival challenge system
            var survivalChallengeSystem = _main?.GetNode<SurvivalChallengeSystem>("SurvivalChallengeSystem");
            if (survivalChallengeSystem != null)
            {
                survivalChallengeSystem._Process(dt);
            }

            // Update play time
            if (StatisticsManager.Instance != null)
            {
                StatisticsManager.Instance.AddPlayTime(dt);
            }

            // Auto save every 5 minutes
            _autoSaveTimer += dt;
            if (_autoSaveTimer >= AutoSaveInterval)
            {
                _autoSaveTimer = 0f;
                // Auto save logic would go here
                GD.Print("Auto save triggered...");
            }

            // Update potion effects
            if (_player != null && PotionManager.Instance != null)
            {
                PotionManager.Instance.UpdatePotionEffects(dt, _player);
            }
        }
        
        /// <summary>
        /// Update player UI elements
        /// </summary>
        public void UpdatePlayerUI()
        {
            if (_player == null) return;
            
            // Update health bar
            var healthBar = _main?.GetNodeOrNull<ProgressBar>("UI/HealthBar");
            if (healthBar != null)
            {
                healthBar.Value = _player.Health;
                healthBar.MaxValue = _player.MaxHealth;
            }

            // Update mana bar
            var manaBar = _main?.GetNodeOrNull<ProgressBar>("UI/ManaBar");
            if (manaBar != null)
            {
                manaBar.Value = _player.Mana;
                manaBar.MaxValue = _player.MaxMana;
            }

            // Update level display
            var levelLabel = _main?.GetNodeOrNull<Label>("UI/LevelLabel");
            if (levelLabel != null)
            {
                levelLabel.Text = "Lv." + _player.Level;
            }

            // Update experience bar
            var expBar = _main?.GetNodeOrNull<ProgressBar>("UI/ExpBar");
            if (expBar != null)
            {
                expBar.Value = _player.Experience;
                expBar.MaxValue = _player.ExperienceToNextLevel;
            }
        }
    }
}
