using Godot;
using System;
using System.Collections.Generic;

public partial class PetEggData : Resource
{
    [Export] public string eggId = "";
    [Export] public string eggName = "";
    [Export] public string description = "";
    [Export] public string petType = "";
    [Export] public int rarity = 1; // 1=Common, 2=Uncommon, 3=Rare, 4=Epic, 5=Legendary
    [Export] public int hatchTimeSeconds = 300; // 5 minutes default
    [Export] public string requiredItemId = "";
    [Export] public int requiredItemCount = 1;
    [Export] public int goldCost = 100;
    [Export] public float[] possiblePetIds = new float[0];
    [Export] public float[] possiblePetWeights = new float[0];
    [Export] public Texture2D eggIcon;
    
    public static string[] RarityNames = { "", "Common", "Uncommon", "Rare", "Epic", "Legendary" };
    public static string[] RarityColors = { "", "#9d9d9d", "#1eff00", "#0070dd", "#a335ee", "#ff8000" };
}
