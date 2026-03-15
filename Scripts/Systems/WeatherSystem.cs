using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 天气系统核心管理器
    /// </summary>
    public class WeatherSystem : BaseSystem
    {
        private static WeatherSystem _instance;
        public static WeatherSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GetNode<WeatherSystem>("/root/WeatherSystem");
                    if (_instance == null)
                    {
                        var node = new WeatherSystem();
                        node.Name = "WeatherSystem";
                        Engine.GetMainLoop().Root.AddChild(node);
                    }
                }
                return _instance;
            }
        }
        
        private Dictionary<string, ZoneWeather> _zoneWeathers;
        private Dictionary<string, PlayerWeatherState> _playerStates;
        private List<WeatherEvent> _weatherEvents;
        private WeatherStatistics _statistics;
        private Random _random;
        
        // 事件
        public Action<string, WeatherType, WeatherIntensity> OnWeatherChanged;
        public Action<string, WeatherEffectType, float> OnWeatherEffectApplied;
        public Action<string, float> OnTransitionProgress;
        
        protected override void Initialize()
        {
            base.Initialize();
            
            _zoneWeathers = new Dictionary<string, ZoneWeather>();
            _playerStates = new Dictionary<string, PlayerWeatherState>();
            _weatherEvents = new List<WeatherEvent>();
            _statistics = new WeatherStatistics();
            _random = new Random();
            
            // 注册到保存系统
            SaveSystem.Instance?.Register(this);
            
            InitializeDefaultZones();
            LoadWeatherData();
            
            GD.Print("[WeatherSystem] Initialized");
        }
        
        private void InitializeDefaultZones()
        {
            // 初始化默认区域天气
            string[] defaultZones = { "forest", "desert", "mountain", "plains", "lake", "town", "dungeon" };
            foreach (string zone in defaultZones)
            {
                CreateZoneWeather(zone, zone);
            }
        }
        
        private void LoadWeatherData()
        {
            // 天气数据现在通过 ImportSaveData 加载
        }
        
        /// <summary>
        /// 创建区域天气
        /// </summary>
        public void CreateZoneWeather(string zoneId, string zoneName)
        {
            if (_zoneWeathers.ContainsKey(zoneId))
                return;
            
            // 随机选择初始天气
            List<WeatherType> weatherTypes = WeatherDatabase.GetZoneWeatherTypes(zoneId);
            if (weatherTypes.Count == 0) return;
            
            WeatherType initialWeather = weatherTypes[_random.Next(weatherTypes.Count)];
            WeatherConfig config = WeatherDatabase.GetWeatherConfig(initialWeather);
            
            ZoneWeather zoneWeather = new ZoneWeather
            {
                ZoneId = zoneId,
                ZoneName = zoneName,
                CurrentWeather = initialWeather,
                Intensity = config?.DefaultIntensity ?? WeatherIntensity.Moderate,
                RemainingTime = (config?.Duration ?? 30f) * 60f, // 转换为秒
                TransitionProgress = 1f,
                IsTransitioning = false,
                LastUpdate = DateTime.Now
            };
            
            _zoneWeathers[zoneId] = zoneWeather;
        }
        
        /// <summary>
        /// 获取区域当前天气
        /// </summary>
        public ZoneWeather GetZoneWeather(string zoneId)
        {
            if (_zoneWeathers.ContainsKey(zoneId))
                return _zoneWeathers[zoneId];
            return null;
        }
        
        /// <summary>
        /// 设置区域天气
        /// </summary>
        public void SetZoneWeather(string zoneId, WeatherType weatherType, WeatherIntensity intensity)
        {
            if (!_zoneWeathers.ContainsKey(zoneId))
                CreateZoneWeather(zoneId, zoneId);
            
            ZoneWeather zoneWeather = _zoneWeathers[zoneId];
            WeatherConfig config = WeatherDatabase.GetWeatherConfig(weatherType);
            
            // 触发天气变化事件
            OnWeatherChanged?.Invoke(zoneId, weatherType, intensity);
            
            // 记录天气事件
            WeatherEvent weatherEvent = new WeatherEvent
            {
                EventId = Guid.NewGuid().ToString(),
                ZoneId = zoneId,
                Weather = weatherType,
                Intensity = intensity,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddMinutes(config?.Duration ?? 30),
                AffectedPlayers = 0
            };
            _weatherEvents.Add(weatherEvent);
            
            // 更新统计
            _statistics.TotalWeatherEvents++;
            if (_statistics.WeatherCounts.ContainsKey(weatherType))
                _statistics.WeatherCounts[weatherType]++;
            else
                _statistics.WeatherCounts[weatherType] = 1;
            
            // 开始过渡
            zoneWeather.IsTransitioning = true;
            zoneWeather.TransitionProgress = 0f;
            
            // 更新天气
            zoneWeather.CurrentWeather = weatherType;
            zoneWeather.Intensity = intensity;
            zoneWeather.RemainingTime = (config?.Duration ?? 30f) * 60f;
            
            _zoneWeathers[zoneId] = zoneWeather;
        }
        
        /// <summary>
        /// 更新天气（每帧调用）
        /// </summary>
        public override void _Process(float delta)
        {
            base._Process(delta);
            
            foreach (var zoneWeather in _zoneWeathers.Values)
            {
                // 处理过渡
                if (zoneWeather.IsTransitioning)
                {
                    zoneWeather.TransitionProgress += delta / 30f; // 默认30秒过渡
                    if (zoneWeather.TransitionProgress >= 1f)
                    {
                        zoneWeather.TransitionProgress = 1f;
                        zoneWeather.IsTransitioning = false;
                    }
                    OnTransitionProgress?.Invoke(zoneWeather.ZoneId, zoneWeather.TransitionProgress);
                }
                
                // 处理持续时间
                zoneWeather.RemainingTime -= delta;
                if (zoneWeather.RemainingTime <= 0)
                {
                    // 切换到下一个天气
                    ChangeWeather(zoneWeather.ZoneId);
                }
                
                zoneWeather.LastUpdate = DateTime.Now;
            }
        }
        
        /// <summary>
        /// 切换区域天气
        /// </summary>
        private void ChangeWeather(string zoneId)
        {
            if (!_zoneWeathers.ContainsKey(zoneId))
                return;
            
            ZoneWeather zoneWeather = _zoneWeathers[zoneId];
            List<WeatherType> weatherTypes = WeatherDatabase.GetZoneWeatherTypes(zoneId);
            
            if (weatherTypes.Count == 0) return;
            
            // 随机选择下一个天气（避免重复）
            WeatherType nextWeather;
            do
            {
                nextWeather = weatherTypes[_random.Next(weatherTypes.Count)];
            } while (nextWeather == zoneWeather.CurrentWeather && weatherTypes.Count > 1);
            
            // 随机选择强度
            WeatherIntensity[] intensities = { WeatherIntensity.Calm, WeatherIntensity.Light, 
                WeatherIntensity.Moderate, WeatherIntensity.Severe };
            WeatherIntensity intensity = intensities[_random.Next(intensities.Length)];
            
            SetZoneWeather(zoneId, nextWeather, intensity);
        }
        
        /// <summary>
        /// 获取天气效果
        /// </summary>
        public List<WeatherEffect> GetWeatherEffects(WeatherType type)
        {
            WeatherConfig config = WeatherDatabase.GetWeatherConfig(type);
            return config?.Effects ?? new List<WeatherEffect>();
        }
        
        /// <summary>
        /// 获取玩家天气状态
        /// </summary>
        public PlayerWeatherState GetPlayerState(string playerId)
        {
            if (!_playerStates.ContainsKey(playerId))
            {
                _playerStates[playerId] = new PlayerWeatherState
                {
                    PlayerId = playerId,
                    Resistances = new List<WeatherResistance>(),
                    WeatherEvents = new Dictionary<WeatherType, int>(),
                    LastWeatherCheck = DateTime.Now
                };
            }
            return _playerStates[playerId];
        }
        
        /// <summary>
        /// 玩家进入区域
        /// </summary>
        public void PlayerEnterZone(string playerId, string zoneId)
        {
            if (!_zoneWeathers.ContainsKey(zoneId))
                return;
            
            ZoneWeather zoneWeather = _zoneWeathers[zoneId];
            PlayerWeatherState playerState = GetPlayerState(playerId);
            
            // 更新玩家暴露时间
            playerState.TotalExposureTime++;
            
            // 记录天气事件
            if (playerState.WeatherEvents.ContainsKey(zoneWeather.CurrentWeather))
                playerState.WeatherEvents[zoneWeather.CurrentWeather]++;
            else
                playerState.WeatherEvents[zoneWeather.CurrentWeather] = 1;
            
            // 应用天气效果
            ApplyWeatherEffects(playerId, zoneWeather);
            
            // 更新统计
            _statistics.PlayerWeatherEvents++;
            
            playerState.LastWeatherCheck = DateTime.Now;
        }
        
        /// <summary>
        /// 应用天气效果
        /// </summary>
        private void ApplyWeatherEffects(string playerId, ZoneWeather zoneWeather)
        {
            WeatherConfig config = WeatherDatabase.GetWeatherConfig(zoneWeather.CurrentWeather);
            if (config == null) return;
            
            PlayerWeatherState playerState = GetPlayerState(playerId);
            
            foreach (var effect in config.Effects)
            {
                // 计算抗性修正
                float resistance = GetPlayerResistance(playerId, zoneWeather.CurrentWeather);
                float modifiedValue = effect.Value * (1f - resistance / 100f);
                
                // 根据强度调整
                float intensityMultiplier = GetIntensityMultiplier(zoneWeather.Intensity);
                modifiedValue *= intensityMultiplier;
                
                // 触发效果事件
                OnWeatherEffectApplied?.Invoke(playerId, effect.Type, modifiedValue);
            }
        }
        
        /// <summary>
        /// 获取玩家抗性
        /// </summary>
        public float GetPlayerResistance(string playerId, WeatherType type)
        {
            PlayerWeatherState state = GetPlayerState(playerId);
            foreach (var resistance in state.Resistances)
            {
                if (resistance.Type == type)
                    return resistance.Resistance;
            }
            return 0f;
        }
        
        /// <summary>
        /// 提升玩家天气抗性
        /// </summary>
        public void ImproveResistance(string playerId, WeatherType type, float amount)
        {
            PlayerWeatherState state = GetPlayerState(playerId);
            bool found = false;
            
            foreach (var resistance in state.Resistances)
            {
                if (resistance.Type == type)
                {
                    resistance.Resistance = Mathf.Min(100f, resistance.Resistance + amount);
                    resistance.LastExposure = DateTime.Now;
                    found = true;
                    break;
                }
            }
            
            if (!found)
            {
                state.Resistances.Add(new WeatherResistance
                {
                    Type = type,
                    Resistance = Mathf.Min(100f, amount),
                    LastExposure = DateTime.Now
                });
            }
        }
        
        /// <summary>
        /// 获取强度乘数
        /// </summary>
        private float GetIntensityMultiplier(WeatherIntensity intensity)
        {
            switch (intensity)
            {
                case WeatherIntensity.Calm: return 0.5f;
                case WeatherIntensity.Light: return 0.75f;
                case WeatherIntensity.Moderate: return 1.0f;
                case WeatherIntensity.Severe: return 1.25f;
                case WeatherIntensity.Extreme: return 1.5f;
                default: return 1.0f;
            }
        }
        
        /// <summary>
        /// 获取天气图标
        /// </summary>
        public string GetWeatherIcon(WeatherType type)
        {
            WeatherConfig config = WeatherDatabase.GetWeatherConfig(type);
            return config?.Icon ?? "clear";
        }
        
        /// <summary>
        /// 获取当前季节
        /// </summary>
        public Season GetCurrentSeason()
        {
            return WeatherDatabase.GetCurrentSeason();
        }
        
        /// <summary>
        /// 获取天气统计
        /// </summary>
        public WeatherStatistics GetStatistics()
        {
            return _statistics;
        }
        
        /// <summary>
        /// 获取所有天气事件
        /// </summary>
        public List<WeatherEvent> GetWeatherEvents()
        {
            return _weatherEvents;
        }
        
        /// <summary>
        /// 获取所有区域ID
        /// </summary>
        public List<string> GetAllZoneIds()
        {
            return new List<string>(_zoneWeathers.Keys);
        }
        
        /// <summary>
        /// 强制刷新区域天气
        /// </summary>
        public void RefreshZoneWeather(string zoneId)
        {
            if (_zoneWeathers.ContainsKey(zoneId))
            {
                _zoneWeathers[zoneId].RemainingTime = 0;
            }
        }
        
        /// <summary>
        /// 获取天气持续时间
        /// </summary>
        public float GetWeatherDuration(WeatherType type)
        {
            WeatherConfig config = WeatherDatabase.GetWeatherConfig(type);
            return config?.Duration ?? 30f;
        }
        
        /// <summary>
        /// 获取天气过渡时间
        /// </summary>
        public float GetWeatherTransitionTime(WeatherType type)
        {
            WeatherConfig config = WeatherDatabase.GetWeatherConfig(type);
            return config?.TransitionTime ?? 10f;
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            var zoneWeathersList = new List<Dictionary>();
            foreach (var zw in _zoneWeathers.Values)
            {
                zoneWeathersList.Add(new Dictionary
                {
                    ["zone_id"] = zw.ZoneId,
                    ["zone_name"] = zw.ZoneName,
                    ["current_weather"] = (int)zw.CurrentWeather,
                    ["intensity"] = (int)zw.Intensity,
                    ["remaining_time"] = zw.RemainingTime,
                    ["transition_progress"] = zw.TransitionProgress,
                    ["is_transitioning"] = zw.IsTransitioning
                });
            }
            data["zone_weathers"] = zoneWeathersList;
            
            var weatherEventsList = new List<Dictionary>();
            foreach (var we in _weatherEvents)
            {
                weatherEventsList.Add(new Dictionary
                {
                    ["event_id"] = we.EventId,
                    ["zone_id"] = we.ZoneId,
                    ["weather"] = (int)we.Weather,
                    ["intensity"] = (int)we.Intensity,
                    ["start_time"] = we.StartTime.ToString("o"),
                    ["affected_players"] = we.AffectedPlayers
                });
            }
            data["weather_events"] = weatherEventsList;
            
            var weatherCounts = new Dictionary();
            foreach (var kvp in _statistics.WeatherCounts)
            {
                weatherCounts[(int)kvp.Key] = kvp.Value;
            }
            data["weather_counts"] = weatherCounts;
            data["total_weather_events"] = _statistics.TotalWeatherEvents;
            data["player_weather_events"] = _statistics.PlayerWeatherEvents;
            
            return data;
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            if (data.Contains("zone_weathers"))
            {
                var zoneList = data["zone_weathers"] as List<object>;
                if (zoneList != null)
                {
                    _zoneWeathers.Clear();
                    foreach (var item in zoneList)
                    {
                        var dict = item as Dictionary;
                        if (dict != null)
                        {
                            var zw = new ZoneWeather
                            {
                                ZoneId = dict["zone_id"] as string,
                                ZoneName = dict["zone_name"] as string,
                                CurrentWeather = (WeatherType)(int)dict["current_weather"],
                                Intensity = (WeatherIntensity)(int)dict["intensity"],
                                RemainingTime = (float)dict["remaining_time"],
                                TransitionProgress = (float)dict["transition_progress"],
                                IsTransitioning = (bool)dict["is_transitioning"],
                                LastUpdate = DateTime.Now
                            };
                            _zoneWeathers[zw.ZoneId] = zw;
                        }
                    }
                }
            }
            
            if (data.Contains("weather_events"))
            {
                var eventsList = data["weather_events"] as List<object>;
                if (eventsList != null)
                {
                    _weatherEvents.Clear();
                    foreach (var item in eventsList)
                    {
                        var dict = item as Dictionary;
                        if (dict != null)
                        {
                            var we = new WeatherEvent
                            {
                                EventId = dict["event_id"] as string,
                                ZoneId = dict["zone_id"] as string,
                                Weather = (WeatherType)(int)dict["weather"],
                                Intensity = (WeatherIntensity)(int)dict["intensity"],
                                StartTime = DateTime.Parse(dict["start_time"] as string),
                                AffectedPlayers = (int)dict["affected_players"]
                            };
                            _weatherEvents.Add(we);
                        }
                    }
                }
            }
            
            if (data.Contains("weather_counts"))
            {
                var counts = data["weather_counts"] as Dictionary;
                if (counts != null)
                {
                    _statistics.WeatherCounts.Clear();
                    foreach (var kvp in counts)
                    {
                        _statistics.WeatherCounts[(WeatherType)(int)kvp.Key] = (int)kvp.Value;
                    }
                }
            }
            
            _statistics.TotalWeatherEvents = data.Contains("total_weather_events") ? (int)data["total_weather_events"] : 0;
            _statistics.PlayerWeatherEvents = data.Contains("player_weather_events") ? (int)data["player_weather_events"] : 0;
        }
        
        /// <summary>
        /// 获取系统ID
        /// </summary>
        public override string GetId()
        {
            return "WeatherSystem";
        }
    }
    
    /// <summary>
    /// 天气存档数据
    /// </summary>
    [System.Serializable]
    public class WeatherSaveData
    {
        public List<ZoneWeather> ZoneWeathers;
        public List<WeatherEvent> WeatherEvents;
        public WeatherStatistics Statistics;
    }
}
