using Godot;
using System;
using System.Collections.Generic;

public partial class PetBreedingData : Resource
{
    public Dictionary<string, bool> UnlockedBreeds = new Dictionary<string, bool>();
    public List<PetBreedingRecord> BreedingHistory = new List<PetBreedingRecord>();
    [Export] public int TotalBreeds = 0;
    [Export] public int SuccessfulBreeds = 0;
    [Export] public int LegendaryBreeds = 0;
    public Dictionary<string, int> OffspringStats = new Dictionary<string, int>();
}

public class PetBreedingRecord
{
    public string Parent1Id { get; set; }
    public string Parent2Id { get; set; }
    public string OffspringId { get; set; }
    public string OffspringType { get; set; }
    public int Rarity { get; set; }
    public DateTime BreedingTime { get; set; }
    public bool WasSuccessful { get; set; }
}

public enum PetBreedResult
{
    Failure,
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
