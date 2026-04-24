using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using GameSystems;
using ClawRPG.Framework;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// CloudSaveSystem - 负责云同步功能
    /// 处理存档的上传、下载、冲突解决等云端操作
    /// </summary>
    public partial class CloudSaveSystem : BaseSystem
    {
        public static CloudSaveSystem Instance { get; private set; }

        // 云同步状态
        private bool _isCloudSyncEnabled = false;
        private bool _isSyncing = false;
        private DateTime _lastSyncTime;
        private string _cloudProvider = "local"; // local, steam, google, etc.
        
        // 云存储 Provider
        private ICloudStorageProvider _storageProvider;
        
        // 云端存储的最大槽位数
        private const int MaxCloudSlots = 10;
        
        // 信号
        public delegate void OnCloudSyncStartEventHandler();
        public delegate void OnCloudSyncCompleteEventHandler(bool success);
        public delegate void OnCloudSyncErrorEventHandler(string error);
        public delegate void OnConflictDetectedEventHandler(int slot, SaveDataManager.SaveData localData, SaveDataManager.SaveData cloudData);
        
        protected override void Initialize()
        {
            Instance = this;
            base.Initialize();
            GD.Print("[CloudSaveSystem] Initialized");
        }

        /// <summary>
        /// 启用云同步
        /// </summary>
        /// <param name="provider">存储 provider 类型: "local"（默认），扩展时填 "steam"、"google" 等</param>
        /// <remarks>
        /// Provider 扩展: 实现 <see cref="ICloudStorageProvider"/> 接口，
        /// 在 EnableCloudSync 中添加对应分支即可支持新 provider。
        /// </remarks>
        public void EnableCloudSync(string provider = "local")
        {
            _cloudProvider = provider;
            _isCloudSyncEnabled = true;
            
            // 根据 provider 参数创建并挂载对应的 provider 实现
            if (provider == "local")
            {
                _storageProvider = new LocalCloudStorageProvider();
                AddChild(_storageProvider);
            }
            // Provider 扩展说明: 要支持更多 provider（如 steam, google），请实现 ICloudStorageProvider
            // 接口并在此添加分支: else if (provider == "steam") { _storageProvider = new SteamCloudStorageProvider(); }
            GD.Print("[CloudSaveSystem] Cloud sync enabled with provider: " + provider);
        }

        /// <summary>
        /// 禁用云同步
        /// </summary>
        public void DisableCloudSync()
        {
            _isCloudSyncEnabled = false;
            
            // 清理 storageProvider
            if (_storageProvider != null)
            {
                _storageProvider.QueueFree();
                _storageProvider = null;
            }
            
            GD.Print("[CloudSaveSystem] Cloud sync disabled");
        }

        /// <summary>
        /// 检查云同步是否启用
        /// </summary>
        public bool IsCloudSyncEnabled() => _isCloudSyncEnabled;

        /// <summary>
        /// 检查是否正在同步
        /// </summary>
        public bool IsSyncing() => _isSyncing;

        /// <summary>
        /// 获取上次同步时间
        /// </summary>
        public DateTime GetLastSyncTime() => _lastSyncTime;

        /// <summary>
        /// 同步单个存档槽到云端
        /// </summary>
        public async void SyncSlotToCloud(int slot, SaveDataManager.SaveData localData)
        {
            if (!_isCloudSyncEnabled || _isSyncing)
            {
                GD.PrintWarn("[CloudSaveSystem] Cloud sync is disabled or already in progress");
                return;
            }

            _isSyncing = true;
            EmitSignal(SignalName.OnCloudSyncStart);

            try
            {
                // 模拟云同步延迟（实际实现中会调用云API）
                await ToSignal(GetTree().CreateTimer(0.5), "timeout");
                
                // 序列化存档数据为 JSON
                string jsonData = JsonSerializer.Serialize(localData);
                
                // 调用云存储 Provider 上传
                bool uploadSuccess = _storageProvider.UploadSlot(slot, jsonData);
                
                if (!uploadSuccess)
                {
                    throw new Exception("UploadSlot returned false for slot " + slot);
                }
                
                GD.Print("[CloudSaveSystem] Slot " + slot + " synced to cloud");
                _lastSyncTime = DateTime.Now;
                
                _isSyncing = false;
                EmitSignal(SignalName.OnCloudSyncComplete, true);
            }
            catch (Exception e)
            {
                GD.PrintErr("[CloudSaveSystem] Failed to sync slot " + slot + ": " + e.Message);
                _isSyncing = false;
                EmitSignal(SignalName.OnCloudSyncComplete, false);
                EmitSignal(SignalName.OnCloudSyncError, e.Message);
            }
        }

        /// <summary>
        /// 从云端下载单个存档槽
        /// </summary>
        public async void SyncSlotFromCloud(int slot, Action<SaveDataManager.SaveData> callback)
        {
            if (!_isCloudSyncEnabled || _isSyncing)
            {
                GD.PrintWarn("[CloudSaveSystem] Cloud sync is disabled or already in progress");
                callback?.Invoke(null);
                return;
            }

            _isSyncing = true;
            EmitSignal(SignalName.OnCloudSyncStart);

            try
            {
                // 模拟云同步延迟（实际实现中会调用云API）
                await ToSignal(GetTree().CreateTimer(0.5), "timeout");
                
                // 调用云存储 Provider 下载
                string jsonData = _storageProvider.DownloadSlot(slot);
                
                // 返回 null 表示云端没有该槽位的数据
                if (string.IsNullOrEmpty(jsonData))
                {
                    GD.Print("[CloudSaveSystem] No cloud data found for slot " + slot);
                    _isSyncing = false;
                    EmitSignal(SignalName.OnCloudSyncComplete, true);
                    callback?.Invoke(null);
                    return;
                }
                
                // 反序列化 JSON 为 SaveData
                SaveDataManager.SaveData saveData = JsonSerializer.Deserialize<SaveDataManager.SaveData>(jsonData);
                
                GD.Print("[CloudSaveSystem] Slot " + slot + " downloaded from cloud");
                _lastSyncTime = DateTime.Now;
                
                _isSyncing = false;
                EmitSignal(SignalName.OnCloudSyncComplete, true);
                callback?.Invoke(saveData);
            }
            catch (Exception e)
            {
                GD.PrintErr("[CloudSaveSystem] Failed to download slot " + slot + ": " + e.Message);
                _isSyncing = false;
                EmitSignal(SignalName.OnCloudSyncComplete, false);
                EmitSignal(SignalName.OnCloudSyncError, e.Message);
                callback?.Invoke(null);
            }
        }

        /// <summary>
        /// 同步所有存档槽到云端
        /// </summary>
        public async void SyncAllToCloud(SaveDataManager.SaveData[] localSaves)
        {
            if (!_isCloudSyncEnabled || _isSyncing)
            {
                GD.PrintWarn("[CloudSaveSystem] Cloud sync is disabled or already in progress");
                return;
            }

            _isSyncing = true;
            EmitSignal(SignalName.OnCloudSyncStart);

            try
            {
                for (int i = 0; i < localSaves.Length; i++)
                {
                    if (localSaves[i] != null)
                    {
                        await ToSignal(GetTree().CreateTimer(0.1), "timeout");
                        
                        // 序列化存档数据为 JSON
                        string jsonData = JsonSerializer.Serialize(localSaves[i]);
                        
                        // 调用云存储 Provider 上传
                        bool uploadSuccess = _storageProvider.UploadSlot(i, jsonData);
                        
                        if (!uploadSuccess)
                        {
                            throw new Exception("UploadSlot returned false for slot " + i);
                        }
                        
                        GD.Print("[CloudSaveSystem] Synced slot " + i + " to cloud");
                    }
                }
                
                _lastSyncTime = DateTime.Now;
                GD.Print("[CloudSaveSystem] All slots synced to cloud");
                
                _isSyncing = false;
                EmitSignal(SignalName.OnCloudSyncComplete, true);
            }
            catch (Exception e)
            {
                GD.PrintErr("[CloudSaveSystem] Failed to sync all slots: " + e.Message);
                _isSyncing = false;
                EmitSignal(SignalName.OnCloudSyncComplete, false);
                EmitSignal(SignalName.OnCloudSyncError, e.Message);
            }
        }

        /// <summary>
        /// 从云端下载所有存档槽
        /// </summary>
        public async void SyncAllFromCloud(Action<SaveDataManager.SaveData[]> callback)
        {
            if (!_isCloudSyncEnabled || _isSyncing)
            {
                GD.PrintWarn("[CloudSaveSystem] Cloud sync is disabled or already in progress");
                callback?.Invoke(null);
                return;
            }

            _isSyncing = true;
            EmitSignal(SignalName.OnCloudSyncStart);

            try
            {
                // 模拟云同步延迟
                await ToSignal(GetTree().CreateTimer(0.5), "timeout");
                
                // 获取云端所有槽位列表
                var cloudSlotList = _storageProvider.ListSlots();
                
                // 下载每个槽位的数据
                var downloadedSaves = new List<SaveDataManager.SaveData>();
                foreach (var slotInfo in cloudSlotList)
                {
                    int slot = slotInfo.Slot;
                    string jsonData = _storageProvider.DownloadSlot(slot);
                    
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        SaveDataManager.SaveData saveData = JsonSerializer.Deserialize<SaveDataManager.SaveData>(jsonData);
                        downloadedSaves.Add(saveData);
                        GD.Print("[CloudSaveSystem] Downloaded slot " + slot + " from cloud");
                    }
                }
                
                _lastSyncTime = DateTime.Now;
                GD.Print("[CloudSaveSystem] All slots downloaded from cloud: " + downloadedSaves.Count + " slots");
                
                _isSyncing = false;
                EmitSignal(SignalName.OnCloudSyncComplete, true);
                callback?.Invoke(downloadedSaves.Count > 0 ? downloadedSaves.ToArray() : null);
            }
            catch (Exception e)
            {
                GD.PrintErr("[CloudSaveSystem] Failed to download all slots: " + e.Message);
                _isSyncing = false;
                EmitSignal(SignalName.OnCloudSyncComplete, false);
                EmitSignal(SignalName.OnCloudSyncError, e.Message);
                callback?.Invoke(null);
            }
        }

        /// <summary>
        /// 检测并处理存档冲突
        /// </summary>
        public void CheckForConflicts(int slot, SaveDataManager.SaveData localData, Action<SaveDataManager.SaveData> resolveCallback)
        {
            if (!_isCloudSyncEnabled)
            {
                resolveCallback?.Invoke(localData);
                return;
            }
            
            // 从云端下载对应槽位数据
            string cloudJson = _storageProvider.DownloadSlot(slot);
            
            // 云端无此槽，直接使用本地数据
            if (string.IsNullOrEmpty(cloudJson))
            {
                GD.Print("[CloudSaveSystem] No cloud data for slot " + slot + ", using local");
                resolveCallback?.Invoke(localData);
                return;
            }
            
            // 反序列化云端数据
            SaveDataManager.SaveData cloudData = JsonSerializer.Deserialize<SaveDataManager.SaveData>(cloudJson);
            
            // 比较时间戳
            double timeDiffSeconds = Math.Abs((localData.SaveTime - cloudData.SaveTime).TotalSeconds);
            
            if (timeDiffSeconds > 60)
            {
                // 冲突：时间差超过阈值，通知上层处理
                GD.Print("[CloudSaveSystem] Conflict detected for slot " + slot + ": time diff = " + timeDiffSeconds + "s");
                EmitSignal(SignalName.OnConflictDetected, slot, localData, cloudData);
                // 由 resolveCallback 处理解决策略（上层可能会调用 SaveDataManager 处理）
                resolveCallback?.Invoke(localData);
            }
            else
            {
                // 无冲突，使用较新的数据
                GD.Print("[CloudSaveSystem] No conflict for slot " + slot + ", using local");
                resolveCallback?.Invoke(localData);
            }
        }

        /// <summary>
        /// 获取云端存档列表
        /// </summary>
        public async void GetCloudSlotList(Action<List<SaveDataManager.SaveSlotInfo>> callback)
        {
            if (!_isCloudSyncEnabled)
            {
                GD.PrintWarn("[CloudSaveSystem] Cloud sync is disabled");
                callback?.Invoke(null);
                return;
            }

            try
            {
                // 模拟API调用
                await ToSignal(GetTree().CreateTimer(0.3), "timeout");
                
                // 获取云端槽位列表
                var slotList = _storageProvider.ListSlots();
                
                GD.Print("[CloudSaveSystem] Got cloud slot list: " + slotList.Count + " slots");
                callback?.Invoke(slotList);
            }
            catch (Exception e)
            {
                GD.PrintErr("[CloudSaveSystem] Failed to get cloud slot list: " + e.Message);
                callback?.Invoke(null);
            }
        }

        /// <summary>
        /// 删除云端存档
        /// </summary>
        public async void DeleteCloudSlot(int slot, Action<bool> callback)
        {
            if (!_isCloudSyncEnabled || _isSyncing)
            {
                GD.PrintWarn("[CloudSaveSystem] Cloud sync is disabled or in progress");
                callback?.Invoke(false);
                return;
            }

            _isSyncing = true;

            try
            {
                // 模拟API调用
                await ToSignal(GetTree().CreateTimer(0.3), "timeout");
                
                // 调用云存储 Provider 删除
                bool deleteSuccess = _storageProvider.DeleteSlot(slot);
                
                if (!deleteSuccess)
                {
                    throw new Exception("DeleteSlot returned false for slot " + slot);
                }
                
                GD.Print("[CloudSaveSystem] Deleted slot " + slot + " from cloud");
                
                _isSyncing = false;
                callback?.Invoke(true);
            }
            catch (Exception e)
            {
                GD.PrintErr("[CloudSaveSystem] Failed to delete cloud slot: " + e.Message);
                _isSyncing = false;
                callback?.Invoke(false);
            }
        }

        /// <summary>
        /// 获取云存储使用情况
        /// </summary>
        public async void GetCloudStorageUsage(Action<long> callback)
        {
            if (!_isCloudSyncEnabled)
            {
                callback?.Invoke(0);
                return;
            }

            try
            {
                // 模拟API调用
                await ToSignal(GetTree().CreateTimer(0.2), "timeout");
                
                // 获取云存储使用量
                long usage = _storageProvider.GetStorageUsageBytes();
                
                GD.Print("[CloudSaveSystem] Cloud storage usage: " + usage + " bytes");
                callback?.Invoke(usage);
            }
            catch (Exception e)
            {
                GD.PrintErr("[CloudSaveSystem] Failed to get storage usage: " + e.Message);
                callback?.Invoke(0);
            }
        }

        /// <summary>
        /// 导出保存数据 - 实现 BaseSystem 接口
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["cloud_sync_enabled"] = _isCloudSyncEnabled;
            data["cloud_provider"] = _cloudProvider;
            data["last_sync_time"] = _lastSyncTime.ToString("o");
            return data;
        }

        /// <summary>
        /// 导入保存数据 - 实现 BaseSystem 接口
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            if (data.ContainsKey("cloud_sync_enabled"))
            {
                _isCloudSyncEnabled = (bool)data["cloud_sync_enabled"];
            }
            if (data.ContainsKey("cloud_provider"))
            {
                _cloudProvider = (string)data["cloud_provider"];
            }
            if (data.ContainsKey("last_sync_time"))
            {
                _lastSyncTime = DateTime.Parse((string)data["last_sync_time"]);
            }
        }

        public override string GetId() => "CloudSaveSystem";
    }
}
