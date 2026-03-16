using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Bosses {
    /// <summary>
    /// Manages boss encounters and spawning
    /// </summary>
    public partial class BossManager : BaseSystem
    {
        public static BossManager Instance { get; private set; }
        
        // Current active boss
        private Boss _currentBoss;
        private BossData _currentBossData;
        
        // Boss encounter state
        private bool _isBossActive;
        private bool _bossDefeated;
        
        // Boss spawn points
        private Dictionary<string, Vector2> _bossSpawnPoints;
        
        // Events
        public event Action<BossData> OnBossSpawned;
        public event Action<BossData> OnBossDefeated;
        public event Action<int> OnBossPhaseChange;
        public event Action OnBossEnrage;
        
        protected override void Initialize()
        {
            Instance = this;
            InitializeSpawnPoints();
            GD.Print("BossManager initialized");
            IsInitialized = true;
        }
        
        private void InitializeSpawnPoints()
        {
            _bossSpawnPoints = new Dictionary<string, Vector2>();
            
            // Define spawn points for each boss (world coordinates)
            _bossSpawnPoints["treant_king"] = new Vector2(500, 300);
            _bossSpawnPoints["crystal_golem"] = new Vector2(1200, 500);
            _bossSpawnPoints["inferno_dragon"] = new Vector2(2000, 400);
            _bossSpawnPoints["dark_assassin"] = new Vector2(800, 700);
            _bossSpawnPoints["frost_wyrm"] = new Vector2(1500, 200);
            _bossSpawnPoints["demon_lord"] = new Vector2(3000, 500);
            _bossSpawnPoints["goblin_king"] = new Vector2(200, 150);
            _bossSpawnPoints["orc_chief"] = new Vector2(600, 400);
            _bossSpawnPoints["skeleton_lord"] = new Vector2(900, 600);
        }
        
        /// <summary>
        /// Spawn a boss by ID
        /// </summary>
        public bool SpawnBoss(string bossId, Vector2 position = default)
        {
            if (_isBossActive)
            {
                GD.Print("A boss is already active!");
                return false;
            }
            
            BossData bossData = BossDatabase.GetBoss(bossId);
            if (bossData == null)
            {
                GD.PrintErr($"Cannot spawn boss: {bossId} not found");
                return false;
            }
            
            // Create boss instance
            var bossScene = GD.Load<PackedScene>("res://Scenes/Boss.tscn");
            if (bossScene == null)
            {
                // Create boss programmatically if scene doesn't exist
                return SpawnBossProgrammatic(bossData, position);
            }
            
            _currentBoss = bossScene.Instantiate<Boss>();
            _currentBoss.BossTitle = bossData.Title;
            _currentBoss.MaxHealth = bossData.MaxHealth;
            _currentBoss.MoveSpeed = bossData.MoveSpeed;
            _currentBoss.AttackDamage = bossData.AttackDamage;
            _currentBoss.AttackRange = bossData.AttackRange;
            _currentBoss.AttackCooldown = bossData.AttackCooldown;
            _currentBoss.ChaseRange = bossData.ChaseRange;
            _currentBoss.DetectionRange = bossData.DetectionRange;
            _currentBoss.ExperienceReward = bossData.ExperienceReward;
            _currentBoss.DropItems = bossData.DropItems;
            _currentBoss.PhaseCount = bossData.PhaseCount;
            _currentBoss.PhaseHealthThresholds = bossData.PhaseHealthThresholds;
            _currentBoss.EnrageTime = bossData.EnrageTime;
            _currentBoss.AbilityCooldown = bossData.AbilityCooldown;
            _currentBoss.SpecialAbilities = bossData.SpecialAbilities;
            
            // Subscribe to events
            _currentBoss.OnPhaseChange += HandlePhaseChange;
            _currentBoss.OnEnrage += HandleEnrage;
            _currentBoss.OnSpecialAbility += HandleSpecialAbility;
            
            // Set position
            if (position == default && _bossSpawnPoints.TryGetValue(bossId, out var spawnPos))
            {
                position = spawnPos;
            }
            _currentBoss.GlobalPosition = position;
            
            // Add to scene
            GetTree().CurrentScene.AddChild(_currentBoss);
            
            _currentBossData = bossData;
            _isBossActive = true;
            _bossDefeated = false; 
            
            GD.Print($"Boss spawned: {bossData.Title}");
            
            OnBossSpawned?.Invoke(bossData);
            
            // Notify UI
            NotifyBossSpawn(bossData);
            
            return true;
        }
        
        private bool SpawnBossProgrammatic(BossData bossData, Vector2 position)
        {
            var bossNode = new Boss();
            bossNode.Name = bossData.Name;
            bossNode.EnemyName = bossData.Name;
            bossNode.BossTitle = bossData.Title;
            bossNode.MaxHealth = bossData.MaxHealth;
            bossNode.MoveSpeed = bossData.MoveSpeed;
            bossNode.AttackDamage = bossData.AttackDamage;
            bossNode.AttackRange = bossData.AttackRange;
            bossNode.AttackCooldown = bossData.AttackCooldown;
            bossNode.ChaseRange = bossData.ChaseRange;
            bossNode.DetectionRange = bossData.DetectionRange;
            bossNode.ExperienceReward = bossData.ExperienceReward;
            bossNode.DropItems = bossData.DropItems;
            bossNode.PhaseCount = bossData.PhaseCount;
            bossNode.PhaseHealthThresholds = bossData.PhaseHealthThresholds;
            bossNode.EnrageTime = bossData.EnrageTime;
            bossNode.AbilityCooldown = bossData.AbilityCooldown;
            bossNode.SpecialAbilities = bossData.SpecialAbilities;
            
            // Add sprite
            var sprite = new Sprite2D();
            bossNode.AddChild(sprite);
            
            // Add collision
            var collision = new CollisionShape2D();
            var shape = new CircleShape2D();
            shape.Radius = 32;
            collision.Shape = shape;
            bossNode.AddChild(collision);
            
            // Set position
            if (position == default && _bossSpawnPoints.TryGetValue(bossData.Id, out var spawnPos))
            {
                position = spawnPos;
            }
            bossNode.GlobalPosition = position;
            
            // Subscribe to events
            bossNode.OnPhaseChange += HandlePhaseChange;
            bossNode.OnEnrage += HandleEnrage;
            bossNode.OnSpecialAbility += HandleSpecialAbility;
            
            // Add to scene
            GetTree().CurrentScene.AddChild(bossNode);
            
            _currentBoss = bossNode;
            _currentBossData = bossData;
            _isBossActive = true;
            _bossDefeated = false; 
            
            GD.Print($"Boss spawned programmatically: {bossData.Title}");
            
            OnBossSpawned?.Invoke(bossData);
            NotifyBossSpawn(bossData);
            
            return true;
        }
        
        private void HandlePhaseChange(int newPhase)
        {
            GD.Print($"Boss phase changed to: {newPhase}");
            OnBossPhaseChange?.Invoke(newPhase);
            NotifyPhaseChange(newPhase);
        }
        
        private void HandleEnrage()
        {
            GD.Print("Boss is enraged!");
            OnBossEnrage?.Invoke();
            NotifyEnrage();
        }
        
        private void HandleSpecialAbility(string ability)
        {
            GD.Print($"Boss uses ability: {ability}");
            NotifyBossAbility(ability);
        }
        
        /// <summary>
        /// Called when boss dies
        /// </summary>
        public void OnBossDeath()
        {
            if (!_isBossActive || _bossDefeated) return;
            
            _bossDefeated = true;
            _isBossActive = false; 
            
            GD.Print($"Boss defeated: {_currentBossData.Title}");
            
            OnBossDefeated?.Invoke(_currentBossData);
            NotifyBossDefeated(_currentBossData);
            
            // Award completion
            AwardBossCompletion();
            
            _currentBoss = null;
            _currentBossData = null;
        }
        
        private void AwardBossCompletion()
        {
            if (_currentBossData == null) return;
            
            var player = GetTree().GetFirstNodeInGroup("player") as Characters.Player;
            if (player != null)
            {
                player.GainExperience(_currentBossData.ExperienceReward);
                GD.Print($"Player awarded {_currentBossData.ExperienceReward} XP for boss kill!");
            }
        }
        
        /// <summary>
        /// Get current boss info
        /// </summary>
        public Boss GetCurrentBoss() => _currentBoss;
        public bool IsBossActive() => _isBossActive;
        public bool IsBossDefeated() => _bossDefeated;
        
        /// <summary>
        /// Get all available bosses
        /// </summary>
        public List<BossData> GetAllBosses() => BossDatabase.GetAllBosses();
        
        // UI Notification methods (would connect to UI system)
        private void NotifyBossSpawn(BossData boss)
        {
            // Connect to GameMessageSystem if available
            var msgSystem = GetTree().GetFirstNodeInGroup("GameMessageSystem");
            if (msgSystem != null)
            {
                msgSystem.Call("ShowDanger", $"⚠️ BOSS APPEARED: {boss.Title}!");
            }
            
            // Connect to BossHealthBarUI if available
            var bossUI = GetTree().GetFirstNodeInGroup("BossHealthBarUI");
            if (bossUI != null)
            {
                bossUI.Call("ShowBoss", boss.Title, boss.MaxHealth);
            }
        }
        
        private void NotifyPhaseChange(int phase)
        {
            var msgSystem = GetTree().GetFirstNodeInGroup("GameMessageSystem");
            if (msgSystem != null)
            {
                msgSystem.Call("ShowWarning", $"⚡ BOSS PHASE {phase}!");
            }
        }
        
        private void NotifyEnrage()
        {
            var msgSystem = GetTree().GetFirstNodeInGroup("GameMessageSystem");
            if (msgSystem != null)
            {
                msgSystem.Call("ShowDanger", $"🔥 BOSS ENRAGED! DANGER!");
            }
            
            var flashEffect = GetTree().GetFirstNodeInGroup("ScreenFlashEffect");
            if (flashEffect != null)
            {
                flashEffect.Call("Flash", "red", 0.5f);
            }
        }
        
        private void NotifyBossAbility(string ability)
        {
            var msgSystem = GetTree().GetFirstNodeInGroup("GameMessageSystem");
            if (msgSystem != null)
            {
                msgSystem.Call("ShowWarning", $"👹 BOSS USES: {ability.ToUpper().Replace("_", " ")}!");
            }
        }
        
        private void NotifyBossDefeated(BossData boss)
        {
            var msgSystem = GetTree().GetFirstNodeInGroup("GameMessageSystem");
            if (msgSystem != null)
            {
                msgSystem.Call("ShowAchievement", $"🏆 BOSS DEFEATED: {boss.Title}!");
            }
            
            var bossUI = GetTree().GetFirstNodeInGroup("BossHealthBarUI");
            if (bossUI != null)
            {
                bossUI.Call("HideBoss");
            }
        }
        
        /// <summary>
        /// Spawn a random boss
        /// </summary>
        public bool SpawnRandomBoss(Vector2 position = default)
        {
            var bosses = BossDatabase.GetAllBosses();
            if (bosses.Count == 0) return false;
            
            var randomBoss = bosses[(int)GD.Randi() % bosses.Count];
            return SpawnBoss(randomBoss.Id, position);
        }
        
        /// <summary>
        /// Spawn a boss by difficulty
        /// </summary>
        public bool SpawnBossByDifficulty(string difficulty, Vector2 position = default)
        {
            var bosses = BossDatabase.GetBossesByDifficulty(difficulty);
            if (bosses.Count == 0) return false;
            
            var randomBoss = bosses[(int)GD.Randi() % bosses.Count];
            return SpawnBoss(randomBoss.Id, position);
        }
        
        /// <summary>
        /// Export save data for persistence
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            data["is_boss_active"] = _isBossActive;
            data["boss_defeated"] = _bossDefeated;
            
            if (_currentBossData != null)
            {
                data["current_boss_id"] = _currentBossData.Id;
            }
            
            return data;
        }
        
        /// <summary>
        /// Import save data on game load
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            if (data.Contains("is_boss_active"))
            {
                _isBossActive = (bool)data["is_boss_active"];
            }
            
            if (data.Contains("boss_defeated"))
            {
                _bossDefeated = (bool)data["boss_defeated"];
            }
            
            if (data.Contains("current_boss_id"))
            {
                var bossId = (string)data["current_boss_id"];
                if (_isBossActive && !_bossDefeated)
                {
                    // Resume the boss encounter if it was active
                    SpawnBoss(bossId);
                }
            }
            
            GD.Print($"BossManager save data loaded: active={_isBossActive}, defeated={_bossDefeated}");
        }
    }
}
