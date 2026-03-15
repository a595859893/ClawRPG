using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// 天气系统核心管理器
    /// </summary>
    public class WeatherSystem : MonoBehaviour
    {
        private static WeatherSystem _instance;
        public static WeatherSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("WeatherSystem");
                    _instance = go.AddComponent<WeatherSystem>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        private Dictionary<string, ZoneWeather> _zoneWeathers;
        private Dictionary<string, PlayerWeatherState> _playerStates;
        private List<WeatherEvent> _weatherEvents;
        private WeatherStatistics _statistics;
        private System.Random _random;
        
        // 事件
        public static Action<string, WeatherType, WeatherIntensity> OnWeatherChanged;
        public static Action<string, WeatherEffectType, float> OnWeatherEffectApplied;
        public static Action<string, float> OnTransitionProgress;
        
        private void Awake()
        {
            _instance = this;
            _zoneWeathers = new Dictionary<string, ZoneWeather>();
            _playerStates = new Dictionary<string, PlayerWeatherState>();
            _weatherEvents = new List<WeatherEvent>();
            _statistics = new WeatherStatistics();
            _random = new System.Random();
            
            WeatherDatabase.Initialize();
        }
        
        private void Start()
        {
            InitializeDefaultZones();
            LoadWeatherData();
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
            // 从存档加载天气数据
            if (SaveSystem.IsLoaded)
            {
                var data = SaveSystem.CurrentSave.WeatherData;
                if (data != null)
                {
                    if (data.ZoneWeathers != null)
                    {
                        foreach (var zw in data.ZoneWeathers)
                        {
                            _zoneWeathers[zw.ZoneId] = zw;
                        }
                    }
                    if (data.WeatherEvents != null)
                    {
                        _weatherEvents = new List<WeatherEvent>(data.WeatherEvents);
                    }
                    if (data.Statistics != null)
                    {
                        _statistics = data.Statistics;
                    }
                }
            }
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
            WeatherType initialWeather = weatherTypes[_random.Next(weatherTypes.Count)];
            WeatherConfig config = WeatherDatabase.GetWeatherConfig(initialWeather);
            
            ZoneWeather zoneWeather = new ZoneWeather
            {
                ZoneId = zoneId,
                ZoneName = zoneName,
                CurrentWeather = initialWeather,
                Intensity = config.DefaultIntensity,
                RemainingTime = config.Duration * 60f, // 转换为秒
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
                EndTime = DateTime.Now.AddMinutes(config.Duration),
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
            zoneWeather.RemainingTime = config.Duration * 60f;
            
            _zoneWeathers[zoneId] = zoneWeather;
        }
        
        /// <summary>
        /// 更新天气（每帧调用）
        /// </summary>
        private void Update()
        {
            float deltaTime = Time.deltaTime;
            
            foreach (var zoneWeather in _zoneWeathers.Values)
            {
                // 处理过渡
                if (zoneWeather.IsTransitioning)
                {
                    zoneWeather.TransitionProgress += deltaTime / 30f; // 默认30秒过渡
                    if (zoneWeather.TransitionProgress >= 1f)
                    {
                        zoneWeather.TransitionProgress = 1f;
                        zoneWeather.IsTransitioning = false;
                    }
                    OnTransitionProgress?.Invoke(zoneWeather.ZoneId, zoneWeather.TransitionProgress);
                }
                
                // 处理持续时间
                zoneWeather.RemainingTime -= deltaTime;
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
        /// 保存天气数据
        /// </summary>
        public void SaveWeatherData()
        {
            if (!SaveSystem.IsLoaded) return;
            
            SaveSystem.CurrentSave.WeatherData = new WeatherSaveData
            {
                ZoneWeathers = new List<ZoneWeather>(_zoneWeathers.Values),
                WeatherEvents = _weatherEvents,
                Statistics = _statistics
            };
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
