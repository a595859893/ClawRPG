using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Systems.MultiplayerVote
{
    /// <summary>
    /// 投票处理系统 - 负责投票的发起、投票、结束和效果执行
    /// 继承 BaseSystem 实现数据持久化
    /// </summary>
    public partial class VoteProcessingSystem : BaseSystem
    {
        private static VoteProcessingSystem _instance;
        public static VoteProcessingSystem Instance => _instance;
        
        // 外部依赖
        private VoteTimer _voteTimer;
        private VoteResults _voteResults;
        
        // 内部数据存储
        private Dictionary<string, ActiveVote> _activeVotes = new Dictionary<string, ActiveVote>();
        
        // 回调函数用于访问 PartyManagementSystem
        private Func<string, PlayerPartyData> _getPlayerPartyDataFunc;
        private Func<string, Party> _getPlayerPartyFunc;
        private Func<string, Dictionary<string, Party>> _getAllPartiesFunc;
        private Action<string, string> _onKickPlayerAction;
        private Action<string, string, string> _onLeaderChangedAction;
        private Func<string, PartyStatistics> _getOrCreateStatisticsFunc;
        
        // Signals - 转发到主系统
        public delegate void VoteStartedEventHandler(ActiveVote vote);
        public delegate void VoteEndedEventHandler(ActiveVote vote, bool passed);
        public delegate void VoteUpdatedEventHandler(ActiveVote vote);
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
        }
        
        protected override string SystemName => "VoteProcessing";
        
        /// <summary>
        /// 初始化依赖
        /// </summary>
        public void Initialize(VoteTimer voteTimer, VoteResults voteResults)
        {
            _voteTimer = voteTimer;
            _voteResults = voteResults;
        }
        
        /// <summary>
        /// 设置回调函数
        /// </summary>
        public void SetCallbacks(
            Func<string, PlayerPartyData> getPlayerPartyData,
            Func<string, Party> getPlayerParty,
            Func<string, Dictionary<string, Party>> getAllParties,
            Action<string, string> onKickPlayer,
            Action<string, string, string> onLeaderChanged,
            Func<string, PartyStatistics> getOrCreateStatistics)
        {
            _getPlayerPartyDataFunc = getPlayerPartyData;
            _getPlayerPartyFunc = getPlayerParty;
            _getAllPartiesFunc = getAllParties;
            _onKickPlayerAction = onKickPlayer;
            _onLeaderChangedAction = onLeaderChanged;
            _getOrCreateStatisticsFunc = getOrCreateStatistics;
        }

        #region Vote System

        /// <summary>
        /// 发起投票
        /// </summary>
        public ActiveVote InitiateVote(string initiatorId, VoteResults.VoteType voteType, string targetId = "", string targetName = "", string reason = "")
        {
            var initiatorData = _getPlayerPartyDataFunc?.Invoke(initiatorId);
            if (initiatorData == null || string.IsNullOrEmpty(initiatorData.CurrentPartyId))
                return null;

            var partyId = initiatorData.CurrentPartyId;
            var allParties = _getAllPartiesFunc?.Invoke();
            if (allParties == null || !allParties.ContainsKey(partyId))
                return null;

            var party = allParties[partyId];

            // 检查是否有太多活跃投票
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
            _voteTimer?.SetVoteTimeout(vote.VoteId, config.DurationSeconds);

            _getOrCreateStatisticsFunc?.Invoke(initiatorId)?.VotesInitiated++;

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

            if (_voteTimer?.IsVoteExpired(voteId) == true)
            {
                EndVote(voteId);
                return false;
            }

            var voterData = _getPlayerPartyDataFunc?.Invoke(voterId);
            if (voterData == null || string.IsNullOrEmpty(voterData.CurrentPartyId))
                return false;

            var partyId = voterData.CurrentPartyId;
            if (GetPartyIdByVote(voteId) != partyId)
                return false;

            var allParties = _getAllPartiesFunc?.Invoke();
            if (allParties == null || !allParties.ContainsKey(partyId))
                return false;

            var party = allParties[partyId];
            var member = party.Members.FirstOrDefault(m => m.PlayerId == voterId);
            if (member == null)
                return false;

            _voteResults.AddVoteRecord(voteId, voterId, member.PlayerName, yes);

            var stats = _getOrCreateStatisticsFunc?.Invoke(voterId);
            if (stats != null)
                stats.VotesCast++;

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

            var voterData = _getPlayerPartyDataFunc?.Invoke(cancellerId);
            if (voterData == null || string.IsNullOrEmpty(voterData.CurrentPartyId))
                return false;

            var allParties = _getAllPartiesFunc?.Invoke();
            if (allParties == null || !allParties.ContainsKey(voterData.CurrentPartyId))
                return false;

            var party = allParties[voterData.CurrentPartyId];
            if (vote.InitiatorId != cancellerId && party.LeaderId != cancellerId)
                return false;

            vote.Status = VoteResults.VoteStatus.Cancelled;
            _voteTimer?.SetVoteStatus(voteId, VoteTimer.VoteStatus.Cancelled);
            EmitSignal(SignalName.VoteEnded, vote, false);
            return true;
        }

        /// <summary>
        /// 结束投票
        /// </summary>
        public void EndVote(string voteId)
        {
            if (!_activeVotes.ContainsKey(voteId))
                return;

            var vote = _activeVotes[voteId];
            if (vote.Status != VoteResults.VoteStatus.Pending)
                return;

            var partyId = GetPartyIdByVote(voteId);
            if (partyId == null)
                return;

            var allParties = _getAllPartiesFunc?.Invoke();
            if (allParties == null || !allParties.ContainsKey(partyId))
                return;

            var party = allParties[partyId];
            bool passed = _voteResults.CalculateVoteResult(voteId, vote.Type.ToString(), party.Members.Count);

            vote.Status = passed ? VoteResults.VoteStatus.Passed : VoteResults.VoteStatus.Failed;
            _voteTimer?.SetVoteStatus(voteId, passed ? VoteTimer.VoteStatus.Passed : VoteTimer.VoteStatus.Failed);

            if (vote.InitiatorId != "")
            {
                var stats = _getOrCreateStatisticsFunc?.Invoke(vote.InitiatorId);
                if (stats != null)
                {
                    if (passed)
                        stats.VotesPassed++;
                    else
                        stats.VotesFailed++;
                }
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
            if (partyId == null)
                return;

            var allParties = _getAllPartiesFunc?.Invoke();
            if (allParties == null || !allParties.ContainsKey(partyId))
                return;

            var party = allParties[partyId];

            switch (vote.Type)
            {
                case VoteResults.VoteType.KickPlayer:
                    _onKickPlayerAction?.Invoke(party.LeaderId, vote.TargetId);
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
                        _onLeaderChangedAction?.Invoke(partyId, newLeader.PlayerId);
                    }
                    break;
            }
        }

        #endregion

        #region Query Methods

        private string GetPartyIdByVote(string voteId)
        {
            var vote = _activeVotes.ContainsKey(voteId) ? _activeVotes[voteId] : null;
            if (vote != null)
            {
                var initiatorData = _getPlayerPartyDataFunc?.Invoke(vote.InitiatorId);
                if (initiatorData != null)
                    return initiatorData.CurrentPartyId;
            }
            return null;
        }

        public ActiveVote GetVote(string voteId)
        {
            return _activeVotes.ContainsKey(voteId) ? _activeVotes[voteId] : null;
        }

        public List<ActiveVote> GetPartyVotes(string partyId)
        {
            var result = new List<ActiveVote>();
            foreach (var vote in _activeVotes.Values)
            {
                if (vote.Status == VoteResults.VoteStatus.Pending)
                {
                    var initiatorData = _getPlayerPartyDataFunc?.Invoke(vote.InitiatorId);
                    if (initiatorData != null && initiatorData.CurrentPartyId == partyId)
                    {
                        result.Add(vote);
                    }
                }
            }
            return result;
        }
        
        public Dictionary<string, ActiveVote> GetAllVotes()
        {
            return _activeVotes;
        }

        #endregion

        #region Update Loop

        /// <summary>
        /// 处理过期的投票
        /// </summary>
        public void ProcessExpiredVotes()
        {
            var expiredVotes = _voteTimer?.GetExpiredVotes() ?? new List<string>();
            foreach (var voteId in expiredVotes)
            {
                EndVote(voteId);
            }
        }

        #endregion

        #region Save/Load

        public override Dictionary<string, object> ExportSaveData()
        {
            var saveData = new Dictionary<string, object>();
            
            // 导出投票
            var votesData = new List<Dictionary>();
            foreach (var vote in _activeVotes.Values)
            {
                var voteRecordsData = new List<Dictionary>();
                foreach (var record in vote.Votes)
                {
                    voteRecordsData.Add(new Dictionary
                    {
                        { "player_id", record.PlayerId },
                        { "player_name", record.PlayerName },
                        { "voted_yes", record.VotedYes },
                        { "vote_time", record.VoteTime }
                    });
                }
                
                votesData.Add(new Dictionary
                {
                    { "vote_id", vote.VoteId },
                    { "type", (int)vote.Type },
                    { "initiator_id", vote.InitiatorId },
                    { "initiator_name", vote.InitiatorName },
                    { "target_id", vote.TargetId },
                    { "target_name", vote.TargetName },
                    { "reason", vote.Reason },
                    { "start_time", vote.StartTime },
                    { "end_time", vote.EndTime },
                    { "status", (int)vote.Status },
                    { "vote_records", voteRecordsData }
                });
            }
            saveData["votes"] = votesData;
            
            return saveData;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            // 导入投票
            if (data.Contains("votes"))
            {
                var votesData = (Godot.Collections.Array)data["votes"];
                foreach (Dictionary voteData in votesData)
                {
                    var vote = new ActiveVote
                    {
                        VoteId = voteData["vote_id"].ToString(),
                        Type = (VoteResults.VoteType)Convert.ToInt32(voteData["type"]),
                        InitiatorId = voteData["initiator_id"].ToString(),
                        InitiatorName = voteData["initiator_name"].ToString(),
                        TargetId = voteData["target_id"].ToString(),
                        TargetName = voteData["target_name"].ToString(),
                        Reason = voteData["reason"].ToString(),
                        StartTime = Convert.ToInt32(voteData["start_time"]),
                        EndTime = Convert.ToInt32(voteData["end_time"]),
                        Status = (VoteResults.VoteStatus)Convert.ToInt32(voteData["status"])
                    };
                    
                    if (voteData.Contains("vote_records"))
                    {
                        var recordsData = (Godot.Collections.Array)voteData["vote_records"];
                        foreach (Dictionary recordData in recordsData)
                        {
                            vote.Votes.Add(new VoteResults.VoteRecord
                            {
                                PlayerId = recordData["player_id"].ToString(),
                                PlayerName = recordData["player_name"].ToString(),
                                VotedYes = (bool)recordData["voted_yes"],
                                VoteTime = Convert.ToInt32(recordData["vote_time"])
                            });
                        }
                    }
                    
                    _activeVotes[vote.VoteId] = vote;
                }
            }
        }

        #endregion
    }
}
