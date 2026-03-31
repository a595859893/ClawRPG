using System;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// Represents an instance of a pet in a battle context.
    /// Used by PetBattleArenaUI for player vs enemy pet combat.
    /// </summary>
    public class PetInstance
    {
        public string Name { get; set; } = "Unknown";
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Level { get; set; } = 1;

        public PetInstance(string name = "Unknown")
        {
            Name = name;
        }
    }
}
