using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Combat;
using ClawRPG.Scripts.Managers;
using ClawRPG.Scripts.Systems.ProceduralDungeon;

namespace ClawRPG.Scripts.Systems.CombatPreload
{
    /// <summary>
    /// 战斗前Combo预览系统
    /// 在战斗开始前展示可用的Combo序列，让玩家确认后再进入战斗
    /// 解决"放了之后才发现不好玩"的问题
    /// </summary>
    public partial class CombatPreloadComboSystem : BaseSystem
    {
        private static CombatPreloadComboSystem _instance;
        public static CombatPreloadComboSystem Instance => _instance;

        // 当前预览状态
        private CombatPreloadState _state = CombatPreloadState.Hidden;
        
        // 可用的Combo列表
        private List<CombatPreloadComboEntry> _availableCombos = new List<CombatPreloadComboEntry>();
        
        // 玩家当前Combo等级
        private int _playerComboLevel = 1;
        
        // Combo系统引用
        private ComboSystem _comboSystem;
        private SkillComboSystem _skillComboSystem;
        
        // 场景引用
        private Control _previewPanel;
        private Control _previewUI;
        
        // 管理器引用 (REQ-117-05: EventBus集成 - 战斗开始流程触发预览)
        private ProceduralDungeonSystem _dungeonSystem;
        private EnemyLifecycleManager _enemyLifecycleManager;
        
        // REQ-121: Combo Buyback
        private int _comboPoint = 1;
        private float _countdownTimer = 0f;
        private const float COUNTDOWN_SECONDS = 3f;
        private string _pendingComboId = null;

        // Signals
        public static Action<CombatPreloadState> OnPreloadStateChanged;
        public static Action<List<CombatPreloadComboEntry>> OnCombosUpdated;
        public static Action<string> OnComboConfirmed; // confirmed comboId
        public static Action OnCombatEntered; // player confirmed and wants to start combat
        public static Action<int> OnComboPointChanged; // REQ-121: combo point changed
        public static Action<float> OnCountdownTick; // REQ-121: countdown seconds remaining

        public override void _Ready()
        {
            _instance = this;
            Initialize();
        }

        protected override void Initialize()
        {
            // 获取Combo系统引用
            _comboSystem = GetNodeOrNull<ComboSystem>("/root/Game/ComboSystem");
            _skillComboSystem = GetNodeOrNull<SkillComboSystem>("/root/Game/SkillComboSystem");
            
            // 获取管理器引用 (REQ-117-05)
            _dungeonSystem = GetNodeOrNull<ProceduralDungeonSystem>("/root/Game/ProceduralDungeonSystem");
            _enemyLifecycleManager = GetNodeOrNull<EnemyLifecycleManager>("/root/Game/EnemyLifecycleManager");
            
            // 如果没有找到，尝试从EventBusManager订阅
            var eventBus = EventBusManager.Instance;
            if (eventBus != null)
            {
                eventBus.Subscribe("combat_preload_requested", OnCombatPreloadRequested);
                eventBus.Subscribe("combat_started", OnCombatStarted);
            }
            
            // 订阅 OnCombatEntered 以在玩家确认后触发战斗开始 (REQ-117-05)
            OnCombatEntered += _OnPlayerConfirmedCombat;
            
            GD.Print("[CombatPreloadComboSystem] Initialized");
        }

        // REQ-121: Countdown timer
        public override void _Process(double delta)
        {
            if (_state != CombatPreloadState.CountingDown)
                return;
            
            _countdownTimer -= (float)delta;
            if (_countdownTimer <= 0f)
            {
                _countdownTimer = 0f;
                // Countdown expired — lock in the pending combo
                _LockInCombo();
            }
            else
            {
                OnCountdownTick?.Invoke(_countdownTimer);
            }
        }

