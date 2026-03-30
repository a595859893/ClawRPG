using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Modules.MultiplayerVote
{
    /// <summary>
    /// Party management system - handles party creation, joining, and management
    /// </summary>
    public partial class MultiplayerPartySystem : BaseSystem
    {
        private static MultiplayerPartySystem _instance;
        public static MultiplayerPartySystem Instance => _instance;

        private MultiplayerVoteData _data = new MultiplayerVoteData();
        private MultiplayerVoteDatabase _database = MultiplayerVoteDatabase.Instance;
        
        // Signals for party events
        public delegate void PartyCreatedEventHandler(Party party);
        public delegate void PartyJoinedEventHandler(string partyId, PartyMember member);
        public delegate void PartyLeftEventHandler(string partyId, string playerId);
        public delegate void PartyMemberKickedEventHandler(string partyId, string playerId);
        public delegate void PartyLeaderChangedEventHandler(string partyId, string newLeaderId);

        public override void _Ready()
        {
            _instance = this;
        }
        
        /// <summary>
        /// System name
        /// </summary>
        protected override string SystemName => "MultiplayerParty";

        /// <summary>
        /// Initialize the party system
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            GD.Print("[MultiplayerPartySystem] Initialized");
        }

        #region Party Data Access

        /// <summary>
        /// Get or create player party data
        /// </summary>
        public PlayerPartyData GetOrCreatePlayerPartyData(string playerId)
        {
            if (!_data.PlayerPartyData.ContainsKey(playerId))
            {
                _data.PlayerPartyData[playerId] = new PlayerPartyData();
            }
            return _data.PlayerPartyData[playerId];
        }

        /// <summary>
        /// Get player party data
        /// </summary>
        public PlayerPartyData GetPlayerPartyData(string playerId)
        {
            return _data.PlayerPartyData.GetValueOrDefault(playerId);
        }

        /// <summary>
        /// Get or create player statistics
        /// </summary>
        public PartyStatistics GetOrCreatePlayerStatistics(string playerId)
        {
            if (!_data.PlayerStatistics.ContainsKey(playerId))
            {
                _data.PlayerStatistics[playerId] = new PartyStatistics();
            }
            return _data.PlayerStatistics[playerId];
        }

        /// <summary>
        /// Get active party by ID
        /// </summary>
        public Party GetParty(string partyId)
        {
            return _data.ActiveParties.GetValueOrDefault(partyId);
        }

        /// <summary>
        /// Get all active parties
        /// </summary>
        public Dictionary<string, Party> GetAllParties()
        {
            return _data.ActiveParties;
        }

        /// <summary>
        /// Get player's current party
        /// </summary>
        public Party GetPlayerParty(string playerId)
        {
            var playerData = GetPlayerPartyData(playerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
                return null;
            
            return _data.ActiveParties.GetValueOrDefault(playerData.CurrentPartyId);
        }

        #endregion

        #region Party Management

        /// <summary>
        /// Create a new party
        /// </summary>
        public Party CreateParty(string leaderId, string leaderName, string partyName = "", bool isPublic = true, string password = "", string gameMode = "", int maxMembers = 4)
        {
            if (string.IsNullOrEmpty(partyName))
            {
                partyName = $"{leaderName}'s Party";
            }

            var party = new Party
            {
                PartyName = partyName,
                LeaderId = leaderId,
                IsPublic = isPublic,
                Password = password,
                GameMode = gameMode,
                MaxMembers = maxMembers > 0 ? maxMembers : _database.DefaultMaxMembers,
                CreateTime = OS.GetUnixTime()
            };

            var leaderMember = new PartyMember
            {
                PlayerId = leaderId,
                PlayerName = leaderName,
                IsLeader = true,
                IsReady = true,
                Role = "Leader",
                JoinTime = OS.GetUnixTime()
            };
            party.Members.Add(leaderMember);

            _data.ActiveParties[party.PartyId] = party;

            // Initialize player data
            GetOrCreatePlayerPartyData(leaderId);
            _data.PlayerPartyData[leaderId].CurrentPartyId = party.PartyId;
            _data.PlayerPartyData[leaderId].TotalPartiesCreated++;
            GetOrCreatePlayerStatistics(leaderId).PartiesCreated++;

            EmitSignal(SignalName.PartyCreated, party);
            return party;
        }

        /// <summary>
        /// Join an existing party
        /// </summary>
        public bool JoinParty(string playerId, string playerName, int level, int power, string partyId, string password = "")
        {
            if (!_data.ActiveParties.ContainsKey(partyId))
                return false;

            var party = _data.ActiveParties[partyId];

            // Check restrictions
            if (party.Members.Count >= party.MaxMembers)
                return false;
            
            if (!party.IsPublic && party.Password != password)
                return false;
            
            if (level < party.MinLevel || level > party.MaxLevel)
                return false;

            // Check if already in party
            if (party.Members.Any(m => m.PlayerId == playerId))
                return false;

            var member = new PartyMember
            {
                PlayerId = playerId,
                PlayerName = playerName,
                Level = level,
                Power = power,
                IsLeader = false,
                IsReady = false,
                Role = "Member",
                JoinTime = OS.GetUnixTime()
            };

            party.Members.Add(member);

            // Update player data
            GetOrCreatePlayerPartyData(playerId);
            _data.PlayerPartyData[playerId].CurrentPartyId = partyId;
            _data.PlayerPartyData[playerId].TotalPartiesJoined++;
            GetOrCreatePlayerStatistics(playerId).PartiesJoined++;

            EmitSignal(SignalName.PartyJoined, partyId, member);
            return true;
        }

        /// <summary>
        /// Leave current party
        /// </summary>
        public bool LeaveParty(string playerId)
        {
            var playerData = GetPlayerPartyData(playerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
                return false;

            var partyId = playerData.CurrentPartyId;
            if (!_data.ActiveParties.ContainsKey(partyId))
                return false;

            var party = _data.ActiveParties[partyId];
            var member = party.Members.FirstOrDefault(m => m.PlayerId == playerId);
            
            if (member == null)
                return false;

            party.Members.Remove(member);

            // If leader left, promote new leader
            if (member.IsLeader && party.Members.Count > 0)
            {
                var newLeader = party.Members.First();
                newLeader.IsLeader = true;
                newLeader.Role = "Leader";
                party.LeaderId = newLeader.PlayerId;
                EmitSignal(SignalName.PartyLeaderChanged, partyId, newLeader.PlayerId);
            }

            // If party empty, remove it
            if (party.Members.Count == 0)
            {
                _data.ActiveParties.Remove(partyId);
            }

            playerData.CurrentPartyId = "";
            playerData.PastPartyIds.Add(partyId);

            EmitSignal(SignalName.PartyLeft, partyId, playerId);
            return true;
        }

        /// <summary>
        /// Kick a player from party
        /// </summary>
        public bool KickPlayer(string kickerId, string targetId)
        {
            var kickerData = GetPlayerPartyData(kickerId);
            if (kickerData == null || string.IsNullOrEmpty(kickerData.CurrentPartyId))
                return false;

            var party = _data.ActiveParties[kickerData.CurrentPartyId];
            
            // Only leader can kick
            if (party.LeaderId != kickerId)
                return false;

            var targetMember = party.Members.FirstOrDefault(m => m.PlayerId == targetId);
            if (targetMember == null || targetMember.IsLeader)
                return false;

            party.Members.Remove(targetMember);

            // Update target's party data
            var targetData = GetPlayerPartyData(targetId);
            if (targetData != null)
            {
                targetData.CurrentPartyId = "";
                targetData.PastPartyIds.Add(party.PartyId);
            }

            GetOrCreatePlayerStatistics(targetId).TimesKicked++;
            GetOrCreatePlayerStatistics(kickerId).TimesKickedOthers++;

            EmitSignal(SignalName.PartyMemberKicked, party.PartyId, targetId);
            return true;
        }

        /// <summary>
        /// Promote a party member to leader
        /// </summary>
        public bool PromoteLeader(string oldLeaderId, string newLeaderId)
        {
            var oldLeaderData = GetPlayerPartyData(oldLeaderId);
            if (oldLeaderData == null || string.IsNullOrEmpty(oldLeaderData.CurrentPartyId))
                return false;

            var party = _data.ActiveParties[oldLeaderData.CurrentPartyId];
            
            // Verify the old leader is actually the leader
            if (party.LeaderId != oldLeaderId)
                return false;

            var oldLeaderMember = party.Members.FirstOrDefault(m => m.PlayerId == oldLeaderId);
            var newLeaderMember = party.Members.FirstOrDefault(m => m.PlayerId == newLeaderId);
            
            if (oldLeaderMember == null || newLeaderMember == null)
                return false;

            // Update old leader
            oldLeaderMember.IsLeader = false;
            oldLeaderMember.Role = "Member";
            
            // Update new leader
            newLeaderMember.IsLeader = true;
            newLeaderMember.Role = "Leader";
            
            // Update party
            party.LeaderId = newLeaderId;

            // Update statistics
            GetOrCreatePlayerStatistics(oldLeaderId).TimesDemoted++;
            GetOrCreatePlayerStatistics(newLeaderId).TimesPromoted++;

            EmitSignal(SignalName.PartyLeaderChanged, party.PartyId, newLeaderId);
            return true;
        }

        /// <summary>
        /// Set player ready status
        /// </summary>
        public bool SetReady(string playerId, bool ready)
        {
            var playerData = GetPlayerPartyData(playerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
                return false;

            var party = _data.ActiveParties[playerData.CurrentPartyId];
            var member = party.Members.FirstOrDefault(m => m.PlayerId == playerId);
            
            if (member == null)
                return false;

            member.IsReady = ready;
            return true;
        }

        /// <summary>
        /// Update party settings
        /// </summary>
        public bool UpdatePartySettings(string playerId, bool? isPublic = null, string password = null, string gameMode = null, int? maxMembers = null, int? minLevel = null, int? maxLevel = null)
        {
            var playerData = GetPlayerPartyData(playerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
                return false;

            var party = _data.ActiveParties[playerData.CurrentPartyId];
            
            // Only leader can update settings
            if (party.LeaderId != playerId)
                return false;

            if (isPublic.HasValue) party.IsPublic = isPublic.Value;
            if (password != null) party.Password = password;
            if (gameMode != null) party.GameMode = gameMode;
            if (maxMembers.HasValue) party.MaxMembers = Math.Max(2, Math.Min(maxMembers.Value, 10));
            if (minLevel.HasValue) party.MinLevel = minLevel.Value;
            if (maxLevel.HasValue) party.MaxLevel = maxLevel.Value;

            return true;
        }

        /// <summary>
        /// Get public parties list
        /// </summary>
        public List<Party> GetPublicParties()
        {
            return _data.ActiveParties.Values
                .Where(p => p.IsPublic)
                .OrderByDescending(p => p.CreateTime)
                .ToList();
        }

        /// <summary>
        /// Get party members count
        /// </summary>
        public int GetPartyMemberCount(string partyId)
        {
            if (!_data.ActiveParties.ContainsKey(partyId))
                return 0;
            return _data.ActiveParties[partyId].Members.Count;
        }

        /// <summary>
        /// Check if player is in a party
        /// </summary>
        public bool IsPlayerInParty(string playerId)
        {
            var playerData = GetPlayerPartyData(playerId);
            return playerData != null && !string.IsNullOrEmpty(playerData.CurrentPartyId);
        }

        /// <summary>
        /// Check if player is party leader
        /// </summary>
        public bool IsPlayerPartyLeader(string playerId)
        {
            var party = GetPlayerParty(playerId);
            return party != null && party.LeaderId == playerId;
        }

        #endregion

        #region Save/Load

        /// <summary>
        /// Export save data
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // Export active parties
            var parties = new List<Dictionary>();
            foreach (var party in _data.ActiveParties.Values)
            {
                var partyData = new Dictionary
                {
                    ["PartyId"] = party.PartyId,
                    ["PartyName"] = party.PartyName,
                    ["LeaderId"] = party.LeaderId,
                    ["IsPublic"] = party.IsPublic,
                    ["Password"] = party.Password,
                    ["GameMode"] = party.GameMode,
                    ["MaxMembers"] = party.MaxMembers,
                    ["MinLevel"] = party.MinLevel,
                    ["MaxLevel"] = party.MaxLevel,
                    ["CreateTime"] = party.CreateTime
                };
                
                var members = new List<Dictionary>();
                foreach (var member in party.Members)
                {
                    members.Add(new Dictionary
                    {
                        ["PlayerId"] = member.PlayerId,
                        ["PlayerName"] = member.PlayerName,
                        ["Level"] = member.Level,
                        ["Power"] = member.Power,
                        ["IsLeader"] = member.IsLeader,
                        ["IsReady"] = member.IsReady,
                        ["Role"] = member.Role,
                        ["JoinTime"] = member.JoinTime
                    });
                }
                partyData["Members"] = members;
                parties.Add(partyData);
            }
            data["ActiveParties"] = parties;

            // Export player party data
            var playerData = new List<Dictionary>();
            foreach (var pd in _data.PlayerPartyData.Values)
            {
                playerData.Add(new Dictionary
                {
                    ["PlayerId"] = pd.PlayerId,
                    ["CurrentPartyId"] = pd.CurrentPartyId,
                    ["TotalPartiesCreated"] = pd.TotalPartiesCreated,
                    ["TotalPartiesJoined"] = pd.TotalPartiesJoined,
                    ["PastPartyIds"] = pd.PastPartyIds
                });
            }
            data["PlayerPartyData"] = playerData;

            // Export player statistics
            var statistics = new List<Dictionary>();
            foreach (var stat in _data.PlayerStatistics.Values)
            {
                statistics.Add(new Dictionary
                {
                    ["PlayerId"] = stat.PlayerId,
                    ["PartiesCreated"] = stat.PartiesCreated,
                    ["PartiesJoined"] = stat.PartiesJoined,
                    ["TimesKicked"] = stat.TimesKicked,
                    ["TimesKickedOthers"] = stat.TimesKickedOthers,
                    ["TimesPromoted"] = stat.TimesPromoted,
                    ["TimesDemoted"] = stat.TimesDemoted
                });
            }
            data["PlayerStatistics"] = statistics;

            return data;
        }

        /// <summary>
        /// Import save data
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            _data = new MultiplayerVoteData();

            // Import active parties
            if (data.Contains("ActiveParties") && data["ActiveParties"] is List<object> parties)
            {
                foreach (Dictionary partyDict in parties)
                {
                    var party = new Party
                    {
                        PartyId = partyDict.GetValueOrDefault("PartyId", "").ToString(),
                        PartyName = partyDict.GetValueOrDefault("PartyName", "").ToString(),
                        LeaderId = partyDict.GetValueOrDefault("LeaderId", "").ToString(),
                        IsPublic = Convert.ToBoolean(partyDict.GetValueOrDefault("IsPublic", true)),
                        Password = partyDict.GetValueOrDefault("Password", "").ToString(),
                        GameMode = partyDict.GetValueOrDefault("GameMode", "").ToString(),
                        MaxMembers = Convert.ToInt32(partyDict.GetValueOrDefault("MaxMembers", 4)),
                        MinLevel = Convert.ToInt32(partyDict.GetValueOrDefault("MinLevel", 1)),
                        MaxLevel = Convert.ToInt32(partyDict.GetValueOrDefault("MaxLevel", 100)),
                        CreateTime = Convert.ToInt32(partyDict.GetValueOrDefault("CreateTime", 0))
                    };

                    if (partyDict.Contains("Members") && partyDict["Members"] is List<object> members)
                    {
                        foreach (Dictionary memberDict in members)
                        {
                            party.Members.Add(new PartyMember
                            {
                                PlayerId = memberDict.GetValueOrDefault("PlayerId", "").ToString(),
                                PlayerName = memberDict.GetValueOrDefault("PlayerName", "").ToString(),
                                Level = Convert.ToInt32(memberDict.GetValueOrDefault("Level", 1)),
                                Power = Convert.ToInt32(memberDict.GetValueOrDefault("Power", 0)),
                                IsLeader = Convert.ToBoolean(memberDict.GetValueOrDefault("IsLeader", false)),
                                IsReady = Convert.ToBoolean(memberDict.GetValueOrDefault("IsReady", false)),
                                Role = memberDict.GetValueOrDefault("Role", "Member").ToString(),
                                JoinTime = Convert.ToInt32(memberDict.GetValueOrDefault("JoinTime", 0))
                            });
                        }
                    }

                    _data.ActiveParties[party.PartyId] = party;
                }
            }

            // Import player party data
            if (data.Contains("PlayerPartyData") && data["PlayerPartyData"] is List<object> playerDataList)
            {
                foreach (Dictionary pdDict in playerDataList)
                {
                    var pd = new PlayerPartyData
                    {
                        PlayerId = pdDict.GetValueOrDefault("PlayerId", "").ToString(),
                        CurrentPartyId = pdDict.GetValueOrDefault("CurrentPartyId", "").ToString(),
                        TotalPartiesCreated = Convert.ToInt32(pdDict.GetValueOrDefault("TotalPartiesCreated", 0)),
                        TotalPartiesJoined = Convert.ToInt32(pdDict.GetValueOrDefault("TotalPartiesJoined", 0))
                    };

                    if (pdDict.Contains("PastPartyIds") && pdDict["PastPartyIds"] is List<object> pastIds)
                    {
                        foreach (var id in pastIds)
                        {
                            pd.PastPartyIds.Add(id.ToString());
                        }
                    }

                    _data.PlayerPartyData[pd.PlayerId] = pd;
                }
            }

            // Import player statistics
            if (data.Contains("PlayerStatistics") && data["PlayerStatistics"] is List<object> statList)
            {
                foreach (Dictionary statDict in statList)
                {
                    var stat = new PartyStatistics
                    {
                        PlayerId = statDict.GetValueOrDefault("PlayerId", "").ToString(),
                        PartiesCreated = Convert.ToInt32(statDict.GetValueOrDefault("PartiesCreated", 0)),
                        PartiesJoined = Convert.ToInt32(statDict.GetValueOrDefault("PartiesJoined", 0)),
                        TimesKicked = Convert.ToInt32(statDict.GetValueOrDefault("TimesKicked", 0)),
                        TimesKickedOthers = Convert.ToInt32(statDict.GetValueOrDefault("TimesKickedOthers", 0)),
                        TimesPromoted = Convert.ToInt32(statDict.GetValueOrDefault("TimesPromoted", 0)),
                        TimesDemoted = Convert.ToInt32(statDict.GetValueOrDefault("TimesDemoted", 0))
                    };

                    _data.PlayerStatistics[stat.PlayerId] = stat;
                }
            }

            GD.Print($"[MultiplayerPartySystem] Loaded {_data.ActiveParties.Count} parties");
        }

        #endregion
    }
}
