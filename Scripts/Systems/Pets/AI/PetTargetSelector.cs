using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Data;

namespace ClawRPG.Systems.Pets.AI {
    /// <summary>
    /// Pet Target Selector - Selects optimal targets for pet combat
    /// Part of PetCombatAI refactoring
    /// </summary>
    public partial class PetTargetSelector : BaseSystem
    {
        /// <summary>
        /// Target selection strategy
        /// </summary>
        public enum TargetStrategy
        {
            Closest,        // Nearest enemy
            LowestHealth,   // Lowest HP enemy
            MostThreat,     // Most threatening enemy
            PlayerTarget,   // Same target as player
            Random          // Random selection
        }
        
        private TargetStrategy _currentStrategy = TargetStrategy.Closest;
        private PetPersonality _personality = PetPersonality.Balanced;
        
        /// <summary>
        /// Set target selection strategy
        /// </summary>
        public void SetStrategy(TargetStrategy strategy)
        {
            _currentStrategy = strategy;
        }
        
        /// <summary>
        /// Set personality (affects default strategy)
        /// </summary>
        public void SetPersonality(PetPersonality personality)
        {
            _personality = personality;
            
            // Adjust strategy based on personality
            _currentStrategy = personality switch
            {
                PetPersonality.Aggressive => TargetStrategy.Closest,
                PetPersonality.Cautious => TargetStrategy.LowestHealth,
                PetPersonality.Defensive => TargetStrategy.MostThreat,
                _ => TargetStrategy.Closest
            };
        }
        
        /// <summary>
        /// Select best target from available enemies
        /// </summary>
        public Node2D SelectTarget(
            List<Node2D> enemies, 
            Vector2 petPosition, 
            CharacterBody2D player = null,
            float healthPercent = 1.0f)
        {
            if (enemies == null || enemies.Count == 0) return null;
            
            // Filter out invalid enemies
            var validEnemies = new List<Node2D>();
            foreach (var enemy in enemies)
            {
                if (enemy != null && IsInstanceValid(enemy))
                {
                    validEnemies.Add(enemy);
                }
            }
            
            if (validEnemies.Count == 0) return null;
            
            // If only one enemy, return it
            if (validEnemies.Count == 1) return validEnemies[0];
            
            // Select based on strategy
            return _currentStrategy switch
            {
                TargetStrategy.Closest => SelectClosest(validEnemies, petPosition),
                TargetStrategy.LowestHealth => SelectLowestHealth(validEnemies),
                TargetStrategy.MostThreat => SelectMostThreatening(validEnemies, petPosition, player),
                TargetStrategy.PlayerTarget => SelectPlayerTarget(validEnemies, player),
                TargetStrategy.Random => SelectRandom(validEnemies),
                _ => SelectClosest(validEnemies, petPosition)
            };
        }
        
        /// <summary>
        /// Select closest enemy
        /// </summary>
        private Node2D SelectClosest(List<Node2D> enemies, Vector2 petPosition)
        {
            Node2D closest = null;
            float minDist = float.MaxValue;
            
            foreach (var enemy in enemies)
            {
                float dist = petPosition.DistanceTo(enemy.GlobalPosition);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = enemy;
                }
            }
            
            return closest;
        }
        
        /// <summary>
        /// Select enemy with lowest health
        /// </summary>
        private Node2D SelectLowestHealth(List<Node2D> enemies)
        {
            Node2D lowest = null;
            float lowestHpPercent = float.MaxValue;
            
            foreach (var enemy in enemies)
            {
                float hpPercent = GetEnemyHealthPercent(enemy);
                if (hpPercent < lowestHpPercent)
                {
                    lowestHpPercent = hpPercent;
                    lowest = enemy;
                }
            }
            
            return lowest ?? enemies[0];
        }
        
