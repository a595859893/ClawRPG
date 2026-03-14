using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.SealedDungeon {
    public enum SealedDungeonState {
        Locked,
        Available,
        InProgress,
        Completed,
        Failed
    }

    public enum DungeonZone {
        Entrance,
        WhisperingCorridor,
        ForgottenChamber,
        ShadowRealm,
        AncientVault,
        CrystalCavern,
        DragonLair,
        VoidPortal,
        CelestialGarden,
        Eternal throne
    }

    public class SealedDungeonData {
        public int DungeonId { get; set; }
        public string DungeonName { get; set; }
        public DungeonZone CurrentZone { get; set; }
        public SealedDungeonState State { get; set; }
        public int CurrentFloor { get; set; }
        public int MaxFloors { get; set; }
        public int ClearedFloors { get; set; }
        public int BestTime { get; set; }
        public int CurrentScore { get; set; }
        public int BestScore { get; set; }
        public int Attempts { get; set; }
        public int Completions { get; set; }
        public DateTime LastAttemptTime { get; set; }
        public List<int> UnlockedZones { get; set; }
        public List<int> CompletedFloors { get; set; }

        public SealedDungeonData() {
            UnlockedZones = new List<int> { 0 };
            CompletedFloors = new List<int>();
            State = SealedDungeonState.Available;
            CurrentFloor = 1;
            MaxFloors = 10;
            BestTime = int.MaxValue;
            BestScore = 0;
        }
    }

    public class ZoneProgress {
        public DungeonZone Zone { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsCompleted { get; set; }
        public int BestTime { get; set; }
        public int BestScore { get; set; }
        public int ClearCount { get; set; }
    }

    public class DungeonReward {
        public int FloorNumber { get; set; }
        public int GoldReward { get; set; }
        public int ExperienceReward { get; set; }
        public List<string> ItemRewards { get; set; }
        public bool IsBossFloor { get; set; }
    }

    public class SealedDungeonStatistics {
        public int TotalAttempts { get; set; }
        public int TotalCompletions { get; set; }
        public int TotalFloorsCleared { get; set; }
        public int TotalGoldEarned { get; set; }
        public int TotalExperienceEarned { get; set; }
        public int LongestStreak { get; set; }
        public int CurrentStreak { get; set; }
        public int BestScore { get; set; }
        public Dictionary<DungeonZone, int> ZoneClearCount { get; set; }

        public SealedDungeonStatistics() {
            ZoneClearCount = new Dictionary<DungeonZone, int>();
            foreach (DungeonZone zone in Enum.GetValues(typeof(DungeonZone))) {
                ZoneClearCount[zone] = 0;
            }
        }
    }

    public class PlayerSealedDungeonData {
        public List<SealedDungeonData> Dungeons { get; set; }
        public SealedDungeonStatistics Statistics { get; set; }
        public List<DungeonZone> UnlockedZones { get; set; }
        public int HighestZoneUnlocked { get; set; }
        public int TotalStars { get; set; }

        public PlayerSealedDungeonData() {
            Dungeons = new List<SealedDungeonData>();
            Statistics = new SealedDungeonStatistics();
            UnlockedZones = new List<DungeonZone> { DungeonZone.Entrance };
            HighestZoneUnlocked = 0;
            TotalStars = 0;
        }
    }
}
