using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using SaveDataManager = ClawRPG.Scripts.Systems.SaveDataManager;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Handles file I/O operations, save slot management, and backup operations.
    /// Provides methods for saving, loading, importing, and exporting game data.
    /// </summary>
    public class SaveFileManager
    {
        // Constants
        /// <summary>Base path for save files.</summary>
        public const string SavePath = "user://saves/";
        
        /// <summary>Path for backup files.</summary>
        public const string BackupPath = "user://saves/backups/";
        
        /// <summary>Maximum number of save slots available.</summary>
        public const int MaxSaveSlots = 3;
        
        /// <summary>Maximum number of backups to keep per slot.</summary>
        public const int MaxBackups = 5;
        
        /// <summary>
        /// Creates a new SaveFileManager and ensures directories exist.
        /// </summary>
        public SaveFileManager()
        {
            EnsureDirectoriesExist();
        }
        
        /// <summary>
        /// Ensures save directories exist, creating them if necessary.
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
        /// Checks if a save exists in the specified slot.
        /// </summary>
        /// <param name="slot">Save slot index.</param>
        /// <returns>True if a save file exists in the slot.</returns>
        public bool HasSave(int slot)
        {
            if (slot < 0 || slot >= MaxSaveSlots) return false;
            string path = GetSavePath(slot);
            return File.Exists(path);
        }
        
        /// <summary>
        /// Gets the file path for a save slot.
        /// </summary>
        /// <param name="slot">Save slot index.</param>
        /// <returns>Full file path for the save slot.</returns>
        public string GetSavePath(int slot)
        {
            return SavePath + "save_" + slot + ".json";
        }
        
        /// <summary>
        /// Gets the file path for slot metadata.
        /// </summary>
        /// <param name="slot">Save slot index.</param>
        /// <returns>Full file path for the slot info file.</returns>
        public string GetSlotInfoPath(int slot)
        {
            return SavePath + "slot_" + slot + "_info.json";
        }
        
        /// <summary>
        /// Saves game data to a slot, optionally creating a backup first.
        /// </summary>
        /// <param name="slot">Save slot index (0 to MaxSaveSlots-1).</param>
        /// <param name="data">SaveData to save.</param>
        /// <param name="createBackup">Whether to create a backup before saving.</param>
        /// <returns>True if save was successful.</returns>
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
        /// Loads game data from a save slot.
        /// </summary>
        /// <param name="slot">Save slot index to load from.</param>
        /// <returns>Loaded SaveData, or null if load failed.</returns>
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
        /// Loads all saves from all slots.
        /// </summary>
        /// <returns>Array of SaveData for each slot (null if empty).</returns>
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
        /// Gets info for all save slots.
        /// </summary>
        /// <returns>Array of SaveSlotInfo for each slot.</returns>
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
        /// Gets info for a specific slot.
        /// </summary>
        /// <param name="slot">Save slot index.</param>
        /// <returns>SaveSlotInfo for the slot.</returns>
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
        /// Updates slot metadata with current save data.
        /// </summary>
        /// <param name="slot">Save slot index.</param>
        /// <param name="data">Save data to extract metadata from.</param>
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
        /// Deletes a save slot and its metadata.
        /// </summary>
        /// <param name="slot">Save slot index to delete.</param>
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
        /// Creates a backup of a save slot.
        /// </summary>
        /// <param name="slot">Save slot index to backup.</param>
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
        /// Cleans old backups, keeping only the most recent MaxBackups.
        /// </summary>
        /// <param name="slot">Save slot index.</param>
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
        /// Loads the most recent backup for a slot (for recovery).
        /// </summary>
        /// <param name="slot">Save slot index.</param>
        /// <returns>Loaded SaveData from backup, or null if no backup exists.</returns>
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
        /// Exports a save to an external path.
        /// </summary>
        /// <param name="slot">Save slot index to export.</param>
        /// <param name="exportPath">External file path to export to.</param>
        /// <returns>True if export was successful.</returns>
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
        /// Imports a save from an external path.
        /// </summary>
        /// <param name="importPath">External file path to import from.</param>
        /// <param name="slot">Target save slot index.</param>
        /// <returns>True if import was successful.</returns>
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
        /// Gets the save file size in bytes.
        /// </summary>
        /// <param name="slot">Save slot index.</param>
        /// <returns>File size in bytes, or 0 if save doesn't exist.</returns>
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
        /// Checks if a save file is corrupted.
        /// </summary>
        /// <param name="slot">Save slot index.</param>
        /// <returns>True if the save is corrupted or doesn't exist.</returns>
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
        /// Generic method to save data to a specific file path.
        /// </summary>
        /// <typeparam name="T">Type of data to save.</typeparam>
        /// <param name="path">File path to save to.</param>
        /// <param name="data">Data to serialize and save.</param>
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
        /// Generic method to load data from a specific file path.
        /// </summary>
        /// <typeparam name="T">Type of data to load.</typeparam>
        /// <param name="path">File path to load from.</param>
        /// <returns>Loaded data, or new instance if file doesn't exist.</returns>
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
