using Godot;
using System;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// Counter attack UI - displays counter attack status and allows type selection
    /// </summary>
    public partial class CounterAttackUI : Control
    {
        private Panel _mainPanel;
        private Label _titleLabel;
        private Label _statusLabel;
        private ProgressBar _cooldownBar;
        private Label _cooldownLabel;
        private ProgressBar _windowBar;
        private Label _windowLabel;
        private VBoxContainer _counterTypeContainer;
        private Label _descriptionLabel;
        
        private bool _isVisible = false;
        private Button[] _counterTypeButtons;
        
        public override void _Ready()
        {
            Visible = false;
            _CreateUI();
            
            // Connect signals
            if (CounterAttackSystem.Instance != null)
            {
                CounterAttackSystem.Instance.Connect(CounterAttackSystem.SignalName.CounterAttack窗口, new Callable(this, nameof(_OnCounterWindowChanged)));
                CounterAttackSystem.Instance.Connect(CounterAttackSystem.SignalName.CounterAttackReady, new Callable(this, nameof(_OnCounterReady)));
                CounterAttackSystem.Instance.Connect(CounterAttackSystem.SignalName.CounterAttackPerformed, new Callable(this, nameof(_OnCounterPerformed)));
            }
        }
        
        private void _CreateUI()
        {
            // Main panel
            _mainPanel = new Panel
            {
                AnchorRight = 0f,
                AnchorBottom = 0f,
                OffsetLeft = 20,
                OffsetTop = 150,
                OffsetRight = 280,
                OffsetBottom = 450,
                CustomMinimumSize = new Vector2(260, 300)
            };
            AddChild(_mainPanel);
            
            // Title
            _titleLabel = new Label
            {
                Text = "⚔️ 反击系统",
                HorizontalAlignment = HorizontalAlignment.Center,
                OffsetTop = 10
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 20);
            _mainPanel.AddChild(_titleLabel);
            
            // Status label
            _statusLabel = new Label
            {
                Text = "准备就绪",
                HorizontalAlignment = HorizontalAlignment.Center,
                OffsetTop = 40,
                OffsetLeft = 10,
                OffsetRight = -10
            };
            _statusLabel.AddThemeFontSizeOverride("font_size", 14);
            _mainPanel.AddChild(_statusLabel);
            
            // Cooldown bar
            _cooldownBar = new ProgressBar
            {
                OffsetTop = 65,
                OffsetLeft = 15,
                OffsetRight = -15,
                OffsetBottom = 85,
                CustomMinimumSize = new Vector2(0, 15),
                Value = 100,
                ShowPercentage = false
            };
            _mainPanel.AddChild(_cooldownBar);
            
            // Cooldown label
            _cooldownLabel = new Label
            {
                Text = "冷却: 0.0s",
                HorizontalAlignment = HorizontalAlignment.Center,
                OffsetTop = 88,
                OffsetLeft = 10
            };
            _cooldownLabel.AddThemeFontSizeOverride("font_size", 12);
            _mainPanel.AddChild(_cooldownLabel);
            
            // Counter window bar (appears during counter opportunity)
            _windowBar = new ProgressBar
            {
                OffsetTop = 110,
                OffsetLeft = 15,
                OffsetRight = -15,
                OffsetBottom = 130,
                CustomMinimumSize = new Vector2(0, 15),
                Value = 0,
                ShowPercentage = false,
                Visible = false
            };
            _windowBar.Modulate = new Color(1f, 0.8f, 0f); // Gold color
            _mainPanel.AddChild(_windowBar);
            
            // Window label
            _windowLabel = new Label
            {
                Text = "反击窗口!",
                HorizontalAlignment = HorizontalAlignment.Center,
                OffsetTop = 133,
                OffsetLeft = 10,
                Visible = false
            };
            _windowLabel.Modulate = new Color(1f, 0.8f, 0f);
            _windowLabel.AddThemeFontSizeOverride("font_size", 12);
            _mainPanel.AddChild(_windowLabel);
            
            // Counter type selection
            _counterTypeContainer = new VBoxContainer
            {
                OffsetTop = 155,
                OffsetLeft = 10,
                OffsetRight = -10,
                OffsetBottom = 260
            };
            _mainPanel.AddChild(_counterTypeContainer);
            
            // Create counter type buttons
            var counterTypes = Enum.GetValues(typeof(CounterAttackSystem.CounterType));
            _counterTypeButtons = new Button[counterTypes.Length];
            
            string[] emojis = {"⚔️", "🛡️", "💃", "🛡️", "🩸", "✨"};
            
            for (int i = 0; i < counterTypes.Length; i++)
            {
                var type = (CounterAttackSystem.CounterType)counterTypes.GetValue(i);
                var btn = new Button
                {
                    Text = $"{emojis[i]} {type}",
                    SizeFlagsHorizontal = SizeFlags.Expand,
                    CustomMinimumSize = new Vector2(0, 35)
                };
                int index = i;
                btn.Pressed += () => _OnCounterTypeSelected(index);
                _counterTypeContainer.AddChild(btn);
                _counterTypeButtons[i] = btn;
            }
            
            // Description label
            _descriptionLabel = new Label
            {
                Text = "选择反击类型",
                OffsetTop = 265,
                OffsetLeft = 10,
                OffsetRight = -10,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            _descriptionLabel.AddThemeFontSizeOverride("font_size", 12);
            _mainPanel.AddChild(_descriptionLabel);
            
            // Update initial state
            _UpdateUI();
        }
        
        private void _OnCounterTypeSelected(int index)
        {
            var counterTypes = (CounterAttackSystem.CounterType[])Enum.GetValues(typeof(CounterAttackSystem.CounterType));
            if (index >= 0 && index < counterTypes.Length)
            {
                CounterAttackSystem.Instance.SetCounterType(counterTypes[index]);
                _UpdateUI();
            }
        }
        
        private void _OnCounterWindowChanged(bool isActive)
        {
            _windowBar.Visible = isActive;
            _windowLabel.Visible = isActive;
            
            if (isActive)
            {
                _statusLabel.Text = "按攻击键反击!";
                _statusLabel.Modulate = new Color(1f, 0.8f, 0f); // Gold
            }
            else
            {
                _statusLabel.Text = "准备就绪";
                _statusLabel.Modulate = new Color(1f, 1f, 1f);
            }
        }
        
        private void _OnCounterReady()
        {
            _statusLabel.Text = "准备就绪";
            _statusLabel.Modulate = new Color(0f, 1f, 0f); // Green
        }
        
        private void _OnCounterPerformed(CounterAttackSystem.CounterType type, float damage)
        {
            _statusLabel.Text = $"反击成功! 造成 {(int)damage} 伤害";
            _statusLabel.Modulate = new Color(0f, 1f, 0f);
        }
        
        public override void _Process(double delta)
        {
            if (CounterAttackSystem.Instance == null) return;
            
            float deltaF = (float)delta;
            
            // Update cooldown bar
            float cooldownProgress = CounterAttackSystem.Instance.GetCooldownProgress();
            _cooldownBar.Value = cooldownProgress * 100;
            
            // Update cooldown label
            float cooldownTimer = CounterAttackSystem.Instance.CounterCooldownTimer;
            if (cooldownTimer > 0)
            {
                _cooldownLabel.Text = $"冷却: {cooldownTimer:F1}s";
            }
            else
            {
                _cooldownLabel.Text = "冷却完成";
            }
            
            // Update window bar
            if (CounterAttackSystem.Instance.IsCounterAttacking)
            {
                float windowProgress = CounterAttackSystem.Instance.GetExecutionWindowProgress();
                _windowBar.Value = windowProgress * 100;
            }
        }
        
        private void _UpdateUI()
        {
            if (CounterAttackSystem.Instance == null) return;
            
            // Update button states
            var currentType = CounterAttackSystem.Instance.CurrentCounterType;
            var counterTypes = (CounterAttackSystem.CounterType[])Enum.GetValues(typeof(CounterAttackSystem.CounterType));
            
            for (int i = 0; i < counterTypes.Length; i++)
            {
                if (_counterTypeButtons[i] != null)
                {
                    bool isSelected = counterTypes[i] == currentType;
                    _counterTypeButtons[i].Modulate = isSelected ? new Color(1f, 0.8f, 0f) : new Color(1f, 1f, 1f);
                }
            }
            
            // Update description
            var data = CounterAttackSystem.Instance.GetCurrentCounterData();
            if (data != null)
            {
                _descriptionLabel.Text = $"{data.Name}: {data.Description}\n\n" +
                    $"伤害倍率: {data.DamageMultiplier}x\n" +
                    $"体力消耗: {data.StaminaCost}\n" +
                    $"冷却时间: {data.Cooldown}s";
            }
        }
        
        public void Toggle()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible)
            {
                _UpdateUI();
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (CounterAttackSystem.Instance != null)
                {
                    CounterAttackSystem.Instance.Disconnect(CounterAttackSystem.SignalName.CounterAttack窗口, new Callable(this, nameof(_OnCounterWindowChanged)));
                    CounterAttackSystem.Instance.Disconnect(CounterAttackSystem.SignalName.CounterAttackReady, new Callable(this, nameof(_OnCounterReady)));
                    CounterAttackSystem.Instance.Disconnect(CounterAttackSystem.SignalName.CounterAttackPerformed, new Callable(this, nameof(_OnCounterPerformed)));
                }
            }
            base.Dispose(disposing);
        }
    }
}