        /// <summary>
        /// Select most threatening enemy
        /// </summary>
        private Node2D SelectMostThreatening(List<Node2D> enemies, Vector2 petPosition, CharacterBody2D player)
        {
            Node2D mostThreatening = null;
            float highestThreat = float.MinValue;
            
            foreach (var enemy in enemies)
            {
                float threat = CalculateThreat(enemy, petPosition, player);
                if (threat > highestThreat)
                {
                    highestThreat = threat;
                    mostThreatening = enemy;
                }
            }
            
            return mostThreatening ?? enemies[0];
        }
        
        /// <summary>
        /// Select same target as player
        /// </summary>
        private Node2D SelectPlayerTarget(List<Node2D> enemies, CharacterBody2D player)
        {
            if (player == null) return SelectClosest(enemies, Vector2.Zero);
            
            // Try to find player's current target
            // This depends on how the player system exposes their target
            // For now, return closest to player
            return SelectClosest(enemies, player.GlobalPosition);
        }
        
        /// <summary>
        /// Select random enemy
        /// </summary>
        private Node2D SelectRandom(List<Node2D> enemies)
        {
            int index = (int)(GD.Randf() * enemies.Count);
            return enemies[index];
        }
        
        /// <summary>
        /// Calculate threat level of enemy
        /// </summary>
        private float CalculateThreat(Node2D enemy, Vector2 petPosition, CharacterBody2D player)
        {
            float threat = 0f;
            
            // Distance to pet (closer = more threatening)
            float distToPet = petPosition.DistanceTo(enemy.GlobalPosition);
            threat -= distToPet * 0.1f;
            
            // Distance to player (closer to player = more threatening)
            if (player != null)
            {
                float distToPlayer = player.GlobalPosition.DistanceTo(enemy.GlobalPosition);
                threat -= distToPlayer * 0.2f; // Higher weight for player safety
            }
            
            // Enemy type/threat level (could be extended)
            // For now, assume all enemies have equal base threat
            
            // Health percentage (lower HP = less threatening)
            float hpPercent = GetEnemyHealthPercent(enemy);
            threat += hpPercent * 50f;
            
            return threat;
        }
        
        /// <summary>
        /// Get enemy health percentage
        /// </summary>
        private float GetEnemyHealthPercent(Node2D enemy)
        {
            if (enemy == null) return 1.0f;
            
            // Try to get health from enemy
            if (enemy.HasMethod("GetCurrentHealth") && enemy.HasMethod("GetMaxHealth"))
            {
                int currentHp = (int)enemy.Call("GetCurrentHealth");
                int maxHp = (int)enemy.Call("GetMaxHealth");
                if (maxHp > 0)
                {
                    return (float)currentHp / maxHp;
                }
            }
            
            return 1.0f;
        }
        
        /// <summary>
        /// Calculate target score (for AI decision making)
        /// Lower score = better target
        /// </summary>
        public float CalculateTargetScore(Node2D enemy, Vector2 petPosition, CharacterBody2D player)
        {
            float score = 0f;
            
            // Distance score (closer is better)
            float dist = petPosition.DistanceTo(enemy.GlobalPosition);
            score += dist * 0.5f;
            
            // Health score (low HP is better)
            float hpPercent = GetEnemyHealthPercent(enemy);
            score += hpPercent * 500f;
            
            // Personality adjustment
            switch (_personality)
            {
                case PetPersonality.Aggressive:
                    // Prefers close combat
                    break;
                case PetPersonality.Cautious:
                    score += dist * 0.3f; // Prefers ranged
                    break;
                case PetPersonality.Defensive:
                    // Prefers enemies threatening player
                    if (player != null)
                    {
                        float enemyToPlayer = enemy.GlobalPosition.DistanceTo(player.GlobalPosition);
                        score -= enemyToPlayer * 0.2f;
                    }
                    break;
            }
            
            return score;
        }
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["strategy"] = (int)_currentStrategy;
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data.ContainsKey("strategy")) {
                _currentStrategy = (TargetStrategy)(int)data["strategy"];
            }
        }
    }
}
