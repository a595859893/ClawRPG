using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Systems.MultiplayerVote
{
    /// <summary>
    /// 多人投票系统主控制器 - 负责协调投票和队伍管理的子系统
    /// 保留向后兼容性，所有功能通过委托给子系统实现
    /// </summary>
    public partial class MultiplayerVoteSystem : BaseSystem
    {
        private static MultiplayerVoteSystem _instance;
        public static MultiplayerVoteSystem Instance => _instance;

        // 子系统
        private PartyManagementSystem _partySystem;
        private VoteProcessingSystem _voteSystem;
        private VoteTimer _voteTimer;
        private VoteResults _voteResults;
        private MultiplayerVotePersistenceSystem _persistenceSystem;
        
        // 向后兼容的 Signals
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
            
            // 初始化基础组件
            _voteTimer = new VoteTimer();
            _voteResults = new VoteResults();
            
            // 初始化子系统
            InitializeSubsystems();
            
            // 连接子系统信号
            ConnectSubsystemSignals();
        }
        
        /// <summary>
        /// 初始化子系统
        /// </summary>
        private void InitializeSubsystems()
        {
            // 创建并初始化 PartyManagementSystem
            _partySystem = new PartyManagementSystem();
            AddChild(_partySystem);
            
            // 创建并初始化 VoteProcessingSystem
            _voteSystem = new VoteProcessingSystem();
            AddChild(_voteSystem);
            _voteSystem.Initialize(_voteTimer, _voteResults);
            
            // 设置 VoteProcessingSystem 的回调函数
            _voteSystem.SetCallbacks(
                _partySystem.GetPlayerPartyData,
                _partySystem.GetPlayerParty,
                _partySystem.GetAllParties,
                OnKickPlayerFromVote,
                OnLeaderChangedFromVote,
                _partySystem.GetPlayerStatistics
            );
            
            // 创建并初始化 PersistenceSystem
            _persistenceSystem = new MultiplayerVotePersistenceSystem();
            AddChild(_persistenceSystem);
            _persistenceSystem.SetSystems(_partySystem, _voteSystem, _voteTimer);
        }
        
        /// <summary>
        /// 连接子系统信号
        /// </summary>
        private void ConnectSubsystemSignals()
        {
            // PartyManagementSystem signals - use _partySystem (not PartySystem.Instance)
            // These are Godot signals, use .Connect() method
            if (_partySystem != null)
            {
                _partySystem.Connect("PartyCreated", new Callable(this, nameof(OnPartyCreated)));
                _partySystem.Connect("PartyJoined", new Callable(this, nameof(OnPartyJoined)));
                _partySystem.Connect("PartyLeft", new Callable(this, nameof(OnPartyLeft)));
                _partySystem.Connect("PartyMemberKicked", new Callable(this, nameof(OnPartyMemberKicked)));
                _partySystem.Connect("PartyLeaderChanged", new Callable(this, nameof(OnPartyLeaderChanged)));
            }
            
            // VoteProcessingSystem signals - these have C# event patterns
            _voteSystem.VoteStarted += OnVoteStarted;
            _voteSystem.VoteEnded += OnVoteEnded;
            _voteSystem.VoteUpdated += OnVoteUpdated;
        }
        
        // 信号转发处理
        private void OnPartyCreated(Party party) => EmitSignal(SignalName.PartyCreated, party);
        private void OnPartyJoined(string partyId, PartyMember member) => EmitSignal(SignalName.PartyJoined, partyId, member);
        private void OnPartyLeft(string partyId, string playerId) => EmitSignal(SignalName.PartyLeft, partyId, playerId);
        private void OnPartyMemberKicked(string partyId, string playerId) => EmitSignal(SignalName.PartyMemberKicked, partyId, playerId);
        private void OnPartyLeaderChanged(string partyId, string newLeaderId) => EmitSignal(SignalName.PartyLeaderChanged, partyId, newLeaderId);
        private void OnVoteStarted(ActiveVote vote) => EmitSignal(SignalName.VoteStarted, vote);
        private void OnVoteEnded(ActiveVote vote, bool passed) => EmitSignal(SignalName.VoteEnded, vote, passed);
        private void OnVoteUpdated(ActiveVote vote) => EmitSignal(SignalName.VoteUpdated, vote);
        
        // 投票效果回调
        private void OnKickPlayerFromVote(string leaderId, string targetId)
        {
            _partySystem.KickPlayer(leaderId, targetId);
        }
        
        private void OnLeaderChangedFromVote(string partyId, string newLeaderId)
        {
            // 已通过 VoteProcessingSystem 处理
        }
        
        protected override string SystemName => "MultiplayerVote";

        #region Party Management - 委托给 PartyManagementSystem

        /// <summary>
        /// 创建队伍
        /// </summary>
        public Party CreateParty(string leaderId, string leaderName, string partyName = "", bool isPublic = true, string password = "", string gameMode = "", int maxMembers = 4)
        {
            return _partySystem.CreateParty(leaderId, leaderName, partyName, isPublic, password, gameMode, maxMembers);
        }

        /// <summary>
        /// 加入队伍
        /// </summary>
        public bool JoinParty(string playerId, string playerName, int level, int power, string partyId, string password = "")
        {
            return _partySystem.JoinParty(playerId, playerName, level, power, partyId, password);
        }

        /// <summary>
        /// 离开队伍
        /// </summary>
        public bool LeaveParty(string playerId)
        {
            return _partySystem.LeaveParty(playerId);
        }

        /// <summary>
        /// 踢出玩家
        /// </summary>
        public bool KickPlayer(string kickerId, string targetId)
        {
            return _partySystem.KickPlayer(kickerId, targetId);
        }

        /// <summary>
        /// 设置准备状态
        /// </summary>
        public bool SetReady(string playerId, bool ready)
        {
            return _partySystem.SetReady(playerId, ready);
        }

        /// <summary>
        /// 更新队伍设置
        /// </summary>
        public bool UpdatePartySettings(string playerId, bool? isPublic = null, string password = null, string gameMode = null, int? maxMembers = null, int? minLevel = null, int? maxLevel = null)
        {
            return _partySystem.UpdatePartySettings(playerId, isPublic, password, gameMode, maxMembers, minLevel, maxLevel);
        }

        #endregion

        #region Vote System - 委托给 VoteProcessingSystem

        /// <summary>
        /// 发起投票
        /// </summary>
        public ActiveVote InitiateVote(string initiatorId, VoteResults.VoteType voteType, string targetId = "", string targetName = "", string reason = "")
        {
            return _voteSystem.InitiateVote(initiatorId, voteType, targetId, targetName, reason);
        }

        /// <summary>
        /// 投票
        /// </summary>
        public bool CastVote(string voterId, string voteId, bool yes)
        {
            return _voteSystem.CastVote(voterId, voteId, yes);
        }

        /// <summary>
        /// 取消投票
        /// </summary>
        public bool CancelVote(string voteId, string cancellerId)
        {
            return _voteSystem.CancelVote(voteId, cancellerId);
        }

        #endregion

        #region Query Methods - 委托给相应子系统

        public Party GetParty(string partyId)
        {
            return _partySystem.GetParty(partyId);
        }

        public Party GetPlayerParty(string playerId)
        {
            return _partySystem.GetPlayerParty(playerId);
        }

        public ActiveVote GetVote(string voteId)
        {
            return _voteSystem.GetVote(voteId);
        }

        public List<Party> GetPublicParties()
        {
            return _partySystem.GetPublicParties();
        }

        public List<ActiveVote> GetPartyVotes(string partyId)
        {
            return _voteSystem.GetPartyVotes(partyId);
        }

        public PlayerPartyData GetPlayerPartyData(string playerId)
        {
            return _partySystem.GetPlayerPartyData(playerId);
        }

        public PartyStatistics GetPlayerStatistics(string playerId)
        {
            return _partySystem.GetPlayerStatistics(playerId);
        }
        
        /// <summary>
        /// 获取投票计时器（向后兼容）
        /// </summary>
        public VoteTimer GetVoteTimer()
        {
            return _voteTimer;
        }
        
        /// <summary>
        /// 获取投票结果计算器（向后兼容）
        /// </summary>
        public VoteResults GetVoteResults()
        {
            return _voteResults;
        }
        
        /// <summary>
        /// 获取队伍管理系统（向后兼容）
        /// </summary>
        public PartyManagementSystem GetPartySystem()
        {
            return _partySystem;
        }
        
        /// <summary>
        /// 获取投票处理系统（向后兼容）
        /// </summary>
        public VoteProcessingSystem GetVoteSystem()
        {
            return _voteSystem;
        }

        #endregion

        #region Update Loop

        public override void _Process(double delta)
        {
            // 处理过期投票
            _voteSystem.ProcessExpiredVotes();
        }

        #endregion

        #region Save/Load

        public override Dictionary ExportSaveData()
        {
            return _persistenceSystem.ExportSaveData();
        }

        public override void ImportSaveData(Dictionary data)
        {
            _persistenceSystem.ImportSaveData(data);
        }

        #endregion
    }
}
