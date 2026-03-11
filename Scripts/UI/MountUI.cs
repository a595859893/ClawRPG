using Godot;
using System;

public partial class MountUI : Control
{
    private MountManager _mountManager;
    private Control _mountPanel;
    private Button _summonButton;
    private Button _dismissButton;
    private Label _mountNameLabel;
    private ProgressBar _staminaBar;
    private Label _levelLabel;
    
    public override void _Ready()
    {
        _mountManager = GetNode<MountManager>("/root/Main/MountManager");
        SetupUI();
    }
    
    private void SetupUI()
    {
        // 创建基础UI
        _mountPanel = new Control();
        _mountPanel.Name = "MountPanel";
        AddChild(_mountPanel);
        
        // 召唤按钮
        _summonButton = new Button();
        _summonButton.Text = "Summon Mount";
        _summonButton.Position = new Vector2(100, 500);
        _summonButton.Size = new Vector2(150, 40);
        _summonButton.Pressed += OnSummonPressed;
        _mountPanel.AddChild(_summonButton);
        
        // 解散按钮
        _dismissButton = new Button();
        _dismissButton.Text = "Dismiss";
        _dismissButton.Position = new Vector2(260, 500);
        _dismissButton.Size = new Vector2(150, 40);
        _dismissButton.Pressed += OnDismissPressed;
        _mountPanel.AddChild(_dismissButton);
        
        // 坐骑名称
        _mountNameLabel = new Label();
        _mountNameLabel.Text = "No Mount";
        _mountNameLabel.Position = new Vector2(100, 550);
        _mountPanel.AddChild(_mountNameLabel);
        
        // 耐力条
        _staminaBar = new ProgressBar();
        _staminaBar.Position = new Vector2(100, 580);
        _staminaBar.Size = new Vector2(200, 20);
        _staminaBar.MaxValue = 100;
        _staminaBar.Value = 100;
        _mountPanel.AddChild(_staminaBar);
        
        // 等级
        _levelLabel = new Label();
        _levelLabel.Text = "Level: 1";
        _levelLabel.Position = new Vector2(100, 610);
        _mountPanel.AddChild(_levelLabel);
    }
    
    private void OnSummonPressed()
    {
        if (_mountManager != null)
        {
            _mountManager.SummonMount();
            UpdateUI();
        }
    }
    
    private void OnDismissPressed()
    {
        if (_mountManager != null)
        {
            _mountManager.DismissMount();
            UpdateUI();
        }
    }
    
    private void UpdateUI()
    {
        if (_mountManager == null) return;
        
        var mount = _mountManager.GetCurrentMount();
        if (mount != null)
        {
            _mountNameLabel.Text = mount.GetType().Name;
            _staminaBar.Value = mount.Stamina;
            _levelLabel.Text = "Level: " + mount.Level;
            _summonButton.Disabled = true;
            _dismissButton.Disabled = false;
        }
        else
        {
            _mountNameLabel.Text = "No Mount";
            _staminaBar.Value = 0;
            _levelLabel.Text = "Level: -";
            _summonButton.Disabled = false;
            _dismissButton.Disabled = true;
        }
    }
    
    public override void _Process(double delta)
    {
        // 更新耐力显示
        if (_mountManager?.GetCurrentMount() != null)
        {
            _staminaBar.Value = _mountManager.GetCurrentMount().Stamina;
        }
    }
}
