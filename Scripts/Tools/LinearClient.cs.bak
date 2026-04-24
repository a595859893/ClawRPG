namespace ClawRPG.Scripts.Tools;

/// <summary>
/// Linear API Client — 项目看板集成
/// 封装 Linear REST API，支持 Issue 创建、状态更新、标签管理
/// 鉴权：Bearer Token 从 ~/.config/openclaw/linear_key 读取
/// </summary>
public partial class LinearClient : BaseSystem
{
    public static LinearClient Instance { get; private set; }

    /// <summary>
    /// Linear API Base URL (GraphQL endpoint)
    /// </summary>
    [Export] private string _apiUrl = "https://api.linear.app/graphql";

    /// <summary>
    /// Personal API Key (从文件加载)
    /// </summary>
    private string _apiKey = string.Empty;

    /// <summary>
    /// Linear Team ID (用于创建 Issue，格式：<team-id>)
    /// </summary>
    [Export] private string _teamId = string.Empty;

    /// <summary>
    /// 最近一次 API 调用的错误信息
    /// </summary>
    public string LastError { get; private set; } = string.Empty;

    /// <summary>
    /// API Key 加载是否成功
    /// </summary>
    public bool IsAuthenticated => !string.IsNullOrEmpty(_apiKey);

    // ── Signals ────────────────────────────────────────────────────────────────

    [Signal]
    public delegate void IssueCreatedEventHandler(string issueId, string title);

    [Signal]
    public delegate void IssueUpdatedEventHandler(string issueId, string newStatus);

    [Signal]
    public delegate void LabelAddedEventHandler(string issueId, string label);

    [Signal]
    public delegate void ApiErrorEventHandler(string error);

    // ── Lifecycle ────────────────────────────────────────────────────────────

    public override void _Ready()
    {
        Instance = this;
        base._Ready();
        LoadApiKey();
    }

    /// <summary>
    /// 从 ~/.config/openclaw/linear_key 加载 API Key
    /// </summary>
    private void LoadApiKey()
    {
        string keyPath = System.IO.Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
            ".config", "openclaw", "linear_key"
        );

