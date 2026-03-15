using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// 天气类型枚举
    /// </summary>
    public enum WeatherType
    {
        Clear,       // 晴朗
        Cloudy,      // 多云
        Rain,        // 小雨
        HeavyRain,   // 大雨
        Storm,       // 暴风雨
        Snow,        // 小雪
        Blizzard,    // 暴风雪
        Fog,        // 雾
        Windy,       // 大风
        Sandstorm    // 沙尘暴
    }
    
    /// <summary>
    /// 天气强度等级
    /// </summary>
    public enum WeatherIntensity
    {
        Calm,      // 平静
        Light,     // 轻度
        Moderate,  // 中度
        Severe,    // 剧烈
        Extreme    // 极端
    }
    
    /// <summary>
    /// 天气效果类型
    /// </summary>
    public enum WeatherEffectType
    {
        VisionReduction,   // 视野降低
        MovementSpeed,     // 移动速度
        AttackSpeed,       // 攻击速度
        Defense,           // 防御力
        HealthRegen,      // 生命恢复
        ManaRegen,        // 法力恢复
        CriticalRate,     // 暴击率
        DodgeRate,         // 闪避率
        ExperienceGain,   // 经验获取
        ItemDropRate      // 物品掉落率
    }
    
    /// <summary>
    /// 天气效果数据
    /// </summary>
    [System.Serializable]
    public class WeatherEffect
    {
        public WeatherEffectType Type;
        public float Value;  // 百分比，正数为增益，负数为减益
        public string Description;
    }
    
    /// <summary>
    /// 单一天气配置
    /// </summary>
    [System.Serializable]
    public class WeatherConfig
    {
        public WeatherType Type;
        public string Name;
        public string Description;
        public WeatherIntensity DefaultIntensity;
        public float Duration;          // 持续时间（分钟）
        public float TransitionTime;    // 过渡时间（秒）
        public List<WeatherEffect> Effects = new List<WeatherEffect>();
        public string Icon;              // 图标名称
        public string ParticleEffect;   // 粒子效果
    }
    
    /// <summary>
    /// 区域天气数据
    /// </summary>
    [System.Serializable]
    public class ZoneWeather
    {
        public string ZoneId;
        public string ZoneName;
        public WeatherType CurrentWeather;
        public WeatherIntensity Intensity;
        public float RemainingTime;     // 剩余时间（秒）
        public float TransitionProgress; // 过渡进度 (0-1)
        public bool IsTransitioning;
        public DateTime LastUpdate;
    }
    
    /// <summary>
    /// 玩家天气抗性数据
    /// </summary>
    [System.Serializable]
    public class WeatherResistance
    {
        public WeatherType Type;
        public float Resistance;  // 0-100% 抗性
        public DateTime LastExposure;  // 上次暴露时间
    }
    
    /// <summary>
    /// 玩家天气状态
    /// </summary>
    [System.Serializable]
    public class PlayerWeatherState
    {
        public string PlayerId;
        public List<WeatherResistance> Resistances = new List<WeatherResistance>();
        public int TotalExposureTime;   // 总暴露时间（秒）
        public Dictionary<WeatherType, int> WeatherEvents = new Dictionary<WeatherType, int>();
        public DateTime LastWeatherCheck;
    }
    
    /// <summary>
    /// 天气事件记录
    /// </summary>
    [System.Serializable]
    public class WeatherEvent
    {
        public string EventId;
        public string ZoneId;
        public WeatherType Weather;
        public WeatherIntensity Intensity;
        public DateTime StartTime;
        public DateTime EndTime;
        public int AffectedPlayers;
    }
    
    /// <summary>
    /// 天气统计
    /// </summary>
    [System.Serializable]
    public class WeatherStatistics
    {
        public int TotalWeatherEvents;
        public Dictionary<WeatherType, int> WeatherCounts = new Dictionary<WeatherType, int>();
        public Dictionary<WeatherType, float> AverageDuration = new Dictionary<WeatherType, float>();
        public int PlayerWeatherEvents;
    }
    
    /// <summary>
    /// 季节类型
    /// </summary>
    public enum Season
    {
        Spring,  // 春季
        Summer,  // 夏季
        Autumn,  // 秋季
        Winter   // 冬季
    }
    
    /// <summary>
    /// 季节天气配置
    /// </summary>
    [System.Serializable]
    public class SeasonConfig
    {
        public Season Season;
        public List<WeatherType> CommonWeathers = new List<WeatherType>();
        public List<WeatherType> RareWeathers = new List<WeatherType>();
        public float Temperature;  // 温度
        public float DayLength;    // 白天时长比例
    }
}
