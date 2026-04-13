using Godot;
using System;
using System.Collections.Generic;
using Framework;

namespace ClawRPG.Systems.Meditation
{
    /// <summary>
    /// Meditation Buff System - Handles buff/增益管理 logic
    /// </summary>
    public partial class MeditationBuffSystem : BaseSystem
    {
        public static MeditationBuffSystem Instance { get; private set; }

        // Active buffs on players
        private Dictionary<string, List<MeditationBuff>> _activeBuffs = new Dictionary<string, List<MeditationBuff>>();

        // Signals
        public MeditationSignals Signals;

        public partial class MeditationSignals : GodotObject
        {
            public delegate void BuffAppliedHandler(string playerId, MeditationType type, string statAffected, float value);
            public delegate void BuffExpiredHandler(string playerId, MeditationType type);

            public event BuffAppliedHandler BuffApplied;
            public event BuffExpiredHandler BuffExpired;

            public void EmitBuffApplied(string playerId, MeditationType type, string statAffected, float value)
            {
                BuffApplied?.Invoke(playerId, type, statAffected, value);
            }

            public void EmitBuffExpired(string playerId, MeditationType type)
            {
                BuffExpired?.Invoke(playerId, type);
            }
        }

        public override void _Ready()
        {
            base._Ready();
            Instance = this;
            Signals = new MeditationSignals();

            // Initialize timer for buff expiration checking
            var timer = new Timer();
            timer.Name = "MeditationBuffTimer";
            timer.WaitTime = 1.0f;
            timer.Autostart = true;
            timer.Timeout += _OnBuffCheckTimer;
            AddChild(timer);
        }

        /// <summary>
        /// Get active buffs for a player
        /// </summary>
        public List<MeditationBuff> GetActiveBuffs(string playerId)
        {
            return _activeBuffs.ContainsKey(playerId) ? _activeBuffs[playerId] : new List<MeditationBuff>();
        }

        /// <summary>
        /// Apply a meditation benefit
        /// </summary>
        public void ApplyBenefit(string playerId, MeditationType type, MeditationBenefit benefit)
        {
            float value = benefit.BaseValue * benefit.EffectMultiplier;

            if (benefit.Duration == -1)
            {
                // Permanent buff - apply directly to player stats
                ApplyPermanentBuff(playerId, benefit.StatAffected, value);
            }
            else
            {
                // Temporary buff
                ApplyTemporaryBuff(playerId, type, benefit.StatAffected, value, benefit.Duration);
            }

            Signals.EmitBuffApplied(playerId, type, benefit.StatAffected, value);
        }

        /// <summary>
        /// Apply permanent stat bonus
        /// </summary>
        private void ApplyPermanentBuff(string playerId, string stat, float value)
        {
            // This would integrate with the player's stat system
            GD.Print($"[Meditation] Applying permanent buff to {playerId}: {stat} + {value}");
        }

        /// <summary>
        /// Apply temporary buff
        /// </summary>
        private void ApplyTemporaryBuff(string playerId, MeditationType type, string stat, float value, int duration)
        {
            if (!_activeBuffs.ContainsKey(playerId))
                _activeBuffs[playerId] = new List<MeditationBuff>();

            var buff = new MeditationBuff
            {
                BuffId = Guid.NewGuid().ToString(),
                Type = type,
                StatAffected = stat,
                Value = value,
                StartTime = DateTime.Now,
                Duration = duration,
                IsPermanent = false
            };

            _activeBuffs[playerId].Add(buff);
        }

        /// <summary>
        /// Apply permanent buff directly
        /// </summary>
        public void ApplyPermanentBuff(string playerId, MeditationBuff buff)
        {
            ApplyPermanentBuff(playerId, buff.StatAffected, buff.Value);
        }

        /// <summary>
        /// Apply temporary buff directly
        /// </summary>
        public void ApplyTemporaryBuff(string playerId, MeditationBuff buff)
        {
            if (!_activeBuffs.ContainsKey(playerId))
                _activeBuffs[playerId] = new List<MeditationBuff>();

            buff.BuffId = Guid.NewGuid().ToString();
            buff.StartTime = DateTime.Now;
            buff.IsPermanent = false;

            _activeBuffs[playerId].Add(buff);
        }

        /// <summary>
        /// Get meditation bonus for stat
        /// </summary>
        public float GetStatBonus(string playerId, string stat)
        {
            float totalBonus = 0f;

            if (!_activeBuffs.ContainsKey(playerId))
                return 0f;

            foreach (var buff in _activeBuffs[playerId])
            {
                if (buff.StatAffected == stat || buff.StatAffected == "AllStats")
                {
                    totalBonus += buff.Value;
                }
            }

            return totalBonus;
        }

        /// <summary>
        /// Check and remove expired buffs
        /// </summary>
        private void _OnBuffCheckTimer()
        {
            var now = DateTime.Now;
            var expiredBuffs = new List<Tuple<string, MeditationBuff>>();

            foreach (var kvp in _activeBuffs)
            {
                var playerId = kvp.Key;
                var buffs = kvp.Value;

                for (int i = buffs.Count - 1; i >= 0; i--)
                {
                    var buff = buffs[i];
                    if (!buff.IsPermanent)
                    {
                        var elapsed = (now - buff.StartTime).TotalSeconds;
                        if (elapsed >= buff.Duration)
                        {
                            expiredBuffs.Add(Tuple.Create(playerId, buff));
                            buffs.RemoveAt(i);
                        }
                    }
                }
            }

            foreach (var expired in expiredBuffs)
            {
                Signals.EmitBuffExpired(expired.Item1, expired.Item2.Type);
            }
        }

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, Variant>();

            // 保存活跃buff（需要恢复过期时间）
            var buffsData = new Dictionary<string, Variant>();
            foreach (var kvp in _activeBuffs)
            {
                var playerBuffs = new List<Dictionary<string, Variant>>();
                foreach (var buff in kvp.Value)
                {
                    playerBuffs.Add(new Dictionary<string, Variant>
                    {
                        { "buffId", buff.BuffId },
                        { "type", (int)buff.Type },
                        { "statAffected", buff.StatAffected },
                        { "value", buff.Value },
                        { "startTime", buff.StartTime.Ticks },
                        { "duration", buff.Duration },
                        { "isPermanent", buff.IsPermanent }
                    });
                }
                buffsData[kvp.Key] = playerBuffs;
            }
            data["buffs"] = buffsData;

            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            // 加载活跃buff
            if (data.TryGetValue("buffs", out var buffsData))
            {
                var buffsDict = (Dictionary<string, Variant>)buffsData;
                foreach (var kvp in buffsDict)
                {
                    var playerId = kvp.Key;
                    var playerBuffs = new List<MeditationBuff>();

                    var buffsList = (List<Variant>)kvp.Value;
                    foreach (var buffVar in buffsList)
                    {
                        var buffDict = (Dictionary<string, Variant>)buffVar;
                        var buff = new MeditationBuff
                        {
                            BuffId = (string)buffDict["buffId"],
                            Type = (MeditationType)(int)buffDict["type"],
                            StatAffected = (string)buffDict["statAffected"],
                            Value = (float)buffDict["value"],
                            StartTime = new DateTime((long)buffDict["startTime"]),
                            Duration = (int)buffDict["duration"],
                            IsPermanent = (bool)buffDict["isPermanent"]
                        };
                        playerBuffs.Add(buff);
                    }

                    _activeBuffs[playerId] = playerBuffs;
                }
            }
        }
    }
}
