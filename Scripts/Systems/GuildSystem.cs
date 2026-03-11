using Godot;
using System;
using System.Collections.Generic;

public partial class GuildSystem : Node {
    public static GuildSystem Instance { get; private set; }

    // 玩家公会数据
    public PlayerGuildData PlayerData { get; private set; } = new PlayerGuildData();
    
    // 当前公会数据
    public GuildData CurrentGuild { get; private set; }
    
    // 玩家申请列表
    public List<GuildApplication> MyApplications { get; private set; } = new List<GuildApplication>();
    
    // 可加入的公会列表
    public List<GuildData> AvailableGuilds { get; private set; } = new List<GuildData>();
    
    // 公会公告板
    public List<GuildAnnouncement> Announcements { get; private set; } = new List<GuildAnnouncement>();

    // 信号
    [Signal] public delegate void GuildCreatedEventHandler(GuildData guild);
    [Signal] public delegate void GuildJoinedEventHandler(GuildData guild);
    [Signal] public delegate void GuildLeftEventHandler();
    [Signal] public delegate void MemberJoinedEventHandler(GuildMember member);
    [Signal] public delegate void MemberLeftEventHandler(string playerId);
    [Signal] public delegate void GuildLevelUpEventHandler(int newLevel);
    [Signal] public delegate void ContributionChangedEventHandler(int newContribution);
    [Signal] public delegate void ApplicationReceivedEventHandler(GuildApplication app);
    [Signal] public delegate void ApplicationProcessedEventHandler(string applicationId, bool accepted);

    // 公会公告
    public class GuildAnnouncement {
        public string AnnouncementId { get; set; } = "";
        public string Content { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public DateTime PostTime { get; set; } = DateTime.Now;
    }

    public override void _Ready() {
        Instance = this;
    }

    // 创建公会
    public bool CreateGuild(string name) {
        if (PlayerData.GuildId != "") {
            GD.PrintErr("玩家已有公会，无法创建新公会");
            return false;
        }

        if (name.Length < 2 || name.Length > 20) {
            GD.PrintErr("公会名称长度必须在2-20之间");
            return false;
        }

        var player = GetTree().CurrentScene.GetNode<Player>("Player");
        string playerId = player?.PlayerId ?? "player1";
        string playerName = player?.PlayerName ?? "Player";

        string guildId = "guild_" + GD.Hash(name + DateTime.Now.ToString()).ToString();
        var guild = GuildDatabase.CreateNewGuild(guildId, name, playerId, playerName);

        CurrentGuild = guild;
        PlayerData.GuildId = guildId;
        PlayerData.Level = GuildLevel.Leader;
        PlayerData.Permissions = GuildPermission.All;
        PlayerData.Contribution = 0;
        PlayerData.JoinDate = DateTime.Now;

        GD.Print($"公会创建成功: {name}");
        EmitSignal(SignalName.GuildCreated, guild);
        return true;
    }

    // 加入公会
    public bool JoinGuild(string guildId) {
        if (PlayerData.GuildId != "") {
            GD.PrintErr("玩家已有公会");
            return false;
        }

        // 查找公会
        var guild = FindGuild(guildId);
        if (guild == null) {
            GD.PrintErr("公会不存在");
            return false;
        }

        if (guild.CurrentMembers >= guild.MaxMembers) {
            GD.PrintErr("公会已满");
            return false;
        }

        // 添加成员
        var player = GetTree().CurrentScene.GetNode<Player>("Player");
        string playerId = player?.PlayerId ?? "player1";
        string playerName = player?.PlayerName ?? "Player";

        var member = new GuildMember {
            PlayerId = playerId,
            PlayerName = playerName,
            Level = GuildLevel.Recruit,
            Permissions = GuildPermission.Invite,
            JoinDate = DateTime.Now,
            LastActive = DateTime.Now,
            IsOnline = true
        };

        guild.Members.Add(member);
        guild.CurrentMembers++;
        guild.LastActivity = DateTime.Now;

        CurrentGuild = guild;
        PlayerData.GuildId = guildId;
        PlayerData.Level = GuildLevel.Recruit;
        PlayerData.Permissions = GuildPermission.Invite;
        PlayerData.Contribution = 0;
        PlayerData.JoinDate = DateTime.Now;

        GD.Print($"成功加入公会: {guild.Name}");
        EmitSignal(SignalName.GuildJoined, guild);
        return true;
    }

