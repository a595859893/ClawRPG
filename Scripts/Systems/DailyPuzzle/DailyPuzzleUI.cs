using System;
using Godot;
using System.Collections.Generic;

public partial class DailyPuzzleUI : Control
{
    private Label _titleLabel;
    private Label _puzzleLabel;
    private Label _timerLabel;
    private Label _hintLabel;
    private LineEdit _answerInput;
    private Button _submitButton;
    private Button _hintButton;
    private Button _closeButton;
    private Label _resultLabel;
    private Label _streakLabel;
    private Label _statsLabel;
    private VBoxContainer _puzzleContainer;
    private VBoxContainer _resultContainer;
    private int _hintsUsed = 0;
    private int _timeRemaining;
    private bool _isTimerRunning;
    private Timer _timer;
    
    public override void _Ready()
    {
        // Create UI
        CreateUI();
        
        // Load today's puzzle
        LoadDailyPuzzle();
    }
    
    private void CreateUI()
    {
        // Main container
        VBoxContainer mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainContainer.CustomMinimumSize = new Vector2(500, 400);
        mainContainer.Position = new Vector2(250, 150);
        AddChild(mainContainer);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "🎯 Daily Puzzle";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(_titleLabel);
        
        // Streak info
        _streakLabel = new Label();
        _streakLabel.Text = $"🔥 Streak: {DailyPuzzleSystem.Instance.GetCurrentStreak()} | Best: {DailyPuzzleSystem.Instance.GetBestStreak()}";
        _streakLabel.HorizontalAlignment = HorizontalAlignment.Center;
        mainContainer.AddChild(_streakLabel);
        
        // Separator
        mainContainer.AddChild(new HSeparator());
        
        // Puzzle container
        _puzzleContainer = new VBoxContainer();
        mainContainer.AddChild(_puzzleContainer);
        
        // Puzzle question
        _puzzleLabel = new Label();
        _puzzleLabel.Text = "Loading puzzle...";
        _puzzleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _puzzleLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        _puzzleLabel.CustomMinimumSize = new Vector2(450, 60);
        _puzzleContainer.AddChild(_puzzleLabel);
        
        // Timer
        _timerLabel = new Label();
        _timerLabel.Text = "Time: --";
        _timerLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _puzzleContainer.AddChild(_timerLabel);
        
        // Answer input
        HBoxContainer inputContainer = new HBoxContainer();
        _puzzleContainer.AddChild(inputContainer);
        
        _answerInput = new LineEdit();
        _answerInput.PlaceholderText = "Enter your answer...";
        _answerInput.CustomMinimumSize = new Vector2(300, 30);
        _answerInput.TextSubmitted += OnAnswerSubmitted;
        inputContainer.AddChild(_answerInput);
        
        // Submit button
        _submitButton = new Button();
        _submitButton.Text = "Submit";
        _submitButton.Pressed += OnSubmitPressed;
        inputContainer.AddChild(_submitButton);
        
        // Hint button
        _hintButton = new Button();
        _hintButton.Text = "💡 Hint (-25% reward)";
        _hintButton.Pressed += OnHintPressed;
        _puzzleContainer.AddChild(_hintButton);
        
        // Hint label
        _hintLabel = new Label();
        _hintLabel.Text = "";
        _hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _hintLabel.Modulate = new Color(1, 1, 0); // Yellow
        _puzzleContainer.AddChild(_hintLabel);
        
        // Result container
        _resultContainer = new VBoxContainer();
        _resultContainer.Visible = false;
        mainContainer.AddChild(_resultContainer);
        
        _resultLabel = new Label();
        _resultLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _resultLabel.AddThemeFontSizeOverride("font_size", 20);
        _resultContainer.AddChild(_resultLabel);
        
        // Statistics
        _statsLabel = new Label();
        _statsLabel.Text = $"Total Solved: {DailyPuzzleSystem.Instance.GetTotalSolved()}";
        _statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _resultContainer.AddChild(_statsLabel);
        
        // Close button
        _closeButton = new Button();
        _closeButton.Text = "Close";
        _closeButton.Pressed += OnClosePressed;
        mainContainer.AddChild(_closeButton);
        
        // Create timer
        _timer = new Timer();
        _timer.WaitTime = 1.0;
        _timer.Timeout += OnTimerTimeout;
        AddChild(_timer);
    }
    
