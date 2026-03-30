using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Systems;

/// <summary>
/// 保存/加载管理器 - 负责游戏数据的持久化
/// </summary>
public class SaveLoadManager : ManagerBase
{
    public static SaveLoadManager Instance { get; private set; }
    
    /// <summary>
    /// 优先级（数值越小越先初始化）
    /// </summary>
    public override int Priority => 5;
    
    /// <summary>
    /// 最大存档槽位数量
    /// </summary>
    public const int MaxSaveSlots = 10;
    
    /// <summary>
    /// 自动存档间隔（秒）
    /// </summary>
    public float AutoSaveInterval { get; set; } = 300f; // 5分钟
    
    /// <summary>
    /// 是否启用自动存档
    /// </summary>
    public bool AutoSaveEnabled { get; set; } = true;
    
    /// <summary>
    /// 自动存档槽位
    /// </summary>
    public int AutoSaveSlot { get; set; } = 0;
    
    /// <summary>
    /// 当前存档槽位
    /// </summary>
    public int CurrentSaveSlot { get; private set; } = -1;
    
    /// <summary>
    /// 最后保存时间
    /// </summary>
    public DateTime LastSaveTime { get; private set; }
    
    /// <summary>
    /// 最后加载时间
    /// </summary>
    public DateTime LastLoadTime { get; private set; }
    
    /// <summary>
    /// 自动存档计时器
    /// </summary>
    private float _autoSaveTimer = 0f;
    
    /// <summary>
    /// 是否有未保存的更改
    /// </summary>
    public bool HasUnsavedChanges { get; private set; } = false;
    
    // 引用 SaveSystem
    private SaveSystem _saveSystem;
    
    // 事件
    public event Action<int> OnSaveStarted;
    public event Action<int> OnSaveCompleted;
    public event Action<int> OnLoadStarted;
    public event Action<int> OnLoadCompleted;
    public event Action<int, float> OnSaveProgress;
    public event Action<int, float> OnLoadProgress;
    public event Action<string> OnSaveError;
    
    public override void _Ready()
    {
        Instance = this;
        base._Ready();
    }
    
    protected override void Initialize()
    {
        GD.Print("[SaveLoadManager] Initialized");
        
        _saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (_saveSystem == null)
        {
            _saveSystem = new SaveSystem();
            _saveSystem.Name = "SaveSystem";
            GetTree().Root.AddChild(_saveSystem);
        }
        
        NotifyInitialized();
    }
    
    public override void ManagerUpdate(double delta)
    {
        if (!AutoSaveEnabled) return;
        
        _autoSaveTimer += (float)delta;
        if (_autoSaveTimer >= AutoSaveInterval)
        {
            _autoSaveTimer = 0f;
            TriggerAutoSave();
        }
    }
    
    /// <summary>
    /// 触发自动存档
    /// </summary>
    private void TriggerAutoSave()
    {
        if (CurrentSaveSlot >= 0)
        {
            SaveGame(CurrentSaveSlot);
            GD.Print("[SaveLoadManager] Auto save triggered");
        }
    }
    
