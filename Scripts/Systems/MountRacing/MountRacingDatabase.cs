using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    [GlobalClass]
    public partial class MountRacingDatabase : Resource {
        [Export] public Dictionary<string, TrackConfig> Tracks = new Dictionary<string, TrackConfig> {
            ["Meadow Sprint"] = new TrackConfig {
                Name = "Meadow Sprint",
                Description = "A gentle race through blooming meadows",
                Length = 1000,
                Difficulty = TrackDifficulty.Easy,
                BaseReward = 50,
                MinPlayers = 1,
                MaxPlayers = 4,
                Obstacles = new string[] {"Boulder", "Tree Root", "Bush"},
                Terrain = "Grass",
                WeatherTypes = new string[] {"Clear", "Cloudy"}
            },
            ["Forest Trail"] = new TrackConfig {
                Name = "Forest Trail",
                Description = "Winding path through ancient forest",
                Length = 1500,
                Difficulty = TrackDifficulty.Normal,
                BaseReward = 75,
                MinPlayers = 1,
                MaxPlayers = 4,
                Obstacles = new string[] {"Log", "Stream", "Branch"},
                Terrain = "Dirt",
                WeatherTypes = new string[] {"Clear", "Rain", "Fog"}
            },
            ["Mountain Pass"] = new TrackConfig {
                Name = "Mountain Pass",
                Description = "Treacherous climb through rocky mountains",
                Length = 2000,
                Difficulty = TrackDifficulty.Hard,
                BaseReward = 100,
                MinPlayers = 2,
                MaxPlayers = 6,
                Obstacles = new string[] {"Cliff", "Gap", "Rockfall"},
                Terrain = "Rock",
                WeatherTypes = new string[] {"Clear", "Snow", "Wind"}
            },
            ["Desert Dunes"] = new TrackConfig {
                Name = "Desert Dunes",
                Description = "Scorching race across endless sand dunes",
                Length = 1800,
                Difficulty = TrackDifficulty.Hard,
                BaseReward = 100,
                MinPlayers = 2,
                MaxPlayers = 6,
                Obstacles = new string[] {"Sandstorm", "Quicksand", "Cactus"},
                Terrain = "Sand",
                WeatherTypes = new string[] {"Clear", "Sandstorm", "Heat"}
            },
            ["Volcanic Valley"] = new TrackConfig {
                Name = "Volcanic Valley",
                Description = "Dangerous course through active volcanic region",
                Length = 2200,
                Difficulty = TrackDifficulty.Nightmare,
                BaseReward = 150,
                MinPlayers = 3,
                MaxPlayers = 8,
                Obstacles = new string[] {"Lava Pool", "Falling Rock", "Steam Vent"},
                Terrain = "Lava",
                WeatherTypes = new string[] {"Clear", "Smoke", "Ash"}
            },
            ["Frost Peak"] = new TrackConfig {
                Name = "Frost Peak",
                Description = "Icy race to the frozen mountain summit",
                Length = 2000,
                Difficulty = TrackDifficulty.Nightmare,
                BaseReward = 150,
                MinPlayers = 3,
                MaxPlayers = 8,
                Obstacles = new string[] {"Ice Crack", "Avalanche", "Frozen River"},
                Terrain = "Ice",
                WeatherTypes = new string[] {"Clear", "Blizzard", "Snow"}
            },
            ["Ocean Shore"] = new TrackConfig {
                Name = "Ocean Shore",
                Description = "Coastal race along crashing waves",
                Length = 1600,
                Difficulty = TrackDifficulty.Normal,
                BaseReward = 80,
                MinPlayers = 2,
                MaxPlayers = 6,
                Obstacles = new string[] {"Wave", "Rock", "Tide Pool"},
                Terrain = "Sand",
                WeatherTypes = new string[] {"Clear", "Rain", "Storm"}
            },
            ["Sky Temple"] = new TrackConfig {
                Name = "Sky Temple",
                Description = "Floating platforms in the clouds",
                Length = 2500,
                Difficulty = TrackDifficulty.Legendary,
                BaseReward = 200,
                MinPlayers = 4,
                MaxPlayers = 8,
                Obstacles = new string[] {"Cloud Burst", "Lightning", "Void Gap"},
                Terrain = "Cloud",
                WeatherTypes = new string[] {"Clear", "Thunder", "Mist"}
            }
        };
        
        [Export] public Dictionary<TrackDifficulty, DifficultyConfig> DifficultySettings = new Dictionary<TrackDifficulty, DifficultyConfig> {
            [TrackDifficulty.Easy] = new DifficultyConfig { SpeedMod = 1.2f, ObstacleChance = 0.3f, TimeLimit = 180 },
            [TrackDifficulty.Normal] = new DifficultyConfig { SpeedMod = 1.0f, ObstacleChance = 0.5f, TimeLimit = 150 },
            [TrackDifficulty.Hard] = new DifficultyConfig { SpeedMod = 0.85f, ObstacleChance = 0.7f, TimeLimit = 120 },
            [TrackDifficulty.Nightmare] = new DifficultyConfig { SpeedMod = 0.7f, ObstacleChance = 0.85f, TimeLimit = 100 },
            [TrackDifficulty.Legendary] = new DifficultyConfig { SpeedMod = 0.5f, ObstacleChance = 1.0f, TimeLimit = 80 }
        };
        
        [Export] public Dictionary<int, RankReward> RankRewards = new Dictionary<int, RankReward> {
            [1] = new RankReward { GoldMultiplier = 2.0f, ExpMultiplier = 2.0f, Title = "Champion" },
            [2] = new RankReward { GoldMultiplier = 1.5f, ExpMultiplier = 1.5f, Title = "Silver" },
            [3] = new RankReward { GoldMultiplier = 1.2f, ExpMultiplier = 1.2f, Title = "Bronze" },
            [4] = new RankReward { GoldMultiplier = 1.0f, ExpMultiplier = 1.0f, Title = "Participant" },
            [5] = new RankReward { GoldMultiplier = 0.8f, ExpMultiplier = 0.8f, Title = "Participant" },
            [6] = new RankReward { GoldMultiplier = 0.6f, ExpMultiplier = 0.6f, Title = "Participant" },
            [7] = new RankReward { GoldMultiplier = 0.5f, ExpMultiplier = 0.5f, Title = "Participant" },
            [8] = new RankReward { GoldMultiplier = 0.5f, ExpMultiplier = 0.5f, Title = "Participant" }
        };
    }
    
    public enum TrackDifficulty { Easy, Normal, Hard, Nightmare, Legendary }
    
    public class TrackConfig {
        public string Name;
        public string Description;
        public int Length;
        public TrackDifficulty Difficulty;
        public int BaseReward;
        public int MinPlayers;
        public int MaxPlayers;
        public string[] Obstacles;
        public string Terrain;
        public string[] WeatherTypes;
    }
    
    public class DifficultyConfig {
        public float SpeedMod;
        public float ObstacleChance;
        public int TimeLimit;
    }
    
    public class RankReward {
        public float GoldMultiplier;
        public float ExpMultiplier;
        public string Title;
    }
}
