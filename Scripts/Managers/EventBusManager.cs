using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 事件总线管理器 - 统一的游戏事件分发系统
/// 减少系统间的直接依赖
/// </summary>
public class EventBusManager : ManagerBase
{
    public static EventBusManager Instance { get; private set; }
    
    /// <summary>
    /// 优先级（数值越小越先初始化）
    /// </summary>
    public override int Priority => 1;
    
    /// <summary>
    /// 事件字典 - 存储所有事件回调
    /// </summary>
    private Dictionary<string, Delegate> _eventHandlers = new Dictionary<string, Delegate>();
    
    /// <summary>
    /// 一次性事件（触发后自动移除）
    /// </summary>
    private Dictionary<string, Delegate> _oneTimeHandlers = new Dictionary<string, Delegate>();
    
    // 预定义事件名称
    public static class Events
    {
        // 玩家事件
        public const string PlayerSpawned = "PlayerSpawned";
        public const string PlayerDied = "PlayerDied";
        public const string PlayerRespawned = "PlayerRespawned";
        public const string PlayerHealthChanged = "PlayerHealthChanged";
        public const string PlayerLevelUp = "PlayerLevelUp";
        
        // 敌人事件
        public const string EnemySpawned = "EnemySpawned";
        public const string EnemyDied = "EnemyDied";
        public const string EnemyDamaged = "EnemyDamaged";
        
        // 战斗事件
        public const string CombatStarted = "CombatStarted";
        public const string CombatEnded = "CombatEnded";
        public const string DamageDealt = "DamageDealt";
        public const string DamageTaken = "DamageTaken";
        
        // 游戏状态事件
        public const string GamePaused = "GamePaused";
        public const string GameResumed = "GameResumed";
        public const string GameOver = "GameOver";
        public const string LevelChanged = "LevelChanged";
        
        // 物品事件
        public const string ItemCollected = "ItemCollected";
        public const string ItemUsed = "ItemUsed";
        public const string ItemEquipped = "ItemEquipped";
        
        // 任务事件
        public const string QuestStarted = "QuestStarted";
        public const string QuestCompleted = "QuestCompleted";
        public const string QuestFailed = "QuestFailed";
        
        // 保存/加载事件
        public const string GameSaved = "GameSaved";
        public const string GameLoaded = "GameLoaded";
        
        // 场景事件
        public const string SceneChanged = "SceneChanged";
        public const string SceneLoading = "SceneLoading";
    }
    
    public override void _Ready()
    {
        Instance = this;
        base._Ready();
    }
    
    protected override void Initialize()
    {
        GD.Print("[EventBusManager] Initialized");
        NotifyInitialized();
    }
    
    #region 订阅事件
    
    /// <summary>
    /// 订阅事件
    /// </summary>
    public void Subscribe<T>(string eventName, Action<T> handler)
    {
        if (handler == null) return;
        
        var key = eventName + "_" + typeof(T).Name;
        
        if (_eventHandlers.ContainsKey(key))
        {
            _eventHandlers[key] = Delegate.Combine(_eventHandlers[key], handler);
        }
        else
        {
            _eventHandlers[key] = handler;
        }
        
        GD.Print($"[EventBusManager] Subscribed to event: {eventName}");
    }
    
    /// <summary>
    /// 订阅无参数事件
    /// </summary>
    public void Subscribe(string eventName, Action handler)
    {
        if (handler == null) return;
        
        if (_eventHandlers.ContainsKey(eventName))
        {
            _eventHandlers[eventName] = Delegate.Combine(_eventHandlers[eventName], handler);
        }
        else
        {
            _eventHandlers[eventName] = handler;
        }
        
        GD.Print($"[EventBusManager] Subscribed to event: {eventName}");
    }
    
    /// <summary>
    /// 订阅一次性事件
    /// </summary>
    public void SubscribeOnce<T>(string eventName, Action<T> handler)
    {
        if (handler == null) return;
        
        var key = eventName + "_" + typeof(T).Name;
        
        if (_oneTimeHandlers.ContainsKey(key))
        {
            _oneTimeHandlers[key] = Delegate.Combine(_oneTimeHandlers[key], handler);
        }
        else
        {
            _oneTimeHandlers[key] = handler;
        }
    }
    
