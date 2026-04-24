// 天气坐骑加成系统
// MountWeatherBonusSystem.cs
// 坐骑根据不同天气获得属性加成

using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 天气坐骑加成系统 - 根据天气类型为不同类型坐骑提供属性加成
/// </summary>
public partial class MountWeatherBonusSystem : BaseSystem
{
    public static MountWeatherBonusSystem Instance { get; private set; }
    
    // 天气类型枚举
    public enum WeatherType
    {
        Clear = 0,
        Cloudy = 1,
        Rain = 2,
        Snow = 3,
        Thunderstorm = 4,
        Fog = 5,
        Sandstorm = 6,
        Hail = 7,
        Blizzard = 8,
        Storm = 9
    }
    
    // 坐骑类型
    public enum MountCategory
    {
        Land = 0,      // 陆地坐骑
        Flying = 1,    // 飞行坐骑
        Aquatic = 2    // 水生坐骑
    }
    
    // 天气加成配置
    private Dictionary<WeatherType, Dictionary<MountCategory, Dictionary<string, float>>> weatherBonuses = new Dictionary<WeatherType, Dictionary<MountCategory, Dictionary<string, float>>>();
    
    // 当前天气
    private WeatherType currentWeather = WeatherType.Clear;
    
    // 玩家坐骑数据
    private Dictionary<int, string> playerMounts = new Dictionary<int, string>(); // mount_id -> mount_type
    
    public override void _Ready()
    {
        Instance = this;
        InitializeWeatherBonuses();
        
        // 监听天气变化
        var weatherSystem = GetTree().GetFirstNodeInGroup("WeatherSystem");
        if (weatherSystem != null)
        {
            // 天气系统存在，连接信号
        }
    }
    
