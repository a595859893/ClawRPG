using Godot;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    public partial class SurvivalChallengeSystem
    {
        /// <summary>生成波次敌人</summary>
        private void SpawnWave(SurvivalChallengeData.ChallengeConfig config)
        {
            if (config.Type == SurvivalChallengeData.ChallengeType.BossRush)
                SpawnBoss(config);
            else
                for (int i = 0; i < config.EnemiesPerWave; i++)
                    SpawnEnemy(config);
        }

        /// <summary>生成普通敌人</summary>
        private void SpawnEnemy(SurvivalChallengeData.ChallengeConfig config)
        {
            var spawner = GetTree().GetFirstNodeInGroup("enemy_spawner");
            if (spawner == null)
            {
                GD.PrintErr("未找到敌人生成器");
                return;
            }
            Vector2 spawnPos = GetSpawnPosition();
            _currentChallenge.EnemiesRemaining--;
        }

        /// <summary>生成Boss</summary>
        private void SpawnBoss(SurvivalChallengeData.ChallengeConfig config)
        {
            _currentChallenge.EnemiesRemaining = 1;
            GD.Print("生成Boss: Wave " + _currentChallenge.CurrentWave);
        }

        /// <summary>获取生成位置</summary>
        private Vector2 GetSpawnPosition()
        {
            if (_player == null) return Vector2.Zero;
            var playerPos = _player.GlobalPosition;
            float randomAngle = (float)GD.RandRange(0, 360);
            float distance = (float)GD.RandRange(200, 400);
            var offset = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * distance;
            return playerPos + offset;
        }

        /// <summary>清理死亡敌人</summary>
        private void CleanupDeadEnemies()
        {
            _activeEnemies.RemoveAll(enemy => !IsInstanceValid(enemy) || !enemy.IsInsideTree());
        }

        // ===== 记录方法 =====
        public void RecordKill(Node2D enemy)
        {
            if (!IsChallengeActive) return;
            _currentChallenge.EnemiesKilled++;
            _currentChallenge.EnemiesRemaining--;
            int killScore = 10;
            if (enemy.HasMethod("Get") && enemy.Get("IsBoss") is bool isBoss && isBoss)
                killScore = 100;
            _currentChallenge.Score += killScore;
            OnEnemyKilled?.Invoke(_currentChallenge.EnemiesKilled);
        }

        public void RecordDamageDealt(int damage)
        {
            if (!IsChallengeActive) return;
            _currentChallenge.DamageDealt += damage;
            _currentChallenge.Score += damage / 10;
        }

        public void RecordDamageTaken(int damage)
        {
            if (!IsChallengeActive) return;
            _currentChallenge.DamageTaken += damage;
        }
    }
}
