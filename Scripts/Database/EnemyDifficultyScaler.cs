using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Database {
    /// <summary>
    /// 敌人难度缩放类 - 根据玩家等级和游戏进度调整敌人属性
    /// </summary>
    public partial class EnemyDifficultyScaler : BaseSystem {
        
        /// <summary>
        /// 难度等级
        /// </summary>
        public enum DifficultyLevel {
            Easy = 1,
            Normal = 2,
            Hard = 3,
            Nightmare = 4
        }
        
        private DifficultyLevel _currentDifficulty = DifficultyLevel.Normal;
        private int _playerLevel = 1;
        private float _difficultyMultiplier = 1.0f;
        
        public override void _Ready() {
            base._Ready();
        }
        
        /// <summary>
        /// 缩放敌人属性
        /// </summary>
        public EnemyType ScaleDifficulty(EnemyType baseEnemy) {
            var scaled = new EnemyType(
                baseEnemy.Id,
                baseEnemy.Name,
                (int)(baseEnemy.MaxHealth * _difficultyMultiplier),
                baseEnemy.MoveSpeed * (1.0f + (_difficultyMultiplier - 1.0f) * 0.5f),
                baseEnemy.AttackDamage * _difficultyMultiplier
            );
            
            scaled.Description = baseEnemy.Description;
            scaled.ExperienceReward = (int)(baseEnemy.ExperienceReward * _difficultyMultiplier);
            scaled.GoldReward = (int)(baseEnemy.GoldReward * _difficultyMultiplier);
            
            return scaled;
        }
        
        /// <summary>
        /// 获取缩放后的属性
        /// </summary>
        public (int health, float damage, float speed) GetScaledStats(int baseHealth, float baseDamage, float baseSpeed) {
            var health = (int)(baseHealth * _difficultyMultiplier);
            var damage = baseDamage * _difficultyMultiplier;
            var speed = baseSpeed * (1.0f + (_difficultyMultiplier - 1.0f) * 0.5f);
            
            return (health, damage, speed);
        }
        
        /// <summary>
        /// 计算等级乘数
        /// </summary>
        public float CalculateLevelMultiplier(int enemyLevel, int playerLevel) {
            if (playerLevel <= enemyLevel) {
                return 1.0f;
            }
            
            var levelDiff = playerLevel - enemyLevel;
            return 1.0f + levelDiff * 0.1f;
        }
        
        /// <summary>
        /// 设置难度等级
        /// </summary>
        public void SetDifficulty(DifficultyLevel level) {
            _currentDifficulty = level;
            _difficultyMultiplier = level switch {
                DifficultyLevel.Easy => 0.7f,
                DifficultyLevel.Normal => 1.0f,
                DifficultyLevel.Hard => 1.5f,
                DifficultyLevel.Nightmare => 2.0f,
                _ => 1.0f
            };
            
            GD.Print($"[EnemyDifficultyScaler] Difficulty set to {level} ({_difficultyMultiplier}x)");
        }
        
        /// <summary>
        /// 获取当前难度等级
        /// </summary>
        public DifficultyLevel GetCurrentDifficulty() {
            return _currentDifficulty;
        }
        
        /// <summary>
        /// 设置玩家等级
        /// </summary>
        public void SetPlayerLevel(int level) {
            _playerLevel = Mathf.Max(1, level);
            UpdateDifficultyMultiplier();
        }
        
        /// <summary>
        /// 更新难度乘数
        /// </summary>
        private void UpdateDifficultyMultiplier() {
            var baseMultiplier = _currentDifficulty switch {
                DifficultyLevel.Easy => 0.7f,
                DifficultyLevel.Normal => 1.0f,
                DifficultyLevel.Hard => 1.5f,
                DifficultyLevel.Nightmare => 2.0f,
                _ => 1.0f
            };
            
            var levelBonus = (_playerLevel - 1) * 0.05f;
            _difficultyMultiplier = baseMultiplier + levelBonus;
        }
        
        /// <summary>
        /// 获取难度乘数
        /// </summary>
        public float GetDifficultyMultiplier() {
            return _difficultyMultiplier;
        }
        
        public override Dictionary<string, object> ExportSaveData() {
            var data = new Dictionary<string, object>();
            data["difficulty"] = (int)_currentDifficulty;
            data["playerLevel"] = _playerLevel;
            data["multiplier"] = _difficultyMultiplier;
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data) {
            if (data == null) return;
            
            if (data.Contains("difficulty")) {
                SetDifficulty((DifficultyLevel)(int)data["difficulty"]);
            }
            
            if (data.Contains("playerLevel")) {
                SetPlayerLevel((int)data["playerLevel"]);
            }
        }
    }
}
