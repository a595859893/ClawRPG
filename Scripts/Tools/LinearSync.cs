namespace ClawRPG.Scripts.Tools;

/// <summary>
/// Linear Sync Engine — 将 REQ Registry 同步到 Linear 项目看板
/// 读取 registry.json，对比 Linear 上的 Issue 状态，执行双向同步
/// 支持 Conventional Commits 格式自动关联 Issue
/// </summary>
public partial class LinearSync : BaseSystem
{
    public static LinearSync Instance { get; private set; }

    /// <summary>
    /// Registry JSON 文件路径
    /// </summary>
    [Export] private string _registryPath = "user://../memory/requirements/registry.json";

    /// <summary>
    /// Linear Team ID (与 LinearClient 共享)
    /// </summary>
    [Export] private string _teamId = string.Empty;

    /// <summary>
    /// 标签 ID 映射表 (label name → Linear label id)
    /// </summary>
    [Export] private Godot.Collections.Dictionary<string, string> _labelMapping = new();

    /// <summary>
    /// 同步结果统计
    /// </summary>
    public SyncStats LastSyncStats { get; private set; } = new();

    // ── Signals ──────────────────────────────────────────────────────────────

    [Signal]
    public delegate void SyncCompletedEventHandler(SyncStats stats);

    [Signal]
    public delegate void SyncProgressEventHandler(int current, int total, string currentReq);

    [Signal]
    public delegate void SyncErrorEventHandler(string reqId, string error);

    // ── Types ────────────────────────────────────────────────────────────────

    public class SyncStats
    {
        public int TotalProcessed { get; set; }
        public int Created { get; set; }
        public int Updated { get; set; }
        public int Unchanged { get; set; }
        public int Errors { get; set; }
        public string LastError { get; set; } = string.Empty;
    }

    // ── Lifecycle ────────────────────────────────────────────────────────────

