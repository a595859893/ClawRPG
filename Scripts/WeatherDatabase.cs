using System;
using System.Collections.Generic;
using ClawRPG.Scripts;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// 天气系统配置数据库
    /// </summary>
    public static class WeatherDatabase
    {
        private static Dictionary<WeatherType, WeatherConfig> _weatherConfigs;
        private static Dictionary<Season, SeasonConfig> _seasonConfigs;
        private static Dictionary<string, List<WeatherType>> _zoneWeatherTypes;
        
        public static void Initialize()
        {
            _weatherConfigs = new Dictionary<WeatherType, WeatherConfig>();
            _seasonConfigs = new Dictionary<Season, SeasonConfig>();
            _zoneWeatherTypes = new Dictionary<string, List<WeatherType>>();
            
            InitializeWeatherConfigs();
            InitializeSeasonConfigs();
            InitializeZoneWeatherTypes();
        }
        
        private static void InitializeWeatherConfigs()
        {
            // 晴朗天气
            var clear = new WeatherConfig
            {
                Type = WeatherType.Clear,
                Name = "晴朗",
                Description = "阳光明媚的天气，所有属性正常",
                DefaultIntensity = WeatherIntensity.Calm,
                Duration = 30f,
                TransitionTime = 10f,
                Icon = "sunny",
                ParticleEffect = "sun_rays",
                Effects = new List<WeatherEffect>
                {
                    new WeatherEffect { Type = WeatherEffectType.ExperienceGain, Value = 10f, Description = "经验获取+10%" },
                    new WeatherEffect { Type = WeatherEffectType.ItemDropRate, Value = 5f, Description = "物品掉落+5%" }
                }
            };
            _weatherConfigs[WeatherType.Clear] = clear;
            
            // 多云天气
            var cloudy = new WeatherConfig
            {
                Type = WeatherType.Cloudy,
                Name = "多云",
                Description = "天空被云层覆盖，视野略有下降",
                DefaultIntensity = WeatherIntensity.Light,
                Duration = 25f,
                TransitionTime = 15f,
                Icon = "cloudy",
                ParticleEffect = "clouds",
                Effects = new List<WeatherEffect>
                {
                    new WeatherEffect { Type = WeatherEffectType.VisionReduction, Value = -10f, Description = "视野-10%" },
                    new WeatherEffect { Type = WeatherEffectType.CriticalRate, Value = 5f, Description = "暴击率+5%" }
                }
            };
            _weatherConfigs[WeatherType.Cloudy] = cloudy;
            
            // 小雨天气
            var rain = new WeatherConfig
            {
                Type = WeatherType.Rain,
                Name = "小雨",
                Description = "细雨绵绵，地面湿滑",
                DefaultIntensity = WeatherIntensity.Light,
                Duration = 20f,
                TransitionTime = 20f,
                Icon = "rain",
                ParticleEffect = "light_rain",
                Effects = new List<WeatherEffect>
                {
                    new WeatherEffect { Type = WeatherEffectType.MovementSpeed, Value = -5f, Description = "移动速度-5%" },
                    new WeatherEffect { Type = WeatherEffectType.AttackSpeed, Value = 5f, Description = "攻击速度+5%" },
                    new WeatherEffect { Type = WeatherEffectType.HealthRegen, Value = 10f, Description = "生命恢复+10%" }
                }
            };
            _weatherConfigs[WeatherType.Rain] = rain;
            
            // 大雨天气
            var heavyRain = new WeatherConfig
            {
                Type = WeatherType.HeavyRain,
                Name = "大雨",
                Description = "倾盆大雨，视野严重下降",
                DefaultIntensity = WeatherIntensity.Moderate,
                Duration = 15f,
                TransitionTime = 25f,
                Icon = "heavy_rain",
                ParticleEffect = "heavy_rain",
                Effects = new List<WeatherEffect>
                {
                    new WeatherEffect { Type = WeatherEffectType.VisionReduction, Value = -25f, Description = "视野-25%" },
                    new WeatherEffect { Type = WeatherEffectType.MovementSpeed, Value = -15f, Description = "移动速度-15%" },
                    new WeatherEffect { Type = WeatherEffectType.Defense, Value = 10f, Description = "防御力+10%" },
                    new WeatherEffect { Type = WeatherEffectType.ManaRegen, Value = 15f, Description = "法力恢复+15%" }
                }
            };
            _weatherConfigs[WeatherType.HeavyRain] = heavyRain;
            
            // 暴风雨天气
            var storm = new WeatherConfig
            {
                Type = WeatherType.Storm,
                Name = "暴风雨",
                Description = "雷电交加，危险至极",
                DefaultIntensity = WeatherIntensity.Severe,
                Duration = 10f,
                TransitionTime = 30f,
                Icon = "storm",
                ParticleEffect = "storm",
                Effects = new List<WeatherEffect>
                {
                    new WeatherEffect { Type = WeatherEffectType.VisionReduction, Value = -40f, Description = "视野-40%" },
                    new WeatherEffect { Type = WeatherEffectType.MovementSpeed, Value = -25f, Description = "移动速度-25%" },
                    new WeatherEffect { Type = WeatherEffectType.DodgeRate, Value = 15f, Description = "闪避率+15%" },
                    new WeatherEffect { Type = WeatherEffectType.CriticalRate, Value = 10f, Description = "暴击率+10%" }
                }
            };
            _weatherConfigs[WeatherType.Storm] = storm;
            
            // 小雪天气
            var snow = new WeatherConfig
            {
                Type = WeatherType.Snow,
                Name = "小雪",
                Description = "雪花飘飘，地面湿滑",
                DefaultIntensity = WeatherIntensity.Light,
                Duration = 20f,
                TransitionTime = 20f,
                Icon = "snow",
                ParticleEffect = "snow",
                Effects = new List<WeatherEffect>
                {
                    new WeatherEffect { Type = WeatherEffectType.MovementSpeed, Value = -10f, Description = "移动速度-10%" },
                    new WeatherEffect { Type = WeatherEffectType.Defense, Value = 5f, Description = "防御力+5%" }
                }
            };
            _weatherConfigs[WeatherType.Snow] = snow;
            
            // 暴风雪天气
            var blizzard = new WeatherConfig
            {
                Type = WeatherType.Blizzard,
                Name = "暴风雪",
                Description = "暴雪狂风，极端寒冷",
                DefaultIntensity = WeatherIntensity.Extreme,
                Duration = 8f,
                TransitionTime = 35f,
                Icon = "blizzard",
                ParticleEffect = "blizzard",
                Effects = new List<WeatherEffect>
                {
                    new WeatherEffect { Type = WeatherEffectType.VisionReduction, Value = -50f, Description = "视野-50%" },
                    new WeatherEffect { Type = WeatherEffectType.MovementSpeed, Value = -35f, Description = "移动速度-35%" },
                    new WeatherEffect { Type = WeatherEffectType.AttackSpeed, Value = -20f, Description = "攻击速度-20%" },
                    new WeatherEffect { Type = WeatherEffectType.HealthRegen, Value = -20f, Description = "生命恢复-20%" }
                }
            };
            _weatherConfigs[WeatherType.Blizzard] = blizzard;
            
            // 雾天气
            var fog = new WeatherConfig
            {
                Type = WeatherType.Fog,
                Name = "大雾",
                Description = "浓雾弥漫，视野模糊",
                DefaultIntensity = WeatherIntensity.Moderate,
                Duration = 15f,
                TransitionTime = 25f,
                Icon = "fog",
                ParticleEffect = "fog",
                Effects = new List<WeatherEffect>
                {
                    new WeatherEffect { Type = WeatherEffectType.VisionReduction, Value = -35f, Description = "视野-35%" },
                    new WeatherEffect { Type = WeatherEffectType.DodgeRate, Value = 20f, Description = "闪避率+20%" }
                }
            };
            _weatherConfigs[WeatherType.Fog] = fog;
            
            // 大风天气
            var windy = new WeatherConfig
            {
                Type = WeatherType.Windy,
                Name = "大风",
                Description = "狂风呼啸，影响远程攻击",
                DefaultIntensity = WeatherIntensity.Moderate,
                Duration = 18f,
                TransitionTime = 15f,
                Icon = "windy",
                ParticleEffect = "wind",
                Effects = new List<WeatherEffect>
                {
                    new WeatherEffect { Type = WeatherEffectType.MovementSpeed, Value = 10f, Description = "移动速度+10%" },
                    new WeatherEffect { Type = WeatherEffectType.AttackSpeed, Value = -10f, Description = "攻击速度-10%" }
                }
            };
            _weatherConfigs[WeatherType.Windy] = windy;
            
            // 沙尘暴天气
            var sandstorm = new WeatherConfig
            {
                Type = WeatherType.Sandstorm,
                Name = "沙尘暴",
                Description = "黄沙漫天，视野极差",
                DefaultIntensity = WeatherIntensity.Severe,
                Duration = 12f,
                TransitionTime = 30f,
                Icon = "sandstorm",
                ParticleEffect = "sandstorm",
                Effects = new List<WeatherEffect>
                {
                    new WeatherEffect { Type = WeatherEffectType.VisionReduction, Value = -45f, Description = "视野-45%" },
                    new WeatherEffect { Type = WeatherEffectType.Defense, Value = -10f, Description = "防御力-10%" },
                    new WeatherEffect { Type = WeatherEffectType.CriticalRate, Value = 15f, Description = "暴击率+15%" }
                }
            };
            _weatherConfigs[WeatherType.Sandstorm] = sandstorm;
        }
        
        private static void InitializeSeasonConfigs()
        {
            // 春季 - 多雨
            _seasonConfigs[Season.Spring] = new SeasonConfig
            {
                Season = Season.Spring,
                CommonWeathers = new List<WeatherType> { WeatherType.Clear, WeatherType.Cloudy, WeatherType.Rain },
                RareWeathers = new List<WeatherType> { WeatherType.Storm, WeatherType.Fog },
                Temperature = 15f,
                DayLength = 1.0f
            };
            
            // 夏季 - 晴朗
            _seasonConfigs[Season.Summer] = new SeasonConfig
            {
                Season = Season.Summer,
                CommonWeathers = new List<WeatherType> { WeatherType.Clear, WeatherType.Cloudy },
                RareWeathers = new List<WeatherType> { WeatherType.Storm, WeatherType.HeavyRain },
                Temperature = 28f,
                DayLength = 1.2f
            };
            
            // 秋季 - 多风
            _seasonConfigs[Season.Autumn] = new SeasonConfig
            {
                Season = Season.Autumn,
                CommonWeathers = new List<WeatherType> { WeatherType.Clear, WeatherType.Cloudy, WeatherType.Windy },
                RareWeathers = new List<WeatherType> { WeatherType.Rain, WeatherType.Fog },
                Temperature = 18f,
                DayLength = 0.9f
            };
            
            // 冬季 - 寒冷
            _seasonConfigs[Season.Winter] = new SeasonConfig
            {
                Season = Season.Winter,
                CommonWeathers = new List<WeatherType> { WeatherType.Clear, WeatherType.Cloudy, WeatherType.Snow },
                RareWeathers = new List<WeatherType> { WeatherType.Blizzard, WeatherType.Fog },
                Temperature = 2f,
                DayLength = 0.8f
            };
        }
        
        private static void InitializeZoneWeatherTypes()
        {
            // 森林区域
            _zoneWeatherTypes["forest"] = new List<WeatherType> 
            { 
                WeatherType.Clear, WeatherType.Cloudy, WeatherType.Rain, 
                WeatherType.Fog, WeatherType.Windy 
            };
            
            // 沙漠区域
            _zoneWeatherTypes["desert"] = new List<WeatherType> 
            { 
                WeatherType.Clear, WeatherType.Cloudy, WeatherType.Sandstorm, 
                WeatherType.Windy, WeatherType.HeavyRain 
            };
            
            // 雪山区域
            _zoneWeatherTypes["mountain"] = new List<WeatherType> 
            { 
                WeatherType.Clear, WeatherType.Cloudy, WeatherType.Snow, 
                WeatherType.Blizzard, WeatherType.Fog 
            };
            
            // 平原区域
            _zoneWeatherTypes["plains"] = new List<WeatherType> 
            { 
                WeatherType.Clear, WeatherType.Cloudy, WeatherType.Rain, 
                WeatherType.Windy, WeatherType.Storm 
            };
            
            // 湖泊区域
            _zoneWeatherTypes["lake"] = new List<WeatherType> 
            { 
                WeatherType.Clear, WeatherType.Cloudy, WeatherType.Rain, 
                WeatherType.Fog, WeatherType.Storm 
            };
            
            // 默认区域
            _zoneWeatherTypes["default"] = new List<WeatherType> 
            { 
                WeatherType.Clear, WeatherType.Cloudy, WeatherType.Rain 
            };
        }
        
        public static WeatherConfig GetWeatherConfig(WeatherType type)
        {
            return _weatherConfigs.ContainsKey(type) ? _weatherConfigs[type] : null;
        }
        
        public static SeasonConfig GetSeasonConfig(Season season)
        {
            return _seasonConfigs.ContainsKey(season) ? _seasonConfigs[season] : null;
        }
        
        public static List<WeatherType> GetZoneWeatherTypes(string zoneId)
        {
            string key = zoneId.ToLower();
            if (_zoneWeatherTypes.ContainsKey(key))
                return _zoneWeatherTypes[key];
            return _zoneWeatherTypes["default"];
        }
        
        public static List<WeatherConfig> GetAllWeatherConfigs()
        {
            return new List<WeatherConfig>(_weatherConfigs.Values);
        }
        
        public static List<SeasonConfig> GetAllSeasonConfigs()
        {
            return new List<SeasonConfig>(_seasonConfigs.Values);
        }
        
        public static Season GetCurrentSeason()
        {
            // 基于月份确定季节
            int month = DateTime.Now.Month;
            if (month >= 3 && month <= 5) return Season.Spring;
            if (month >= 6 && month <= 8) return Season.Summer;
            if (month >= 9 && month <= 11) return Season.Autumn;
            return Season.Winter;
        }
    }
}
