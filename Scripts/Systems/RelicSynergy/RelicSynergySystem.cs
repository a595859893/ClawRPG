using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems;

/// <summary>
/// 遗物协同系统 — 检测并触发遗物组合的隐藏协同效果
///REQ-185 实现
/// </summary>
public partial class RelicSynergySystem : BaseSystem
{
    public static RelicSynergySystem Instance { get; private set; }
    
    private PlayerSynergyData _playerData = new();
    
    // 信号定义
    public delegate void SynergyDiscovered(string synergyId, string message);
    public delegate void SynergyActivated(string synergyId, string bonusType, float bonusValue);
    public delegate void SynergyDeactivated(string synergyId);
    
    // 运行时活跃的协同效果（局内）
    private readonly Dictionary<string, float> _activeSynergyBonuses = new();
    
    public override void _Ready()
    {
        Instance = this;
        RelicSynergyDatabase.Initialize();
        SubscribeRelicSignals();
        GD.Print("[RelicSynergySystem] Initialized");
    }
    
    #region 信号订阅
    
    private void SubscribeRelicSignals()
    {
        // 延迟订阅，等待 RelicSystem 初始化
        if (RelicSystem.Instance == null)
        {
            GD.Print("[RelicSynergySystem] RelicSystem not ready, retrying in 1 second...");
            var timer = new Godot.Timer { OneShot = true, WaitTime = 1.0f };
            AddChild(timer);
            timer.Timeout += () => {
                timer.QueueFree();
                SubscribeRelicSignals();
            };
            timer.Start();
            return;
        }
        
        RelicSystem.Instance.Connect("RelicEquipped", Callable.From<string>(OnRelicEquipped));
        RelicSystem.Instance.Connect("RelicUnequipped", Callable.From<string>(OnRelicUnequipped));
        GD.Print("[RelicSynergySystem] Subscribed to RelicSystem signals");
    }
    
    #endregion
    
    #region 核心检测逻辑
    
    /// <summary>
    /// 当遗物被装备时检测协同
    /// </summary>
    private void OnRelicEquipped(string relicId)
    {
        CheckAndActivateSynergies();
    }
    
    /// <summary>
    /// 当遗物被卸下时检测并移除失效协同
    /// </summary>
    private void OnRelicUnequipped(string relicId)
    {
        DeactivateSynergiesInvolving(relicId);
    }
    
    /// <summary>
    /// 检查当前装备遗物是否形成协同，并触发发现/激活事件
    /// </summary>
    private void CheckAndActivateSynergies()
    {
        if (RelicSystem.Instance == null) return;
        
        var equipped = RelicSystem.Instance.GetEquippedRelics();
        var equippedIds = new List<string>();
        foreach (var r in equipped)
            equippedIds.Add(r.Id);
        
        // 检查所有已知协同
        var allSynergies = RelicSynergyDatabase.GetAllSynergies();
        foreach (var kvp in allSynergies)
        {
            var synergy = kvp.Value;
            
            // 跳过本局已发现或已激活的协同
            if (_playerData.DiscoveredThisRun.Contains(synergy.SynergyId))
                continue;
            
            // 检查是否所有协同遗物都已装备
            bool allEquipped = true;
            foreach (var reqRelicId in synergy.RelicIds)
            {
                if (!equippedIds.Contains(reqRelicId))
                {
                    allEquipped = false;
                    break;
                }
            }
            
            if (!allEquipped) continue;
            
            // 发现新协同！
            DiscoverSynergy(synergy);
        }
        
        // 更新当前活跃协同列表（用于 GetActiveSynergyBonus）
        RefreshActiveSynergies(equippedIds, allSynergies);
    }
    
    /// <summary>
    /// 触发协同发现流程
    /// </summary>
    private void DiscoverSynergy(RelicSynergyEntry synergy)
    {
        // 记录发现
        _playerData.DiscoveredThisRun.Add(synergy.SynergyId);
        _playerData.AllTimeDiscoveries.Add(synergy.SynergyId);
        
        // 发射发现信号（UI 订阅此信号显示通知）
        EmitSignal(nameof(SynergyDiscovered), synergy.SynergyId, synergy.DiscoveryMessage);
        
        // 通知 NarrativeLog 记录发现事件
        NotifyNarrativeLog(synergy);
        
        // 激活协同效果
        ActivateSynergyEffect(synergy);
        
        GD.Print($"[RelicSynergySystem] ✨ Synergy discovered: {synergy.SynergyName}");
    }
    
    /// <summary>
    /// 激活协同效果（局内生效）
    /// </summary>
    private void ActivateSynergyEffect(RelicSynergyEntry synergy)
    {
        if (!_activeSynergyBonuses.ContainsKey(synergy.SynergyId))
            _activeSynergyBonuses[synergy.SynergyId] = synergy.BonusValue;
        
        _playerData.ActiveSynergyIds.Add(synergy.SynergyId);
        
        // 通知战斗系统加成
        EmitSignal(nameof(SynergyActivated), synergy.SynergyId, synergy.BonusType, synergy.BonusValue);
        
        GD.Print($"[RelicSynergySystem] Activated synergy: {synergy.SynergyId} ({synergy.BonusType} +{synergy.BonusValue:P0})");
    }
    
