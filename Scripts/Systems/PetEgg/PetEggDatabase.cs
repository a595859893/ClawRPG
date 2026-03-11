using Godot;
using System;
using System.Collections.Generic;

public class PetEggDatabase
{
    private static Dictionary<string, PetEggData> eggs = new Dictionary<string, PetEggData>();
    
    public static void Initialize()
    {
        if (eggs.Count > 0) return;
        
        // Common Eggs
        AddEgg(new PetEggData {
            eggId = "egg_wolf_basic",
            eggName = "Wolf Egg",
            description = "A common wolf egg found in the forest.",
            petType = "wolf",
            rarity = 1,
            hatchTimeSeconds = 300,
            goldCost = 100,
            possiblePetIds = new float[] { 1f, 2f, 3f },
            possiblePetWeights = new float[] { 60f, 30f, 10f }
        });
        
        AddEgg(new PetEggData {
            eggId = "egg_bear_basic",
            eggName = "Bear Egg",
            description = "A common bear egg from the mountains.",
            petType = "bear",
            rarity = 1,
            hatchTimeSeconds = 360,
            goldCost = 120,
            possiblePetIds = new float[] { 4f, 5f, 6f },
            possiblePetWeights = new float[] { 60f, 30f, 10f }
        });
        
        AddEgg(new PetEggData {
            eggId = "egg_eagle_basic",
            eggName = "Eagle Egg",
            description = "A common eagle egg from the cliffs.",
            petType = "eagle",
            rarity = 1,
            hatchTimeSeconds = 300,
            goldCost = 100,
            possiblePetIds = new float[] { 7f, 8f, 9f },
            possiblePetWeights = new float[] { 60f, 30f, 10f }
        });
        
        // Uncommon Eggs
        AddEgg(new PetEggData {
            eggId = "egg_fox_uncommon",
            eggName = "Fox Egg",
            description = "An uncommon fox egg with mysterious aura.",
            petType = "fox",
            rarity = 2,
            hatchTimeSeconds = 480,
            goldCost = 250,
            possiblePetIds = new float[] { 10f, 11f, 12f, 13f },
            possiblePetWeights = new float[] { 50f, 30f, 15f, 5f }
        });
        
        AddEgg(new PetEggData {
            eggId = "egg_horse_uncommon",
            eggName = "Horse Egg",
            description = "An uncommon horse egg with golden spots.",
            petType = "horse",
            rarity = 2,
            hatchTimeSeconds = 420,
            goldCost = 200,
            possiblePetIds = new float[] { 14f, 15f, 16f, 17f },
            possiblePetWeights = new float[] { 50f, 30f, 15f, 5f }
        });
        
        // Rare Eggs
        AddEgg(new PetEggData {
            eggId = "egg_dragon_rare",
            eggName = "Dragon Egg",
            description = "A rare dragon egg radiating power.",
            petType = "dragon",
            rarity = 3,
            hatchTimeSeconds = 600,
            goldCost = 500,
            possiblePetIds = new float[] { 18f, 19f, 20f, 21f, 22f },
            possiblePetWeights = new float[] { 40f, 30f, 20f, 8f, 2f }
        });
        
        AddEgg(new PetEggData {
            eggId = "egg_phoenix_rare",
            eggName = "Phoenix Egg",
            description = "A rare phoenix egg glowing with flames.",
            petType = "phoenix",
            rarity = 3,
            hatchTimeSeconds = 600,
            goldCost = 550,
            possiblePetIds = new float[] { 23f, 24f, 25f, 26f, 27f },
            possiblePetWeights = new float[] { 40f, 30f, 20f, 8f, 2f }
        });
        
        // Epic Eggs
        AddEgg(new PetEggData {
            eggId = "egg_griffin_epic",
            eggName = "Griffin Egg",
            description = "An epic griffin egg with majestic presence.",
            petType = "griffin",
            rarity = 4,
            hatchTimeSeconds = 900,
            goldCost = 1000,
            possiblePetIds = new float[] { 28f, 29f, 30f },
            possiblePetWeights = new float[] { 50f, 35f, 15f }
        });
        
        AddEgg(new PetEggData {
            eggId = "egg_unicorn_epic",
            eggName = "Unicorn Egg",
            description = "An epic unicorn egg shimmering with magic.",
            petType = "unicorn",
            rarity = 4,
            hatchTimeSeconds = 900,
            goldCost = 1200,
            possiblePetIds = new float[] { 31f, 32f, 33f },
            possiblePetWeights = new float[] { 50f, 35f, 15f }
        });
        
        // Legendary Eggs
        AddEgg(new PetEggData {
            eggId = "egg_celestial_legendary",
            eggName = "Celestial Egg",
            description = "A legendary egg from the heavens.",
            petType = "celestial",
            rarity = 5,
            hatchTimeSeconds = 1800,
            goldCost = 3000,
            possiblePetIds = new float[] { 34f, 35f, 36f },
            possiblePetWeights = new float[] { 40f, 40f, 20f }
        });
        
        AddEgg(new PetEggData {
            eggId = "egg_shadow_legendary",
            eggName = "Shadow Egg",
            description = "A legendary egg from the void.",
            petType = "shadow",
            rarity = 5,
            hatchTimeSeconds = 1800,
            goldCost = 3500,
            possiblePetIds = new float[] { 37f, 38f, 39f },
            possiblePetWeights = new float[] { 40f, 40f, 20f }
        });
        
        AddEgg(new PetEggData {
            eggId = "egg_ancient_legendary",
            eggName = "Ancient Egg",
            description = "A legendary egg from ancient times.",
            petType = "ancient",
            rarity = 5,
            hatchTimeSeconds = 2400,
            goldCost = 5000,
            possiblePetIds = new float[] { 40f, 41f, 42f },
            possiblePetWeights = new float[] { 35f, 40f, 25f }
        });
    }
    
    private static void AddEgg(PetEggData egg)
    {
        eggs[egg.eggId] = egg;
    }
    
    public static PetEggData GetEgg(string eggId)
    {
        if (eggs.ContainsKey(eggId))
            return eggs[eggId];
        return null;
    }
    
    public static List<PetEggData> GetAllEggs()
    {
        return new List<PetEggData>(eggs.Values);
    }
    
    public static List<PetEggData> GetEggsByRarity(int rarity)
    {
        List<PetEggData> result = new List<PetEggData>();
        foreach (var egg in eggs.Values)
        {
            if (egg.rarity == rarity)
                result.Add(egg);
        }
        return result;
    }
    
    public static Dictionary<string, PetEggData> GetEggs()
    {
        return eggs;
    }
}
