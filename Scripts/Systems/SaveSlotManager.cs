using Godot;
using System;
using System.Collections.Generic;
using GameSystems;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// SaveSlotManager - 负责存档槽的管理操作
    /// 处理槽位的创建、读取、删除、快速保存等逻辑
    /// </summary>
    public partial class SaveSlotManager : BaseSystem
    {
        public static SaveSlotManager Instance { get; private set; }

        // 文件管理器实例
        private SaveFileManager _fileManager;

        // 自动保存相关
        private float _autoSaveTimer = 0f;
        private bool _autoSaveEnabled = true;
        private const float AutoSaveInterval = 300f; // 5 minutes

        // 信号
        public delegate void OnSaveCompleteEventHandler(int slot, bool success);
        public delegate void OnLoadCompleteEventHandler(int slot, bool success);
        public delegate void OnAutoSaveEventHandler(int slot);

        protected override void Initialize()
        {
            Instance = this;
            _fileManager = new SaveFileManager();
            base.Initialize();
            GD.Print("[SaveSlotManager] Initialized");
        }

        /// <summary>
        /// 处理自动保存计时
        /// </summary>
        public void ProcessAutoSaveTimer(double delta)
        {
            if (_autoSaveEnabled)
            {
                _autoSaveTimer += (float)delta;
                if (_autoSaveTimer >= AutoSaveInterval)
                {
                    _autoSaveTimer = 0f;
                }
            }
        }

        /// <summary>
        /// 获取自动保存计时器
        /// </summary>
        public float GetAutoSaveTimer() => _autoSaveTimer;

        /// <summary>
        /// 设置自动保存计时器
        /// </summary>
        public void SetAutoSaveTimer(float timer) => _autoSaveTimer = timer;

        /// <summary>
        /// 检查存档槽是否有数据
        /// </summary>
        public bool HasSave(int slot)
        {
            return _fileManager.HasSave(slot);
        }

        /// <summary>
        /// 保存游戏到槽位
        /// </summary>
        public void SaveGame(int slot, SaveDataManager.SaveData data, bool createBackup = true)
        {
            if (slot < 0 || slot >= SaveFileManager.MaxSaveSlots)
            {
                GD.PrintErr("Invalid save slot: " + slot);
                EmitSignal(SignalName.OnSaveComplete, slot, false);
                return;
            }

            bool success = _fileManager.SaveGame(slot, data, createBackup);
            EmitSignal(SignalName.OnSaveComplete, slot, success);
        }

        /// <summary>
        /// 从槽位加载游戏
        /// </summary>
        public SaveDataManager.SaveData LoadGame(int slot)
        {
            if (slot < 0 || slot >= SaveFileManager.MaxSaveSlots)
            {
                GD.PrintErr("Invalid save slot: " + slot);
                EmitSignal(SignalName.OnLoadComplete, slot, false);
                return null;
            }

            var data = _fileManager.LoadGame(slot);
            bool success = data != null;

            if (!success)
            {
                // 尝试从备份加载
                data = _fileManager.LoadFromBackup(slot);
                success = data != null;
            }

            EmitSignal(SignalName.OnLoadComplete, slot, success);
            return data;
        }

        /// <summary>
        /// 获取所有存档
        /// </summary>
        public SaveDataManager.SaveData[] GetAllSaves()
        {
            return _fileManager.GetAllSaves();
        }

        /// <summary>
        /// 获取所有槽位信息
        /// </summary>
        public SaveDataManager.SaveSlotInfo[] GetAllSlotInfo()
        {
            return _fileManager.GetAllSlotInfo();
        }

        /// <summary>
        /// 获取指定槽位信息
        /// </summary>
        public SaveDataManager.SaveSlotInfo GetSlotInfo(int slot)
        {
            return _fileManager.GetSlotInfo(slot);
        }

        /// <summary>
        /// 删除存档槽
        /// </summary>
        public void DeleteSave(int slot)
        {
            _fileManager.DeleteSave(slot);
        }

        /// <summary>
        /// 快速保存
        /// </summary>
        public void QuickSave(Node player, SaveSerializer serializer)
        {
            var data = serializer.CreateSaveDataFromPlayer(player);
            data.SaveName = "Quick Save";
            SaveGame(0, data);
        }

        /// <summary>
        /// 快速加载
        /// </summary>
        public SaveDataManager.SaveData QuickLoad()
        {
            return LoadGame(0);
        }

        /// <summary>
        /// 导出存档到外部文件
        /// </summary>
        public bool ExportSave(int slot, string exportPath)
        {
            return _fileManager.ExportSave(slot, exportPath);
        }

        /// <summary>
        /// 从外部文件导入存档
        /// </summary>
        public bool ImportSave(string importPath, int slot)
        {
            return _fileManager.ImportSave(importPath, slot);
        }

        /// <summary>
        /// 获取存档文件大小
        /// </summary>
        public long GetSaveFileSize(int slot)
        {
            return _fileManager.GetSaveFileSize(slot);
        }

        /// <summary>
        /// 检查存档是否损坏
        /// </summary>
        public bool IsSaveCorrupted(int slot)
        {
            return _fileManager.IsSaveCorrupted(slot);
        }

        /// <summary>
        /// 启用/禁用自动保存
        /// </summary>
        public void EnableAutoSave(bool enable)
        {
            _autoSaveEnabled = enable;
            if (enable)
            {
                _autoSaveTimer = 0f; // 重新启用时重置计时器
            }
        }

        /// <summary>
        /// 检查自动保存是否启用
        /// </summary>
        public bool IsAutoSaveEnabled()
        {
            return _autoSaveEnabled;
        }

        /// <summary>
        /// 获取文件管理器实例
        /// </summary>
        public SaveFileManager GetFileManager()
        {
            return _fileManager;
        }

        /// <summary>
        /// 导出保存数据 - 实现 BaseSystem 接口
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["auto_save_enabled"] = _autoSaveEnabled;
            data["auto_save_timer"] = _autoSaveTimer;
            return data;
        }

        /// <summary>
        /// 导入保存数据 - 实现 BaseSystem 接口
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            if (data.ContainsKey("auto_save_enabled"))
            {
                _autoSaveEnabled = (bool)data["auto_save_enabled"];
            }
            if (data.ContainsKey("auto_save_timer"))
            {
                _autoSaveTimer = (float)data["auto_save_timer"];
            }
        }

        public override string GetId() => "SaveSlotManager";
    }
}
