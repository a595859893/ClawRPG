using System;

namespace ClawRPG.Scripts.Data
{
    /// <summary>
    /// Represents a pet's personality type, influencing decision-making and behavior.
    /// </summary>
    public enum PetPersonality
    {
        Balanced,    // Default, balanced decision-making
        Aggressive,  // Prioritizes attacking, takes risks
        Cautious,    // Prioritizes self-preservation, avoids damage
        Defensive    // Prioritizes protecting owner, defensive positioning
    }
}
