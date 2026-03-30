using Godot;
using System;

public class MultiplayerChatMain : BaseSystem
{
    private MultiplayerChatSystem _chatSystem;
    private MultiplayerChatData _chatData;
    private MultiplayerChatDatabase _chatDatabase;
    private MultiplayerChatUI _chatUI;
    private Control _chatUIPanel;
    
    public override void _Ready()
    {
        // Create and add system nodes
        _chatData = new MultiplayerChatData();
        _chatData.Name = "MultiplayerChatData";
        AddChild(_chatData);
        
        _chatDatabase = new MultiplayerChatDatabase();
        _chatDatabase.Name = "MultiplayerChatDatabase";
        AddChild(_chatDatabase);
        
        _chatSystem = new MultiplayerChatSystem();
        _chatSystem.Name = "MultiplayerChatSystem";
        AddChild(_chatSystem);
        
        // Create UI
        _chatUI = new MultiplayerChatUI();
        _chatUI.Name = "MultiplayerChatUI";
        
        // Create panel for chat UI
        _chatUIPanel = new PanelContainer();
        _chatUIPanel.Name = "MultiplayerChatPanel";
        _chatUIPanel.AnchorLeft = 0;
        _chatUIPanel.AnchorTop = 0.5f;
        _chatUIPanel.AnchorRight = 0;
        _chatUIPanel.AnchorBottom = 1;
        _chatUIPanel.OffsetRight = 400;
        _chatUIPanel.OffsetTop = -250;
        _chatUIPanel.OffsetBottom = 250;
        _chatUIPanel.Hide();
        
        var scrollContainer = new ScrollContainer();
        scrollContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        scrollContainer.AddChild(_chatUI);
        
        _chatUIPanel.AddChild(scrollContainer);
        
        // Add to UI layer
        var uiLayer = GetTree().Root.GetNode("UI");
        if (uiLayer != null)
        {
            uiLayer.AddChild(_chatUIPanel);
        }
        else
        {
            // Try to add to root
            GetTree().Root.AddChild(_chatUIPanel);
        }
        
        GD.Print("MultiplayerChat system initialized");
    }
    
    public void ToggleChatUI()
    {
        if (_chatUIPanel != null)
        {
            if (_chatUIPanel.Visible)
                _chatUIPanel.Hide();
            else
                _chatUIPanel.Show();
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // Ctrl+Shift+C for chat (avoiding conflict with character creation)
            if (keyEvent.Control && keyEvent.Shift && keyEvent.Scancode == KeyList.C)
            {
                ToggleChatUI();
                GetTree().SetInputAsHandled();
            }
        }
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        // MultiplayerChatMain 是容器系统，无持久化状态
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        // MultiplayerChatMain 是容器系统，无持久化状态
    }
}
