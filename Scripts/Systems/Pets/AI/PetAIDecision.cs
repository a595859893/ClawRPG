using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Pets.AI
{
    /// <summary>
    /// 宠物 AI 决策 - 决定宠物行为
    /// </summary>
    public partial class PetAIDecision : BaseSystem
    {
        /// <summary>
        /// AI 决策结果
        /// </summary>
        public class Decision
        {
            public PetAIState TargetState { get; set; }
            public Vector2 TargetPosition { get; set; }
            public Node2D TargetEnemy { get; set; }
            public bool ShouldAttack { get; set; }
            public bool ShouldSupport { get; set; }
            public float Confidence { get; set; }  // 决策置信度
        }
        
        /// <summary>
        /// 宠物性格类型
        /// </summary>
        public enum PersonalityType
        {
            Aggressive,    // 激进型 - 优先攻击
            Defensive,     // 防守型 - 优先保护
            Balanced,      // 平衡型 - 攻防兼备
            Cautious,     // 谨慎型 - 保持距离
            Supportive    // 支援型 - 优先辅助
        }
        
        private PersonalityType _personality = PersonalityType.Balanced;
        
        public override void _Ready()
        {
            base._Ready();
        }
        
        /// <summary>
        /// 设置宠物性格
        /// </summary>
        public void SetPersonality(PersonalityType personality)
        {
            _personality = personality;
        }
        
        /// <summary>
        /// 根据宠物类型确定性格
        /// </summary>
        public void DeterminePersonalityFromPetType(PetType type)
        {
            switch (type)
            {
                case PetType.Attack:
                    _personality = PersonalityType.Aggressive;
                    break;
                case PetType.Defense:
                    _personality = PersonalityType.Defensive;
                    break;
                case PetType.Magic:
                    _personality = PersonalityType.Supportive;
                    break;
                case PetType.Support:
                    _personality = PersonalityType.Supportive;
                    break;
                default:
                    _personality = PersonalityType.Balanced;
                    break;
            }
        }
        
        /// <summary>
        /// 做决策 - 基于当前状态选择最佳行为
        /// </summary>
        public Decision MakeDecision(PetAIState currentState, Vector2 playerPos, Vector2 petPos, 
                                    List<Node2D> nearbyEnemies, float distanceToPlayer, 
                                    float healthPercent, bool playerInCombat)
        {
            var decision = new Decision();
            
            // 检查是否需要保护玩家
            if (playerInCombat && _personality == PersonalityType.Defensive)
            {
                decision.TargetState = PetAIState.Supporting;
                decision.ShouldSupport = true;
                decision.Confidence = 0.9f;
                return decision;
            }
            
            // 检查是否需要战斗
            if (nearbyEnemies.Count > 0)
            {
                // 根据性格选择目标
                var target = SelectTarget(nearbyEnemies);
                decision.TargetEnemy = target;
                
                if (target != null)
                {
                    float distToEnemy = petPos.DistanceTo(target.GlobalPosition);
                    
                    // 根据性格决定距离
                    float preferredDist = GetPreferredDistance(target);
                    
                    if (distToEnemy < preferredDist * 0.7f)
                    {
                        decision.TargetState = PetAIState.Retreating;
                        decision.ShouldAttack = true;
                    }
                    else if (distToEnemy > preferredDist * 1.3f)
                    {
                        decision.TargetState = PetAIState.Engaging;
                        decision.ShouldAttack = true;
                    }
                    else
                    {
                        decision.TargetState = PetAIState.Attacking;
                        decision.ShouldAttack = true;
                    }
                    
                    decision.TargetPosition = target.GlobalPosition;
                    decision.Confidence = 0.8f;
                    return decision;
                }
            }
            
            // 跟随玩家
            decision.TargetState = PetAIState.Following;
            decision.TargetPosition = playerPos;
            decision.ShouldAttack = false;
            decision.Confidence = 0.6f;
            
            return decision;
        }
        
        /// <summary>
        /// 选择目标
        /// </summary>
        private Node2D SelectTarget(List<Node2D> enemies)
        {
            if (enemies.Count == 0) return null;
            
            switch (_personality)
            {
                case PersonalityType.Aggressive:
                    // 优先选择最近的敌人
                    return GetClosestEnemy(enemies);
                    
                case PersonalityType.Cautious:
                    // 优先选择最弱的敌人
                    return GetWeakestEnemy(enemies);
                    
                case PersonalityType.Defensive:
                    // 优先选择威胁最大的敌人
                    return GetMostThreateningEnemy(enemies);
                    
                default:
                    return GetClosestEnemy(enemies);
            }
        }
        
        /// <summary>
        /// 获取最近的敌人
        /// </summary>
        private Node2D GetClosestEnemy(List<Node2D> enemies)
        {
            Node2D closest = null;
            float minDist = float.MaxValue;
            
            foreach (var enemy in enemies)
            {
                // 需要从 PetCombatAI 获取宠物位置，这里简化处理
                float dist = enemy.GlobalPosition.Length();  // 简化
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = enemy;
                }
            }
            
            return closest;
        }
        
        /// <summary>
        /// 获取最弱的敌人
        /// </summary>
        private Node2D GetWeakestEnemy(List<Node2D> enemies)
        {
            // 简化实现，实际需要获取敌人血量
            return enemies[0];
        }
        
        /// <summary>
        /// 获取威胁最大的敌人
        /// </summary>
        private Node2D GetMostThreateningEnemy(List<Node2D> enemies)
        {
            // 简化实现，实际需要判断敌人类型
            return enemies[0];
        }
        
        /// <summary>
        /// 获取偏好的战斗距离
        /// </summary>
        private float GetPreferredDistance(Node2D target)
        {
            switch (_personality)
            {
                case PersonalityType.Aggressive:
                    return 80f;   // 靠近打
                case PersonalityType.Cautious:
                    return 200f;  // 保持距离
                case PersonalityType.Defensive:
                    return 100f;  // 保护玩家
                default:
                    return 120f;
            }
        }
        
        /// <summary>
        /// 评估战斗形势
        /// </summary>
        public float EvaluateBattleSituation(List<Node2D> enemies, float petHealth)
        {
            float score = 0f;
            
            // 敌人数量
            score -= enemies.Count * 10f;
            
            // 宠物血量
            score += petHealth * 20f;
            
            // 根据性格调整
            switch (_personality)
            {
                case PersonalityType.Aggressive:
                    score += 10f;  // 激进型更有信心
                    break;
                case PersonalityType.Cautious:
                    score -= 10f;  // 谨慎型更保守
                    break;
            }
            
            return score;
        }
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            // 加载数据
        }
    }
}