    // 离开公会
    public bool LeaveGuild() {
        if (PlayerData.GuildId == "") {
            GD.PrintErr("玩家没有公会");
            return false;
        }

        if (CurrentGuild == null) return false;

        // 会长不能直接离开
        if (PlayerData.Level == GuildLevel.Leader && CurrentGuild.Members.Count > 1) {
            GD.PrintErr("会长必须转让会长职位后才能离开");
            return false;
        }

        string playerId = GetTree().CurrentScene.GetNode<Player>("Player")?.PlayerId ?? "player1";
        
        // 移除成员
        CurrentGuild.Members.RemoveAll(m => m.PlayerId == playerId);
        CurrentGuild.CurrentMembers--;

        // 如果没有成员了，解散公会
        if (CurrentGuild.CurrentMembers <= 0) {
            DisbandGuild();
        }

        PlayerData = new PlayerGuildData();
        CurrentGuild = null;

        GD.Print("离开公会成功");
        EmitSignal(SignalName.GuildLeft);
        return true;
    }

    // 解散公会
    public bool DisbandGuild() {
        if (PlayerData.Level != GuildLevel.Leader) {
            GD.PrintErr("只有会长可以解散公会");
            return false;
        }

        // TODO: 从全局公会列表中移除

        CurrentGuild = null;
        PlayerData = new PlayerGuildData();

        GD.Print("公会已解散");
        EmitSignal(SignalName.GuildLeft);
        return true;
    }

    // 查找公会
    private GuildData FindGuild(string guildId) {
        // 简化版本：从可用列表中查找
        foreach (var guild in AvailableGuilds) {
            if (guild.GuildId == guildId) return guild;
        }
        if (CurrentGuild != null && CurrentGuild.GuildId == guildId) return CurrentGuild;
        return null;
    }

    // 申请加入公会
    public bool ApplyToGuild(string guildId, string message = "") {
        if (PlayerData.GuildId != "") {
            GD.PrintErr("玩家已有公会");
            return false;
        }

        var guild = FindGuild(guildId);
        if (guild == null) {
            GD.PrintErr("公会不存在");
            return false;
        }

        var player = GetTree().CurrentScene.GetNode<Player>("Player");
        string playerId = player?.PlayerId ?? "player1";
        string playerName = player?.PlayerName ?? "Player";

        var application = new GuildApplication {
            ApplicationId = "app_" + GD.Hash(playerId + guildId).ToString(),
            GuildId = guildId,
            PlayerId = playerId,
            PlayerName = playerName,
            Message = message,
            PlayerLevel = player?.Level ?? 1,
            ApplyTime = DateTime.Now
        };

        MyApplications.Add(application);
        
        // TODO: 发送到服务器端
        
        GD.Print($"已申请加入公会: {guild.Name}");
        return true;
    }

    // 处理申请
    public bool ProcessApplication(string applicationId, bool accept) {
        if (!HasPermission(GuildPermission.Invite)) {
            GD.PrintErr("没有权限处理申请");
            return false;
        }

        var app = CurrentGuild.Applications?.Find(a => a.ApplicationId == applicationId);
        if (app == null) {
            GD.PrintErr("申请不存在");
            return false;
        }

        app.IsAccepted = accept;
        
        if (accept) {
            // 自动加入公会
            var member = new GuildMember {
                PlayerId = app.PlayerId,
                PlayerName = app.PlayerName,
                Level = GuildLevel.Recruit,
                Permissions = GuildPermission.Invite,
                JoinDate = DateTime.Now,
                LastActive = DateTime.Now,
                IsOnline = false
            };
            CurrentGuild.Members.Add(member);
            CurrentGuild.CurrentMembers++;
        }

        GD.Print($"申请处理: {(accept ? "接受" : "拒绝")} {app.PlayerName}");
        EmitSignal(SignalName.ApplicationProcessed, applicationId, accept);
        return true;
    }

