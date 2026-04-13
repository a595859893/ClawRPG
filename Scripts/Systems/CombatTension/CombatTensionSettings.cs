using Godot;
using System;

namespace ClawRPG.Systems.CombatTension;

/// <summary>
/// 战斗紧张度设置管理
/// 提供启用/禁用开关，供 GameSettings 或 UISettings 集成
/// </summary>
public partial class CombatTensionSettings : Godot.Node
{
    public static CombatTensionSettings Instance { get; private set; }

    [Export] private bool _combatTensionEnabled = true;
    [Export] private bool _showEdgeOverlay = true;
    [Export] private bool _showParticles = true;
    [Export] private bool _enableBgmLayers = true;
    [Export] private float _sensitivity = 1.0f;  // 紧张度敏感度系数

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>
    /// 获取总开关
    /// </summary>
    public bool IsCombatTensionEnabled() => _combatTensionEnabled;

    /// <summary>
    /// 设置总开关
    /// </summary>
    public void SetCombatTensionEnabled(bool enabled)
    {
        _combatTensionEnabled = enabled;

        // 通知所有战斗紧张度系统
        NotifySettingChanged();
    }

    /// <summary>
    /// 获取边缘叠加层开关
    /// </summary>
    public bool IsEdgeOverlayEnabled() => _showEdgeOverlay;

    /// <summary>
    /// 设置边缘叠加层开关
    /// </summary>
    public void SetEdgeOverlayEnabled(bool enabled)
    {
        _showEdgeOverlay = enabled;
        NotifySettingChanged();
    }

    /// <summary>
    /// 获取粒子背景开关
    /// </summary>
    public bool IsParticlesEnabled() => _showParticles;

    /// <summary>
    /// 设置粒子背景开关
    /// </summary>
    public void SetParticlesEnabled(bool enabled)
    {
        _showParticles = enabled;
        NotifySettingChanged();
    }

    /// <summary>
    /// 获取 BGM 分层开关
    /// </summary>
    public bool IsBgmLayersEnabled() => _enableBgmLayers;

    /// <summary>
    /// 设置 BGM 分层开关
    /// </summary>
    public void SetBgmLayersEnabled(bool enabled)
    {
        _enableBgmLayers = enabled;
        NotifySettingChanged();
    }

    /// <summary>
    /// 获取敏感度
    /// </summary>
    public float GetSensitivity() => _sensitivity;

    /// <summary>
    /// 设置敏感度
    /// </summary>
    public void SetSensitivity(float sensitivity)
    {
        _sensitivity = Mathf.Clamp(sensitivity, 0.1f, 2.0f);
        NotifySettingChanged();
    }

    private void NotifySettingChanged()
    {
        // 通知 CombatTensionSystem 设置已更改
        var tensionSystem = GetNodeOrNull<Godot.Node>("/root/CombatTensionSystem");
        if (tensionSystem != null && tensionSystem is CombatTensionSystem cts)
        {
            cts.SetEnabled(_combatTensionEnabled);
        }

        // 通知 CombatTensionOverlay 设置已更改
        var overlay = GetNodeOrNull<CombatTensionOverlay>("../CombatTensionOverlay");
        if (overlay != null)
        {
            overlay.SetEnabled(_combatTensionEnabled && _showEdgeOverlay);
        }
    }

    // ========== 持久化 ==========

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public Godot.Collections.Dictionary ExportSaveData()
    {
        return new Godot.Collections.Dictionary
        {
            ["combatTensionEnabled"] = _combatTensionEnabled,
            ["showEdgeOverlay"] = _showEdgeOverlay,
            ["showParticles"] = _showParticles,
            ["enableBgmLayers"] = _enableBgmLayers,
            ["sensitivity"] = _sensitivity
        };
    }

    /// <summary>
    /// 导入保存数据
    /// </summary>
    public void ImportSaveData(Godot.Collections.Dictionary data)
    {
        if (data == null) return;

        if (data.ContainsKey("combatTensionEnabled"))
            _combatTensionEnabled = (bool)data["combatTensionEnabled"];
        if (data.ContainsKey("showEdgeOverlay"))
            _showEdgeOverlay = (bool)data["showEdgeOverlay"];
        if (data.ContainsKey("showParticles"))
            _showParticles = (bool)data["showParticles"];
        if (data.ContainsKey("enableBgmLayers"))
            _enableBgmLayers = (bool)data["enableBgmLayers"];
        if (data.ContainsKey("sensitivity"))
            _sensitivity = (float)data["sensitivity"];

        NotifySettingChanged();
    }
}
