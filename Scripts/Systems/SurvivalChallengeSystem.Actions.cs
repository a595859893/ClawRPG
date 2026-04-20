using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    public partial class SurvivalChallengeSystem
    {
        /// <summary>开始挑战</summary>
        public bool StartChallenge(string configId)
        {
            if (IsChallengeActive)
            {
                GD.PrintErr("已有进行中的挑战");
                return false;
            }
            var config = SurvivalChallengeDatabase.GetChallenge(configId);
            if (config == null)
            {
                GD.PrintErr("未找到挑战配置: " + configId);
                return false;
            }
            _player = GetTree().GetFirstNodeInGroup("player") as Node2D;
            if (_player == null)
            {
                GD.PrintErr("未找到玩家节点");
                return false;
            }
            if (config.EntryFee > 0)
            {
                var playerStats = _player.Get("Player") as GodotObject;
                if (playerStats != null)
                {
                    int playerGold = (int)playerStats.Get("Gold");
                    if (playerGold < config.EntryFee)
                    {
                        GD.PrintErr("金币不足: 需要 " + config.EntryFee + ", 当前 " + playerGold);
                        return false;
                    }
                    playerStats.Set("Gold", playerGold - config.EntryFee);
                }
            }
            _currentChallenge = new SurvivalChallengeData.ActiveChallenge
            {
                InstanceId = Guid.NewGuid().ToString(),
                ConfigId = configId,
                State = SurvivalChallengeData.ChallengeState.InProgress,
                CurrentWave = 1,
                EnemiesKilled = 0,
                DamageDealt = 0,
                DamageTaken = 0,
                EnemiesRemaining = config.EnemiesPerWave,
                ElapsedTime = 0f,
                LastSpawnTime = 0f,
                Score = 0,
                IsWaveInProgress = true
            };
            SpawnWave(config);
            OnChallengeStarted?.Invoke(_currentChallenge);
            GD.Print("挑战开始: " + config.Name);
            return true;
        }

        /// <summary>完成挑战</summary>
        public void CompleteChallenge(bool success)
        {
            if (_currentChallenge == null) return;
            var config = SurvivalChallengeDatabase.GetChallenge(_currentChallenge.ConfigId);
            if (config == null) return;
            _currentChallenge.State = success ?
                SurvivalChallengeData.ChallengeState.Completed :
                SurvivalChallengeData.ChallengeState.Failed;

            int goldReward = 0;
            int expReward = 0;
            if (success || _currentChallenge.EnemiesKilled > 0)
            {
                float waveMultiplier = 1.0f + (_currentChallenge.CurrentWave - 1) * 0.2f;
                goldReward = (int)(config.BaseGoldReward * waveMultiplier * config.GoldMultiplier);
                expReward = (int)(config.BaseExpReward * waveMultiplier * config.ExpMultiplier);
                goldReward += _currentChallenge.EnemiesKilled * 5;
                expReward += _currentChallenge.EnemiesKilled * 2;
            }
            UpdatePlayerData(config.Id, goldReward, expReward);
            if (goldReward > 0 || expReward > 0)
                GrantRewards(goldReward, expReward);

            var result = new SurvivalChallengeData.ChallengeResult
            {
                ConfigId = config.Id,
                Success = success,
                WaveReached = _currentChallenge.CurrentWave,
                EnemiesKilled = _currentChallenge.EnemiesKilled,
                DamageDealt = _currentChallenge.DamageDealt,
                DamageTaken = _currentChallenge.DamageTaken,
                TimeElapsed = _currentChallenge.ElapsedTime,
                Score = _currentChallenge.Score,
                GoldReward = goldReward,
                ExpReward = expReward,
                Grade = CalculateGrade(success)
            };
            SaveData();
            OnChallengeCompleted?.Invoke(result);
            GD.Print($"挑战完成: {config.Name}, 波次: {result.WaveReached}, 击杀: {result.EnemiesKilled}, 得分: {result.Score}, 评级: {result.Grade}");
            _currentChallenge = null;
        }

        /// <summary>放弃当前挑战</summary>
        public void AbandonChallenge()
        {
            if (!IsChallengeActive) return;
            CompleteChallenge(false);
            GD.Print("挑战已放弃");
        }

        private string CalculateGrade(bool success)
        {
            if (!success) return "D";
            var config = SurvivalChallengeDatabase.GetChallenge(_currentChallenge.ConfigId);
            if (config == null) return "C";
            int targetKills = config.WaveCount > 0 ? config.WaveCount * config.EnemiesPerWave :
                (int)(config.TimeLimit / config.EnemySpawnInterval);
            float killRatio = (float)_currentChallenge.EnemiesKilled / Mathf.Max(1, targetKills);
            if (killRatio >= 0.9f) return "S";
            if (killRatio >= 0.7f) return "A";
            if (killRatio >= 0.5f) return "B";
            if (killRatio >= 0.3f) return "C";
            return "D";
        }
    }
}