    // 踢出成员
    public bool KickMember(string playerId) {
        if (!HasPermission(GuildPermission.Kick)) {
            GD.PrintErr("没有权限踢出成员");
            return false;
        }

        if (CurrentGuild == null) return false;

        var member = CurrentGuild.Members.Find(m => m.PlayerId == playerId);
        if (member == null) {
            GD.PrintErr("成员不存在");
            return false;
        }

        if (member.Level >= PlayerData.Level) {
            GD.PrintErr("不能踢出同级或更高职位的成员");
            return false;
        }

        CurrentGuild.Members.Remove(member);
        CurrentGuild.CurrentMembers--;

        GD.Print($"已将 {member.PlayerName} 踢出公会");
        EmitSignal(SignalName.MemberLeft, playerId);
        return true;
    }

    // 晋升成员
    public bool PromoteMember(string playerId) {
        if (!HasPermission(GuildPermission.Promote)) {
            GD.PrintErr("没有权限晋升成员");
            return false;
        }

        if (CurrentGuild == null) return false;

        var member = CurrentGuild.Members.Find(m => m.PlayerId == playerId);
        if (member == null) {
            GD.PrintErr("成员不存在");
            return false;
        }

        if (member.Level >= GuildLevel.ViceLeader) {
            GD.PrintErr("已达到最高职位");
            return false;
        }

        member.Level++;
        member.Permissions = GuildDatabase.GetLevelPermissions(member.Level);

        GD.Print($"已晋升 {member.PlayerName} 为 {GuildDatabase.GetLevelName(member.Level)}");
        return true;
    }

    // 降职成员
    public bool DemoteMember(string playerId) {
        if (!HasPermission(GuildPermission.Demote)) {
            GD.PrintErr("没有权限降职成员");
            return false;
        }

        if (CurrentGuild == null) return false;

        var member = CurrentGuild.Members.Find(m => m.PlayerId == playerId);
        if (member == null) {
            GD.PrintErr("成员不存在");
            return false;
        }

        if (member.Level <= GuildLevel.Recruit) {
            GD.PrintErr("已达到最低职位");
            return false;
        }

        if (member.Level >= PlayerData.Level) {
            GD.PrintErr("不能降职同级或更高职位的成员");
            return false;
        }

        member.Level--;
        member.Permissions = GuildDatabase.GetLevelPermissions(member.Level);

        GD.Print($"已降职 {member.PlayerName} 为 {GuildDatabase.GetLevelName(member.Level)}");
        return true;
    }

    // 转让会长
    public bool TransferLeadership(string playerId) {
        if (PlayerData.Level != GuildLevel.Leader) {
            GD.PrintErr("只有会长可以转让会长");
            return false;
        }

        if (CurrentGuild == null) return false;

        var newLeader = CurrentGuild.Members.Find(m => m.PlayerId == playerId);
        if (newLeader == null) {
            GD.PrintErr("成员不存在");
            return false;
        }

        var oldLeaderId = CurrentGuild.LeaderId;
        CurrentGuild.LeaderId = playerId;
        CurrentGuild.LeaderName = newLeader.PlayerName;

        // 降职原会长
        var oldLeader = CurrentGuild.Members.Find(m => m.PlayerId == oldLeaderId);
        if (oldLeader != null) {
            oldLeader.Level = GuildLevel.Elder;
            oldLeader.Permissions = GuildPermission.Elder;
        }

        // 晋升新会长
        newLeader.Level = GuildLevel.Leader;
        newLeader.Permissions = GuildPermission.All;

        GD.Print($"已将会长转让给 {newLeader.PlayerName}");
        return true;
    }

