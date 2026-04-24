using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.GuildHall {
    public partial class GuildHallSystem : BaseSystem {
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
        
        /// <summary>
        /// 导出保存数据 (BaseSystem 接口)
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["guild_id"] = _data.GuildId;
            data["guild_name"] = _data.GuildName;
            data["hall_level"] = _data.HallLevel;
            data["experience"] = _data.Experience;
            data["required_experience"] = _data.RequiredExperience;
            data["gold_deposited"] = _data.GoldDeposited;
            
            // 解锁的房间
            var unlockedRooms = new Godot.Collections.Array();
            foreach (var room in _data.UnlockedRooms) unlockedRooms.Add(room);
            data["unlocked_rooms"] = unlockedRooms;
            
            // 家具
            var furniture = new Godot.Collections.Array();
            foreach (var item in _data.Furniture) furniture.Add(item);
            data["furniture"] = furniture;
            
            // 访客
            var visitors = new Godot.Collections.Array();
            foreach (var v in _data.Visitors) visitors.Add(v);
            data["visitors"] = visitors;
            
            // 装饰品库存
            var decorationInventory = new Godot.Collections.Array();
            foreach (var d in _data.DecorationInventory) decorationInventory.Add(d);
            data["decoration_inventory"] = decorationInventory;
            
            // 房间等级
            var roomLevels = new Godot.Collections.Dictionary();
            foreach (var kvp in _data.RoomLevels) roomLevels[kvp.Key] = kvp.Value;
            data["room_levels"] = roomLevels;
            
            // 统计
            var statistics = new Godot.Collections.Dictionary();
            foreach (var kvp in _data.Statistics) statistics[kvp.Key] = kvp.Value;
            data["statistics"] = statistics;
            
            return data;
        }
        
        /// <summary>
        /// 导入保存数据 (BaseSystem 接口)
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            _data.GuildId = data.ContainsKey("guild_id") ? Convert.ToInt32(data["guild_id"]) : 1;
            _data.GuildName = data.ContainsKey("guild_name") ? data["guild_name"].ToString() : "My Guild";
            _data.HallLevel = data.ContainsKey("hall_level") ? Convert.ToInt32(data["hall_level"]) : 1;
            _data.Experience = data.ContainsKey("experience") ? Convert.ToInt32(data["experience"]) : 0;
            _data.RequiredExperience = data.ContainsKey("required_experience") ? Convert.ToInt32(data["required_experience"]) : 1000;
            _data.GoldDeposited = data.ContainsKey("gold_deposited") ? Convert.ToInt32(data["gold_deposited"]) : 0;
            
            // 解锁的房间
            _data.UnlockedRooms.Clear();
            if (data.ContainsKey("unlocked_rooms")) {
                var rooms = data["unlocked_rooms"] as Godot.Collections.Array;
                if (rooms != null) {
                    foreach (var room in rooms) _data.UnlockedRooms.Add(room.ToString());
                }
            }
            
            // 家具
            _data.Furniture.Clear();
            if (data.ContainsKey("furniture")) {
                var furniture = data["furniture"] as Godot.Collections.Array;
                if (furniture != null) {
                    foreach (var item in furniture) _data.Furniture.Add(item.ToString());
                }
            }
            
            // 访客
            _data.Visitors.Clear();
            if (data.ContainsKey("visitors")) {
                var visitors = data["visitors"] as Godot.Collections.Array;
                if (visitors != null) {
                    foreach (var v in visitors) _data.Visitors.Add(v.ToString());
                }
            }
            
            // 装饰品库存
            _data.DecorationInventory.Clear();
            if (data.ContainsKey("decoration_inventory")) {
                var inventory = data["decoration_inventory"] as Godot.Collections.Array;
                if (inventory != null) {
                    foreach (var d in inventory) _data.DecorationInventory.Add(d.ToString());
                }
            }
            
            // 房间等级
            _data.RoomLevels.Clear();
            if (data.ContainsKey("room_levels")) {
                var levels = data["room_levels"] as Godot.Collections.Dictionary;
                if (levels != null) {
                    foreach (var kvp in levels) _data.RoomLevels[kvp.Key.ToString()] = Convert.ToInt32(kvp.Value);
                }
            }
            
            // 统计
            _data.Statistics.Clear();
            if (data.ContainsKey("statistics")) {
                var stats = data["statistics"] as Godot.Collections.Dictionary;
                if (stats != null) {
                    foreach (var kvp in stats) _data.Statistics[kvp.Key.ToString()] = Convert.ToInt32(kvp.Value);
                }
            }
            
            GD.Print($"[GuildHall] Imported: level {_data.HallLevel}, {_data.UnlockedRooms.Count} rooms");
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
            if (value is Godot.Collections.Dictionary dict) {
                var type = typeof(T);
                var obj = Activator.CreateInstance<T>();
                return obj;
            }
            return default(T);
        }
    }
}
