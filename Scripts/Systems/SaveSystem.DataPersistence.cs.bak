using Godot;
using System;
using System.Collections.Generic;
using GameSystems;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// SaveSystem — 大量独立数据类型 Save/Load 转发
    /// 所有方法转发到 _slotManager.GetFileManager().SaveDataWithLogging(...)
    /// 按数据类型分组
    /// </summary>
    public partial class SaveSystem
    {
        // ========== 宠物相关数据保存/加载 ==========

        public void SavePetTalentData(PlayerPetTalentData data)
        {
            _slotManager?.GetFileManager()?.SaveDataWithLogging("user://pet_talent_data.json", data, "SaveSystem");
        }

        public PlayerPetTalentData LoadPetTalentData()
        {
            return _slotManager?.GetFileManager()?.LoadDataWithLogging<PlayerPetTalentData>("user://pet_talent_data.json", "SaveSystem");
        }

        // ========== 战利品掉落数据保存/加载 ==========

        public void SaveLootDropData(LootDropData.PlayerLootData data)
        {
            _slotManager?.GetFileManager()?.SaveDataWithLogging("user://loot_drop_data.json", data, "SaveSystem");
        }

        public LootDropData.PlayerLootData LoadLootDropData()
        {
            return _slotManager?.GetFileManager()?.LoadDataWithLogging<LootDropData.PlayerLootData>("user://loot_drop_data.json", "SaveSystem");
        }

        // ========== 装备耐久度数据保存/加载 ==========

        public void SaveEquipmentDurabilityData(Dictionary<string, object> data)
        {
            _slotManager?.GetFileManager()?.SaveDataWithLogging("user://equipment_durability_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadEquipmentDurabilityData()
        {
            return _slotManager?.GetFileManager()?.LoadDataWithLogging<Dictionary<string, object>>("user://equipment_durability_data.json", "SaveSystem");
        }

        // ========== 收藏品数据保存/加载 ==========

        public void SaveCollectibleData(Dictionary<string, object> data)
        {
            _slotManager?.GetFileManager()?.SaveDataWithLogging("user://collectible_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadCollectibleData()
        {
            return _slotManager?.GetFileManager()?.LoadDataWithLogging<Dictionary<string, object>>("user://collectible_data.json", "SaveSystem");
        }

        // ========== 季节活动数据保存/加载 ==========

        public void SaveSeasonalEventData(Dictionary<string, object> data)
        {
            _slotManager?.GetFileManager()?.SaveDataWithLogging("user://seasonal_event_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadSeasonalEventData()
        {
            return _slotManager?.GetFileManager()?.LoadDataWithLogging<Dictionary<string, object>>("user://seasonal_event_data.json", "SaveSystem");
        }

        // ========== 坐骑竞速数据保存/加载 ==========

        public void SaveMountRaceData(Dictionary<string, object> data)
        {
            _slotManager?.GetFileManager()?.SaveDataWithLogging("user://mount_race_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadMountRaceData()
        {
            return _slotManager?.GetFileManager()?.LoadDataWithLogging<Dictionary<string, object>>("user://mount_race_data.json", "SaveSystem");
        }

        // ========== 坐骑战斗竞技场数据保存/加载 ==========

        public void SaveMountBattleArenaData(Dictionary<string, object> data)
        {
            _slotManager?.GetFileManager()?.SaveDataWithLogging("user://mount_battle_arena_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadMountBattleArenaData()
        {
            return _slotManager?.GetFileManager()?.LoadDataWithLogging<Dictionary<string, object>>("user://mount_battle_arena_data.json", "SaveSystem");
        }

        // ========== 天赋数据保存/加载 ==========

        public void SavePlayerTalentData(Dictionary<string, object> data)
        {
            _slotManager?.GetFileManager()?.SaveDataWithLogging("user://player_talent_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadPlayerTalentData()
        {
            return _slotManager?.GetFileManager()?.LoadDataWithLogging<Dictionary<string, object>>("user://player_talent_data.json", "SaveSystem");
        }

        // ========== 宠物探险数据保存/加载 ==========

        public void SavePetExpeditionData(Dictionary<string, object> data)
        {
            _slotManager?.GetFileManager()?.SaveDataWithLogging("user://pet_expedition_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadPetExpeditionData()
        {
            return _slotManager?.GetFileManager()?.LoadDataWithLogging<Dictionary<string, object>>("user://pet_expedition_data.json", "SaveSystem");
        }

        // ========== 宠物训练数据保存/加载 ==========

        public void SavePetTrainingData(Dictionary<string, Variant> data)
        {
            _slotManager?.GetFileManager()?.SaveDataWithLogging("user://pet_training_data.json", data, "SaveSystem");
        }

        public Dictionary<string, Variant> LoadPetTrainingData()
        {
            return _slotManager?.GetFileManager()?.LoadDataWithLogging<Dictionary<string, Variant>>("user://pet_training_data.json", "SaveSystem");
        }

        // ========== 宠物栖息地数据保存/加载 ==========

        public void SavePetHabitatData(PlayerHabitatData data)
        {
            try
            {
                var dict = SerializePetHabitatData(data);
                if (dict != null)
                {
                    _slotManager?.GetFileManager()?.SaveDataWithLogging("user://pet_habitat_data.json", dict, "SaveSystem");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("[SaveSystem] Failed to save pet habitat data: " + e.Message);
            }
        }

        public PlayerHabitatData LoadPetHabitatData()
        {
            try
            {
                var dict = _slotManager?.GetFileManager()?.LoadDataWithLogging<Dictionary<string, object>>("user://pet_habitat_data.json", "SaveSystem");
                if (dict != null)
                {
                    var data = DeserializePetHabitatData(dict);
                    GD.Print("[SaveSystem] Pet habitat data loaded");
                    return data;
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("[SaveSystem] Failed to load pet habitat data: " + e.Message);
            }
            return new PlayerHabitatData();
        }

        // ========== 坐骑探险数据保存/加载 ==========

        public void SaveMountExpeditionData(Dictionary<string, object> data)
        {
            _slotManager?.GetFileManager()?.SaveDataWithLogging("user://mount_expedition_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadMountExpeditionData()
        {
            return _slotManager?.GetFileManager()?.LoadDataWithLogging<Dictionary<string, object>>("user://mount_expedition_data.json", "SaveSystem");
        }

        // ========== 快速模式奖励数据 ==========

        public void SaveQuickModeData(Dictionary<string, object> data)
        {
            try
            {
                var quickModeData = new Dictionary<string, object>();
                foreach (var kvp in data)
                {
                    quickModeData[kvp.Key] = kvp.Value;
                }

                // 同时保存到主存档
                var mainSave = LoadGame(0);
                if (mainSave != null)
                {
                    mainSave.QuickModeRewardData = quickModeData;
                    SaveGame(0, mainSave, false);
                }

                GD.Print("[SaveSystem] Quick mode reward data saved");
            }
            catch (Exception e)
            {
                GD.PrintErr("[SaveSystem] Failed to save quick mode reward data: " + e.Message);
            }
        }

        public Dictionary<string, object> LoadQuickModeData()
        {
            try
            {
                var mainSave = LoadGame(0);
                if (mainSave != null && mainSave.QuickModeRewardData != null)
                {
                    GD.Print("[SaveSystem] Quick mode reward data loaded");
                    return mainSave.QuickModeRewardData;
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("[SaveSystem] Failed to load quick mode reward data: " + e.Message);
            }
            return new Dictionary<string, object>();
        }
    }
}
