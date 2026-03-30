using Godot;
using System;
using System.Collections.Generic;

public class CardCollectionData
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
    
    public CardCollectionData()
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
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public Dictionary ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // 卡牌收藏
        data["owned_cards"] = new Dictionary(OwnedCards);
        
        // 喜欢的卡牌
        data["favorite_cards"] = new Array(FavoriteCards);
        
        // 统计数据
        data["total_unique_cards"] = TotalUniqueCards;
        data["total_cards_obtained"] = TotalCardsObtained;
        data["total_duplicates"] = TotalDuplicates;
        data["total_gold_spent"] = TotalGoldSpent;
        data["total_gold_earned"] = TotalGoldEarned;
        data["packs_opened"] = PacksOpened;
        
        // 获取历史
        var historyList = new Array();
        foreach (var record in ObtainHistory)
        {
            var recordDict = new Dictionary
            {
                { "card_id", record.CardId },
                { "count", record.Count },
                { "source", record.Source },
                { "timestamp", record.Timestamp }
            };
            historyList.Add(recordDict);
        }
        data["obtain_history"] = historyList;
        
        // 分类解锁
        data["unlocked_categories"] = new Dictionary(UnlockedCategories);
        
        // 可用于组卡的卡牌
        data["deck_buildable_cards"] = new Array(DeckBuildableCards);
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        // 卡牌收藏
        if (data.Contains("owned_cards"))
        {
            var cardsDict = (Dictionary)data["owned_cards"];
            OwnedCards = new Dictionary<string, int>();
            foreach (var kvp in cardsDict)
            {
                OwnedCards[kvp.Key] = (int)kvp.Value;
            }
        }
        
        // 喜欢的卡牌
        if (data.Contains("favorite_cards"))
        {
            var favArray = (Array)data["favorite_cards"];
            FavoriteCards = new List<string>();
            foreach (string card in favArray)
            {
                FavoriteCards.Add(card);
            }
        }
        
        // 统计数据
        TotalUniqueCards = (int)data.GetValueOrDefault("total_unique_cards", 0);
        TotalCardsObtained = (int)data.GetValueOrDefault("total_cards_obtained", 0);
        TotalDuplicates = (int)data.GetValueOrDefault("total_duplicates", 0);
        TotalGoldSpent = (int)data.GetValueOrDefault("total_gold_spent", 0);
        TotalGoldEarned = (int)data.GetValueOrDefault("total_gold_earned", 0);
        PacksOpened = (int)data.GetValueOrDefault("packs_opened", 0);
        
        // 获取历史
        if (data.Contains("obtain_history"))
        {
            var historyArray = (Array)data["obtain_history"];
            ObtainHistory = new List<CardObtainRecord>();
            foreach (Dictionary recordDict in historyArray)
            {
                var record = new CardObtainRecord(
                    (string)recordDict["card_id"],
                    (int)recordDict["count"],
                    (string)recordDict["source"]
                );
                record.Timestamp = (int)recordDict["timestamp"];
                ObtainHistory.Add(record);
            }
        }
        
        // 分类解锁
        if (data.Contains("unlocked_categories"))
        {
            var categoriesDict = (Dictionary)data["unlocked_categories"];
            UnlockedCategories = new Dictionary<string, bool>();
            foreach (var kvp in categoriesDict)
            {
                UnlockedCategories[kvp.Key] = (bool)kvp.Value;
            }
        }
        
        // 可用于组卡的卡牌
        if (data.Contains("deck_buildable_cards"))
        {
            var deckArray = (Array)data["deck_buildable_cards"];
            DeckBuildableCards = new List<string>();
            foreach (string card in deckArray)
            {
                DeckBuildableCards.Add(card);
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
