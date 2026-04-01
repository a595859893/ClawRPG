using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Managers
{
    /// <summary>
    /// 游戏状态管理器 - 负责管理游戏状态（标题、游戏中、暂停、游戏结束）
    /// </summary>
    public partial class GameStateManager : BaseSystem
    {
        public static GameStateManager Instance { get; private set; }
        
        /// <summary>
        /// 优先级（数值越小越先初始化）
        /// </summary>
        public int Priority => 10;
        
        // Game states
        public enum GameState
        {
            TitleScreen,
            Playing,
            Paused,
            GameOver
        }
        
        // Current state
        private GameState _currentState = GameState.Playing;
        private GameState _previousState = GameState.Playing;
        
        // State properties
        private bool _isPaused = false;
        private int _currentDay = 1;
        private float _dayTimer = 0f;
        private float _dayLength = 600f; // 10 minutes per day
        
        // Game time tracking
        private float _totalPlayTime = 0f;
        private float _sessionPlayTime = 0f;
        private DateTime _sessionStartTime;
        
        // Pause state
        private bool _shiftEToggleCooldown = false;
        
        // Events
        public event Action<GameState> OnStateChanged;
        public event Action<GameState, GameState> OnStateTransition;
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        public event Action OnGameOver;
        
        // Static pause property for global access
        public static bool IsPaused { get; private set; }
        
        public override void _Ready()
        {
            Instance = this;
            base._Ready();
            _sessionStartTime = DateTime.Now;
        }
        
        protected override void Initialize()
        {
            GD.Print("[GameStateManager] Initialized");
            SetState(GameState.Playing);
        }
        
        /// <summary>
        /// 设置游戏状态
        /// </summary>
        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;
            
            var oldState = _currentState;
            _previousState = oldState;
            _currentState = newState;
            
            // Handle state-specific logic
            HandleStateChange(newState);
            
            GD.Print("[GameStateManager] Game state changed: " + oldState + " -> " + newState);
            OnStateChanged?.Invoke(newState);
            OnStateTransition?.Invoke(oldState, newState);
            
            // 通过事件总线发布全局事件
            if (EventBusManager.Instance != null)
            {
                var stateData = new GameStateEventData(oldState, newState);
                EventBusManager.Instance.Emit(EventBusManager.Events.LevelChanged, stateData);
            }
        }
        
        private void HandleStateChange(GameState newState)
        {
            switch (newState)
            {
                case GameState.Playing:
                    ResumeGame();
                    break;
                    
                case GameState.Paused:
                    PauseGame();
                    break;
                    
                case GameState.GameOver:
                    HandleGameOver();
                    break;
                    
                case GameState.TitleScreen:
                    // Handle title screen transition
                    break;
            }
        }
        
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            if (_isPaused) return;
            
            _isPaused = true;
            IsPaused = true;
            
            GetTree().Paused = true;
            
            GD.Print("[GameStateManager] Game paused");
            OnGamePaused?.Invoke();
            
            // 通过事件总线发布全局事件
            if (EventBusManager.Instance != null)
            {
                var pauseData = new GamePauseEventData(true, _currentState, _totalPlayTime);
                EventBusManager.Instance.Emit(EventBusManager.Events.GamePaused, pauseData);
            }
        }
        
        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            if (!_isPaused) return;
            
            _isPaused = false;
            IsPaused = false;
            
            GetTree().Paused = false;
            
            GD.Print("[GameStateManager] Game resumed");
            OnGameResumed?.Invoke();
            
            // 通过事件总线发布全局事件
            if (EventBusManager.Instance != null)
            {
                var pauseData = new GamePauseEventData(false, _currentState, _totalPlayTime);
                EventBusManager.Instance.Emit(EventBusManager.Events.GameResumed, pauseData);
            }
        }
        
        /// <summary>
        /// 切换暂停状态
        /// </summary>
        public void TogglePause()
        {
            if (_currentState == GameState.GameOver || _currentState == GameState.TitleScreen)
                return;
            
            if (_isPaused)
            {
                SetState(GameState.Playing);
            }
            else
            {
                SetState(GameState.Paused);
            }
        }
        
        /// <summary>
        /// 处理游戏结束
        /// </summary>
        private void HandleGameOver()
        {
            PauseGame();
            GD.Print("[GameStateManager] Game Over!");
            OnGameOver?.Invoke();
            
            // 通过事件总线发布全局事件
            if (EventBusManager.Instance != null)
            {
                var killCount = EnemyLifecycleManager.Instance?.KillCount ?? 0;
                var deathCount = PlayerLifecycleManager.Instance?.DeathCount ?? 0;
                var gameOverData = new GameOverEventData(
                    (int)_totalPlayTime, 
                    killCount, 
                    deathCount, 
                    _currentDay
                );
                EventBusManager.Instance.Emit(EventBusManager.Events.GameOver, gameOverData);
            }
        }
        
        /// <summary>
        /// 开始新游戏
        /// </summary>
        public void StartNewGame()
        {
            _currentDay = 1;
            _dayTimer = 0f;
            _totalPlayTime = 0f;
            _sessionPlayTime = 0f;
            
            SetState(GameState.Playing);
            GD.Print("[GameStateManager] New game started");
        }
        
        /// <summary>
        /// 返回标题画面
        /// </summary>
        public void ReturnToTitle()
        {
            SetState(GameState.TitleScreen);
        }
        
        /// <summary>
        /// 获取当前状态
        /// </summary>
        public GameState GetState()
        {
            return _currentState;
        }
        
        /// <summary>
        /// 检查是否在游戏中
        /// </summary>
        public bool IsPlaying()
        {
            return _currentState == GameState.Playing;
        }
        
        /// <summary>
        /// 检查是否暂停
        /// </summary>
        public bool IsGamePaused()
        {
            return _isPaused;
        }
        
        /// <summary>
        /// 检查是否游戏结束
        /// </summary>
        public bool IsGameOver()
        {
            return _currentState == GameState.GameOver;
        }
        
        /// <summary>
        /// 获取当前天数
        /// </summary>
        public int GetCurrentDay()
        {
            return _currentDay;
        }
        
        /// <summary>
        /// 设置天数
        /// </summary>
        public void SetDay(int day)
        {
            _currentDay = Math.Max(1, day);
        }
        
        /// <summary>
        /// 获取总游戏时间（秒）
        /// </summary>
        public float GetTotalPlayTime()
        {
            return _totalPlayTime;
        }
        
        /// <summary>
        /// 获取本次会话游戏时间（秒）
        /// </summary>
        public float GetSessionPlayTime()
        {
            return _sessionPlayTime;
        }
        
        /// <summary>
        /// 获取会话开始时间
        /// </summary>
        public DateTime GetSessionStartTime()
        {
            return _sessionStartTime;
        }
        
        /// <summary>
        /// 增加游戏时间
        /// </summary>
        public void AddPlayTime(float deltaTime)
        {
            if (_currentState == GameState.Playing && !_isPaused)
            {
                _totalPlayTime += deltaTime;
                _sessionPlayTime += deltaTime;
                _dayTimer += deltaTime;
                
                // Check for day progression
                if (_dayTimer >= _dayLength)
                {
                    _dayTimer = 0f;
                    _currentDay++;
                    GD.Print("[GameStateManager] Day " + _currentDay + " begins!");
                }
            }
        }
        
        /// <summary>
        /// 设置一天的长度
        /// </summary>
        public void SetDayLength(float seconds)
        {
            _dayLength = Math.Max(1f, seconds);
        }
        
        /// <summary>
        /// 获取之前的状态
        /// </summary>
        public GameState GetPreviousState()
        {
            return _previousState;
        }
        
        // Shift+E toggle cooldown
        public bool ShiftEToggleCooldown
        {
            get => _shiftEToggleCooldown;
            set => _shiftEToggleCooldown = value;
        }
        
        // Getters
        public GameState GetCurrentState() => _currentState;
        public bool IsPausedGame() => _isPaused;
        public float GetDayTimer() => _dayTimer;
        public float GetDayLength() => _dayLength;
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary
            {
                { "currentState", (int)_currentState },
                { "currentDay", _currentDay },
                { "dayTimer", _dayTimer },
                { "dayLength", _dayLength },
                { "totalPlayTime", _totalPlayTime }
            };
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.Contains("currentState")) 
                _currentState = (GameState)Convert.ToInt32(data["currentState"]);
            if (data.Contains("currentDay")) 
                _currentDay = Convert.ToInt32(data["currentDay"]);
            if (data.Contains("dayTimer")) 
                _dayTimer = Convert.ToSingle(data["dayTimer"]);
            if (data.Contains("dayLength")) 
                _dayLength = Convert.ToSingle(data["dayLength"]);
            if (data.Contains("totalPlayTime")) 
                _totalPlayTime = Convert.ToSingle(data["totalPlayTime"]);
        }
    }
}
