using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Systems.MultiplayerVote
{
    /// <summary>
    /// 多人投票系统主控制器 - 负责协调投票和队伍管理
    /// </summary>
    public partial class MultiplayerVoteSystem : BaseSystem
    {
        private static MultiplayerVoteSystem _instance;
        public static MultiplayerVoteSystem Instance => _instance;

        // 子系统引用
        private VoteTimer _voteTimer;
        private VoteResults _voteResults;
        
        // 数据存储
        private Dictionary<string, ActiveVote> _activeVotes = new Dictionary<string, ActiveVote>();
        private Dictionary<string, Party> _activeParties = new Dictionary<string, Party>();
        private Dictionary<string, PlayerPartyData> _playerPartyData = new Dictionary<string, PlayerPartyData>();
        private Dictionary<string, PartyStatistics> _playerStatistics = new Dictionary<string, PartyStatistics>();
        
        // Signals
        [Signal] public delegate void VoteStartedEventHandler(ActiveVote vote);
        [Signal] public delegate void VoteEndedEventHandler(ActiveVote vote, bool passed);
        [Signal] public delegate void VoteUpdatedEventHandler(ActiveVote vote);
        [Signal] public delegate void PartyCreatedEventHandler(Party party);
        [Signal] public delegate void PartyJoinedEventHandler(string partyId, PartyMember member);
        [Signal] public delegate void PartyLeftEventHandler(string partyId, string playerId);
        [Signal] public delegate void PartyMemberKickedEventHandler(string partyId, string playerId);
        [Signal] public delegate void PartyLeaderChangedEventHandler(string partyId, string newLeaderId);

        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            
            // 初始化子系统
            _voteTimer = new VoteTimer();
            _voteResults = new VoteResults();
        }
        
        protected override string SystemName => "MultiplayerVote";

        #region Party Management

        /// <summary>
        /// 创建队伍
        /// </summary>
        public Party CreateParty(string leaderId, string leaderName, string partyName = "", bool isPublic = true, string password = "", string gameMode = "", int maxMembers = 4)
        {
            if (string.IsNullOrEmpty(partyName))
            {
                partyName = $"{leaderName}'s Party";
            }

            var party = new Party
            {
                PartyId = Guid.NewGuid().ToString(),
                PartyName = partyName,
                LeaderId = leaderId,
                IsPublic = isPublic,
                Password = password,
                GameMode = gameMode,
                MaxMembers = maxMembers > 0 ? maxMembers : 4,
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

            _activeParties[party.PartyId] = party;

            GetOrCreatePlayerPartyData(leaderId);
            _playerPartyData[leaderId].CurrentPartyId = party.PartyId;
            _playerPartyData[leaderId].TotalPartiesCreated++;
            GetOrCreatePlayerStatistics(leaderId).PartiesCreated++;

            EmitSignal(SignalName.PartyCreated, party);
            return party;
        }

        /// <summary>
        /// 加入队伍
        /// </summary>
        public bool JoinParty(string playerId, string playerName, int level, int power, string partyId, string password = "")
        {
            if (!_activeParties.ContainsKey(partyId))
                return false;

            var party = _activeParties[partyId];

            if (party.Members.Count >= party.MaxMembers)
                return false;
            
            if (!party.IsPublic && party.Password != password)
                return false;
            
            if (level < party.MinLevel || level > party.MaxLevel)
                return false;

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

            GetOrCreatePlayerPartyData(playerId);
            _playerPartyData[playerId].CurrentPartyId = partyId;
            _playerPartyData[playerId].TotalPartiesJoined++;
            GetOrCreatePlayerStatistics(playerId).PartiesJoined++;

            EmitSignal(SignalName.PartyJoined, partyId, member);
            return true;
        }

        /// <summary>
        /// 离开队伍
        /// </summary>
        public bool LeaveParty(string playerId)
        {
            var playerData = GetPlayerPartyData(playerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
                return false;

            var partyId = playerData.CurrentPartyId;
            if (!_activeParties.ContainsKey(partyId))
                return false;

            var party = _activeParties[partyId];
            var member = party.Members.FirstOrDefault(m => m.PlayerId == playerId);
            
            if (member == null)
                return false;

            party.Members.Remove(member);

            if (member.IsLeader && party.Members.Count > 0)
            {
                var newLeader = party.Members.First();
                newLeader.IsLeader = true;
                newLeader.Role = "Leader";
                party.LeaderId = newLeader.PlayerId;
                EmitSignal(SignalName.PartyLeaderChanged, partyId, newLeader.PlayerId);
            }

            if (party.Members.Count == 0)
            {
                _activeParties.Remove(partyId);
            }

            playerData.CurrentPartyId = "";
            playerData.PastPartyIds.Add(partyId);

            EmitSignal(SignalName.PartyLeft, partyId, playerId);
            return true;
        }

        /// <summary>
        /// 踢出玩家
        /// </summary>
        public bool KickPlayer(string kickerId, string targetId)
        {
            var kickerData = GetPlayerPartyData(kickerId);
            if (kickerData == null || string.IsNullOrEmpty(kickerData.CurrentPartyId))
                return false;

            var party = _activeParties[kickerData.CurrentPartyId];
            
            if (party.LeaderId != kickerId)
                return false;

            var targetMember = party.Members.FirstOrDefault(m => m.PlayerId == targetId);
            if (targetMember == null || targetMember.IsLeader)
                return false;

            party.Members.Remove(targetMember);

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
        /// 设置准备状态
        /// </summary>
        public bool SetReady(string playerId, bool ready)
        {
            var playerData = GetPlayerPartyData(playerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
                return false;

            var party = _activeParties[playerData.CurrentPartyId];
            var member = party.Members.FirstOrDefault(m => m.PlayerId == playerId);
            
            if (member == null)
                return false;

            member.IsReady = ready;
            return true;
        }

        /// <summary>
        /// 更新队伍设置
        /// </summary>
        public bool UpdatePartySettings(string playerId, bool? isPublic = null, string password = null, string gameMode = null, int? maxMembers = null, int? minLevel = null, int? maxLevel = null)
        {
            var playerData = GetPlayerPartyData(playerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
                return false;

            var party = _activeParties[playerData.CurrentPartyId];
            
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

        #endregion

        #region Vote System

        /// <summary>
        /// 发起投票
        /// </summary>
        public ActiveVote InitiateVote(string initiatorId, VoteResults.VoteType voteType, string targetId = "", string targetName = "", string reason = "")
        {
            var initiatorData = GetPlayerPartyData(initiatorId);
            if (initiatorData == null || string.IsNullOrEmpty(initiatorData.CurrentPartyId))
                return null;

            var partyId = initiatorData.CurrentPartyId;
            if (!_activeParties.ContainsKey(partyId))
                return null;

            var party = _activeParties[partyId];

            var activePartyVotes = _activeVotes.Values.Where(v => GetPartyIdByVote(v.VoteId) == partyId && v.Status == VoteResults.VoteStatus.Pending).ToList();
            if (activePartyVotes.Count >= 5)
                return null;

            var config = _voteResults.GetVoteConfig(voteType.ToString());
            if (config == null)
                return null;

            var initiatorMember = party.Members.FirstOrDefault(m => m.PlayerId == initiatorId);
            if (initiatorMember == null)
                return null;

            int currentTime = OS.GetUnixTime();
            var vote = new ActiveVote
            {
                VoteId = Guid.NewGuid().ToString(),
                Type = voteType,
                InitiatorId = initiatorId,
                InitiatorName = initiatorMember.PlayerName,
                TargetId = targetId,
                TargetName = targetName,
                Reason = reason,
                StartTime = currentTime,
                EndTime = currentTime + config.DurationSeconds,
                Status = VoteResults.VoteStatus.Pending
            };

            vote.Votes.Add(new VoteResults.VoteRecord
            {
                PlayerId = initiatorId,
                PlayerName = initiatorMember.PlayerName,
                VotedYes = true,
                VoteTime = currentTime
            });

            _activeVotes[vote.VoteId] = vote;
            _voteTimer.SetVoteTimeout(vote.VoteId, config.DurationSeconds);

            GetOrCreatePlayerStatistics(initiatorId).VotesInitiated++;

            EmitSignal(SignalName.VoteStarted, vote);
            return vote;
        }

        /// <summary>
        /// 投票
        /// </summary>
        public bool CastVote(string voterId, string voteId, bool yes)
        {
            if (!_activeVotes.ContainsKey(voteId))
                return false;

            var vote = _activeVotes[voteId];
            if (vote.Status != VoteResults.VoteStatus.Pending)
                return false;

            if (_voteTimer.IsVoteExpired(voteId))
            {
                EndVote(voteId);
                return false;
            }

            var voterData = GetPlayerPartyData(voterId);
            if (voterData == null || string.IsNullOrEmpty(voterData.CurrentPartyId))
                return false;

            var partyId = voterData.CurrentPartyId;
            if (GetPartyIdByVote(voteId) != partyId)
                return false;

            var party = _activeParties[partyId];
            var member = party.Members.FirstOrDefault(m => m.PlayerId == voterId);
            if (member == null)
                return false;

            _voteResults.AddVoteRecord(voteId, voterId, member.PlayerName, yes);

            GetOrCreatePlayerStatistics(voterId).VotesCast++;

            EmitSignal(SignalName.VoteUpdated, vote);
            
            CheckVoteCompletion(voteId, party.Members.Count);
            
            return true;
        }

        /// <summary>
        /// 取消投票
        /// </summary>
        public bool CancelVote(string voteId, string cancellerId)
        {
            if (!_activeVotes.ContainsKey(voteId))
                return false;

            var vote = _activeVotes[voteId];
            if (vote.Status != VoteResults.VoteStatus.Pending)
                return false;

            var voterData = GetPlayerPartyData(cancellerId);
            if (voterData == null || string.IsNullOrEmpty(voterData.CurrentPartyId))
                return false;

            var party = _activeParties[voterData.CurrentPartyId];
            if (vote.InitiatorId != cancellerId && party.LeaderId != cancellerId)
                return false;

            vote.Status = VoteResults.VoteStatus.Cancelled;
            _voteTimer.SetVoteStatus(voteId, VoteResults.VoteStatus.Cancelled);
            EmitSignal(SignalName.VoteEnded, vote, false);
            return true;
        }

        /// <summary>
        /// 结束投票
        /// </summary>
        private void EndVote(string voteId)
        {
            if (!_activeVotes.ContainsKey(voteId))
                return;

            var vote = _activeVotes[voteId];
            if (vote.Status != VoteResults.VoteStatus.Pending)
                return;

            var partyId = GetPartyIdByVote(voteId);
            if (partyId == null || !_activeParties.ContainsKey(partyId))
                return;

            var party = _activeParties[partyId];
            bool passed = _voteResults.CalculateVoteResult(voteId, vote.Type.ToString(), party.Members.Count);

            vote.Status = passed ? VoteResults.VoteStatus.Passed : VoteResults.VoteStatus.Failed;
            _voteTimer.SetVoteStatus(voteId, vote.Status);

            if (vote.InitiatorId != "")
            {
                var stats = GetOrCreatePlayerStatistics(vote.InitiatorId);
                if (passed)
                    stats.VotesPassed++;
                else
                    stats.VotesFailed++;
            }

            if (passed)
            {
                ExecuteVoteEffects(vote);
            }

            EmitSignal(SignalName.VoteEnded, vote, passed);
        }

        /// <summary>
        /// 检查投票是否完成
        /// </summary>
        private void CheckVoteCompletion(string voteId, int totalPlayers)
        {
            if (!_activeVotes.ContainsKey(voteId))
                return;

            var vote = _activeVotes[voteId];
            var config = _voteResults.GetVoteConfig(vote.Type.ToString());

            if (_voteResults.AllPlayersVoted(voteId, totalPlayers))
            {
                EndVote(voteId);
                return;
            }

            if (config != null && config.PassThreshold == 1.0f)
            {
                var (yesVotes, noVotes) = _voteResults.GetVoteStats(voteId);
                int remaining = totalPlayers - yesVotes - noVotes;
                if (noVotes > 0 || remaining > 0)
                {
                    return;
                }
                EndVote(voteId);
            }
        }

        /// <summary>
        /// 执行投票效果
        /// </summary>
        private void ExecuteVoteEffects(ActiveVote vote)
        {
            var partyId = GetPartyIdByVote(vote.VoteId);
            if (partyId == null || !_activeParties.ContainsKey(partyId))
                return;

            var party = _activeParties[partyId];

            switch (vote.Type)
            {
                case VoteResults.VoteType.KickPlayer:
                    KickPlayer(party.LeaderId, vote.TargetId);
                    break;
                    
                case VoteResults.VoteType.PromoteLeader:
                    var newLeader = party.Members.FirstOrDefault(m => m.PlayerId == vote.TargetId);
                    if (newLeader != null)
                    {
                        var oldLeader = party.Members.FirstOrDefault(m => m.IsLeader);
                        if (oldLeader != null)
                        {
                            oldLeader.IsLeader = false;
                            oldLeader.Role = "Member";
                        }
                        newLeader.IsLeader = true;
                        newLeader.Role = "Leader";
                        party.LeaderId = newLeader.PlayerId;
                        EmitSignal(SignalName.PartyLeaderChanged, partyId, newLeader.PlayerId);
                    }
                    break;
            }
        }

        #endregion

        #region Query Methods

        private string GetPartyIdByVote(string voteId)
        {
            foreach (var party in _activeParties.Values)
            {
                foreach (var member in party.Members)
                {
                    // 查找发起者所在的队伍
                }
            }
            
            var vote = _activeVotes.ContainsKey(voteId) ? _activeVotes[voteId] : null;
            if (vote != null)
            {
                var initiatorData = GetPlayerPartyData(vote.InitiatorId);
                if (initiatorData != null)
                    return initiatorData.CurrentPartyId;
            }
            return null;
        }

        public Party GetParty(string partyId)
        {
            return _activeParties.ContainsKey(partyId) ? _activeParties[partyId] : null;
        }

        public Party GetPlayerParty(string playerId)
        {
            var playerData = GetPlayerPartyData(playerId);
            if (playerData == null || string.IsNullOrEmpty(playerData.CurrentPartyId))
                return null;

            return _activeParties.ContainsKey(playerData.CurrentPartyId) 
                ? _activeParties[playerData.CurrentPartyId] 
                : null;
        }

        public ActiveVote GetVote(string voteId)
        {
            return _activeVotes.ContainsKey(voteId) ? _activeVotes[voteId] : null;
        }

        public List<Party> GetPublicParties()
        {
            return _activeParties.Values
                .Where(p => p.IsPublic && p.Members.Count < p.MaxMembers)
                .OrderByDescending(p => p.Members.Count)
                .ToList();
        }

        public List<ActiveVote> GetPartyVotes(string partyId)
        {
            var result = new List<ActiveVote>();
            foreach (var vote in _activeVotes.Values)
            {
                if (vote.Status == VoteResults.VoteStatus.Pending)
                {
                    var initiatorData = GetPlayerPartyData(vote.InitiatorId);
                    if (initiatorData != null && initiatorData.CurrentPartyId == partyId)
                    {
                        result.Add(vote);
                    }
                }
            }
            return result;
        }

        public PlayerPartyData GetPlayerPartyData(string playerId)
        {
            return _playerPartyData.ContainsKey(playerId) ? _playerPartyData[playerId] : null;
        }

        public PartyStatistics GetPlayerStatistics(string playerId)
        {
            return _playerStatistics.ContainsKey(playerId) ? _playerStatistics[playerId] : null;
        }

        private PlayerPartyData GetOrCreatePlayerPartyData(string playerId)
        {
            if (!_playerPartyData.ContainsKey(playerId))
            {
                _playerPartyData[playerId] = new PlayerPartyData { PlayerId = playerId };
            }
            return _playerPartyData[playerId];
        }

        private PartyStatistics GetOrCreatePlayerStatistics(string playerId)
        {
            if (!_playerStatistics.ContainsKey(playerId))
            {
                _playerStatistics[playerId] = new PartyStatistics();
            }
            return _playerStatistics[playerId];
        }

        #endregion

        #region Update Loop

        public override void _Process(double delta)
        {
            // 检查过期投票
            var expiredVotes = _voteTimer.GetExpiredVotes();
            foreach (var voteId in expiredVotes)
            {
                EndVote(voteId);
            }
        }

        #endregion

        #region Save/Load

        public override Dictionary ExportSaveData()
        {
            var saveData = new Dictionary();
            
            // 导出队伍
            var partiesData = new List<Dictionary>();
            foreach (var party in _activeParties.Values)
            {
                partiesData.Add(new Dictionary
                {
                    { "party_id", party.PartyId },
                    { "party_name", party.PartyName },
                    { "leader_id", party.LeaderId },
                    { "is_public", party.IsPublic },
                    { "password", party.Password },
                    { "game_mode", party.GameMode },
                    { "max_members", party.MaxMembers },
                    { "min_level", party.MinLevel },
                    { "max_level", party.MaxLevel },
                    { "create_time", party.CreateTime }
                });
            }
            saveData["parties"] = partiesData;

            return saveData;
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
        }

        #endregion
    }
    
    #region Data Classes
    
    /// <summary>
    /// 活跃投票
    /// </summary>
    public class ActiveVote
    {
        public string VoteId { get; set; }
        public VoteResults.VoteType Type { get; set; }
        public string InitiatorId { get; set; }
        public string InitiatorName { get; set; }
        public string TargetId { get; set; }
        public string TargetName { get; set; }
        public string Reason { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public VoteResults.VoteStatus Status { get; set; }
        public List<VoteResults.VoteRecord> Votes { get; set; } = new List<VoteResults.VoteRecord>();
    }
    
    /// <summary>
    /// 队伍
    /// </summary>
    public class Party
    {
        public string PartyId { get; set; }
        public string PartyName { get; set; }
        public string LeaderId { get; set; }
        public bool IsPublic { get; set; }
        public string Password { get; set; }
        public string GameMode { get; set; }
        public int MaxMembers { get; set; } = 4;
        public int MinLevel { get; set; } = 1;
        public int MaxLevel { get; set; } = 100;
        public int CreateTime { get; set; }
        public List<PartyMember> Members { get; set; } = new List<PartyMember>();
    }
    
    /// <summary>
    /// 队伍成员
    /// </summary>
    public class PartyMember
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int Level { get; set; }
        public int Power { get; set; }
        public bool IsLeader { get; set; }
        public bool IsReady { get; set; }
        public string Role { get; set; }
        public int JoinTime { get; set; }
    }
    
    /// <summary>
    /// 玩家队伍数据
    /// </summary>
    public class PlayerPartyData
    {
        public string PlayerId { get; set; }
        public string CurrentPartyId { get; set; }
        public List<string> PendingInvites { get; set; } = new List<string>();
        public List<string> PastPartyIds { get; set; } = new List<string>();
        public int TotalPartiesJoined { get; set; }
        public int TotalPartiesCreated { get; set; }
        public int VotesCast { get; set; }
        public int VotesInitiated { get; set; }
    }
    
    /// <summary>
    /// 队伍统计
    /// </summary>
    public class PartyStatistics
    {
        public int TotalVotes { get; set; }
        public int VotesPassed { get; set; }
        public int VotesFailed { get; set; }
        public int PartiesCreated { get; set; }
        public int PartiesJoined { get; set; }
        public int TimesKicked { get; set; }
        public int TimesKickedOthers { get; set; }
        public int VotesInitiated { get; set; }
    }
    
    #endregion
}