    private void InitializeWeatherBonuses()
    {
        // 晴朗天气 - 陆地坐骑速度加成
        weatherBonuses[WeatherType.Clear] = new Dictionary<MountCategory, Dictionary<string, float>>();
        weatherBonuses[WeatherType.Clear][MountCategory.Land] = new Dictionary<string, float>
        {
            { "speed", 0.15f },
            { "attack", 0.10f }
        };
        weatherBonuses[WeatherType.Clear][MountCategory.Flying] = new Dictionary<string, float>
        {
            { "speed", 0.20f },
            { "attack", 0.15f }
        };
        weatherBonuses[WeatherType.Clear][MountCategory.Aquatic] = new Dictionary<string, float>
        {
            { "speed", 0.05f }
        };
        
        // 多云天气 - 所有坐骑平衡加成
        weatherBonuses[WeatherType.Cloudy] = new Dictionary<MountCategory, Dictionary<string, float>>();
        weatherBonuses[WeatherType.Cloudy][MountCategory.Land] = new Dictionary<string, float>
        {
            { "defense", 0.10f },
            { "health", 0.05f }
        };
        weatherBonuses[WeatherType.Cloudy][MountCategory.Flying] = new Dictionary<string, float>
        {
            { "defense", 0.10f },
            { "health", 0.05f }
        };
        weatherBonuses[WeatherType.Cloudy][MountCategory.Aquatic] = new Dictionary<string, float>
        {
            { "defense", 0.10f },
            { "health", 0.05f }
        };
        
        // 雨天 - 水生坐骑强力加成
        weatherBonuses[WeatherType.Rain] = new Dictionary<MountCategory, Dictionary<string, float>>();
        weatherBonuses[WeatherType.Rain][MountCategory.Land] = new Dictionary<string, float>
        {
            { "speed", -0.10f },  // 减速
            { "defense", 0.05f }
        };
        weatherBonuses[WeatherType.Rain][MountCategory.Flying] = new Dictionary<string, float>
        {
            { "speed", -0.05f },
            { "magic", 0.10f }
        };
        weatherBonuses[WeatherType.Rain][MountCategory.Aquatic] = new Dictionary<string, float>
        {
            { "speed", 0.25f },
            { "attack", 0.15f },
            { "health", 0.10f }
        };
        
        // 雪天 - 冰系坐骑加成
        weatherBonuses[WeatherType.Snow] = new Dictionary<MountCategory, Dictionary<string, float>>();
        weatherBonuses[WeatherType.Snow][MountCategory.Land] = new Dictionary<string, float>
        {
            { "defense", 0.15f },
            { "health", 0.10f }
        };
        weatherBonuses[WeatherType.Snow][MountCategory.Flying] = new Dictionary<string, float>
        {
            { "speed", 0.10f },
            { "ice_resist", 0.20f }
        };
        weatherBonuses[WeatherType.Snow][MountCategory.Aquatic] = new Dictionary<string, float>
        {
            { "ice_resist", 0.25f },
            { "defense", 0.10f }
        };
        
        // 雷暴 - 飞行坐骑强力加成
        weatherBonuses[WeatherType.Thunderstorm] = new Dictionary<MountCategory, Dictionary<string, float>>();
        weatherBonuses[WeatherType.Thunderstorm][MountCategory.Land] = new Dictionary<string, float>
        {
            { "speed", -0.15f },  // 减速
            { "defense", -0.10f }
        };
        weatherBonuses[WeatherType.Thunderstorm][MountCategory.Flying] = new Dictionary<string, float>
        {
            { "speed", 0.30f },
            { "attack", 0.25f },
            { "lightning_damage", 0.20f }
        };
        weatherBonuses[WeatherType.Thunderstorm][MountCategory.Aquatic] = new Dictionary<string, float>
        {
            { "speed", 0.15f },
            { "lightning_resist", 0.20f }
        };
        
        // 雾天 - 隐蔽加成
        weatherBonuses[WeatherType.Fog] = new Dictionary<MountCategory, Dictionary<string, float>>();
        weatherBonuses[WeatherType.Fog][MountCategory.Land] = new Dictionary<string, float>
        {
            { "dodge", 0.15f },
            { "defense", 0.05f }
        };
        weatherBonuses[WeatherType.Fog][MountCategory.Flying] = new Dictionary<string, float>
        {
            { "dodge", 0.20f },
            { "speed", 0.05f }
        };
        weatherBonuses[WeatherType.Fog][MountCategory.Aquatic] = new Dictionary<string, float>
        {
            { "dodge", 0.15f },
            { "stealth", 0.10f }
        };
        
        // 沙尘暴 - 陆地坐骑抵抗
        weatherBonuses[WeatherType.Sandstorm] = new Dictionary<MountCategory, Dictionary<string, float>>();
        weatherBonuses[WeatherType.Sandstorm][MountCategory.Land] = new Dictionary<string, float>
        {
            { "defense", 0.20f },
            { "fire_resist", 0.15f },
            { "health", 0.10f }
        };
        weatherBonuses[WeatherType.Sandstorm][MountCategory.Flying] = new Dictionary<string, float>
        {
            { "speed", -0.20f },
            { "defense", -0.10f }
        };
        weatherBonuses[WeatherType.Sandstorm][MountCategory.Aquatic] = new Dictionary<string, float>
        {
            { "speed", -0.10f },
            { "defense", 0.05f }
        };
        
        // 冰雹 - 防御加成
        weatherBonuses[WeatherType.Hail] = new Dictionary<MountCategory, Dictionary<string, float>>();
        weatherBonuses[WeatherType.Hail][MountCategory.Land] = new Dictionary<string, float>
        {
            { "defense", 0.20f },
            { "health", 0.10f }
        };
        weatherBonuses[WeatherType.Hail][MountCategory.Flying] = new Dictionary<string, float>
        {
            { "defense", 0.15f },
            { "ice_resist", 0.15f }
        };
        weatherBonuses[WeatherType.Hail][MountCategory.Aquatic] = new Dictionary<string, float>
        {
            { "defense", 0.20f },
            { "ice_resist", 0.20f }
        };
        
        // 暴风雪 - 冰系坐骑超级加成
        weatherBonuses[WeatherType.Blizzard] = new Dictionary<MountCategory, Dictionary<string, float>>();
        weatherBonuses[WeatherType.Blizzard][MountCategory.Land] = new Dictionary<string, float>
        {
            { "speed", -0.25f },
            { "defense", 0.10f }
        };
        weatherBonuses[WeatherType.Blizzard][MountCategory.Flying] = new Dictionary<string, float>
        {
            { "speed", 0.15f },
            { "ice_resist", 0.30f },
            { "attack", 0.10f }
        };
        weatherBonuses[WeatherType.Blizzard][MountCategory.Aquatic] = new Dictionary<string, float>
        {
            { "ice_resist", 0.35f },
            { "defense", 0.15f }
        };
        
        // 风暴 - 水生坐骑加成
        weatherBonuses[WeatherType.Storm] = new Dictionary<MountCategory, Dictionary<string, float>>();
        weatherBonuses[WeatherType.Storm][MountCategory.Land] = new Dictionary<string, float>
        {
            { "speed", -0.10f },
            { "defense", -0.05f }
        };
        weatherBonuses[WeatherType.Storm][MountCategory.Flying] = new Dictionary<string, float>
        {
            { "speed", 0.10f },
            { "attack", 0.10f }
        };
        weatherBonuses[WeatherType.Storm][MountCategory.Aquatic] = new Dictionary<string, float>
        {
            { "speed", 0.30f },
            { "attack", 0.20f },
            { "health", 0.15f }
        };
    }
    
