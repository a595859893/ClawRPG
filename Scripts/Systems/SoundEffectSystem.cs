using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Sound Effect System - Manages game sound effects
    /// </summary>
    public class SoundEffectSystem : Node {
        private static SoundEffectSystem _instance;
        public static SoundEffectSystem Instance => _instance;
        
        // AudioStreamPlayers for different sound categories
        private AudioStreamPlayer _uiPlayer;
        private AudioStreamPlayer _combatPlayer;
        private AudioStreamPlayer _ambientPlayer;
        
        // Built-in sound data (procedural tones)
        private Dictionary<string, AudioStream> _soundCache;
        
        public override void _Ready() {
            _instance = this;
            _soundCache = new Dictionary<string, AudioStream>();
            
            // Create audio players
            _uiPlayer = new AudioStreamPlayer();
            _uiPlayer.Name = "UIPlayer";
            AddChild(_uiPlayer);
            
            _combatPlayer = new AudioStreamPlayer();
            _combatPlayer.Name = "CombatPlayer";
            AddChild(_combatPlayer);
            
            _ambientPlayer = new AudioStreamPlayer();
            _ambientPlayer.Name = "AmbientPlayer";
            AddChild(_ambientPlayer);
            
            GD.Print("SoundEffectSystem initialized");
        }
        
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
            // In a full implementation, this would play actual audio files
            // For now, we just log the sound being played
            GD.Print($"[SFX] UI Sound: {soundName}");
        }

        private void PlayCombatSound(string soundName) {
            GD.Print($"[SFX] Combat Sound: {soundName}");
        }

        private void PlayAmbientSound(string soundName) {
            GD.Print($"[SFX] Ambient Sound: {soundName}");
        }
        
        /// <summary>
        /// Load a sound from resources
        /// </summary>
        private AudioStream LoadSound(string path) {
            if (_soundCache.ContainsKey(path)) {
                return _soundCache[path];
            }
            
            if (ResourceLoader.Exists(path)) {
                var sound = ResourceLoader.Load<AudioStream>(path);
                _soundCache[path] = sound;
                return sound;
            }
            
            return null;
        }
        
        /// <summary>
        /// Set volume for a specific player
        /// </summary>
        public void SetUIVolume(float volume) {
            _uiPlayer.VolumeDb = LinearToDb(volume);
        }
        
        public void SetCombatVolume(float volume) {
            _combatPlayer.VolumeDb = LinearToDb(volume);
        }
        
        public void SetAmbientVolume(float volume) {
            _ambientPlayer.VolumeDb = LinearToDb(volume);
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
        }
        
        /// <summary>
        /// Unmute all sounds
        /// </summary>
        public void UnmuteAll() {
            _uiPlayer.VolumeDb = 0;
            _combatPlayer.VolumeDb = 0;
            _ambientPlayer.VolumeDb = 0;
        }
        
        #endregion
        
        public override void _ExitTree() {
            if (_instance == this) {
                _instance = null;
            }
        }
    }
}
