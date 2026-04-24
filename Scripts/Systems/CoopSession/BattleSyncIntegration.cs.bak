using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 战斗同步集成助手
    /// 提供 CoopSessionSystem 与 BattleSyncSystem 之间的桥接
    /// </summary>
    public class BattleSyncIntegration
    {
        private BattleSyncSystem _battleSync;
        private CoopSessionSystem _coopSession;
        
        public BattleSyncIntegration(CoopSessionSystem coopSession)
        {
            _coopSession = coopSession;
            _battleSync = BattleSyncSystem.Instance;
            
            // 订阅战斗同步信号
            if (_battleSync != null)
            {
                _battleSync.ConnectBattleSignals();
            }
        }

        /// <summary>
        /// 开始战斗会话（当玩家进入地下城时调用）
        /// </summary>
        public void OnSessionStarted(string sessionId)
        {
            var session = _coopSession.GetSession(sessionId);
            if (session == null) return;

            if (_battleSync != null)
            {
                _battleSync.StartBattleSession(sessionId, session.Party.Members);
                GD.Print($"[BattleSyncIntegration] Battle session started for: {sessionId}");
            }
        }

        /// <summary>
        /// 结束战斗会话
        /// </summary>
        public void OnSessionEnded(string sessionId)
        {
            if (_battleSync != null)
            {
                _battleSync.EndBattleSession();
                GD.Print($"[BattleSyncIntegration] Battle session ended for: {sessionId}");
            }
        }

        /// <summary>
        /// 玩家加入战斗
        /// </summary>
        public void OnPlayerJoined(int playerId, string playerName, int level)
        {
            if (_battleSync != null)
            {
                // 根据等级设置玩家最大生命值
                float maxHealth = 100 + (level - 1) * 10;
                float maxMana = 100 + (level - 1) * 5;
                _battleSync.AddPlayer(playerId, playerName, maxHealth, maxMana);
            }
        }

        /// <summary>
        /// 玩家离开战斗
        /// </summary>
        public void OnPlayerLeft(int playerId)
        {
            _battleSync?.RemovePlayer(playerId);
        }

        /// <summary>
        /// 记录攻击操作
        /// </summary>
        public void RecordAttack(int attackerId, string attackerName, int targetId, float damage, bool isCritical = false)
        {
            _battleSync?.RecordAction(attackerId, attackerName, BattleActionType.Attack, damage, "", targetId, 0, 0, isCritical);
        }

        /// <summary>
        /// 记录技能释放
        /// </summary>
        public void RecordSkill(int casterId, string casterName, string skillId, int targetId, float value, bool isCritical = false)
        {
            _battleSync?.RecordAction(casterId, casterName, BattleActionType.Skill, value, skillId, targetId, 0, 0, isCritical);
        }

        /// <summary>
        /// 记录治疗
        /// </summary>
        public void RecordHeal(int healerId, string healerName, int targetId, float amount)
        {
            _battleSync?.RecordAction(healerId, healerName, BattleActionType.Heal, amount, "", targetId);
        }

        /// <summary>
        /// 记录受到伤害
        /// </summary>
        public void RecordDamage(int targetId, int attackerId, float damage)
        {
            var target = _battleSync?.GetPlayerState(targetId);
            if (target != null)
            {
                _battleSync?.RecordAction(attackerId, "Enemy", BattleActionType.Damage, damage, "", targetId);
            }
        }

        /// <summary>
        /// 施加Buff
        /// </summary>
        public void ApplyBuff(int casterId, string casterName, int targetId, string buffId, float duration)
        {
            _battleSync?.RecordAction(casterId, casterName, BattleActionType.BuffApply, duration, buffId, targetId);
        }

        /// <summary>
        /// 移除Buff
        /// </summary>
        public void RemoveBuff(int casterId, string casterName, int targetId, string buffId)
        {
            _battleSync?.RecordAction(casterId, casterName, BattleActionType.BuffRemove, 0, buffId, targetId);
        }

        /// <summary>
        /// 记录玩家死亡
        /// </summary>
        public void RecordDeath(int playerId)
        {
            var player = _battleSync?.GetPlayerState(playerId);
            if (player != null)
            {
                _battleSync?.RecordAction(playerId, player.PlayerName, BattleActionType.Death);
            }
        }

        /// <summary>
        /// 记录玩家复活
        /// </summary>
        public void RecordRevive(int playerId, float healthPercent = 0.5f)
        {
            var player = _battleSync?.GetPlayerState(playerId);
            if (player != null)
            {
                _battleSync?.RecordAction(playerId, player.PlayerName, BattleActionType.Revive, healthPercent);
            }
        }

        /// <summary>
        /// 添加敌人
        /// </summary>
        public void AddEnemy(int enemyId, string enemyType, float maxHealth, float x, float y)
        {
            _battleSync?.AddEnemy(enemyId, enemyType, maxHealth, x, y);
        }

        /// <summary>
        /// 敌人死亡
        /// </summary>
        public void OnEnemyKilled(int enemyId, int killerId)
        {
            _battleSync?.RemoveEnemy(enemyId);
            
            // 记录贡献到 CoopSession
            _coopSession.RecordContribution(_battleSync.CurrentSessionId, killerId, damage: 0, kills: 1);
        }

        /// <summary>
        /// 设置敌人仇恨（用于吸引仇恨玩法）
        /// </summary>
        public void SetEnemyAggro(int enemyId, int targetPlayerId)
        {
            _battleSync?.SetEnemyAggro(enemyId, targetPlayerId);
        }

        /// <summary>
        /// 获取所有存活玩家
        /// </summary>
        public List<BattleSyncData.PlayerBattleState> GetAlivePlayers()
        {
            if (_battleSync == null) return new List<BattleSyncData.PlayerBattleState>();
            return _battleSync.GetAllPlayerStates().Where(p => !p.IsDead).ToList();
        }

        /// <summary>
        /// 获取低血量玩家（需要治疗）
        /// </summary>
        public List<BattleSyncData.PlayerBattleState> GetLowHealthPlayers(float threshold = 0.3f)
        {
            if (_battleSync == null) return new List<BattleSyncData.PlayerBattleState>();
            return _battleSync.GetAllPlayerStates()
                .Where(p => !p.IsDead && p.Health / p.MaxHealth < threshold)
                .ToList();
        }

        /// <summary>
        /// 获取当前仇恨目标的玩家（坦克）
        /// </summary>
        public int? GetTankPlayerId(int enemyId)
        {
            var enemy = _battleSync?.GetEnemyState(enemyId);
            if (enemy != null && enemy.AggroPlayerId > 0)
            {
                return (int)enemy.AggroPlayerId;
            }
            return null;
        }
    }

    /// <summary>
    /// 战斗同步集成扩展方法
    /// </summary>
    public static class BattleSyncExtensions
    {
        /// <summary>
        /// 战斗同步系统信号连接扩展方法
        /// </summary>
        public static void ConnectBattleSignals(this BattleSyncSystem battleSync)
        {
            // 连接玩家死亡信号 - 自动记录到CoopSession
            battleSync.OnPlayerDied += (playerId) =>
            {
                var session = CoopSessionSystem.Instance.GetCurrentSession();
                if (session != null)
                {
                    CoopSessionSystem.Instance.UpdatePlayerHealth(session.SessionId, playerId, 0);
                }
            };

            // 连接玩家复活信号 - 更新CoopSession状态
            battleSync.OnPlayerRevived += (playerId) =>
            {
                var session = CoopSessionSystem.Instance.GetCurrentSession();
                if (session != null)
                {
                    CoopSessionSystem.Instance.RevivePlayer(session.SessionId, playerId);
                }
            };

            // 连接敌人击杀信号 - 记录贡献
            battleSync.OnEnemyKilled += (enemyId, killerId) =>
            {
                var session = CoopSessionSystem.Instance.GetCurrentSession();
                if (session != null)
                {
                    CoopSessionSystem.Instance.RecordContribution(session.SessionId, killerId, kills: 1);
                }
            };

            GD.Print("[BattleSyncIntegration] Battle signals connected");
        }
    }
}
