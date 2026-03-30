using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ClawRPG.Systems
{
/// <summary>
/// 分享模板管理器 - 负责生成分享文本和模板
/// </summary>
public class ShareTemplateManager : BaseSystem
{
    public static ShareTemplateManager Instance { get; private set; }

    // 分享数据
    public class ShareData
    {
        public string PlayerName;
        public int PlayerLevel;
        public int Score;
        public int EnemiesDefeated;
        public float DamageDealt;
        public float TimeSurvived;
        public string GameMode;
        public int Difficulty;
        public int GoldEarned;
        public int ExpGained;
        public List<string> Achievements;
        public string[] LootObtained;
        public string ScreenshotPath;
    }

    // 战绩模板
    public enum ShareTemplate
    {
        Simple,         // 简洁版
        Detailed,      // 详细版
        Achievements,   // 成就版
        Comparison      // 对比版
    }

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>
    /// 生成分享文本
    /// </summary>
    public string GenerateShareText(ShareData data, ShareTemplate template = ShareTemplate.Simple)
    {
        switch (template)
        {
            case ShareTemplate.Simple:
                return GenerateSimpleTemplate(data);
            case ShareTemplate.Detailed:
                return GenerateDetailedTemplate(data);
            case ShareTemplate.Achievements:
                return GenerateAchievementsTemplate(data);
            case ShareTemplate.Comparison:
                return GenerateComparisonTemplate(data);
            default:
                return GenerateSimpleTemplate(data);
        }
    }

    /// <summary>
    /// 简洁版模板
    /// </summary>
    private string GenerateSimpleTemplate(ShareData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("🎮 ClawRPG 战绩分享");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━");
        sb.AppendLine($"玩家: {data.PlayerName} (L{data.PlayerLevel})");
        sb.AppendLine($"模式: {data.GameMode} 难度:{data.Difficulty}");
        sb.AppendLine($"得分: {data.Score}");
        sb.AppendLine($"击败: {data.EnemiesDefeated} 敌人");
        
        if (data.TimeSurvived > 0)
        {
            sb.AppendLine($"存活: {FormatTime(data.TimeSurvived)}");
        }
        
        sb.AppendLine("━━━━━━━━━━━━━━━━━━");
        sb.AppendLine("#ClawRPG #战绩");
        
        return sb.ToString();
    }

    /// <summary>
    /// 详细版模板
    /// </summary>
    private string GenerateDetailedTemplate(ShareData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("⚔️ ClawRPG 详细战绩");
        sb.AppendLine("═══════════════════════════");
        sb.AppendLine($"👤 玩家: {data.PlayerName}");
        sb.AppendLine($"📊 等级: {data.PlayerLevel}");
        sb.AppendLine($"🎯 模式: {data.GameMode}");
        sb.AppendLine($"💎 难度: {GetDifficultyName(data.Difficulty)}");
        sb.AppendLine("");
        sb.AppendLine("【战斗数据】");
        sb.AppendLine($"  击败敌人: {data.EnemiesDefeated}");
        sb.AppendLine($"  造成伤害: {FormatNumber(data.DamageDealt)}");
        
        if (data.TimeSurvived > 0)
        {
            sb.AppendLine($"  存活时间: {FormatTime(data.TimeSurvived)}");
        }
        
        sb.AppendLine("");
        sb.AppendLine("【收益】");
        sb.AppendLine($"  💰 金币: +{data.GoldEarned}");
        sb.AppendLine($"  ✨ 经验: +{data.ExpGained}");
        sb.AppendLine($"  🏆 得分: {data.Score}");
        
        if (data.LootObtained != null && data.LootObtained.Length > 0)
        {
            sb.AppendLine("");
            sb.AppendLine("【战利品】");
            foreach (var item in data.LootObtained)
            {
                sb.AppendLine($"  • {item}");
            }
        }
        
        sb.AppendLine("═══════════════════════════");
        sb.AppendLine("#ClawRPG #战绩 #游戏");
        
        return sb.ToString();
    }

    /// <summary>
    /// 成就版模板
    /// </summary>
    private string GenerateAchievementsTemplate(ShareData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("🏆 ClawRPG 成就解锁!");
        sb.AppendLine("⭐━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine($"恭喜 {data.PlayerName} 完成挑战!");
        sb.AppendLine("");
        sb.AppendLine($"📊 最终得分: {data.Score}");
        
        if (data.Achievements != null && data.Achievements.Count > 0)
        {
            sb.AppendLine("");
            sb.AppendLine("【解锁成就】");
            foreach (var achievement in data.Achievements)
            {
                sb.AppendLine($"  ✅ {achievement}");
            }
        }
        
        if (data.EnemiesDefeated >= 100)
        {
            sb.AppendLine($"  ✅ 击杀达人: 击败 {data.EnemiesDefeated} 敌人");
        }
        
        if (data.DamageDealt >= 10000)
        {
            sb.AppendLine($"  ✅ 伤害王者: 造成 {FormatNumber(data.DamageDealt)} 伤害");
        }
        
        if (data.TimeSurvived >= 600)
        {
            sb.AppendLine($"  ✅ 生存大师: 存活 {FormatTime(data.TimeSurvived)}");
        }
        
        sb.AppendLine("⭐━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine("#ClawRPG #成就 #MVP");
        
        return sb.ToString();
    }

    /// <summary>
    /// 对比版模板
    /// </summary>
    private string GenerateComparisonTemplate(ShareData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("📈 ClawRPG 数据对比");
        sb.AppendLine("▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔");
        sb.AppendLine($"玩家: {data.PlayerName}");
        sb.AppendLine($"等级: {data.PlayerLevel} → {data.PlayerLevel + CalculateLevelGain(data.ExpGained)}");
        sb.AppendLine("");
        sb.AppendLine("本次战斗:");
        sb.AppendLine($"  伤害: {FormatNumber(data.DamageDealt)}");
        sb.AppendLine($"  击杀: {data.EnemiesDefeated}");
        sb.AppendLine($"  收益: {data.GoldEarned}💰 / {data.ExpGained}✨");
        sb.AppendLine("");
        sb.AppendLine("▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔");
        sb.AppendLine("#ClawRPG #成长");
        
        return sb.ToString();
    }

    /// <summary>
    /// 生成群聊战绩卡片
    /// </summary>
    public string GenerateGroupCard(ShareData data)
    {
        var sb = new StringBuilder();
        
        // QQ/微信卡片格式
        sb.AppendLine("[CQ:card,type=game,data=");
        sb.AppendLine("{");
        sb.AppendLine($"  \"title\":\"ClawRPG 战绩\",");
        sb.AppendLine($"  \"description\":\"{data.PlayerName} - L{data.PlayerLevel}\",");
        sb.AppendLine($"  \"url\":\"clawrpg://share/{data.Score}\",");
        sb.AppendLine($"  \"preview\":\"{(data.ScreenshotPath ?? "")}\"");
        sb.AppendLine("}]");
        
        return sb.ToString();
    }

    /// <summary>
    /// 生成战绩JSON(用于机器人发送)
    /// </summary>
    public string GenerateShareJson(ShareData data)
    {
        var dict = new Dictionary<string, object>
        {
            { "type", "share_result" },
            { "player_name", data.PlayerName },
            { "player_level", data.PlayerLevel },
            { "score", data.Score },
            { "enemies_defeated", data.EnemiesDefeated },
            { "damage_dealt", data.DamageDealt },
            { "time_survived", data.TimeSurvived },
            { "game_mode", data.GameMode },
            { "difficulty", data.Difficulty },
            { "gold_earned", data.GoldEarned },
            { "exp_gained", data.ExpGained },
            { "timestamp", DateTimeOffset.Now.ToUnixTimeMilliseconds() }
        };
        
        if (data.Achievements != null)
        {
            dict["achievements"] = data.Achievements;
        }
        
        return JsonSerializer.Serialize(dict);
    }

    #region Helper Methods

    /// <summary>
    /// 格式化数字
    /// </summary>
    private string FormatNumber(float num)
    {
        if (num >= 1000000)
            return (num / 1000000).ToString("F1") + "M";
        if (num >= 1000)
            return (num / 1000).ToString("F1") + "K";
        return num.ToString("F0");
    }

    /// <summary>
    /// 格式化时间
    /// </summary>
    private string FormatTime(float seconds)
    {
        int hours = (int)(seconds / 3600);
        int minutes = (int)((seconds % 3600) / 60);
        int secs = (int)(seconds % 60);
        
        if (hours > 0)
            return $"{hours}h {minutes}m {secs}s";
        if (minutes > 0)
            return $"{minutes}m {secs}s";
        return $"{secs}s";
    }

    /// <summary>
    /// 获取难度名称
    /// </summary>
    private string GetDifficultyName(int difficulty)
    {
        switch (difficulty)
        {
            case 1: return "简单";
            case 2: return "普通";
            case 3: return "困难";
            case 4: return "专家";
            case 5: return "噩梦";
            default: return "未知";
        }
    }

    /// <summary>
    /// 计算升级
    /// </summary>
    private int CalculateLevelGain(int exp)
    {
        // 简单模拟: 每1000经验升一级
        return exp / 1000;
    }

    #endregion

    public override void _ExitTree()
    {
        Instance = null;
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary<string, object>();
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        // No persistent data needed
    }
}
}
