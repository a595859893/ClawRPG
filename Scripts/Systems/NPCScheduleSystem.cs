using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// NPC日程系统 - 管理NPC的日常活动和时间表
    /// </summary>
    public class NPCScheduleSystem : BaseSystem
    {
        public static NPCScheduleSystem Instance { get; private set; }

        // 时间系统 (0-24小时)
        private float _gameTime = 8.0f; // 从早上8点开始
        private float _timeScale = 1.0f; // 时间流逝速度
        private bool _isPaused = false; 

        // NPC日程数据
        private Dictionary<string, NPCSchedule> _npcSchedules = new Dictionary<string, NPCSchedule>();

        // 信号
        public delegate void TimeChangedDelegate(float hour);
        public event TimeChangedDelegate OnTimeChanged;

        public delegate void NPCStateChangedDelegate(string npcId, string oldState, string newState);
        public event NPCStateChangedDelegate OnNPCStateChanged;

        public override void _Ready()
        {
            Instance = this;
            InitializeSchedules();
        }

        public override void _Process(float delta)
        {
            if (_isPaused) return;

            float oldTime = _gameTime;
            _gameTime += delta * _timeScale * 0.1f; // 1秒 = 6分钟游戏时间

            if (_gameTime >= 24.0f)
            {
                _gameTime = 0.0f; // 新的一天
            }

            if ((int)oldTime != (int)_gameTime)
            {
                OnTimeChanged?.Invoke(_gameTime);
                UpdateNPCSchedules();
            }
        }

        private void InitializeSchedules()
        {
            // 战士导师 - 训练/休息
            AddNPCSchedule("warrior_mentor", new NPCSchedule {
                States = new List<NPCScheduleState> {
                    new NPCScheduleState { State = "sleeping", StartHour = 22, EndHour = 6 },
                    new NPCScheduleState { State = "training", StartHour = 6, EndHour = 12 },
                    new NPCScheduleState { State = "idle", StartHour = 12, EndHour = 14 },
                    new NPCScheduleState { State = "training", StartHour = 14, EndHour = 18 },
                    new NPCScheduleState { State = "idle", StartHour = 18, EndHour = 22 }
                }
            });

            // 法师导师 - 研究/休息
            AddNPCSchedule("mage_mentor", new NPCSchedule {
                States = new List<NPCScheduleState> {
                    new NPCScheduleState { State = "sleeping", StartHour = 23, EndHour = 7 },
                    new NPCScheduleState { State = "researching", StartHour = 7, EndHour = 12 },
                    new NPCScheduleState { State = "idle", StartHour = 12, EndHour = 13 },
                    new NPCScheduleState { State = "researching", StartHour = 13, EndHour = 18 },
                    new NPCScheduleState { State = "teaching", StartHour = 18, EndHour = 20 },
                    new NPCScheduleState { State = "idle", StartHour = 20, EndHour = 23 }
                }
            });

            // 商店老板 - 营业/休息
            AddNPCSchedule("shop_owner", new NPCSchedule {
                States = new List<NPCScheduleState> {
                    new NPCScheduleState { State = "sleeping", StartHour = 22, EndHour = 7 },
                    new NPCScheduleState { State = "opening_shop", StartHour = 7, EndHour = 8 },
                    new NPCScheduleState { State = "working", StartHour = 8, EndHour = 12 },
                    new NPCScheduleState { State = "break", StartHour = 12, EndHour = 13 },
                    new NPCScheduleState { State = "working", StartHour = 13, EndHour = 18 },
                    new NPCScheduleState { State = "closing_shop", StartHour = 18, EndHour = 19 },
                    new NPCScheduleState { State = "idle", StartHour = 19, EndHour = 22 }
                }
            });

            // 铁匠 - 锻造/休息
            AddNPCSchedule("blacksmith", new NPCSchedule {
                States = new List<NPCScheduleState> {
                    new NPCScheduleState { State = "sleeping", StartHour = 21, EndHour = 6 },
                    new NPCScheduleState { State = "forging", StartHour = 6, EndHour = 12 },
                    new NPCScheduleState { State = "idle", StartHour = 12, EndHour = 13 },
                    new NPCScheduleState { State = "forging", StartHour = 13, EndHour = 18 },
                    new NPCScheduleState { State = "idle", StartHour = 18, EndHour = 21 }
                }
            });

            // 酒馆老板 - 营业/休息
            AddNPCSchedule("tavern_owner", new NPCSchedule {
                States = new List<NPCScheduleState> {
                    new NPCScheduleState { State = "sleeping", StartHour = 1, EndHour = 10 },
                    new NPCScheduleState { State = "preparing", StartHour = 10, EndHour = 11 },
                    new NPCScheduleState { State = "working", StartHour = 11, EndHour = 14 },
                    new NPCScheduleState { State = "break", StartHour = 14, EndHour = 16 },
                    new NPCScheduleState { State = "working", StartHour = 16, EndHour = 1 }
                }
            });

            // 城镇守卫 - 巡逻/休息
            AddNPCSchedule("town_guard", new NPCSchedule {
                States = new List<NPCScheduleState> {
                    new NPCScheduleState { State = "sleeping", StartHour = 22, EndHour = 6 },
                    new NPCScheduleState { State = "patrolling", StartHour = 6, EndHour = 10 },
                    new NPCScheduleState { State = "guarding", StartHour = 10, EndHour = 14 },
                    new NPCScheduleState { State = "patrolling", StartHour = 14, EndHour = 18 },
                    new NPCScheduleState { State = "guarding", StartHour = 18, EndHour = 22 }
                }
            });
        }

        public void AddNPCSchedule(string npcId, NPCSchedule schedule)
        {
            if (!_npcSchedules.ContainsKey(npcId))
            {
                _npcSchedules[npcId] = schedule;
                schedule.CurrentState = GetStateForTime(npcId, _gameTime);
            }
        }

        public void RemoveNPCSchedule(string npcId)
        {
            _npcSchedules.Remove(npcId);
        }

        public string GetNPCState(string npcId)
        {
            if (_npcSchedules.TryGetValue(npcId, out var schedule))
            {
                return schedule.CurrentState;
            }
            return "unknown";
        }

        public float GetGameTime()
        {
            return _gameTime;
        }

        public string GetTimeOfDay()
        {
            if (_gameTime >= 6 && _gameTime < 12) return "morning";
            if (_gameTime >= 12 && _gameTime < 14) return "noon";
            if (_gameTime >= 14 && _gameTime < 18) return "afternoon";
            if (_gameTime >= 18 && _gameTime < 22) return "evening";
            return "night";
        }

        public void SetTimeScale(float scale)
        {
            _timeScale = Mathf.Max(0, scale);
        }

        public void SetPaused(bool paused)
        {
            _isPaused = paused;
        }

        public void SetGameTime(float hour)
        {
            _gameTime = Mathf.Clamp(hour, 0, 24);
            OnTimeChanged?.Invoke(_gameTime);
            UpdateNPCSchedules();
        }

        private void UpdateNPCSchedules()
        {
            foreach (var kvp in _npcSchedules)
            {
                string npcId = kvp.Key;
                NPCSchedule schedule = kvp.Value;
                string newState = GetStateForTime(npcId, _gameTime);

                if (schedule.CurrentState != newState)
                {
                    string oldState = schedule.CurrentState;
                    schedule.CurrentState = newState;
                    OnNPCStateChanged?.Invoke(npcId, oldState, newState);
                    GD.Print($"[NPCSchedule] {npcId}: {oldState} -> {newState}");
                }
            }
        }

        private string GetStateForTime(string npcId, float time)
        {
            if (_npcSchedules.TryGetValue(npcId, out var schedule))
            {
                foreach (var state in schedule.States)
                {
                    if (IsTimeInRange(time, state.StartHour, state.EndHour))
                    {
                        return state.State;
                    }
                }
            }
            return "idle";
        }

        private bool IsTimeInRange(float time, float startHour, float endHour)
        {
            if (startHour <= endHour)
            {
                return time >= startHour && time < endHour;
            }
            else // 跨午夜 (e.g., 22:00 - 06:00)
            {
                return time >= startHour || time < endHour;
            }
        }

        public Dictionary<string, string> GetAllNPCStates()
        {
            var states = new Dictionary<string, string>();
            foreach (var kvp in _npcSchedules)
            {
                states[kvp.Key] = kvp.Value.CurrentState;
            }
            return states;
        }

        // 存档支持
        public Dictionary<string, object> Save()
        {
            var data = new Dictionary<string, object> {
                { "game_time", _gameTime },
                { "time_scale", _timeScale },
                { "is_paused", _isPaused }
            };
            return data;
        }

        public void Load(Dictionary<string, object> data)
        {
            if (data.ContainsKey("game_time"))
                _gameTime = Convert.ToSingle(data["game_time"]);
            if (data.ContainsKey("time_scale"))
                _timeScale = Convert.ToSingle(data["time_scale"]);
            if (data.ContainsKey("is_paused"))
                _isPaused = Convert.ToBoolean(data["is_paused"]);
        }
        
        /// <summary>
        /// Export save data for persistence
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            return new Dictionary
            {
                { "game_time", _gameTime },
                { "time_scale", _timeScale },
                { "is_paused", _isPaused }
            };
        }
        
        /// <summary>
        /// Import save data from persistence
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            if (data.Contains("game_time")) _gameTime = (float)data["game_time"];
            if (data.Contains("time_scale")) _timeScale = (float)data["time_scale"];
            if (data.Contains("is_paused")) _isPaused = (bool)data["is_paused"];
        }
    }

    public class NPCSchedule
    {
        public List<NPCScheduleState> States { get; set; } = new List<NPCScheduleState>();
        public string CurrentState { get; set; } = "idle";
    }

    public class NPCScheduleState
    {
        public string State { get; set; } = "idle";
        public float StartHour { get; set; }
        public float EndHour { get; set; }
    }
}
