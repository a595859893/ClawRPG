using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Modules.MultiplayerVote;

namespace ClawRPG.Systems.MultiplayerVote
{
    /// <summary>
    /// 投票管理器 - 提供投票操作的高级接口
    /// </summary>
    public partial class VoteManager : BaseSystem
    {
        private MultiplayerVoteSystem _voteSystem;
        
        public override void _Ready()
        {
            base._Ready();
            _voteSystem = MultiplayerVoteSystem.Instance;
        }
        
        /// <summary>
        /// 开始投票
        /// </summary>
        public ActiveVote StartVote(VoteResults.VoteType type, string initiatorId, string initiatorName, string targetId = "", string targetName = "", string reason = "")
        {
            return _voteSystem.InitiateVote(initiatorId, type, targetId, targetName, reason);
        }
        
        /// <summary>
        /// 投票
        /// </summary>
        public bool Vote(string voteId, string playerId, bool approve)
        {
            return _voteSystem.CastVote(playerId, voteId, approve);
        }
        
        /// <summary>
        /// 检查投票结果
        /// </summary>
        public bool CheckVoteResult(string voteId, string partyId)
        {
            var party = _voteSystem.GetParty(partyId);
            if (party == null)
                return false;
            
            var votes = _voteSystem.GetPartyVotes(partyId);
            var vote = votes.Find(v => v.VoteId == voteId);
            
            if (vote == null)
                return false;
            
            return vote.Status == VoteResults.VoteStatus.Passed;
        }
        
        /// <summary>
        /// 获取投票状态
        /// </summary>
        public VoteResults.VoteStatus GetVoteStatus(string voteId)
        {
            var vote = _voteSystem.GetVote(voteId);
            return vote != null ? vote.Status : VoteResults.VoteStatus.Cancelled;
        }
        
        /// <summary>
        /// 取消投票
        /// </summary>
        public bool CancelVote(string voteId, string cancellerId)
        {
            return _voteSystem.CancelVote(voteId, cancellerId);
        }
        
        /// <summary>
        /// 获取活跃投票
        /// </summary>
        public List<ActiveVote> GetActiveVotes(string partyId)
        {
            return _voteSystem.GetPartyVotes(partyId);
        }
        
        /// <summary>
        /// 创建队伍
        /// </summary>
        public Party CreateParty(string leaderId, string leaderName, string partyName = "", bool isPublic = true, string password = "")
        {
            return _voteSystem.CreateParty(leaderId, leaderName, partyName, isPublic, password);
        }
        
        /// <summary>
        /// 加入队伍
        /// </summary>
        public bool JoinParty(string playerId, string playerName, int level, int power, string partyId, string password = "")
        {
            return _voteSystem.JoinParty(playerId, playerName, level, power, partyId, password);
        }
        
        /// <summary>
        /// 离开队伍
        /// </summary>
        public bool LeaveParty(string playerId)
        {
            return _voteSystem.LeaveParty(playerId);
        }
        
        /// <summary>
        /// 踢出玩家
        /// </summary>
        public bool KickPlayer(string kickerId, string targetId)
        {
            return _voteSystem.KickPlayer(kickerId, targetId);
        }
        
        /// <summary>
        /// 设置准备状态
        /// </summary>
        public bool SetReady(string playerId, bool ready)
        {
            return _voteSystem.SetReady(playerId, ready);
        }
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            // 加载数据
        }
    }
}