    /// <summary>
    /// 保存游戏到指定槽位
    /// </summary>
    public bool SaveGame(int slot, Dictionary data = null)
    {
        if (slot < 0 || slot >= MaxSaveSlots)
        {
            GD.PrintErr($"[SaveLoadManager] Invalid save slot: {slot}");
            return false;
        }
        
        OnSaveStarted?.Invoke(slot);
        
        try
        {
            // 收集游戏数据
            var saveData = data ?? CollectSaveData();
            
            // 添加元数据
            saveData["saveTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            saveData["saveSlot"] = slot;
            saveData["gameVersion"] = "1.0.0";
            
            // 调用 SaveSystem 保存
            bool success = _saveSystem.SaveGame(slot, saveData);
            
            if (success)
            {
                CurrentSaveSlot = slot;
                LastSaveTime = DateTime.Now;
                HasUnsavedChanges = false;
                
                GD.Print($"[SaveLoadManager] Game saved to slot {slot}");
                OnSaveCompleted?.Invoke(slot);
                return true;
            }
            else
            {
                OnSaveError?.Invoke("Failed to save game");
                return false;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[SaveLoadManager] Save error: {ex.Message}");
            OnSaveError?.Invoke(ex.Message);
            return false;
        }
    }
    
    /// <summary>
    /// 加载游戏从指定槽位
    /// </summary>
    public bool LoadGame(int slot)
    {
        if (slot < 0 || slot >= MaxSaveSlots)
        {
            GD.PrintErr($"[SaveLoadManager] Invalid save slot: {slot}");
            return false;
        }
        
        if (!_saveSystem.HasSave(slot))
        {
            GD.PrintErr($"[SaveLoadManager] No save found in slot {slot}");
            return false;
        }
        
        OnLoadStarted?.Invoke(slot);
        
        try
        {
            var saveData = _saveSystem.LoadGame(slot);
            
            if (saveData != null)
            {
                // 应用数据到游戏系统
                ApplySaveData(saveData);
                
                CurrentSaveSlot = slot;
                LastLoadTime = DateTime.Now;
                
                GD.Print($"[SaveLoadManager] Game loaded from slot {slot}");
                OnLoadCompleted?.Invoke(slot);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[SaveLoadManager] Load error: {ex.Message}");
            OnSaveError?.Invoke(ex.Message);
            return false;
        }
    }
    
    /// <summary>
    /// 收集所有游戏数据
    /// </summary>
    private Dictionary CollectSaveData()
    {
        var allData = new Dictionary<string, object>();
        
        // 从 GameManager 获取所有系统数据
        var gameManager = GetNode("/root/Main");
        if (gameManager != null && gameManager.HasMethod("ExportAllData"))
        {
            var systemData = gameManager.Call("ExportAllData") as Dictionary;
            foreach (DictionaryEntry entry in systemData)
            {
                allData[entry.Key] = entry.Value;
            }
        }
        
        return allData;
    }
    
    /// <summary>
    /// 应用保存数据到游戏系统
    /// </summary>
    private void ApplySaveData(Dictionary saveData)
    {
        var gameManager = GetNode("/root/Main");
        if (gameManager != null && gameManager.HasMethod("ImportAllData"))
        {
            gameManager.Call("ImportAllData", saveData);
        }
    }
    
    /// <summary>
    /// 检查指定槽位是否有存档
    /// </summary>
    public bool HasSave(int slot)
    {
        return _saveSystem?.HasSave(slot) ?? false;
    }
    
    /// <summary>
    /// 获取存档信息
    /// </summary>
    public Dictionary GetSaveInfo(int slot)
    {
        if (!_saveSystem.HasSave(slot)) return null;
        
        var saveData = _saveSystem.LoadGame(slot);
        if (saveData == null) return null;
        
        return new Dictionary
        {
            { "saveTime", saveData.Contains("saveTime") ? saveData["saveTime"] : "Unknown" },
            { "gameVersion", saveData.Contains("gameVersion") ? saveData["gameVersion"] : "Unknown" }
        };
    }
    
    /// <summary>
    /// 删除指定槽位的存档
    /// </summary>
    public bool DeleteSave(int slot)
    {
        if (slot < 0 || slot >= MaxSaveSlots) return false;
        
        return _saveSystem.DeleteSave(slot);
    }
    
    /// <summary>
    /// 标记有未保存的更改
    /// </summary>
    public void MarkDirty()
    {
        HasUnsavedChanges = true;
    }
    
    /// <summary>
    /// 重置自动存档计时器
    /// </summary>
    public void ResetAutoSaveTimer()
    {
        _autoSaveTimer = 0f;
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary
        {
            { "currentSaveSlot", CurrentSaveSlot },
            { "autoSaveInterval", AutoSaveInterval },
            { "autoSaveEnabled", AutoSaveEnabled },
            { "lastSaveTime", LastSaveTime.ToString("yyyy-MM-dd HH:mm:ss") }
        };
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("currentSaveSlot"))
            CurrentSaveSlot = Convert.ToInt32(data["currentSaveSlot"]);
        if (data.Contains("autoSaveInterval"))
            AutoSaveInterval = Convert.ToSingle(data["autoSaveInterval"]);
        if (data.Contains("autoSaveEnabled"))
            AutoSaveEnabled = Convert.ToBoolean(data["autoSaveEnabled"]);
    }
}
