using Godot;
using System;
using System.Collections.Generic;

public class PetBreedingDatabase : Resource
{
    [Export] public Dictionary<string, PetBreedConfig> BreedConfigs = new Dictionary<string, PetBreedConfig>();

    public PetBreedingDatabase()
    {
        // Fire Dragon + Ice Dragon = Volcanic Dragon
        BreedConfigs["FireDragon_IceDragon"] = new PetBreedConfig
        {
            ResultType = "VolcanicDragon",
            ResultName = "Volcanic Dragon",
            Description = "A dragon born from the fusion of fire and ice elements",
            BaseSuccessRate = 0.25f,
            RarityWeights = new Dictionary<int, float>
            {
                { 1, 0.50f }, // Common
                { 2, 0.30f }, // Uncommon
                { 3, 0.15f }, // Rare
                { 4, 0.04f }, // Epic
                { 5, 0.01f }  // Legendary
            }
        };

        // Wolf + Fox = Fenris
        BreedConfigs["Wolf_Fox"] = new PetBreedConfig
        {
            ResultType = "Fenris",
            ResultName = "Fenris Wolf",
            Description = "A cunning predator combining wolf and fox traits",
            BaseSuccessRate = 0.35f,
            RarityWeights = new Dictionary<int, float>
            {
                { 1, 0.45f },
                { 2, 0.35f },
                { 3, 0.15f },
                { 4, 0.04f },
                { 5, 0.01f }
            }
        };

        // Phoenix + Thunderbird = Solar Phoenix
        BreedConfigs["Phoenix_Thunderbird"] = new PetBreedConfig
        {
            ResultType = "SolarPhoenix",
            ResultName = "Solar Phoenix",
            Description = "A magnificent bird of light and storms",
            BaseSuccessRate = 0.20f,
            RarityWeights = new Dictionary<int, float>
            {
                { 1, 0.40f },
                { 2, 0.35f },
                { 3, 0.18f },
                { 5, 0.07f } // Higher legendary chance
            }
        };

        // Slime + Jelly = Mega Slime
        BreedConfigs["Slime_Jelly"] = new PetBreedConfig
        {
            ResultType = "MegaSlime",
            ResultName = "Mega Slime",
            Description = "A giant slime with enhanced properties",
            BaseSuccessRate = 0.50f,
            RarityWeights = new Dictionary<int, float>
            {
                { 1, 0.40f },
                { 2, 0.40f },
                { 3, 0.15f },
                { 4, 0.04f },
                { 5, 0.01f }
            }
        };

        // Ghost + Skeleton = Lich Pet
        BreedConfigs["Ghost_Skeleton"] = new PetBreedConfig
        {
            ResultType = "LichPet",
            ResultName = "Lich Familiar",
            Description = "An undead spirit bound to serve",
            BaseSuccessRate = 0.30f,
            RarityWeights = new Dictionary<int, float>
            {
                { 1, 0.45f },
                { 2, 0.30f },
                { 3, 0.18f },
                { 4, 0.06f },
                { 5, 0.01f }
            }
        };

        // Bear + Turtle = Armored Bear
        BreedConfigs["Bear_Turtle"] = new PetBreedConfig
        {
            ResultType = "ArmoredBear",
            ResultName = "Armored Bear",
            Description = "A heavily protected bear with turtle shell",
            BaseSuccessRate = 0.40f,
            RarityWeights = new Dictionary<int, float>
            {
                { 1, 0.40f },
                { 2, 0.35f },
                { 3, 0.20f },
                { 4, 0.04f },
                { 5, 0.01f }
            }
        };

        // Owl + Eagle = Sky Guardian
        BreedConfigs["Owl_Eagle"] = new PetBreedConfig
        {
            ResultType = "SkyGuardian",
            ResultName = "Sky Guardian",
            Description = "A wise and powerful aerial predator",
            BaseSuccessRate = 0.35f,
            RarityWeights = new Dictionary<int, float>
            {
                { 1, 0.45f },
                { 2, 0.30f },
                { 3, 0.18f },
                { 4, 0.06f },
                { 5, 0.01f }
            }
        };

        // Fish + Serpent = Sea Dragon
        BreedConfigs["Fish_Serpent"] = new PetBreedConfig
        {
            ResultType = "SeaDragon",
            ResultName = "Sea Dragon",
            Description = "A dragon of the oceanic depths",
            BaseSuccessRate = 0.20f,
            RarityWeights = new Dictionary<int, float>
            {
                { 1, 0.40f },
                { 2, 0.35f },
                { 3, 0.18f },
                { 4, 0.05f },
                { 5, 0.02f }
            }
        };

        // Cat + Tiger = Sabertooth
        BreedConfigs["Cat_Tiger"] = new PetBreedConfig
        {
            ResultType = "Sabertooth",
            ResultName = "Sabertooth",
            Description = "A powerful feline predator",
            BaseSuccessRate = 0.35f,
            RarityWeights = new Dictionary<int, float>
            {
                { 1, 0.45f },
                { 2, 0.30f },
                { 3, 0.18f },
                { 4, 0.06f },
                { 5, 0.01f }
            }
        };

        // Butterfly + Beetle = Crystal Insect
        BreedConfigs["Butterfly_Beetle"] = new PetBreedConfig
        {
            ResultType = "CrystalInsect",
            ResultName = "Crystal Insect",
            Description = "A beautiful insect with crystalline wings",
            BaseSuccessRate = 0.45f,
            RarityWeights = new Dictionary<int, float>
            {
                { 1, 0.35f },
                { 2, 0.40f },
                { 3, 0.20f },
                { 4, 0.04f },
                { 5, 0.01f }
            }
        };

        // Unicorn + Pegasus = Celestial Steed
        BreedConfigs["Unicorn_Pegasus"] = new PetBreedConfig
        {
            ResultType = "CelestialSteed",
            ResultName = "Celestial Steed",
            Description = "A divine winged horse of pure light",
            BaseSuccessRate = 0.15f,
            RarityWeights = new Dictionary<int, float>
            {
                { 1, 0.35f },
                { 2, 0.35f },
                { 3, 0.20f },
                { 4, 0.08f },
                { 5, 0.02f }
            }
        };

        // Elemental + Elemental = Pure Elemental (same type bonus)
        BreedConfigs["Elemental_Elemental"] = new PetBreedConfig
        {
            ResultType = "PureElemental",
            ResultName = "Pure Elemental",
            Description = "A concentrated form of elemental energy",
            BaseSuccessRate = 0.40f,
            RarityWeights = new Dictionary<int, float>
            {
                { 1, 0.30f },
                { 2, 0.35f },
                { 3, 0.25f },
                { 4, 0.08f },
                { 5, 0.02f }
            }
        };
    }
}

public class PetBreedConfig
{
    public string ResultType { get; set; }
    public string ResultName { get; set; }
    public string Description { get; set; }
    public float BaseSuccessRate { get; set; }
    public Dictionary<int, float> RarityWeights { get; set; }
}
