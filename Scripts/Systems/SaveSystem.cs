using Godot;
using System;
using System.IO;
using System.Collections.Generic;
using GameSystems;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// SaveSystem - 存档系统协调器
    /// 协调 SaveSerializer, SaveSlotManager, CloudSaveSystem 三个子系统
    /// 提供统一的存档接口
    /// </summary>
    public partial class SaveSystem : BaseSystem
    {
        // 子系统实例
        private SaveSerializer _serializer;
        private SaveSlotManager _slotManager;
        private CloudSaveSystem _cloudSystem;
        
        // 向后兼容的属性
        public static SaveSystem Instance { get; private set; }
        
        // Signals - 转发自子系统
        public delegate void OnSaveCompleteEventHandler(int slot, bool success);
        public delegate void OnLoadCompleteEventHandler(int slot, bool success);
        public delegate void OnAutoSaveEventHandler(int slot);
        public delegate void OnCloudSyncStartEventHandler();
        public delegate void OnCloudSyncCompleteEventHandler(bool success);
        public delegate void OnCloudSyncErrorEventHandler(string error);
        
        // Type aliases for backward compatibility
        public class SaveData : SaveDataManager.SaveData { }
        public class SaveSlotInfo : SaveDataManager.SaveSlotInfo { }
        
        public override void _Ready()
        {
            Instance = this;
            
            // 初始化子系统
            InitializeSubsystems();
            
            // 转发子系统信号
            ForwardSubsystemSignals();
        }
        
        public override void _Process(double delta)
        {
            // 处理自动保存计时（转发到 SlotManager）
            _slotManager?.ProcessAutoSaveTimer(delta);
            
            // 检查是否需要触发自动保存
            if (_slotManager != null && _slotManager.IsAutoSaveEnabled())
            {
                float timer = _slotManager.GetAutoSaveTimer();
                if (timer <= 0) // 计时器归零表示到达保存间隔
                {
                    PerformAutoSave();
                }
            }
        }
        
        /// <summary>
        /// 初始化所有子系统
        /// </summary>
        private void InitializeSubsystems()
        {
            // 初始化或获取子系统实例
            if (SaveSerializer.Instance == null)
            {
                _serializer = new SaveSerializer();
                AddChild(_serializer);
            }
            else
            {
                _serializer = SaveSerializer.Instance;
            }
            
            if (SaveSlotManager.Instance == null)
            {
                _slotManager = new SaveSlotManager();
                AddChild(_slotManager);
            }
            else
            {
                _slotManager = SaveSlotManager.Instance;
            }
            
            if (CloudSaveSystem.Instance == null)
            {
                _cloudSystem = new CloudSaveSystem();
                AddChild(_cloudSystem);
            }
            else
            {
                _cloudSystem = CloudSaveSystem.Instance;
            }
            
            // 设置序列化器的系统引用
            _serializer.SetSystemReferences(this);
            
            GD.Print("[SaveSystem] All subsystems initialized");
        }
        
        /// <summary>
        /// 转发子系统信号
        /// </summary>
        private void ForwardSubsystemSignals()
        {
            // 转发 SlotManager 信号
            if (_slotManager != null)
            {
                _slotManager.OnSaveComplete += (slot, success) =>
                    EmitSignal(SignalName.OnSaveComplete, slot, success);
                _slotManager.OnLoadComplete += (slot, success) =>
                    EmitSignal(SignalName.OnLoadComplete, slot, success);
                _slotManager.OnAutoSave += (slot) =>
                    EmitSignal(SignalName.OnAutoSave, slot);
            }
            
            // 转发 CloudSystem 信号
            if (_cloudSystem != null)
            {
                _cloudSystem.OnCloudSyncStart += () =>
                    EmitSignal(SignalName.OnCloudSyncStart);
                _cloudSystem.OnCloudSyncComplete += (success) =>
                    EmitSignal(SignalName.OnCloudSyncComplete, success);
                _cloudSystem.OnCloudSyncError += (error) =>
                    EmitSignal(SignalName.OnCloudSyncError, error);
            }
        }
        
        // ========== 槽位管理操作 (转发到 SaveSlotManager) ==========
        
        /// <summary>
        /// 检查存档槽是否有数据
        /// </summary>
        public bool HasSave(int slot)
        {
            return _slotManager?.HasSave(slot) ?? false;
        }
        
        /// <summary>
        /// 保存游戏到槽位
        /// </summary>
        public void SaveGame(int slot, SaveData data, bool createBackup = true)
        {
            _slotManager?.SaveGame(slot, data, createBackup);
            
            // 云同步（如果启用）
            if (_cloudSystem?.IsCloudSyncEnabled() == true && data != null)
            {
                _cloudSystem.SyncSlotToCloud(slot, data);
            }
        }
        
        /// <summary>
        /// 加载游戏从槽位
        /// </summary>
        public SaveData LoadGame(int slot)
        {
            return _slotManager?.LoadGame(slot);
        }
        
        /// <summary>
        /// 获取所有存档
        /// </summary>
        public SaveData[] GetAllSaves()
        {
            return _slotManager?.GetAllSaves();
        }
        
        /// <summary>
        /// 获取所有槽位信息
        /// </summary>
        public SaveSlotInfo[] GetAllSlotInfo()
        {
            return _slotManager?.GetAllSlotInfo();
        }
        
        /// <summary>
        /// 获取槽位信息
        /// </summary>
        public SaveSlotInfo GetSlotInfo(int slot)
        {
            return _slotManager?.GetSlotInfo(slot);
        }
        
        /// <summary>
        /// 删除存档槽
        /// </summary>
        public void DeleteSave(int slot)
        {
            _slotManager?.DeleteSave(slot);
            
            // 同时删除云端存档
            if (_cloudSystem?.IsCloudSyncEnabled() == true)
            {
                _cloudSystem.DeleteCloudSlot(slot, null);
            }
        }
        
        // ========== 自动保存和快速保存 ==========
        
        /// <summary>
        /// 执行自动保存
        /// </summary>
        private void PerformAutoSave()
        {
            var player = GetTree().GetFirstNodeInGroup("player") as Node;
            if (player == null) return;
            
            var data = _serializer?.CreateSaveDataFromPlayer(player);
            if (data == null) return;
            
            data.SaveName = "Auto Save";
            data.LocationName = _serializer?.GetCurrentAreaName() ?? "Unknown Area";
            
            SaveGame(0, data, false);
            GD.Print("Auto-save completed");
        }
        
        /// <summary>
        /// 快速保存
        /// </summary>
        public void QuickSave(Node player)
        {
            var data = _serializer?.CreateSaveDataFromPlayer(player);
            if (data == null) return;
            
            data.SaveName = "Quick Save";
            SaveGame(0, data);
        }
        
        /// <summary>
        /// 快速加载
        /// </summary>
        public SaveData QuickLoad()
        {
            return LoadGame(0);
        }
        
        /// <summary>
        /// 启用/禁用自动保存
        /// </summary>
        public void EnableAutoSave(bool enable)
        {
            _slotManager?.EnableAutoSave(enable);
        }
        
        /// <summary>
        /// 检查自动保存是否启用
        /// </summary>
        public bool IsAutoSaveEnabled()
        {
            return _slotManager?.IsAutoSaveEnabled() ?? false;
        }
        
        // ========== 数据序列化 ==========
        
        /// <summary>
        /// 从玩家节点创建保存数据
        /// </summary>
        public SaveData CreateSaveDataFromPlayer(Node player)
        {
            return _serializer?.CreateSaveDataFromPlayer(player);
        }
        
        /// <summary>
        /// 获取当前区域名称
        /// </summary>
        public string GetCurrentAreaName()
        {
            return _serializer?.GetCurrentAreaName() ?? "Unknown Area";
        }
        
        /// <summary>
        /// 序列化宠物栖息地数据
        /// </summary>
        public Dictionary<string, object> SerializePetHabitatData(PlayerHabitatData data)
        {
            return _serializer?.SerializePetHabitatData(data);
        }
        
        /// <summary>
        /// 反序列化宠物栖息地数据
        /// </summary>
        public PlayerHabitatData DeserializePetHabitatData(Dictionary<string, object> dict)
        {
            return _serializer?.DeserializePetHabitatData(dict);
        }
        
        // ========== 文件操作 ==========
        
        /// <summary>
        /// 导出存档到外部文件
        /// </summary>
        public bool ExportSave(int slot, string exportPath)
        {
            return _slotManager?.ExportSave(slot, exportPath) ?? false;
        }
        
        /// <summary>
        /// 从外部文件导入存档
        /// </summary>
        public bool ImportSave(string importPath, int slot)
        {
            return _slotManager?.ImportSave(importPath, slot) ?? false;
        }
        
        /// <summary>
        /// 获取存档文件大小
        /// </summary>
        public long GetSaveFileSize(int slot)
        {
            return _slotManager?.GetSaveFileSize(slot) ?? 0;
        }
        
        /// <summary>
        /// 检查存档是否损坏
        /// </summary>
        public bool IsSaveCorrupted(int slot)
        {
            return _slotManager?.IsSaveCorrupted(slot) ?? true;
        }
        
        // ========== 加密操作 ==========
        
        /// <summary>
        /// 启用加密
        /// </summary>
        public void EnableEncryption()
        {
            _slotManager?.GetFileManager()?.EnableEncryption();
        }
        
        /// <summary>
        /// 禁用加密
        /// </summary>
        public void DisableEncryption()
        {
            _slotManager?.GetFileManager()?.DisableEncryption();
        }
        
        // ========== 云同步操作 ==========
        
        /// <summary>
        /// 启用云同步
        /// </summary>
        public void EnableCloudSync(string provider = "local")
        {
            _cloudSystem?.EnableCloudSync(provider);
        }
        
        /// <summary>
        /// 禁用云同步
        /// </summary>
        public void DisableCloudSync()
        {
            _cloudSystem?.DisableCloudSync();
        }
        
        /// <summary>
        /// 检查云同步是否启用
        /// </summary>
        public bool IsCloudSyncEnabled()
        {
            return _cloudSystem?.IsCloudSyncEnabled() ?? false;
        }
        
        /// <summary>
        /// 同步所有存档到云端
        /// </summary>
        public void SyncAllToCloud()
        {
            var saves = GetAllSaves();
            _cloudSystem?.SyncAllToCloud(saves);
        }
        
        /// <summary>
        /// 从云端同步所有存档
        /// </summary>
        public void SyncAllFromCloud(Action<SaveData[]> callback)
        {
            _cloudSystem?.SyncAllFromCloud(callback);
        }
        
        /// <summary>
        /// 获取上次同步时间
        /// </summary>
        public DateTime GetLastCloudSyncTime()
        {
            return _cloudSystem?.GetLastSyncTime() ?? DateTime.MinValue;
        }
        
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

        public void SaveMountRaceData(Dictionary<string, object> data)
        {
            _slotManager?.GetFileManager()?.SaveDataWithLogging("user://mount_race_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadMountRaceData()
        {
            return _slotManager?.GetFileManager()?.LoadDataWithLogging<Dictionary<string, object>>("user://mount_race_data.json", "SaveSystem");
        }

        public void SaveMountBattleArenaData(Dictionary<string, object> data)
        {
            _slotManager?.GetFileManager()?.SaveDataWithLogging("user://mount_battle_arena_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadMountBattleArenaData()
        {
            return _slotManager?.GetFileManager()?.LoadDataWithLogging<Dictionary<string, object>>("user://mount_battle_arena_data.json", "SaveSystem");
        }

        public void SavePlayerTalentData(Dictionary<string, object> data)
        {
            _slotManager?.GetFileManager()?.SaveDataWithLogging("user://player_talent_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadPlayerTalentData()
        {
            return _slotManager?.GetFileManager()?.LoadDataWithLogging<Dictionary<string, object>>("user://player_talent_data.json", "SaveSystem");
        }

        public void SavePetExpeditionData(Dictionary<string, object> data)
        {
            _slotManager?.GetFileManager()?.SaveDataWithLogging("user://pet_expedition_data.json", data, "SaveSystem");
        }

        public Dictionary<string, object> LoadPetExpeditionData()
        {
            return _slotManager?.GetFileManager()?.LoadDataWithLogging<Dictionary<string, object>>("user://pet_expedition_data.json", "SaveSystem");
        }

        public void SavePetTrainingData(Dictionary<string, Variant> data)
        {
            _slotManager?.GetFileManager()?.SaveDataWithLogging("user://pet_training_data.json", data, "SaveSystem");
        }

        public Dictionary<string, Variant> LoadPetTrainingData()
        {
            return _slotManager?.GetFileManager()?.LoadDataWithLogging<Dictionary<string, Variant>>("user://pet_training_data.json", "SaveSystem");
        }

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
        
        // ========== BaseSystem 接口实现 ==========
        
        /// <summary>
        /// 获取系统唯一ID
        /// </summary>
        public override string GetId() => "SaveSystem";
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            // 导出各子系统数据
            if (_slotManager != null)
            {
                var slotData = _slotManager.ExportSaveData();
                foreach (var kvp in slotData)
                {
                    data[kvp.Key] = kvp.Value;
                }
            }
            
            if (_cloudSystem != null)
            {
                var cloudData = _cloudSystem.ExportSaveData();
                foreach (var kvp in cloudData)
                {
                    data[kvp.Key] = kvp.Value;
                }
            }
            
            return data;
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            // 转发到各子系统
            _slotManager?.ImportSaveData(data);
            _cloudSystem?.ImportSaveData(data);
        }
    }
}