    // 贡献度
    public void AddContribution(int amount) {
        if (CurrentGuild == null) return;

        PlayerData.Contribution += amount;
        PlayerData.TotalContribution += amount;
        PlayerData.WeeklyContribution += amount;
        CurrentGuild.TotalContribution += amount;
        CurrentGuild.WeeklyContribution += amount;
        CurrentGuild.LastActivity = DateTime.Now;

        // 检查升级
        CheckLevelUp();

        GD.Print($"贡献度 +{amount}, 当前: {PlayerData.Contribution}");
        EmitSignal(SignalName.ContributionChanged, PlayerData.Contribution);
    }

    // 消耗贡献度
    public bool SpendContribution(int amount) {
        if (PlayerData.Contribution < amount) {
            GD.PrintErr("贡献度不足");
            return false;
        }

        PlayerData.Contribution -= amount;
        GD.Print($"消耗贡献度 {amount}, 剩余: {PlayerData.Contribution}");
        EmitSignal(SignalName.ContributionChanged, PlayerData.Contribution);
        return true;
    }

    // 检查升级
    private void CheckLevelUp() {
        if (CurrentGuild == null) return;

        int expNeeded = GuildDatabase.GetUpgradeExp(CurrentGuild.Level);
        while (CurrentGuild.Experience >= expNeeded && CurrentGuild.Level < 10) {
            CurrentGuild.Level++;
            CurrentGuild.MaxMembers = GuildDatabase.GetLevelMaxMembers(CurrentGuild.Level);
            GD.Print($"公会升级到 {CurrentGuild.Level} 级");
            EmitSignal(SignalName.GuildLevelUp, CurrentGuild.Level);
            expNeeded = GuildDatabase.GetUpgradeExp(CurrentGuild.Level);
        }
    }

    // 升级建筑
    public bool UpgradeBuilding(string buildingId) {
        if (!HasPermission(GuildPermission.UpgradeGuild)) {
            GD.PrintErr("没有权限升级建筑");
            return false;
        }

        if (CurrentGuild == null) return false;

        if (!CurrentGuild.Buildings.ContainsKey(buildingId)) {
            GD.PrintErr("建筑不存在");
            return false;
        }

        var building = CurrentGuild.Buildings[buildingId];
        if (building.Level >= building.MaxLevel) {
            GD.PrintErr("建筑已达最大等级");
            return false;
        }

        if (CurrentGuild.TotalContribution < building.UpgradeRequirement) {
            GD.PrintErr("公会贡献度不足");
            return false;
        }

        CurrentGuild.TotalContribution -= building.UpgradeRequirement;
        building.Level++;
        building.UpgradeCost = (int)(building.UpgradeCost * 1.5);
        building.UpgradeRequirement = (int)(building.UpgradeRequirement * 1.5);

        GD.Print($"建筑 {building.Name} 升级到 {building.Level} 级");
        return true;
    }

    // 学习技能
    public bool LearnSkill(string skillId) {
        if (!HasPermission(GuildPermission.UpgradeGuild)) {
            GD.PrintErr("没有权限学习技能");
            return false;
        }

        if (CurrentGuild == null) return false;

        if (!CurrentGuild.Skills.ContainsKey(skillId)) {
            GD.PrintErr("技能不存在");
            return false;
        }

        var skill = CurrentGuild.Skills[skillId];
        if (skill.Level >= skill.MaxLevel) {
            GD.PrintErr("技能已达最大等级");
            return false;
        }

        int cost = skill.CostPerLevel;
        if (CurrentGuild.TotalContribution < cost) {
            GD.PrintErr("公会贡献度不足");
            return false;
        }

        CurrentGuild.TotalContribution -= cost;
        skill.Level++;
        skill.IsUnlocked = true;

        GD.Print($"技能 {skill.Name} 升级到 {skill.Level} 级");
        return true;
    }

