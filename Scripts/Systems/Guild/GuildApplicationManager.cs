using Godot;
using System;
using System.Collections.Generic;
using GameSystems;

/// <summary>
/// 公会申请管理器
/// 负责申请加入公会、处理申请等
/// </summary>
public partial class GuildApplicationManager
{
    private readonly List<GuildApplication> _myApplications = new List<GuildApplication>();

    /// <summary>
    /// 所有申请（玩家发出的）
    /// </summary>
    public List<GuildApplication> MyApplications => _myApplications;

    /// <summary>
    /// 申请加入公会
    /// </summary>
    /// <param name="guildId">目标公会ID</param>
    /// <param name="message">申请留言</param>
    /// <returns>是否申请成功</returns>
    public bool ApplyToGuild(string guildId, string message = "")
    {
        var playerData = GuildSystem.Instance.PlayerData;
        if (playerData.GuildId != "")
        {
            GD.PrintErr("玩家已有公会");
            return false;
        }

        var guild = FindGuild(guildId);
        if (guild == null)
        {
            GD.PrintErr("公会不存在: " + guildId);
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

        _myApplications.Add(application);
        GD.Print($"已申请加入公会: {guild.Name}");
        return true;
    }

    /// <summary>
    /// 处理申请（accept=接受=加入公会，reject=拒绝）
    /// </summary>
    /// <param name="applicationId">申请ID</param>
    /// <param name="accept">true=接受 false=拒绝</param>
    /// <returns>是否处理成功</returns>
    public bool ProcessApplication(string applicationId, bool accept)
    {
        if (!GuildSystem.Instance.HasPermission(GuildPermission.Invite))
        {
            GD.PrintErr("没有权限处理申请");
            return false;
        }

        var currentGuild = GuildSystem.Instance.CurrentGuild;
        if (currentGuild == null)
        {
            GD.PrintErr("当前不在任何公会");
            return false;
        }

        var app = currentGuild.Applications?.Find(a => a.ApplicationId == applicationId);
        if (app == null)
        {
            GD.PrintErr("申请不存在: " + applicationId);
            return false;
        }

        app.IsAccepted = accept;

        if (accept)
        {
            var member = new GuildMember {
                PlayerId = app.PlayerId,
                PlayerName = app.PlayerName,
                Level = GuildLevel.Recruit,
                Permissions = GuildPermission.Invite,
                JoinDate = DateTime.Now,
                LastActive = DateTime.Now,
                IsOnline = false
            };
            currentGuild.Members.Add(member);
            currentGuild.CurrentMembers++;
        }

        GD.Print($"申请处理: {(accept ? "接受" : "拒绝")} {app.PlayerName}");
        GuildSystem.Instance.EmitSignal(GuildSystem.SignalName.ApplicationProcessed, applicationId, accept);
        return true;
    }

    /// <summary>
    /// 查找公会
    /// </summary>
    private GuildData FindGuild(string guildId)
    {
        foreach (var guild in GuildSystem.Instance.AvailableGuilds)
            if (guild.GuildId == guildId) return guild;
        var current = GuildSystem.Instance.CurrentGuild;
        if (current != null && current.GuildId == guildId) return current;
        return null;
    }

    /// <summary>
    /// 清空所有申请
    /// </summary>
    public void Clear()
    {
        _myApplications.Clear();
    }

    /// <summary>
    /// 从保存数据恢复
    /// </summary>
    public void FromSaveData(List<Dictionary> data)
    {
        _myApplications.Clear();
        if (data == null) return;

        foreach (var dict in data)
        {
            var app = new GuildApplication {
                ApplicationId = dict.Contains("application_id") ? dict["application_id"].ToString() : "",
                GuildId = dict.Contains("guild_id") ? dict["guild_id"].ToString() : "",
                PlayerId = dict.Contains("player_id") ? dict["player_id"].ToString() : "",
                PlayerName = dict.Contains("player_name") ? dict["player_name"].ToString() : "",
                Message = dict.Contains("message") ? dict["message"].ToString() : "",
                PlayerLevel = dict.Contains("player_level") ? (int)dict["player_level"] : 1,
                IsAccepted = dict.Contains("is_accepted") && dict["is_accepted"] is bool b && b
            };
            if (dict.Contains("apply_time") && !string.IsNullOrEmpty(dict["apply_time"].ToString()))
            {
                try { app.ApplyTime = DateTime.Parse(dict["apply_time"].ToString()); }
                catch { app.ApplyTime = DateTime.Now; }
            }
            _myApplications.Add(app);
        }
    }

    /// <summary>
    /// 导出为保存数据格式
    /// </summary>
    public List<Dictionary> ToSaveData()
    {
        var result = new List<Dictionary>();
        foreach (var app in _myApplications)
        {
            result.Add(new Dictionary {
                ["application_id"] = app.ApplicationId,
                ["guild_id"] = app.GuildId,
                ["player_id"] = app.PlayerId,
                ["player_name"] = app.PlayerName,
                ["message"] = app.Message,
                ["player_level"] = app.PlayerLevel,
                ["is_accepted"] = app.IsAccepted,
                ["apply_time"] = app.ApplyTime.ToString("o")
            });
        }
        return result;
    }
}
