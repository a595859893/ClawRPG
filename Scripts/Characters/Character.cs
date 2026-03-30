using Godot;

namespace ClawRPG.Scripts.Characters
{
    /// <summary>
    /// Base class for all character entities (player, enemy, boss).
    /// TODO: This is a stub - expand with shared character properties.
    /// </summary>
    public class Character : Node2D
    {
        public float CurrentHealth { get; set; }
        public float MaxHealth { get; set; }
        public float Attack { get; set; }
        public float Defense { get; set; }
    }
}
