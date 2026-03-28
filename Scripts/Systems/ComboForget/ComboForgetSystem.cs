using Godot;
using System;

namespace Framework
{
    /// <summary>
    /// Combo 遗忘系统 - Godot Node 单例
    /// 订阅 GameOver 事件，在每局游戏结束时推进遗忘逻辑
    /// </summary>
    public class ComboForgetSystem : Node
    {
        public static ComboForgetSystem Instance { get; private set; }
        
        // 确保 ComboForgetData 单例在系统启动时就初始化
        private static readonly ComboForgetData _forgetDataInstance = new ComboForgetData();
        private ComboForgetData _data;
        
        public override void _Ready()
        {
            Instance = this;
            _data = ComboForgetData.Instance;
            
            // 订阅游戏结束事件
            EventBusManager.Instance.Subscribe<GameOverEventData>(EventBusManager.Events.GameOver, OnGameOver);
            
            // 订阅 ComboSystem 的执行事件
            ComboSystem.ComboExecuted += OnComboExecuted;
            
            GD.Print("[ComboForgetSystem] Initialized");
        }
        
        public override void _ExitTree()
        {
            EventBusManager.Instance.Unsubscribe<GameOverEventData>(EventBusManager.Events.GameOver, OnGameOver);
            ComboSystem.ComboExecuted -= OnComboExecuted;
        }
        
        private void OnGameOver(GameOverEventData data)
        {
            // 每局游戏结束时，推进所有 combo 的遗忘计时
            _data.OnRunEnded();
        }
        
        private void OnComboExecuted(string comboId, float damage, string effectName)
        {
            // 当 combo 被执行时，记录使用（唤醒或重置计时）
            _data.RecordComboUsage(comboId);
        }
        
        // ========== 公开 API ==========
        
        /// <summary>
        /// 注册一个新发现的 combo
        /// </summary>
        public void RegisterCombo(string comboId)
        {
            _data.RegisterCombo(comboId);
        }
        
        /// <summary>
        /// 检查 combo 是否处于休眠状态
        /// </summary>
        public bool IsDormant(string comboId) => _data.IsDormant(comboId);
        
        /// <summary>
        /// 尝试锁定一个 combo
        /// </summary>
        public bool TryLockCombo(string comboId) => _data.TryLockCombo(comboId);
        
        /// <summary>
        /// 解锁一个 combo
        /// </summary>
        public void UnlockCombo(string comboId) => _data.UnlockCombo(comboId);
        
        /// <summary>
        /// 获取已锁定的 combo 数量
        /// </summary>
        public int GetLockedCount() => _data.GetLockedCount();
        
        /// <summary>
        /// 获取已锁定的 combo ID 列表
        /// </summary>
        public System.Collections.Generic.List<string> GetLockedComboIds() => _data.GetLockedComboIds();
        
        /// <summary>
        /// 获取遗忘信息
        /// </summary>
        public (int games, bool isLocked, bool isDormant, int totalUse) GetForgetInfo(string comboId)
            => _data.GetForgetInfo(comboId);
        
        /// <summary>
        /// 获取最多可锁定的数量
        /// </summary>
        public int GetMaxLocked() => ComboForgetData.MAX_LOCKED_COMBOS;
    }
}
