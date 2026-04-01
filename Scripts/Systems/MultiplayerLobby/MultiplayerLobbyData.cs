using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 多人游戏大厅数据
    /// 房间信息、会话管理、玩家状态追踪
    /// </summary>
    public partial class MultiplayerLobbyData : BaseSystem
    {
        // 房间状态
        public enum RoomState { Waiting, Starting, InProgress, Finished }
        
        // 房间信息
        public class LobbyRoom
        {
            public string RoomId;
            public string RoomName;
            public string HostName;
            public int MaxPlayers = 4;
            public int CurrentPlayers = 1;
            public RoomState State = RoomState.Waiting;
            public string GameMode; // "CoopDungeon", "PvPBattle", "Racing", "BossRush"
            public int Difficulty = 1; // 1-5
            public bool IsPrivate = false;
            public string Password = "";
            public long CreatedAt;
            public List<LobbyPlayer> Players = new List<LobbyPlayer>();
        }
        
        // 玩家信息
        public class LobbyPlayer
        {
            public int PlayerId;
            public string PlayerName;
            public bool IsReady = false;
            public bool IsHost = false;
            public string SelectedClass;
            public int Level = 1;
        }
        
        // 活跃房间追踪
        public Dictionary<string, LobbyRoom> ActiveRooms = new Dictionary<string, LobbyRoom>();
        
        // 当前所在房间
        public string CurrentRoomId = "";
        
        // 房间历史记录
        public List<string> RoomHistory = new List<string>();
        
        // 统计追踪
        public int TotalRoomsCreated = 0;
        public int TotalRoomsJoined = 0;
        public int TotalGamesPlayed = 0;
        public int TotalWins = 0;
        public int TotalLosses = 0;
        
        // 待确认的邀请
        public class LobbyInvite
        {
            public string RoomId;
            public string FromPlayer;
            public long Timestamp;
        }
        public List<LobbyInvite> PendingInvites = new List<LobbyInvite>();
        
        public override void _Ready()
        {
            Name = "MultiplayerLobbyData";
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // 统计追踪
            data["total_rooms_created"] = TotalRoomsCreated;
            data["total_rooms_joined"] = TotalRoomsJoined;
            data["total_games_played"] = TotalGamesPlayed;
            data["total_wins"] = TotalWins;
            data["total_losses"] = TotalLosses;
            
            // 房间历史记录
            data["room_history"] = new Array(RoomHistory);
            
            return data;
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            // 统计追踪
            TotalRoomsCreated = (int)data.GetValueOrDefault("total_rooms_created", 0);
            TotalRoomsJoined = (int)data.GetValueOrDefault("total_rooms_joined", 0);
            TotalGamesPlayed = (int)data.GetValueOrDefault("total_games_played", 0);
            TotalWins = (int)data.GetValueOrDefault("total_wins", 0);
            TotalLosses = (int)data.GetValueOrDefault("total_losses", 0);
            
            // 房间历史记录
            if (data.Contains("room_history"))
            {
                var historyArray = (Array)data["room_history"];
                RoomHistory = new List<string>();
                foreach (string roomId in historyArray)
                {
                    RoomHistory.Add(roomId);
                }
            }
        }
    }
}
