using Godot;
using System;
using System.Collections.Generic;

public partial class CardCollectionSystem : BaseSystem
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
    
    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // Owned cards
        var ownedCardsArray = new Godot.Array();
        foreach (var kvp in _data.OwnedCards)
        {
            var cardData = new Dictionary<string, object>();
            cardData["card_id"] = kvp.Key;
            cardData["count"] = kvp.Value;
            ownedCardsArray.Add(cardData);
        }
        data["owned_cards"] = ownedCardsArray;
        
        // Favorite cards
        data["favorite_cards"] = new Godot.Array(_data.FavoriteCards);
        
        // Unlocked categories
        var categoriesArray = new Godot.Array();
        foreach (var kvp in _data.UnlockedCategories)
        {
            if (kvp.Value)
            {
                categoriesArray.Add(kvp.Key);
            }
        }
        data["unlocked_categories"] = categoriesArray;
        
        // Statistics
        data["total_unique_cards"] = _data.TotalUniqueCards;
        data["total_cards_obtained"] = _data.TotalCardsObtained;
        data["total_duplicates"] = _data.TotalDuplicates;
        data["packs_opened"] = _data.PacksOpened;
        data["total_gold_spent"] = _data.TotalGoldSpent;
        
        return data;
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("owned_cards"))
        {
            _data.OwnedCards.Clear();
            var cardsArray = (Godot.Array)data["owned_cards"];
            foreach (Dictionary cardData in cardsArray)
            {
                string cardId = (string)cardData["card_id"];
                int count = (int)cardData["count"];
                _data.OwnedCards[cardId] = count;
            }
        }
        
        if (data.Contains("favorite_cards"))
        {
            _data.FavoriteCards.Clear();
            var favArray = (Godot.Array)data["favorite_cards"];
            foreach (string cardId in favArray)
            {
                _data.FavoriteCards.Add(cardId);
            }
        }
        
        if (data.Contains("unlocked_categories"))
        {
            _data.UnlockedCategories.Clear();
            var catArray = (Godot.Array)data["unlocked_categories"];
            foreach (string category in catArray)
            {
                _data.UnlockedCategories[category] = true;
            }
        }
        
        if (data.Contains("total_unique_cards")) _data.TotalUniqueCards = (int)data["total_unique_cards"];
        if (data.Contains("total_cards_obtained")) _data.TotalCardsObtained = (int)data["total_cards_obtained"];
        if (data.Contains("total_duplicates")) _data.TotalDuplicates = (int)data["total_duplicates"];
        if (data.Contains("packs_opened")) _data.PacksOpened = (int)data["packs_opened"];
        if (data.Contains("total_gold_spent")) _data.TotalGoldSpent = (int)data["total_gold_spent"];
    }
}
