using Godot;
using System;
using ClawRPG.Scripts.Managers;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// GameEventHandler - 处理游戏全局事件订阅
    /// </summary>
    public partial class GameEventHandler : BaseSystem
    {
        private Main _main;
        
        public void Initialize(Main main)
        {
            _main = main;
            SubscribeEvents();
            GD.Print("[GameEventHandler] Events subscribed");
        }
        
        /// <summary>
        /// 订阅所有游戏事件
        /// </summary>
        private void SubscribeEvents()
        {
            if (EventBusManager.Instance == null) return;

            // 玩家死亡事件
            EventBusManager.Instance.Subscribe<PlayerDiedEventData>(EventBusManager.Events.PlayerDied, OnPlayerDied);
            
            // 敌人击杀事件
            EventBusManager.Instance.Subscribe<EnemyDiedEventData>(EventBusManager.Events.EnemyDied, OnEnemyDied);
            
            // 场景切换事件
            EventBusManager.Instance.Subscribe<string>(EventBusManager.Events.SceneChanged, OnSceneChanged);
            
            // 游戏暂停/恢复事件
            EventBusManager.Instance.Subscribe<GamePauseEventData>(EventBusManager.Events.GamePaused, OnGamePaused);
            EventBusManager.Instance.Subscribe<GamePauseEventData>(EventBusManager.Events.GameResumed, OnGameResumed);
            
            // 游戏结束事件
            EventBusManager.Instance.Subscribe<GameOverEventData>(EventBusManager.Events.GameOver, OnGameOver);
        }
        
        /// <summary>
        /// 连接信号（成就、称号、任务等）
        /// </summary>
        public void ConnectSignals()
        {
            // 成就解锁声音
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked += achievement =>
                {
                    SoundEffectSystem.Instance?.PlayAchievementUnlock();
                };
            }

            // 称号解锁声音
            if (TitleSystem.Instance != null)
            {
                TitleSystem.Instance.OnTitleUnlocked += title =>
                {
                    SoundEffectSystem.Instance?.PlayTitleUnlock();
                };
            }

            // 任务完成声音
            QuestSystem.OnQuestCompleted += quest =>
            {
                SoundEffectSystem.Instance?.PlayQuestComplete();
            };
            
            GD.Print("[GameEventHandler] Signals connected");
        }
        
        private void OnPlayerDied(PlayerDiedEventData data)
        {
            GD.Print($"[Main] Player died! Death count: {data.DeathCount}");
        }
        
        private void OnEnemyDied(EnemyDiedEventData data)
        {
            GD.Print($"[Main] Enemy killed! Total kills: {data.KillCount}");
        }
        
        private void OnSceneChanged(string scenePath)
        {
            GD.Print($"[Main] Scene changed to: {scenePath}");
        }
        
        private void OnGamePaused(GamePauseEventData data)
        {
            GD.Print($"[Main] Game paused at playtime: {data.PlayTime}");
        }
        
        private void OnGameResumed(GamePauseEventData data)
        {
            GD.Print("[Main] Game resumed");
        }
        
        private void OnGameOver(GameOverEventData data)
        {
            GD.Print($"[Main] Game Over! Play time: {data.TotalPlayTime}s, Kills: {data.KillCount}, Deaths: {data.DeathCount}");
        }
    }
}
