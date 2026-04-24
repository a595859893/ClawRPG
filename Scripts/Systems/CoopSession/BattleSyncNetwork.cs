using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 战斗网络同步 - 消息序列化、网络广播、延迟补偿
    /// </summary>
    public partial class BattleSyncNetwork : BaseSystem
    {
        // 延迟补偿器引用
        protected BattleLagCompensation _lagCompensation;
        
        // 序列化器引用
        protected BattleActionSerializer _serializer;

        // 网络配置
        protected bool _isNetworkEnabled = false;

        #region 网络状态

        /// <summary>
        /// 是否启用了网络同步
        /// </summary>
        public bool IsNetworkEnabled => _isNetworkEnabled;

        /// <summary>
        /// 启用网络同步
        /// </summary>
        public virtual void EnableNetwork()
        {
            _isNetworkEnabled = true;
            GD.Print("[BattleSyncNetwork] Network sync enabled");
        }

        /// <summary>
        /// 禁用网络同步
        /// </summary>
        public virtual void DisableNetwork()
        {
            _isNetworkEnabled = false;
            GD.Print("[BattleSyncNetwork] Network sync disabled");
        }

        #endregion

        #region 消息序列化

        /// <summary>
        /// 序列化战斗操作为网络消息
        /// </summary>
        public virtual Dictionary<string, object> SerializeBattleAction(BattleSyncData.BattleAction action)
        {
            return new Dictionary<string, object>
            {
                { "actionId", action.ActionId },
                { "playerId", action.PlayerId },
                { "playerName", action.PlayerName },
                { "type", action.Type.ToString() },
                { "skillId", action.SkillId },
                { "value", action.Value },
                { "targetX", action.TargetX },
                { "targetY", action.TargetY },
                { "targetId", action.TargetId },
                { "isCritical", action.IsCritical },
                { "timestamp", action.Timestamp }
            };
        }

        /// <summary>
        /// 序列化多个战斗操作
        /// </summary>
        public virtual ArrayList SerializeBattleActions(IEnumerable<BattleSyncData.BattleAction> actions)
        {
            var serialized = new ArrayList();
            foreach (var action in actions)
            {
                serialized.Add(SerializeBattleAction(action));
            }
            return serialized;
        }

        /// <summary>
        /// 从网络消息反序列化战斗操作
        /// </summary>
        public virtual BattleSyncData.BattleAction DeserializeBattleAction(Dictionary<string, object> data)
        {
            var action = new BattleSyncData.BattleAction
            {
                ActionId = data.ContainsKey("actionId") ? data["actionId"]?.ToString() ?? "" : "",
                PlayerId = data.ContainsKey("playerId") ? Convert.ToInt32(data["playerId"]) : 0,
                PlayerName = data.ContainsKey("playerName") ? data["playerName"]?.ToString() ?? "" : "",
                SkillId = data.ContainsKey("skillId") ? data["skillId"]?.ToString() ?? "" : "",
                Value = data.ContainsKey("value") ? Convert.ToSingle(data["value"]) : 0,
                TargetX = data.ContainsKey("targetX") ? Convert.ToSingle(data["targetX"]) : 0,
                TargetY = data.ContainsKey("targetY") ? Convert.ToSingle(data["targetY"]) : 0,
                TargetId = data.ContainsKey("targetId") ? Convert.ToInt32(data["targetId"]) : -1,
                IsCritical = data.ContainsKey("isCritical") && Convert.ToBoolean(data["isCritical"]),
                Timestamp = data.ContainsKey("timestamp") ? Convert.ToInt64(data["timestamp"]) : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // 解析类型
            if (data.ContainsKey("type") && Enum.TryParse<BattleActionType>(data["type"]?.ToString(), out var actionType))
            {
                action.Type = actionType;
            }

            return action;
        }

        /// <summary>
        /// 从网络消息反序列化多个战斗操作
        /// </summary>
        public virtual List<BattleSyncData.BattleAction> DeserializeBattleActions(ArrayList data)
        {
            var actions = new List<BattleSyncData.BattleAction>();
            if (data == null) return actions;

            foreach (Dictionary actionData in data)
            {
                actions.Add(DeserializeBattleAction(new Dictionary<string, object>(actionData)));
            }
            return actions;
        }

        #endregion

        #region 网络广播

        /// <summary>
        /// 通过网络广播战斗操作到其他玩家
        /// </summary>
        public virtual void BroadcastActionsToNetwork(List<BattleSyncData.BattleAction> actions)
        {
            if (!_isNetworkEnabled || actions == null || actions.Count == 0)
                return;

            // 检查是否在房间中
            if (MultiplayerManager.Instance == null || !MultiplayerManager.Instance.IsInRoom)
                return;

            // 创建广播消息
            var serializedActions = SerializeBattleActions(actions);

            var message = new Dictionary<string, object>
            {
                { "type", "battle_action" },
                { "room_id", MultiplayerManager.Instance.GetRoomInfo()?.RoomId ?? "" },
                { "actions", serializedActions }
            };

            NetworkClient.Instance.SendJson(message);
            GD.Print($"[BattleSyncNetwork] Broadcasted {actions.Count} actions to network");
        }

        /// <summary>
        /// 广播单个战斗操作
        /// </summary>
        public virtual void BroadcastAction(BattleSyncData.BattleAction action)
        {
            BroadcastActionsToNetwork(new List<BattleSyncData.BattleAction> { action });
        }

        #endregion

        #region 消息接收

        /// <summary>
        /// 处理接收到的网络消息
        /// </summary>
        public virtual void HandleNetworkMessage(Dictionary<string, object> message)
        {
            if (message == null || !message.ContainsKey("type"))
                return;

            var messageType = message["type"]?.ToString();
            
            switch (messageType)
            {
                case "battle_action":
                    HandleBattleActionMessage(message);
                    break;
                    
                case "battle_snapshot":
                    HandleBattleSnapshotMessage(message);
                    break;
                    
                case "player_joined":
                    HandlePlayerJoinedMessage(message);
                    break;
                    
                case "player_left":
                    HandlePlayerLeftMessage(message);
                    break;
            }
        }

        /// <summary>
        /// 处理战斗操作消息
        /// </summary>
        protected virtual void HandleBattleActionMessage(Dictionary<string, object> message)
        {
            if (!message.ContainsKey("actions") || message["actions"] is not ArrayList actionsData)
                return;

            var actions = DeserializeBattleActions(actionsData);
            
            foreach (var action in actions)
            {
                // 应用延迟补偿
                if (_lagCompensation != null)
                {
                    action.Timestamp = _lagCompensation.AdjustTimestamp(action.Timestamp);
                }

                // 触发远程操作处理
                OnRemoteActionReceived?.Invoke(action);
            }

            GD.Print($"[BattleSyncNetwork] Received {actions.Count} battle actions from network");
        }

        /// <summary>
        /// 处理战斗快照消息
        /// </summary>
        protected virtual void HandleBattleSnapshotMessage(Dictionary<string, object> message)
        {
            // 快照处理由主系统处理
            GD.Print("[BattleSyncNetwork] Battle snapshot received from network");
        }

        /// <summary>
        /// 处理玩家加入消息
        /// </summary>
        protected virtual void HandlePlayerJoinedMessage(Dictionary<string, object> message)
        {
            GD.Print("[BattleSyncNetwork] Player joined battle from network");
        }

        /// <summary>
        /// 处理玩家离开消息
        /// </summary>
        protected virtual void HandlePlayerLeftMessage(Dictionary<string, object> message)
        {
            GD.Print("[BattleSyncNetwork] Player left battle from network");
        }

        #endregion

        #region 延迟补偿

        /// <summary>
        /// 设置延迟补偿器
        /// </summary>
        public virtual void SetLagCompensation(BattleLagCompensation lagCompensation)
        {
            _lagCompensation = lagCompensation;
        }

        /// <summary>
        /// 应用延迟补偿到操作时间戳
        /// </summary>
        public virtual long ApplyLagCompensation(long timestamp)
        {
            if (_lagCompensation != null)
            {
                return _lagCompensation.AdjustTimestamp(timestamp);
            }
            return timestamp;
        }

        #endregion

        #region 状态同步

        /// <summary>
        /// 请求全量状态同步
        /// </summary>
        public virtual void RequestStateSync()
        {
            if (!_isNetworkEnabled || MultiplayerManager.Instance == null || !MultiplayerManager.Instance.IsInRoom)
                return;

            var message = new Dictionary<string, object>
            {
                { "type", "request_battle_snapshot" },
                { "room_id", MultiplayerManager.Instance.GetRoomInfo()?.RoomId ?? "" }
            };

            NetworkClient.Instance.SendJson(message);
            GD.Print("[BattleSyncNetwork] Requested state sync");
        }

        /// <summary>
        /// 广播状态变化请求
        /// </summary>
        public virtual void BroadcastStateChangeRequest(string stateType, Dictionary<string, object> stateData)
        {
            if (!_isNetworkEnabled || MultiplayerManager.Instance == null || !MultiplayerManager.Instance.IsInRoom)
                return;

            var message = new Dictionary<string, object>
            {
                { "type", "battle_state_change" },
                { "room_id", MultiplayerManager.Instance.GetRoomInfo()?.RoomId ?? "" },
                { "state_type", stateType },
                { "state_data", stateData }
            };

            NetworkClient.Instance.SendJson(message);
        }

        #endregion

        #region 回调事件

        /// <summary>
        /// 远程操作接收回调
        /// </summary>
        public Action<BattleSyncData.BattleAction> OnRemoteActionReceived { get; set; }

        /// <summary>
        /// 快照接收回调
        /// </summary>
        public Action<BattleSyncData.BattleSnapshot> OnSnapshotReceived { get; set; }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化网络组件
        /// </summary>
        protected void InitializeNetworkComponents()
        {
            _serializer = new BattleActionSerializer();
            _lagCompensation = new BattleLagCompensation();
            GD.Print("[BattleSyncNetwork] Network components initialized");
        }

        #endregion

        #region 持久化

        /// <summary>
        /// 导出持久化数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // 序列化网络状态
            data["IsNetworkEnabled"] = _isNetworkEnabled;
            
            GD.Print($"[BattleSyncNetwork] 导出网络状态: {_isNetworkEnabled}");
            return data;
        }

        /// <summary>
        /// 导入持久化数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null)
            {
                GD.Print("[BattleSyncNetwork] 无数据可导入");
                return;
            }
            
            // 导入网络状态
            if (data.ContainsKey("IsNetworkEnabled"))
            {
                _isNetworkEnabled = data["IsNetworkEnabled"] is bool ine && ine;
            }
            
            GD.Print($"[BattleSyncNetwork] 导入网络状态: {_isNetworkEnabled}");
        }

        #endregion
    }
}
