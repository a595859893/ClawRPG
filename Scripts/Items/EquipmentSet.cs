using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Items {
    /// <summary>
    /// Equipment set bonus effect
    /// </summary>
    public class SetBonusEffect
    {
        public int RequiredPieceCount { get; set; }  // Required pieces to activate
        public string BonusName { get; set; }       // Bonus name
        public string Description { get; set; }      // Bonus description
        
        // Bonus stats
        public float DamageBonus { get; set; }
        public float DefenseBonus { get; set; }
        public float HealthBonus { get; set; }
        public float ManaBonus { get; set; }
        public float CriticalChanceBonus { get; set; }
        public float CriticalDamageBonus { get; set; }
        public float AttackSpeedBonus { get; set; }
        public float MoveSpeedBonus { get; set; }
        public float FireResistance { get; set; }
        public float IceResistance { get; set; }
        public float LightningResistance { get; set; }
        public float PoisonResistance { get; set; }
        public float DarkResistance { get; set; }
        public float HolyResistance { get; set; }
        
        public SetBonusEffect()
        {
        }
        
        public SetBonusEffect(int pieces, string name, string desc)
        {
            RequiredPieceCount = pieces;
            BonusName = name;
            Description = desc;
        }
    }
    
    /// <summary>
    /// Equipment set data
    /// </summary>
    public class EquipmentSet
    {
        public int SetId { get; set; }
        public string SetName { get; set; }
        public string Description { get; set; }
        public string SetNameCN { get; set; }  // Chinese name
        public List<int> EquipmentIds { get; set; }  // List of equipment IDs in this set
        public List<SetBonusEffect> Bonuses { get; set; }  // List of set bonuses
        
        public EquipmentSet()
        {
            EquipmentIds = new List<int>();
            Bonuses = new List<SetBonusEffect>();
        }
    }
    
    /// <summary>
    /// Player's active set bonus
    /// </summary>
    public class ActiveSetBonus
    {
        public EquipmentSet Set { get; set; }
        public int EquippedPieces { get; set; }
        public SetBonusEffect ActiveBonus { get; set; }
        public bool IsActive => ActiveBonus != null;
        
        public ActiveSetBonus()
        {
        }
        
        public ActiveSetBonus(EquipmentSet set, int pieces)
        {
            Set = set;
            EquippedPieces = pieces;
            CalculateActiveBonus();
        }
        
        public void CalculateActiveBonus()
        {
            ActiveBonus = null;
            if (Set == null || Set.Bonuses == null) return;
            
            // Find the highest bonus that can be activated
            foreach (var bonus in Set.Bonuses)
            {
                if (EquippedPieces >= bonus.RequiredPieceCount)
                {
                    ActiveBonus = bonus;
                }
            }
        }
    }
}
