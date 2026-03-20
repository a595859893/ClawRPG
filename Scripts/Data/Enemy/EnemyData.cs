using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Data.Enemy
{
    /// <summary>
    /// 敌人类型数据 - 包含敌人的所有配置信息
    /// </summary>
    public class EnemyType
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int MaxHealth { get; set; }
        public float Speed { get; set; }
        public float Damage { get; set; }
        
        public string Description { get; set; }
        public float AttackRange { get; set; }
        public float AttackCooldown { get; set; }
        public float ChaseRange { get; set; }
        public float DetectionRange { get; set; }
        public int ExperienceReward { get; set; }
        public int GoldReward { get; set; }
        public Dictionary<string, float> DropTable { get; set; }
        public Dictionary<string, float> StatusEffectVulnerability { get; set; }
        public Color SpriteModulate { get; set; }
        
        public EnemyType()
        {
            DropTable = new Dictionary<string, float>();
            StatusEffectVulnerability = new Dictionary<string, float>();
            SpriteModulate = Colors.White;
        }
        
        public EnemyType(string id, string name, int maxHealth, float speed, float damage) : this()
        {
            Id = id;
            Name = name;
            MaxHealth = maxHealth;
            Speed = speed;
            Damage = damage;
        }
    }
}