        try
        {
            if (System.IO.File.Exists(keyPath))
            {
                _apiKey = System.IO.File.ReadAllText(keyPath).Trim();
                if (string.IsNullOrEmpty(_apiKey))
                    GD.PrintErr("[LinearClient] API key file is empty.");
            }
            else
            {
                GD.PrintErr($"[LinearClient] API key not found at: {keyPath}");
                LastError = $"API key file not found: {keyPath}";
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"[LinearClient] Failed to load API key: {ex.Message}");
            LastError = ex.Message;
        }
    }

    // ── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// 在 Linear 中创建一个 Issue
    /// </summary>
    /// <param name="title">Issue 标题</param>
    /// <param name="description">Issue 描述 (支持 Markdown)</param>
    /// <param name="priority">优先级 0-4</param>
    /// <param name="labelIds">标签 ID 列表</param>
    /// <param name="onComplete">完成回调 (success, issueId)</param>
    public void CreateIssue(
        string title,
        string description,
        int priority,
        string[] labelIds,
        System.Action<bool, string> onComplete)
    {
        if (!IsAuthenticated)
        {
            LastError = "Not authenticated. Check API key.";
            EmitSignal(ApiErrorSignal, LastError);
            onComplete?.Invoke(false, LastError);
            return;
        }

        string labelIdsJson = "[]";
        if (labelIds != null && labelIds.Length > 0)
        {
            var items = new System.Text.StringBuilder("[");
            for (int i = 0; i < labelIds.Length; i++)
            {
                items.Append($"\"{labelIds[i]}\"");
                if (i < labelIds.Length - 1) items.Append(",");
            }
            items.Append("]");
            labelIdsJson = items.ToString();
        }

        string graphql = @"
            mutation IssueCreate($title: String!, $body: String!, $teamId: String!, $priority: Int, $labelIds: [String!]) {
                issueCreate(input: {
                    title: $title,
                    description: $body,
                    teamId: $teamId,
                    priority: $priority,
                    labelIds: $labelIds
                }) {
                    success
                    issue {
                        id
                        identifier
                        title
                    }
                }
            }";

        string variables = $"{{\"title\":\"{EscapeJson(title)}\",\"body\":\"{EscapeJson(description)}\",\"teamId\":\"{_teamId}\",\"priority\":{priority},\"labelIds\":{labelIdsJson}}}";

        SendGraphQLRequest(graphql, variables, (success, response) =>
        {
            if (!success)
            {
                onComplete?.Invoke(false, LastError);
                return;
            }

            // 解析 issue id from response
            string issueId = ExtractJsonString(response, "id");
            string identifier = ExtractJsonString(response, "identifier");
            if (!string.IsNullOrEmpty(issueId))
            {
                EmitSignal(IssueCreatedSignal, issueId, title);
                onComplete?.Invoke(true, issueId);
            }
            else
            {
                string err = ExtractJsonString(response, "message");
                if (string.IsNullOrEmpty(err)) err = "Failed to parse issue ID from response";
                LastError = err;
                EmitSignal(ApiErrorSignal, err);
                onComplete?.Invoke(false, err);
            }
        });
    }

    /// <summary>
    /// 更新 Linear Issue 的状态
    /// </summary>
    /// <param name="issueId">Linear Issue ID</param>
    /// <param name="status">状态: Backlog | Ready | In Progress | Done</param>
    /// <param name="onComplete">完成回调</param>
    public void UpdateIssueStatus(string issueId, string status, System.Action<bool> onComplete)
    {
        if (!IsAuthenticated)
        {
            LastError = "Not authenticated.";
            EmitSignal(ApiErrorSignal, LastError);
            onComplete?.Invoke(false);
            return;
        }

        string stateName = MapStatusToLinearState(status);
        string graphql = @"
            mutation UpdateIssueState($issueId: String!, $stateName: String!) {
                issueUpdate(id: $issueId, input: { stateName: $stateName }) {
                    success
                }
            }";

        string variables = $"{{\"issueId\":\"{issueId}\",\"stateName\":\"{stateName}\"}}";

        SendGraphQLRequest(graphql, variables, (success, response) =>
        {
            if (success)
            {
                EmitSignal(IssueUpdatedSignal, issueId, status);
                onComplete?.Invoke(true);
            }
            else
            {
                EmitSignal(ApiErrorSignal, LastError);
                onComplete?.Invoke(false);
            }
        });
    }

    /// <summary>
    /// 为 Issue 添加标签
    /// </summary>
    /// <param name="issueId">Linear Issue ID</param>
    /// <param name="labelId">Linear Label ID</param>
    /// <param name="onComplete">完成回调</param>
    public void AddLabel(string issueId, string labelId, System.Action<bool> onComplete)
    {
        if (!IsAuthenticated)
        {
            LastError = "Not authenticated.";
            EmitSignal(ApiErrorSignal, LastError);
            onComplete?.Invoke(false);
            return;
        }

        string graphql = @"
            mutation AddLabel($issueId: String!, $labelId: String!) {
                issueAddLabel(id: $issueId, labelId: $labelId) {
                    success
                }
            }";

        string variables = $"{{\"issueId\":\"{issueId}\",\"labelId\":\"{labelId}\"}}";

        SendGraphQLRequest(graphql, variables, (success, response) =>
        {
            if (success)
            {
                EmitSignal(LabelAddedSignal, issueId, labelId);
                onComplete?.Invoke(true);
            }
            else
            {
                EmitSignal(ApiErrorSignal, LastError);
                onComplete?.Invoke(false);
            }
        });
    }

    /// <summary>
    /// 获取 Linear Issue 的当前状态
    /// </summary>
    /// <param name="issueId">Linear Issue ID</param>
    /// <param name="onComplete">回调 (success, status)</param>
    public void GetIssueStatus(string issueId, System.Action<bool, string> onComplete)
    {
        if (!IsAuthenticated)
        {
            LastError = "Not authenticated.";
            EmitSignal(ApiErrorSignal, LastError);
            onComplete?.Invoke(false, LastError);
            return;
        }

        string graphql = @"
            query GetIssue($issueId: String!) {
                issue(id: $issueId) {
                    id
                    identifier
                    title
                    state {
                        name
                    }
                }
            }";

        string variables = $"{{\"issueId\":\"{issueId}\"}}";

        SendGraphQLRequest(graphql, variables, (success, response) =>
        {
            if (success)
            {
                string stateName = ExtractJsonNestedString(response, "state", "name");
                onComplete?.Invoke(true, stateName);
            }
            else
            {
                onComplete?.Invoke(false, LastError);
            }
        });
    }

    // ── HTTP Request Layer ────────────────────────────────────────────────────

    /// <summary>
    /// 发送 GraphQL 请求到 Linear API
    /// </summary>
    private void SendGraphQLRequest(string query, string variables, System.Action<bool, string> onComplete)
    {
        // 创建临时 HTTPRequest node (Godot way)
        var httpNode = new Godot.HTTPRequest();
        GetTree().Root.AddChild(httpNode);
        httpNode.Name = $"LinearAPI_{System.Guid.NewGuid():N}";

        // 序列化请求体
        string jsonBody = $"{{\"query\":\"{EscapeJson(query)}\",\"variables\":{variables}}}";

        string[] headers = [
            "Content-Type: application/json",
            $"Authorization: {_apiKey}"
        ];

        httpNode.RequestCompleted += (result, responseCode, headersArray, body) =>
        {
            string responseText = System.Text.Encoding.UTF8.GetString(body);
            httpNode.QueueFree();

            if (responseCode != 200)
            {
                LastError = $"HTTP {responseCode}: {responseText}";
                GD.PrintErr($"[LinearClient] API error: {LastError}");
                onComplete?.Invoke(false, responseText);
                return;
            }

            // 检查 GraphQL errors 字段
            string errors = ExtractJsonString(responseText, "errors");
            if (!string.IsNullOrEmpty(errors))
            {
                LastError = $"GraphQL error: {errors}";
                GD.PrintErr($"[LinearClient] GraphQL error: {errors}");
                onComplete?.Invoke(false, errors);
                return;
            }

            // 提取 data 字段
            string data = ExtractJsonString(responseText, "data");
            if (string.IsNullOrEmpty(data))
            {
                LastError = "Empty data in response";
                onComplete?.Invoke(false, LastError);
                return;
            }

            onComplete?.Invoke(true, data);
        };

        httpNode.Request(_apiUrl, headers, Godot.HTTPClient.Method.Post, jsonBody);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// 将 REQ 状态映射到 Linear State 名称
    /// </summary>
    private string MapStatusToLinearState(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "pending" => "Backlog",
            "backlog" => "Backlog",
            "broken-down" => "Ready",
            "ready" => "Ready",
            "in_progress" => "In Progress",
            "in progress" => "In Progress",
            "review" => "In Review",
            "completed" => "Done",
            "done" => "Done",
            _ => "Backlog"
        };
    }

    /// <summary>
    /// 简单的 JSON 字符串提取 (避免引入 JSON 解析依赖)
    /// </summary>
    private string ExtractJsonString(string json, string key)
    {
        string pattern = $"\"{key}\"";
        int keyIndex = json.IndexOf(pattern, System.StringComparison.OrdinalIgnoreCase);
        if (keyIndex < 0) return string.Empty;

        int colonIndex = json.IndexOf(':', keyIndex);
        if (colonIndex < 0) return string.Empty;

        int valueStart = json.IndexOf('"', colonIndex);
        if (valueStart < 0) return string.Empty;
        valueStart++; // skip opening quote

        int valueEnd = valueStart;
        while (valueEnd < json.Length)
        {
            char c = json[valueEnd];
            if (c == '"' && json[valueEnd - 1] != '\\') break;
            valueEnd++;
        }

        return json.Substring(valueStart, valueEnd - valueStart);
    }

    /// <summary>
    /// 提取嵌套 JSON 对象中的字段
    /// </summary>
    private string ExtractJsonNestedString(string json, string parentKey, string childKey)
    {
        string parentPattern = $"\"{parentKey}\"";
        int parentIndex = json.IndexOf(parentPattern, System.StringComparison.OrdinalIgnoreCase);
        if (parentIndex < 0) return string.Empty;

        int objStart = json.IndexOf('{', parentIndex);
        if (objStart < 0) return string.Empty;

        int braceCount = 1;
        int objEnd = objStart + 1;
        while (objEnd < json.Length && braceCount > 0)
        {
            if (json[objEnd] == '{') braceCount++;
            else if (json[objEnd] == '}') braceCount--;
            objEnd++;
        }

        string nestedJson = json.Substring(objStart, objEnd - objStart);
        return ExtractJsonString(nestedJson, childKey);
    }

    private string EscapeJson(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
}