    // 设置当前天气
    public void SetWeather(WeatherType weather)
    {
        currentWeather = weather;
        GD.Print($"[MountWeatherBonus] Weather changed to: {weather}");
    }
    
    // 获取坐骑的天气加成
    public Dictionary<string, float> GetMountWeatherBonus(string mountType, MountCategory category)
    {
        if (!weatherBonuses.ContainsKey(currentWeather))
        {
            return new Dictionary<string, float>();
        }
        
        if (!weatherBonuses[currentWeather].ContainsKey(category))
        {
            return new Dictionary<string, float>();
        }
        
        return new Dictionary<string, float>(weatherBonuses[currentWeather][category]);
    }
    
    // 获取当前天气
    public WeatherType GetCurrentWeather()
    {
        return currentWeather;
    }
    
    // 获取天气名称
    public string GetWeatherName(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Clear: return "晴朗";
            case WeatherType.Cloudy: return "多云";
            case WeatherType.Rain: return "雨天";
            case WeatherType.Snow: return "雪天";
            case WeatherType.Thunderstorm: return "雷暴";
            case WeatherType.Fog: return "雾天";
            case WeatherType.Sandstorm: return "沙尘暴";
            case WeatherType.Hail: return "冰雹";
            case WeatherType.Blizzard: return "暴风雪";
            case WeatherType.Storm: return "风暴";
            default: return "未知";
        }
    }
    
    // 获取天气图标
    public string GetWeatherIcon(WeatherType weather)
    {
        switch (weather)
        {
            case WeatherType.Clear: return "☀️";
            case WeatherType.Cloudy: return "☁️";
            case WeatherType.Rain: return "🌧️";
            case WeatherType.Snow: return "❄️";
            case WeatherType.Thunderstorm: return "⛈️";
            case WeatherType.Fog: return "🌫️";
            case WeatherType.Sandstorm: return "🌪️";
            case WeatherType.Hail: return "🧊";
            case WeatherType.Blizzard: return "🌨️";
            case WeatherType.Storm: return "🌀";
            default: return "❓";
        }
    }
    
    // 计算最终属性值
    public float CalculateFinalAttribute(float baseValue, string attributeName, MountCategory category)
    {
        var bonuses = GetMountWeatherBonus("", category);
        
        if (bonuses.ContainsKey(attributeName))
        {
            float multiplier = 1.0f + bonuses[attributeName];
            return baseValue * multiplier;
        }
        
        return baseValue;
    }
    
    // 随机设置天气（用于测试）
    public void RandomWeather()
    {
        Array weatherTypes = Enum.GetValues(typeof(WeatherType));
        WeatherType randomWeather = (WeatherType)weatherTypes.GetValue(GD.Randi() % weatherTypes.Length);
        SetWeather(randomWeather);
    }
    
    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary
        {
            { "current_weather", (int)currentWeather }
        };
    }
    
    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("current_weather"))
        {
            currentWeather = (WeatherType)(int)data["current_weather"];
        }
    }
}
