namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 合作会话配置
    /// </summary>
    public class CoopSessionConfig
    {
        // 标准配置
        public static readonly CoopSessionConfig Standard = new CoopSessionConfig
        {
            MaxPlayers = 4,
            IsQuickMode = false,
            TimeLimitMinutes = 60,
            ExpMultiplier = 1.0f,
            DropRateMultiplier = 1.0f
        };
        
        // 快速模式配置
        public static readonly CoopSessionConfig QuickMode = new CoopSessionConfig
        {
            MaxPlayers = 4,
            IsQuickMode = true,
            TimeLimitMinutes = 20,
            ExpMultiplier = 1.5f,
            DropRateMultiplier = 1.2f
        };
        
        // 双人模式配置
        public static readonly CoopSessionConfig Duo = new CoopSessionConfig
        {
            MaxPlayers = 2,
            IsQuickMode = false,
            TimeLimitMinutes = 45,
            ExpMultiplier = 1.2f,
            DropRateMultiplier = 1.1f
        };
        
        // 团队模式配置
        public static readonly CoopSessionConfig Raid = new CoopSessionConfig
        {
            MaxPlayers = 8,
            IsQuickMode = false,
            TimeLimitMinutes = 90,
            ExpMultiplier = 1.5f,
            DropRateMultiplier = 1.5f
        };
        
        public int MaxPlayers { get; set; }
        public bool IsQuickMode { get; set; }
        public int TimeLimitMinutes { get; set; }
        public float ExpMultiplier { get; set; }
        public float DropRateMultiplier { get; set; }
    }
}
