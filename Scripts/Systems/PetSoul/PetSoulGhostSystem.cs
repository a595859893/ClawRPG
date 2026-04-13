using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.PetSoul
{
    /// <summary>
    /// 宠物灵魂游魂系统（REQ-195）
    /// 宠物战死后变成灵魂，在 Safe House 游荡，累积一定死亡次数后升华为守护灵
    /// </summary>
    public partial class PetSoulGhostSystem : BaseSystem
    {
        private static PetSoulGhostSystem _instance;
        public static PetSoulGhostSystem Instance => _instance;

        private PetSoulGhostDatabase _database;

        // 升华守护灵被动效果配置
        private const float GUARDIAN_COOLDOWN_BONUS = 0.03f;  // +3% 宠物技能冷却速度

        // 游荡配置
        private const float WANDER_INTERVAL_MIN = 10f;  // 最小游荡间隔（秒）
        private const float WANDER_INTERVAL_MAX = 30f;  // 最大游荡间隔（秒）
        private const float MONOLOGUE_COOLDOWN = 15f;   // 独白冷却时间（秒）
        private const float INTERACTION_COOLDOWN = 5f;   // 互动冷却时间（秒）
        private const float SAFEHOUSE_RANGE = 300f;    // 判定"靠近玩家"的距离

        // Signals
        public delegate void SoulAddedEventHandler(PetSoulGhostEntry soul);
        public delegate void SoulStateChangedEventHandler(int petId, SoulState oldState, SoulState newState);
        public delegate void SoulTranscendedEventHandler(int petId);
        public delegate void SoulMonologueEventHandler(int petId, string text);
        public delegate void GuardianSpiritBonusChangedEventHandler(float bonusPercent);
        public delegate void SoulInteractedEventHandler(int petId);

        public event SoulAddedEventHandler OnSoulAdded;
        public event SoulStateChangedEventHandler OnSoulStateChanged;
        public event SoulTranscendedEventHandler OnSoulTranscended;
        public event SoulMonologueEventHandler OnSoulMonologue;
        public event GuardianSpiritBonusChangedEventHandler OnGuardianSpiritBonusChanged;
        public event SoulInteractedEventHandler OnSoulInteracted;

        /// <summary>守护灵冷却加速加成（跨局次持久化）</summary>
        private float _guardianCooldownBonus = 0f;

        /// <summary>当前活跃的游荡计时器</summary>
        private float _nextWanderTime = 0f;

        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            _database = new PetSoulGhostDatabase();
            SubscribeToSignals();
            ScheduleNextWander();
        }

        /// <summary>
        /// 订阅相关信号
        /// </summary>
        private void SubscribeToSignals()
        {
            // 订阅宠物死亡信号（来自 PetCombatCompanion）
            var petCompanion = GetNodeOrNull<Godot.Node>("/root/PetCombatCompanion");
            if (petCompanion != null && petCompanion.HasSignal("PetDied"))
            {
                petCompanion.Connect("PetDied", Callable.From<Godot.Collections.Dictionary>(OnPetDied), (uint)ConnectFlags.Deferred);
            }

            // 订阅 Safe House 进入信号（如果存在）
            var safeHouse = GetNodeOrNull<Godot.Node>("/root/SafeHouse");
            if (safeHouse != null)
            {
                if (safeHouse.HasSignal("Entered"))
                    safeHouse.Connect("Entered", Callable.From(OnEnteredSafeHouse), (uint)ConnectFlags.Deferred);
                if (safeHouse.HasSignal("Exited"))
                    safeHouse.Connect("Exited", Callable.From(OnExitedSafeHouse), (uint)ConnectFlags.Deferred);
            }

            // 订阅玩家移动信号用于检测靠近
            var player = GetNodeOrNull<Godot.Node>("/root/Player");
            if (player != null)
            {
                // 玩家位置更新信号（如果存在）
                if (player.HasSignal("PositionChanged"))
                    player.Connect("PositionChanged", Callable.From<Vector2>(OnPlayerPositionChanged), (uint)ConnectFlags.Deferred);
            }
        }

        /// <summary>
        /// 处理宠物死亡事件 — 创建灵魂
        /// </summary>
        private void OnPetDied(Godot.Collections.Dictionary petData)
        {
            int petId = petData.ContainsKey("pet_id") ? Convert.ToInt32(petData["pet_id"]) : 0;
            string petName = petData.ContainsKey("pet_name") ? petData["pet_name"].ToString() : "Unknown";
            string petType = petData.ContainsKey("pet_type") ? petData["pet_type"].ToString() : "Default";
            string petColor = petData.ContainsKey("pet_color") ? petData["pet_color"].ToString() : "#FFFFFF";
            int friendshipLevel = petData.ContainsKey("friendship") ? Convert.ToInt32(petData["friendship"]) : 0;

            if (petId == 0)
                return;

            // 添加或更新灵魂
            _database.AddOrUpdateGhost(petId, petName, petType, petColor, friendshipLevel);

            var ghost = _database.GetGhost(petId);
            if (ghost != null)
            {
                OnSoulAdded?.Invoke(ghost);
                GD.Print($"[PetSoul] Soul manifested for {petName} (death #{ghost.DeathCount})");

                // 检查升华条件
                CheckAndTranscend(petId);

                // 如果已经有升华的灵魂，更新加成
                UpdateGuardianBonus();
            }
        }

        /// <summary>
        /// 检查并升华灵魂
        /// </summary>
        private void CheckAndTranscend(int petId)
        {
            var ghost = _database.GetGhost(petId);
            if (ghost == null || !ghost.CanTranscend)
                return;

            _database.TranscendGhost(petId);
            var updated = _database.GetGhost(petId);
            OnSoulTranscended?.Invoke(petId);
            GD.Print($"[PetSoul] {updated.PetName} has transcended! Guardian spirit activated. (Deaths: {updated.DeathCount}, Threshold: {updated.TranscendenceThreshold})");
            UpdateGuardianBonus();
        }

        /// <summary>
        /// 更新守护灵冷却加成并通知相关系统
        /// </summary>
        private void UpdateGuardianBonus()
        {
            var transcended = _database.GetTranscendedGhosts();
            int count = transcended.Count;
            _guardianCooldownBonus = count * GUARDIAN_COOLDOWN_BONUS;

            OnGuardianSpiritBonusChanged?.Invoke(_guardianCooldownBonus);

            // 通知宠物技能系统应用冷却加成
            var petSkillSystem = GetNodeOrNull<Godot.Node>("/root/PetSkillSystem");
            if (petSkillSystem != null)
            {
                petSkillSystem.Set("GuardianCooldownBonus", _guardianCooldownBonus);
            }
        }

        /// <summary>
        /// Safe House 进入回调 — 开始游荡逻辑
        /// </summary>
        private void OnEnteredSafeHouse()
        {
            // 重置所有非升华灵魂的状态为游荡
            foreach (var ghost in _database.Ghosts.Values)
            {
                if (!ghost.IsTranscended && ghost.State == SoulState.Transcended)
                {
                    // shouldn't happen, but guard
                }
                else if (!ghost.IsTranscended)
                {
                    var oldState = ghost.State;
                    ghost.State = SoulState.Wandering;
                    if (oldState != SoulState.Wandering)
                        OnSoulStateChanged?.Invoke(ghost.PetId, oldState, SoulState.Wandering);
                }
            }
            ScheduleNextWander();
        }

        /// <summary>
        /// Safe House 离开回调 — 停止游荡
        /// </summary>
        private void OnExitedSafeHouse()
        {
            _nextWanderTime = 0f;
        }

        /// <summary>
        /// 玩家位置变化 — 检测是否有灵魂靠近
        /// </summary>
        private void OnPlayerPositionChanged(Vector2 playerPos)
        {
            float currentTime = Time.GetUnixTimeFromSystem();

            foreach (var ghost in _database.GetWanderingGhosts())
            {
                float dist = playerPos.DistanceTo(ghost.WanderPosition);
                bool wasNear = ghost.State == SoulState.NearPlayer || ghost.State == SoulState.Interacting;
                bool isNear = dist <= SAFEHOUSE_RANGE;

                if (isNear && !wasNear)
                {
                    // 进入靠近状态
                    var oldState = ghost.State;
                    ghost.State = SoulState.NearPlayer;
                    OnSoulStateChanged?.Invoke(ghost.PetId, oldState, SoulState.NearPlayer);
                    TryShowMonologue(ghost, currentTime);
                }
                else if (!isNear && wasNear)
                {
                    // 离开靠近状态
                    var oldState = ghost.State;
                    ghost.State = SoulState.Wandering;
                    OnSoulStateChanged?.Invoke(ghost.PetId, oldState, SoulState.Wandering);
                }
            }
        }

        /// <summary>
        /// 尝试显示独白
        /// </summary>
        private void TryShowMonologue(PetSoulGhostEntry ghost, float currentTime)
        {
            if (currentTime - ghost.LastMonologueTime < MONOLOGUE_COOLDOWN)
                return;

            string monologue = SoulMonologueLibrary.GetRandomMonologue(ghost.PetType);
            ghost.LastMonologueTime = currentTime;
            OnSoulMonologue?.Invoke(ghost.PetId, monologue);
        }

        /// <summary>
        /// 触发灵魂互动（玩家按键）
        /// </summary>
        public void TriggerInteraction(int petId)
        {
            var ghost = _database.GetGhost(petId);
            if (ghost == null || ghost.IsTranscended)
                return;

            float currentTime = Time.GetUnixTimeFromSystem();
            if (currentTime - ghost.LastInteractionTime < INTERACTION_COOLDOWN)
                return;

            ghost.LastInteractionTime = currentTime;
            var oldState = ghost.State;
            ghost.State = SoulState.Interacting;
            OnSoulStateChanged?.Invoke(petId, oldState, SoulState.Interacting);
            OnSoulInteracted?.Invoke(petId);

            // 2秒后恢复游荡状态
            var timer = new Godot.Timer();
            timer.WaitTime = 2f;
            timer.OneShot = true;
            timer.Timeout += () => RestoreToWandering(petId);
            AddChild(timer);
            timer.Start();
        }

        private void RestoreToWandering(int petId)
        {
            var ghost = _database.GetGhost(petId);
            if (ghost == null || ghost.IsTranscended)
                return;
            ghost.State = SoulState.Wandering;
            OnSoulStateChanged?.Invoke(petId, SoulState.Interacting, SoulState.Wandering);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            // 检查是否在 Safe House（通过是否存在升华的非游荡状态灵魂判断）
            bool inSafeHouse = false;
            foreach (var ghost in _database.Ghosts.Values)
            {
                if (ghost.State != SoulState.Transcended)
                {
                    inSafeHouse = true;
                    break;
                }
            }
            if (!inSafeHouse)
                return;

            // 处理游荡计时
            float currentTime = Time.GetUnixTimeFromSystem();
            if (_nextWanderTime > 0 && currentTime >= _nextWanderTime)
            {
                PerformWander();
                ScheduleNextWander();
            }
        }

        /// <summary>
        /// 执行一次游荡移动
        /// </summary>
        private void PerformWander()
        {
            var wandering = _database.GetWanderingGhosts();
            foreach (var ghost in wandering)
            {
                // 随机偏移当前位置
                float dx = (float)(GD.Randf() * 2 - 1) * 150f;
                float dy = (float)(GD.Randf() * 2 - 1) * 100f;
                Vector2 newPos = ghost.WanderPosition + new Vector2(dx, dy);
                // 限制在 Safe House 区域内
                newPos.X = Mathf.Clamp(newPos.X, -400, 400);
                newPos.Y = Mathf.Clamp(newPos.Y, -200, 200);
                _database.UpdateWanderPosition(ghost.PetId, newPos);
                GD.Print($"[PetSoul] {ghost.PetName} wandered to ({newPos.X:F1}, {newPos.Y:F1})");
            }
        }

        /// <summary>
        /// 调度下次游荡时间
        /// </summary>
        private void ScheduleNextWander()
        {
            float interval = (float)GD.RandRange(WANDER_INTERVAL_MIN, WANDER_INTERVAL_MAX);
            _nextWanderTime = Time.GetUnixTimeFromSystem() + interval;
        }

        // ========== Public API ==========

        /// <summary>
        /// 获取所有宠物灵魂
        /// </summary>
        public List<PetSoulGhostEntry> GetAllGhosts() => new List<PetSoulGhostEntry>(_database.Ghosts.Values);

        /// <summary>
        /// 获取游荡中的灵魂
        /// </summary>
        public List<PetSoulGhostEntry> GetWanderingGhosts() => _database.GetWanderingGhosts();

        /// <summary>
        /// 获取升华的守护灵
        /// </summary>
        public List<PetSoulGhostEntry> GetTranscendedGhosts() => _database.GetTranscendedGhosts();

        /// <summary>
        /// 获取守护灵冷却加成
        /// </summary>
        public float GetGuardianCooldownBonus() => _guardianCooldownBonus;

        /// <summary>
        /// 获取指定宠物灵魂
        /// </summary>
        public PetSoulGhostEntry GetGhost(int petId) => _database.GetGhost(petId);

        // ========== Persistence ==========

        public override Dictionary<string, object> ExportSaveData()
        {
            var ghostsData = new List<Dictionary<string, object>>();
            foreach (var ghost in _database.Ghosts.Values)
            {
                ghostsData.Add(new Dictionary<string, object>
                {
                    { "pet_id", ghost.PetId },
                    { "pet_name", ghost.PetName },
                    { "pet_type", ghost.PetType },
                    { "pet_color", ghost.PetColor },
                    { "death_count", ghost.DeathCount },
                    { "state", (int)ghost.State },
                    { "is_transcended", ghost.IsTranscended },
                    { "transcended_ts", ghost.TranscendedTimestamp },
                    { "wander_x", ghost.WanderPosition.X },
                    { "wander_y", ghost.WanderPosition.Y },
                    { "last_wander", ghost.LastWanderTime },
                    { "last_monologue", ghost.LastMonologueTime },
                    { "last_interaction", ghost.LastInteractionTime },
                    { "friendship", ghost.FriendshipLevel },
                    { "first_death_ts", ghost.FirstDeathTimestamp }
                });
            }

            return new Dictionary<string, object>
            {
                { "ghosts", ghostsData },
                { "transcended_ids", _database.TranscendedPetIds },
                { "guardian_bonus", _guardianCooldownBonus }
            };
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (!data.ContainsKey("ghosts"))
                return;

            _database.Ghosts.Clear();
            _database.TranscendedPetIds.Clear();

            var ghostsList = (List<object>)data["ghosts"];
            foreach (var entry in ghostsList)
            {
                var dict = (Dictionary<string, object>)entry;
                var ghost = new PetSoulGhostEntry
                {
                    PetId = dict.ContainsKey("pet_id") ? Convert.ToInt32(dict["pet_id"]) : 0,
                    PetName = dict.ContainsKey("pet_name") ? dict["pet_name"].ToString() : "",
                    PetType = dict.ContainsKey("pet_type") ? dict["pet_type"].ToString() : "Default",
                    PetColor = dict.ContainsKey("pet_color") ? dict["pet_color"].ToString() : "#FFFFFF",
                    DeathCount = dict.ContainsKey("death_count") ? Convert.ToInt32(dict["death_count"]) : 0,
                    State = dict.ContainsKey("state") ? (SoulState)Convert.ToInt32(dict["state"]) : SoulState.Wandering,
                    IsTranscended = dict.ContainsKey("is_transcended") && Convert.ToBoolean(dict["is_transcended"]),
                    TranscendedTimestamp = dict.ContainsKey("transcended_ts") ? Convert.ToSingle(dict["transcended_ts"]) : 0f,
                    WanderPosition = new Vector2(
                        dict.ContainsKey("wander_x") ? Convert.ToSingle(dict["wander_x"]) : 0f,
                        dict.ContainsKey("wander_y") ? Convert.ToSingle(dict["wander_y"]) : 0f
                    ),
                    LastWanderTime = dict.ContainsKey("last_wander") ? Convert.ToSingle(dict["last_wander"]) : 0f,
                    LastMonologueTime = dict.ContainsKey("last_monologue") ? Convert.ToSingle(dict["last_monologue"]) : 0f,
                    LastInteractionTime = dict.ContainsKey("last_interaction") ? Convert.ToSingle(dict["last_interaction"]) : 0f,
                    FriendshipLevel = dict.ContainsKey("friendship") ? Convert.ToInt32(dict["friendship"]) : 0,
                    FirstDeathTimestamp = dict.ContainsKey("first_death_ts") ? Convert.ToSingle(dict["first_death_ts"]) : 0f
                };
                _database.Ghosts[ghost.PetId] = ghost;
                if (ghost.IsTranscended && !_database.TranscendedPetIds.Contains(ghost.PetId))
                    _database.TranscendedPetIds.Add(ghost.PetId);
            }

            if (data.ContainsKey("guardian_bonus"))
                _guardianCooldownBonus = Convert.ToSingle(data["guardian_bonus"]);

            UpdateGuardianBonus();
        }
    }
}
