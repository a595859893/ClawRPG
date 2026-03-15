using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// 天气系统UI管理器
    /// </summary>
    public class WeatherUI : MonoBehaviour
    {
        private static WeatherUI _instance;
        public static WeatherUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<WeatherUI>();
                }
                return _instance;
            }
        }
        
        [Header("UI Components")]
        public GameObject weatherPanel;
        public Transform zoneListContainer;
        public Transform effectListContainer;
        
        [Header("Prefabs")]
        public GameObject zoneWeatherItemPrefab;
        public GameObject weatherEffectItemPrefab;
        
        [Header("Current Weather Display")]
        public Text currentZoneText;
        public Text currentWeatherText;
        public Text currentIntensityText;
        public Text remainingTimeText;
        public Image weatherIcon;
        public Slider transitionSlider;
        
        [Header("Statistics Display")]
        public Text totalEventsText;
        public Text playerExposureText;
        public Text currentSeasonText;
        
        private bool _isVisible;
        private string _currentZoneId = "town";
        
        private void Awake()
        {
            _instance = this;
            if (weatherPanel != null)
                weatherPanel.SetActive(false);
        }
        
        private void Start()
        {
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            // 注册天气变化事件
            WeatherSystem.OnWeatherChanged += OnWeatherChanged;
            WeatherSystem.OnTransitionProgress += OnTransitionProgress;
        }
        
        /// <summary>
        /// 切换天气UI显示
        /// </summary>
        public void ToggleWeatherUI()
        {
            _isVisible = !_isVisible;
            if (weatherPanel != null)
                weatherPanel.SetActive(_isVisible);
            
            if (_isVisible)
            {
                RefreshWeatherDisplay();
            }
        }
        
        /// <summary>
        /// 显示天气UI
        /// </summary>
        public void ShowWeatherUI()
        {
            _isVisible = true;
            if (weatherPanel != null)
                weatherPanel.SetActive(true);
            RefreshWeatherDisplay();
        }
        
        /// <summary>
        /// 隐藏天气UI
        /// </summary>
        public void HideWeatherUI()
        {
            _isVisible = false;
            if (weatherPanel != null)
                weatherPanel.SetActive(false);
        }
        
        /// <summary>
        /// 刷新天气显示
        /// </summary>
        public void RefreshWeatherDisplay()
        {
            if (WeatherSystem.Instance == null) return;
            
            // 刷新区域列表
            RefreshZoneList();
            
            // 刷新当前天气
            RefreshCurrentWeather();
            
            // 刷新统计
            RefreshStatistics();
            
            // 刷新效果列表
            RefreshEffectList();
        }
        
        /// <summary>
        /// 刷新区域列表
        /// </summary>
        private void RefreshZoneList()
        {
            if (zoneListContainer == null || zoneWeatherItemPrefab == null) return;
            
            // 清除旧列表
            foreach (Transform child in zoneListContainer)
            {
                Destroy(child.gameObject);
            }
            
            // 获取所有区域
            List<string> zoneIds = WeatherSystem.Instance.GetAllZoneIds();
            foreach (string zoneId in zoneIds)
            {
                ZoneWeather zoneWeather = WeatherSystem.Instance.GetZoneWeather(zoneId);
                if (zoneWeather == null) continue;
                
                GameObject item = Instantiate(zoneWeatherItemPrefab, zoneListContainer);
                
                // 设置区域信息
                Text zoneNameText = item.transform.Find("ZoneName")?.GetComponent<Text>();
                Text weatherText = item.transform.Find("WeatherName")?.GetComponent<Text>();
                Text timeText = item.transform.Find("RemainingTime")?.GetComponent<Text>();
                Image iconImage = item.transform.Find("WeatherIcon")?.GetComponent<Image>();
                
                if (zoneNameText != null)
                    zoneNameText.text = zoneWeather.ZoneName;
                
                if (weatherText != null)
                {
                    WeatherConfig config = WeatherDatabase.GetWeatherConfig(zoneWeather.CurrentWeather);
                    weatherText.text = config?.Name ?? "Unknown";
                }
                
                if (timeText != null)
                {
                    TimeSpan time = TimeSpan.FromSeconds(zoneWeather.RemainingTime);
                    timeText.text = time.ToString(@"mm\:ss");
                }
                
                // 点击选择区域
                Button btn = item.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => SelectZone(zoneId));
                }
            }
        }
        
        /// <summary>
        /// 刷新当前天气显示
        /// </summary>
        private void RefreshCurrentWeather()
        {
            if (WeatherSystem.Instance == null) return;
            
            ZoneWeather zoneWeather = WeatherSystem.Instance.GetZoneWeather(_currentZoneId);
            if (zoneWeather == null) return;
            
            WeatherConfig config = WeatherDatabase.GetWeatherConfig(zoneWeather.CurrentWeather);
            
            if (currentZoneText != null)
                currentZoneText.text = zoneWeather.ZoneName;
            
            if (currentWeatherText != null)
                currentWeatherText.text = config?.Name ?? "Unknown";
            
            if (currentIntensityText != null)
                currentIntensityText.text = zoneWeather.Intensity.ToString();
            
            if (remainingTimeText != null)
            {
                TimeSpan time = TimeSpan.FromSeconds(zoneWeather.RemainingTime);
                remainingTimeText.text = time.ToString(@"mm\:ss");
            }
            
            if (transitionSlider != null)
                transitionSlider.value = zoneWeather.TransitionProgress;
        }
        
        /// <summary>
        /// 刷新统计显示
        /// </summary>
        private void RefreshStatistics()
        {
            if (WeatherSystem.Instance == null) return;
            
            WeatherStatistics stats = WeatherSystem.Instance.GetStatistics();
            
            if (totalEventsText != null)
                totalEventsText.text = $"总天气事件: {stats.TotalWeatherEvents}";
            
            if (playerExposureText != null)
                playerExposureText.text = $"玩家暴露次数: {stats.PlayerWeatherEvents}";
            
            if (currentSeasonText != null)
                currentSeasonText.text = $"当前季节: {WeatherSystem.Instance.GetCurrentSeason()}";
        }
        
        /// <summary>
        /// 刷新效果列表
        /// </summary>
        private void RefreshEffectList()
        {
            if (effectListContainer == null || weatherEffectItemPrefab == null) return;
            
            // 清除旧列表
            foreach (Transform child in effectListContainer)
            {
                Destroy(child.gameObject);
            }
            
            if (WeatherSystem.Instance == null) return;
            
            ZoneWeather zoneWeather = WeatherSystem.Instance.GetZoneWeather(_currentZoneId);
            if (zoneWeather == null) return;
            
            List<WeatherEffect> effects = WeatherSystem.Instance.GetWeatherEffects(zoneWeather.CurrentWeather);
            
            foreach (var effect in effects)
            {
                GameObject item = Instantiate(weatherEffectItemPrefab, effectListContainer);
                
                Text effectText = item.transform.Find("EffectText")?.GetComponent<Text>();
                Text valueText = item.transform.Find("ValueText")?.GetComponent<Text>();
                
                if (effectText != null)
                    effectText.text = GetEffectTypeName(effect.Type);
                
                if (valueText != null)
                {
                    string sign = effect.Value >= 0 ? "+" : "";
                    valueText.text = $"{sign}{effect.Value:F0}%";
                    valueText.color = effect.Value >= 0 ? Color.green : Color.red;
                }
            }
        }
        
        /// <summary>
        /// 选择区域
        /// </summary>
        public void SelectZone(string zoneId)
        {
            _currentZoneId = zoneId;
            RefreshCurrentWeather();
            RefreshEffectList();
        }
        
        /// <summary>
        /// 天气变化回调
        /// </summary>
        private void OnWeatherChanged(string zoneId, WeatherType type, WeatherIntensity intensity)
        {
            if (_isVisible && zoneId == _currentZoneId)
            {
                RefreshCurrentWeather();
                RefreshEffectList();
            }
        }
        
        /// <summary>
        /// 过渡进度回调
        /// </summary>
        private void OnTransitionProgress(string zoneId, float progress)
        {
            if (_isVisible && zoneId == _currentZoneId && transitionSlider != null)
            {
                transitionSlider.value = progress;
            }
        }
        
        /// <summary>
        /// 获取效果类型名称
        /// </summary>
        private string GetEffectTypeName(WeatherEffectType type)
        {
            switch (type)
            {
                case WeatherEffectType.VisionReduction: return "视野";
                case WeatherEffectType.MovementSpeed: return "移动速度";
                case WeatherEffectType.AttackSpeed: return "攻击速度";
                case WeatherEffectType.Defense: return "防御力";
                case WeatherEffectType.HealthRegen: return "生命恢复";
                case WeatherEffectType.ManaRegen: return "法力恢复";
                case WeatherEffectType.CriticalRate: return "暴击率";
                case WeatherEffectType.DodgeRate: return "闪避率";
                case WeatherEffectType.ExperienceGain: return "经验获取";
                case WeatherEffectType.ItemDropRate: return "物品掉落";
                default: return type.ToString();
            }
        }
        
        /// <summary>
        /// 刷新按钮点击
        /// </summary>
        public void OnRefreshButtonClicked()
        {
            RefreshWeatherDisplay();
        }
        
        /// <summary>
        /// 强制刷新指定区域天气
        /// </summary>
        public void OnRefreshZoneButtonClicked()
        {
            if (WeatherSystem.Instance != null)
            {
                WeatherSystem.Instance.RefreshZoneWeather(_currentZoneId);
            }
        }
        
        /// <summary>
        /// 切换到下一区域
        /// </summary>
        public void OnNextZoneButtonClicked()
        {
            List<string> zoneIds = WeatherSystem.Instance.GetAllZoneIds();
            int currentIndex = zoneIds.IndexOf(_currentZoneId);
            int nextIndex = (currentIndex + 1) % zoneIds.Count;
            SelectZone(zoneIds[nextIndex]);
        }
        
        /// <summary>
        /// 切换到上一区域
        /// </summary>
        public void OnPreviousZoneButtonClicked()
        {
            List<string> zoneIds = WeatherSystem.Instance.GetAllZoneIds();
            int currentIndex = zoneIds.IndexOf(_currentZoneId);
            int prevIndex = (currentIndex - 1 + zoneIds.Count) % zoneIds.Count;
            SelectZone(zoneIds[prevIndex]);
        }
        
        private void OnDestroy()
        {
            if (WeatherSystem.Instance != null)
            {
                WeatherSystem.OnWeatherChanged -= OnWeatherChanged;
                WeatherSystem.OnTransitionProgress -= OnTransitionProgress;
            }
        }
    }
}
