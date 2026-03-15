using Godot;
using System;
using System.Collections.Generic;
using WebSocketSharp;

/// <summary>
/// 网络客户端 - WebSocket 连接管理
/// 支持指数退避重连 + jitter 防惊群
/// 心跳保连 (Ping/Pong)
/// </summary>
public class NetworkClient : BaseSystem
{
    public static NetworkClient Instance { get; private set; }

    // WebSocket 连接
    private WebSocket _ws;
    private string _serverUrl = "ws://localhost:8080";
    
    // 重连配置
    private int _maxReconnectAttempts = 10;
    private int _baseReconnectDelayMs = 1000;  // 1秒基础延迟
    private int _maxReconnectDelayMs = 30000;  // 30秒最大延迟
    private float _reconnectJitter = 1.0f;     // jitter 范围
    
    // 状态
    private int _reconnectAttempts = 0;
    private bool _isConnected = false; 
    private bool _isConnecting = false; 
    
    // 心跳
    private Timer _heartbeatTimer;
    private int _heartbeatIntervalMs = 5000;  // 5秒心跳间隔
    private int _heartbeatTimeoutMs = 15000;  // 15秒超时
    private DateTime _lastPingTime;
    
    // 消息队列
    private Queue<string> _messageQueue = new Queue<string>();
    private readonly object _queueLock = new object();
    
    // 信号
    public delegate void ConnectedEvent();
    public delegate void DisconnectedEvent(string reason);
    public delegate void MessageReceivedEvent(string message);
    public delegate void ErrorEvent(string error);
    
    public event ConnectedEvent OnConnected;
    public event DisconnectedEvent OnDisconnected;
    public event MessageReceivedEvent OnMessageReceived;
    public event ErrorEvent OnError;
    
    public bool IsConnected => _isConnected;
    public bool IsConnecting => _isConnecting;

    public override void _Ready()
    {
        Instance = this;
        SetupHeartbeat();
    }

    private void SetupHeartbeat()
    {
        _heartbeatTimer = new Timer();
        _heartbeatTimer.WaitTime = _heartbeatIntervalMs / 1000.0;
        _heartbeatTimer.OneShot = false; 
        _heartbeatTimer.Timeout += OnHeartbeatTimeout;
        AddChild(_heartbeatTimer);
    }

    /// <summary>
    /// 连接到服务器
    /// </summary>
    public void Connect(string url = "")
    {
        if (!string.IsNullOrEmpty(url))
            _serverUrl = url;
        
        if (_isConnected || _isConnecting)
            return;
        
        _isConnecting = true;
        AttemptConnection();
    }

    private void AttemptConnection()
    {
        try
        {
            _ws = new WebSocket(_serverUrl);
            _ws.OnOpen += OnWsOpen;
            _ws.OnClose += OnWsClose;
            _ws.OnError += OnWsError;
            _ws.OnMessage += OnWsMessage;
            _ws.ConnectAsync();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[NetworkClient] Connection error: {ex.Message}");
            HandleConnectionFailed();
        }
    }

    private void OnWsOpen(object sender, EventArgs e)
    {
        _isConnected = true;
        _isConnecting = false; 
        _reconnectAttempts = 0;
        
        GD.Print("[NetworkClient] Connected to server");
        
        // 启动心跳
        _heartbeatTimer.Start();
        
        // 发送队列中的消息
        FlushMessageQueue();
        
        OnConnected?.Invoke();
    }

    private void OnWsClose(object sender, CloseEventArgs e)
    {
        _isConnected = false; 
        _isConnecting = false; 
        _heartbeatTimer.Stop();
        
        string reason = e.Reason ?? "Unknown";
        GD.Print($"[NetworkClient] Disconnected: {reason}");
        
        OnDisconnected?.Invoke(reason);
        
        // 尝试重连
        if (e.Code != 1000) // 1000 = 正常关闭
        {
            ScheduleReconnect();
        }
    }

