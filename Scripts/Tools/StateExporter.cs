namespace ClawRPG.Scripts.Tools;

using System.Collections.Generic;
using Godot;

/// <summary>
/// 运行时状态导出器 - 将任意节点树状态 dump 为 JSON
/// 用于 headless 测试后的结果分析
/// </summary>
public partial class StateExporter : BaseSystem
{
    public static StateExporter Instance { get; private set; }

    /// <summary>
    /// 导出文件路径 (user:// 相对路径)
    /// </summary>
    [Export] private string _exportPath = "state_exports/runtime_state.json";

    /// <summary>
    /// 是否自动导出 (每次游戏暂停/切换场景时)
    /// </summary>
    [Export] private bool _autoExportOnPause = false;

    /// <summary>
    /// 是否在游戏结束时自动导出
    /// </summary>
    [Export] private bool _autoExportOnGameOver = true;

    /// <summary>
    /// 导出完成信号
    /// </summary>
    [Signal] public delegate void ExportCompletedEventHandler(string filePath);
    [Signal] public delegate void ExportFailedEventHandler(string reason);

    public override void _Ready()
    {
        Instance = this;
        base._Ready();

        if (_autoExportOnGameOver)
        {
            // 连接游戏结束信号 (根据实际信号名称调整)
            var gs = GetTree().Root.GetNodeOrNull("GameState");
            if (gs != null)
            {
                // gs.Connect("game_ended", new Callable(this, nameof(OnGameEnded)));
            }
        }
    }

    /// <summary>
    /// 导出整个场景树的状态
    /// </summary>
    /// <param name="root">根节点，为空则导出整个场景</param>
    /// <param name="maxDepth">最大递归深度，防止过深</param>
    public string ExportTree(Node root = null, int maxDepth = 10)
    {
        var target = root ?? GetTree().Root;
        var state = NodeToDictionary(target, 0, maxDepth);
        return SaveToFile(state);
    }

    /// <summary>
    /// 导出指定系统/节点的状态
    /// </summary>
    public string ExportSystem(string nodePath)
    {
        var node = GetTree().Root.GetNodeOrNull(nodePath);
        if (node == null)
        {
            EmitSignal(SignalName.ExportFailed, $"Node not found: {nodePath}");
            return "";
        }
        var state = NodeToDictionary(node, 0, 10);
        return SaveToFile(state);
    }

    /// <summary>
    /// 导出多个指定节点 (支持 glob 路径)
    /// </summary>
    public string ExportSystems(string[] nodePaths)
    {
        var result = new Dictionary<string, object>();
        foreach (var path in nodePaths)
        {
            var node = GetTree().Root.GetNodeOrNull(path);
            if (node != null)
                result[path] = NodeToDictionary(node, 0, 8);
            else
                result[path] = new Dictionary<string, object> { { "_error", $"not found: {path}" } };
        }
        return SaveToFile(result);
    }

    /// <summary>
    /// 导出关键战斗数据
    /// </summary>
    public string ExportCombatState()
    {
        var state = new Dictionary<string, object>
        {
            ["timestamp"] = Time.GetDatetimeStringFromSystem(),
            ["scene"] = GetTree().CurrentScene?.Name ?? "unknown",
        };

        // 导出 Player 状态
        var player = GetTree().Root.GetNodeOrNull("Player");
        if (player != null)
        {
            state["player"] = NodeToDictionary(player, 0, 5);
        }

        // 导出 CombatUI 状态
        var combatUI = GetTree().Root.GetNodeOrNull("CombatUI");
        if (combatUI != null)
        {
            state["combatUI"] = NodeToDictionary(combatUI, 0, 4);
        }

        // 导出所有 Boss
        var bosses = GetTree().Root.GetTreeNodesInGroup("bosses");
        if (bosses.Count > 0)
        {
            var bossList = new List<Dictionary<string, object>>();
            foreach (var b in bosses)
                bossList.Add(NodeToDictionary(b, 0, 5));
            state["bosses"] = bossList;
        }

        return SaveToFile(state);
    }