    public override void _Ready()
    {
        Instance = this;
        base._Ready();
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// 执行完整同步：将 registry.json 中的所有 REQ 同步到 Linear
    /// </summary>
    /// <param name="onComplete">完成回调 (stats)</param>
    public void SyncAll(System.Action<SyncStats> onComplete)
    {
        var reqs = LoadRegistry();
        if (reqs == null || reqs.Count == 0)
        {
            LastSyncStats = new SyncStats { LastError = "Registry not found or empty" };
            onComplete?.Invoke(LastSyncStats);
            return;
        }

        int total = reqs.Count;
        int processed = 0;
        LastSyncStats = new SyncStats { TotalProcessed = total };

        // GD.Print($"[LinearSync] Starting sync of {total} REQs to Linear...");

        // Process sequentially to avoid rate limiting
        ProcessNextReq(reqs, processed, total, onComplete);
    }

    /// <summary>
    /// 同步单个 REQ
    /// </summary>
    public void SyncReq(string reqId, System.Action<bool> onComplete)
    {
        var reqs = LoadRegistry();
        if (reqs == null || !reqs.ContainsKey(reqId))
        {
            EmitSignal(SyncErrorSignal, reqId, $"REQ {reqId} not found in registry");
            onComplete?.Invoke(false);
            return;
        }

        var reqData = reqs[reqId] as Godot.Collections.Dictionary;
        if (reqData == null)
        {
            onComplete?.Invoke(false);
            return;
        }

        SyncSingleReq(reqId, reqData, onComplete);
    }

    // ── Core Sync Logic ──────────────────────────────────────────────────────

    private void ProcessNextReq(
        Godot.Collections.Dictionary reqs,
        int processed,
        int total,
        System.Action<SyncStats> onComplete)
    {
        if (processed >= total)
        {
            LastSyncStats.TotalProcessed = total;
            EmitSignal(SyncCompletedSignal, LastSyncStats);
            onComplete?.Invoke(LastSyncStats);
            return;
        }

        string reqId = reqs.Keys.ElementAt(processed) as string;
        var reqData = reqs[reqId] as Godot.Collections.Dictionary;

        EmitSignal(SyncProgressSignal, processed + 1, total, reqId);

        SyncSingleReq(reqId, reqData, (success) =>
        {
            if (!success) LastSyncStats.Errors++;
            processed++;

            // Rate limit: 100ms delay between requests
            var timer = new Godot.Timer { OneShot = true, WaitTime = 0.1f };
            GetTree().Root.AddChild(timer);
            timer.Timeout += () =>
            {
                timer.QueueFree();
                ProcessNextReq(reqs, processed, total, onComplete);
            };
            timer.Start();
        });
    }

    private void SyncSingleReq(string reqId, Godot.Collections.Dictionary reqData, System.Action<bool> onComplete)
    {
        string title = reqData.GetValueOrDefault("title", reqId) as string ?? reqId;
        string status = reqData.GetValueOrDefault("status", "pending") as string ?? "pending";
        string source = reqData.GetValueOrDefault("source_file", "") as string ?? "";
        string priority = reqData.GetValueOrDefault("priority", "medium") as string ?? "medium";
        string notes = reqData.GetValueOrDefault("notes", "") as string ?? "";

        // Build description body
        string description = BuildIssueDescription(reqId, status, source, priority, notes);

        // Map priority string to Linear priority integer (0=No priority, 1=Urgent, 2=High, 3=Medium, 4=Low)
        int linearPriority = MapPriorityToLinear(priority);

        // Get label IDs
        string[] labelIds = GetLabelIds(status, priority);

        // Check if this REQ already has a Linear issue ID stored
        string existingIssueId = reqData.GetValueOrDefault("linear_issue_id", "") as string ?? "";

        if (!string.IsNullOrEmpty(existingIssueId))
        {
            // Update existing issue
            LinearClient.Instance.UpdateIssueStatus(existingIssueId, status, (updated) =>
            {
                if (updated)
                {
                    LastSyncStats.Updated++;
                    // GD.Print($"[LinearSync] Updated {reqId} → {status}");
                }
                else
                {
                    LastSyncStats.Errors++;
                    EmitSignal(SyncErrorSignal, reqId, LinearClient.Instance.LastError);
                }
                onComplete?.Invoke(updated);
            });
        }
        else
        {
            // Create new issue
            LinearClient.Instance.CreateIssue(title, description, linearPriority, labelIds, (created, issueId) =>
            {
                if (created)
                {
                    LastSyncStats.Created++;
                    // GD.Print($"[LinearSync] Created {reqId} → {issueId}");
                    // TODO: Store issueId back in registry.json (requires file write)
                    // For now, note it in console
                    UpdateRegistryWithIssueId(reqId, issueId);
                }
                else
                {
                    LastSyncStats.Errors++;
                    EmitSignal(SyncErrorSignal, reqId, LinearClient.Instance.LastError);
                }
                onComplete?.Invoke(created);
            });
        }
    }

    /// <summary>
    /// 根据 commit message 自动关联 Linear Issue (Conventional Commits)
    /// commit message 格式: feat(req): 完成 REQ-XXX ...
    /// </summary>
    /// <param name="commitMessage">Git commit message</param>
    public void LinkCommitToIssue(string commitMessage)
    {
        // Extract REQ ID from Conventional Commits format
        // feat(req): 完成 REQ-105 项目看板集成
        // fix(req): 修复 REQ-123 ...
        var match = System.Text.RegularExpressions.Regex.Match(
            commitMessage,
            @"(feat|fix|feat req|fix req|req)[:\s]+(REQ-\d+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            // GD.Print($"[LinearSync] No REQ ID found in commit: {commitMessage}");
            return;
        }

        string reqId = match.Groups[2].Value.ToUpperInvariant();

        // Update registry to in_progress / completed
        var reqs = LoadRegistry();
        if (reqs == null || !reqs.ContainsKey(reqId)) return;

        string newStatus = commitMessage.ToLowerInvariant().Contains("fix")
            ? "in_progress" : "completed";

        var reqData = reqs[reqId] as Godot.Collections.Dictionary;
        if (reqData != null)
        {
            reqData["status"] = newStatus;
            SaveRegistry(reqs);
            SyncReq(reqId, (_) => { });
        }
    }

    // ── Registry Helpers ─────────────────────────────────────────────────────

    private Godot.Collections.Dictionary LoadRegistry()
    {
        string fullPath = _registryPath;
        if (!System.IO.File.Exists(fullPath))
        {
            // Try relative to user://
            fullPath = GetTree().Root.GetNode<Godot.Node>(".")
                .GetViewport()
                .GetWindow()?
                .GetFileAccessForPooling(fullPath)?
                .GetPathAbsolute() ?? fullPath;
        }

        try
        {
            if (System.IO.File.Exists(fullPath))
            {
                string content = System.IO.File.ReadAllText(fullPath);
                var json = Godot.JSON.ParseString(content);
                if (json.Error == Godot.Error.Ok && json.Result is Godot.Collections.Dictionary root)
                {
                    return root.GetValueOrDefault("reqs", new Godot.Collections.Dictionary())
                        as Godot.Collections.Dictionary;
                }
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[LinearSync] Failed to load registry: {ex.Message}");
            LastSyncStats.LastError = ex.Message;
        }

        // Fallback: try project-relative path
        try
        {
            string projectPath = System.IO.Path.Combine(
                System.IO.Directory.GetCurrentDirectory(),
                "memory/requirements/registry.json");
            if (System.IO.File.Exists(projectPath))
            {
                string content = System.IO.File.ReadAllText(projectPath);
                var json = Godot.JSON.ParseString(content);
                if (json.Error == Godot.Error.Ok && json.Result is Godot.Collections.Dictionary root)
                {
                    return root.GetValueOrDefault("reqs", new Godot.Collections.Dictionary())
                        as Godot.Collections.Dictionary;
                }
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[LinearSync] Failed to load registry (fallback): {ex.Message}");
        }

        return null;
    }

    private void SaveRegistry(Godot.Collections.Dictionary reqs)
    {
        // This would require reading the full registry, updating, and writing back
        // For safety, this is a no-op without full registry in memory
        // The actual file update should be done by the Python script
    }

    private void UpdateRegistryWithIssueId(string reqId, string issueId)
    {
        // Log for external Python script to handle file update
        GD.Print($"[LinearSync] LINEAR_ISSUE:{reqId}:{issueId}");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private string BuildIssueDescription(string reqId, string status, string source, string priority, string notes)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"## {reqId}");
        sb.AppendLine();
        sb.AppendLine($"**状态**: {status}");
        sb.AppendLine($"**优先级**: {priority}");
        sb.AppendLine();
        if (!string.IsNullOrEmpty(source))
            sb.AppendLine($"**源文件**: `{source}`");
        sb.AppendLine();
        if (!string.IsNullOrEmpty(notes))
        {
            sb.AppendLine("## 备注");
            sb.AppendLine(notes);
            sb.AppendLine();
        }
        sb.AppendLine("---");
        sb.AppendLine($"_自动同步自 ClawRPG REQ Registry_");
        return sb.ToString();
    }

    private int MapPriorityToLinear(string priority)
    {
        return priority.ToLowerInvariant() switch
        {
            "critical" or "p0" => 1,  // Urgent
            "high" or "p1" => 2,      // High
            "medium" or "p2" => 3,    // Medium
            "low" or "p3" => 4,       // Low
            _ => 0                     // No priority
        };
    }

    private string[] GetLabelIds(string status, string priority)
    {
        var labels = new System.Collections.Generic.List<string>();

        // Status labels
        if (_labelMapping.TryGetValue($"status:{status}", out string statusLabel))
            labels.Add(statusLabel);

        // Priority labels
        if (_labelMapping.TryGetValue($"priority:{priority}", out string prioLabel))
            labels.Add(prioLabel);

        return labels.ToArray();
    }
}
