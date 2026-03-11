using Godot;
using System;
using System.Collections.Generic;

namespace GameSystems {
    // 公会等级枚举
    public enum GuildLevel {
        None = 0,
        Recruit = 1,
        Member = 2,
        Elder = 3,
        ViceLeader = 4,
        Leader = 5
    }

    // 公会权限
    [Flags]
    public enum GuildPermission {
        None = 0,
        Invite = 1,
        Kick = 2,
        Promote = 4,
        Demote = 8,
        ManageNotice = 16,
        ManageBank = 32,
        UpgradeGuild = 64,
        AcceptQuest = 128,
        All = 255
    }

    // 玩家公会数据
    public class PlayerGuildData {
        public string GuildId { get; set; } = "";
        public GuildLevel Level { get; set; } = GuildLevel.None;
        public GuildPermission Permissions { get; set; } = GuildPermission.None;
        public int Contribution { get; set; } = 0;       // 个人贡献度
        public int TotalContribution { get; set; } = 0;   // 累计贡献度
        public int WeeklyContribution { get; set; } = 0; // 本周贡献度
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public DateTime LastActive { get; set; } = DateTime.Now;
    }

    // 公会成员数据
    public class GuildMember {
        public string PlayerId { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public GuildLevel Level { get; set; } = GuildLevel.Recruit;
        public GuildPermission Permissions { get; set; } = GuildPermission.Invite;
        public int Contribution { get; set; } = 0;
        public int TotalContribution { get; set; } = 0;
        public int WeeklyContribution { get; set; } = 0;
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public DateTime LastActive { get; set; } = DateTime.Now;
        public bool IsOnline { get; set; } = false; 
    }

    // 公会建筑数据
    public class GuildBuilding {
        public string BuildingId { get; set; } = "";
        public string Name { get; set; } = "";
        public int Level { get; set; } = 1;
        public int MaxLevel { get; set; } = 10;
        public int UpgradeCost { get; set; } = 1000;
        public int UpgradeRequirement { get; set; } = 100; // 升级所需贡献度
        public string Description { get; set; } = "";
        public Dictionary<string, int> Bonuses { get; set; } = new Dictionary<string, int>();
    }

    // 公会技能数据
    public class GuildSkill {
        public string SkillId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int Level { get; set; } = 0;
        public int MaxLevel { get; set; } = 5;
        public int CostPerLevel { get; set; } = 100; // 每次升级消耗贡献度
        public Dictionary<string, int> Bonuses { get; set; } = new Dictionary<string, int>();
        public bool IsUnlocked { get; set; } = false; 
    }

    // 公会数据
    public class GuildData {
        public string GuildId { get; set; } = "";
        public string Name { get; set; } = "";
        public string LeaderId { get; set; } = "";
        public string LeaderName { get; set; } = "";
        public string Description { get; set; } = "";
        public string Notice { get; set; } = "";
        public string Icon { get; set; } = "default";
        public int Level { get; set; } = 1;
        public int MaxMembers { get; set; } = 20;
        public int CurrentMembers { get; set; } = 0;
        public int TotalContribution { get; set; } = 0;  // 公会总贡献度
        public int WeeklyContribution { get; set; } = 0; // 本周贡献度
        public int Experience { get; set; } = 0;
        public int Gold { get; set; } = 0;              // 公会资金
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime LastActivity { get; set; } = DateTime.Now;
        
        // 成员列表
        public List<GuildMember> Members { get; set; } = new List<GuildMember>();
        
        // 建筑
        public Dictionary<string, GuildBuilding> Buildings { get; set; } = new Dictionary<string, GuildBuilding>();
        
        // 技能
        public Dictionary<string, GuildSkill> Skills { get; set; } = new Dictionary<string, GuildSkill>();
        
        // 公开设置
        public bool IsPublic { get; set; } = true;
        public bool AllowInvite { get; set; } = true;
        public int RequiredLevel { get; set; } = 1;
        public int RequiredContribution { get; set; } = 0;
    }

    // 公会申请
    public class GuildApplication {
        public string ApplicationId { get; set; } = "";
        public string GuildId { get; set; } = "";
        public string PlayerId { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public string Message { get; set; } = "";
        public int PlayerLevel { get; set; } = 1;
        public int PlayerContribution { get; set; } = 0;
        public DateTime ApplyTime { get; set; } = DateTime.Now;
        public bool IsAccepted { get; set; } = false; 
    }
}