        /// <summary>
        /// REQ-121: Lock in the pending combo after countdown expires
        /// </summary>
        private void _LockInCombo()
        {
            if (string.IsNullOrEmpty(_pendingComboId))
            {
                GD.PrintWrn("[CombatPreloadComboSystem] Countdown expired but no pending combo");
                _state = CombatPreloadState.Confirmed;
                OnPreloadStateChanged?.Invoke(_state);
                OnCombatEntered?.Invoke();
                return;
            }
            
            _state = CombatPreloadState.Confirmed;
            OnPreloadStateChanged?.Invoke(_state);
            GD.Print($"[CombatPreloadComboSystem] Combo locked after countdown: {_pendingComboId}");
        }

        /// <summary>
        /// 请求显示战斗前预览
        /// </summary>
        public void RequestPreload()
        {
            if (_state != CombatPreloadState.Hidden)
            {
                GD.PrintWrn("[CombatPreloadComboSystem] Preload already in progress");
                return;
            }
            
            _LoadAvailableCombos();
            _comboPoint = 1; // REQ-121: reset combo point per battle
            _countdownTimer = 0f;
            _pendingComboId = null;
            OnComboPointChanged?.Invoke(_comboPoint);
            _state = CombatPreloadState.Showing;
            OnPreloadStateChanged?.Invoke(_state);
            OnCombosUpdated?.Invoke(_availableCombos);
            
            GD.Print($"[CombatPreloadComboSystem] Showing preload with {_availableCombos.Count} combos");
        }

        /// <summary>
        /// 加载当前可用的Combo列表
        /// </summary>
        private void _LoadAvailableCombos()
        {
            _availableCombos.Clear();
            
            // 获取 ComboFatigueSystem 实例（REQ-128）
            var fatigueSystem = ComboFatigueSystem.Instance;
            
            // 从ComboSystem获取已解锁的Combos
            if (_comboSystem != null)
            {
                _playerComboLevel = _comboSystem.GetComboLevel();
                var unlockedCombos = _comboSystem.GetUnlockedCombos();
                
                foreach (var combo in unlockedCombos)
                {
                    // REQ-128: 查询每个 Combo 的疲劳状态
                    var (adaptation, multiplier, status) = fatigueSystem.GetFatigueInfo(combo.comboId);
                    
                    var entry = new CombatPreloadComboEntry
                    {
                        ComboId = combo.comboId,
                        ComboName = combo.comboName,
                        Description = combo.description,
                        SkillSequence = new List<string>(combo.skillSequence),
                        DamageMultiplier = combo.damageMultiplier,
                        ComboPointReward = combo.comboPointReward,
                        EffectName = combo.effectName,
                        ComboType = _ConvertComboType(combo.comboType),
                        Rarity = _ConvertRarity(combo.comboRarity),
                        RequiredComboLevel = combo.requiredComboLevel,
                        IsUnlocked = true,
                        CurrentProgress = 0,
                        // REQ-128: 疲劳字段
                        FatigueLevel = adaptation,
                        EffectiveDamageMultiplier = combo.damageMultiplier * multiplier,
                        FatigueStatus = status,
                        FatigueColor = _GetFatigueColor(adaptation)
                    };
                    _availableCombos.Add(entry);
                }
            }
            
            // 从SkillComboSystem补充额外Combos
            if (_skillComboSystem != null)
            {
                var stats = _skillComboSystem.GetStatistics();
                // Skill combos 可以通过 SkillComboDatabase 获取
                // 这里添加从数据库加载的逻辑
            }
            
            // 按类型和稀有度排序
            _availableCombos.Sort((a, b) => {
                int typeCompare = ((int)a.ComboType).CompareTo((int)b.ComboType);
                if (typeCompare != 0) return typeCompare;
                return ((int)b.Rarity).CompareTo((int)a.Rarity);
            });
        }

        private CombatPreloadComboType _ConvertComboType(ComboData.ComboType type)
        {
            return type switch
            {
                ComboData.ComboType.Offensive => CombatPreloadComboType.Offensive,
                ComboData.ComboType.Defensive => CombatPreloadComboType.Defensive,
                ComboData.ComboType.Support => CombatPreloadComboType.Support,
                ComboData.ComboType.Utility => CombatPreloadComboType.Utility,
                ComboData.ComboType.Special => CombatPreloadComboType.Special,
                _ => CombatPreloadComboType.Offensive
            };
        }

