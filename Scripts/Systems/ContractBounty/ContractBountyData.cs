using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ContractBounty
{
    /// <summary>
    /// Contract Bounty System - 委托赏金系统
    /// 玩家可以接受击杀特定目标的委托任务
    /// </summary>
    
    public enum ContractDifficulty
    {
        Easy,        // 普通怪物
        Medium,      // 精英怪物
        Hard,        // Boss级
        Legendary    // 世界Boss
    }
    
    public enum ContractStatus
    {
        Available,   // 可接受
        Active,      // 进行中
        Completed,   // 已完成
        Failed,      // 已失败
        Expired      // 已过期
    }
    
    public enum ContractType
    {
        MonsterHunt,    // 怪物狩猎
        Assassination,   // 暗杀
        Rescue,          // 救援
        Escort,          // 护送
        Collection,     // 收集
        Defense          // 防御
    }
    
    [System.Serializable]
    public class ContractTarget
    {
        public string targetId;
        public string targetName;
        public string targetDescription;
        public int requiredKills;
        public int currentKills;
        public int level;
        public ContractDifficulty difficulty;
    }
    
    [System.Serializable]
    public class ContractReward
    {
        public int gold;
        public int experience;
        public List<string> items;
        public int reputation;
    }
    
    [System.Serializable]
    public class Contract
    {
        public string contractId;
        public string title;
        public string description;
        public string clientName;
        public ContractType type;
        public ContractDifficulty difficulty;
        public ContractStatus status;
        public ContractTarget target;
        public ContractReward reward;
        public int timeLimit;        // 秒
        public DateTime startTime;
        public DateTime expirationTime;
        public string location;
        public string tips;
    }
    
    [System.Serializable]
    public class ContractBountyData
    {
        public List<Contract> availableContracts = new List<Contract>();
        public List<Contract> activeContracts = new List<Contract>();
        public List<Contract> completedContracts = new List<Contract>();
        public List<Contract> failedContracts = new List<Contract>();
        
        public int totalCompleted;
        public int totalFailed;
        public int totalGoldEarned;
        public int totalExpEarned;
        public int currentStreak;
        public int bestStreak;
        
        // Contract discoveries
        public HashSet<string> discoveredContracts = new HashSet<string>();
        public Dictionary<string, int> contractCompletionCount = new Dictionary<string, int>();
    }
}
