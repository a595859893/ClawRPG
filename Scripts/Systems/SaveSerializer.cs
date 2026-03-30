using Godot;
using System;
using System.Collections.Generic;
using GameSystems;
using ClawRPG.Scripts.Mounts;
using ClawRPG.Scripts.UI;
using ClawRPG.Scripts.Systems.Emote;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// SaveSerializer - 负责所有游戏数据的序列化和反序列化
    /// 处理数据转换、导入导出等序列化相关逻辑
    /// </summary>
    public partial class SaveSerializer : BaseSystem
    {
        public static SaveSerializer Instance { get; private set; }

        // 引用其他系统
        private QuickSlotSystem _quickSlotSystem;
        private MountManager _mountManager;
        private BookmarkSystem _bookmarkSystem;
        private AutoBookmarkSystem _autoBookmarkSystem;
        private EquipmentEnhancementSystem _enhancementSystem;
        private AutoPotionSystem _autoPotionSystem;
        private EnchantmentSystem _enchantmentSystem;
        private BountyManager _bountyManager;
        private WeatherSystem _weatherSystem;
        private EquipmentVisuals _equipmentVisuals;
        private ComboSystem _comboSystem;
        private StyleMasterySystem _styleMasterySystem;
        private KeybindingSystem _keybindingSystem;
        private PetStorySystem _petStorySystem;
        private EmoteSystem _emoteSystem;
        private SealedTowerManager _sealedTowerManager;
        private PrestigeSystem _prestigeSystem;
        private QuickModeRewardSystem _quickModeRewardSystem;

        protected override void Initialize()
        {
            Instance = this;
            base.Initialize();
            GD.Print("[SaveSerializer] Initialized");
        }

        /// <summary>
        /// 从玩家节点创建保存数据
        /// </summary>
        public SaveDataManager.SaveData CreateSaveDataFromPlayer(Node player)
        {
            var data = new SaveDataManager.SaveData();

            // 获取玩家属性
            data.Level = 1;
            data.Experience = 0;
            data.CurrentHealth = 100;
            data.MaxHealth = 100;
            data.CurrentMana = 50;
            data.MaxMana = 50;
            data.Gold = 0;
            data.X = player?.Position.X ?? 0;
            data.Y = player?.Position.Y ?? 0;
            data.CurrentArea = "forest";

            // 保存快速槽数据
            if (_quickSlotSystem != null)
            {
                var quickSlotData = _quickSlotSystem.Serialize();
                if (quickSlotData != null)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        data.QuickSlotItemIds[i] = quickSlotData.ContainsKey($"slot_{i}_item") ? (string)quickSlotData[$"slot_{i}_item"] : "";
                        data.QuickSlotQuantities[i] = quickSlotData.ContainsKey($"slot_{i}_qty") ? (int)quickSlotData[$"slot_{i}_qty"] : 0;
                    }
                }
            }

            // 保存坐骑数据
            if (_mountManager != null)
            {
                data.MountData = _mountManager.Serialize();
            }

            // 保存书签数据
            if (_bookmarkSystem != null)
            {
                data.BookmarkData = _bookmarkSystem.Serialize();
            }

            // 保存自动书签数据
            if (_autoBookmarkSystem != null)
            {
                data.AutoBookmarkData = _autoBookmarkSystem.Serialize();
            }

            // 保存强化数据
            if (_enhancementSystem != null)
            {
                data.EnhancementData = _enhancementSystem.Serialize();
            }

            // 保存自动药水数据
            if (_autoPotionSystem != null)
            {
                data.AutoPotionData = _autoPotionSystem.Serialize();
            }

            // 保存附魔数据
            if (_enchantmentSystem != null)
            {
                data.EnchantmentData = _enchantmentSystem.Serialize();
            }

            // 保存赏金数据
            if (_bountyManager != null)
            {
                data.BountyData = _bountyManager.Serialize();
            }

            // 保存天气数据
            if (_weatherSystem != null)
            {
                data.WeatherData = _weatherSystem.Serialize();
            }

            // 保存装备外观数据
            if (_equipmentVisuals != null)
            {
                data.EquipmentVisualsData = _equipmentVisuals.Serialize();
                data.UnlockedVisuals = _equipmentVisuals.GetUnlockedVisualsData();
            }

            // 保存连击系统数据
            var skillComboSystem = SkillComboSystem.Instance;
            if (skillComboSystem != null)
            {
                data.ComboData = skillComboSystem.ExportSaveData();
            }
    
            // 保存 Combo 遗忘系统数据 (REQ-154)
            if (Framework.ComboForgetData.Instance != null)
            {
                data.ComboForgetData = Framework.ComboForgetData.Instance.ExportSaveData();
            }
    
            // 保存风格精通系统数据
            if (_styleMasterySystem != null)
            {
                data.StyleMasteryData = _styleMasterySystem.ExportSaveData();
            }

            // 保存按键绑定数据
            if (_keybindingSystem != null)
            {
                data.KeybindingData = _keybindingSystem.Serialize();
            }

            // 保存宠物故事数据
            if (_petStorySystem != null)
            {
                data.PetStoryData = _petStorySystem.Serialize();
            }

            // 保存表情数据
            if (_emoteSystem != null)
            {
                var emoteData = new Dictionary<string, object>();
                _emoteSystem.SaveData(emoteData);
                data.EmoteData = emoteData;
            }

            // 保存封印塔数据
            if (_sealedTowerManager != null)
            {
                data.SealedTowerData = _sealedTowerManager.SaveData();
            }

            // 保存声望数据
            if (_prestigeSystem != null)
            {
                data.PrestigeData = _prestigeSystem.SaveData();
            }

            // 保存快速模式奖励数据
            if (_quickModeRewardSystem != null)
            {
                data.QuickModeRewardData = _quickModeRewardSystem.ExportSaveData();
            }

            return data;
        }

        /// <summary>
        /// 获取当前区域名称
        /// </summary>
        public string GetCurrentAreaName()
        {
            return "Unknown Area";
        }

        /// <summary>
        /// 设置系统引用(在 SaveSystem 中调用)
        /// </summary>
        public void SetSystemReferences(SaveSystem saveSystem)
        {
            _quickSlotSystem = QuickSlotSystem.Instance;
            _mountManager = MountManager.Instance;
            _bookmarkSystem = BookmarkSystem.Instance;
            _autoBookmarkSystem = saveSystem.GetNodeOrNull<AutoBookmarkSystem>("AutoBookmarkSystem");
            _enhancementSystem = saveSystem.GetNodeOrNull<EnhancementSystem>("EnhancementSystem");
            _autoPotionSystem = saveSystem.GetNodeOrNull<AutoPotionSystem>("AutoPotionSystem");
            _enchantmentSystem = EnchantmentSystem.Instance;
            _bountyManager = BountyManager.Instance;
            _weatherSystem = saveSystem.GetNodeOrNull<WeatherSystem>("WeatherSystem");
            _equipmentVisuals = saveSystem.GetNodeOrNull<UI.EquipmentVisuals>("EquipmentVisuals");
            _comboSystem = saveSystem.GetNodeOrNull<ComboSystem>("ComboSystem");
            _styleMasterySystem = saveSystem.GetNodeOrNull<StyleMasterySystem>("StyleMasterySystem");
            _keybindingSystem = saveSystem.GetNodeOrNull<KeybindingSystem>("KeybindingSystem");
            _petStorySystem = saveSystem.GetNodeOrNull<PetStorySystem>("PetStorySystem");
            _emoteSystem = saveSystem.GetNodeOrNull<EmoteSystem>("EmoteSystem");
            _sealedTowerManager = saveSystem.GetNodeOrNull<SealedTowerManager>("SealedTowerManager");
            _prestigeSystem = saveSystem.GetNodeOrNull<PrestigeSystem>("PrestigeSystem");
            _quickModeRewardSystem = saveSystem.GetNodeOrNull<QuickModeRewardSystem>("QuickModeRewardSystem");
        }

        /// <summary>
        /// 序列化宠物栖息地数据
        /// </summary>
        public Dictionary<string, object> SerializePetHabitatData(PlayerHabitatData data)
        {
            var dict = new Dictionary<string, object>();

            dict["current_habitat_id"] = data.CurrentHabitatId;
            dict["total_comfort"] = data.TotalComfort;
            dict["total_attraction"] = data.TotalAttraction;
            dict["decorations_purchased"] = data.DecorationsPurchased;
            dict["gold_spent_on_decorations"] = data.GoldSpentOnDecorations;
            dict["habitat_visits"] = data.HabitatVisits;
            dict["pets_attracted"] = data.PetsAttracted;

            // 序列化已放置的装饰
            var placedList = new List<Dictionary<string, object>>();
            foreach (var dec in data.PlacedDecorations)
            {
                placedList.Add(new Dictionary<string, object>
                {
                    ["decoration_id"] = dec.DecorationId,
                    ["slot"] = dec.Slot,
                    ["placed_at"] = dec.PlacedAt.ToString("o")
                });
            }
            dict["placed_decorations"] = placedList;

            // 序列化装饰数量
            dict["decoration_counts"] = data.DecorationCounts;

            return dict;
        }

        /// <summary>
        /// 反序列化宠物栖息地数据
        /// </summary>
        public PlayerHabitatData DeserializePetHabitatData(Dictionary<string, object> dict)
        {
            var data = new PlayerHabitatData();

            data.CurrentHabitatId = dict.ContainsKey("current_habitat_id") ? (string)dict["current_habitat_id"] : "meadow";
            data.TotalComfort = dict.ContainsKey("total_comfort") ? Convert.ToInt32(dict["total_comfort"]) : 0;
            data.TotalAttraction = dict.ContainsKey("total_attraction") ? Convert.ToInt32(dict["total_attraction"]) : 0;
            data.DecorationsPurchased = dict.ContainsKey("decorations_purchased") ? Convert.ToInt32(dict["decorations_purchased"]) : 0;
            data.GoldSpentOnDecorations = dict.ContainsKey("gold_spent_on_decorations") ? Convert.ToInt32(dict["gold_spent_on_decorations"]) : 0;
            data.HabitatVisits = dict.ContainsKey("habitat_visits") ? Convert.ToInt32(dict["habitat_visits"]) : 0;
            data.PetsAttracted = dict.ContainsKey("pets_attracted") ? Convert.ToInt32(dict["pets_attracted"]) : 0;

            // 反序列化已放置的装饰
            if (dict.ContainsKey("placed_decorations") && dict["placed_decorations"] != null)
            {
                var placedList = (System.Text.Json.JsonElement)dict["placed_decorations"];
                foreach (var item in placedList.EnumerateArray())
                {
                    var dec = new PlacedDecoration
                    {
                        DecorationId = item.GetProperty("decoration_id").GetString(),
                        Slot = item.GetProperty("slot").GetInt32(),
                        PlacedAt = DateTime.Parse(item.GetProperty("placed_at").GetString())
                    };
                    data.PlacedDecorations.Add(dec);
                }
            }

            // 反序列化装饰数量
            if (dict.ContainsKey("decoration_counts") && dict["decoration_counts"] != null)
            {
                var counts = (System.Text.Json.JsonElement)dict["decoration_counts"];
                foreach (var item in counts.EnumerateObject())
                {
                    data.DecorationCounts[item.Name] = item.Value.GetInt32();
                }
            }

            return data;
        }

        /// <summary>
        /// 导出保存数据 - 实现 BaseSystem 接口
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            // Serializer 本身的状态较少
            data["initialized"] = IsInitialized;
            return data;
        }

        /// <summary>
        /// 导入保存数据 - 实现 BaseSystem 接口
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            // 无状态需要恢复
        }

        public override string GetId() => "SaveSerializer";
    }
}
