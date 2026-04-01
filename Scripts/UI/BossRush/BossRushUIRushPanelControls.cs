using Godot;
using System;

/// <summary>
/// Boss Rush UI - Rush Panel Controls Component
/// Handles controls (difficulty selection, action buttons)
/// </summary>
namespace ClawRPG.Scripts.UI.BossRush
{
    public partial class BossRushUIRushPanelControls : Control
    {
        private BossRushSystem _bossRushSystem;
        
        // UI Elements
        private OptionButton _difficultyOption;
        private Button _startButton;
        private Button _advanceButton;
        private Button _quitButton;
        private Button _pauseButton;
        
        // Callbacks
        public Action<string> OnStartPressed { get; set; }
        public Action OnAdvancePressed { get; set; }
        public Action OnQuitPressed { get; set; }
        public Action OnPausePressed { get; set; }
        
        public BossRushUIRushPanelControls()
        {
        }
        
        public void Initialize(BossRushSystem system)
        {
            _bossRushSystem = system;
        }
        
        public void CreateElements(Control parent)
        {
            // Difficulty selection
            var diffLabel = new Label
            {
                Text = "Difficulty:"
            };
            diffLabel.Position = new Vector2(250, 260);
            parent.AddChild(diffLabel);
            
            _difficultyOption = new OptionButton
            {
                Position = new Vector2(350, 255),
                Size = new Vector2(150, 30)
            };
            _difficultyOption.AddItem("Easy");
            _difficultyOption.AddItem("Normal");
            _difficultyOption.AddItem("Hard");
            _difficultyOption.AddItem("Nightmare");
            _difficultyOption.AddItem("Legendary");
            _difficultyOption.Selected = 1; // Normal default
            parent.AddChild(_difficultyOption);
            
            // Action buttons
            CreateActionButtons(parent);
        }
        
        private void CreateActionButtons(Control parent)
        {
            _startButton = new Button
            {
                Text = "  Start Rush  ",
                Position = new Vector2(250, 320),
                Size = new Vector2(300, 50)
            };
            _startButton.Pressed += OnStartButtonPressed;
            parent.AddChild(_startButton);
            
            _advanceButton = new Button
            {
                Text = "  Next Boss  ",
                Position = new Vector2(250, 380),
                Size = new Vector2(300, 50),
                Disabled = true
            };
            _advanceButton.Pressed += OnAdvanceButtonPressed;
            parent.AddChild(_advanceButton);
            
            _quitButton = new Button
            {
                Text = "  Quit Rush  ",
                Position = new Vector2(250, 440),
                Size = new Vector2(140, 40),
                Disabled = true
            };
            _quitButton.Pressed += OnQuitButtonPressed;
            parent.AddChild(_quitButton);
            
            _pauseButton = new Button
            {
                Text = "  Pause  ",
                Position = new Vector2(410, 440),
                Size = new Vector2(140, 40),
                Disabled = true
            };
            _pauseButton.Pressed += OnPauseButtonPressed;
            parent.AddChild(_pauseButton);
        }
        
        private void OnStartButtonPressed()
        {
            string difficulty = _difficultyOption.GetItemText(_difficultyOption.Selected);
            OnStartPressed?.Invoke(difficulty);
        }
        
        private void OnAdvanceButtonPressed()
        {
            OnAdvancePressed?.Invoke();
        }
        
        private void OnQuitButtonPressed()
        {
            OnQuitPressed?.Invoke();
        }
        
        private void OnPauseButtonPressed()
        {
            OnPausePressed?.Invoke();
        }
        
        public void UpdateButtonStates()
        {
            if (_bossRushSystem == null) return;
            
            var state = _bossRushSystem.GetState();
            bool inRush = _bossRushSystem.IsInRush();
            
            _startButton.Disabled = inRush;
            _advanceButton.Disabled = !inRush;
            _quitButton.Disabled = !inRush;
            _pauseButton.Disabled = !inRush;
            
            if (state == BossRushState.Paused)
                _pauseButton.Text = "Resume";
            else
                _pauseButton.Text = "Pause";
        }
    }
}
