using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
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
        HeavySnow,   // 大雪
        Fog,         // 雾
        Sandstorm,   // 沙尘暴
        Night        // 夜晚
    }

    /// <summary>
    /// 天气强度
    /// </summary>
    public enum WeatherIntensity
    {
        Light,   // 轻微
        Normal,  // 普通
        Heavy    // 强烈
    }

    /// <summary>
    /// 天气数据类
    /// </summary>
    public class WeatherData
    {
        public WeatherType Type { get; set; }
        public WeatherIntensity Intensity { get; set; }
        public float Duration { get; set; }           // 持续时间（秒）
        public float RemainingTime { get; set; }      // 剩余时间
        public float DamageMultiplier { get; set; }   // 伤害倍率
        public float DefenseMultiplier { get; set; } // 防御倍率
        public float ExperienceMultiplier { get; set; } // 经验倍率
        public float DropMultiplier { get; set; }     // 掉落倍率
        public float VisibilityRadius { get; set; }   // 视野半径
        public string EffectColor { get; set; }       // 效果颜色

        public WeatherData()
        {
            Type = WeatherType.Clear;
            Intensity = WeatherIntensity.Normal;
            Duration = 300f;
            RemainingTime = 300f;
            DamageMultiplier = 1.0f;
            DefenseMultiplier = 1.0f;
            ExperienceMultiplier = 1.0f;
            DropMultiplier = 1.0f;
            VisibilityRadius = 500f;
            EffectColor = "#FFFFFF";
        }
    }

    /// <summary>
    /// 天气数据库
    /// </summary>
    public class WeatherDatabase
    {
        private static WeatherDatabase _instance;
        public static WeatherDatabase Instance => _instance ??= new WeatherDatabase();

        private Dictionary<WeatherType, Dictionary<WeatherIntensity, WeatherData>> _weatherTemplates;

        public WeatherDatabase()
        {
            _weatherTemplates = new Dictionary<WeatherType, Dictionary<WeatherIntensity, WeatherData>>();
            InitializeWeatherTemplates();
        }

        private void InitializeWeatherTemplates()
        {
            // 晴朗
            _weatherTemplates[WeatherType.Clear] = new Dictionary<WeatherIntensity, WeatherData>
            {
                { WeatherIntensity.Light, new WeatherData { Type = WeatherType.Clear, Intensity = WeatherIntensity.Light, Duration = 600f, ExperienceMultiplier = 1.1f, VisibilityRadius = 600f, EffectColor = "#FFD700" } },
                { WeatherIntensity.Normal, new WeatherData { Type = WeatherType.Clear, Intensity = WeatherIntensity.Normal, Duration = 600f, ExperienceMultiplier = 1.2f, VisibilityRadius = 600f, EffectColor = "#FFD700" } },
                { WeatherIntensity.Heavy, new WeatherData { Type = WeatherType.Clear, Intensity = WeatherIntensity.Heavy, Duration = 600f, ExperienceMultiplier = 1.3f, VisibilityRadius = 600f, EffectColor = "#FFD700" } }
            };

            // 多云
            _weatherTemplates[WeatherType.Cloudy] = new Dictionary<WeatherIntensity, WeatherData>
            {
                { WeatherIntensity.Light, new WeatherData { Type = WeatherType.Cloudy, Intensity = WeatherIntensity.Light, Duration = 400f, DamageMultiplier = 1.05f, VisibilityRadius = 550f, EffectColor = "#A9A9A9" } },
                { WeatherIntensity.Normal, new WeatherData { Type = WeatherType.Cloudy, Intensity = WeatherIntensity.Normal, Duration = 400f, DamageMultiplier = 1.1f, VisibilityRadius = 500f, EffectColor = "#808080" } },
                { WeatherIntensity.Heavy, new WeatherData { Type = WeatherType.Cloudy, Intensity = WeatherIntensity.Heavy, Duration = 400f, DamageMultiplier = 1.15f, VisibilityRadius = 450f, EffectColor = "#696969" } }
            };

            // 小雨
            _weatherTemplates[WeatherType.Rain] = new Dictionary<WeatherIntensity, WeatherData>
            {
                { WeatherIntensity.Light, new WeatherData { Type = WeatherType.Rain, Intensity = WeatherIntensity.Light, Duration = 300f, DefenseMultiplier = 1.1f, VisibilityRadius = 500f, EffectColor = "#4169E1" } },
                { WeatherIntensity.Normal, new WeatherData { Type = WeatherType.Rain, Intensity = WeatherIntensity.Normal, Duration = 300f, DefenseMultiplier = 1.15f, VisibilityRadius = 450f, EffectColor = "#1E90FF" } },
                { WeatherIntensity.Heavy, new WeatherData { Type = WeatherType.Rain, Intensity = WeatherIntensity.Heavy, Duration = 300f, DefenseMultiplier = 1.2f, VisibilityRadius = 400f, EffectColor = "#0000CD" } }
            };

            // 大雨
            _weatherTemplates[WeatherType.HeavyRain] = new Dictionary<WeatherIntensity, WeatherData>
            {
                { WeatherIntensity.Light, new WeatherData { Type = WeatherType.HeavyRain, Intensity = WeatherIntensity.Light, Duration = 250f, DefenseMultiplier = 1.2f, VisibilityRadius = 400f, EffectColor = "#191970" } },
                { WeatherIntensity.Normal, new WeatherData { Type = WeatherType.HeavyRain, Intensity = WeatherIntensity.Normal, Duration = 250f, DefenseMultiplier = 1.25f, VisibilityRadius = 350f, EffectColor = "#000080" } },
                { WeatherIntensity.Heavy, new WeatherData { Type = WeatherType.HeavyRain, Intensity = WeatherIntensity.Heavy, Duration = 250f, DefenseMultiplier = 1.3f, VisibilityRadius = 300f, EffectColor = "#000000" } }
            };

            // 暴风雨
            _weatherTemplates[WeatherType.Storm] = new Dictionary<WeatherIntensity, WeatherData>
            {
                { WeatherIntensity.Light, new WeatherData { Type = WeatherType.Storm, Intensity = WeatherIntensity.Light, Duration = 200f, DamageMultiplier = 1.2f, DefenseMultiplier = 0.9f, VisibilityRadius = 350f, EffectColor = "#4B0082" } },
                { WeatherIntensity.Normal, new WeatherData { Type = WeatherType.Storm, Intensity = WeatherIntensity.Normal, Duration = 200f, DamageMultiplier = 1.3f, DefenseMultiplier = 0.85f, VisibilityRadius = 300f, EffectColor = "#2E0854" } },
                { WeatherIntensity.Heavy, new WeatherData { Type = WeatherType.Storm, Intensity = WeatherIntensity.Heavy, Duration = 200f, DamageMultiplier = 1.4f, DefenseMultiplier = 0.8f, VisibilityRadius = 250f, EffectColor = "#1A0533" } }
            };

            // 小雪
            _weatherTemplates[WeatherType.Snow] = new Dictionary<WeatherIntensity, WeatherData>
            {
                { WeatherIntensity.Light, new WeatherData { Type = WeatherType.Snow, Intensity = WeatherIntensity.Light, Duration = 350f, DefenseMultiplier = 1.15f, ExperienceMultiplier = 1.1f, VisibilityRadius = 450f, EffectColor = "#E0FFFF" } },
                { WeatherIntensity.Normal, new WeatherData { Type = WeatherType.Snow, Intensity = WeatherIntensity.Normal, Duration = 350f, DefenseMultiplier = 1.2f, ExperienceMultiplier = 1.15f, VisibilityRadius = 400f, EffectColor = "#B0E0E6" } },
                { WeatherIntensity.Heavy, new WeatherData { Type = WeatherType.Snow, Intensity = WeatherIntensity.Heavy, Duration = 350f, DefenseMultiplier = 1.25f, ExperienceMultiplier = 1.2f, VisibilityRadius = 350f, EffectColor = "#87CEEB" } }
            };

            // 大雪
            _weatherTemplates[WeatherType.HeavySnow] = new Dictionary<WeatherIntensity, WeatherData>
            {
                { WeatherIntensity.Light, new WeatherData { Type = WeatherType.HeavySnow, Intensity = WeatherIntensity.Light, Duration = 300f, DefenseMultiplier = 1.25f, ExperienceMultiplier = 1.2f, VisibilityRadius = 350f, EffectColor = "#778899" } },
                { WeatherIntensity.Normal, new WeatherData { Type = WeatherType.HeavySnow, Intensity = WeatherIntensity.Normal, Duration = 300f, DefenseMultiplier = 1.3f, ExperienceMultiplier = 1.25f, VisibilityRadius = 300f, EffectColor = "#708090" } },
                { WeatherIntensity.Heavy, new WeatherData { Type = WeatherType.HeavySnow, Intensity = WeatherIntensity.Heavy, Duration = 300f, DefenseMultiplier = 1.35f, ExperienceMultiplier = 1.3f, VisibilityRadius = 250f, EffectColor = "#2F4F4F" } }
            };

            // 雾
            _weatherTemplates[WeatherType.Fog] = new Dictionary<WeatherIntensity, WeatherData>
            {
                { WeatherIntensity.Light, new WeatherData { Type = WeatherType.Fog, Intensity = WeatherIntensity.Light, Duration = 280f, VisibilityRadius = 400f, EffectColor = "#D3D3D3" } },
                { WeatherIntensity.Normal, new WeatherData { Type = WeatherType.Fog, Intensity = WeatherIntensity.Normal, Duration = 280f, VisibilityRadius = 300f, EffectColor = "#C0C0C0" } },
                { WeatherIntensity.Heavy, new WeatherData { Type = WeatherType.Fog, Intensity = WeatherIntensity.Heavy, Duration = 280f, VisibilityRadius = 200f, EffectColor = "#A9A9A9" } }
            };

            // 沙尘暴
            _weatherTemplates[WeatherType.Sandstorm] = new Dictionary<WeatherIntensity, WeatherData>
            {
                { WeatherIntensity.Light, new WeatherData { Type = WeatherType.Sandstorm, Intensity = WeatherIntensity.Light, Duration = 220f, DefenseMultiplier = 0.9f, VisibilityRadius = 350f, EffectColor = "#DAA520" } },
                { WeatherIntensity.Normal, new WeatherData { Type = WeatherType.Sandstorm, Intensity = WeatherIntensity.Normal, Duration = 220f, DefenseMultiplier = 0.85f, VisibilityRadius = 280f, EffectColor = "#CD853F" } },
                { WeatherIntensity.Heavy, new WeatherData { Type = WeatherType.Sandstorm, Intensity = WeatherIntensity.Heavy, Duration = 220f, DefenseMultiplier = 0.8f, VisibilityRadius = 200f, EffectColor = "#A0522D" } }
            };

            // 夜晚
            _weatherTemplates[WeatherType.Night] = new Dictionary<WeatherIntensity, WeatherData>
            {
                { WeatherIntensity.Light, new WeatherData { Type = WeatherType.Night, Intensity = WeatherIntensity.Light, Duration = 500f, DamageMultiplier = 1.1f, VisibilityRadius = 400f, EffectColor = "#191970" } },
                { WeatherIntensity.Normal, new WeatherData { Type = WeatherType.Night, Intensity = WeatherIntensity.Normal, Duration = 500f, DamageMultiplier = 1.15f, VisibilityRadius = 350f, EffectColor = "#000033" } },
                { WeatherIntensity.Heavy, new WeatherData { Type = WeatherType.Night, Intensity = WeatherIntensity.Heavy, Duration = 500f, DamageMultiplier = 1.2f, VisibilityRadius = 300f, EffectColor = "#000000" } }
            };
        }

        public WeatherData GetWeatherTemplate(WeatherType type, WeatherIntensity intensity)
        {
            if (_weatherTemplates.TryGetValue(type, out var intensities) && 
                intensities.TryGetValue(intensity, out var template))
            {
                return new WeatherData
                {
                    Type = template.Type,
                    Intensity = template.Intensity,
                    Duration = template.Duration,
                    RemainingTime = template.Duration,
                    DamageMultiplier = template.DamageMultiplier,
                    DefenseMultiplier = template.DefenseMultiplier,
                    ExperienceMultiplier = template.ExperienceMultiplier,
                    DropMultiplier = template.DropMultiplier,
                    VisibilityRadius = template.VisibilityRadius,
                    EffectColor = template.EffectColor
                };
            }
            return new WeatherData();
        }

        public List<WeatherType> GetAvailableWeatherTypes()
        {
            return new List<WeatherType>(_weatherTemplates.Keys);
        }
    }

    /// <summary>
    /// 天气系统管理器
    /// </summary>
    public partial class WeatherSystem : Node
    {
        private static WeatherSystem _instance;
        public static WeatherSystem Instance => _instance;

        [Signal]
        public delegate void WeatherChangedEventHandler(WeatherData newWeather, WeatherData oldWeather);

        [Signal]
        public delegate void WeatherUpdatedEventHandler(WeatherData currentWeather);

        private WeatherData _currentWeather;
        private Random _random;
        private float _weatherChangeTimer;
        private bool _autoChange;

        public WeatherData CurrentWeather => _currentWeather;
        public bool AutoChange
        {
            get => _autoChange;
            set => _autoChange = value;
        }

        public override void _Ready()
        {
            _instance = this;
            _random = new Random();
            _currentWeather = WeatherDatabase.Instance.GetWeatherTemplate(WeatherType.Clear, WeatherIntensity.Normal);
            _weatherChangeTimer = 0f;
            _autoChange = true;
        }

        public override void _Process(float delta)
        {
            if (_autoChange && _currentWeather != null)
            {
                _currentWeather.RemainingTime -= delta;
                _weatherChangeTimer += delta;

                // 每秒发出一次更新信号
                if (_weatherChangeTimer >= 1.0f)
                {
                    EmitSignal(SignalName.WeatherUpdated, _currentWeather);
                    _weatherChangeTimer = 0f;
                }

                // 天气结束时切换
                if (_currentWeather.RemainingTime <= 0)
                {
                    ChangeToRandomWeather();
                }
            }
        }

        /// <summary>
        /// 切换到指定天气
        /// </summary>
        public void ChangeWeather(WeatherType type, WeatherIntensity intensity)
        {
            var oldWeather = _currentWeather;
            _currentWeather = WeatherDatabase.Instance.GetWeatherTemplate(type, intensity);
            _weatherChangeTimer = 0f;
            
            EmitSignal(SignalName.WeatherChanged, _currentWeather, oldWeather);
        }

        /// <summary>
        /// 切换到随机天气
        /// </summary>
        public void ChangeToRandomWeather()
        {
            var weatherTypes = WeatherDatabase.Instance.GetAvailableWeatherTypes();
            var randomType = weatherTypes[_random.Next(weatherTypes.Count)];
            var intensities = (WeatherIntensity[])(Enum.GetValues(typeof(WeatherIntensity)));
            var randomIntensity = intensities[_random.Next(intensities.Length)];
            
            ChangeWeather(randomType, randomIntensity);
        }

        /// <summary>
        /// 清除天气（设为晴朗）
        /// </summary>
        public void ClearWeather()
        {
            ChangeWeather(WeatherType.Clear, WeatherIntensity.Normal);
        }

        /// <summary>
        /// 获取天气显示名称
        /// </summary>
        public static string GetWeatherName(WeatherType type)
        {
            return type switch
            {
                WeatherType.Clear => "晴朗",
                WeatherType.Cloudy => "多云",
                WeatherType.Rain => "小雨",
                WeatherType.HeavyRain => "大雨",
                WeatherType.Storm => "暴风雨",
                WeatherType.Snow => "小雪",
                WeatherType.HeavySnow => "大雪",
                WeatherType.Fog => "雾",
                WeatherType.Sandstorm => "沙尘暴",
                WeatherType.Night => "夜晚",
                _ => "未知"
            };
        }

        /// <summary>
        /// 获取天气图标
        /// </summary>
        public static string GetWeatherIcon(WeatherType type)
        {
            return type switch
            {
                WeatherType.Clear => "☀️",
                WeatherType.Cloudy => "☁️",
                WeatherType.Rain => "🌧️",
                WeatherType.HeavyRain => "⛈️",
                WeatherType.Storm => "🌩️",
                WeatherType.Snow => "❄️",
                WeatherType.HeavySnow => "🌨️",
                WeatherType.Fog => "🌫️",
                WeatherType.Sandstorm => "🌪️",
                WeatherType.Night => "🌙",
                _ => "❓"
            };
        }

        /// <summary>
        /// 序列化天气数据
        /// </summary>
        public Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { "weatherType", (int)_currentWeather.Type },
                { "weatherIntensity", (int)_currentWeather.Intensity },
                { "remainingTime", _currentWeather.RemainingTime },
                { "autoChange", _autoChange }
            };
        }

        /// <summary>
        /// 反序列化天气数据
        /// </summary>
        public void Deserialize(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            var type = (WeatherType)(int)data.GetValueOrDefault("weatherType", 0);
            var intensity = (WeatherIntensity)(int)data.GetValueOrDefault("weatherIntensity", 1);
            var remainingTime = (float)data.GetValueOrDefault("remainingTime", 300f);
            _autoChange = (bool)data.GetValueOrDefault("autoChange", true);
            
            var oldWeather = _currentWeather;
            _currentWeather = WeatherDatabase.Instance.GetWeatherTemplate(type, intensity);
            _currentWeather.RemainingTime = remainingTime;
            
            EmitSignal(SignalName.WeatherChanged, _currentWeather, oldWeather);
        }
    }
}
