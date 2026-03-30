using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Systems.MultiplayerVote
{
    /// <summary>
    /// 多人投票持久化系统 - 负责整体数据的保存和加载
    /// 继承 BaseSystem 实现数据持久化接口
    /// </summary>
    public partial class MultiplayerVotePersistenceSystem : BaseSystem
    {
        private static MultiplayerVotePersistenceSystem _instance;
        public static MultiplayerVotePersistenceSystem Instance => _instance;
        
        // 外部依赖
        private PartyManagementSystem _partySystem;
        private VoteProcessingSystem _voteSystem;
        private VoteTimer _voteTimer;
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
        }
        
        protected override string SystemName => "MultiplayerVotePersistence";
        
        /// <summary>
        /// 初始化依赖
        /// </summary>
        public void Initialize(PartyManagementSystem partySystem, VoteProcessingSystem voteSystem, VoteTimer voteTimer)
        {
            _partySystem = partySystem;
            _voteSystem = voteSystem;
            _voteTimer = voteTimer;
        }
        
        /// <summary>
        /// 设置子系统引用
        /// </summary>
        public void SetSystems(PartyManagementSystem partySystem, VoteProcessingSystem voteSystem, VoteTimer voteTimer)
        {
            _partySystem = partySystem;
            _voteSystem = voteSystem;
            _voteTimer = voteTimer;
        }

        #region Save/Load

        public override Dictionary<string, object> ExportSaveData()
        {
            var saveData = new Dictionary<string, object>();
            
            // 导出 PartyManagementSystem 数据
            if (_partySystem != null)
            {
                saveData["party_system"] = _partySystem.ExportSaveData();
            }
            
            // 导出 VoteProcessingSystem 数据
            if (_voteSystem != null)
            {
                saveData["vote_system"] = _voteSystem.ExportSaveData();
            }
            
            // 导出 VoteTimer 数据
            if (_voteTimer != null)
            {
                saveData["vote_timer"] = _voteTimer.ExportSaveData();
            }
            
            return saveData;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            // 导入 PartyManagementSystem 数据
            if (data.Contains("party_system"))
            {
                var partyData = (Dictionary)data["party_system"];
                _partySystem?.ImportSaveData(partyData);
            }
            
            // 导入 VoteProcessingSystem 数据
            if (data.Contains("vote_system"))
            {
                var voteData = (Dictionary)data["vote_system"];
                _voteSystem?.ImportSaveData(voteData);
            }
            
            // 导入 VoteTimer 数据
            if (data.Contains("vote_timer"))
            {
                var timerData = (Dictionary)data["vote_timer"];
                _voteTimer?.ImportSaveData(timerData);
            }
        }

        #endregion
        
        /// <summary>
        /// 重置所有数据
        /// </summary>
        public void ResetAll()
        {
            // 重置各子系统数据
            _partySystem?.ImportSaveData(new Dictionary<string, object>());
            _voteSystem?.ImportSaveData(new Dictionary<string, object>());
            
            GD.Print($"[{SystemName}] All data reset");
        }
    }
}
