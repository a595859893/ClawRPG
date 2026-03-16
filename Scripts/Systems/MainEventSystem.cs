using Godot;
using System;
using ClawRPG.Scripts.Events;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// MainEventSystem - 负责事件总线连接和游戏事件处理
    /// </summary>
    public partial class MainEventSystem : BaseSystem
    {
        private Main _main;

        public void Initialize(Main main)
        {
            _main = main;
        }

        /// <summary>
        /// 连接所有信号
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

            // 通过 EventBus 订阅游戏事件（事件驱动架构）
            ConnectEventBusSignals();

            GD.Print("Signals connected");
        }
        
        /// <summary>
        /// 连接事件总线信号
        /// </summary>
        public void ConnectEventBusSignals()
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
            
            GD.Print("[MainEventSystem] EventBus signals connected");
        }
        
        /// <summary>
        /// 处理玩家死亡事件
        /// </summary>
        public void OnPlayerDied(PlayerDiedEventData data)
        {
            GD.Print($"[MainEventSystem] Player died! Death count: {data.DeathCount}");
        }
        
        /// <summary>
        /// 处理敌人击杀事件
        /// </summary>
        public void OnEnemyDied(EnemyDiedEventData data)
        {
            GD.Print($"[MainEventSystem] Enemy killed! Total kills: {data.KillCount}");
        }
        
        /// <summary>
        /// 处理场景切换事件
        /// </summary>
        public void OnSceneChanged(string scenePath)
        {
            GD.Print($"[MainEventSystem] Scene changed to: {scenePath}");
        }
        
        /// <summary>
        /// 处理游戏暂停事件
        /// </summary>
        public void OnGamePaused(GamePauseEventData data)
        {
            GD.Print($"[MainEventSystem] Game paused at playtime: {data.PlayTime}");
        }
        
        /// <summary>
        /// 处理游戏恢复事件
        /// </summary>
        public void OnGameResumed(GamePauseEventData data)
        {
            GD.Print("[MainEventSystem] Game resumed");
        }
        
        /// <summary>
        /// 处理游戏结束事件
        /// </summary>
        public void OnGameOver(GameOverEventData data)
        {
            GD.Print($"[MainEventSystem] Game Over! Play time: {data.TotalPlayTime}s, Kills: {data.KillCount}, Deaths: {data.DeathCount}");
        }

        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            // MainEventSystem 主要负责事件连接，无持久化状态
            return data;
        }

        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            base.ImportSaveData(data);
            // MainEventSystem 主要负责事件连接，无持久化状态
        }
    }
}
