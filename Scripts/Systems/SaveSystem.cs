using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using GameSystems;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Enhanced Save and Load system with auto-save, backup, and metadata
    /// Coordinates SaveDataManager, SaveFileManager, and SaveEncryption
    /// </summary>
    public partial class SaveSystem : BaseSystem
    {
        // Module instances
        private SaveFileManager _fileManager;
        private SaveEncryption _encryption;
        
        // Auto-save state
        private float _autoSaveTimer = 0f;
        private bool _autoSaveEnabled = true;
        
        // Constants
        private const float AutoSaveInterval = 300f; // 5 minutes
        
        // Signals
        [Signal] public delegate void OnSaveCompleteEventHandler(int slot, bool success);
        [Signal] public delegate void OnLoadCompleteEventHandler(int slot, bool success);
        [Signal] public delegate void OnAutoSaveEventHandler(int slot);
        
        // Type aliases for backward compatibility
        public class SaveData : SaveDataManager.SaveData { }
        public class SaveSlotInfo : SaveDataManager.SaveSlotInfo { }
        
        public override void _Ready()
        {
            // Initialize modules
            _fileManager = new SaveFileManager();
            _encryption = new SaveEncryption();
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
        
        /// <summary>
        /// Check if a save slot has data
        /// </summary>
        public bool HasSave(int slot)
        {
            return _fileManager.HasSave(slot);
        }
        
        /// <summary>
        /// Save game to a slot
        /// </summary>
        public void SaveGame(int slot, SaveData data, bool createBackup = true)
        {
            if (slot < 0 || slot >= SaveFileManager.MaxSaveSlots)
            {
                GD.PrintErr("Invalid save slot: " + slot);
                EmitSignal(SignalName.OnSaveComplete, slot, false);
                return;
            }
            
            bool success = _fileManager.SaveGame(slot, data, createBackup);
            EmitSignal(SignalName.OnSaveComplete, slot, success);
        }
        
        /// <summary>
        /// Load game from a slot
        /// </summary>
        public SaveData LoadGame(int slot)
        {
            if (slot < 0 || slot >= SaveFileManager.MaxSaveSlots)
            {
                GD.PrintErr("Invalid save slot: " + slot);
                EmitSignal(SignalName.OnLoadComplete, slot, false);
                return null;
            }
            
            var data = _fileManager.LoadGame(slot);
            bool success = data != null;
            
            if (!success)
            {
                // Try to load from backup
                data = _fileManager.LoadFromBackup(slot);
                success = data != null;
            }
            
            EmitSignal(SignalName.OnLoadComplete, slot, success);
            return data;
        }
        
        /// <summary>
        /// Get all saves
        /// </summary>
        public SaveData[] GetAllSaves()
        {
            return _fileManager.GetAllSaves();
        }
        
        /// <summary>
        /// Get all slot info
        /// </summary>
        public SaveSlotInfo[] GetAllSlotInfo()
        {
            return _fileManager.GetAllSlotInfo();
        }
        
        /// <summary>
        /// Get slot info
        /// </summary>
        public SaveSlotInfo GetSlotInfo(int slot)
        {
            return _fileManager.GetSlotInfo(slot);
        }
        
        /// <summary>
        /// Delete a save slot
        /// </summary>
        public void DeleteSave(int slot)
        {
            _fileManager.DeleteSave(slot);
        }
        
        /// <summary>
        /// Perform auto-save
        /// </summary>
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
        
        /// <summary>
        /// Create save data from player node
        /// </summary>
        private SaveData CreateSaveDataFromPlayer(Node player)
        {
            var data = new SaveData();
            
            // Get player properties
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
                data.UnlockedVisuals = equipVisuals.GetUnlockedVisualsData();
            }
            
            // Save combo system data
            var comboSystem = GetNodeOrNull<Systems.ComboSystem>("ComboSystem");
            if (comboSystem != null)
            {
                data.ComboData = comboSystem.Serialize();
            }
            
            // Save keybinding data
            var keybindingSystem = GetNodeOrNull<Systems.KeybindingSystem>("KeybindingSystem");
            if (keybindingSystem != null)
            {
                data.KeybindingData = keybindingSystem.Serialize();
            }
            
            // Save pet story data
            var petStorySystem = GetNodeOrNull<PetStorySystem>("PetStorySystem");
            if (petStorySystem != null)
            {
                data.PetStoryData = petStorySystem.Serialize();
            }
            
            // Save emote data
            var emoteSystem = GetNodeOrNull<Systems.Emote.EmoteSystem>("EmoteSystem");
            if (emoteSystem != null)
            {
                var emoteData = new Dictionary<string, object>();
                emoteSystem.SaveData(emoteData);
                data.EmoteData = emoteData;
            }
            
            // Save sealed tower data
            var sealedTowerManager = GetNodeOrNull<Systems.SealedTowerManager>("SealedTowerManager");
            if (sealedTowerManager != null)
            {
                var sealedTowerData = sealedTowerManager.SaveData();
                data.SealedTowerData = sealedTowerData;
            }
            
            // Save prestige data
            var prestigeSystem = GetNodeOrNull<Systems.PrestigeSystem>("PrestigeSystem");
            if (prestigeSystem != null)
            {
                var prestigeData = prestigeSystem.SaveData();
                data.PrestigeData = prestigeData;
            }
            
            // Save quick mode reward data
            var quickModeRewardSystem = GetNodeOrNull<Systems.QuickModeRewardSystem>("QuickModeRewardSystem");
            if (quickModeRewardSystem != null)
            {
                data.QuickModeRewardData = quickModeRewardSystem.ExportSaveData();
            }
            
            return data;
        }
        
        /// <summary>
        /// Get current area name
        /// </summary>
        private string GetCurrentAreaName()
        {
            return "Unknown Area";
        }
        
        /// <summary>
        /// Quick save
        /// </summary>
        public void QuickSave(Node player)
        {
            var data = CreateSaveDataFromPlayer(player);
            data.SaveName = "Quick Save";
            SaveGame(0, data);
        }
        
        /// <summary>
        /// Quick load
        /// </summary>
        public SaveData QuickLoad()
        {
            return LoadGame(0);
        }
        
        /// <summary>
        /// Enable/disable auto-save
        /// </summary>
        public void EnableAutoSave(bool enable)
        {
            _autoSaveEnabled = enable;
            if (enable)
            {
                _autoSaveTimer = 0f; // Reset timer when re-enabled
            }
        }
        
        /// <summary>
        /// Check if auto-save is enabled
        /// </summary>
        public bool IsAutoSaveEnabled()
        {
            return _autoSaveEnabled;
        }
        
        /// <summary>
        /// Export save to external file
        /// </summary>
        public bool ExportSave(int slot, string exportPath)
        {
            return _fileManager.ExportSave(slot, exportPath);
        }
        
        /// <summary>
        /// Import save from external file
        /// </summary>
        public bool ImportSave(string importPath, int slot)
        {
            return _fileManager.ImportSave(importPath, slot);
        }
        
        /// <summary>
        /// Get save file size
        /// </summary>
        public long GetSaveFileSize(int slot)
        {
            return _fileManager.GetSaveFileSize(slot);
        }
        
        /// <summary>
        /// Check if save is corrupted
        /// </summary>
        public bool IsSaveCorrupted(int slot)
        {
            return _fileManager.IsSaveCorrupted(slot);
        }
        
        /// <summary>
        /// Enable encryption
        /// </summary>
        public void EnableEncryption()
        {
            _encryption.Enable();
        }
        
        /// <summary>
        /// Disable encryption
        /// </summary>
        public void DisableEncryption()
        {
            _encryption.Disable();
        }
        
        // ===== Pet Talent System Save/Load =====
        
        public void SavePetTalentData(PlayerPetTalentData data)
        {
            _fileManager.SaveDataWithLogging("user://pet_talent_data.json", data, "SaveSystem");
        }

        public PlayerPetTalentData LoadPetTalentData()
        {
            return _fileManager.LoadDataWithLogging<PlayerPetTalentData>("user://pet_talent_data.json", "SaveSystem");
        }
        
        // ===== Loot Drop System Save/Load =====
        
        public void SaveLootDropData(LootDropData.PlayerLootData data)
        {
            _fileManager.SaveDataWithLogging("user://loot_drop_data.json", data, "SaveSystem");
        }

        public LootDropData.PlayerLootData LoadLootDropData()
        {
            return _fileManager.LoadDataWithLogging<LootDropData.PlayerLootData>("user://loot_drop_data.json", "SaveSystem");
        }
        
        // ===== Equipment Durability System Save/Load =====
        
        public void SaveEquipmentDurabilityData(Dictionary<string, object> data)
        {
            _fileManager.SaveDataWithLogging("user://equipment_durability_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadEquipmentDurabilityData()
        {
            return _fileManager.LoadDataWithLogging<Dictionary<string, object>>("user://equipment_durability_data.json", "SaveSystem");
        }

        // ===== Collectible System Save/Load =====

        public void SaveCollectibleData(Dictionary<string, object> data)
        {
            _fileManager.SaveDataWithLogging("user://collectible_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadCollectibleData()
        {
            return _fileManager.LoadDataWithLogging<Dictionary<string, object>>("user://collectible_data.json", "SaveSystem");
        }

        // ===== Seasonal Event System Save/Load =====

        public void SaveSeasonalEventData(Dictionary<string, object> data)
        {
            _fileManager.SaveDataWithLogging("user://seasonal_event_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadSeasonalEventData()
        {
            return _fileManager.LoadDataWithLogging<Dictionary<string, object>>("user://seasonal_event_data.json", "SaveSystem");
        }

        public void SaveMountRaceData(Dictionary<string, object> data)
        {
            _fileManager.SaveDataWithLogging("user://mount_race_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadMountRaceData()
        {
            return _fileManager.LoadDataWithLogging<Dictionary<string, object>>("user://mount_race_data.json", "SaveSystem");
        }

        public void SaveMountBattleArenaData(Dictionary<string, object> data)
        {
            _fileManager.SaveDataWithLogging("user://mount_battle_arena_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadMountBattleArenaData()
        {
            return _fileManager.LoadDataWithLogging<Dictionary<string, object>>("user://mount_battle_arena_data.json", "SaveSystem");
        }

        public void SavePlayerTalentData(Dictionary<string, object> data)
        {
            _fileManager.SaveDataWithLogging("user://player_talent_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadPlayerTalentData()
        {
            return _fileManager.LoadDataWithLogging<Dictionary<string, object>>("user://player_talent_data.json", "SaveSystem");
        }

        public void SavePetExpeditionData(Dictionary<string, object> data)
        {
            _fileManager.SaveDataWithLogging("user://pet_expedition_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadPetExpeditionData()
        {
            return _fileManager.LoadDataWithLogging<Dictionary<string, object>>("user://pet_expedition_data.json", "SaveSystem");
        }

        public void SavePetTrainingData(Dictionary<string, Variant> data)
        {
            _fileManager.SaveDataWithLogging("user://pet_training_data.json", data, "SaveSystem");
        }

        public Dictionary<string, Variant> LoadPetTrainingData()
        {
            return _fileManager.LoadDataWithLogging<Dictionary<string, Variant>>("user://pet_training_data.json", "SaveSystem");
        }

        public void SavePetHabitatData(PlayerHabitatData data)
        {
            try
            {
                var dict = new Dictionary<string, object>();
                
                dict["current_habitat_id"] = data.CurrentHabitatId;
                dict["total_comfort"] = data.TotalComfort;
                dict["total_attraction"] = data.TotalAttraction;
                dict["decorations_purchased"] = data.DecorationsPurchased;
                dict["gold_spent_on_decorations"] = data.GoldSpentOnDecorations;
                dict["habitat_visits"] = data.HabitatVisits;
                dict["pets_attracted"] = data.PetsAttracted;
                
                // Serialize placed decorations
                var placedList = new List<Dictionary<string, object>>();
                foreach (var dec in data.PlacedDecorations)
                {
                    placedList.Add(new Dictionary<string, object>
                    {
                        ["decoration_id"] = dec.DecorationId,
                        ["slot"] = dec.Slot,
                        ["placed_at"] = dec.PlacedAt.ToString("o")
                    });
                }
                dict["placed_decorations"] = placedList;
                
                // Serialize decoration counts
                dict["decoration_counts"] = data.DecorationCounts;
                
                _fileManager.SaveDataWithLogging("user://pet_habitat_data.json", dict, "SaveSystem");
            }
            catch (Exception e)
            {
                GD.PrintErr("[SaveSystem] Failed to save pet habitat data: " + e.Message);
            }
        }

        public PlayerHabitatData LoadPetHabitatData()
        {
            try
            {
                string path = "user://pet_habitat_data.json";
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    if (dict != null)
                    {
                        var data = new PlayerHabitatData();
                        
                        data.CurrentHabitatId = dict.ContainsKey("current_habitat_id") ? (string)dict["current_habitat_id"] : "meadow";
                        data.TotalComfort = dict.ContainsKey("total_comfort") ? Convert.ToInt32(dict["total_comfort"]) : 0;
                        data.TotalAttraction = dict.ContainsKey("total_attraction") ? Convert.ToInt32(dict["total_attraction"]) : 0;
                        data.DecorationsPurchased = dict.ContainsKey("decorations_purchased") ? Convert.ToInt32(dict["decorations_purchased"]) : 0;
                        data.GoldSpentOnDecorations = dict.ContainsKey("gold_spent_on_decorations") ? Convert.ToInt32(dict["gold_spent_on_decorations"]) : 0;
                        data.HabitatVisits = dict.ContainsKey("habitat_visits") ? Convert.ToInt32(dict["habitat_visits"]) : 0;
                        data.PetsAttracted = dict.ContainsKey("pets_attracted") ? Convert.ToInt32(dict["pets_attracted"]) : 0;
                        
                        // Deserialize placed decorations
                        if (dict.ContainsKey("placed_decorations") && dict["placed_decorations"] != null)
                        {
                            var placedList = (System.Text.Json.JsonElement)dict["placed_decorations"];
                            foreach (var item in placedList.EnumerateArray())
                            {
                                var dec = new PlacedDecoration
                                {
                                    DecorationId = item.GetProperty("decoration_id").GetString(),
                                    Slot = item.GetProperty("slot").GetInt32(),
                                    PlacedAt = DateTime.Parse(item.GetProperty("placed_at").GetString())
                                };
                                data.PlacedDecorations.Add(dec);
                            }
                        }
                        
                        // Deserialize decoration counts
                        if (dict.ContainsKey("decoration_counts") && dict["decoration_counts"] != null)
                        {
                            var counts = (System.Text.Json.JsonElement)dict["decoration_counts"];
                            foreach (var item in counts.EnumerateObject())
                            {
                                data.DecorationCounts[item.Name] = item.Value.GetInt32();
                            }
                        }
                        
                        GD.Print("[SaveSystem] Pet habitat data loaded");
                        return data;
                    }
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("[SaveSystem] Failed to load pet habitat data: " + e.Message);
            }
            return new PlayerHabitatData();
        }

        public void SaveMountExpeditionData(Dictionary<string, object> data)
        {
            _fileManager.SaveDataWithLogging("user://mount_expedition_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadMountExpeditionData()
        {
            return _fileManager.LoadDataWithLogging<Dictionary<string, object>>("user://mount_expedition_data.json", "SaveSystem");
        }

        // ============ Quick Mode Reward Data ============
        
        /// <summary>
        /// Save quick mode reward system data (standalone, not part of main save)
        /// </summary>
        public void SaveQuickModeData(Dictionary<string, object> data)
        {
            try
            {
                var quickModeData = new Dictionary<string, object>();
                foreach (var kvp in data)
                {
                    quickModeData[kvp.Key] = kvp.Value;
                }
                
                // Also save to main save data
                var mainSave = LoadGame(0);
                if (mainSave != null)
                {
                    mainSave.QuickModeRewardData = quickModeData;
                    SaveGame(0, mainSave, false);
                }
                
                GD.Print("[SaveSystem] Quick mode reward data saved");
            }
            catch (Exception e)
            {
                GD.PrintErr("[SaveSystem] Failed to save quick mode reward data: " + e.Message);
            }
        }
        
        /// <summary>
        /// Load quick mode reward system data
        /// </summary>
        public Dictionary<string, object> LoadQuickModeData()
        {
            try
            {
                var mainSave = LoadGame(0);
                if (mainSave != null && mainSave.QuickModeRewardData != null)
                {
                    GD.Print("[SaveSystem] Quick mode reward data loaded");
                    return mainSave.QuickModeRewardData;
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("[SaveSystem] Failed to load quick mode reward data: " + e.Message);
            }
            return new Dictionary<string, object>();
        }
    }
}
