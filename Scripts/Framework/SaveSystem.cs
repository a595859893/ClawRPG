using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// 统一存档系统 - 自动发现并序列化所有实现了 ISaveable 的系统
/// </summary>
public class SaveSystem : BaseSystem
{
    /// <summary>
    /// 单例
    /// </summary>
    public static SaveSystem Instance { get; private set; }
    
    /// <summary>
    /// 可保存的接口
    /// </summary>
    public interface ISaveable
    {
        Dictionary ExportSaveData();
        void ImportSaveData(Dictionary data);
        string GetSaveId();
    }
    
    /// <summary>
    /// 所有已注册的可保存对象
    /// </summary>
    private readonly Dictionary<string, ISaveable> _saveables = new Dictionary<string, ISaveable>();
    
    /// <summary>
    /// 存档文件路径
    /// </summary>
    private const string SAVE_FILE = "user://save_game.dat";
    
    public override void _Ready()
    {
        base._Ready();
        
        if (Instance != null && Instance != this)
        {
            GD.PrintErr("[SaveSystem] Instance already exists!");
            QueueFree();
            return;
        }
        
        Instance = this;
        GD.Print("[SaveSystem] Initialized");
    }
    
    /// <summary>
    /// 注册可保存对象
    /// </summary>
    public void Register(ISaveable saveable)
    {
        var saveId = saveable.GetSaveId();
        if (!string.IsNullOrEmpty(saveId))
        {
            _saveables[saveId] = saveable;
            GD.Print($"[SaveSystem] Registered: {saveId}");
        }
    }
    
    /// <summary>
    /// 注销可保存对象
    /// </summary>
    public void Unregister(ISaveable saveable)
    {
        var saveId = saveable.GetSaveId();
        _saveables.Remove(saveId);
        GD.Print($"[SaveSystem] Unregistered: {saveId}");
    }
    
    /// <summary>
    /// 导出所有数据
    /// </summary>
    public Dictionary ExportAllData()
    {
        var allData = new Dictionary();
        
        foreach (var kvp in _saveables)
        {
            try
            {
                allData[kvp.Key] = kvp.Value.ExportSaveData();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[SaveSystem] Failed to export {kvp.Key}: {ex.Message}");
            }
        }
        
        GD.Print($"[SaveSystem] Exported {_saveables.Count} systems");
        return allData;
    }
    
    /// <summary>
    /// 导入所有数据
    /// </summary>
    public void ImportAllData(Dictionary data)
    {
        if (data == null) return;
        
        foreach (var kvp in _saveables)
        {
            if (data.Contains(kvp.Key))
            {
                try
                {
                    kvp.Value.ImportSaveData((Dictionary)data[kvp.Key]);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[SaveSystem] Failed to import {kvp.Key}: {ex.Message}");
                }
            }
        }
        
        GD.Print($"[SaveSystem] Imported {_saveables.Count} systems");
    }
    
    /// <summary>
    /// 保存游戏
    /// </summary>
    public bool SaveGame()
    {
        try
        {
            var data = ExportAllData();
            var file = new File();
            
            // 使用 JSON 序列化
            var json = JSON.Print(data);
            file.Open(SAVE_FILE, File.ModeFlags.Write);
            file.StoreString(json);
            file.Close();
            
            GD.Print($"[SaveSystem] Game saved to {SAVE_FILE}");
            return true;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[SaveSystem] Save failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 加载游戏
    /// </summary>
    public bool LoadGame()
    {
        try
        {
            var file = new File();
            if (!file.FileExists(SAVE_FILE))
            {
                GD.Print("[SaveSystem] No save file found");
                return false;
            }
            
            file.Open(SAVE_FILE, File.ModeFlags.Read);
            var json = file.GetAsText();
            file.Close();
            
            var result = JSON.Parse(json);
            if (result.Error != Error.Ok)
            {
                GD.PrintErr($"[SaveSystem] JSON parse failed: {result.ErrorString}");
                return false;
            }
            
            ImportAllData((Dictionary)result.Result);
            GD.Print("[SaveSystem] Game loaded");
            return true;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[SaveSystem] Load failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 删除存档
    /// </summary>
    public void DeleteSave()
    {
        var dir = new Directory();
        if (dir.FileExists(SAVE_FILE))
        {
            dir.Remove(SAVE_FILE);
            GD.Print("[SaveSystem] Save deleted");
        }
    }
    
    /// <summary>
    /// 检查存档是否存在
    /// </summary>
    public bool HasSave()
    {
        var file = new File();
        return file.FileExists(SAVE_FILE);
    }
    
    /// <summary>
    /// 获取系统唯一ID
    /// </summary>
    public override string GetId() => "SaveSystem";
    
    /// <summary>
    /// 导出保存数据 - 实现 BaseSystem 接口
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return ExportAllData();
    }
    
    /// <summary>
    /// 导入保存数据 - 实现 BaseSystem 接口
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        ImportAllData(data);
    }
}