    /// <summary>
    /// 导出完整游戏状态快照
    /// </summary>
    public string ExportFullSnapshot()
    {
        var state = new Dictionary<string, object>
        {
            ["export_time"] = Time.GetDatetimeStringFromSystem(),
            ["tick"] = Time.GetTicksMsec(),
            ["scene"] = GetTree().CurrentScene?.Name ?? "unknown",
            ["paused"] = GetTree().Paused,
            ["frame"] = Engine.GetFramesDrawn(),
            ["fps"] = Engine.GetFramesPerSecond(),
            ["root_children"] = GetTree().Root.GetChildCount(),
        };

        // 遍历所有 BaseSystem 子类，收集 ExportSaveData
        var systemsData = new Dictionary<string, object>();
        foreach (var child in GetTree().Root.GetChildren())
        {
            if (child is Framework.BaseSystem sys)
            {
                try
                {
                    var saveData = sys.ExportSaveData();
                    systemsData[sys.Name] = saveData ?? new Dictionary<string, object>();
                }
                catch (System.Exception ex)
                {
                    systemsData[child.Name] = new Dictionary<string, object> { { "_error", ex.Message } };
                }
            }
        }
        state["systems"] = systemsData;

        return SaveToFile(state);
    }

    /// <summary>
    /// 核心递归：将节点转为 Dictionary
    /// </summary>
    private Dictionary<string, object> NodeToDictionary(Node node, int depth, int maxDepth)
    {
        var dict = new Dictionary<string, object>
        {
            ["type"] = node.GetType().Name,
            ["path"] = node.GetPath().ToString(),
            ["name"] = node.Name,
        };

        if (depth >= maxDepth)
        {
            dict["_truncated"] = true;
            return dict;
        }

        // 导出关键属性 (可通过反射或 Attribute 扩展)
        ExportNodeProperties(node, dict);

        // 递归导出子节点
        if (node.GetChildCount() > 0)
        {
            var children = new List<Dictionary<string, object>>();
            foreach (Node child in node.GetChildren())
            {
                // 跳过隐藏节点以减少噪音
                if (child.Name.BeginsWith("@") || child.Name.BeginsWith("_")) continue;
                children.Add(NodeToDictionary(child, depth + 1, maxDepth));
            }
            if (children.Count > 0)
                dict["children"] = children;
        }

        return dict;
    }

    /// <summary>
    /// 导出节点的关键属性 (可扩展)
    /// </summary>
    private void ExportNodeProperties(Node node, Dictionary<string, object> dict)
    {
        // 根据节点类型导出有意义的信息
        switch (node)
        {
            case CharacterBody2D body:
                dict["position"] = body.Position;
                dict["velocity"] = body.Velocity;
                break;
            case Node2D n2d:
                dict["position"] = n2d.Position;
                dict["rotation"] = n2d.RotationDegrees;
                break;
            case Control ctrl:
                dict["size"] = ctrl.Size;
                dict["visible"] = ctrl.Visible;
                break;
        }

        // 尝试导出 Export 属性 (通过反射)
        foreach (var prop in node.GetType().GetProperties())
        {
            if (prop.GetCustomAttributes(typeof(ExportAttribute), true).Length > 0)
            {
                try
                {
                    var val = prop.GetValue(node);
                    dict[$"@{prop.Name}"] = val?.ToString() ?? "null";
                }
                catch { }
            }
        }
    }

    /// <summary>
    /// 将 Dictionary 保存为 JSON 文件
    /// </summary>
    private string SaveToFile(Dictionary<string, object> state)
    {
        try
        {
            var json = Json.Stringify(state);
            var path = ProjectSettings.GlobalizePath($"user://{_exportPath}");

            // 确保目录存在
            var dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            System.IO.File.WriteAllText(path, json);
            GD.Print($"[StateExporter] Exported to: {path}");
            EmitSignal(SignalName.ExportCompleted, path);
            return path;
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[StateExporter] Save failed: {ex.Message}");
            EmitSignal(SignalName.ExportFailed, ex.Message);
            return "";
        }
    }

    private string SaveToFile(object state)
    {
        try
        {
            var json = Json.Stringify(state);
            var path = ProjectSettings.GlobalizePath($"user://{_exportPath}");

            var dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            System.IO.File.WriteAllText(path, json);
            GD.Print($"[StateExporter] Exported to: {path}");
            EmitSignal(SignalName.ExportCompleted, path);
            return path;
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[StateExporter] Save failed: {ex.Message}");
            EmitSignal(SignalName.ExportFailed, ex.Message);
            return "";
        }
    }

    /// <summary>
    /// 读取上一次导出的状态文件
    /// </summary>
    public Dictionary<string, object> LoadLastExport()
    {
        var path = ProjectSettings.GlobalizePath($"user://{_exportPath}");
        if (!System.IO.File.Exists(path))
            return new Dictionary<string, object> { { "_error", "No export file found" } };

        var json = System.IO.File.ReadAllText(path);
        var result = Json.Parse(json);
        return result as Dictionary<string, object>;
    }
}