        private CombatPreloadComboRarity _ConvertRarity(ComboData.Rarity rarity)
        {
            return rarity switch
            {
                ComboData.Rarity.Common => CombatPreloadComboRarity.Common,
                ComboData.Rarity.Uncommon => CombatPreloadComboRarity.Uncommon,
                ComboData.Rarity.Rare => CombatPreloadComboRarity.Rare,
                ComboData.Rarity.Epic => CombatPreloadComboRarity.Epic,
                ComboData.Rarity.Legendary => CombatPreloadComboRarity.Legendary,
                _ => CombatPreloadComboRarity.Common
            };
        }
        
        // REQ-128: 根据疲劳等级返回对应颜色
        private Color _GetFatigueColor(float adaptation)
        {
            if (adaptation <= 0f) return new Color(0.3f, 1f, 0.3f);       // Fresh → 绿色
            if (adaptation < 0.2f) return new Color(0.7f, 1f, 0.4f);    // Slightly Familiar → 浅绿
            if (adaptation < 0.35f) return new Color(1f, 0.9f, 0.2f); // Adapted → 黄色
            if (adaptation < 0.45f) return new Color(1f, 0.6f, 0.2f);  // Highly Adapted → 橙色
            return new Color(1f, 0.3f, 0.3f);                           // Fully Adapted → 红色
        }

        /// <summary>
        /// 确认选择某个Combo作为本场战斗的计划，开始3秒倒计时（REQ-121）
        /// </summary>
        public void ConfirmCombo(string comboId)
        {
            if (_state != CombatPreloadState.Showing && _state != CombatPreloadState.CountingDown)
            {
                GD.PrintWrn($"[CombatPreloadComboSystem] Cannot confirm when state is {_state}");
                return;
            }
            
            _pendingComboId = comboId;
            _countdownTimer = COUNTDOWN_SECONDS;
            _state = CombatPreloadState.CountingDown;
            OnPreloadStateChanged?.Invoke(_state);
            OnComboConfirmed?.Invoke(comboId);
            OnCountdownTick?.Invoke(_countdownTimer);
            
            GD.Print($"[CombatPreloadComboSystem] Combo countdown started: {comboId} ({COUNTDOWN_SECONDS}s)");
        }

        /// <summary>
        /// REQ-121: 倒计时期间更换Combo，消耗1个Combo Point
        /// </summary>
        public void BuybackCombo(string newComboId)
        {
            if (_state != CombatPreloadState.CountingDown)
            {
                GD.PrintWrn($"[CombatPreloadComboSystem] Buyback only allowed during countdown, current state: {_state}");
                return;
            }
            
            if (_comboPoint <= 0)
            {
                GD.PrintWrn("[CombatPreloadComboSystem] No Combo Point remaining for buyback");
                return;
            }
            
            _comboPoint--;
            _pendingComboId = newComboId;
            _countdownTimer = COUNTDOWN_SECONDS; // Reset countdown
            OnComboPointChanged?.Invoke(_comboPoint);
            OnComboConfirmed?.Invoke(newComboId);
            OnCountdownTick?.Invoke(_countdownTimer);
            
            GD.Print($"[CombatPreloadComboSystem] Combo buyback: {newComboId}, remaining Combo Point: {_comboPoint}");
        }

        /// <summary>
        /// REQ-121: 获取当前剩余Combo Point
        /// </summary>
        public int GetComboPoint() => _comboPoint;

        /// <summary>
        /// REQ-121: 获取当前pending的comboId
        /// </summary>
        public string GetPendingComboId() => _pendingComboId;

