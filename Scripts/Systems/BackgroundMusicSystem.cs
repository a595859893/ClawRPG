using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Background Music System - Manages dynamic background music
    /// </summary>
    public partial class BackgroundMusicSystem : BaseSystem {
        private static BackgroundMusicSystem _instance;
        public static BackgroundMusicSystem Instance => _instance;
        
        // Audio stream player for background music
        private AudioStreamPlayer _musicPlayer;
        private AudioStreamPlayer _battleMusicPlayer;
        
        // Crossfade player for smooth transitions
        private AudioStreamPlayer _crossfadePlayer;
        
        // Volume control
        private float _musicVolume = 0.7f;
        private float _battleMusicVolume = 0.8f;
        
        // Track management
        private string _currentTrack = "";
        private string _currentBattleTrack = "";
        private bool _inBattle = false; 
        
        // Crossfade settings
        private float _crossfadeDuration = 2.0f;
        private float _crossfadeTimer = 0f;
        private bool _isCrossfading = false; 
        private AudioStream _nextTrack = null;
        
        // Music database - maps zone/event to music tracks
        private Dictionary<string, MusicTrack> _musicDatabase;
        
        // Current zone
        private string _currentZone = "default";

        // ===== 战斗节拍感知 (REQ-131) =====
        // 节奏强度映射: Calm→0.0, Normal→0.3, Intense→0.6, Frenzied→1.0
        private float _rhythmIntensity = 0f;
        private bool _rhythmSubscriptionActive = false;
        
        public override void _Ready() {
            _instance = this;
            _musicDatabase = new Dictionary<string, MusicTrack>();
            
            // Create music players
            _musicPlayer = new AudioStreamPlayer();
            _musicPlayer.Name = "MusicPlayer";
            _musicPlayer.Bus = "Music";
        }
        
        /// <summary>
        /// 系统名称
        /// </summary>
        protected override string SystemName => "BackgroundMusic";
            AddChild(_musicPlayer);
            
            _battleMusicPlayer = new AudioStreamPlayer();
            _battleMusicPlayer.Name = "BattleMusicPlayer";
            _battleMusicPlayer.Bus = "Music";
            AddChild(_battleMusicPlayer);
            
            _crossfadePlayer = new AudioStreamPlayer();
            _crossfadePlayer.Name = "CrossfadePlayer";
            _crossfadePlayer.Bus = "Music";
            AddChild(_crossfadePlayer);
            
            // Setup music database
            InitializeMusicDatabase();

            // 订阅战斗节拍信号 (REQ-131)
            SubscribeToRhythmEvents();

            GD.Print("BackgroundMusicSystem initialized");
        }
        
        private void InitializeMusicDatabase() {
            // Zone music tracks (placeholder - would load actual audio files)
            _musicDatabase["default"] = new MusicTrack { 
                Name = "Default Theme", 
                Category = MusicCategory.Exploration,
                Intensity = MusicIntensity.Low
            };
            
            _musicDatabase["forest"] = new MusicTrack {
                Name = "Forest Theme",
                Category = MusicCategory.Exploration,
                Intensity = MusicIntensity.Low
            };
            
            _musicDatabase["dungeon"] = new MusicTrack {
                Name = "Dungeon Theme",
                Category = MusicCategory.Exploration,
                Intensity = MusicIntensity.Medium
            };
            
            _musicDatabase["town"] = new MusicTrack {
                Name = "Town Theme",
                Category = MusicCategory.Exploration,
                Intensity = MusicIntensity.Low
            };
            
            _musicDatabase["castle"] = new MusicTrack {
                Name = "Castle Theme",
                Category = MusicCategory.Exploration,
                Intensity = MusicIntensity.Medium
            };
            
            _musicDatabase["mountain"] = new MusicTrack {
                Name = "Mountain Theme",
                Category = MusicCategory.Exploration,
                Intensity = MusicIntensity.Medium
            };
            
            _musicDatabase["volcano"] = new MusicTrack {
                Name = "Volcano Theme",
                Category = MusicCategory.Exploration,
                Intensity = MusicIntensity.High
            };
            
            _musicDatabase["underwater"] = new MusicTrack {
                Name = "Underwater Theme",
                Category = MusicCategory.Exploration,
                Intensity = MusicIntensity.Low
            };
            
            _musicDatabase["menu"] = new MusicTrack {
                Name = "Main Menu",
                Category = MusicCategory.Menu,
                Intensity = MusicIntensity.Low
            };
            
            // Battle music
            _musicDatabase["battle_normal"] = new MusicTrack {
                Name = "Normal Battle",
                Category = MusicCategory.Battle,
                Intensity = MusicIntensity.Medium
            };
            
            _musicDatabase["battle_boss"] = new MusicTrack {
                Name = "Boss Battle",
                Category = MusicCategory.Battle,
                Intensity = MusicIntensity.High
            };
            
            _musicDatabase["battle_miniboss"] = new MusicTrack {
                Name = "Mini Boss",
                Category = MusicCategory.Battle,
                Intensity = MusicIntensity.MediumHigh
            };
            
            // Event music
            _musicDatabase["event_victory"] = new MusicTrack {
                Name = "Victory Fanfare",
                Category = MusicCategory.Event,
                Intensity = MusicIntensity.High
            };
            
            _musicDatabase["event_defeat"] = new MusicTrack {
                Name = "Defeat Theme",
                Category = MusicCategory.Event,
                Intensity = MusicIntensity.Low
            };
            
            _musicDatabase["event_shop"] = new MusicTrack {
                Name = "Shop Theme",
                Category = MusicCategory.Event,
                Intensity = MusicIntensity.Low
            };
            
            _musicDatabase["event_inn"] = new MusicTrack {
                Name = "Inn Theme",
                Category = MusicCategory.Event,
                Intensity = MusicIntensity.Low
            };
        }
        
        #region Public Methods
        
        /// <summary>
        /// Change to a specific zone's music
        /// </summary>
        public void ChangeZoneMusic(string zoneName) {
            if (_musicDatabase.TryGetValue(zoneName, out var track)) {
                _currentZone = zoneName;
                PlayMusic(track.Name);
                GD.Print($"[BGM] Changed to zone: {zoneName} - {track.Name}");
            }
        }
        
        /// <summary>
        /// Start battle music
        /// </summary>
        public void StartBattleMusic(bool isBoss = false) {
            _inBattle = true;
            string trackKey = isBoss ? "battle_boss" : "battle_normal";

            if (_musicDatabase.TryGetValue(trackKey, out var track)) {
                _currentBattleTrack = track.Name;
                CrossfadeTo(track.Name);
                GD.Print($"[BGM] Starting {(isBoss ? "boss" : "normal")} battle music");
            }

            // 应用当前节奏强度到战斗音乐 (REQ-131)
            var rhythmData = CombatRhythmData.Instance;
            if (rhythmData != null) {
                float targetIntensity = LevelToIntensity(rhythmData.GetCurrentLevel());
                _rhythmIntensity = targetIntensity;
                float rhythmBoost = targetIntensity * 0.3f;
                float effectiveBattleVolume = Mathf.Clamp(_battleMusicVolume + rhythmBoost, 0f, 1f);
                _battleMusicPlayer.VolumeDb = LinearToDb(effectiveBattleVolume);
            }

            // Emit signal for battle start
            EmitSignal(nameof(BattleMusicStarted), isBoss);
        }
        
        /// <summary>
        /// Stop battle music and return to exploration
        /// </summary>
        public void StopBattleMusic() {
            if (_inBattle) {
                _inBattle = false;
                _rhythmIntensity = 0f; // 退出战斗重置节奏强度 (REQ-131)
                CrossfadeToZoneMusic(_currentZone);
                GD.Print("[BGM] Stopping battle music");

                EmitSignal(nameof(BattleMusicStopped));
            }
        }
        
        /// <summary>
        /// Play victory music
        /// </summary>
        public void PlayVictoryMusic() {
            if (_musicDatabase.TryGetValue("event_victory", out var track)) {
                PlayMusic(track.Name);
                GD.Print("[BGM] Playing victory music");
            }
        }
        
        /// <summary>
        /// Play defeat music
        /// </summary>
        public void PlayDefeatMusic() {
            if (_musicDatabase.TryGetValue("event_defeat", out var track)) {
                PlayMusic(track.Name);
                GD.Print("[BGM] Playing defeat music");
            }
        }
        
        /// <summary>
        /// Play shop music
        /// </summary>
        public void PlayShopMusic() {
            if (_musicDatabase.TryGetValue("event_shop", out var track)) {
                PlayMusic(track.Name);
                GD.Print("[BGM] Playing shop music");
            }
        }
        
        /// <summary>
        /// Play inn/rest music
        /// </summary>
        public void PlayInnMusic() {
            if (_musicDatabase.TryGetValue("event_inn", out var track)) {
                PlayMusic(track.Name);
                GD.Print("[BGM] Playing inn music");
            }
        }
        
        /// <summary>
        /// Play main menu music
        /// </summary>
        public void PlayMenuMusic() {
            if (_musicDatabase.TryGetValue("menu", out var track)) {
                PlayMusic(track.Name);
                GD.Print("[BGM] Playing menu music");
            }
        }
        
        /// <summary>
        /// Pause current music
        /// </summary>
        public void PauseMusic() {
            _musicPlayer.StreamPaused = true;
            _battleMusicPlayer.StreamPaused = true;
        }
        
        /// <summary>
        /// Resume music
        /// </summary>
        public void ResumeMusic() {
            _musicPlayer.StreamPaused = false; 
            _battleMusicPlayer.StreamPaused = false; 
        }
        
        /// <summary>
        /// Stop all music
        /// </summary>
        public void StopMusic() {
            _musicPlayer.Stop();
            _battleMusicPlayer.Stop();
            _crossfadePlayer.Stop();
            _currentTrack = "";
            _currentBattleTrack = "";
        }
        
        /// <summary>
        /// Set music volume (0.0 - 1.0)
        /// </summary>
        public void SetVolume(float volume) {
            _musicVolume = Mathf.Clamp(volume, 0f, 1f);
            _musicPlayer.VolumeDb = LinearToDb(_musicVolume);
            _battleMusicPlayer.VolumeDb = LinearToDb(_battleMusicVolume);
        }
        
        /// <summary>
        /// Set crossfade duration
        /// </summary>
        public void SetCrossfadeDuration(float duration) {
            _crossfadeDuration = Mathf.Max(0.5f, duration);
        }
        
        /// <summary>
        /// Get current track name
        /// </summary>
        public string GetCurrentTrack() => _currentTrack;
        
        /// <summary>
        /// Check if in battle
        /// </summary>
        public bool IsInBattle() => _inBattle;
        
        #endregion
        
        #region Private Methods
        
        private void PlayMusic(string trackName) {
            // In a full implementation, this would load and play actual audio files
            // For now, we just log the track being played
            _currentTrack = trackName;
            _musicPlayer.Play();
            GD.Print($"[BGM] Playing: {trackName}");
        }
        
        private void CrossfadeTo(string trackName) {
            // Crossfade logic would be implemented here
            _currentTrack = trackName;
            _musicPlayer.Play();
            GD.Print($"[BGM] Crossfading to: {trackName}");
        }
        
        private void CrossfadeToZoneMusic(string zoneName) {
            if (_musicDatabase.TryGetValue(zoneName, out var track)) {
                CrossfadeTo(track.Name);
            }
        }
        
        private float LinearToDb(float linear) {
            if (linear <= 0) return -80;
            return 20 * Mathf.Log(linear) / Mathf.Log(10);
        }

        #region 战斗节拍感知 (REQ-131)

        private void SubscribeToRhythmEvents()
        {
            if (_rhythmSubscriptionActive) return;
            var rhythmData = CombatRhythmData.Instance;
            if (rhythmData == null)
            {
                GD.Print("[BGM] CombatRhythmData not found — rhythm sync disabled");
                return;
            }
            rhythmData.RhythmLevelChanged += OnRhythmLevelChanged;
            _rhythmSubscriptionActive = true;
            GD.Print("[BGM] Subscribed to CombatRhythmData.RhythmLevelChanged");
        }

        private void OnRhythmLevelChanged(CombatRhythmData.RhythmLevel newLevel, CombatRhythmData.RhythmLevel oldLevel)
        {
            float targetIntensity = LevelToIntensity(newLevel);
            _rhythmIntensity = targetIntensity;

            if (!_inBattle) return;

            // 根据节奏等级调整战斗音乐强度
            // 基础音量 + 节奏增幅（最高+30%）
            float rhythmBoost = targetIntensity * 0.3f;
            float effectiveBattleVolume = _battleMusicVolume + rhythmBoost;
            effectiveBattleVolume = Mathf.Clamp(effectiveBattleVolume, 0f, 1f);

            // 应用到 battle music player
            _battleMusicPlayer.VolumeDb = LinearToDb(effectiveBattleVolume);
            GD.Print($"[BGM] Rhythm → {newLevel} (intensity={targetIntensity:F1}), battle volume: {effectiveBattleVolume:F2}");
        }

        private float LevelToIntensity(CombatRhythmData.RhythmLevel level)
        {
            return level switch
            {
                CombatRhythmData.RhythmLevel.Calm => 0.0f,
                CombatRhythmData.RhythmLevel.Normal => 0.3f,
                CombatRhythmData.RhythmLevel.Intense => 0.6f,
                CombatRhythmData.RhythmLevel.Frenzied => 1.0f,
                _ => 0f
            };
        }

        /// <summary>
        /// 获取当前节奏强度 (0.0–1.0)
        /// </summary>
        public float GetRhythmIntensity() => _rhythmIntensity;

        #endregion

        #region Process
        
        public override void _Process(float delta) {
            // Handle crossfade
            if (_isCrossfading) {
                _crossfadeTimer += delta;
                float progress = _crossfadeTimer / _crossfadeDuration;
                
                if (progress >= 1.0f) {
                    _isCrossfading = false; 
                    _crossfadeTimer = 0f;
                    _musicPlayer.VolumeDb = LinearToDb(_musicVolume);
                } else {
                    // Fade out current, fade in new
                    float volume = Mathf.Sin(progress * Mathf.PI) * _musicVolume;
                    _musicPlayer.VolumeDb = LinearToDb(volume);
                }
            }
        }
        
        #endregion
        
        #region Signals
        
        [Signal]
        public delegate void BattleMusicStarted(bool isBoss);
        
        [Signal]
        public delegate void BattleMusicStopped();
        
        [Signal]
        public delegate void TrackChanged(string trackName);
        
        #endregion
        
        public override void _ExitTree() {
            if (_instance == this) {
                _instance = null;
            }
        }
    }
    
    #region Data Classes
    
    public class MusicTrack {
        public string Name { get; set; } = "";
        public MusicCategory Category { get; set; } = MusicCategory.Exploration;
        public MusicIntensity Intensity { get; set; } = MusicIntensity.Low;
    }
    
    public enum MusicCategory {
        Exploration,
        Battle,
        Event,
        Menu,
        Cutscene
    }
    
    public enum MusicIntensity {
        Low,
        Medium,
        MediumHigh,
        High
    }
    
    #endregion

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        data["music_volume"] = _musicVolume;
        data["battle_music_volume"] = _battleMusicVolume;
        data["crossfade_duration"] = _crossfadeDuration;
        return data;
    }

    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        if (data.Contains("music_volume")) _musicVolume = (float)data["music_volume"];
        if (data.Contains("battle_music_volume")) _battleMusicVolume = (float)data["battle_music_volume"];
        if (data.Contains("crossfade_duration")) _crossfadeDuration = (float)data["crossfade_duration"];
    }
}
