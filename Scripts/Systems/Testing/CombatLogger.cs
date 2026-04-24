namespace ClawRPG.Scripts.Systems.Testing
{
    using System;
    using System.Collections.Generic;
    using Godot;

    /// <summary>
    /// Records structured combat events for version diff analysis.
    /// Used to detect numerical regressions in combat calculations.
    /// </summary>
    public class CombatLogger
    {
        public string Version { get; set; }
        public string Timestamp { get; set; }
        public string Scene { get; set; }
        public CombatActorSnapshot Player { get; set; }
        public List<CombatActorSnapshot> Enemies { get; set; } = new List<CombatActorSnapshot>();
        public List<CombatEvent> Events { get; set; } = new List<CombatEvent>();
        public CombatOutcome Outcome { get; set; }

        private bool _isRecording;
        private int _currentFrame;

        public CombatLogger()
        {
            Version = "unknown";
            Timestamp = DateTime.UtcNow.ToString("o");
            _isRecording = false;
            _currentFrame = 0;
        }

        /// <summary>
        /// Start a new combat recording session.
        /// </summary>
        public void StartRecording(string scene, CombatActorSnapshot player, List<CombatActorSnapshot> enemies)
        {
            Scene = scene;
            Player = player;
            Enemies = enemies != null ? new List<CombatActorSnapshot>(enemies) : new List<CombatActorSnapshot>();
            Events.Clear();
            _isRecording = true;
            _currentFrame = 0;
            Timestamp = DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// Stop recording and finalize the log.
        /// </summary>
        public void StopRecording()
        {
            _isRecording = false;
        }

        /// <summary>
        /// Advance frame counter.
        /// </summary>
        public void NextFrame()
        {
            _currentFrame++;
        }

        /// <summary>
        /// Log a skill used by an actor.
        /// </summary>
        public void LogSkillUsed(string actorId, string skillName, int damage)
        {
            if (!_isRecording) return;
            Events.Add(new CombatEvent
            {
                Frame = _currentFrame,
                Type = "skill_used",
                Actor = actorId,
                Skill = skillName,
                Damage = damage
            });
        }

        /// <summary>
        /// Log an enemy death event.
        /// </summary>
        public void LogEnemyDied(string actorId)
        {
            if (!_isRecording) return;
            Events.Add(new CombatEvent
            {
                Frame = _currentFrame,
                Type = "enemy_died",
                Actor = actorId
            });
        }

        /// <summary>
        /// Log direct damage dealt.
        /// </summary>
        public void LogDamage(string sourceId, string targetId, int amount, string damageType = "physical")
        {
            if (!_isRecording) return;
            Events.Add(new CombatEvent
            {
                Frame = _currentFrame,
                Type = "damage",
                Actor = sourceId,
                Target = targetId,
                Damage = amount,
                DamageType = damageType
            });
        }

        /// <summary>
        /// Log a status effect applied.
        /// </summary>
        public void LogStatusEffect(string targetId, string effectName, int duration, bool applied)
        {
            if (!_isRecording) return;
            Events.Add(new CombatEvent
            {
                Frame = _currentFrame,
                Type = applied ? "status_applied" : "status_removed",
                Actor = targetId,
                Skill = effectName,
                Duration = duration
            });
        }

        /// <summary>
        /// Log player HP change.
        /// </summary>
        public void LogHpChange(string actorId, int oldHp, int newHp)
        {
            if (!_isRecording) return;
            Events.Add(new CombatEvent
            {
                Frame = _currentFrame,
                Type = "hp_change",
                Actor = actorId,
                Damage = oldHp - newHp,
                Duration = newHp  // reusing Duration field to carry current HP
            });
        }

        /// <summary>
        /// Finalize and set combat outcome.
        /// </summary>
        public void SetOutcome(bool won, int damageTaken, int rounds)
        {
            Outcome = new CombatOutcome
            {
                Won = won,
                DamageTaken = damageTaken,
                Rounds = rounds
            };
            StopRecording();
        }

        /// <summary>
        /// Export the full combat log as a serializable dictionary.
        /// </summary>
        public Dictionary<string, object> Export()
        {
            var dict = new Dictionary<string, object>
            {
                { "version", Version },
                { "timestamp", Timestamp },
                { "scene", Scene ?? "" },
                { "player", Player?.Export() ?? new Dictionary<string, object>() },
                { "enemies", Enemies.ConvertAll(e => e.Export()) },
                { "events", Events.ConvertAll(e => e.Export()) },
                { "outcome", Outcome?.Export() ?? new Dictionary<string, object>() }
            };
            return dict;
        }

        /// <summary>
        /// Import a combat log from a dictionary.
        /// </summary>
        public static CombatLogger Import(Dictionary<string, object> data)
        {
            var logger = new CombatLogger
            {
                Version = data.ContainsKey("version") ? (string)data["version"] : "unknown",
                Timestamp = data.ContainsKey("timestamp") ? (string)data["timestamp"] : "",
                Scene = data.ContainsKey("scene") ? (string)data["scene"] : ""
            };

            if (data.ContainsKey("player") && data["player"] is Dictionary<string, object> playerData)
                logger.Player = CombatActorSnapshot.Import(playerData);

            if (data.ContainsKey("enemies") && data["enemies"] is Godot.Collections.Array enemiesArr)
            {
                foreach (Dictionary<string, object> enemyData in enemiesArr)
                    logger.Enemies.Add(CombatActorSnapshot.Import(enemyData));
            }

            if (data.ContainsKey("events") && data["events"] is Godot.Collections.Array eventsArr)
            {
                foreach (Dictionary<string, object> eventData in eventsArr)
                    logger.Events.Add(CombatEvent.Import(eventData));
            }

            if (data.ContainsKey("outcome") && data["outcome"] is Dictionary<string, object> outcomeData)
                logger.Outcome = CombatOutcome.Import(outcomeData);

            return logger;
        }
    }

    /// <summary>
    /// Snapshot of an actor's state at the start of combat.
    /// </summary>
    public class CombatActorSnapshot
    {
        public string Id { get; set; }
        public int Hp { get; set; }
        public int Attack { get; set; }
        public List<string> Skills { get; set; } = new List<string>();

        public Dictionary<string, object> Export()
        {
            return new Dictionary<string, object>
            {
                { "id", Id ?? "" },
                { "hp", Hp },
                { "attack", Attack },
                { "skills", Skills }
            };
        }

        public static CombatActorSnapshot Import(Dictionary<string, object> data)
        {
            return new CombatActorSnapshot
            {
                Id = data.ContainsKey("id") ? (string)data["id"] : "",
                Hp = data.ContainsKey("hp") ? Convert.ToInt32(data["hp"]) : 0,
                Attack = data.ContainsKey("attack") ? Convert.ToInt32(data["attack"]) : 0,
                Skills = data.ContainsKey("skills") ? new List<string>((Godot.Collections.Array)data["skills"]) : new List<string>()
            };
        }
    }

    /// <summary>
    /// A single combat event frame.
    /// </summary>
    public class CombatEvent
    {
        public int Frame { get; set; }
        public string Type { get; set; }
        public string Actor { get; set; }
        public string Target { get; set; }
        public string Skill { get; set; }
        public int Damage { get; set; }
        public string DamageType { get; set; }
        public int Duration { get; set; }

        public Dictionary<string, object> Export()
        {
            var dict = new Dictionary<string, object>
            {
                { "frame", Frame },
                { "type", Type ?? "" },
                { "actor", Actor ?? "" }
            };
            if (!string.IsNullOrEmpty(Target)) dict["target"] = Target;
            if (!string.IsNullOrEmpty(Skill)) dict["skill"] = Skill;
            if (Damage != 0) dict["damage"] = Damage;
            if (!string.IsNullOrEmpty(DamageType)) dict["damage_type"] = DamageType;
            if (Duration != 0) dict["duration"] = Duration;
            return dict;
        }

        public static CombatEvent Import(Dictionary<string, object> data)
        {
            return new CombatEvent
            {
                Frame = data.ContainsKey("frame") ? Convert.ToInt32(data["frame"]) : 0,
                Type = data.ContainsKey("type") ? (string)data["type"] : "",
                Actor = data.ContainsKey("actor") ? (string)data["actor"] : "",
                Target = data.ContainsKey("target") ? (string)data["target"] : "",
                Skill = data.ContainsKey("skill") ? (string)data["skill"] : "",
                Damage = data.ContainsKey("damage") ? Convert.ToInt32(data["damage"]) : 0,
                DamageType = data.ContainsKey("damage_type") ? (string)data["damage_type"] : "",
                Duration = data.ContainsKey("duration") ? Convert.ToInt32(data["duration"]) : 0
            };
        }
    }

    /// <summary>
    /// Final outcome of a combat encounter.
    /// </summary>
    public class CombatOutcome
    {
        public bool Won { get; set; }
        public int DamageTaken { get; set; }
        public int Rounds { get; set; }

        public Dictionary<string, object> Export()
        {
            return new Dictionary<string, object>
            {
                { "won", Won },
                { "damage_taken", DamageTaken },
                { "rounds", Rounds }
            };
        }

        public static CombatOutcome Import(Dictionary<string, object> data)
        {
            return new CombatOutcome
            {
                Won = data.ContainsKey("won") ? (bool)data["won"] : false,
                DamageTaken = data.ContainsKey("damage_taken") ? Convert.ToInt32(data["damage_taken"]) : 0,
                Rounds = data.ContainsKey("rounds") ? Convert.ToInt32(data["rounds"]) : 0
            };
        }
    }
}
