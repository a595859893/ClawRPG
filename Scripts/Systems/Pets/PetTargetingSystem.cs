using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物目标选择系统 - 选择攻击/治疗目标
    /// 职责：敌人优先级评估、智能目标选择、目标评分
    /// </summary>
    public class PetTargetingSystem : BaseSystem
    {
        private static PetTargetingSystem _instance;
        public static PetTargetingSystem Instance => _instance ??= new PetTargetingSystem();

        // 宠物引用
        private Pet _activePet;
        private CharacterBody2D _player;
        private PetPersonality _personality = PetPersonality.Balanced;
        
        // 区域检测
        private Area2D _detectionArea;
        
        // 信号
        public Action<Node2D> OnTargetChanged;

        public void Initialize(CharacterBody2D player)
        {
            _instance = this;
            _player = player;
            GD.Print("[PetTargetingSystem] Initialized");
        }

        public void SetActivePet(Pet pet)
        {
            _activePet = pet;
        }

        public void SetPlayer(CharacterBody2D player)
        {
            _player = player;
        }

        public void SetDetectionArea(Area2D area)
        {
            _detectionArea = area;
        }

        public void SetPersonality(PetPersonality personality)
        {
            _personality = personality;
        }

        /// <summary>
        /// 智能目标选择 - 从检测区域中选择最佳目标
        /// </summary>
        public Node2D SelectSmartTarget()
        {
            int tickId = PetDecisionSystem.NextDecisionTick(); // REQ-137: 目标选择决策节点
            if (_detectionArea == null) return null;

            var bodies = _detectionArea.GetOverlappingAreas();
            if (bodies.Count == 0) return null;
            
            Node2D bestTarget = null;
            float bestScore = float.MaxValue;
            
            foreach (var body in bodies)
            {
                var enemy = body?.GetParent();
                if (enemy == null || !enemy.IsInGroup("enemy")) continue;
                
                float score = CalculateTargetScore(enemy as Node2D);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = enemy as Node2D;
                }
            }
            
            // REQ-137: 记录目标选择决策
            if (PetReplayTraceSystem.Instance != null && bestTarget != null)
            {
                float timestamp = (float)Time.GetTicksMsec() / 1000f;
                float dist = GlobalPosition.DistanceTo(bestTarget.GlobalPosition);
                string reason = $"评分:{bestScore:F0} 性格:{_personality}";
                var record = PetDecisionRecord.CreateTargetSelection(tickId, timestamp, bestTarget, dist, reason);
                PetReplayTraceSystem.Instance.RecordDecision(record);
            }

            return bestTarget;
        }

        /// <summary>
        /// 计算目标评分（越低越好）
        /// 考虑因素：距离、血量、威胁等级、宠物性格
        /// </summary>
        public float CalculateTargetScore(Node2D enemy)
        {
            if (enemy == null) return float.MaxValue;
            
            float score = 0f;
            
            // 距离权重 (越近越好)
            float dist = GlobalPosition.DistanceTo(enemy.GlobalPosition);
            score += dist * 0.5f;
            
            // 血量权重 (低血量优先)
            if (enemy.HasMethod("GetCurrentHealth"))
            {
                int currentHp = (int)enemy.Call("GetCurrentHealth");
                int maxHp = (int)enemy.Call("GetMaxHealth");
                float hpPercent = maxHp > 0 ? (float)currentHp / maxHp : 1f;
                score += hpPercent * 500f; // 低血量降低分数
            }
            
            // 性格影响
            switch (_personality)
            {
                case PetPersonality.Aggressive:
                    // 优先攻击玩家正在攻击的敌人
                    break;
                case PetPersonality.Cautious:
                    score += dist * 0.3f; // 更偏好远程
                    break;
                case PetPersonality.Defensive:
                    // 优先攻击靠近玩家的敌人
                    if (_player != null)
                    {
                        float enemyToPlayer = enemy.GlobalPosition.DistanceTo(_player.GlobalPosition);
                        score -= enemyToPlayer * 0.2f; // 靠近玩家的敌人分数更低
                    }
                    break;
            }
            
            return score;
        }

        /// <summary>
        /// 获取范围内所有敌人
        /// </summary>
        public List<Node2D> GetEnemiesInRange(float range)
        {
            var enemies = new List<Node2D>();
            
            if (_detectionArea == null) return enemies;
            
            var bodies = _detectionArea.GetOverlappingAreas();
            foreach (var body in bodies)
            {
                var enemy = body?.GetParent() as Node2D;
                if (enemy == null || !enemy.IsInGroup("enemy")) continue;
                
                float dist = GlobalPosition.DistanceTo(enemy.GlobalPosition);
                if (dist <= range)
                {
                    enemies.Add(enemy);
                }
            }
            
            return enemies;
        }

        /// <summary>
        /// 获取最近的目标
        /// </summary>
        public Node2D GetNearestTarget()
        {
            if (_detectionArea == null) return null;
            
            Node2D nearest = null;
            float nearestDist = float.MaxValue;
            
            var bodies = _detectionArea.GetOverlappingAreas();
            foreach (var body in bodies)
            {
                var enemy = body?.GetParent() as Node2D;
                if (enemy == null || !enemy.IsInGroup("enemy")) continue;
                
                float dist = GlobalPosition.DistanceTo(enemy.GlobalPosition);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = enemy;
                }
            }
            
            return nearest;
        }

        /// <summary>
        /// 获取最低血量目标
        /// </summary>
        public Node2D GetLowestHealthTarget()
        {
            if (_detectionArea == null) return null;
            
            Node2D lowestHp = null;
            float lowestPercent = float.MaxValue;
            
            var bodies = _detectionArea.GetOverlappingAreas();
            foreach (var body in bodies)
            {
                var enemy = body?.GetParent() as Node2D;
                if (enemy == null || !enemy.IsInGroup("enemy")) continue;
                if (!enemy.HasMethod("GetCurrentHealth")) continue;
                
                int currentHp = (int)enemy.Call("GetCurrentHealth");
                int maxHp = (int)enemy.Call("GetMaxHealth");
                float hpPercent = maxHp > 0 ? (float)currentHp / maxHp : 1f;
                
                if (hpPercent < lowestPercent)
                {
                    lowestPercent = hpPercent;
                    lowestHp = enemy;
                }
            }
            
            return lowestHp;
        }

        /// <summary>
        /// 检查目标是否有效
        /// </summary>
        public bool IsValidTarget(Node2D target)
        {
            if (target == null) return false;
            if (!target.IsInGroup("enemy")) return false;
            
            // 检查目标是否在检测区域内
            if (_detectionArea != null)
            {
                var bodies = _detectionArea.GetOverlappingAreas();
                foreach (var body in bodies)
                {
                    if (body?.GetParent() == target)
                        return true;
                }
                return false;
            }
            
            return true;
        }

        public override Dictionary ExportSaveData()
        {
            return new Dictionary();
        }

        public override void ImportSaveData(Dictionary data)
        {
            // 无持久化数据
        }
    }
}
