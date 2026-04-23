using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.GuildHall {
    public partial class GuildHallData : Godot.Resource {
        [Export] public int GuildId { get; set; }
        [Export] public string GuildName { get; set; }
        [Export] public int HallLevel { get; set; }
        [Export] public int Experience { get; set; }
        [Export] public int RequiredExperience { get; set; }
        public List<string> UnlockedRooms { get; set; } = new List<string>();
        public List<string> Furniture { get; set; } = new List<string>();
        public int GoldDeposited { get; set; }
        public List<string> Visitors { get; set; } = new List<string>();
        public Dictionary<string, int> RoomLevels { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> Statistics { get; set; } = new Dictionary<string, int>();
        public List<string> DecorationInventory { get; set; } = new List<string>();
        
        public GuildHallData() {
            HallLevel = 1;
            Experience = 0;
            RequiredExperience = 1000;
            UnlockedRooms = new List<string> { "Main Hall" };
            Furniture = new List<string>();
            Visitors = new List<string>();
            RoomLevels = new Dictionary<string, int>();
            Statistics = new Dictionary<string, int>();
            DecorationInventory = new List<string>();
            
            Statistics["TotalVisits"] = 0;
            Statistics["TotalGoldDeposited"] = 0;
            Statistics["TotalItemsCrafted"] = 0;
            Statistics["TotalMeetingsHeld"] = 0;
        }
    }
}