    /// <summary>
    /// 订阅一次性无参数事件
    /// </summary>
    public void SubscribeOnce(string eventName, Action handler)
    {
        if (handler == null) return;
        
        if (_oneTimeHandlers.ContainsKey(eventName))
        {
            _oneTimeHandlers[eventName] = Delegate.Combine(_oneTimeHandlers[eventName], handler);
        }
        else
        {
            _oneTimeHandlers[eventName] = handler;
        }
    }
    
    #endregion
    
    #region 取消订阅
    
    /// <summary>
    /// 取消订阅事件
    /// </summary>
    public void Unsubscribe<T>(string eventName, Action<T> handler)
    {
        if (handler == null) return;
        
        var key = eventName + "_" + typeof(T).Name;
        
        if (_eventHandlers.ContainsKey(key))
        {
            _eventHandlers[key] = Delegate.Remove(_eventHandlers[key], handler);
            if (_eventHandlers[key] == null)
            {
                _eventHandlers.Remove(key);
            }
        }
    }
    
    /// <summary>
    /// 取消订阅无参数事件
    /// </summary>
    public void Unsubscribe(string eventName, Action handler)
    {
        if (handler == null) return;
        
        if (_eventHandlers.ContainsKey(eventName))
        {
            _eventHandlers[eventName] = Delegate.Remove(_eventHandlers[eventName], handler);
            if (_eventHandlers[eventName] == null)
            {
                _eventHandlers.Remove(eventName);
            }
        }
    }
    
    /// <summary>
    /// 取消订阅所有事件
    /// </summary>
    public void UnsubscribeAll(string eventName)
    {
        _eventHandlers.Remove(eventName);
        _oneTimeHandlers.Remove(eventName);
    }
    
    /// <summary>
    /// 清除所有事件订阅
    /// </summary>
    public void ClearAll()
    {
        _eventHandlers.Clear();
        _oneTimeHandlers.Clear();
        GD.Print("[EventBusManager] All event handlers cleared");
    }
    
    #endregion
    
    #region 触发事件
    
    /// <summary>
    /// 触发事件
    /// </summary>
    public void Emit<T>(string eventName, T eventData)
    {
        var key = eventName + "_" + typeof(T).Name;
        
        // 触发持久订阅者
        if (_eventHandlers.TryGetValue(key, out var handler))
        {
            try
            {
                (handler as Action<T>)?.Invoke(eventData);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[EventBusManager] Error invoking event {eventName}: {ex.Message}");
            }
        }
        
        // 触发一次性订阅者
        if (_oneTimeHandlers.TryGetValue(key, out var oneTimeHandler))
        {
            try
            {
                (oneTimeHandler as Action<T>)?.Invoke(eventData);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[EventBusManager] Error invoking one-time event {eventName}: {ex.Message}");
            }
            finally
            {
                _oneTimeHandlers.Remove(key);
            }
        }
    }
    
    /// <summary>
    /// 触发无参数事件
    /// </summary>
    public void Emit(string eventName)
    {
        // 触发持久订阅者
        if (_eventHandlers.TryGetValue(eventName, out var handler))
        {
            try
            {
                (handler as Action)?.Invoke();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[EventBusManager] Error invoking event {eventName}: {ex.Message}");
            }
        }
        
        // 触发一次性订阅者
        if (_oneTimeHandlers.TryGetValue(eventName, out var oneTimeHandler))
        {
            try
            {
                (oneTimeHandler as Action)?.Invoke();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[EventBusManager] Error invoking one-time event {eventName}: {ex.Message}");
            }
            finally
            {
                _oneTimeHandlers.Remove(eventName);
            }
        }
    }
    
    #endregion
    
    #region 便捷方法
    
    /// <summary>
    /// 检查是否已订阅事件
    /// </summary>
    public bool HasSubscriber(string eventName)
    {
        return _eventHandlers.ContainsKey(eventName) || _oneTimeHandlers.ContainsKey(eventName);
    }
    
    /// <summary>
    /// 获取订阅者数量
    /// </summary>
    public int GetSubscriberCount(string eventName)
    {
        int count = 0;
        if (_eventHandlers.ContainsKey(eventName))
        {
            var handler = _eventHandlers[eventName];
            if (handler is Action action)
            {
                count += action.GetInvocationList().Length;
            }
        }
        return count;
    }
    
    #endregion
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        // 事件总线不保存数据
        return new Dictionary();
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        // 事件总线不加载数据
    }
}
