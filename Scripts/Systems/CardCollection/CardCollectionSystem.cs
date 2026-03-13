using Godot;
using System;
using System.Collections.Generic;

public class CardCollectionSystem : Node
{
    private CardCollectionData _data;
    private CardCollectionDatabase _database;
    private Random _random = new Random();
    
    // Signals
    public delegate void CardObtainedEventHandler(string cardId, int count);
    public delegate void PackOpenedEventHandler(string packId);
    public delegate void FavoriteChangedEventHandler(string cardId, bool isFavorite);
    public delegate void CategoryUnlockedEventHandler(string category);
    
    public event CardObtainedEventHandler CardObtained;
    public event PackOpenedEventHandler PackOpened;
    public event FavoriteChangedEventHandler FavoriteChanged;
    public event CategoryUnlockedEventHandler CategoryUnlocked;
    
    public override void _Ready()
    {
        _database = GetNode<CardCollectionDatabase>("/root/CardCollectionDatabase");
        _data = GetNode<CardCollectionData>("/root/CardCollectionData");
    }
    
    // Open a card pack and obtain cards
    public List<string> OpenPack(string packId)
    {
        var pack = _database.GetPack(packId);
        if (pack == null)
        {
            GD.PrintErr("CardCollectionSystem: Pack not found: " + packId);
            return new List<string>();
        }
        
        List<string> obtainedCards = new List<string>();
        
        // Randomly select cards from pack
        foreach (var entry in pack.Cards)
        {
            if (_random.Next(100) < entry.Weight * 20) // 20% chance per entry
            {
                string cardId = entry.CardId;
                ObtainCard(cardId, 1, "pack");
                obtainedCards.Add(cardId);
            }
        }
        
        // Ensure at least one card obtained
        if (obtainedCards.Count == 0)
        {
            var firstEntry = pack.Cards[_random.Next(pack.Cards.Count)];
            ObtainCard(firstEntry.CardId, 1, "pack");
            obtainedCards.Add(firstEntry.CardId);
        }
        
        _data.PacksOpened++;
        
        // Deduct gold (if player has gold system)
        // _data.TotalGoldSpent += pack.Price;
        
        PackOpened?.Invoke(packId);
        
        return obtainedCards;
    }
    
    // Obtain a card
    public void ObtainCard(string cardId, int count, string source)
    {
        if (_data.OwnedCards.ContainsKey(cardId))
        {
            _data.OwnedCards[cardId] += count;
            _data.TotalDuplicates += count - 1;
        }
        else
        {
            _data.OwnedCards[cardId] = count;
            _data.TotalUniqueCards++;
            
            // Add to deck buildable if it's a valid card
            var card = _database.GetCard(cardId);
            if (card != null && !_data.DeckBuildableCards.Contains(cardId))
            {
                _data.DeckBuildableCards.Add(cardId);
            }
        }
        
        _data.TotalCardsObtained += count;
        
        // Add to history
        var record = new CardObtainRecord(cardId, count, source);
        _data.ObtainHistory.Insert(0, record);
        
        // Keep only last 50 records
        if (_data.ObtainHistory.Count > 50)
        {
            _data.ObtainHistory.RemoveAt(_data.ObtainHistory.Count - 1);
        }
        
        CardObtained?.Invoke(cardId, count);
    }
    
    // Toggle favorite status
    public void ToggleFavorite(string cardId)
    {
        if (!_data.OwnedCards.ContainsKey(cardId))
            return;
            
        if (_data.FavoriteCards.Contains(cardId))
        {
            _data.FavoriteCards.Remove(cardId);
            FavoriteChanged?.Invoke(cardId, false);
        }
        else
        {
            _data.FavoriteCards.Add(cardId);
            FavoriteChanged?.Invoke(cardId, true);
        }
    }
    
    // Check if card is favorite
    public bool IsFavorite(string cardId)
    {
        return _data.FavoriteCards.Contains(cardId);
    }
    
    // Get card count
    public int GetCardCount(string cardId)
    {
        if (_data.OwnedCards.ContainsKey(cardId))
            return _data.OwnedCards[cardId];
        return 0;
    }
    
    // Get all owned cards
    public Dictionary<string, int> GetOwnedCards()
    {
        return new Dictionary<string, int>(_data.OwnedCards);
    }
    
    // Get cards by category
    public List<string> GetCardsByCategory(string category)
    {
        List<string> result = new List<string>();
        foreach (var kvp in _data.OwnedCards)
        {
            var card = _database.GetCard(kvp.Key);
            if (card != null && card.Category == category)
            {
                result.Add(kvp.Key);
            }
        }
        return result;
    }
    
    // Get cards by rarity
    public List<string> GetCardsByRarity(string rarity)
    {
        List<string> result = new List<string>();
        foreach (var kvp in _data.OwnedCards)
        {
            var card = _database.GetCard(kvp.Key);
            if (card != null && card.Rarity == rarity)
            {
                result.Add(kvp.Key);
            }
        }
        return result;
    }
    
    // Get favorite cards
    public List<string> GetFavoriteCards()
    {
        return new List<string>(_data.FavoriteCards);
    }
    
    // Get deck buildable cards
    public List<string> GetDeckBuildableCards()
    {
        return new List<string>(_data.DeckBuildableCards);
    }
    
    // Check if category is unlocked
    public bool IsCategoryUnlocked(string category)
    {
        if (_data.UnlockedCategories.ContainsKey(category))
            return _data.UnlockedCategories[category];
        return false;
    }
    
    // Unlock category
    public void UnlockCategory(string category)
    {
        _data.UnlockedCategories[category] = true;
        CategoryUnlocked?.Invoke(category);
    }
    
    // Get statistics
    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            { "TotalUniqueCards", _data.TotalUniqueCards },
            { "TotalCardsObtained", _data.TotalCardsObtained },
            { "TotalDuplicates", _data.TotalDuplicates },
            { "PacksOpened", _data.PacksOpened },
            { "TotalGoldSpent", _data.TotalGoldSpent },
            { "FavoriteCount", _data.FavoriteCards.Count },
            { "DeckBuildableCount", _data.DeckBuildableCards.Count }
        };
    }
    
    // Get collection progress (percentage)
    public float GetCollectionProgress()
    {
        int totalCards = _database.Cards.Count;
        if (totalCards == 0) return 0f;
        return (float)_data.TotalUniqueCards / totalCards * 100f;
    }
    
    // Get rarity distribution
    public Dictionary<string, int> GetRarityDistribution()
    {
        Dictionary<string, int> distribution = new Dictionary<string, int>
        {
            { "Common", 0 },
            { "Uncommon", 0 },
            { "Rare", 0 },
            { "Epic", 0 },
            { "Legendary", 0 }
        };
        
        foreach (var kvp in _data.OwnedCards)
        {
            var card = _database.GetCard(kvp.Key);
            if (card != null && distribution.ContainsKey(card.Rarity))
            {
                distribution[card.Rarity]++;
            }
        }
        
        return distribution;
    }
    
    // Get category distribution
    public Dictionary<string, int> GetCategoryDistribution()
    {
        Dictionary<string, int> distribution = new Dictionary<string, int>
        {
            { "Attack", 0 },
            { "Skill", 0 },
            { "Power", 0 },
            { "Defense", 0 },
            { "Special", 0 }
        };
        
        foreach (var kvp in _data.OwnedCards)
        {
            var card = _database.GetCard(kvp.Key);
            if (card != null && distribution.ContainsKey(card.Category))
            {
                distribution[card.Category]++;
            }
        }
        
        return distribution;
    }
}
