using Godot;
using System;
using System.Collections.Generic;

public partial class MilestoneMain : BaseSystem
{
    public static MilestoneSystem MilestoneSystem { get; private set; }
    public static MilestoneUI MilestoneUI { get; private set; }
    
    public override void _Ready()
    {
        MilestoneSystem = new MilestoneSystem();
        GD.Print("Milestone System initialized");
    }
    
    public static void ToggleMilestoneUI()
    {
        var sceneTree = Engine.GetSingleton("Engine").GetMainLoop() as SceneTree;
        if (sceneTree == null) return;
        
        var root = sceneTree.Root;
        
        // Find existing UI
        foreach (Node child in root.GetChildren())
        {
            if (child is MilestoneUI)
            {
                child.QueueFree();
                return;
            }
        }
        
        // Create new UI
        var ui = new MilestoneUI();
        root.AddChild(ui);
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        // MilestoneMain 是容器系统，无持久化状态
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        // MilestoneMain 是容器系统，无持久化状态
    }
}
