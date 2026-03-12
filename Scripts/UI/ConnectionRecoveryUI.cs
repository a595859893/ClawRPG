using Godot;
using System;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Connection Recovery UI - Displays network connection status
    /// </summary>
    public class ConnectionRecoveryUI : Control {
        private ConnectionRecoverySystem _system;
        private Label _statusLabel;
        private Label _pingLabel;
        private Label _qualityLabel;
        private ProgressBar _reconnectProgress;
        private Button _reconnectButton;
        private Button _offlineModeButton;
        private TextureRect _statusIcon;
        
        private bool _visible = false;
        private float _updateTimer = 0f;
        
        public override void _Ready() {
            _system = ConnectionRecoverySystem.Instance;
            
            SetupUI();
            GD.Print("[ConnectionRecoveryUI] Initialized");
        }
        
        private void SetupUI() {
            // Main container
            var mainContainer = new VBoxContainer();
            mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
            mainContainer.Position = new Vector2(-200, -150);
            mainContainer.Size = new Vector2(400, 300);
            mainContainer.CustomMinimumSize = new Vector2(400, 300);
            AddChild(mainContainer);
            
            // Title
            var titleLabel = new Label();
            titleLabel.Text = "  🌐 Connection Status";
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            mainContainer.AddChild(titleLabel);
            
            // Separator
            var separator1 = new HSeparator();
            mainContainer.AddChild(separator1);
            
            // Status section
            var statusContainer = new HBoxContainer();
            mainContainer.AddChild(statusContainer);
            
            // Status icon
            _statusIcon = new TextureRect();
            _statusIcon.CustomMinimumSize = new Vector2(32, 32);
            _statusIcon.Modulate = new Color(0, 1, 0);
            statusContainer.AddChild(_statusIcon);
            
            // Status label
            _statusLabel = new Label();
            _statusLabel.Text = "Connected";
            _statusLabel.AddThemeFontSizeOverride("font_size", 20);
            statusContainer.AddChild(_statusLabel);
            
            // Connection quality
            _qualityLabel = new Label();
            _qualityLabel.Text = "Quality: Excellent";
            _qualityLabel.AddThemeFontSizeOverride("font_size", 16);
            _qualityLabel.Modulate = new Color(0.5f, 1, 0.5f);
            mainContainer.AddChild(_qualityLabel);
            
            // Separator
            var separator2 = new HSeparator();
            mainContainer.AddChild(separator2);
            
            // Ping section
            var pingContainer = new HBoxContainer();
            mainContainer.AddChild(pingContainer);
            
            var pingTitle = new Label();
            pingTitle.Text = "Ping: ";
            pingTitle.AddThemeFontSizeOverride("font_size", 16);
            pingContainer.AddChild(pingTitle);
            
            _pingLabel = new Label();
            _pingLabel.Text = "50 ms";
            _pingLabel.AddThemeFontSizeOverride("font_size", 16);
            _pingLabel.Modulate = new Color(0, 1, 0);
            pingContainer.AddChild(_pingLabel);
            
            // Reconnect progress
            var progressContainer = new VBoxContainer();
            mainContainer.AddChild(progressContainer);
            
            var progressLabel = new Label();
            progressLabel.Text = "Reconnecting...";
            progressLabel.AddThemeFontSizeOverride("font_size", 14);
            progressContainer.AddChild(progressLabel);
            
            _reconnectProgress = new ProgressBar();
            _reconnectProgress.CustomMinimumSize = new Vector2(380, 20);
            _reconnectProgress.MinValue = 0;
            _reconnectProgress.MaxValue = 100;
            _reconnectProgress.Value = 0;
            _reconnectProgress.Visible = false;
            progressContainer.AddChild(_reconnectProgress);
            
            // Button container
            var buttonContainer = new HBoxContainer();
            mainContainer.AddChild(buttonContainer);
            
            // Reconnect button
            _reconnectButton = new Button();
            _reconnectButton.Text = "  Reconnect  ";
            _reconnectButton.Pressed += OnReconnectPressed;
            _reconnectButton.Visible = false;
            buttonContainer.AddChild(_reconnectButton);
            
            // Offline mode button
            _offlineModeButton = new Button();
            _offlineModeButton.Text = "  Offline Mode  ";
            _offlineModeButton.Pressed += OnOfflineModePressed;
            _offlineModeButton.Visible = false;
            buttonContainer.AddChild(_offlineModeButton);
            
            // Info text
            var infoLabel = new Label();
            infoLabel.Text = "Press C to toggle this window";
            infoLabel.AddThemeFontSizeOverride("font_size", 12);
            infoLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);
            mainContainer.AddChild(infoLabel);
            
            // Hide by default
            mainContainer.Visible = false;
        }
        
        private void OnReconnectPressed() {
            _system.DisableOfflineMode();
            _system.AttemptReconnect();
        }
        
        private void OnOfflineModePressed() {
            if (_system.IsOfflineMode()) {
                _system.DisableOfflineMode();
            } else {
                _system.EnableOfflineMode();
            }
        }
        
        public override void _Process(float delta) {
            _updateTimer += delta;
            
            if (_updateTimer >= 0.5f) {
                _updateTimer = 0;
                UpdateDisplay();
            }
            
            // Handle input
            if (Input.IsActionJustPressed("ui_cancel") || Input.IsActionJustPressed("connection_toggle")) {
                ToggleVisibility();
            }
        }
        
        private void UpdateDisplay() {
            var data = _system.Data;
            
            // Update status
            _statusLabel.Text = _system.GetStateString();
            
            // Update status color
            switch (_system.Data.State) {
                case ConnectionState.Connected:
                    _statusLabel.Modulate = new Color(0, 1, 0);
                    break;
                case ConnectionState.Reconnecting:
                case ConnectionState.Connecting:
                    _statusLabel.Modulate = new Color(1, 1, 0);
                    break;
                case ConnectionState.Disconnected:
                    _statusLabel.Modulate = new Color(1, 0.5f, 0);
                    break;
                case ConnectionState.OfflineMode:
                    _statusLabel.Modulate = new Color(1, 0, 0);
                    break;
            }
            
            // Update ping
            _pingLabel.Text = $"{data.LastPing:F0} ms";
            if (data.AveragePing < 100) {
                _pingLabel.Modulate = new Color(0, 1, 0);
            } else if (data.AveragePing < 200) {
                _pingLabel.Modulate = new Color(1, 1, 0);
            } else {
                _pingLabel.Modulate = new Color(1, 0, 0);
            }
            
            // Update quality
            _qualityLabel.Text = $"Quality: {_system.GetConnectionQuality()}";
            
            // Update progress
            _reconnectProgress.Value = _system.GetReconnectionProgress();
            
            // Show/hide buttons
            bool showReconnect = data.State == ConnectionState.Disconnected || 
                                 data.State == ConnectionState.OfflineMode;
            _reconnectButton.Visible = showReconnect;
            _reconnectProgress.Visible = data.State == ConnectionState.Reconnecting;
            
            _offlineModeButton.Text = _system.IsOfflineMode() ? "  Go Online  " : "  Offline Mode  ";
            _offlineModeButton.Visible = showReconnect;
        }
        
        public void ToggleVisibility() {
            _visible = !_visible;
            
            // Find main container and toggle
            foreach (var child in GetChildren()) {
                if (child is VBoxContainer container) {
                    container.Visible = _visible;
                    break;
                }
            }
            
            if (_visible) {
                UpdateDisplay();
            }
        }
        
        public override void _Input(InputEvent @event) {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
                if (keyEvent.Keycode == Key.C) {
                    ToggleVisibility();
                }
            }
        }
    }
}
