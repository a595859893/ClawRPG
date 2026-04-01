using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Managers
{
    /// <summary>
    /// 游戏初始化管理器 - 负责游戏启动时的各项系统初始化
    /// </summary>
    public partial class GameInitializationManager : BaseSystem
    {
        public static GameInitializationManager Instance { get; private set; }
        
        // Initialization state
        private bool _isInitialized = false;
        private bool _isLoadingSaveData = false;
        private List<string> _initializationOrder = new List<string>();
        
        // Events
        public event Action OnInitializationComplete;
        public event Action<string> OnSystemInitialized;
        
        public override void _Ready()
        {
            Instance = this;
            base._Ready();
        }
        
        protected override void Initialize()
        {
            GD.Print("[GameInitializationManager] Starting game initialization...");
            
            // Record start time
            var startTime = DateTime.Now;
            
            // Initialize core systems in order
            InitializeCoreSystems();
            InitializeCombatSystems();
            InitializeUIManagers();
            InitializeContentSystems();
            InitializeWorldSystems();
            InitializeMultiplayerSystems();
            
            _isInitialized = true;
            var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
            GD.Print($"[GameInitializationManager] Initialization complete in {elapsed}ms");
            
            OnInitializationComplete?.Invoke();
        }
        
        private void InitializeCoreSystems()
        {
            // Core game systems that must be ready first
            AddToInitializationOrder("SaveSystem");
            AddToInitializationOrder("StatisticsManager");
            AddToInitializationOrder("GameStateManager");
        }
        
        private void InitializeCombatSystems()
        {
            // Combat-related systems
            AddToInitializationOrder("CombatStatusSystem");
            AddToInitializationOrder("ComboSystem");
            AddToInitializationOrder("MomentumSystem");
            AddToInitializationOrder("SkillTreeSystem");
            AddToInitializationOrder("WeaponMasterySystem");
        }
        
        private void InitializeUIManagers()
        {
            // UI systems that need to be ready
            AddToInitializationOrder("CombatUISystem");
            AddToInitializationOrder("HotkeyHUD");
        }
        
        private void InitializeContentSystems()
        {
            // Content-related systems
            AddToInitializationOrder("QuestSystem");
            AddToInitializationOrder("AchievementSystem");
            AddToInitializationOrder("TitleSystem");
            AddToInitializationOrder("DailyQuestSystem");
        }
        
        private void InitializeWorldSystems()
        {
            // World/environment systems
            AddToInitializationOrder("WeatherSystem");
            AddToInitializationOrder("RegionManager");
            AddToInitializationOrder("WorldBossSystem");
        }
        
        private void InitializeMultiplayerSystems()
        {
            // Multiplayer systems
            AddToInitializationOrder("PartySystem");
            AddToInitializationOrder("LeaderboardSystem");
        }
        
        private void AddToInitializationOrder(string systemName)
        {
            _initializationOrder.Add(systemName);
            OnSystemInitialized?.Invoke(systemName);
        }
        
        /// <summary>
        /// 加载游戏数据
        /// </summary>
        public void LoadGameData()
        {
            _isLoadingSaveData = true;
            
            try
            {
                var saveSystem = SaveSystem.Instance;
                if (saveSystem != null && saveSystem.HasSave(0))
                {
                    GD.Print("[GameInitializationManager] Found save file, loading...");
                    var data = saveSystem.LoadGame(0);
                    if (data != null)
                    {
                        LoadAllSystemsData(data);
                        GD.Print("[GameInitializationManager] Save data loaded successfully!");
                    }
                }
                else
                {
                    GD.Print("[GameInitializationManager] No save file found, starting new game");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[GameInitializationManager] Error loading save data: {ex.Message}");
            }
            finally
            {
                _isLoadingSaveData = false;
            }
        }
        
        private void LoadAllSystemsData(SaveDataManager.SaveData data)
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
            
            if (StatisticsManager.Instance != null)
            {
                StatisticsManager.Instance.LoadStatistics(statsData);
            }

            // Load combo system data (SkillComboSystem is the single source)
            var skillComboSystem = SkillComboSystem.Instance;
            if (skillComboSystem != null && data.ComboData != null)
            {
                skillComboSystem.ImportSaveData(new Dictionary(data.ComboData));
            }
            
            // Load keybinding data
            var keybindingSystem = GetNodeOrNull<Systems.KeybindingSystem>("/root/Main/KeybindingSystem");
            if (keybindingSystem != null && data.KeybindingData != null)
            {
                keybindingSystem.Deserialize(data.KeybindingData);
            }
            
            // Load pet story data
            var petStorySystem = GetNodeOrNull<PetStorySystem>("/root/Main/PetStorySystem");
            if (petStorySystem != null && data.PetStoryData != null)
            {
                petStorySystem.Deserialize(data.PetStoryData);
            }
        }
        
        /// <summary>
        /// 保存游戏数据
        /// </summary>
        public void SaveGameData()
        {
            if (_isLoadingSaveData) return;
            
            try
            {
                var saveSystem = SaveSystem.Instance;
                if (saveSystem == null)
                {
                    GD.PrintErr("[GameInitializationManager] SaveSystem.Instance is null");
                    return;
                }
                saveSystem.SaveGame(0, CreateSaveData());
                GD.Print("[GameInitializationManager] Game saved successfully!");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[GameInitializationManager] Error saving game: {ex.Message}");
            }
        }
        
        private SaveDataManager.SaveData CreateSaveData()
        {
            var data = new SaveDataManager.SaveData();
            // Collect data from all systems
            // This would be implemented based on the SaveData structure
            return data;
        }
        
        /// <summary>
        /// 重置游戏数据
        /// </summary>
        public void ResetGameData()
        {
            GD.Print("[GameInitializationManager] Resetting game data...");
            _isInitialized = false;
            // Reset all systems
            _initializationOrder.Clear();
            Initialize();
        }
        
        // Getters
        public bool IsGameInitialized() => _isInitialized;
        public bool IsLoadingSaveData() => _isLoadingSaveData;
        public List<string> GetInitializationOrder() => new List<string>(_initializationOrder);
        
        /// <summary>
        /// 创建节点结构 - 由 Main 在启动时调用
        /// </summary>
        public void CreateNodeStructure(Node2D mainNode)
        {
            GD.Print("[GameInitializationManager] Creating node structure...");
            
            // 创建敌人节点
            var enemies = new Node2D { Name = "Enemies" };
            mainNode.AddChild(enemies);

            // 创建物品节点
            var items = new Node2D { Name = "Items" };
            mainNode.AddChild(items);
            
            GD.Print("[GameInitializationManager] Node structure created");
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary
            {
                { "isInitialized", _isInitialized },
                { "initializationOrder", _initializationOrder }
            };
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.Contains("initializationOrder"))
            {
                _initializationOrder = data["initializationOrder"] as List<string>;
            }
        }
    }
}
