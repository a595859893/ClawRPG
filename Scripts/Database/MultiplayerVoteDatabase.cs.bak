using System;
using System.Collections.Generic;
using ClawRPG.Systems.MultiplayerVote;

namespace ClawRPG.Modules.MultiplayerVote
{
    /// <summary>
    /// Configuration database for multiplayer vote and party system
    /// </summary>
    public class MultiplayerVoteDatabase
    {
        private static MultiplayerVoteDatabase _instance;
        public static MultiplayerVoteDatabase Instance => _instance ??= new MultiplayerVoteDatabase();

        // Vote configurations
        public Dictionary<VoteType, VoteResults.VoteConfig> VoteConfigs { get; private set; } = new Dictionary<VoteType, VoteResults.VoteConfig>();
        
        // Party settings
        public int DefaultMaxMembers { get; set; } = 4;
        public int MaxPartyNameLength { get; set; } = 20;
        public int PartyInviteTimeout { get; set; } = 60;  // seconds
        public int VoteCooldown { get; set; } = 30;  // seconds between votes
        public int MaxActiveVotes { get; set; } = 3;

        private MultiplayerVoteDatabase()
        {
            InitializeVoteConfigs();
        }

        private void InitializeVoteConfigs()
        {
            // Kick Player
            VoteConfigs[VoteType.KickPlayer] = new VoteResults.VoteConfig
            {
                Type = VoteType.KickPlayer,
                Name = "Kick Player",
                Description = "Vote to remove a player from the party",
                DurationSeconds = 30,
                PassThreshold = 0.6f,
                RequireMajority = true,
                AutoCancelOnLeaver = true
            };

            // Start Game
            VoteConfigs[VoteType.StartGame] = new VoteResults.VoteConfig
            {
                Type = VoteType.StartGame,
                Name = "Start Game",
                Description = "Vote to start the game",
                DurationSeconds = 15,
                PassThreshold = 0.7f,
                RequireMajority = true,
                AutoCancelOnLeaver = false
            };

            // Pause Game
            VoteConfigs[VoteType.PauseGame] = new VoteResults.VoteConfig
            {
                Type = VoteType.PauseGame,
                Name = "Pause Game",
                Description = "Vote to pause the game",
                DurationSeconds = 10,
                PassThreshold = 0.5f,
                RequireMajority = false,
                AutoCancelOnLeaver = false
            };

            // Surrender
            VoteConfigs[VoteType.Surrender] = new VoteResults.VoteConfig
            {
                Type = VoteType.Surrender,
                Name = "Surrender",
                Description = "Vote to surrender the match",
                DurationSeconds = 20,
                PassThreshold = 0.6f,
                RequireMajority = true,
                AutoCancelOnLeaver = false
            };

            // Map Vote
            VoteConfigs[VoteType.MapVote] = new VoteResults.VoteConfig
            {
                Type = VoteType.MapVote,
                Name = "Map Vote",
                Description = "Vote for the next map",
                DurationSeconds = 45,
                PassThreshold = 0.5f,
                RequireMajority = false,
                AutoCancelOnLeaver = false
            };

            // Difficulty Vote
            VoteConfigs[VoteType.DifficultyVote] = new VoteResults.VoteConfig
            {
                Type = VoteType.DifficultyVote,
                Name = "Difficulty Vote",
                Description = "Vote for the difficulty level",
                DurationSeconds = 30,
                PassThreshold = 0.5f,
                RequireMajority = false,
                AutoCancelOnLeaver = false
            };

            // Ready Check
            VoteConfigs[VoteType.ReadyCheck] = new VoteResults.VoteConfig
            {
                Type = VoteType.ReadyCheck,
                Name = "Ready Check",
                Description = "Check if all players are ready",
                DurationSeconds = 15,
                PassThreshold = 1.0f,  // 100% needed
                RequireMajority = true,
                AutoCancelOnLeaver = true
            };

            // Invite Player
            VoteConfigs[VoteType.InvitePlayer] = new VoteResults.VoteConfig
            {
                Type = VoteType.InvitePlayer,
                Name = "Invite Player",
                Description = "Vote to invite a new player",
                DurationSeconds = 20,
                PassThreshold = 0.5f,
                RequireMajority = false,
                AutoCancelOnLeaver = false
            };

            // Promote Leader
            VoteConfigs[VoteType.PromoteLeader] = new VoteResults.VoteConfig
            {
                Type = VoteType.PromoteLeader,
                Name = "Promote Leader",
                Description = "Vote to promote a new party leader",
                DurationSeconds = 25,
                PassThreshold = 0.7f,
                RequireMajority = true,
                AutoCancelOnLeaver = true
            };

            // Cancel Match
            VoteConfigs[VoteType.CancelMatch] = new VoteResults.VoteConfig
            {
                Type = VoteType.CancelMatch,
                Name = "Cancel Match",
                Description = "Vote to cancel the current match",
                DurationSeconds = 20,
                PassThreshold = 0.6f,
                RequireMajority = true,
                AutoCancelOnLeaver = false
            };
        }

        public VoteResults.VoteConfig GetVoteConfig(VoteType type)
        {
            return VoteConfigs.ContainsKey(type) ? VoteConfigs[type] : null;
        }

        public int GetDefaultVoteDuration(VoteType type)
        {
            var config = GetVoteConfig(type);
            return config?.DurationSeconds ?? 30;
        }

        public float GetDefaultPassThreshold(VoteType type)
        {
            var config = GetVoteConfig(type);
            return config?.PassThreshold ?? 0.5f;
        }
    }
}
