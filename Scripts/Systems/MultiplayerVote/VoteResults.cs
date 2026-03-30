using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Systems.MultiplayerVote
{
    /// <summary>
    /// 投票结果计算器 - 处理投票结果的计算和判定
    /// </summary>
    public partial class VoteResults : BaseSystem
    {
        /// <summary>
        /// 投票配置
        /// </summary>
        public class VoteConfig
        {
            public VoteType Type { get; set; }
            public int DurationSeconds { get; set; } = 30;
            public float PassThreshold { get; set; } = 0.5f;
            public bool RequireMajority { get; set; } = true;
        }
        
        /// <summary>
        /// 投票类型
        /// </summary>
        public enum VoteType
        {
            KickPlayer,
            PromoteLeader,
            StartGame,
            Surrender,
            Pause,
            MapChoice
        }
        
        /// <summary>
        /// 投票记录
        /// </summary>
        public class VoteRecord
        {
            public string PlayerId { get; set; }
            public string PlayerName { get; set; }
            public bool VotedYes { get; set; }
            public int VoteTime { get; set; }
        }
        
        private Dictionary<string, List<VoteRecord>> _voteRecords = new Dictionary<string, List<VoteRecord>>();
        private Dictionary<string, VoteConfig> _voteConfigs = new Dictionary<string, VoteConfig>();
        
        public override void _Ready()
        {
            base._Ready();
            InitializeConfigs();
        }
        
        /// <summary>
        /// 初始化默认投票配置
        /// </summary>
        private void InitializeConfigs()
        {
            _voteConfigs[VoteType.KickPlayer.ToString()] = new VoteConfig
            {
                Type = VoteType.KickPlayer,
                DurationSeconds = 30,
                PassThreshold = 0.5f,
                RequireMajority = true
            };
            
            _voteConfigs[VoteType.PromoteLeader.ToString()] = new VoteConfig
            {
                Type = VoteType.PromoteLeader,
                DurationSeconds = 20,
                PassThreshold = 0.6f,
                RequireMajority = true
            };
            
            _voteConfigs[VoteType.StartGame.ToString()] = new VoteConfig
            {
                Type = VoteType.StartGame,
                DurationSeconds = 15,
                PassThreshold = 1.0f,
                RequireMajority = false
            };
            
            _voteConfigs[VoteType.Surrender.ToString()] = new VoteConfig
            {
                Type = VoteType.Surrender,
                DurationSeconds = 20,
                PassThreshold = 0.5f,
                RequireMajority = true
            };
        }
        
        /// <summary>
        /// 添加投票记录
        /// </summary>
        public void AddVoteRecord(string voteId, string playerId, string playerName, bool votedYes)
        {
            if (!_voteRecords.ContainsKey(voteId))
            {
                _voteRecords[voteId] = new List<VoteRecord>();
            }
            
            // 检查是否已投票
            var existing = _voteRecords[voteId].FirstOrDefault(v => v.PlayerId == playerId);
            if (existing != null)
            {
                existing.VotedYes = votedYes;
                existing.VoteTime = OS.GetUnixTime();
            }
            else
            {
                _voteRecords[voteId].Add(new VoteRecord
                {
                    PlayerId = playerId,
                    PlayerName = playerName,
                    VotedYes = votedYes,
                    VoteTime = OS.GetUnixTime()
                });
            }
        }
        
        /// <summary>
        /// 计算投票结果
        /// </summary>
        public bool CalculateVoteResult(string voteId, string voteType, int totalPlayers)
        {
            if (!_voteRecords.ContainsKey(voteId))
                return false;
            
            var records = _voteRecords[voteId];
            int yesVotes = records.Count(r => r.VotedYes);
            int noVotes = records.Count(r => !r.VotedYes);
            
            float yesPercentage = totalPlayers > 0 ? (float)yesVotes / totalPlayers : 0;
            
            // 获取配置
            VoteConfig config = null;
            if (_voteConfigs.ContainsKey(voteType))
            {
                config = _voteConfigs[voteType];
            }
            
            if (config != null)
            {
                if (config.RequireMajority)
                {
                    return yesPercentage >= config.PassThreshold;
                }
                else
                {
                    return yesPercentage >= config.PassThreshold;
                }
            }
            
            // 默认：超过半数同意
            return yesVotes > totalPlayers / 2;
        }
        
        /// <summary>
        /// 获取赞成票比例
        /// </summary>
        public float GetYesPercentage(string voteId)
        {
            if (!_voteRecords.ContainsKey(voteId))
                return 0;
            
            var records = _voteRecords[voteId];
            if (records.Count == 0)
                return 0;
            
            int yesVotes = records.Count(r => r.VotedYes);
            return (float)yesVotes / records.Count;
        }
        
        /// <summary>
        /// 获取投票统计
        /// </summary>
        public (int yesVotes, int noVotes) GetVoteStats(string voteId)
        {
            if (!_voteRecords.ContainsKey(voteId))
                return (0, 0);
            
            var records = _voteRecords[voteId];
            int yesVotes = records.Count(r => r.VotedYes);
            int noVotes = records.Count(r => !r.VotedYes);
            
            return (yesVotes, noVotes);
        }
        
        /// <summary>
        /// 获取投票配置
        /// </summary>
        public VoteConfig GetVoteConfig(string voteType)
        {
            if (_voteConfigs.ContainsKey(voteType))
            {
                return _voteConfigs[voteType];
            }
            return null;
        }
        
        /// <summary>
        /// 设置投票配置
        /// </summary>
        public void SetVoteConfig(string voteType, VoteConfig config)
        {
            _voteConfigs[voteType] = config;
        }
        
        /// <summary>
        /// 获取投票记录
        /// </summary>
        public List<VoteRecord> GetVoteRecords(string voteId)
        {
            if (!_voteRecords.ContainsKey(voteId))
                return new List<VoteRecord>();
            
            return _voteRecords[voteId];
        }
        
        /// <summary>
        /// 检查是否所有人都已投票
        /// </summary>
        public bool AllPlayersVoted(string voteId, int totalPlayers)
        {
            if (!_voteRecords.ContainsKey(voteId))
                return false;
            
            return _voteRecords[voteId].Count >= totalPlayers;
        }
        
        /// <summary>
        /// 清理投票记录
        /// </summary>
        public void CleanupVote(string voteId)
        {
            _voteRecords.Remove(voteId);
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
