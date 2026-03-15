using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 生存挑战系统 - 管理生存挑战模式
    /// 支持：无尽波次、限时击杀、Boss Rush、竞技场生存、无尽地下城
    /// </summary>
    public partial class SurvivalChallengeSystem : BaseSystem
    {
        // 单例
        private static SurvivalChallengeSystem _instance;
        public static SurvivalChallengeSystem Instance => _instance ??= new SurvivalChallengeSystem();
        
        // 玩家数据
        private SurvivalChallengeData.PlayerChallengeData _playerData = new();
        
        // 当前活跃挑战
        private SurvivalChallengeData.ActiveChallenge _currentChallenge;
        
        // 敌人列表
        private List<Node2D> _activeEnemies = new();
        
        // 玩家引用
        private Node2D _player;
        
        // 信号
        public Action<SurvivalChallengeData.ChallengeResult> OnChallengeCompleted;
        public Action<SurvivalChallengeData.ActiveChallenge> OnChallengeStarted;
        public Action<int> OnWaveStarted;
        public Action<int> OnEnemyKilled;
        public Action<float> OnTimeUpdated;
        
        // 存档数据
        private string _saveKey = "survival_challenge_data";
        
        public bool IsChallengeActive => _currentChallenge != null && 
            _currentChallenge.State == SurvivalChallengeData.ChallengeState.InProgress;
        
        public SurvivalChallengeData.ActiveChallenge CurrentChallenge => _currentChallenge;

        public override void _Ready()
        {
            base._Ready();
            Initialize();
        }

        protected override void Initialize()
        {
            _instance = this;
            LoadData();
            IsInitialized = true;
            GD.Print("生存挑战系统已初始化");
        }
        
        /// <summary>
        /// 开始挑战
        /// </summary>
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
            
            // 检查玩家金币
            _player = GetTree().GetFirstNodeInGroup("player") as Node2D;
            if (_player == null)
            {
                GD.PrintErr("未找到玩家节点");
                return false;
            }
            
            // 检查金币
            if (config.EntryFee > 0)
            {
                var playerStats = _player.Get("Player") as Godot.Object;
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
            
            // 创建挑战实例
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
            
            // 生成第一波敌人
            SpawnWave(config);
            
            OnChallengeStarted?.Invoke(_currentChallenge);
            GD.Print("挑战开始: " + config.Name);
            return true;
        }
        
        /// <summary>
        /// 更新挑战状态
        /// </summary>
        public void _Process(float delta)
        {
            if (!IsChallengeActive) return;
            
            var config = SurvivalChallengeDatabase.GetChallenge(_currentChallenge.ConfigId);
            if (config == null) return;
            
            // 更新时间
            _currentChallenge.ElapsedTime += delta;
            OnTimeUpdated?.Invoke(_currentChallenge.ElapsedTime);
            
            // 检查时间限制
            if (config.TimeLimit > 0 && _currentChallenge.ElapsedTime >= config.TimeLimit)
            {
                CompleteChallenge(false);
                return;
            }
            
            // 波次处理
            if (_currentChallenge.IsWaveInProgress && _currentChallenge.EnemiesRemaining <= 0)
            {
                // 波次完成
                _currentChallenge.CurrentWave++;
                _currentChallenge.IsWaveInProgress = false;
                
                // 检查是否有限定波次
                if (config.WaveCount > 0 && _currentChallenge.CurrentWave > config.WaveCount)
                {
                    CompleteChallenge(true);
                    return;
                }
                
                // 开始新波次
                _currentChallenge.EnemiesRemaining = config.EnemiesPerWave;
                _currentChallenge.IsWaveInProgress = true;
                SpawnWave(config);
                OnWaveStarted?.Invoke(_currentChallenge.CurrentWave);
            }
            
            // 清理死亡敌人
            CleanupDeadEnemies();
        }
        
        /// <summary>
        /// 生成波次敌人
        /// </summary>
        private void SpawnWave(SurvivalChallengeData.ChallengeConfig config)
        {
            // 根据挑战类型生成不同敌人
            if (config.Type == SurvivalChallengeData.ChallengeType.BossRush)
            {
                SpawnBoss(config);
            }
            else
            {
                for (int i = 0; i < config.EnemiesPerWave; i++)
                {
                    SpawnEnemy(config);
                }
            }
        }
        
        /// <summary>
        /// 生成普通敌人
        /// </summary>
        private void SpawnEnemy(SurvivalChallengeData.ChallengeConfig config)
        {
            // 获取敌人生成器
            var spawner = GetTree().GetFirstNodeInGroup("enemy_spawner");
            if (spawner == null)
            {
                GD.PrintErr("未找到敌人生成器");
                return;
            }
            
            // 在玩家附近生成敌人
            Vector2 spawnPos = GetSpawnPosition();
            
            // 通知敌人生成器生成敌人（简化版本：直接创建）
            // 这里应该调用敌人生成器，实际实现依赖项目具体结构
            _currentChallenge.EnemiesRemaining--;
        }
        
        /// <summary>
        /// 生成Boss
        /// </summary>
        private void SpawnBoss(SurvivalChallengeData.ChallengeConfig config)
        {
            _currentChallenge.EnemiesRemaining = 1;
            // Boss生成逻辑
            GD.Print("生成Boss: Wave " + _currentChallenge.CurrentWave);
        }
        
        /// <summary>
        /// 获取生成位置
        /// </summary>
        private Vector2 GetSpawnPosition()
        {
            if (_player == null) return Vector2.Zero;
            
            // 在玩家周围随机位置生成
            var playerPos = _player.GlobalPosition;
            var randomAngle = (float)GD.RandRange(0, 360);
            var distance = (float)GD.RandRange(200, 400);
            var offset = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * distance;
            
            return playerPos + offset;
        }
        
        /// <summary>
        /// 清理死亡敌人
        /// </summary>
        private void CleanupDeadEnemies()
        {
            _activeEnemies.RemoveAll(enemy => !IsInstanceValid(enemy) || !enemy.IsInsideTree());
        }
        
        /// <summary>
        /// 记录击杀
        /// </summary>
        public void RecordKill(Node2D enemy)
        {
            if (!IsChallengeActive) return;
            
            _currentChallenge.EnemiesKilled++;
            _currentChallenge.EnemiesRemaining--;
            
            // 计算得分
            int killScore = 10;
            if (enemy.HasMethod("Get") && enemy.Get("IsBoss") is bool isBoss && isBoss)
            {
                killScore = 100;
            }
            _currentChallenge.Score += killScore;
            
            OnEnemyKilled?.Invoke(_currentChallenge.EnemiesKilled);
        }
        
        /// <summary>
        /// 记录造成伤害
        /// </summary>
        public void RecordDamageDealt(int damage)
        {
            if (!IsChallengeActive) return;
            _currentChallenge.DamageDealt += damage;
            _currentChallenge.Score += damage / 10;
        }
        
        /// <summary>
        /// 记录受到伤害
        /// </summary>
        public void RecordDamageTaken(int damage)
        {
            if (!IsChallengeActive) return;
            _currentChallenge.DamageTaken += damage;
        }
        
        /// <summary>
        /// 完成挑战
        /// </summary>
        public void CompleteChallenge(bool success)
        {
            if (_currentChallenge == null) return;
            
            var config = SurvivalChallengeDatabase.GetChallenge(_currentChallenge.ConfigId);
            if (config == null) return;
            
            _currentChallenge.State = success ? 
                SurvivalChallengeData.ChallengeState.Completed : 
                SurvivalChallengeData.ChallengeState.Failed;
            
            // 计算奖励
            int goldReward = 0;
            int expReward = 0;
            
            if (success || _currentChallenge.EnemiesKilled > 0)
            {
                // 基于击杀数和波次计算奖励
                float waveMultiplier = 1.0f + (_currentChallenge.CurrentWave - 1) * 0.2f;
                goldReward = (int)(config.BaseGoldReward * waveMultiplier * config.GoldMultiplier);
                expReward = (int)(config.BaseExpReward * waveMultiplier * config.ExpMultiplier);
                
                // 额外奖励
                goldReward += _currentChallenge.EnemiesKilled * 5;
                expReward += _currentChallenge.EnemiesKilled * 2;
            }
            
            // 更新玩家数据
            UpdatePlayerData(config.Id, goldReward, expReward);
            
            // 发放奖励
            if (goldReward > 0 || expReward > 0)
            {
                GrantRewards(goldReward, expReward);
            }
            
            // 创建结果
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
            
            // 保存数据
            SaveData();
            
            // 发送信号
            OnChallengeCompleted?.Invoke(result);
            
            GD.Print($"挑战完成: {config.Name}, 波次: {result.WaveReached}, 击杀: {result.EnemiesKilled}, 得分: {result.Score}, 评级: {result.Grade}");
            
            _currentChallenge = null;
        }
        
        /// <summary>
        /// 计算评级
        /// </summary>
        private string CalculateGrade(bool success)
        {
            if (!success) return "D";
            
            var config = SurvivalChallengeDatabase.GetChallenge(_currentChallenge.ConfigId);
            if (config == null) return "C";
            
            // 基于击杀数和时间计算评级
            int targetKills = config.WaveCount > 0 ? config.WaveCount * config.EnemiesPerWave : 
                (int)(config.TimeLimit / config.EnemySpawnInterval);
            
            float killRatio = (float)_currentChallenge.EnemiesKilled / Mathf.Max(1, targetKills);
            
            if (killRatio >= 0.9f) return "S";
            if (killRatio >= 0.7f) return "A";
            if (killRatio >= 0.5f) return "B";
            if (killRatio >= 0.3f) return "C";
            return "D";
        }
        
        /// <summary>
        /// 更新玩家数据
        /// </summary>
        private void UpdatePlayerData(string configId, int gold, int exp)
        {
            // 更新最佳波次
            if (!_playerData.BestWaves.ContainsKey(configId) || 
                _playerData.BestWaves[configId] < _currentChallenge.CurrentWave)
            {
                _playerData.BestWaves[configId] = _currentChallenge.CurrentWave;
            }
            
            // 更新最高分
            if (!_playerData.BestScores.ContainsKey(configId) || 
                _playerData.BestScores[configId] < _currentChallenge.Score)
            {
                _playerData.BestScores[configId] = _currentChallenge.Score;
            }
            
            // 更新最佳时间
            if (!_playerData.BestTimes.ContainsKey(configId) || 
                _playerData.BestTimes[configId] > _currentChallenge.ElapsedTime)
            {
                _playerData.BestTimes[configId] = _currentChallenge.ElapsedTime;
            }
            
            // 更新完成次数
            if (!_playerData.CompletionCount.ContainsKey(configId))
            {
                _playerData.CompletionCount[configId] = 0;
            }
            _playerData.CompletionCount[configId]++;
            
            // 更新总击杀
            if (!_playerData.TotalKills.ContainsKey(configId))
            {
                _playerData.TotalKills[configId] = 0;
            }
            _playerData.TotalKills[configId] += _currentChallenge.EnemiesKilled;
            
            // 更新总金币
            if (!_playerData.TotalGoldEarned.ContainsKey(configId))
            {
                _playerData.TotalGoldEarned[configId] = 0;
            }
            _playerData.TotalGoldEarned[configId] += gold;
        }
        
        /// <summary>
        /// 发放奖励
        /// </summary>
        private void GrantRewards(int gold, int exp)
        {
            if (_player == null) return;
            
            var playerStats = _player.Get("Player") as Godot.Object;
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
                    
                    // 检查升级
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
        
        /// <summary>
        /// 获取统计数据
        /// </summary>
        public Dictionary<string, int> GetStatistics()
        {
            int totalKills = 0;
            int totalGold = 0;
            int completions = 0;
            
            foreach (var kvp in _playerData.TotalKills)
            {
                totalKills += kvp.Value;
            }
            
            foreach (var kvp in _playerData.TotalGoldEarned)
            {
                totalGold += kvp.Value;
            }
            
            foreach (var kvp in _playerData.CompletionCount)
            {
                completions += kvp.Value;
            }
            
            return new Dictionary<string, int>
            {
                { "total_kills", totalKills },
                { "total_gold", totalGold },
                { "total_completions", completions },
                { "best_wave", GetBestWave() },
                { "best_score", GetBestScore() }
            };
        }
        
        /// <summary>
        /// 获取最佳波次
        /// </summary>
        public int GetBestWave()
        {
            int best = 0;
            foreach (var kvp in _playerData.BestWaves)
            {
                if (kvp.Value > best) best = kvp.Value;
            }
            return best;
        }
        
        /// <summary>
        /// 获取最高分
        /// </summary>
        public int GetBestScore()
        {
            int best = 0;
            foreach (var kvp in _playerData.BestScores)
            {
                if (kvp.Value > best) best = kvp.Value;
            }
            return best;
        }
        
        /// <summary>
        /// 放弃当前挑战
        /// </summary>
        public void AbandonChallenge()
        {
            if (!IsChallengeActive) return;
            
            CompleteChallenge(false);
            GD.Print("挑战已放弃");
        }
        
        /// <summary>
        /// 保存数据
        /// </summary>
        protected override Dictionary ExportSaveData()
        {
            var saveDict = new Dictionary();
            
            var bestWaves = new Godot.Collections.Dictionary();
            foreach (var kvp in _playerData.BestWaves)
                bestWaves[kvp.Key] = kvp.Value;
            saveDict["best_waves"] = bestWaves;
            
            var bestScores = new Godot.Collections.Dictionary();
            foreach (var kvp in _playerData.BestScores)
                bestScores[kvp.Key] = kvp.Value;
            saveDict["best_scores"] = bestScores;
            
            var bestTimes = new Godot.Collections.Dictionary();
            foreach (var kvp in _playerData.BestTimes)
                bestTimes[kvp.Key] = kvp.Value;
            saveDict["best_times"] = bestTimes;
            
            var completionCount = new Godot.Collections.Dictionary();
            foreach (var kvp in _playerData.CompletionCount)
                completionCount[kvp.Key] = kvp.Value;
            saveDict["completion_count"] = completionCount;
            
            var totalKills = new Godot.Collections.Dictionary();
            foreach (var kvp in _playerData.TotalKills)
                totalKills[kvp.Key] = kvp.Value;
            saveDict["total_kills"] = totalKills;
            
            var totalGold = new Godot.Collections.Dictionary();
            foreach (var kvp in _playerData.TotalGoldEarned)
                totalGold[kvp.Key] = kvp.Value;
            saveDict["total_gold"] = totalGold;
            
            return saveDict;
        }
        
        /// <summary>
        /// 加载数据
        /// </summary>
        protected override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            // 加载最佳波次
            if (data.ContainsKey("best_waves"))
            {
                var bestWaves = data["best_waves"] as Godot.Collections.Dictionary;
                if (bestWaves != null)
                {
                    foreach (var key in bestWaves.Keys)
                    {
                        _playerData.BestWaves[key.ToString()] = Convert.ToInt32(bestWaves[key]);
                    }
                }
            }
            
            // 加载最高分
            if (data.ContainsKey("best_scores"))
            {
                var bestScores = data["best_scores"] as Godot.Collections.Dictionary;
                if (bestScores != null)
                {
                    foreach (var key in bestScores.Keys)
                    {
                        _playerData.BestScores[key.ToString()] = Convert.ToInt32(bestScores[key]);
                    }
                }
            }
            
            // 加载最佳时间
            if (data.ContainsKey("best_times"))
            {
                var bestTimes = data["best_times"] as Godot.Collections.Dictionary;
                if (bestTimes != null)
                {
                    foreach (var key in bestTimes.Keys)
                    {
                        _playerData.BestTimes[key.ToString()] = Convert.ToSingle(bestTimes[key]);
                    }
                }
            }
            
            // 加载完成次数
            if (data.ContainsKey("completion_count"))
            {
                var completionCount = data["completion_count"] as Godot.Collections.Dictionary;
                if (completionCount != null)
                {
                    foreach (var key in completionCount.Keys)
                    {
                        _playerData.CompletionCount[key.ToString()] = Convert.ToInt32(completionCount[key]);
                    }
                }
            }
            
            // 加载总击杀数
            if (data.ContainsKey("total_kills"))
            {
                var totalKills = data["total_kills"] as Godot.Collections.Dictionary;
                if (totalKills != null)
                {
                    foreach (var key in totalKills.Keys)
                    {
                        _playerData.TotalKills[key.ToString()] = Convert.ToInt32(totalKills[key]);
                    }
                }
            }
            
            // 加载总金币
            if (data.ContainsKey("total_gold"))
            {
                var totalGold = data["total_gold"] as Godot.Collections.Dictionary;
                if (totalGold != null)
                {
                    foreach (var key in totalGold.Keys)
                    {
                        _playerData.TotalGoldEarned[key.ToString()] = Convert.ToInt32(totalGold[key]);
                    }
                }
            }
            
            GD.Print("[SurvivalChallengeSystem] Save data imported");
        }
    }
}
