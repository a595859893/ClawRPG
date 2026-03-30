using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Systems.MultiplayerVote
{
    /// <summary>
    /// 投票计时器 - 管理投票的时间限制和过期处理
    /// </summary>
    public partial class VoteTimer : BaseSystem
    {
        private Dictionary<string, int> _voteEndTimes = new Dictionary<string, int>();
        private Dictionary<string, VoteStatus> _voteStatuses = new Dictionary<string, VoteStatus>();
        
        /// <summary>
        /// 投票状态
        /// </summary>
        public enum VoteStatus
        {
            Pending,
            Passed,
            Failed,
            Cancelled
        }
        
        public override void _Ready()
        {
            base._Ready();
        }
        
        /// <summary>
        /// 设置投票超时
        /// </summary>
        public void SetVoteTimeout(string voteId, int durationSeconds)
        {
            int endTime = OS.GetUnixTime() + durationSeconds;
            _voteEndTimes[voteId] = endTime;
            _voteStatuses[voteId] = VoteStatus.Pending;
        }
        
        /// <summary>
        /// 检查投票是否过期
        /// </summary>
        public bool IsVoteExpired(string voteId)
        {
            if (!_voteEndTimes.ContainsKey(voteId))
                return false;
            
            return OS.GetUnixTime() > _voteEndTimes[voteId];
        }
        
        /// <summary>
        /// 获取剩余时间（秒）
        /// </summary>
        public int GetRemainingTime(string voteId)
        {
            if (!_voteEndTimes.ContainsKey(voteId))
                return 0;
            
            int remaining = _voteEndTimes[voteId] - OS.GetUnixTime();
            return Math.Max(0, remaining);
        }
        
        /// <summary>
        /// 设置投票状态
        /// </summary>
        public void SetVoteStatus(string voteId, VoteStatus status)
        {
            _voteStatuses[voteId] = status;
        }
        
        /// <summary>
        /// 获取投票状态
        /// </summary>
        public VoteStatus GetVoteStatus(string voteId)
        {
            if (!_voteStatuses.ContainsKey(voteId))
                return VoteStatus.Pending;
            
            return _voteStatuses[voteId];
        }
        
        /// <summary>
        /// 取消投票计时
        /// </summary>
        public void CancelVoteTimer(string voteId)
        {
            _voteEndTimes.Remove(voteId);
            _voteStatuses[voteId] = VoteStatus.Cancelled;
        }
        
        /// <summary>
        /// 获取所有过期的投票ID
        /// </summary>
        public List<string> GetExpiredVotes()
        {
            var expired = new List<string>();
            foreach (var kvp in _voteEndTimes)
            {
                if (_voteStatuses[kvp.Key] == VoteStatus.Pending && IsVoteExpired(kvp.Key))
                {
                    expired.Add(kvp.Key);
                }
            }
            return expired;
        }
        
        /// <summary>
        /// 更新投票结束时间
        /// </summary>
        public void ExtendVoteTime(string voteId, int additionalSeconds)
        {
            if (_voteEndTimes.ContainsKey(voteId))
            {
                _voteEndTimes[voteId] += additionalSeconds;
            }
        }
        
        /// <summary>
        /// 清理投票计时数据
        /// </summary>
        public void CleanupVote(string voteId)
        {
            _voteEndTimes.Remove(voteId);
            _voteStatuses.Remove(voteId);
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
