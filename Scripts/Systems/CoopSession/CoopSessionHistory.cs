using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 玩家合作会话历史
    /// </summary>
    public class CoopSessionHistory
    {
        public int PlayerId { get; set; }
        public List<CoopSessionRecord> Sessions { get; set; }
        
        // 统计数据
        public int TotalSessionsJoined { get; set; }
        public int TotalSessionsCompleted { get; set; }
        public int TotalSessionsWon { get; set; }
        public int TotalExpEarned { get; set; }
        public int TotalGoldEarned { get; set; }
        
        public CoopSessionHistory()
        {
            Sessions = new List<CoopSessionRecord>();
        }
    }

    /// <summary>
    /// 单次会话记录
    /// </summary>
    public class CoopSessionRecord
    {
        public string SessionId { get; set; } = "";
        public string DungeonName { get; set; } = "";
        public CoopAdventureType AdventureType { get; set; }
        public bool WasVictory { get; set; }
        public int FloorReached { get; set; }
        public TimeSpan Duration { get; set; }
        public int ExpEarned { get; set; }
        public int GoldEarned { get; set; }
        public DateTime PlayedAt { get; set; }
    }
}
