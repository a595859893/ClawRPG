using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using SaveDataManager = ClawRPG.Scripts.Systems.SaveDataManager;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Handles file I/O, save slot management, and backup operations
    /// </summary>
    public class SaveFileManager
    {
        // Constants
        public const string SavePath = "user://saves/";
        public const string BackupPath = "user://saves/backups/";
        public const int MaxSaveSlots = 3;
        public const int MaxBackups = 5;
        
        public SaveFileManager()
        {
            EnsureDirectoriesExist();
        }
        
        /// <summary>
        /// Ensure save directories exist
        /// </summary>
        public void EnsureDirectoriesExist()
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
        
        /// <summary>
        /// Check if a save slot has data
        /// </summary>
        public bool HasSave(int slot)
        {
            if (slot < 0 || slot >= MaxSaveSlots) return false;
            string path = GetSavePath(slot);
            return File.Exists(path);
        }
        
        /// <summary>
        /// Get the file path for a save slot
        /// </summary>
        public string GetSavePath(int slot)
        {
            return SavePath + "save_" + slot + ".json";
        }
        
        /// <summary>
        /// Get the file path for slot metadata
        /// </summary>
        public string GetSlotInfoPath(int slot)
        {
            return SavePath + "slot_" + slot + "_info.json";
        }
        
        /// <summary>
        /// Save game data to a slot
        /// </summary>
        public bool SaveGame(int slot, SaveDataManager.SaveData data, bool createBackup = true)
        {
            if (slot < 0 || slot >= MaxSaveSlots)
            {
                GD.PrintErr("Invalid save slot: " + slot);
                return false;
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
                
                string json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                string path = GetSavePath(slot);
                File.WriteAllText(path, json);
                
                // Update slot info
                UpdateSlotInfo(slot, data);
                
                GD.Print("Game saved to slot " + slot);
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr("Failed to save game: " + e.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Load game data from a slot
        /// </summary>
        public SaveDataManager.SaveData LoadGame(int slot)
        {
            if (slot < 0 || slot >= MaxSaveSlots)
            {
                GD.PrintErr("Invalid save slot: " + slot);
                return null;
            }
            
            try
            {
                string path = GetSavePath(slot);
                
                if (!File.Exists(path))
                {
                    GD.PrintErr("No save file found in slot " + slot);
                    return null;
                }
                
                string json = File.ReadAllText(path);
                var data = System.Text.Json.JsonSerializer.Deserialize<SaveDataManager.SaveData>(json);
                
                GD.Print("Game loaded from slot " + slot + " - " + data.SaveName);
                return data;
            }
            catch (Exception e)
            {
                GD.PrintErr("Failed to load game: " + e.Message);
                return null;
            }
        }
        
        /// <summary>
        /// Load all saves
        /// </summary>
        public SaveDataManager.SaveData[] GetAllSaves()
        {
            var saves = new SaveDataManager.SaveData[MaxSaveSlots];
            
            for (int i = 0; i < MaxSaveSlots; i++)
            {
                if (HasSave(i))
                {
                    saves[i] = LoadGame(i);
                }
            }
            
            return saves;
        }
        
        /// <summary>
        /// Get info for all save slots
        /// </summary>
        public SaveDataManager.SaveSlotInfo[] GetAllSlotInfo()
        {
            var infos = new SaveDataManager.SaveSlotInfo[MaxSaveSlots];
            
            for (int i = 0; i < MaxSaveSlots; i++)
            {
                infos[i] = GetSlotInfo(i);
            }
            
            return infos;
        }
        
        /// <summary>
        /// Get info for a specific slot
        /// </summary>
        public SaveDataManager.SaveSlotInfo GetSlotInfo(int slot)
        {
            string infoPath = GetSlotInfoPath(slot);
            
            if (!File.Exists(infoPath))
            {
                return new SaveDataManager.SaveSlotInfo { Slot = slot };
            }
            
            try
            {
                string json = File.ReadAllText(infoPath);
                return System.Text.Json.JsonSerializer.Deserialize<SaveDataManager.SaveSlotInfo>(json);
            }
            catch
            {
                return new SaveDataManager.SaveSlotInfo { Slot = slot };
            }
        }
        
        /// <summary>
        /// Update slot metadata
        /// </summary>
        public void UpdateSlotInfo(int slot, SaveDataManager.SaveData data)
        {
            var info = new SaveDataManager.SaveSlotInfo
            {
                Slot = slot,
                SaveName = data.SaveName,
                SaveTime = data.SaveTime,
                PlayTime = data.PlayTime,
                LocationName = data.CurrentArea,
                Level = data.Level
            };
            
            string json = System.Text.Json.JsonSerializer.Serialize(info, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(GetSlotInfoPath(slot), json);
        }
        
        /// <summary>
        /// Delete a save slot
        /// </summary>
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
        
        /// <summary>
        /// Create a backup for a slot
        /// </summary>
        public void CreateBackup(int slot)
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
        
        /// <summary>
        /// Clean old backups, keeping only MaxBackups
        /// </summary>
        public void CleanOldBackups(int slot)
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
        
        /// <summary>
        /// Load from backup (for recovery)
        /// </summary>
        public SaveDataManager.SaveData LoadFromBackup(int slot)
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
                var data = System.Text.Json.JsonSerializer.Deserialize<SaveDataManager.SaveData>(json);
                
                GD.Print("Game restored from backup: " + latestBackup);
                return data;
            }
            catch (Exception e)
            {
                GD.PrintErr("Failed to load backup: " + e.Message);
                return null;
            }
        }
        
        /// <summary>
        /// Export save to external path
        /// </summary>
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
        
        /// <summary>
        /// Import save from external path
        /// </summary>
        public bool ImportSave(string importPath, int slot)
        {
            if (!File.Exists(importPath)) return false;
            
            try
            {
                // Validate the import file first
                string json = File.ReadAllText(importPath);
                var data = System.Text.Json.JsonSerializer.Deserialize<SaveDataManager.SaveData>(json);
                
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
        
        /// <summary>
        /// Get save file size in bytes
        /// </summary>
        public long GetSaveFileSize(int slot)
        {
            string path = GetSavePath(slot);
            if (File.Exists(path))
            {
                return new FileInfo(path).Length;
            }
            return 0;
        }
        
        /// <summary>
        /// Check if save is corrupted
        /// </summary>
        public bool IsSaveCorrupted(int slot)
        {
            string path = GetSavePath(slot);
            if (!File.Exists(path)) return true;
            
            try
            {
                string json = File.ReadAllText(path);
                var data = System.Text.Json.JsonSerializer.Deserialize<SaveDataManager.SaveData>(json);
                return data == null;
            }
            catch
            {
                return true;
            }
        }
        
        /// <summary>
        /// Generic save data to a specific file path
        /// </summary>
        public void SaveToFile<T>(string path, T data)
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                string json = System.Text.Json.JsonSerializer.Serialize(data, options);
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to save data to {path}: " + e.Message);
            }
        }
        
        /// <summary>
        /// Generic load data from a specific file path
        /// </summary>
        public T LoadFromFile<T>(string path) where T : new()
        {
            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    return System.Text.Json.JsonSerializer.Deserialize<T>(json);
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to load data from {path}: " + e.Message);
            }
            return new T();
        }
        
        /// <summary>
        /// Save data with error handling and logging
        /// </summary>
        public bool SaveDataWithLogging(string path, object data, string systemName)
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                string json = System.Text.Json.JsonSerializer.Serialize(data, options);
                File.WriteAllText(path, json);
                GD.Print($"[{systemName}] Data saved");
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"[{systemName}] Failed to save data: " + e.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Load data with error handling and logging
        /// </summary>
        public T LoadDataWithLogging<T>(string path, string systemName) where T : new()
        {
            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var data = System.Text.Json.JsonSerializer.Deserialize<T>(json);
                    GD.Print($"[{systemName}] Data loaded");
                    return data;
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"[{systemName}] Failed to load data: " + e.Message);
            }
            return new T();
        }
    }
}
