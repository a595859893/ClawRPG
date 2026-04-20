using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    public partial class SurvivalChallengeSystem
    {
        /// <summary>更新玩家数据</summary>
        private void UpdatePlayerData(string configId, int gold, int exp)
        {
            if (!_playerData.BestWaves.ContainsKey(configId) ||
                _playerData.BestWaves[configId] < _currentChallenge.CurrentWave)
                _playerData.BestWaves[configId] = _currentChallenge.CurrentWave;

            if (!_playerData.BestScores.ContainsKey(configId) ||
                _playerData.BestScores[configId] < _currentChallenge.Score)
                _playerData.BestScores[configId] = _currentChallenge.Score;

            if (!_playerData.BestTimes.ContainsKey(configId) ||
                _playerData.BestTimes[configId] > _currentChallenge.ElapsedTime)
                _playerData.BestTimes[configId] = _currentChallenge.ElapsedTime;

            if (!_playerData.CompletionCount.ContainsKey(configId))
                _playerData.CompletionCount[configId] = 0;
            _playerData.CompletionCount[configId]++;

            if (!_playerData.TotalKills.ContainsKey(configId))
                _playerData.TotalKills[configId] = 0;
            _playerData.TotalKills[configId] += _currentChallenge.EnemiesKilled;

            if (!_playerData.TotalGoldEarned.ContainsKey(configId))
                _playerData.TotalGoldEarned[configId] = 0;
            _playerData.TotalGoldEarned[configId] += gold;
        }

        /// <summary>发放奖励</summary>
        private void GrantRewards(int gold, int exp)
        {
            if (_player == null) return;
            var playerStats = _player.Get("Player") as GodotObject;
            if (playerStats != null)
            {
                if (gold > 0)
                {
                    int currentGold = (int)playerStats.Get("Gold");
                    playerStats.Set("Gold", currentGold + gold);
                }
                if (exp > 0)
                {
                    int currentExp = (int)playerStats.Get("Experience");
                    int currentLevel = (int)playerStats.Get("Level");
                    playerStats.Set("Experience", currentExp + exp);
                    int newExp = (int)playerStats.Get("Experience");
                    int expToLevel = (int)playerStats.Get("ExperienceToNextLevel");
                    if (newExp >= expToLevel)
                    {
                        playerStats.Set("Level", currentLevel + 1);
                        playerStats.Set("Experience", newExp - expToLevel);
                    }
                }
            }
        }

        /// <summary>获取统计数据</summary>
        public Dictionary<string, int> GetStatistics()
        {
            int totalKills = 0, totalGold = 0, completions = 0;
            foreach (var kvp in _playerData.TotalKills) totalKills += kvp.Value;
            foreach (var kvp in _playerData.TotalGoldEarned) totalGold += kvp.Value;
            foreach (var kvp in _playerData.CompletionCount) completions += kvp.Value;
            return new Dictionary<string, int>
            {
                { "total_kills", totalKills },
                { "total_gold", totalGold },
                { "total_completions", completions },
                { "best_wave", GetBestWave() },
                { "best_score", GetBestScore() }
            };
        }

        public int GetBestWave()
        {
            int best = 0;
            foreach (var kvp in _playerData.BestWaves)
                if (kvp.Value > best) best = kvp.Value;
            return best;
        }

        public int GetBestScore()
        {
            int best = 0;
            foreach (var kvp in _playerData.BestScores)
                if (kvp.Value > best) best = kvp.Value;
            return best;
        }

        // ===== 存档 =====
        public override Dictionary<string, object> ExportSaveData()
        {
            var saveDict = new Dictionary<string, object>();
            var bestWaves = new Godot.Collections.Dictionary();
            foreach (var kvp in _playerData.BestWaves) bestWaves[kvp.Key] = kvp.Value;
            saveDict["best_waves"] = bestWaves;

            var bestScores = new Godot.Collections.Dictionary();
            foreach (var kvp in _playerData.BestScores) bestScores[kvp.Key] = kvp.Value;
            saveDict["best_scores"] = bestScores;

            var bestTimes = new Godot.Collections.Dictionary();
            foreach (var kvp in _playerData.BestTimes) bestTimes[kvp.Key] = kvp.Value;
            saveDict["best_times"] = bestTimes;

            var completionCount = new Godot.Collections.Dictionary();
            foreach (var kvp in _playerData.CompletionCount) completionCount[kvp.Key] = kvp.Value;
            saveDict["completion_count"] = completionCount;

            var totalKills = new Godot.Collections.Dictionary();
            foreach (var kvp in _playerData.TotalKills) totalKills[kvp.Key] = kvp.Value;
            saveDict["total_kills"] = totalKills;

            var totalGold = new Godot.Collections.Dictionary();
            foreach (var kvp in _playerData.TotalGoldEarned) totalGold[kvp.Key] = kvp.Value;
            saveDict["total_gold"] = totalGold;

            return saveDict;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            void LoadDict<T>(string key, Dictionary<string, T> target, Func<object, T> converter)
            {
                if (!data.ContainsKey(key)) return;
                var dict = data[key] as Godot.Collections.Dictionary;
                if (dict == null) return;
                foreach (var k in dict.Keys)
                    target[k.ToString()] = converter(dict[k]);
            }

            LoadDict("best_waves", _playerData.BestWaves, v => Convert.ToInt32(v));
            LoadDict("best_scores", _playerData.BestScores, v => Convert.ToInt32(v));
            LoadDict("best_times", _playerData.BestTimes, v => Convert.ToSingle(v));
            LoadDict("completion_count", _playerData.CompletionCount, v => Convert.ToInt32(v));
            LoadDict("total_kills", _playerData.TotalKills, v => Convert.ToInt32(v));
            LoadDict("total_gold", _playerData.TotalGoldEarned, v => Convert.ToInt32(v));

            GD.Print("[SurvivalChallengeSystem] Save data imported");
        }
    }
}
