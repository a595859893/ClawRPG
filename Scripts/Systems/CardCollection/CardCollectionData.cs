using Godot;
using System;
using System.Collections.Generic;

public class CardCollectionData : BaseSystem
{
    // Card collection entries: card_id -> count
    public Dictionary<string, int> OwnedCards = new Dictionary<string, int>();
    
    // Cards marked as favorite
    public List<string> FavoriteCards = new List<string>();
    
    // Total cards collected (unique count)
    public int TotalUniqueCards = 0;
    
    // Statistics
    public int TotalCardsObtained = 0;
    public int TotalDuplicates = 0;
    public int TotalGoldSpent = 0;
    public int TotalGoldEarned = 0;
    public int PacksOpened = 0;
    
    // Card obtain history
    public List<CardObtainRecord> ObtainHistory = new List<CardObtainRecord>();
    
    // Category unlock tracking
    public Dictionary<string, bool> UnlockedCategories = new Dictionary<string, bool>();
    
    // Deck building integration: cards available for deck building
    public List<string> DeckBuildableCards = new List<string>();
    
    public override void _Ready()
    {
        // Initialize default categories as unlocked
        string[] defaultCategories = { "Attack", "Skill", "Power", "Defense" };
        foreach (var cat in defaultCategories)
        {
            if (!UnlockedCategories.ContainsKey(cat))
            {
                UnlockedCategories[cat] = true;
            }
        }
    }
}

public class CardObtainRecord
{
    public string CardId;
    public int Count;
    public string Source; // "pack", "trade", "reward", "shop"
    public int Timestamp;
    
    public CardObtainRecord(string cardId, int count, string source)
    {
        CardId = cardId;
        Count = count;
        Source = source;
        Timestamp = OS.GetUnixTime();
    }
}
