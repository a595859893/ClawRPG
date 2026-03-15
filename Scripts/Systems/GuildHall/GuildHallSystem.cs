using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.GuildHall {
    public class GuildHallSystem : BaseSystem {
        private GuildHallData _data;
        private GuildHallDatabase _database;
        
        public override void _Ready() {
            base._Ready();
            _database = GetNode<GuildHallDatabase>("GuildHallDatabase");
            InitializeData();
        }
        
        private void InitializeData() {
            _data = new GuildHallData();
            _data.GuildId = 1;
            _data.GuildName = "My Guild";
        }
        
        // Room Management
        public bool UnlockRoom(string roomName) {
            if (_data.UnlockedRooms.Contains(roomName)) return false;
            
            var room = _database.GetRoom(roomName);
            if (room == null) return false;
            
            if (_data.HallLevel < room.RequiredLevel) return false;
            
            _data.UnlockedRooms.Add(roomName);
            _data.RoomLevels[roomName] = 1;
            
            GD.Print($"[GuildHall] Room unlocked: {roomName}");
            return true;
        }
        
        public bool UpgradeRoom(string roomName) {
            if (!_data.UnlockedRooms.Contains(roomName)) return false;
            
            var room = _database.GetRoom(roomName);
            if (room == null) return false;
            
            int currentLevel = _data.RoomLevels.ContainsKey(roomName) ? _data.RoomLevels[roomName] : 1;
            
            _data.RoomLevels[roomName] = currentLevel + 1;
            
            GD.Print($"[GuildHall] Room upgraded: {roomName} to level {currentLevel + 1}");
            return true;
        }
        
        public int GetRoomLevel(string roomName) {
            return _data.RoomLevels.ContainsKey(roomName) ? _data.RoomLevels[roomName] : 0;
        }
        
        // Decoration Management
        public bool PurchaseDecoration(string decorationName) {
            var decoration = _database.GetDecoration(decorationName);
            if (decoration == null) return false;
            
            _data.DecorationInventory.Add(decorationName);
            
            GD.Print($"[GuildHall] Decoration purchased: {decorationName}");
            return true;
        }
        
        public bool PlaceDecoration(string decorationName) {
            if (!_data.DecorationInventory.Contains(decorationName)) return false;
            if (_data.Furniture.Contains(decorationName)) return false;
            
            _data.Furniture.Add(decorationName);
            
            GD.Print($"[GuildHall] Decoration placed: {decorationName}");
            return true;
        }
        
        public bool RemoveDecoration(string decorationName) {
            if (!_data.Furniture.Contains(decorationName)) return false;
            
            _data.Furniture.Remove(decorationName);
            
            GD.Print($"[GuildHall] Decoration removed: {decorationName}");
            return true;
        }
        
        // Gold Management
        public bool DepositGold(int amount) {
            if (amount <= 0) return false;
            
            _data.GoldDeposited += amount;
            if (_data.Statistics.ContainsKey("TotalGoldDeposited")) {
                _data.Statistics["TotalGoldDeposited"] += amount;
            }
            
            GD.Print($"[GuildHall] Gold deposited: {amount}");
            return true;
        }
        
        public bool WithdrawGold(int amount) {
            if (amount <= 0) return false;
            if (_data.GoldDeposited < amount) return false;
            
            _data.GoldDeposited -= amount;
            
            GD.Print($"[GuildHall] Gold withdrawn: {amount}");
            return true;
        }
        
        // Experience and Level
        public void AddExperience(int amount) {
            if (amount <= 0) return;
            
            _data.Experience += amount;
            
            while (_data.Experience >= _data.RequiredExperience) {
                _data.Experience -= _data.RequiredExperience;
                _data.HallLevel++;
                _data.RequiredExperience = CalculateRequiredExperience(_data.HallLevel);
                
                GD.Print($"[GuildHall] Guild hall upgraded to level {_data.HallLevel}");
            }
        }
        
        private int CalculateRequiredExperience(int level) {
            return (int)(1000 * Math.Pow(1.5, level - 1));
        }
        
        // Visitor Management
        public void RecordVisit(string playerName) {
            if (!_data.Visitors.Contains(playerName)) {
                _data.Visitors.Add(playerName);
            }
            
            _data.Statistics["TotalVisits"]++;
            
            GD.Print($"[GuildHall] Player visited: {playerName}");
        }
        
        // Statistics
        public Dictionary<string, int> GetStatistics() {
            return new Dictionary<string, int>(_data.Statistics);
        }
        
        public int GetTotalVisits() {
            return _data.Statistics.ContainsKey("TotalVisits") ? _data.Statistics["TotalVisits"] : 0;
        }
        
        public int GetTotalGoldDeposited() {
            return _data.Statistics.ContainsKey("TotalGoldDeposited") ? _data.Statistics["TotalGoldDeposited"] : 0;
        }
        
        // Getters
        public GuildHallData GetData() => _data;
        public GuildHallDatabase GetDatabase() => _database;
        public int GetHallLevel() => _data.HallLevel;
        public int GetExperience() => _data.Experience;
        public int GetRequiredExperience() => _data.RequiredExperience;
        public int GetGoldDeposited() => _data.GoldDeposited;
        public List<string> GetUnlockedRooms() => new List<string>(_data.UnlockedRooms);
        public List<string> GetFurniture() => new List<string>(_data.Furniture);
        public List<string> GetVisitors() => new List<string>(_data.Visitors);
        public List<string> GetDecorationInventory() => new List<string>(_data.DecorationInventory);
        
        // Save/Load
        public string GetSaveData() {
            return JsonHelper.ToJson(new Dictionary<string, object> {
                {"GuildId", _data.GuildId},
                {"GuildName", _data.GuildName},
                {"HallLevel", _data.HallLevel},
                {"Experience", _data.Experience},
                {"RequiredExperience", _data.RequiredExperience},
                {"UnlockedRooms", _data.UnlockedRooms},
                {"Furniture", _data.Furniture},
                {"GoldDeposited", _data.GoldDeposited},
                {"Visitors", _data.Visitors},
                {"RoomLevels", _data.RoomLevels},
                {"Statistics", _data.Statistics},
                {"DecorationInventory", _data.DecorationInventory}
            });
        }
        
        public void LoadFromSave(string jsonData) {
            var data = JsonHelper.FromJson<Dictionary<string, object>>(jsonData);
            if (data == null) return;
            
            _data.GuildId = data.ContainsKey("GuildId") ? Convert.ToInt32(data["GuildId"]) : 1;
            _data.GuildName = data.ContainsKey("GuildName") ? data["GuildName"].ToString() : "My Guild";
            _data.HallLevel = data.ContainsKey("HallLevel") ? Convert.ToInt32(data["HallLevel"]) : 1;
            _data.Experience = data.ContainsKey("Experience") ? Convert.ToInt32(data["Experience"]) : 0;
            _data.RequiredExperience = data.ContainsKey("RequiredExperience") ? Convert.ToInt32(data["RequiredExperience"]) : 1000;
            _data.GoldDeposited = data.ContainsKey("GoldDeposited") ? Convert.ToInt32(data["GoldDeposited"]) : 0;
            
            GD.Print("[GuildHall] Data loaded from save");
        }
    }
    
    public static class JsonHelper {
        public static string ToJson(object obj) {
            return Godot.JSON.Print(obj);
        }
        
        public static T FromJson<T>(string json) {
            var result = Godot.JSON.Parse(json);
            if (result.Error != Error.Ok) return default(T);
            return JsonValueToType<T>(result.Result);
        }
        
        private static T JsonValueToType<T>(object value) {
            if (value is Godot.Dictionary dict) {
                var type = typeof(T);
                var obj = Activator.CreateInstance<T>();
                return obj;
            }
            return default(T);
        }
    }
}
