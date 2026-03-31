using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.CoopSession;

/// <summary>
/// 多人游戏管理器 - 协调者
/// 委托给三个子系统：
/// - SessionManager: 会话管理
/// - NetworkSyncSystem: 网络状态同步
/// - LobbySystem: 大堂管理
/// </summary>
public partial class MultiplayerManager : BaseSystem
{
    public static MultiplayerManager Instance { get; private set; }
    
    // 三个子系统
    public SessionManager Session { get; private set; }
    public NetworkSyncSystem NetworkSync { get; private set; }
    public LobbySystem Lobby { get; private set; }
    
    // 信号 - 委托给子系统
    public delegate void RoomCreatedEventHandler(string roomId);
    public delegate void RoomJoinedEventHandler(string roomId);
    public delegate void RoomLeftEventHandler();
    public delegate void ConnectionFailedEventHandler(string reason);
    
    // 兼容旧接口的属性
    public bool IsInRoom => Session?.IsInRoom ?? false;
    public bool IsHost => Session?.IsHost ?? false;
    public bool IsReady => NetworkSync?.IsReady ?? false;
    public bool NeedsPassword => Session?.NeedsPassword ?? false;
    public int LocalPlayerId => Session?.LocalPlayerId ?? -1;
    public string PlayerName
    {
        get => Session?.PlayerName ?? "Player";
        set 
        {
            if (Session != null) Session.SetPlayerName(value);
            if (Lobby != null) Lobby.SetLocalPlayerName(value);
        }
    }
    
    public override void _Ready()
    {
        Instance = this;
        
        // 创建三个子系统
        Session = new SessionManager();
        NetworkSync = new NetworkSyncSystem();
        Lobby = new LobbySystem();
        
        // 添加为子节点
        AddChild(Session);
        AddChild(NetworkSync);
        AddChild(Lobby);
        
        // 初始化子系统
        Session._Ready();
        NetworkSync._Ready();
        Lobby._Ready();
        
        // 转发子系统的信号
        ConnectSubsystemSignals();
        
        // 监听网络客户端事件
        if (NetworkClient.Instance != null)
        {
            NetworkClient.Instance.OnConnected += OnConnected;
            NetworkClient.Instance.OnDisconnected += OnDisconnected;
            NetworkClient.Instance.OnMessageReceived += OnMessageReceived;
            NetworkClient.Instance.OnError += OnError;
        }
        
        GD.Print("[MultiplayerManager] Initialized with 3 subsystems");
    }
    
    private void ConnectSubsystemSignals()
    {
        // Session 信号转发
        if (Session != null)
        {
            Session.RoomCreated += (roomId) => EmitSignal(SignalName.RoomCreated, roomId);
            Session.RoomJoined += (roomId) => EmitSignal(SignalName.RoomJoined, roomId);
            Session.RoomLeft += () => EmitSignal(SignalName.RoomLeft);
            Session.ConnectionFailed += (reason) => EmitSignal(SignalName.ConnectionFailed, reason);
        }
    }
    
    public override void _Process(double delta)
    {
        // 子系统各自处理自己的逻辑
    }
}
