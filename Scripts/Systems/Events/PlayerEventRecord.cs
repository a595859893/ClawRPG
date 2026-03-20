using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Events
{
    /// <summary>
    /// 玩家事件记录（用于持久化）
    /// </summary>
    public class PlayerEventRecord
    {
        public string PlayerId { get; set; }
        public int ChoicesMade { get; set; }
        public Dictionary<string, List<string>> EventChoiceHistory { get; set; }  // eventId -> chosen optionIds
        public List<string> UnlockedEvents { get; set; }
        
        public PlayerEventRecord()
        {
            EventChoiceHistory = new Dictionary<string, List<string>>();
            UnlockedEvents = new List<string>();
        }
    }
}
