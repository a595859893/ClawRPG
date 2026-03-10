using Godot;
using System;
using System.IO;
using System.Text.Json;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Save and Load system for game progress
    /// </summary>
    public partial class SaveSystem : Node
    {
        private const string SavePath = "user://saves/";
        private const int MaxSaveSlots = 3;
        
        public class SaveData
        {
            public int Slot { get; set; }
            public string SaveName { get; set; } = "";
            public DateTime SaveTime { get; set; }
            
            // Player data
            public int Level { get; set; } = 1;
            public int Experience { get; set; }
            public int CurrentHealth { get; set; } = 100;
            public int MaxHealth { get; set; } = 100;
            public int CurrentMana { get; set; } = 50;
            public int MaxMana { get; set; } = 50;
            public int Gold { get; set; }
            
            // Position
            public float X { get; set; }
            public float Y { get; set; }
            
            // Inventory
            public int[] Inventory { get; set; } = new int[0];
            public int[] Equipment { get; set; } = new int[4]; // weapon, armor, accessory1, accessory2
            
            // Quest progress
            public int[] CompletedQuests { get; set; } = new int[0];
            public int[] ActiveQuests { get; set; } = new int[0];
            
            // Skills
            public int[] LearnedSkills { get; set; } = new int[0];
            
            // World state
            public string CurrentArea { get; set; } = "forest";
            public bool[] ExploredAreas { get; set; } = new bool[10];
        }
        
        public override void _Ready()
        {
            // Ensure save directory exists
            DirAccess dir = DirAccess.Open(SavePath);
            if (dir == null)
            {
                DirAccess.MakeDirRecursiveAbsolute(SavePath);
            }
        }
        
        public bool HasSave(int slot)
        {
            string path = GetSavePath(slot);
            return File.Exists(path);
        }
        
        public void SaveGame(int slot, SaveData data)
        {
            data.Slot = slot;
            data.SaveTime = DateTime.Now;
            
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            string path = GetSavePath(slot);
            
            File.WriteAllText(path, json);
            
            GD.Print("Game saved to slot " + slot);
        }
        
        public SaveData LoadGame(int slot)
        {
            string path = GetSavePath(slot);
            
            if (!File.Exists(path))
            {
                GD.PrintErr("No save file found in slot " + slot);
                return null;
            }
            
            string json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<SaveData>(json);
            
            GD.Print("Game loaded from slot " + slot);
            return data;
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
        
        public void DeleteSave(int slot)
        {
            string path = GetSavePath(slot);
            
            if (File.Exists(path))
            {
                File.Delete(path);
                GD.Print("Save deleted from slot " + slot);
            }
        }
        
        private string GetSavePath(int slot)
        {
            return SavePath + "save_" + slot + ".json";
        }
        
        // Quick save/load for convenience
        public void QuickSave(SaveData data)
        {
            SaveGame(0, data);
        }
        
        public SaveData QuickLoad()
        {
            return LoadGame(0);
        }
    }
}
