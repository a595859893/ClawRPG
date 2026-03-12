using Godot;
using System;
using System.Collections.Generic;

public class PetSynthesisDatabase
{
    private static PetSynthesisDatabase _instance;
    public static PetSynthesisDatabase Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PetSynthesisDatabase();
            return _instance;
        }
    }
    
    // Synthesis recipes
    public List<PetSynthesisRecipe> Recipes = new List<PetSynthesisRecipe>();
    
    // Elemental combinations
    public Dictionary<string, List<string>> ElementalCombinations = new Dictionary<string, List<string>>();
    
    // Rarity weights for synthesis
    public Dictionary<SynthesisResult, float> RarityWeights = new Dictionary<SynthesisResult, float>();
    
    public PetSynthesisDatabase()
    {
        InitializeRecipes();
        InitializeElementalCombinations();
        InitializeRarityWeights();
    }
    
    private void InitializeRecipes()
    {
        // Elemental Fusion Recipes
        Recipes.Add(new PetSynthesisRecipe
        {
            Id = "fire_fire",
            Name = "Inferno Wolf",
            Description = "Combine two fire pets into an Inferno Wolf",
            Pet1Type = "FireWolf",
            Pet2Type = "FireWolf",
            ResultPetType = "InfernoWolf",
            ResultRarity = SynthesisResult.Rare,
            RequiredLevel = 10,
            GoldCost = 1000,
            SuccessRate = 0.5f,
            SynthesisType = SynthesisType.ElementalFusion
        });
        
        Recipes.Add(new PetSynthesisRecipe
        {
            Id = "fire_water",
            Name = "Steam Dragon",
            Description = "Combine fire and water pets into a Steam Dragon",
            Pet1Type = "FirePet",
            Pet2Type = "WaterPet",
            ResultPetType = "SteamDragon",
            ResultRarity = SynthesisResult.Epic,
            RequiredLevel = 15,
            GoldCost = 1500,
            SuccessRate = 0.35f,
            SynthesisType = SynthesisType.ElementalFusion
        });
        
        Recipes.Add(new PetSynthesisRecipe
        {
            Id = "ice_fire",
            Name = "Thermal Elemental",
            Description = "Combine ice and fire into a Thermal Elemental",
            Pet1Type = "IcePet",
            Pet2Type = "FirePet",
            ResultPetType = "ThermalElemental",
            ResultRarity = SynthesisResult.Rare,
            RequiredLevel = 12,
            GoldCost = 1200,
            SuccessRate = 0.4f,
            SynthesisType = SynthesisType.ElementalFusion
        });
        
        Recipes.Add(new PetSynthesisRecipe
        {
            Id = "lightning_water",
            Name = "Storm Serpent",
            Description = "Combine lightning and water into a Storm Serpent",
            Pet1Type = "LightningPet",
            Pet2Type = "WaterPet",
            ResultPetType = "StormSerpent",
            ResultRarity = SynthesisResult.Epic,
            RequiredLevel = 18,
            GoldCost = 2000,
            SuccessRate = 0.3f,
            SynthesisType = SynthesisType.ElementalFusion
        });
        
        // Beast Fusion Recipes
        Recipes.Add(new PetSynthesisRecipe
        {
            Id = "wolf_wolf",
            Name = "Alpha Wolf",
            Description = "Combine two wolves into an Alpha Wolf",
            Pet1Type = "Wolf",
            Pet2Type = "Wolf",
            ResultPetType = "AlphaWolf",
            ResultRarity = SynthesisResult.Uncommon,
            RequiredLevel = 5,
            GoldCost = 500,
            SuccessRate = 0.6f,
            SynthesisType = SynthesisType.BeastFusion
        });
        
        Recipes.Add(new PetSynthesisRecipe
        {
            Id = "bear_wolf",
            Name = "Fenris Bear",
            Description = "Combine bear and wolf into a Fenris Bear",
            Pet1Type = "Bear",
            Pet2Type = "Wolf",
            ResultPetType = "FenrisBear",
            ResultRarity = SynthesisResult.Rare,
            RequiredLevel = 12,
            GoldCost = 1500,
            SuccessRate = 0.4f,
            SynthesisType = SynthesisType.BeastFusion
        });
        
        // Mythical Fusion Recipes
        Recipes.Add(new PetSynthesisRecipe
        {
            Id = "dragon_dragon",
            Name = "Elder Dragon",
            Description = "Combine two dragons into an Elder Dragon",
            Pet1Type = "Dragon",
            Pet2Type = "Dragon",
            ResultPetType = "ElderDragon",
            ResultRarity = SynthesisResult.Legendary,
            RequiredLevel = 25,
            GoldCost = 5000,
            SuccessRate = 0.15f,
            SynthesisType = SynthesisType.MythicalFusion
        });
        
        Recipes.Add(new PetSynthesisRecipe
        {
            Id = "phoenix_dragon",
            Name = "Solar Phoenix",
            Description = "Combine phoenix and dragon into a Solar Phoenix",
            Pet1Type = "Phoenix",
            Pet2Type = "Dragon",
            ResultPetType = "SolarPhoenix",
            ResultRarity = SynthesisResult.Legendary,
            RequiredLevel = 30,
            GoldCost = 8000,
            SuccessRate = 0.1f,
            SynthesisType = SynthesisType.MythicalFusion
        });
        
        // Shadow Fusion Recipes
        Recipes.Add(new PetSynthesisRecipe
        {
            Id = "shadow_shadow",
            Name = "Void Walker",
            Description = "Combine two shadow pets into a Void Walker",
            Pet1Type = "ShadowPet",
            Pet2Type = "ShadowPet",
            ResultPetType = "VoidWalker",
            ResultRarity = SynthesisResult.Epic,
            RequiredLevel = 20,
            GoldCost = 3000,
            SuccessRate = 0.25f,
            SynthesisType = SynthesisType.ShadowFusion
        });
        
        // Celestial Fusion Recipes
        Recipes.Add(new PetSynthesisRecipe
        {
            Id = "celestial_celestial",
            Name = "Divine Guardian",
            Description = "Combine two celestial pets into a Divine Guardian",
            Pet1Type = "CelestialPet",
            Pet2Type = "CelestialPet",
            ResultPetType = "DivineGuardian",
            ResultRarity = SynthesisResult.Legendary,
            RequiredLevel = 28,
            GoldCost = 6000,
            SuccessRate = 0.12f,
            SynthesisType = SynthesisType.CelestialFusion
        });
        
        // Chaos Fusion Recipes
        Recipes.Add(new PetSynthesisRecipe
        {
            Id = "chaos_chaos",
            Name = "Chaos Lord",
            Description = "Combine two chaos pets into a Chaos Lord",
            Pet1Type = "ChaosPet",
            Pet2Type = "ChaosPet",
            ResultPetType = "ChaosLord",
            ResultRarity = SynthesisResult.Legendary,
            RequiredLevel = 30,
            GoldCost = 10000,
            SuccessRate = 0.08f,
            SynthesisType = SynthesisType.ChaosFusion
        });
    }
    
    private void InitializeElementalCombinations()
    {
        ElementalCombinations["Fire"] = new List<string> { "Water", "Ice", "Lightning" };
        ElementalCombinations["Water"] = new List<string> { "Fire", "Lightning", "Nature" };
        ElementalCombinations["Ice"] = new List<string> { "Fire", "Lightning", "Physical" };
        ElementalCombinations["Lightning"] = new List<string> { "Water", "Ice", "Earth" };
        ElementalCombinations["Nature"] = new List<string> { "Fire", "Water", "Earth" };
        ElementalCombinations["Shadow"] = new List<string> { "Holy", "Light" };
        ElementalCombinations["Holy"] = new List<string> { "Shadow", "Dark" };
    }
    
    private void InitializeRarityWeights()
    {
        RarityWeights[SynthesisResult.Failure] = 0.15f;
        RarityWeights[SynthesisResult.Common] = 0.30f;
        RarityWeights[SynthesisResult.Uncommon] = 0.25f;
        RarityWeights[SynthesisResult.Rare] = 0.18f;
        RarityWeights[SynthesisResult.Epic] = 0.09f;
        RarityWeights[SynthesisResult.Legendary] = 0.03f;
    }
    
    public PetSynthesisRecipe GetRecipe(string pet1Type, string pet2Type)
    {
        foreach (var recipe in Recipes)
        {
            if ((recipe.Pet1Type == pet1Type && recipe.Pet2Type == pet2Type) ||
                (recipe.Pet1Type == pet2Type && recipe.Pet2Type == pet1Type))
            {
                return recipe;
            }
        }
        return null;
    }
    
    public List<PetSynthesisRecipe> GetRecipesByType(SynthesisType type)
    {
        var result = new List<PetSynthesisRecipe>();
        foreach (var recipe in Recipes)
        {
            if (recipe.SynthesisType == type)
                result.Add(recipe);
        }
        return result;
    }
    
    public List<PetSynthesisRecipe> GetAvailableRecipes(int playerLevel)
    {
        var result = new List<PetSynthesisRecipe>();
        foreach (var recipe in Recipes)
        {
            if (recipe.RequiredLevel <= playerLevel)
                result.Add(recipe);
        }
        return result;
    }
    
    public SynthesisResult RollRarity(float successRate)
    {
        var random = new Random();
        float roll = (float)random.NextDouble();
        
        // If synthesis failed
        if (roll > successRate)
        {
            return SynthesisResult.Failure;
        }
        
        // Roll for rarity among successful outcomes
        float rarityRoll = (float)random.NextDouble();
        float cumulative = 0f;
        
        foreach (var kvp in RarityWeights)
        {
            if (kvp.Key == SynthesisResult.Failure) continue;
            cumulative += kvp.Value;
            if (rarityRoll <= cumulative)
                return kvp.Key;
        }
        
        return SynthesisResult.Common;
    }
    
    public string GetRandomResultPet(SynthesisResult rarity, SynthesisType type)
    {
        var random = new Random();
        
        var candidates = new List<string>();
        
        switch (type)
        {
            case SynthesisType.ElementalFusion:
                switch (rarity)
                {
                    case SynthesisResult.Common:
                        candidates = new List<string> { "FlameSpirit", "WaterSprite", "IceShard", "SparkElemental" };
                        break;
                    case SynthesisResult.Uncommon:
                        candidates = new List<string> { "MagmaGuardian", "TideLord", "FrostWyrm", "ThunderSpirit" };
                        break;
                    case SynthesisResult.Rare:
                        candidates = new List<string> { "InfernoBeast", "AbyssalKraken", "BlizzardGiant", "StormTitan" };
                        break;
                    case SynthesisResult.Epic:
                        candidates = new List<string> { "SteamDragon", "ThermalElemental", "StormSerpent" };
                        break;
                    case SynthesisResult.Legendary:
                        candidates = new List<string> { "SolarPhoenix", "CosmicLeviathan" };
                        break;
                }
                break;
                
            case SynthesisType.BeastFusion:
                switch (rarity)
                {
                    case SynthesisResult.Common:
                        candidates = new List<string> { "WildCat", "DireWolf", "ForestBear" };
                        break;
                    case SynthesisResult.Uncommon:
                        candidates = new List<string> { "AlphaWolf", "IronBear", "ShadowHound" };
                        break;
                    case SynthesisResult.Rare:
                        candidates = new List<string> { "FenrisBear", "Nightstalker", "TitanBeast" };
                        break;
                    case SynthesisResult.Epic:
                        candidates = new List<string> { "AncientTitan", "PhantomBeast" };
                        break;
                    case SynthesisResult.Legendary:
                        candidates = new List<string> { "PrimordialBeast", "WorldEater" };
                        break;
                }
                break;
                
            case SynthesisType.MythicalFusion:
                switch (rarity)
                {
                    case SynthesisResult.Rare:
                        candidates = new List<string> { "YoungDragon", "CelestialDrake" };
                        break;
                    case SynthesisResult.Epic:
                        candidates = new List<string> { "ElderDragon", "PhoenixDrake" };
                        break;
                    case SynthesisResult.Legendary:
                        candidates = new List<string> { "SolarPhoenix", "CosmicDragon", "DivineBeast" };
                        break;
                }
                break;
                
            case SynthesisType.ShadowFusion:
                switch (rarity)
                {
                    case SynthesisResult.Uncommon:
                        candidates = new List<string> { "DarkSprite", "ShadowWolf" };
                        break;
                    case SynthesisResult.Rare:
                        candidates = new List<string> { "NightmareHound", "ShadowStalker" };
                        break;
                    case SynthesisResult.Epic:
                        candidates = new List<string> { "VoidWalker", "AbyssalHunter" };
                        break;
                    case SynthesisResult.Legendary:
                        candidates = new List<string> { "LordOfShadows", "EternalDarkness" };
                        break;
                }
                break;
                
            case SynthesisType.CelestialFusion:
                switch (rarity)
                {
                    case SynthesisResult.Rare:
                        candidates = new List<string> { "LightSpirit", "StarAngel" };
                        break;
                    case SynthesisResult.Epic:
                        candidates = new List<string> { "CelestialGuardian", "RadiantSeraph" };
                        break;
                    case SynthesisResult.Legendary:
                        candidates = new List<string> { "DivineGuardian", "Archangel", "HeavenlyKnight" };
                        break;
                }
                break;
                
            case SynthesisType.ChaosFusion:
                switch (rarity)
                {
                    case SynthesisResult.Rare:
                        candidates = new List<string> { "ChaosImp", "UnstableSpirit" };
                        break;
                    case SynthesisResult.Epic:
                        candidates = new List<string> { "ChaosSpawn", "AnomalyEntity" };
                        break;
                    case SynthesisResult.Legendary:
                        candidates = new List<string> { "ChaosLord", "EntropyMaster", "RealityBreaker" };
                        break;
                }
                break;
        }
        
        if (candidates.Count == 0)
            return "MysteriousPet";
            
        return candidates[random.Next(candidates.Count)];
    }
}

public class PetSynthesisRecipe
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Pet1Type { get; set; }
    public string Pet2Type { get; set; }
    public string ResultPetType { get; set; }
    public SynthesisResult ResultRarity { get; set; }
    public int RequiredLevel { get; set; }
    public int GoldCost { get; set; }
    public float SuccessRate { get; set; }
    public SynthesisType SynthesisType { get; set; }
}