    private void OnWsError(object sender, ErrorEventArgs e)
    {
        string errorMsg = e.Message ?? "Unknown error";
        GD.PrintErr($"[NetworkClient] Error: {errorMsg}");
        
        OnError?.Invoke(errorMsg);
    }

    private void OnWsMessage(object sender, MessageEventArgs e)
    {
        if (e.Data == null) return;
        
        // 处理心跳响应
        if (e.Data == "PONG")
        {
            // 服务器响应心跳
            return;
        }
        
        // 处理心跳请求
        if (e.Data == "PING")
        {
            Send("PONG");
            return;
        }
        
        _lastPingTime = DateTime.Now;
        
        // 触发消息接收事件
        OnMessageReceived?.Invoke(e.Data);
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    public void Send(string message)
    {
        if (_isConnected && _ws != null && _ws.IsAlive)
        {
            try
            {
                _ws.SendAsync(message, null);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[NetworkClient] Send error: {ex.Message}");
            }
        }
        else
        {
            // 加入消息队列，等待重连后发送
            lock (_queueLock)
            {
                _messageQueue.Enqueue(message);
            }
        }
    }

    /// <summary>
    /// 发送 JSON 消息
    /// </summary>
    public void SendJson(object data)
    {
        string json = Godot.JSON.Stringify(data);
        Send(json);
    }

    private void FlushMessageQueue()
    {
        lock (_queueLock)
        {
            while (_messageQueue.Count > 0)
            {
                string msg = _messageQueue.Dequeue();
                Send(msg);
            }
        }
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public void Disconnect()
    {
        _heartbeatTimer.Stop();
        
        if (_ws != null)
        {
            _ws.CloseAsync(1000, "Client disconnect");
        }
        
        _isConnected = false; 
        _isConnecting = false; 
    }

    /// <summary>
    /// 指数退避重连 + jitter
    /// </summary>
    private void ScheduleReconnect()
    {
        if (_reconnectAttempts >= _maxReconnectAttempts)
        {
            GD.Print("[NetworkClient] Max reconnect attempts reached");
            return;
        }
        
        _reconnectAttempts++;
        
        // 指数退避: 2^n * base_delay
        int delay = (int)Mathf.Pow(2, _reconnectAttempts - 1) * _baseReconnectDelayMs;
        delay = Mathf.Min(delay, _maxReconnectDelayMs);
        
        // 添加 jitter: random(0, delay * jitter)
        var random = new Random();
        int jitter = random.Next((int)(delay * _reconnectJitter));
        delay += jitter;
        
        GD.Print($"[NetworkClient] Reconnecting in {delay}ms (attempt {_reconnectAttempts}/{_maxReconnectAttempts})");
        
        // 使用 Timer 延迟重连
        var reconnectTimer = new Timer();
        reconnectTimer.WaitTime = delay / 1000.0;
        reconnectTimer.OneShot = true;
        reconnectTimer.Timeout += () => {
            reconnectTimer.QueueFree();
            AttemptConnection();
        };
        GetTree().Root.AddChild(reconnectTimer);
        reconnectTimer.Start();
    }

    private void HandleConnectionFailed()
    {
        _isConnecting = false; 
        ScheduleReconnect();
    }

    private void OnHeartbeatTimeout()
    {
        if (!_isConnected) return;
        
        var timeSincePing = (DateTime.Now - _lastPingTime).TotalMilliseconds;
        
        if (timeSincePing > _heartbeatTimeoutMs)
        {
            GD.Print("[NetworkClient] Heartbeat timeout, reconnecting...");
            _ws.CloseAsync(1001, "Heartbeat timeout");
        }
        else
        {
            // 发送心跳
            Send("PING");
            _lastPingTime = DateTime.Now;
        }
    }

    public override void _ExitTree()
    {
        Disconnect();
        Instance = null;
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return new Dictionary();
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        // No persistent data needed
    }
}
