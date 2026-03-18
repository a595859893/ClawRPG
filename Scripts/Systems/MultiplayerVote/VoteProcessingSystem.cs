using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Systems.MultiplayerVote
{
    /// <summary>
    /// 投票处理系统 - 负责投票的发起、投票、结束等逻辑
    /// </summary>
    public partial class VoteProcessingSystem : BaseSystem
    {
        private static VoteProcessingSystem _instance;
        public static VoteProcessingSystem Instance => _instance;
        
        // 活跃投票
        private Dictionary<string, ActiveVote> _activeVotes = new Dictionary<string, ActiveVote>();
        
        // Signals
        [Signal] public delegate void VoteStartedEventHandler(ActiveVote vote);
        [Signal] public delegate void VoteEndedEventHandler(ActiveVote vote, bool passed);
        [Signal] public delegate void VoteUpdatedEventHandler(ActiveVote vote);
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
        }
        
        protected override string SystemName => "VoteProcessing";
        
        #region Vote Operations
        
        /// <summary>
        /// 发起投票
        /// </summary>
        public ActiveVote InitiateVote(string initiatorId, VoteResults.VoteType voteType, string targetId = "", string targetName = "", string reason = "")
        {
            var vote = new ActiveVote
            {
                VoteId = Guid.NewGuid().ToString(),
                InitiatorId = initiatorId,
                VoteType = voteType,
                TargetId = targetId,
                TargetName = targetName,
                Reason = reason,
                StartTime = OS.GetUnixTime(),
                YesVotes = new List<string>(),
                NoVotes = new List<string>(),
                Voters = new List<string>()
            };
            
            _activeVotes[vote.VoteId] = vote;
            
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
            
            // Check if already voted
            if (vote.Voters.Contains(voterId))
                return false;
            
            // Check if vote is still active
            if (vote.EndTime > 0 && OS.GetUnixTime() > vote.EndTime)
                return false;
            
            if (yes)
            {
                vote.YesVotes.Add(voterId);
            }
            else
            {
                vote.NoVotes.Add(voterId);
            }
            
            vote.Voters.Add(voterId);
            
            EmitSignal(SignalName.VoteUpdated, vote);
            return true;
        }
        
        /// <summary>
        /// 结束投票
        /// </summary>
        public bool EndVote(string voteId)
        {
            if (!_activeVotes.ContainsKey(voteId))
                return false;
            
            var vote = _activeVotes[voteId];
            
            // Calculate result
            var passed = vote.YesVotes.Count > vote.NoVotes.Count;
            
            // Apply vote effects if passed
            if (passed)
            {
                ExecuteVoteEffects(vote);
            }
            
            // Remove from active votes
            _activeVotes.Remove(voteId);
            
            EmitSignal(SignalName.VoteEnded, vote, passed);
            return true;
        }
        
        /// <summary>
        /// 取消投票
        /// </summary>
        public bool CancelVote(string voteId)
        {
            if (!_activeVotes.ContainsKey(voteId))
                return false;
            
            _activeVotes.Remove(voteId);
            return true;
        }
        
        /// <summary>
        /// 获取活跃投票
        /// </summary>
        public ActiveVote GetActiveVote(string voteId)
        {
            return _activeVotes.ContainsKey(voteId) ? _activeVotes[voteId] : null;
        }
        
        /// <summary>
        /// 获取玩家所有活跃投票
        /// </summary>
        public List<ActiveVote> GetPlayerActiveVotes(string playerId)
        {
            return _activeVotes.Values.Where(v => 
                v.InitiatorId == playerId || 
                v.TargetId == playerId || 
                v.Voters.Contains(playerId)).ToList();
        }
        
        /// <summary>
        /// 获取队伍的所有活跃投票
        /// </summary>
        public List<ActiveVote> GetPartyActiveVotes(string partyId)
        {
            return _activeVotes.Values.Where(v => v.PartyId == partyId).ToList();
        }
        
        #endregion
        
        #region Vote Effects
        
        private void ExecuteVoteEffects(ActiveVote vote)
        {
            switch (vote.VoteType)
            {
                case VoteResults.VoteType.KickPlayer:
                    // Execute kick player effect
                    PartyManagementSystem.Instance?.KickPlayer(vote.InitiatorId, vote.TargetId);
                    break;
                    
                case VoteResults.VoteType.ChangeLeader:
                    // Execute change leader effect
                    // This would require additional implementation
                    break;
                    
                case VoteResults.VoteType.StartGame:
                    // Execute start game effect
                    break;
                    
                case VoteResults.VoteType.DisbandParty:
                    // Execute disband party effect
                    break;
                    
                default:
                    break;
            }
        }
        
        #endregion
        
        #region Vote Validation
        
        /// <summary>
        /// 检查是否可以发起投票
        /// </summary>
        public bool CanInitiateVote(string playerId, VoteResults.VoteType voteType)
        {
            // Add cooldown check or other validation
            return true;
        }
        
        /// <summary>
        /// 检查是否可以投票
        /// </summary>
        public bool CanVote(string voterId, string voteId)
        {
            if (!_activeVotes.ContainsKey(voteId))
                return false;
            
            var vote = _activeVotes[voteId];
            
            // Already voted
            if (vote.Voters.Contains(voterId))
                return false;
            
            // Vote expired
            if (vote.EndTime > 0 && OS.GetUnixTime() > vote.EndTime)
                return false;
            
            return true;
        }
        
        #endregion
        
        #region Persistence
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            var votesArray = new Array();
            foreach (var vote in _activeVotes.Values)
            {
                votesArray.Add(JsonSerializer.Serialize(vote));
            }
            data["activeVotes"] = votesArray;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            _activeVotes.Clear();
            
            if (data.Contains("activeVotes"))
            {
                var votesArray = (Array)data["activeVotes"];
                foreach (string voteJson in votesArray)
                {
                    var vote = JsonSerializer.Deserialize<ActiveVote>(voteJson);
                    if (vote != null)
                    {
                        _activeVotes[vote.VoteId] = vote;
                    }
                }
            }
        }
        
        #endregion
    }
}
