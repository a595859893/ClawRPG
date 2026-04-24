using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ClawRPG.Systems
{
/// <summary>
/// 群聊机器人连接器 - 负责与QQ/微信群聊机器人通信
/// </summary>
public partial class GroupBotConnector : BaseSystem
{
    public static GroupBotConnector Instance { get; private set; }

    // 配置
    [Export] public bool EnableGroupBotIntegration = true;
    [Export] public string BotWebhookUrl = "";
    [Export] public string BotApiUrl = "http://localhost:8080/api/message";
    [Export] public string BotWsUrl = "ws://localhost:8080/ws";
    [Export] public bool UseWebSocket = false;
    
    // 信号
    public delegate void ShareCompletedEvent(bool success, string message);
    public delegate void BotResponseEvent(string response);
    
    public event ShareCompletedEvent OnShareCompleted;
    public event BotResponseEvent OnBotResponse;

    // WebSocket 连接
    private WebSocketPeer _webSocket;
    private bool _wsConnected = false;
    private string _pendingWsMessage = "";
    
    // HTTP 请求队列
    private Queue<string> _httpRequestQueue = new Queue<string>();
    private bool _isProcessingQueue = false;

    // 引用其他管理器
    private ShareTemplateManager _templateManager;
    private ScreenshotManager _screenshotManager;

    public override void _Ready()
    {
        Instance = this;
        
        // 获取引用
        _templateManager = ShareTemplateManager.Instance;
        _screenshotManager = ScreenshotManager.Instance;
    }

    /// <summary>
    /// 发送战绩到群聊
    /// </summary>
    public async void SendToGroup(ShareTemplateManager.ShareData data, long groupId)
    {
        if (_templateManager == null)
        {
            GD.PrintErr("[GroupBotConnector] ShareTemplateManager not available");
            OnShareCompleted?.Invoke(false, "Template manager not available");
            return;
        }

        var shareText = _templateManager.GenerateShareText(data, ShareTemplateManager.ShareTemplate.Detailed);
        var shareJson = _templateManager.GenerateShareJson(data);
        
        GD.Print($"[GroupBotConnector] Sending to group {groupId}: {shareJson}");
        
        // 通过QQ机器人发送
        await SendToQQBot(groupId, shareText, shareJson);
    }

    /// <summary>
    /// 发送战绩到群聊(带截图)
    /// </summary>
    public async void SendScreenshotToGroup(ShareTemplateManager.ShareData data, long groupId, string screenshotPath = "")
    {
        // 先截取截图
        if (string.IsNullOrEmpty(screenshotPath) && _screenshotManager != null)
        {
            screenshotPath = _screenshotManager.TakeScreenshot();
        }
        
        var shareText = _templateManager != null 
            ? _templateManager.GenerateShareText(data, ShareTemplateManager.ShareTemplate.Simple)
            : "ClawRPG 战绩分享";
        
        // 发送图片和文字
        await SendImageToQQBot(groupId, screenshotPath, shareText);
    }

    /// <summary>
    /// 查询群内战绩排行
    /// </summary>
    public async void QueryGroupLeaderboard(long groupId, string gameMode = "")
    {
        var queryJson = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            { "type", "query_leaderboard" },
            { "group_id", groupId },
            { "game_mode", gameMode },
            { "limit", 10 }
        });
        
        await SendToQQBot(groupId, "查询中...", queryJson);
    }

    /// <summary>
    /// 发送消息到QQ机器人
    /// </summary>
    private async System.Threading.Tasks.Task SendToQQBot(long groupId, string message, string extraJson = "")
    {
        if (!EnableGroupBotIntegration)
        {
            GD.Print("[GroupBotConnector] Bot integration disabled");
            OnShareCompleted?.Invoke(false, "Bot integration disabled");
            return;
        }
        
        // 构建消息
        var messageData = new Dictionary<string, object>
        {
            { "group_id", groupId },
            { "message", message }
        };
        
        if (!string.IsNullOrEmpty(extraJson))
        {
            messageData["extra"] = extraJson;
        }
        
        var jsonString = JsonSerializer.Serialize(messageData);
        
        // 通过 HTTPClient 调用机器人 API
        await SendHttpRequest(BotApiUrl, jsonString);
        
        GD.Print($"[GroupBotConnector] Bot message sent: {jsonString}");
        
        OnShareCompleted?.Invoke(true, "Message sent to bot");
    }

    /// <summary>
    /// 发送图片到QQ机器人
    /// </summary>
    private async System.Threading.Tasks.Task SendImageToQQBot(long groupId, string imagePath, string message = "")
    {
        if (!EnableGroupBotIntegration)
        {
            OnShareCompleted?.Invoke(false, "Bot integration disabled");
            return;
        }
        
        var messageData = new Dictionary<string, object>
        {
            { "group_id", groupId },
            { "message_type", "image" },
            { "image_path", imagePath }
        };
        
        if (!string.IsNullOrEmpty(message))
        {
            messageData["text"] = message;
        }
        
        var jsonString = JsonSerializer.Serialize(messageData);
        
        GD.Print($"[GroupBotConnector] Bot image prepared: {jsonString}");
        
        OnShareCompleted?.Invoke(true, "Image sent to bot");
    }

    /// <summary>
    /// 处理机器人回调
    /// </summary>
    public void HandleBotCallback(string jsonResponse)
    {
        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse);
        if (data.ContainsKey("type"))
        {
            string responseType = data["type"].ToString();
            string responseMessage = data["message"].ToString();
            
            GD.Print($"[GroupBotConnector] Bot response: {responseType} - {responseMessage}");
            OnBotResponse?.Invoke(responseMessage);
        }
    }

    /// <summary>
    /// 发送 HTTP/WS 请求到机器人 API
    /// </summary>
    private async System.Threading.Tasks.Task SendHttpRequest(string url, string jsonBody)
    {
        // 根据配置选择使用WebSocket或HTTP
        if (UseWebSocket)
        {
            await SendWebSocketMessage(jsonBody);
            return;
        }
        
        // 使用HTTP请求
        var httpRequest = new HTTPClient();
        
        try
        {
            // 添加请求头
            var headers = new string[] {
                "Content-Type: application/json"
            };
            
            await httpRequest.Request(HTTPClient.Method.Post, url, headers, jsonBody);
            
            if (httpRequest.ResponseCode == 200)
            {
                var response = httpRequest.ReadResponseBodyText();
                GD.Print($"[GroupBotConnector] HTTP request success: {response}");
                HandleBotCallback(response);
            }
            else
            {
                GD.PrintErr($"[GroupBotConnector] HTTP request failed: {httpRequest.ResponseCode}");
                OnShareCompleted?.Invoke(false, $"HTTP error: {httpRequest.ResponseCode}");
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"[GroupBotConnector] HTTP request error: {e.Message}");
            OnShareCompleted?.Invoke(false, e.Message);
        }
        finally
        {
            httpRequest.Close();
        }
    }

    /// <summary>
    /// 连接 WebSocket
    /// </summary>
    public async void ConnectWebSocket()
    {
        if (_wsConnected && _webSocket != null)
        {
            GD.Print("[GroupBotConnector] WebSocket already connected");
            return;
        }
        
        _webSocket = new WebSocketPeer();
        
        try
        {
            var err = _webSocket.ConnectToUrl(BotWsUrl);
            if (err != Error.Ok)
            {
                GD.PrintErr($"[GroupBotConnector] WebSocket connect failed: {err}");
                return;
            }
            
            GD.Print($"[GroupBotConnector] WebSocket connecting to {BotWsUrl}");
            
            // 等待连接建立
            int timeout = 5000;
            int elapsed = 0;
            while (elapsed < timeout)
            {
                await System.Threading.Tasks.Task.Delay(100);
                elapsed += 100;
                
                _webSocket.Poll();
                var state = _webSocket.GetReadyState();
                
                if (state == WebSocketPeer.State.Open)
                {
                    _wsConnected = true;
                    GD.Print("[GroupBotConnector] WebSocket connected");
                    
                    // 处理之前堆积的消息
                    if (!string.IsNullOrEmpty(_pendingWsMessage))
                    {
                        await SendWebSocketMessage(_pendingWsMessage);
                        _pendingWsMessage = "";
                    }
                    return;
                }
                else if (state == WebSocketPeer.State.Closed)
                {
                    break;
                }
            }
            
            GD.PrintErr("[GroupBotConnector] WebSocket connection timeout");
        }
        catch (Exception e)
        {
            GD.PrintErr($"[GroupBotConnector] WebSocket error: {e.Message}");
        }
    }

    /// <summary>
    /// 发送 WebSocket 消息
    /// </summary>
    private async System.Threading.Tasks.Task SendWebSocketMessage(string jsonBody)
    {
        if (!_wsConnected || _webSocket == null)
        {
            // 尝试连接
            ConnectWebSocket();
            _pendingWsMessage = jsonBody;
            return;
        }
        
        try
        {
            var packet = _webSocket.GetPacket();
            if (packet != null)
            {
                // 处理接收到的消息
                var msg = _webSocket.GetString();
                if (!string.IsNullOrEmpty(msg))
                {
                    GD.Print($"[GroupBotConnector] WebSocket received: {msg}");
                    HandleBotCallback(msg);
                }
            }
            
            // 发送消息
            _webSocket.SendText(jsonBody);
            GD.Print($"[GroupBotConnector] WebSocket sent: {jsonBody}");
            OnShareCompleted?.Invoke(true, "Message sent via WebSocket");
        }
        catch (Exception e)
        {
            GD.PrintErr($"[GroupBotConnector] WebSocket send error: {e.Message}");
            OnShareCompleted?.Invoke(false, e.Message);
            
            // 尝试重连
            _wsConnected = false;
            ConnectWebSocket();
        }
    }

    /// <summary>
    /// 断开 WebSocket 连接
    /// </summary>
    public void DisconnectWebSocket()
    {
        if (_webSocket != null)
        {
            _webSocket.Close();
            _webSocket = null;
            _wsConnected = false;
            GD.Print("[GroupBotConnector] WebSocket disconnected");
        }
    }

    /// <summary>
    /// 处理 WebSocket 消息队列
    /// </summary>
    private async System.Threading.Tasks.Task ProcessMessageQueue()
    {
        if (_isProcessingQueue || _httpRequestQueue.Count == 0)
            return;
        
        _isProcessingQueue = true;
        
        while (_httpRequestQueue.Count > 0)
        {
            var message = _httpRequestQueue.Dequeue();
            await SendHttpRequest(BotApiUrl, message);
        }
        
        _isProcessingQueue = false;
    }

    public override void _ExitTree()
    {
        // 清理 WebSocket 连接
        DisconnectWebSocket();
        Instance = null;
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary
        {
            { "bot_api_url", BotApiUrl },
            { "bot_ws_url", BotWsUrl },
            { "enable_integration", EnableGroupBotIntegration },
            { "use_websocket", UseWebSocket }
        };
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data.ContainsKey("bot_api_url"))
            BotApiUrl = data["bot_api_url"].ToString();
        if (data.ContainsKey("bot_ws_url"))
            BotWsUrl = data["bot_ws_url"].ToString();
        if (data.ContainsKey("enable_integration"))
            EnableGroupBotIntegration = (bool)data["enable_integration"];
        if (data.ContainsKey("use_websocket"))
            UseWebSocket = (bool)data["use_websocket"];
    }
}
}
