using Godot;
using System;
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
            InitializeSubsystems();
            ForwardSubsystemSignals();
        }

        public override void _Process(double delta)
        {
            _slotManager?.ProcessAutoSaveTimer(delta);
            if (_slotManager != null && _slotManager.IsAutoSaveEnabled())
            {
                float timer = _slotManager.GetAutoSaveTimer();
                if (timer <= 0)
                {
                    PerformAutoSave();
                }
            }
        }

        private void InitializeSubsystems()
        {
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

            _serializer.SetSystemReferences(this);
            GD.Print("[SaveSystem] All subsystems initialized");
        }

        private void ForwardSubsystemSignals()
        {
            if (_slotManager != null)
            {
                _slotManager.OnSaveComplete += (slot, success) =>
                    EmitSignal(SignalName.OnSaveComplete, slot, success);
                _slotManager.OnLoadComplete += (slot, success) =>
                    EmitSignal(SignalName.OnLoadComplete, slot, success);
                _slotManager.OnAutoSave += (slot) =>
                    EmitSignal(SignalName.OnAutoSave, slot);
            }

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

        // ========== 槽位管理操作 ==========

        public bool HasSave(int slot) => _slotManager?.HasSave(slot) ?? false;

        public void SaveGame(int slot, SaveData data, bool createBackup = true)
        {
            _slotManager?.SaveGame(slot, data, createBackup);
            if (_cloudSystem?.IsCloudSyncEnabled() == true && data != null)
            {
                _cloudSystem.SyncSlotToCloud(slot, data);
            }
        }

        public SaveData LoadGame(int slot) => _slotManager?.LoadGame(slot);
        public SaveData[] GetAllSaves() => _slotManager?.GetAllSaves();
        public SaveSlotInfo[] GetAllSlotInfo() => _slotManager?.GetAllSlotInfo();
        public SaveSlotInfo GetSlotInfo(int slot) => _slotManager?.GetSlotInfo(slot);

        public void DeleteSave(int slot)
        {
            _slotManager?.DeleteSave(slot);
            if (_cloudSystem?.IsCloudSyncEnabled() == true)
            {
                _cloudSystem.DeleteCloudSlot(slot, null);
            }
        }

        // ========== 自动保存和快速保存 ==========

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

        public void QuickSave(Node player)
        {
            var data = _serializer?.CreateSaveDataFromPlayer(player);
            if (data == null) return;
            data.SaveName = "Quick Save";
            SaveGame(0, data);
        }

        public SaveData QuickLoad() => LoadGame(0);
        public void EnableAutoSave(bool enable) => _slotManager?.EnableAutoSave(enable);
        public bool IsAutoSaveEnabled() => _slotManager?.IsAutoSaveEnabled() ?? false;

        // ========== 数据序列化 ==========

        public SaveData CreateSaveDataFromPlayer(Node player) => _serializer?.CreateSaveDataFromPlayer(player);
        public string GetCurrentAreaName() => _serializer?.GetCurrentAreaName() ?? "Unknown Area";
        public Dictionary<string, object> SerializePetHabitatData(PlayerHabitatData data) => _serializer?.SerializePetHabitatData(data);
        public PlayerHabitatData DeserializePetHabitatData(Dictionary<string, object> dict) => _serializer?.DeserializePetHabitatData(dict);

        // ========== 文件操作 ==========

        public bool ExportSave(int slot, string exportPath) => _slotManager?.ExportSave(slot, exportPath) ?? false;
        public bool ImportSave(string importPath, int slot) => _slotManager?.ImportSave(importPath, slot) ?? false;
        public long GetSaveFileSize(int slot) => _slotManager?.GetSaveFileSize(slot) ?? 0;
        public bool IsSaveCorrupted(int slot) => _slotManager?.IsSaveCorrupted(slot) ?? true;

        // ========== 加密操作 ==========

        public void EnableEncryption() => _slotManager?.GetFileManager()?.EnableEncryption();
        public void DisableEncryption() => _slotManager?.GetFileManager()?.DisableEncryption();

        // ========== 云同步操作 ==========

        public void EnableCloudSync(string provider = "local") => _cloudSystem?.EnableCloudSync(provider);
        public void DisableCloudSync() => _cloudSystem?.DisableCloudSync();
        public bool IsCloudSyncEnabled() => _cloudSystem?.IsCloudSyncEnabled() ?? false;

        public void SyncAllToCloud()
        {
            var saves = GetAllSaves();
            _cloudSystem?.SyncAllToCloud(saves);
        }

        public void SyncAllFromCloud(Action<SaveData[]> callback) => _cloudSystem?.SyncAllFromCloud(callback);
        public DateTime GetLastCloudSyncTime() => _cloudSystem?.GetLastSyncTime() ?? DateTime.MinValue;

        // ========== BaseSystem 接口实现 ==========

        public override string GetId() => "SaveSystem";

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            if (_slotManager != null)
            {
                var slotData = _slotManager.ExportSaveData();
                foreach (var kvp in slotData) data[kvp.Key] = kvp.Value;
            }
            if (_cloudSystem != null)
            {
                var cloudData = _cloudSystem.ExportSaveData();
                foreach (var kvp in cloudData) data[kvp.Key] = kvp.Value;
            }
            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            _slotManager?.ImportSaveData(data);
            _cloudSystem?.ImportSaveData(data);
        }
    }
}
