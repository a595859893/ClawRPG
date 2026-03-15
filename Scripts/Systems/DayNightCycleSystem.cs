using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Day/Night Cycle System - Manages time progression and environmental effects
    /// </summary>
    public class DayNightCycleSystem : BaseSystem {
        // Time configuration
        private float _dayDuration = 600f; // 10 minutes real time = 24 in-game hours
        private float _currentTime = 0f; // 0-1 representing 0-24 hours
        private float _timeScale = 1f;
        
        // Current phase
        public enum TimePhase {
            Dawn,      // 5:00 - 7:00
            Day,       // 7:00 - 17:00
            Dusk,      // 17:00 - 19:00
            Night      // 19:00 - 5:00
        }
        
        private TimePhase _currentPhase = TimePhase.Day;
        
        // Environmental modifiers
        public float BrightnessMultiplier { get; private set; } = 1f;
        public float ExperienceMultiplier { get; private set; } = 1f;
        public float GoldMultiplier { get; private set; } = 1f;
        public float DropRateMultiplier { get; private set; } = 1f;
        public float SpeedMultiplier { get; private set; } = 1f;
        
        // Visual settings
        private Color _ambientLightColor = new Color(1, 1, 1);
        private float _ambientLightEnergy = 1f;
        
        // Time phase definitions (in hours, 0-24)
        private readonly Dictionary<TimePhase, (float start, float end, Color lightColor, float energy)> _phaseSettings = new() {
            { TimePhase.Dawn, (5f, 7f, new Color(1f, 0.8f, 0.6f), 0.7f) },
            { TimePhase.Day, (7f, 17f, new Color(1f, 1f, 1f), 1f) },
            { TimePhase.Dusk, (17f, 19f, new Color(1f, 0.6f, 0.4f), 0.6f) },
            { TimePhase.Night, (19f, 24f, new Color(0.3f, 0.3f, 0.5f), 0.3f) }
        };
        
        // Events
        public event Action<TimePhase> OnPhaseChanged;
        public event Action<float> OnTimeUpdated;
        
        public override void _Ready() {
            base._Ready();
            LoadSettings();
        }
        
        public override void _Process(float delta) {
            // Advance time
            _currentTime += delta * _timeScale * (24f / _dayDuration);
            if (_currentTime >= 24f) {
                _currentTime -= 24f;
            }
            
            UpdatePhase();
            UpdateModifiers();
            OnTimeUpdated?.Invoke(_currentTime);
        }
        
        private void UpdatePhase() {
            TimePhase newPhase = GetPhaseForTime(_currentTime);
            if (newPhase != _currentPhase) {
                _currentPhase = newPhase;
                UpdatePhaseVisuals();
                OnPhaseChanged?.Invoke(_currentPhase);
            }
        }
        
        private TimePhase GetPhaseForTime(float time) {
            foreach (var phase in _phaseSettings) {
                if (time >= phase.Value.start || time < phase.Value.end) {
                    return phase.Key;
                }
            }
            return TimePhase.Day;
        }
        
        private void UpdateModifiers() {
            // Update modifiers based on phase
            switch (_currentPhase) {
                case TimePhase.Dawn:
                    ExperienceMultiplier = 1.1f;
                    GoldMultiplier = 1.05f;
                    DropRateMultiplier = 1.1f;
                    SpeedMultiplier = 1.05f;
                    break;
                case TimePhase.Day:
                    ExperienceMultiplier = 1.0f;
                    GoldMultiplier = 1.0f;
                    DropRateMultiplier = 1.0f;
                    SpeedMultiplier = 1.0f;
                    break;
                case TimePhase.Dusk:
                    ExperienceMultiplier = 1.15f;
                    GoldMultiplier = 1.1f;
                    DropRateMultiplier = 1.2f;
                    SpeedMultiplier = 0.95f;
                    break;
                case TimePhase.Night:
                    ExperienceMultiplier = 1.25f;
                    GoldMultiplier = 1.2f;
                    DropRateMultiplier = 1.3f;
                    SpeedMultiplier = 0.9f;
                    break;
            }
        }
        
        private void UpdatePhaseVisuals() {
            if (_phaseSettings.TryGetValue(_currentPhase, out var settings)) {
                _ambientLightColor = settings.lightColor;
                _ambientLightEnergy = settings.energy;
                BrightnessMultiplier = settings.energy;
            }
        }
        
        // Public API
        public float GetCurrentTime() => _currentTime;
        
        public TimePhase GetCurrentPhase() => _currentPhase;
        
        public string GetTimeString() {
            int hours = (int)_currentTime;
            int minutes = (int)((_currentTime - hours) * 60);
            return $"{hours:D2}:{minutes:D2}";
        }
        
        public string GetPhaseName() {
            return _currentPhase switch {
                TimePhase.Dawn => "Dawn (黎明)",
                TimePhase.Day => "Day (白天)",
                TimePhase.Dusk => "Dusk (黄昏)",
                TimePhase.Night => "Night (夜晚)",
                _ => "Unknown"
            };
        }
        
        public void SetTimeScale(float scale) {
            _timeScale = Mathf.Clamp(scale, 0f, 10f);
        }
        
        public void SetTime(float hour) {
            _currentTime = Mathf.Clamp(hour, 0f, 24f);
            UpdatePhase();
            UpdateModifiers();
        }
        
        public Color GetAmbientLightColor() => _ambientLightColor;
        public float GetAmbientLightEnergy() => _ambientLightEnergy;
        
        // Settings
        public void LoadSettings() {
            // Load from save
        }
        
        #region Data Persistence
        
        public override Dictionary ExportSaveData()
        {
            return new Dictionary
            {
                { "current_time", _currentTime },
                { "time_scale", _timeScale },
                { "day_duration", _dayDuration }
            };
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            if (data.TryGetValue("current_time", out var time)) {
                _currentTime = Convert.ToSingle(time);
            }
            if (data.TryGetValue("time_scale", out var scale)) {
                _timeScale = Convert.ToSingle(scale);
            }
            if (data.TryGetValue("day_duration", out var duration)) {
                _dayDuration = Convert.ToSingle(duration);
            }
            
            UpdatePhase();
            UpdateModifiers();
        }
        
        #endregion
    }
}
