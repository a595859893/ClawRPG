using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    [GlobalClass]
    public partial class MountRacingData : Resource {
        public Dictionary<string, MountRacingRecord> RacingHistory = new Dictionary<string, MountRacingRecord>();
        public List<string> UnlockedTracks = new List<string> {"Meadow Sprint", "Forest Trail"};
        public Dictionary<string, int> BestTimes = new Dictionary<string, int>();
        public Dictionary<string, int> TotalRaces = new Dictionary<string, int>();
        public Dictionary<string, int> TotalWins = new Dictionary<string, int>();
        [Export] public int TotalGoldEarned = 0;
        [Export] public int TotalExpEarned = 0;
        
        public int GetBestTime(string trackId) {
            return BestTimes.ContainsKey(trackId) ? BestTimes[trackId] : -1;
        }
        
        public int GetWinCount(string trackId) {
            return TotalWins.ContainsKey(trackId) ? TotalWins[trackId] : 0;
        }
    }
    
    public class MountRacingRecord {
        public string TrackId;
        public string MountId;
        public int Time;
        public int Rank;
        public DateTime Timestamp;
        public int GoldReward;
        public int ExpReward;
    }
}