    private void LoadDailyPuzzle()
    {
        var puzzle = DailyPuzzleSystem.Instance.GetDailyPuzzle();
        if (puzzle == null)
        {
            _puzzleLabel.Text = "No puzzle available";
            return;
        }
        
        // Check if already solved
        if (DailyPuzzleSystem.Instance.IsTodayPuzzleSolved())
        {
            ShowAlreadySolved();
            return;
        }
        
        // Display puzzle
        string typeName = DailyPuzzleDatabase.PuzzleTypeNames[(int)puzzle.Type];
        _puzzleLabel.Text = $"[{typeName}] {puzzle.Question}";
        
        // Set timer
        _timeRemaining = puzzle.TimeLimit;
        _timerLabel.Text = $"Time: {_timeRemaining}s";
        
        // Reset state
        _hintsUsed = 0;
        _hintLabel.Text = "";
        _answerInput.Text = "";
        _puzzleContainer.Visible = true;
        _resultContainer.Visible = false;
        _submitButton.Disabled = false;
        _hintButton.Disabled = false;
        
        // Start timer
        _isTimerRunning = true;
        _timer.Start();
        
        // Focus input
        _answerInput.GrabFocus();
    }
    
    private void ShowAlreadySolved()
    {
        _puzzleContainer.Visible = false;
        _resultContainer.Visible = true;
        
        _resultLabel.Text = "✅ Today's Puzzle Solved!";
        _resultLabel.Modulate = new Color(0, 1, 0); // Green
        
        var stats = DailyPuzzleSystem.Instance.GetStatistics();
        if (stats.SolvedPuzzles.TryGetValue(DailyPuzzleSystem.Instance.GetCurrentPuzzleId(), out var record))
        {
            _statsLabel.Text = $"Gold: {record.GoldEarned} | Exp: {record.ExpEarned}\nTime: {record.TimeTakenSeconds}s | Hints: {record.HintsUsed}";
        }
        
        _streakLabel.Text = $"🔥 Streak: {DailyPuzzleSystem.Instance.GetCurrentStreak()} | Best: {DailyPuzzleSystem.Instance.GetBestStreak()}";
    }
    
    private void OnAnswerSubmitted(string text)
    {
        OnSubmitPressed();
    }
    
    private void OnSubmitPressed()
    {
        if (_answerInput.Text.Trim() == "")
            return;
        
        // Stop timer
        _isTimerRunning = false;
        _timer.Stop();
        
        // Calculate time taken
        var puzzle = DailyPuzzleSystem.Instance.GetDailyPuzzle();
        int timeTaken = puzzle.TimeLimit - _timeRemaining;
        
        // Solve puzzle
        var record = DailyPuzzleSystem.Instance.SolvePuzzle(_answerInput.Text, timeTaken, _hintsUsed);
        
        if (record != null)
        {
            // Show success
            _puzzleContainer.Visible = false;
            _resultContainer.Visible = true;
            
            if (record.GoldEarned > 0)
            {
                _resultLabel.Text = "✅ Correct!";
                _resultLabel.Modulate = new Color(0, 1, 0);
                _statsLabel.Text = $"Gold: +{record.GoldEarned} | Exp: +{record.ExpEarned}";
            }
            else
            {
                _resultLabel.Text = "❌ Wrong Answer";
                _resultLabel.Modulate = new Color(1, 0, 0);
                _statsLabel.Text = "Try again tomorrow!";
            }
        }
        
        // Update streak
        _streakLabel.Text = $"🔥 Streak: {DailyPuzzleSystem.Instance.GetCurrentStreak()} | Best: {DailyPuzzleSystem.Instance.GetBestStreak()}";
    }
    
    private void OnHintPressed()
    {
        string hint = DailyPuzzleSystem.Instance.GetHint();
        if (hint != "")
        {
            _hintLabel.Text = "💡 " + hint;
            _hintsUsed++;
            _hintButton.Text = $"💡 Hint ({_hintsUsed} used, -25% reward each)";
        }
    }
    
    private void OnTimerTimeout()
    {
        if (!_isTimerRunning)
            return;
        
        _timeRemaining--;
        _timerLabel.Text = $"Time: {_timeRemaining}s";
        
        if (_timeRemaining <= 0)
        {
            _isTimerRunning = false;
            _timer.Stop();
            
            // Time's up - record as failed
            DailyPuzzleSystem.Instance.SolvePuzzle("", 999, _hintsUsed);
            
            _resultLabel.Text = "⏰ Time's Up!";
            _resultLabel.Modulate = new Color(1, 0.5, 0); // Orange
            _statsLabel.Text = "Try again tomorrow!";
            
            _puzzleContainer.Visible = false;
            _resultContainer.Visible = true;
        }
    }
    
    private void OnClosePressed()
    {
        _isTimerRunning = false;
        _timer.Stop();
        QueueFree();
    }
    
    public override void _ExitTree()
    {
        _isTimerRunning = false;
        _timer?.Stop();
    }
}
