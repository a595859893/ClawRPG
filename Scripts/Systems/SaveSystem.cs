using Godot;
using System;
using System.IO;
using System.Text.Json;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Enhanced Save and Load system with auto-save, backup, and metadata
    /// </summary>
    public partial class SaveSystem : Node
    {
        // Constants
        private const string SavePath = "user://saves/";
        private const string BackupPath = "user://saves/backups/";
        private const int MaxSaveSlots = 3;
        private const int MaxBackups = 5;
        private const float AutoSaveInterval = 300f; // 5 minutes
        
        // Auto-save state
        private float _autoSaveTimer = 0f;
        private bool _autoSaveEnabled = true;
        
        // Signals
        [Signal] public delegate void OnSaveCompleteEventHandler(int slot, bool success);
        [Signal] public delegate void OnLoadCompleteEventHandler(int slot, bool success);
        [Signal] public delegate void OnAutoSaveEventHandler(int slot);
        
        public class SaveData
        {
            public int Slot { get; set; }
            public string SaveName { get; set; } = "";
            public DateTime SaveTime { get; set; }
            public TimeSpan PlayTime { get; set; }
            public string LocationName { get; set; } = "Unknown";
            
            // Player data
            public int Level { get; set; } = 1;
            public int Experience { get; set; }
            public int CurrentHealth { get; set; } = 100;
            public int MaxHealth { get; set; } = 100;
            public int CurrentMana { get; set; } = 50;
            public int MaxMana { get; set; } = 50;
            public int Gold { get; set; }
            public int Strength { get; set; } = 10;
            public int Agility { get; set; } = 10;
            public int Intelligence { get; set; } = 10;
            
            // Position
            public float X { get; set; }
            public float Y { get; set; }
            
            // Inventory
            public int[] Inventory { get; set; } = new int[30];
            public int[] InventoryCounts { get; set; } = new int[30];
            public int[] Equipment { get; set; } = new int[4];
            
            // Quest progress
            public int[] CompletedQuests { get; set; } = new int[0];
            public int[] ActiveQuests { get; set; } = new int[0];
            public int[] QuestProgress { get; set; } = new int[0];
            
            // Skills
            public int[] LearnedSkills { get; set; } = new int[0];
            
            // World state
            public string CurrentArea { get; set; } = "forest";
            public bool[] ExploredAreas { get; set; } = new bool[10];
            
            // Pet data
            public int ActivePetId { get; set; } = -1;
            public int PetLevel { get; set; } = 1;
            
            // Game stats
            public int TotalKills { get; set; }
            public int TotalDeaths { get; set; }
            public int TotalDamageDealt { get; set; }
            public int TotalDamageTaken { get; set; }
            
            // Extended game stats
            public int TotalHealing { get; set; }
            public int CriticalHits { get; set; }
            public int PerfectBlocks { get; set; }
            public int Dodges { get; set; }
            public int GoldEarned { get; set; }
            public int GoldSpent { get; set; }
            public int ExperienceGained { get; set; }
            public int ItemsCollected { get; set; }
            public int ItemsCrafted { get; set; }
            public int QuestsCompleted { get; set; }
            public int SkillsLearned { get; set; }
            public int SkillsUsed { get; set; }
            public int RegionsDiscovered { get; set; }
            public int EnemiesEncountered { get; set; }
            public int BossesDefeated { get; set; }
            public float TotalPlayTime { get; set; }
            public int HighestLevel { get; set; }
            public int HighestCombo { get; set; }
            public int AchievementsUnlocked { get; set; }
            
            // Title system data
            public string CurrentTitleId { get; set; } = "";
            public string[] UnlockedTitleIds { get; set; } = new string[0];
            
            // Quick slot data
            public string[] QuickSlotItemIds { get; set; } = new string[9];
            public int[] QuickSlotQuantities { get; set; } = new int[9];
            
            // Mount system data
            public Dictionary<string, Dictionary<string, object>> MountData { get; set; } = new();
            
            // Bookmark system data
            public Dictionary<string, object> BookmarkData { get; set; } = new();
            
            // Auto bookmark system data
            public Dictionary<string, object> AutoBookmarkData { get; set; } = new();
            
            // Enhancement system data
            public Dictionary<string, object> EnhancementData { get; set; } = new();
            
            // Auto potion system data
            public Dictionary<string, object> AutoPotionData { get; set; } = new();
            
            // Enchantment system data
            public Dictionary<string, object> EnchantmentData { get; set; } = new();
            
            // Bounty system data
            public Dictionary<string, object> BountyData { get; set; } = new();
            
            // Weather system data
            public Dictionary<string, object> WeatherData { get; set; } = new();
            
            // Equipment visuals data
            public Dictionary<string, string> EquipmentVisualsData { get; set; } = new();
            
            // Player data (legacy support)
            public object PlayerData { get; set; }
        }
        
        // Save slot metadata (stored separately for quick loading)
        public class SaveSlotInfo
        {
            public int Slot { get; set; }
            public string SaveName { get; set; }
            public DateTime SaveTime { get; set; }
            public TimeSpan PlayTime { get; set; }
            public string LocationName { get; set; }
            public int Level { get; set; }
        }
        
        public override void _Ready()
        {
            EnsureDirectoriesExist();
        }
        
        public override void _Process(double delta)
        {
            // Auto-save timer
            if (_autoSaveEnabled)
            {
                _autoSaveTimer += (float)delta;
                if (_autoSaveTimer >= AutoSaveInterval)
                {
                    _autoSaveTimer = 0f;
                    PerformAutoSave();
                }
            }
        }
        
        private void EnsureDirectoriesExist()
        {
            // Main saves directory
            DirAccess dir = DirAccess.Open(SavePath);
            if (dir == null)
            {
                DirAccess.MakeDirRecursiveAbsolute(SavePath);
            }
            
            // Backup directory
            dir = DirAccess.Open(BackupPath);
            if (dir == null)
            {
                DirAccess.MakeDirRecursiveAbsolute(BackupPath);
            }
        }
        
        public bool HasSave(int slot)
        {
            if (slot < 0 || slot >= MaxSaveSlots) return false;
            string path = GetSavePath(slot);
            return File.Exists(path);
        }
        
        public void SaveGame(int slot, SaveData data, bool createBackup = true)
        {
            if (slot < 0 || slot >= MaxSaveSlots)
            {
                GD.PrintErr("Invalid save slot: " + slot);
                EmitSignal(SignalName.OnSaveComplete, slot, false);
                return;
            }
            
            try
            {
                // Create backup before saving
                if (createBackup && HasSave(slot))
                {
                    CreateBackup(slot);
                }
                
                data.Slot = slot;
                data.SaveTime = DateTime.Now;
                
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                string path = GetSavePath(slot);
                File.WriteAllText(path, json);
                
                // Update slot info
                UpdateSlotInfo(slot, data);
                
                GD.Print("Game saved to slot " + slot);
                EmitSignal(SignalName.OnSaveComplete, slot, true);
            }
            catch (Exception e)
            {
                GD.PrintErr("Failed to save game: " + e.Message);
                EmitSignal(SignalName.OnSaveComplete, slot, false);
            }
        }
        
        public SaveData LoadGame(int slot)
        {
            if (slot < 0 || slot >= MaxSaveSlots)
            {
                GD.PrintErr("Invalid save slot: " + slot);
                EmitSignal(SignalName.OnLoadComplete, slot, false);
                return null;
            }
            
            try
            {
                string path = GetSavePath(slot);
                
                if (!File.Exists(path))
                {
                    GD.PrintErr("No save file found in slot " + slot);
                    EmitSignal(SignalName.OnLoadComplete, slot, false);
                    return null;
                }
                
                string json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<SaveData>(json);
                
                GD.Print("Game loaded from slot " + slot + " - " + data.SaveName);
                EmitSignal(SignalName.OnLoadComplete, slot, true);
                return data;
            }
            catch (Exception e)
            {
                GD.PrintErr("Failed to load game: " + e.Message);
                EmitSignal(SignalName.OnLoadComplete, slot, false);
                
                // Try to load from backup
                return LoadFromBackup(slot);
            }
        }
        
        public SaveData[] GetAllSaves()
        {
            var saves = new SaveData[MaxSaveSlots];
            
            for (int i = 0; i < MaxSaveSlots; i++)
            {
                if (HasSave(i))
                {
                    saves[i] = LoadGame(i);
                }
            }
            
            return saves;
        }
        
        public SaveSlotInfo[] GetAllSlotInfo()
        {
            var infos = new SaveSlotInfo[MaxSaveSlots];
            
            for (int i = 0; i < MaxSaveSlots; i++)
            {
                infos[i] = GetSlotInfo(i);
            }
            
            return infos;
        }
        
        public SaveSlotInfo GetSlotInfo(int slot)
        {
            string infoPath = GetSlotInfoPath(slot);
            
            if (!File.Exists(infoPath))
            {
                return new SaveSlotInfo { Slot = slot };
            }
            
            try
            {
                string json = File.ReadAllText(infoPath);
                return JsonSerializer.Deserialize<SaveSlotInfo>(json);
            }
            catch
            {
                return new SaveSlotInfo { Slot = slot };
            }
        }
        
        private void UpdateSlotInfo(int slot, SaveData data)
        {
            var info = new SaveSlotInfo
            {
                Slot = slot,
                SaveName = data.SaveName,
                SaveTime = data.SaveTime,
                PlayTime = data.PlayTime,
                LocationName = data.CurrentArea,
                Level = data.Level
            };
            
            string json = JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(GetSlotInfoPath(slot), json);
        }
        
        public void DeleteSave(int slot)
        {
            if (slot < 0 || slot >= MaxSaveSlots) return;
            
            string path = GetSavePath(slot);
            string infoPath = GetSlotInfoPath(slot);
            
            if (File.Exists(path))
            {
                // Create final backup before deletion
                CreateBackup(slot);
                File.Delete(path);
                GD.Print("Save deleted from slot " + slot);
            }
            
            if (File.Exists(infoPath))
            {
                File.Delete(infoPath);
            }
        }
        
        private void CreateBackup(int slot)
        {
            string sourcePath = GetSavePath(slot);
            if (!File.Exists(sourcePath)) return;
            
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupName = $"save_{slot}_{timestamp}.json";
                string backupFullPath = BackupPath + backupName;
                
                File.Copy(sourcePath, backupFullPath);
                GD.Print("Backup created: " + backupName);
                
                // Clean old backups
                CleanOldBackups(slot);
            }
            catch (Exception e)
            {
                GD.PrintErr("Failed to create backup: " + e.Message);
            }
        }
        
        private void CleanOldBackups(int slot)
        {
            try
            {
                var files = Directory.GetFiles(BackupPath, $"save_{slot}_*.json");
                
                if (files.Length > MaxBackups)
                {
                    // Sort by creation time and delete oldest
                    Array.Sort(files);
                    for (int i = 0; i < files.Length - MaxBackups; i++)
                    {
                        File.Delete(files[i]);
                    }
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("Failed to clean backups: " + e.Message);
            }
        }
        
        private SaveData LoadFromBackup(int slot)
        {
            try
            {
                var files = Directory.GetFiles(BackupPath, $"save_{slot}_*.json");
                
                if (files.Length == 0)
                {
                    GD.PrintErr("No backup found for slot " + slot);
                    return null;
                }
                
                // Load most recent backup
                Array.Sort(files);
                string latestBackup = files[files.Length - 1];
                
                string json = File.ReadAllText(latestBackup);
                var data = JsonSerializer.Deserialize<SaveData>(json);
                
                GD.Print("Game restored from backup: " + latestBackup);
                EmitSignal(SignalName.OnLoadComplete, slot, true);
                return data;
            }
            catch (Exception e)
            {
                GD.PrintErr("Failed to load backup: " + e.Message);
                return null;
            }
        }
        
        private void PerformAutoSave()
        {
            // Get player data from game state
            var player = GetTree().GetFirstNodeInGroup("player") as Node;
            if (player == null) return;
            
            var data = CreateSaveDataFromPlayer(player);
            data.SaveName = "Auto Save";
            data.LocationName = GetCurrentAreaName();
            
            SaveGame(0, data, false); // Use slot 0 for auto-save without backup
            GD.Print("Auto-save completed");
            
            EmitSignal(SignalName.OnAutoSave, 0);
        }
        
        private SaveData CreateSaveDataFromPlayer(Node player)
        {
            var data = new SaveData();
            
            // Get player properties via reflection or direct access
            // This is a simplified version - actual implementation would read from Player node
            data.Level = 1;
            data.Experience = 0;
            data.CurrentHealth = 100;
            data.MaxHealth = 100;
            data.CurrentMana = 50;
            data.MaxMana = 50;
            data.Gold = 0;
            data.X = player?.Position.X ?? 0;
            data.Y = player?.Position.Y ?? 0;
            data.CurrentArea = "forest";
            
            // Save quick slot data
            if (QuickSlotSystem.Instance != null)
            {
                var quickSlotData = QuickSlotSystem.Instance.Serialize();
                if (quickSlotData != null)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        data.QuickSlotItemIds[i] = quickSlotData.ContainsKey($"slot_{i}_item") ? (string)quickSlotData[$"slot_{i}_item"] : "";
                        data.QuickSlotQuantities[i] = quickSlotData.ContainsKey($"slot_{i}_qty") ? (int)quickSlotData[$"slot_{i}_qty"] : 0;
                    }
                }
            }
            
            // Save mount data
            if (MountManager.Instance != null)
            {
                data.MountData = MountManager.Instance.Serialize();
            }
            
            // Save bookmark data
            if (BookmarkSystem.Instance != null)
            {
                data.BookmarkData = BookmarkSystem.Instance.Serialize();
            }
            
            // Save auto bookmark data
            var autoBookmarkSystem = GetNodeOrNull<Systems.AutoBookmarkSystem>("AutoBookmarkSystem");
            if (autoBookmarkSystem != null)
            {
                data.AutoBookmarkData = autoBookmarkSystem.Serialize();
            }
            
            // Save enhancement data
            var enhancementSystem = GetNodeOrNull<Systems.Enhancement.EnhancementSystem>("EnhancementSystem");
            if (enhancementSystem != null)
            {
                data.EnhancementData = enhancementSystem.Serialize();
            }
            
            // Save auto potion data
            var autoPotionSystem = GetNodeOrNull<Systems.AutoPotionSystem>("AutoPotionSystem");
            if (autoPotionSystem != null)
            {
                data.AutoPotionData = autoPotionSystem.Serialize();
            }
            
            // Save enchantment data
            data.EnchantmentData = ClawRPG.Scripts.Systems.Enchantment.EnchantmentSystem.Instance.Serialize();
            
            // Save bounty data
            data.BountyData = BountyManager.Instance.Serialize();
            
            // Save weather data
            var weatherSystem = GetNodeOrNull<WeatherSystem>("WeatherSystem");
            if (weatherSystem != null)
            {
                data.WeatherData = weatherSystem.Serialize();
            }
            
            // Save equipment visuals data
            var equipVisuals = GetNodeOrNull<UI.EquipmentVisuals>("EquipmentVisuals");
            if (equipVisuals != null)
            {
                data.EquipmentVisualsData = equipVisuals.Serialize();
            }
            
            return data;
        }
        
        private string GetCurrentAreaName()
        {
            // Get from game state or return default
            return "Unknown Area";
        }
        
        private string GetSavePath(int slot)
        {
            return SavePath + "save_" + slot + ".json";
        }
        
        private string GetSlotInfoPath(int slot)
        {
            return SavePath + "slot_" + slot + "_info.json";
        }
        
        // Quick save/load for convenience
        public void QuickSave(Node player)
        {
            var data = CreateSaveDataFromPlayer(player);
            data.SaveName = "Quick Save";
            SaveGame(0, data);
        }
        
        public SaveData QuickLoad()
        {
            return LoadGame(0);
        }
        
        // Auto-save control
        public void EnableAutoSave(bool enable)
        {
            _autoSaveEnabled = enable;
            if (enable)
            {
                _autoSaveTimer = 0f; // Reset timer when re-enabled
            }
        }
        
        public bool IsAutoSaveEnabled()
        {
            return _autoSaveEnabled;
        }
        
        // Export save to external file
        public bool ExportSave(int slot, string exportPath)
        {
            string sourcePath = GetSavePath(slot);
            if (!File.Exists(sourcePath)) return false;
            
            try
            {
                File.Copy(sourcePath, exportPath, true);
                GD.Print("Save exported to: " + exportPath);
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr("Failed to export save: " + e.Message);
                return false;
            }
        }
        
        // Import save from external file
        public bool ImportSave(string importPath, int slot)
        {
            if (!File.Exists(importPath)) return false;
            
            try
            {
                // Validate the import file first
                string json = File.ReadAllText(importPath);
                var data = JsonSerializer.Deserialize<SaveData>(json);
                
                if (data == null)
                {
                    GD.PrintErr("Invalid save file format");
                    return false;
                }
                
                string destPath = GetSavePath(slot);
                File.Copy(importPath, destPath, true);
                
                // Update slot info
                UpdateSlotInfo(slot, data);
                
                GD.Print("Save imported from: " + importPath + " to slot " + slot);
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr("Failed to import save: " + e.Message);
                return false;
            }
        }
        
        // Get save file size
        public long GetSaveFileSize(int slot)
        {
            string path = GetSavePath(slot);
            if (File.Exists(path))
            {
                return new FileInfo(path).Length;
            }
            return 0;
        }
        
        // Check if save is corrupted
        public bool IsSaveCorrupted(int slot)
        {
            string path = GetSavePath(slot);
            if (!File.Exists(path)) return true;
            
            try
            {
                string json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<SaveData>(json);
                return data == null;
            }
            catch
            {
                return true;
            }
        }
    }
}
