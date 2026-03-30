using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Items;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Procedural Equipment Generation System - generates random affixes for equipment
    /// </summary>
    public partial class ProceduralEquipmentSystem : BaseSystem
    {
        private static ProceduralEquipmentSystem _instance;
        public static ProceduralEquipmentSystem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ProceduralEquipmentSystem();
                return _instance;
            }
        }
        
        private Random _random = new Random();
        private PlayerAffixData _playerAffixData = new PlayerAffixData();
        
        // Rarity weights for affix selection
        private static readonly Dictionary<ItemQuality, float> QualityWeights = new Dictionary<ItemQuality, float>
        {
            { ItemQuality.Common, 50f },
            { ItemQuality.Uncommon, 30f },
            { ItemQuality.Rare, 15f },
            { ItemQuality.Epic, 4f },
            { ItemQuality.Legendary, 1f }
        };
        
        public event Action<int, EquipmentAffixData> OnAffixesGenerated;
        
        public ProceduralEquipmentSystem()
        {
            EquipmentAffixDatabase.Initialize();
        }

        public override void _Ready()
        {
            base._Ready();
            Initialize();
        }

        protected override void Initialize()
        {
            IsInitialized = true;
            GD.Print("[ProceduralEquipmentSystem] initialized");
        }
        
        /// <summary>
        /// Generate random affixes for equipment based on its quality
        /// </summary>
        public EquipmentAffixData GenerateAffixesForEquipment(int itemId, ItemQuality quality)
        {
            EquipmentAffixData data = new EquipmentAffixData
            {
                ItemId = itemId,
                Affixes = new List<EquipmentAffix>(),
                TotalScore = 0
            };
            
            int affixCount = EquipmentAffixDatabase.GetAffixCount(quality);
            if (affixCount == 0)
            {
                return data;
            }
            
            // Get available affixes
            List<EquipmentAffix> availableAffixes = EquipmentAffixDatabase.GetAffixesForQuality(quality);
            if (availableAffixes.Count == 0)
            {
                return data;
            }
            
            // Track used attribute names to avoid duplicates
            HashSet<string> usedAttributes = new HashSet<string>();
            
            for (int i = 0; i < affixCount; i++)
            {
                // Try to find a unique affix
                EquipmentAffix selectedAffix = SelectRandomAffix(availableAffixes, usedAttributes, i == 0);
                
                if (selectedAffix != null)
                {
                    data.Affixes.Add(selectedAffix);
                    usedAttributes.Add(selectedAffix.AttributeName);
                    data.TotalScore += CalculateAffixScore(selectedAffix, quality);
                }
            }
            
            // Store in player data
            _playerAffixData.EquipmentAffixes[itemId] = data;
            
            // Fire event
            OnAffixesGenerated?.Invoke(itemId, data);
            
            GD.Print($"[ProceduralEquipment] Generated {data.Affixes.Count} affixes for item {itemId}, score: {data.TotalScore:F1}");
            
            return data;
        }
        
        /// <summary>
        /// Select a random affix from available pool
        /// </summary>
        private EquipmentAffix SelectRandomAffix(List<EquipmentAffix> pool, HashSet<string> usedAttributes, bool allowAnyType)
        {
            // Filter out already used attributes
            List<EquipmentAffix> validAffixes = new List<EquipmentAffix>();
            foreach (var affix in pool)
            {
                if (!usedAttributes.Contains(affix.AttributeName))
                {
                    validAffixes.Add(affix);
                }
            }
            
            if (validAffixes.Count == 0)
                return null;
            
            // Weighted random selection
            float totalWeight = 0f;
            foreach (var affix in validAffixes)
            {
                totalWeight += affix.Weight;
            }
            
            float randomValue = (float)(_random.NextDouble() * totalWeight);
            float cumulative = 0f;
            
            foreach (var affix in validAffixes)
            {
                cumulative += affix.Weight;
                if (randomValue <= cumulative)
                {
                    return affix;
                }
            }
            
            return validAffixes[validAffixes.Count - 1];
        }
        
        /// <summary>
        /// Calculate affix score based on value and rarity
        /// </summary>
        private float CalculateAffixScore(EquipmentAffix affix, ItemQuality quality)
        {
            // Base score from attribute value
            float baseScore = affix.AttributeValue;
            
            // Attribute weights
            Dictionary<string, float> attrWeights = new Dictionary<string, float>
            {
                { "attack", 1.2f },
                { "defense", 1.0f },
                { "health", 0.5f },
                { "speed", 1.1f },
                { "crit_rate", 1.3f },
                { "crit_damage", 1.4f },
                { "lifesteal", 1.5f },
                { "dodge", 1.3f },
                { "resistance", 1.0f }
            };
            
            float attrWeight = attrWeights.ContainsKey(affix.AttributeName) ? attrWeights[affix.AttributeName] : 1.0f;
            
            // Quality multiplier
            float qualityMultiplier = GetQualityMultiplier(quality);
            
            return baseScore * attrWeight * qualityMultiplier;
        }
        
        /// <summary>
        /// Get quality multiplier for scoring
        /// </summary>
        public static float GetQualityMultiplier(ItemQuality quality)
        {
            switch (quality)
            {
                case ItemQuality.Common: return 1.0f;
                case ItemQuality.Uncommon: return 2.0f;
                case ItemQuality.Rare: return 4.0f;
                case ItemQuality.Epic: return 8.0f;
                case ItemQuality.Legendary: return 16.0f;
                default: return 1.0f;
            }
        }
        
        /// <summary>
        /// Calculate total equipment score with affixes
        /// </summary>
        public float GetEquipmentTotalScore(int itemId, float baseScore)
        {
            if (_playerAffixData.EquipmentAffixes.TryGetValue(itemId, out var affixData))
            {
                return baseScore + affixData.TotalScore;
            }
            return baseScore;
        }
        
        /// <summary>
        /// Get affixes for an equipment item
        /// </summary>
        public List<EquipmentAffix> GetAffixesForEquipment(int itemId)
        {
            if (_playerAffixData.EquipmentAffixes.TryGetValue(itemId, out var affixData))
            {
                return affixData.Affixes;
            }
            return new List<EquipmentAffix>();
        }
        
        /// <summary>
        /// Get attribute bonus from affixes
        /// </summary>
        public float GetAffixBonus(int itemId, string attributeName)
        {
            var affixes = GetAffixesForEquipment(itemId);
            foreach (var affix in affixes)
            {
                if (affix.AttributeName == attributeName)
                {
                    return affix.AttributeValue;
                }
            }
            return 0f;
        }
        
        /// <summary>
        /// Get all affix bonuses as a dictionary
        /// </summary>
        public Dictionary<string, float> GetAllAffixBonuses(int itemId)
        {
            Dictionary<string, float> bonuses = new Dictionary<string, float>();
            var affixes = GetAffixesForEquipment(itemId);
            
            foreach (var affix in affixes)
            {
                if (!bonuses.ContainsKey(affix.AttributeName))
                {
                    bonuses[affix.AttributeName] = 0f;
                }
                bonuses[affix.AttributeName] += affix.AttributeValue;
            }
            
            return bonuses;
        }
        
        /// <summary>
        /// Check if equipment has affixes
        /// </summary>
        public bool HasAffixes(int itemId)
        {
            return _playerAffixData.EquipmentAffixes.ContainsKey(itemId) && 
                   _playerAffixData.EquipmentAffixes[itemId].Affixes.Count > 0;
        }
        
        /// <summary>
        /// Generate formatted affix description
        /// </summary>
        public string GetAffixDescription(int itemId)
        {
            var affixes = GetAffixesForEquipment(itemId);
            if (affixes.Count == 0) return "";
            
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var affix in affixes)
            {
                if (sb.Length > 0) sb.Append("\n");
                sb.Append(affix.Description);
            }
            return sb.ToString();
        }
        
        /// <summary>
        /// Save affix data
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            Dictionary data = new Dictionary<string, object>();
            Godot.Collections.Array equipmentList = new Godot.Collections.Array();
            
            foreach (var kvp in _playerAffixData.EquipmentAffixes)
            {
                Godot.Collections.Dictionary itemData = new Godot.Collections.Dictionary
                {
                    { "item_id", kvp.Key },
                    { "score", kvp.Value.TotalScore }
                };
                
                Godot.Collections.Array affixIds = new Godot.Collections.Array();
                foreach (var affix in kvp.Value.Affixes)
                {
                    affixIds.Add(affix.Id);
                }
                itemData["affix_ids"] = affixIds;
                
                equipmentList.Add(itemData);
            }
            
            data["equipment_affixes"] = equipmentList;
            return data;
        }
        
        /// <summary>
        /// Load affix data
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            _playerAffixData = new PlayerAffixData();
            
            if (data == null || !data.ContainsKey("equipment_affixes"))
                return;
            
            var equipmentList = data["equipment_affixes"] as Godot.Collections.Array;
            if (equipmentList == null) return;
            
            // Get all affixes for lookup
            var allAffixes = EquipmentAffixDatabase.GetAllAffixes();
            
            foreach (var itemData in equipmentList)
            {
                var dict = itemData as Godot.Collections.Dictionary;
                if (dict == null) continue;
                
                int itemId = dict.ContainsKey("item_id") ? Convert.ToInt32(dict["item_id"]) : 0;
                float score = dict.ContainsKey("score") ? Convert.ToSingle(dict["score"]) : 0;
                
                var affixIds = dict["affix_ids"] as Godot.Collections.Array;
                if (affixIds == null) continue;
                
                EquipmentAffixData affixData = new EquipmentAffixData
                {
                    ItemId = itemId,
                    TotalScore = score,
                    Affixes = new List<EquipmentAffix>()
                };
                
                foreach (var affixIdObj in affixIds)
                {
                    string affixId = affixIdObj.ToString();
                    foreach (var affix in allAffixes)
                    {
                        if (affix.Id == affixId)
                        {
                            affixData.Affixes.Add(affix);
                            break;
                        }
                    }
                }
                
                _playerAffixData.EquipmentAffixes[itemId] = affixData;
            }
            
            GD.Print($"[ProceduralEquipment] Loaded {_playerAffixData.EquipmentAffixes.Count} equipment affixes");
        }
    }
}