        /// <summary>
        /// 确认并进入战斗（使用默认/无特定Combo）
        /// </summary>
        public void ConfirmAndEnterCombat()
        {
            if (_state != CombatPreloadState.Showing && _state != CombatPreloadState.CountingDown)
            {
                GD.PrintWrn("[CombatPreloadComboSystem] Cannot enter combat when not showing or counting down");
                return;
            }
            
            _state = CombatPreloadState.Confirmed;
            OnPreloadStateChanged?.Invoke(_state);
            OnCombatEntered?.Invoke();
            
            GD.Print("[CombatPreloadComboSystem] Combat entered without specific combo");
        }

        /// <summary>
        /// 取消预览
        /// </summary>
        public void Cancel()
        {
            if (_state == CombatPreloadState.Hidden)
                return;
            
            _state = CombatPreloadState.Cancelled;
            OnPreloadStateChanged?.Invoke(_state);
            
            GD.Print("[CombatPreloadComboSystem] Preload cancelled");
            
            // 重置状态
            _state = CombatPreloadState.Hidden;
        }

        /// <summary>
        /// 获取当前可用的Combo列表
        /// </summary>
        public List<CombatPreloadComboEntry> GetAvailableCombos() => _availableCombos;

        /// <summary>
        /// 获取当前预览状态
        /// </summary>
        public CombatPreloadState GetState() => _state;

        /// <summary>
        /// 获取玩家Combo等级
        /// </summary>
        public int GetPlayerComboLevel() => _playerComboLevel;

        // Event handlers
        private void OnCombatPreloadRequested(object[] args)
        {
            RequestPreload();
        }

        private void OnCombatStarted(object[] args)
        {
            // 战斗开始时关闭预览
            if (_state == CombatPreloadState.Showing || _state == CombatPreloadState.CountingDown)
            {
                _state = CombatPreloadState.Hidden;
                _countdownTimer = 0f;
            }
        }
        
        /// <summary>
        /// REQ-117-05: 玩家确认战斗后触发 - 实际生成敌人并开始战斗
        /// </summary>
        private void _OnPlayerConfirmedCombat()
        {
            GD.Print("[CombatPreloadComboSystem] Player confirmed combat - spawning enemies");
            
            // 从地下城系统获取当前房间信息
            if (_dungeonSystem?.CurrentDungeon?.CurrentRoom != null)
            {
                var room = _dungeonSystem.CurrentDungeon.CurrentRoom;
                
                // 生成房间内的敌人
                if (room.Enemies != null && room.Enemies.Count > 0)
                {
                    foreach (var enemyType in room.Enemies)
                    {
                        _SpawnEnemyForRoom(enemyType);
                    }
                }
                else
                {
                    // 如果房间没有预设敌人，随机生成
                    _SpawnDefaultEnemiesForRoom(room.Type);
                }
                
                GD.Print($"[CombatPreloadComboSystem] Spawned enemies for room: {room.Type}");
            }
            else
            {
                // Fallback: 如果没有地下城系统，随机生成敌人
                _SpawnDefaultEnemiesForRoom(RoomType.Combat);
            }
            
            // 重置预览状态
            _state = CombatPreloadState.Hidden;
        }
        
        /// <summary>
        /// 为房间生成指定类型的敌人
        /// </summary>
        private void _SpawnEnemyForRoom(string enemyType)
        {
            if (_enemyLifecycleManager != null)
            {
                _enemyLifecycleManager.SpawnEnemy(null, enemyType);
            }
        }
        
        /// <summary>
        /// 根据房间类型生成默认敌人
        /// </summary>
        private void _SpawnDefaultEnemiesForRoom(RoomType roomType)
        {
            if (_enemyLifecycleManager == null) return;
            
            int enemyCount = roomType switch
            {
                RoomType.Combat => 3,
                RoomType.Elite => 1,
                RoomType.Boss => 1,
                _ => 0
            };
            
            for (int i = 0; i < enemyCount; i++)
            {
                _enemyLifecycleManager.SpawnEnemy(null, "default");
            }
        }

        /// <summary>
        /// Export save data (preview choices are not persisted)
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary<string, object>();
        }

        /// <summary>
        /// Import save data
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            // No persistent data needed
        }
    }
}
