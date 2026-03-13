using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 多人游戏大厅数据
    /// 房间信息、会话管理、玩家状态追踪
    /// </summary>
    public class MultiplayerLobbyData : Node
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
    }
}
