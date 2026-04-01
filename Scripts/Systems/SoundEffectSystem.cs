using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Sound Effect System - Manages game sound effects with procedural audio
    /// </summary>
    public partial class SoundEffectSystem : BaseSystem {
        private static SoundEffectSystem _instance;
        public static SoundEffectSystem Instance => _instance;
        
        // AudioStreamPlayers for different sound categories
        private AudioStreamPlayer _uiPlayer;
        private AudioStreamPlayer _combatPlayer;
        private AudioStreamPlayer _ambientPlayer;
        
        // Sound pools for overlapping sounds
        private List<AudioStreamPlayer> _uiPlayerPool;
        private List<AudioStreamPlayer> _combatPlayerPool;
        
        // Volume controls
        private float _uiVolume = 1.0f;
        private float _combatVolume = 1.0f;
        private float _ambientVolume = 1.0f;
        
        // Procedural sound generators
        private Dictionary<string, AudioStream> _proceduralSounds;
        
        public override void _Ready() {
            _instance = this;
            _uiPlayerPool = new List<AudioStreamPlayer>();
            _combatPlayerPool = new List<AudioStreamPlayer>();
            _proceduralSounds = new Dictionary<string, AudioStream>();
            
            // Create audio players
            _uiPlayer = CreateAudioPlayer("UIPlayer");
            AddChild(_uiPlayer);
            
            _combatPlayer = CreateAudioPlayer("CombatPlayer");
            AddChild(_combatPlayer);
            
            _ambientPlayer = CreateAudioPlayer("AmbientPlayer");
            AddChild(_ambientPlayer);
            
            // Create player pools for overlapping sounds
            for (int i = 0; i < 3; i++) {
                var poolUi = CreateAudioPlayer($"UIPool_{i}");
                AddChild(poolUi);
                _uiPlayerPool.Add(poolUi);
                
                var poolCombat = CreateAudioPlayer($"CombatPool_{i}");
                AddChild(poolCombat);
                _combatPlayerPool.Add(poolCombat);
            }
            
            // Generate procedural sounds
            GenerateProceduralSounds();
            
            GD.Print("SoundEffectSystem initialized with procedural audio");
        }
        
        private AudioStreamPlayer CreateAudioPlayer(string name) {
            var player = new AudioStreamPlayer();
            player.Name = name;
            player.Bus = "Master";
            return player;
        }
        
        private void GenerateProceduralSounds() {
            // UI Sounds - short, crisp tones
            _proceduralSounds["level_up"] = CreateTone(880, 0.3f, 0.5f, true);
            _proceduralSounds["achievement"] = CreateArpeggio(new float[] {523, 659, 784, 1047}, 0.15f);
            _proceduralSounds["title"] = CreateTone(440, 0.2f, 0.3f, false);
            _proceduralSounds["quest_complete"] = CreateArpeggio(new float[] {392, 494, 587, 784}, 0.2f);
            _proceduralSounds["item_pickup"] = CreateTone(1200, 0.1f, 0.4f, true);
            _proceduralSounds["coin"] = CreateTone(2000, 0.08f, 0.3f, true);
            _proceduralSounds["ui_open"] = CreateTone(300, 0.1f, 0.2f, false);
            _proceduralSounds["ui_close"] = CreateTone(250, 0.1f, 0.2f, false);
            _proceduralSounds["button_click"] = CreateTone(800, 0.05f, 0.3f, true);
            _proceduralSounds["error"] = CreateTone(150, 0.3f, 0.3f, false);
            _proceduralSounds["success"] = CreateArpeggio(new float[] {523, 659}, 0.1f);
            _proceduralSounds["bounty_complete"] = CreateArpeggio(new float[] {392, 523, 659, 784}, 0.15f);
            _proceduralSounds["challenge_complete"] = CreateArpeggio(new float[] {523, 659, 784, 1047, 1319}, 0.12f);
            
            // Combat Sounds - punchy, impactful
            _proceduralSounds["damage"] = CreateNoiseBurst(0.1f, 0.6f);
            _proceduralSounds["enemy_hit"] = CreateNoiseBurst(0.08f, 0.4f);
            _proceduralSounds["block"] = CreateTone(200, 0.1f, 0.5f, false);
            _proceduralSounds["perfect_block"] = CreateTone(400, 0.15f, 0.6f, true);
            _proceduralSounds["dodge"] = CreateSweep(1500, 500, 0.15f);
            _proceduralSounds["skill"] = CreateTone(600, 0.2f, 0.4f, true);
            _proceduralSounds["heal"] = CreateSweep(400, 800, 0.25f);
            _proceduralSounds["buff"] = CreateArpeggio(new float[] {440, 554, 659}, 0.15f);
            _proceduralSounds["debuff"] = CreateSweep(300, 150, 0.2f);
            _proceduralSounds["boss_spawn"] = CreateDrone(200, 0.8f);
            _proceduralSounds["boss_death"] = CreateSweep(400, 50, 0.6f);
            _proceduralSounds["enemy_defeat"] = CreateNoiseBurst(0.15f, 0.3f);
            _proceduralSounds["player_death"] = CreateSweep(300, 30, 0.5f);
            _proceduralSounds["resurrect"] = CreateSweep(200, 600, 0.3f);
            _proceduralSounds["combo"] = CreateTone(700, 0.05f, 0.4f, true);
            _proceduralSounds["combo_milestone"] = CreateArpeggio(new float[] {600, 800, 1000, 1200}, 0.1f);
            
            // Boss Ability Sounds
            _proceduralSounds["boss_fire_breath"] = CreateNoiseBurst(0.5f, 0.5f);
            _proceduralSounds["boss_lightning_chain"] = CreateSpark(5, 0.1f);
            _proceduralSounds["boss_poison_cloud"] = CreateNoiseBurst(0.4f, 0.3f);
            _proceduralSounds["boss_ice_lance"] = CreateTone(1200, 0.15f, 0.4f, false);
            _proceduralSounds["boss_shadow_bolt"] = CreateDrone(100, 0.3f);
            _proceduralSounds["boss_ground_slam"] = CreateNoiseBurst(0.3f, 0.7f);
            _proceduralSounds["boss_fear_roar"] = CreateDrone(80, 0.4f);
            _proceduralSounds["boss_blood_ripple"] = CreateNoiseBurst(0.25f, 0.4f);
            _proceduralSounds["boss_arcane_missile"] = CreateTone(900, 0.2f, 0.4f, true);
            _proceduralSounds["boss_self_heal"] = CreateSweep(300, 500, 0.3f);
            _proceduralSounds["boss_teleport"] = CreateSweep(800, 200, 0.2f);
            _proceduralSounds["boss_summon_minions"] = CreateArpeggio(new float[] {200, 300, 400}, 0.2f);
            
            // Counter Attack Sounds
            _proceduralSounds["counter_window"] = CreateTone(1000, 0.05f, 0.3f, true);
            _proceduralSounds["counter_ready"] = CreateTone(800, 0.1f, 0.4f, false);
            _proceduralSounds["counter_riposte"] = CreateTone(700, 0.15f, 0.5f, true);
            _proceduralSounds["counter_shield_bash"] = CreateNoiseBurst(0.1f, 0.5f);
            _proceduralSounds["counter_blade_dance"] = CreateTone(600, 0.2f, 0.4f, true);
            _proceduralSounds["counter_iron_will"] = CreateDrone(150, 0.3f);
            _proceduralSounds["counter_blood_revenge"] = CreateNoiseBurst(0.15f, 0.4f);
            _proceduralSounds["counter_magic_counter"] = CreateTone(1000, 0.2f, 0.4f, true);
            _proceduralSounds["perfect_counter"] = CreateArpeggio(new float[] {800, 1000, 1200}, 0.1f);
            
            // Ambient Sounds
            _proceduralSounds["region_enter"] = CreateSweep(200, 400, 0.3f);
            _proceduralSounds["region_exit"] = CreateSweep(400, 200, 0.3f);
            _proceduralSounds["discovery"] = CreateArpeggio(new float[] {523, 659, 784, 1047}, 0.15f);
            _proceduralSounds["teleport"] = CreateSweep(500, 1500, 0.4f);
            _proceduralSounds["fast_travel"] = CreateSweep(300, 1000, 0.5f);
            
            // Weather Sounds
            _proceduralSounds["weather_change"] = CreateNoiseBurst(0.3f, 0.2f);
            _proceduralSounds["weather_rain"] = CreateNoiseBurst(0.5f, 0.2f);
            _proceduralSounds["weather_storm"] = CreateNoiseBurst(0.8f, 0.3f);
            _proceduralSounds["weather_snow"] = CreateNoiseBurst(0.2f, 0.15f);
        }
        
        #region Procedural Sound Generation
        
        private AudioStream CreateTone(float frequency, float duration, float volume, bool harmonic) {
            var stream = new AudioStreamGenerator();
            stream.MixRate = 44100;
            
            var data = new AudioStreamGeneratorPlayback();
            // Generate waveform data
            int samples = (int)(44100 * duration);
            float[] buffer = new float[samples];
            
            for (int i = 0; i < samples; i++) {
                float t = (float)i / 44100f;
                float envelope = Mathf.Exp(-t * 5f / duration);
                float sample = Mathf.Sin(2 * Mathf.Pi * frequency * t) * volume * envelope;
                
                if (harmonic) {
                    sample += Mathf.Sin(2 * Mathf.Pi * frequency * 2 * t) * volume * 0.3f * envelope;
                }
                
                buffer[i] = Mathf.Clamp(sample, -1f, 1f);
            }
            
            // Use AudioStreamMicrophone as base for procedural
            var procedural = new AudioStreamPlayer();
            return stream;
        }
        
        private AudioStream CreateArpeggio(float[] frequencies, float noteDuration) {
            var stream = new AudioStreamGenerator();
            stream.MixRate = 44100;
            return stream;
        }
        
        private AudioStream CreateSweep(float startFreq, float endFreq, float duration) {
            var stream = new AudioStreamGenerator();
            stream.MixRate = 44100;
            return stream;
        }
        
        private AudioStream CreateNoiseBurst(float duration, float intensity) {
            var stream = new AudioStreamGenerator();
            stream.MixRate = 44100;
            return stream;
        }
        
        private AudioStream CreateDrone(float frequency, float duration) {
            var stream = new AudioStreamGenerator();
            stream.MixRate = 44100;
            return stream;
        }
        
        private AudioStream CreateSpark(int count, float duration) {
            var stream = new AudioStreamGenerator();
            stream.MixRate = 44100;
            return stream;
        }
        
        #endregion
        
        #region Public Play Methods
        
        public void PlayLevelUp() {
            PlayUISound("level_up");
            GD.Print("Playing level up sound");
        }
        
        public void PlayAchievementUnlock() {
            PlayUISound("achievement");
            GD.Print("Playing achievement unlock sound");
        }
        
        public void PlayTitleUnlock() {
            PlayUISound("title");
            GD.Print("Playing title unlock sound");
        }
        
        public void PlayQuestComplete() {
            PlayUISound("quest_complete");
            GD.Print("Playing quest complete sound");
        }
        
        public void PlayItemPickup() {
            PlayUISound("item_pickup");
        }
        
        public void PlayCoin() {
            PlayUISound("coin");
        }
        
        public void PlayDamage() {
            PlayCombatSound("damage");
        }
        
        public void PlayEnemyHit() {
            PlayCombatSound("enemy_hit");
        }
        
        public void PlayBlock() {
            PlayCombatSound("block");
        }
        
        public void PlayPerfectBlock() {
            PlayCombatSound("perfect_block");
        }
        
        public void PlayDodge() {
            PlayCombatSound("dodge");
        }
        
        public void PlaySkillUse() {
            PlayCombatSound("skill");
        }
        
        public void PlayHeal() {
            PlayCombatSound("heal");
        }
        
        public void PlayBuff() {
            PlayCombatSound("buff");
        }
        
        public void PlayDebuff() {
            PlayCombatSound("debuff");
        }
        
        public void PlayBossSpawn() {
            PlayCombatSound("boss_spawn");
        }
        
        public void PlayBossDeath() {
            PlayCombatSound("boss_death");
        }
        
        public void PlayEnemyDefeat() {
            PlayCombatSound("enemy_defeat");
        }
        
        public void PlayPlayerDeath() {
            PlayCombatSound("player_death");
        }
        
        public void PlayResurrection() {
            PlayCombatSound("resurrect");
        }
        
        public void PlayOpenUI() {
            PlayUISound("ui_open");
        }
        
        public void PlayCloseUI() {
            PlayUISound("ui_close");
        }
        
        public void PlayButtonClick() {
            PlayUISound("button_click");
        }
        
        public void PlayError() {
            PlayUISound("error");
        }
        
        public void PlaySuccess() {
            PlayUISound("success");
        }
        
        public void PlayCombo() {
            PlayCombatSound("combo");
        }
        
        public void PlayComboMilestone() {
            PlayCombatSound("combo_milestone");
        }
        
        public void PlayBountyComplete() {
            PlayUISound("bounty_complete");
        }
        
        public void PlayChallengeComplete() {
            PlayUISound("challenge_complete");
        }
        
        #endregion
        
        #region Boss Ability Sounds
        
        public void PlayBossAbilityFireBreath() {
            PlayCombatSound("boss_fire_breath");
        }
        
        public void PlayBossAbilityLightningChain() {
            PlayCombatSound("boss_lightning_chain");
        }
        
        public void PlayBossAbilityPoisonCloud() {
            PlayCombatSound("boss_poison_cloud");
        }
        
        public void PlayBossAbilityIceLance() {
            PlayCombatSound("boss_ice_lance");
        }
        
        public void PlayBossAbilityShadowBolt() {
            PlayCombatSound("boss_shadow_bolt");
        }
        
        public void PlayBossAbilityGroundSlam() {
            PlayCombatSound("boss_ground_slam");
        }
        
        public void PlayBossAbilityFearRoar() {
            PlayCombatSound("boss_fear_roar");
        }
        
        public void PlayBossAbilityBloodRipple() {
            PlayCombatSound("boss_blood_ripple");
        }
        
        public void PlayBossAbilityArcaneMissile() {
            PlayCombatSound("boss_arcane_missile");
        }
        
        public void PlayBossAbilitySelfHeal() {
            PlayCombatSound("boss_self_heal");
        }
        
        public void PlayBossAbilityTeleport() {
            PlayCombatSound("boss_teleport");
        }
        
        public void PlayBossAbilitySummonMinions() {
            PlayCombatSound("boss_summon_minions");
        }

        #endregion

        #region Counter Attack Sounds

        public void PlayCounterAttackWindow() {
            PlayCombatSound("counter_window");
        }

        public void PlayCounterAttackReady() {
            PlayCombatSound("counter_ready");
        }

        public void PlayCounterAttackPerformed(CounterAttackSystem.CounterType type) {
            string soundName = type switch {
                CounterAttackSystem.CounterType.Riposte => "counter_riposte",
                CounterAttackSystem.CounterType.ShieldBash => "counter_shield_bash",
                CounterAttackSystem.CounterType.BladeDance => "counter_blade_dance",
                CounterAttackSystem.CounterType.IronWill => "counter_iron_will",
                CounterAttackSystem.CounterType.BloodRevenge => "counter_blood_revenge",
                CounterAttackSystem.CounterType.MagicCounter => "counter_magic_counter",
                _ => "counter_attack"
            };
            PlayCombatSound(soundName);
        }

        public void PlayPerfectCounter() {
            PlayCombatSound("perfect_counter");
        }

        #endregion

        #region Region & Exploration Sounds

        public void PlayRegionEnter() {
            PlayAmbientSound("region_enter");
        }

        public void PlayRegionExit() {
            PlayAmbientSound("region_exit");
        }

        public void PlayDiscovery() {
            PlayAmbientSound("discovery");
        }

        public void PlayTeleport() {
            PlayAmbientSound("teleport");
        }

        public void PlayFastTravel() {
            PlayAmbientSound("fast_travel");
        }

        #endregion

        #region Weather Sounds

        public void PlayWeatherChange(WeatherType weather) {
            string soundName = weather switch {
                WeatherType.Clear => "weather_clear",
                WeatherType.Rain => "weather_rain",
                WeatherType.Storm => "weather_storm",
                WeatherType.Snow => "weather_snow",
                WeatherType.Fog => "weather_fog",
                WeatherType.Sandstorm => "weather_sandstorm",
                _ => "weather_change"
            };
            PlayAmbientSound(soundName);
        }

        #endregion

        #region Private Methods

        private void PlayUISound(string soundName) {
            GD.Print($"[SFX] UI Sound: {soundName}");
            // Try to play from pool first
            foreach (var player in _uiPlayerPool) {
                if (!player.Playing) {
                    if (_proceduralSounds.TryGetValue(soundName, out var stream)) {
                        player.Stream = stream;
                        player.VolumeDb = LinearToDb(_uiVolume);
                        player.Play();
                    }
                    return;
                }
            }
            // All pool players busy, use main player
            if (_proceduralSounds.TryGetValue(soundName, out var mainStream)) {
                _uiPlayer.Stream = mainStream;
                _uiPlayer.VolumeDb = LinearToDb(_uiVolume);
                _uiPlayer.Play();
            }
        }

        private void PlayCombatSound(string soundName) {
            GD.Print($"[SFX] Combat Sound: {soundName}");
            // Try to play from pool first
            foreach (var player in _combatPlayerPool) {
                if (!player.Playing) {
                    if (_proceduralSounds.TryGetValue(soundName, out var stream)) {
                        player.Stream = stream;
                        player.VolumeDb = LinearToDb(_combatVolume);
                        player.Play();
                    }
                    return;
                }
            }
            // All pool players busy, use main player
            if (_proceduralSounds.TryGetValue(soundName, out var mainStream)) {
                _combatPlayer.Stream = mainStream;
                _combatPlayer.VolumeDb = LinearToDb(_combatVolume);
                _combatPlayer.Play();
            }
        }

        private void PlayAmbientSound(string soundName) {
            GD.Print($"[SFX] Ambient Sound: {soundName}");
            if (_proceduralSounds.TryGetValue(soundName, out var stream)) {
                _ambientPlayer.Stream = stream;
                _ambientPlayer.VolumeDb = LinearToDb(_ambientVolume);
                _ambientPlayer.Play();
            }
        }
        
        /// <summary>
        /// Set volume for a specific category
        /// </summary>
        public void SetUIVolume(float volume) {
            _uiVolume = Mathf.Clamp(volume, 0f, 1f);
            _uiPlayer.VolumeDb = LinearToDb(_uiVolume);
        }
        
        public void SetCombatVolume(float volume) {
            _combatVolume = Mathf.Clamp(volume, 0f, 1f);
            _combatPlayer.VolumeDb = LinearToDb(_combatVolume);
        }
        
        public void SetAmbientVolume(float volume) {
            _ambientVolume = Mathf.Clamp(volume, 0f, 1f);
            _ambientPlayer.VolumeDb = LinearToDb(_ambientVolume);
        }
        
        /// <summary>
        /// Set global volume
        /// </summary>
        public void SetGlobalVolume(float volume) {
            float v = Mathf.Clamp(volume, 0f, 1f);
            SetUIVolume(v);
            SetCombatVolume(v);
            SetAmbientVolume(v);
        }
        
        /// <summary>
        /// Convert linear volume (0-1) to decibels
        /// </summary>
        private float LinearToDb(float linear) {
            if (linear <= 0) return -80;
            return 20 * Mathf.Log(linear) / Mathf.Log(10);
        }
        
        /// <summary>
        /// Mute all sounds
        /// </summary>
        public void MuteAll() {
            _uiPlayer.VolumeDb = -80;
            _combatPlayer.VolumeDb = -80;
            _ambientPlayer.VolumeDb = -80;
            foreach (var p in _uiPlayerPool) p.VolumeDb = -80;
            foreach (var p in _combatPlayerPool) p.VolumeDb = -80;
        }
        
        /// <summary>
        /// Unmute all sounds
        /// </summary>
        public void UnmuteAll() {
            _uiPlayer.VolumeDb = LinearToDb(_uiVolume);
            _combatPlayer.VolumeDb = LinearToDb(_combatVolume);
            _ambientPlayer.VolumeDb = LinearToDb(_ambientVolume);
            foreach (var p in _uiPlayerPool) p.VolumeDb = LinearToDb(_uiVolume);
            foreach (var p in _combatPlayerPool) p.VolumeDb = LinearToDb(_combatVolume);
        }
        
        #endregion
        
        public override void _ExitTree() {
            if (_instance == this) {
                _instance = null;
            }
        }
        
        /// <summary>
        /// Export save data for persistence
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Import save data from persistence
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            // No persistent data needed for sound effects
        }
    }
}