    // 更新公告
    public bool UpdateNotice(string notice) {
        if (!HasPermission(GuildPermission.ManageNotice)) {
            GD.PrintErr("没有权限管理公告");
            return false;
        }

        if (CurrentGuild == null) return false;

        CurrentGuild.Notice = notice;
        GD.Print($"公会公告已更新");
        return true;
    }

    // 发布公告
    public bool PostAnnouncement(string content) {
        if (!HasPermission(GuildPermission.ManageNotice)) {
            GD.PrintErr("没有权限发布公告");
            return false;
        }

        if (CurrentGuild == null) return false;

        var player = GetTree().CurrentScene.GetNode<Player>("Player");
        string playerName = player?.PlayerName ?? "Player";

        var announcement = new GuildAnnouncement {
            AnnouncementId = "ann_" + GD.Hash(content + DateTime.Now.ToString()).ToString(),
            Content = content,
            AuthorName = playerName,
            PostTime = DateTime.Now
        };

        Announcements.Insert(0, announcement);
        if (Announcements.Count > 20) {
            Announcements.RemoveAt(Announcements.Count - 1);
        }

        return true;
    }

    // 是否有权限
    public bool HasPermission(GuildPermission permission) {
        return (PlayerData.Permissions & permission) == permission;
    }

    // 获取公会加成
    public Dictionary<string, int> GetGuildBonuses() {
        var bonuses = new Dictionary<string, int>();

        if (CurrentGuild == null) return bonuses;

        // 建筑加成
        foreach (var building in CurrentGuild.Buildings.Values) {
            if (building.Level > 0) {
                foreach (var bonus in building.Bonuses) {
                    if (!bonuses.ContainsKey(bonus.Key)) bonuses[bonus.Key] = 0;
                    bonuses[bonus.Key] += bonus.Value * building.Level;
                }
            }
        }

        // 技能加成
        foreach (var skill in CurrentGuild.Skills.Values) {
            if (skill.Level > 0) {
                foreach (var bonus in skill.Bonuses) {
                    if (!bonuses.ContainsKey(bonus.Key)) bonuses[bonus.Key] = 0;
                    bonuses[bonus.Key] += bonus.Value * skill.Level;
                }
            }
        }

        return bonuses;
    }

    // 加载数据
    public void LoadData(Dictionary<string, object> data) {
        if (data == null) return;

        if (data.ContainsKey("player_data")) {
            var pd = data["player_data"] as Dictionary<string, object>;
            PlayerData.GuildId = pd.GetValueOrDefault("guild_id", "").ToString();
            PlayerData.Level = (GuildLevel)(int)pd.GetValueOrDefault("level", 0);
            PlayerData.Permissions = (GuildPermission)(int)pd.GetValueOrDefault("permissions", 0);
            PlayerData.Contribution = (int)pd.GetValueOrDefault("contribution", 0);
            PlayerData.TotalContribution = (int)pd.GetValueOrDefault("total_contribution", 0);
            PlayerData.WeeklyContribution = (int)pd.GetValueOrDefault("weekly_contribution", 0);
        }
    }

    // 保存数据
    public Dictionary<string, object> SaveData() {
        var data = new Dictionary<string, object>();
        
        var pd = new Dictionary<string, object> {
            { "guild_id", PlayerData.GuildId },
            { "level", (int)PlayerData.Level },
            { "permissions", (int)PlayerData.Permissions },
            { "contribution", PlayerData.Contribution },
            { "total_contribution", PlayerData.TotalContribution },
            { "weekly_contribution", PlayerData.WeeklyContribution }
        };
        data["player_data"] = pd;

        return data;
    }
}