    /// <summary>
    /// 当某遗物被卸下时，移除涉及它的协同效果
    /// </summary>
    private void DeactivateSynergiesInvolving(string relicId)
    {
        var equipped = RelicSystem.Instance.GetEquippedRelics();
        var equippedIds = new HashSet<string>();
        foreach (var r in equipped)
            equippedIds.Add(r.Id);
        
        // 重新检测哪些协同仍然满足条件
        var toRemove = new List<string>();
        foreach (var synergyId in _playerData.ActiveSynergyIds)
        {
            var synergy = RelicSynergyDatabase.GetSynergy(synergyId);
            if (synergy == null) continue;
            
            bool stillActive = true;
            foreach (var reqRelicId in synergy.RelicIds)
            {
                if (!equippedIds.Contains(reqRelicId))
                {
                    stillActive = false;
                    break;
                }
            }
            
            if (!stillActive)
            {
                toRemove.Add(synergyId);
            }
        }
        
        foreach (var synergyId in toRemove)
        {
            _activeSynergyBonuses.Remove(synergyId);
            _playerData.ActiveSynergyIds.Remove(synergyId);
            EmitSignal(nameof(SynergyDeactivated), synergyId);
            GD.Print($"[RelicSynergySystem] Deactivated synergy: {synergyId} (relic {relicId} unequipped)");
        }
    }
    
    private void RefreshActiveSynergies(List<string> equippedIds, Dictionary<string, RelicSynergyEntry> allSynergies)
    {
        // 重新检查当前激活的协同是否仍然有效
        var stillActive = new List<string>();
        foreach (var synergyId in _playerData.ActiveSynergyIds)
        {
            if (!allSynergies.TryGetValue(synergyId, out var synergy))
                continue;
            
            bool valid = true;
            foreach (var reqRelicId in synergy.RelicIds)
            {
                if (!equippedIds.Contains(reqRelicId))
                {
                    valid = false;
                    break;
                }
            }
            
            if (valid) stillActive.Add(synergyId);
        }
        
        _playerData.ActiveSynergyIds = stillActive;
    }
    
    #endregion
    
    #region NarrativeLog 集成
    
    private void NotifyNarrativeLog(RelicSynergyEntry synergy)
    {
        // 尝试通知 NarrativeLogSystem
        try
        {
            var narrativeSystem = GetTree().Root.GetNodeOrNull("MainLoop/NarrativeLogSystem");
            if (narrativeSystem != null)
            {
                // 通过反射调用 AddEntry
                var method = narrativeSystem.GetType().GetMethod("AddEntry");
                method?.Invoke(narrativeSystem, new object[] { "synergy_discovery", synergy.DiscoveryMessage, null });
            }
        }
        catch (Exception ex)
        {
            GD.Print($"[RelicSynergySystem] Could not notify NarrativeLog: {ex.Message}");
        }
    }
    
    #endregion
    
    #region 公开 API
    
    /// <summary>
    /// 获取当前活跃协同的总加成（按类型）
    /// </summary>
    public float GetActiveSynergyBonus(string bonusType)
    {
        float total = 0f;
        foreach (var kvp in _activeSynergyBonuses)
        {
            var synergy = RelicSynergyDatabase.GetSynergy(kvp.Key);
            if (synergy != null && synergy.BonusType == bonusType)
                total += kvp.Value;
        }
        return total;
    }
    
    /// <summary>
    /// 获取当前活跃协同列表
    /// </summary>
    public List<string> GetActiveSynergyIds()
    {
        return new List<string>(_playerData.ActiveSynergyIds);
    }
    
    /// <summary>
    /// 获取已发现的所有协同
    /// </summary>
    public HashSet<string> GetAllTimeDiscoveries()
    {
        return new HashSet<string>(_playerData.AllTimeDiscoveries);
    }
    
    /// <summary>
    /// 获取本局已发现的协同
    /// </summary>
    public HashSet<string> GetDiscoveredThisRun()
    {
        return new HashSet<string>(_playerData.DiscoveredThisRun);
    }
    
    /// <summary>
    /// 获取协同条目详情
    /// </summary>
    public RelicSynergyEntry GetSynergyDetails(string synergyId)
    {
        return RelicSynergyDatabase.GetSynergy(synergyId);
    }
    
    /// <summary>
    /// 是否有新发现的协同（用于 UI 提示）
    /// </summary>
    public bool HasNewDiscovery()
    {
        return _playerData.DiscoveredThisRun.Count > 0;
    }
    
    /// <summary>
    /// 重置本局协同发现（用于新游戏）
    /// </summary>
    public void ResetForNewRun()
    {
        _playerData.DiscoveredThisRun.Clear();
        _playerData.ActiveSynergyIds.Clear();
        _activeSynergyBonuses.Clear();
    }
    
    #endregion
    
    #region 持久化
    
    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary<string, object>
        {
            { "all_time_discoveries", new List<string>(_playerData.AllTimeDiscoveries) }
        };
    }
    
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.TryGetValue("all_time_discoveries", out var discoveries))
        {
            var list = (Array)discoveries;
            _playerData.AllTimeDiscoveries.Clear();
            foreach (var item in list)
                _playerData.AllTimeDiscoveries.Add(item.ToString());
        }
        
        GD.Print($"[RelicSynergySystem] Loaded {_playerData.AllTimeDiscoveries.Count} all-time discoveries");
    }
    
    #endregion
}
