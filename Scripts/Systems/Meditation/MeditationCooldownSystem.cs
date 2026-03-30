using Godot;
using System;
using System.Collections.Generic;
using Framework;

namespace ClawRPG.Systems.Meditation
{
    /// <summary>
    /// Meditation Cooldown System - Handles cooldown management
    /// </summary>
    public partial class MeditationCooldownSystem : BaseSystem
    {
        public static MeditationCooldownSystem Instance { get; private set; }

        // Cooldown tracking
        private Dictionary<string, Dictionary<MeditationType, DateTime>> _cooldowns = new Dictionary<string, Dictionary<MeditationType, DateTime>>();

        public override void _Ready()
        {
            base._Ready();
            Instance = this;
        }

        /// <summary>
        /// Check if meditation type is on cooldown
        /// </summary>
        public bool IsOnCooldown(string playerId, MeditationType type)
        {
            if (!_cooldowns.ContainsKey(playerId))
                return false;

            if (!_cooldowns[playerId].ContainsKey(type))
                return false;

            return DateTime.Now < _cooldowns[playerId][type];
        }

        /// <summary>
        /// Get remaining cooldown time in seconds
        /// </summary>
        public int GetCooldownRemaining(string playerId, MeditationType type)
        {
            if (!IsOnCooldown(playerId, type))
                return 0;

            var cooldownEnd = _cooldowns[playerId][type];
            return (int)(cooldownEnd - DateTime.Now).TotalSeconds;
        }

        /// <summary>
        /// Set cooldown for meditation type
        /// </summary>
        public void SetCooldown(string playerId, MeditationType type, TimeSpan duration)
        {
            if (!_cooldowns.ContainsKey(playerId))
                _cooldowns[playerId] = new Dictionary<MeditationType, DateTime>();

            _cooldowns[playerId][type] = DateTime.Now.Add(duration);
        }

        /// <summary>
        /// Clear all cooldowns for a player
        /// </summary>
        public void ClearCooldowns(string playerId)
        {
            if (_cooldowns.ContainsKey(playerId))
            {
                _cooldowns.Remove(playerId);
            }
        }

        /// <summary>
        /// Clear specific cooldown for a player
        /// </summary>
        public void ClearCooldown(string playerId, MeditationType type)
        {
            if (_cooldowns.ContainsKey(playerId) && _cooldowns[playerId].ContainsKey(type))
            {
                _cooldowns[playerId].Remove(type);
            }
        }

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, Variant>();

            // 保存冷却时间
            var cooldownsData = new Dictionary<string, Variant>();
            foreach (var playerKvp in _cooldowns)
            {
                var playerCooldowns = new Dictionary<string, Variant>();
                foreach (var cdKvp in playerKvp.Value)
                {
                    playerCooldowns[cdKvp.Key.ToString()] = cdKvp.Value.Ticks;
                }
                cooldownsData[playerKvp.Key] = playerCooldowns;
            }
            data["cooldowns"] = cooldownsData;

            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            // 加载冷却时间
            if (data.TryGetValue("cooldowns", out var cooldownsData))
            {
                var cdDict = (Dictionary<string, Variant>)cooldownsData;
                foreach (var playerKvp in cdDict)
                {
                    var playerId = playerKvp.Key;
                    var playerCooldowns = new Dictionary<MeditationType, DateTime>();

                    var pcDict = (Dictionary<string, Variant>)playerKvp.Value;
                    foreach (var cd in pcDict)
                    {
                        if (Enum.TryParse<MeditationType>(cd.Key, out var meditationType))
                            playerCooldowns[meditationType] = new DateTime((long)cd.Value);
                    }

                    _cooldowns[playerId] = playerCooldowns;
                }
            }
        }
    }
}
