using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Combo Echo Trail System — REQ-130
/// Visualizes the last 3 combo execution paths as fading trails on screen.
/// </summary>
public partial class ComboEchoTrailSystem : BaseSystem
{
    public static ComboEchoTrailSystem Instance { get; private set; }

    // ─── Settings ────────────────────────────────────────────────────────────
    [Export] private bool _showComboTrails = true;
    public bool ShowComboTrails => _showComboTrails && GameSettings.Instance?.ShowComboTrails ?? false;

    // ─── Trail Storage ───────────────────────────────────────────────────────
    private class EchoTrail
    {
        public string ComboId;
        public List<Vector2> Points = new List<Vector2>();
        public float Age;            // seconds since creation
        public float MaxAge = 3.0f; // fade out over 3 seconds
        public float Alpha = 1.0f;
        public bool IsRepeating;     // true if this combo was executed before
        public Color BaseColor;
    }

    private List<EchoTrail> _trails = new List<EchoTrail>();
    private const int MAX_TRAILS = 3;

    // ─── Recording ──────────────────────────────────────────────────────────
    private bool _isRecording;
    private EchoTrail _currentTrail;
    private float _recordAccumulator;
    private const float RECORD_INTERVAL = 0.05f; // sample every 50ms
    private HashSet<string> _executedComboIds = new HashSet<string>();

    // ─── Object Pool ────────────────────────────────────────────────────────
    private class TrailLine2D
    {
        public Line2D Node;
        public bool InUse;
    }
    private List<TrailLine2D> _pool = new List<TrailLine2D>();
    private const int POOL_SIZE = 8;

    // ─── Canvas ─────────────────────────────────────────────────────────────
    private CanvasLayer _canvas;
    private Control _trailContainer;

    // ─── Combo Tracking ─────────────────────────────────────────────────────
    private string _lastExecutedComboId = "";

    public override void _Ready()
    {
        Instance = this;
        _InitializePool();
        _InitializeCanvas();
        _SubscribeToSignals();
        GD.Print("[ComboEchoTrailSystem] Initialized");
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        // Record player position while recording
        if (_isRecording && _currentTrail != null)
        {
            _recordAccumulator += dt;
            if (_recordAccumulator >= RECORD_INTERVAL)
            {
                _recordAccumulator = 0f;
                _RecordPoint();
            }
        }

        // Age and fade trails
        for (int i = _trails.Count - 1; i >= 0; i--)
        {
            var trail = _trails[i];
            trail.Age += dt;
            trail.Alpha = Mathf.Clamp(1f - (trail.Age / trail.MaxAge), 0f, 1f);

            if (trail.Age >= trail.MaxAge)
            {
                _ReleaseTrail(i);
            }
        }

        // Update Line2D visuals
        _UpdateVisuals();
    }

    // ─── Initialization ──────────────────────────────────────────────────────

    private void _InitializePool()
    {
        for (int i = 0; i < POOL_SIZE; i++)
        {
            var line = new Line2D();
            line.DefaultColor = Colors.White;
            line.Width = 3f;
            line.BeginCapMode = Line2D.LineCapMode.Round;
            line.EndCapMode = Line2D.LineCapMode.Round;
            line.JointMode = Line2D.LineJointMode.LineJointModeRound;
            line.Visible = false;
            _pool.Add(new TrailLine2D { Node = line, InUse = false });
        }
    }

    private void _InitializeCanvas()
    {
        _canvas = new CanvasLayer();
        _canvas.Layer = 150; // above most UI
        _trailContainer = new Control();
        _trailContainer.SetAnchorsAndMarginsPreset(Control.LayoutPreset.FullRect);
        _canvas.AddChild(_trailContainer);

        foreach (var tl in _pool)
        {
            _trailContainer.AddChild(tl.Node);
        }

        GetTree().CurrentScene.AddChild(_canvas);
    }

    private void _SubscribeToSignals()
    {
        // Subscribe to SkillComboSystem combo completion
        if (SkillComboSystem.Instance != null)
        {
            SkillComboSystem.Instance.ComboCompleted += OnComboCompleted;
            GD.Print("[ComboEchoTrailSystem] Connected to SkillComboSystem.ComboCompleted");
        }
        else
        {
            GD.PushWarning("[ComboEchoTrailSystem] SkillComboSystem.Instance is null at init");
        }

        // Also listen to old ComboSystem signal
        ComboSystem.ComboExecuted += OnLegacyComboExecuted;

        // Register to SystemInitializationManager
        // (handled by class registration in SystemInitializationManager.cs)
    }

    // ─── Recording ───────────────────────────────────────────────────────────

    private void _StartRecording(string comboId)
    {
        if (!ShowComboTrails) return;

        _isRecording = true;
        _recordAccumulator = 0f;
        _currentTrail = new EchoTrail
        {
            ComboId = comboId,
            Age = 0f,
            IsRepeating = _executedComboIds.Contains(comboId),
            BaseColor = _GetComboColor(comboId)
        };
        _executedComboIds.Add(comboId);

        // Add to trails list (keep only MAX_TRAILS)
        _trails.Add(_currentTrail);
        if (_trails.Count > MAX_TRAILS)
        {
            _ReleaseTrail(0);
        }

        _RecordPoint(); // record first point immediately
    }

    private void _StopRecording()
    {
        _isRecording = false;
        _currentTrail = null;
    }

    private void _RecordPoint()
    {
        if (_currentTrail == null) return;

        var player = GetTree().GetFirstNodeInGroup("Player");
        if (player != null)
        {
            // Store world position projected to screen
            var worldPos = player is Node2D node2d ? node2d.GlobalPosition : player.Position;
            _currentTrail.Points.Add(worldPos);
        }
    }

    // ─── Signal Handlers ────────────────────────────────────────────────────

    private void OnComboCompleted(string comboId, int streak)
    {
        _lastExecutedComboId = comboId;
        _StopRecording(); // stop any in-progress recording

        // Brief delay before starting next trail to let VFX breathe
        _StartRecording(comboId);

        // Auto-stop recording after a short window (trail records during combo)
        var timer = new Godot.Timer();
        timer.WaitTime = 1.5f;
        timer.OneShot = true;
        timer.Timeout += () =>
        {
            _StopRecording();
            timer.QueueFree();
        };
        AddChild(timer);
        timer.Start();
    }

    private void OnLegacyComboExecuted(string comboId, float damage, string effectName)
    {
        // Legacy ComboSystem — record this too
        if (string.IsNullOrEmpty(_lastExecutedComboId) || _lastExecutedComboId != comboId)
        {
            _lastExecutedComboId = comboId;
            _StopRecording();
            _StartRecording(comboId);

            var timer = new Godot.Timer();
            timer.WaitTime = 1.5f;
            timer.OneShot = true;
            timer.Timeout += () =>
            {
                _StopRecording();
                timer.QueueFree();
            };
            AddChild(timer);
            timer.Start();
        }
    }

    // ─── Visual Update ───────────────────────────────────────────────────────

    private void _UpdateVisuals()
    {
        if (!ShowComboTrails) return;

        // Assign trail visuals from pool
        int poolIdx = 0;
        for (int i = 0; i < _trails.Count && poolIdx < _pool.Count; i++)
        {
            var trail = _trails[i];
            if (trail.Points.Count < 2)
            {
                if (_pool[poolIdx].InUse)
                {
                    _pool[poolIdx].Node.Visible = false;
                    _pool[poolIdx].InUse = false;
                }
                poolIdx++;
                continue;
            }

            var tl = _pool[poolIdx];
            tl.InUse = true;
            tl.Node.Visible = true;

            // Set points
            tl.Node.ClearPoints();
            foreach (var pt in trail.Points)
            {
                tl.Node.AddPoint(pt);
            }

            // Color: base color * alpha, repeating combos are more opaque
            float repeatBoost = trail.IsRepeating ? 0.3f : 0f;
            float finalAlpha = (trail.Alpha + repeatBoost * (1f - trail.Alpha)) * 0.8f;
            tl.Node.DefaultColor = new Color(trail.BaseColor.R, trail.BaseColor.G, trail.BaseColor.B, finalAlpha);

            // Width tapers with age (newer = thicker)
            tl.Node.Width = Mathf.Lerp(1.5f, 4f, trail.Alpha);

            poolIdx++;
        }

        // Hide unused pool entries
        for (int i = poolIdx; i < _pool.Count; i++)
        {
            if (_pool[i].InUse)
            {
                _pool[i].Node.Visible = false;
                _pool[i].InUse = false;
            }
        }
    }

    // ─── Cleanup ────────────────────────────────────────────────────────────

    private void _ReleaseTrail(int index)
    {
        if (index < 0 || index >= _trails.Count) return;
        _trails.RemoveAt(index);
    }

    private void _ClearAllTrails()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            _pool[i].Node.Visible = false;
            _pool[i].InUse = false;
        }
        _trails.Clear();
    }

    // ─── Settings ───────────────────────────────────────────────────────────

    /// <summary>
    /// Call this to toggle trail visibility from settings UI.
    /// </summary>
    public void SetShowComboTrails(bool show)
    {
        _showComboTrails = show;
        if (!show)
        {
            _ClearAllTrails();
        }
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private Color _GetComboColor(string comboId)
    {
        // Color-code by combo type / rarity if available
        if (SkillComboSystem.Instance != null)
        {
            var db = SkillComboDatabase.Instance;
            if (db != null && db.TryGetCombo(comboId, out var combo))
            {
                return combo.Rarity switch
                {
                    SkillComboDatabase.ComboRarity.Common => new Color(0.7f, 0.7f, 0.7f, 1f),    // grey
                    SkillComboDatabase.ComboRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f, 1f), // green
                    SkillComboDatabase.ComboRarity.Rare => new Color(0.2f, 0.5f, 1f, 1f),       // blue
                    SkillComboDatabase.ComboRarity.Epic => new Color(0.7f, 0.3f, 1f, 1f),      // purple
                    SkillComboDatabase.ComboRarity.Legendary => new Color(1f, 0.7f, 0.2f, 1f), // gold
                    _ => new Color(0.5f, 0.8f, 1f, 1f)
                };
            }
        }
        return new Color(0.5f, 0.8f, 1f, 1f); // default cyan
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        // Re-subscribe if tree changes
        if (SkillComboSystem.Instance != null)
        {
            SkillComboSystem.Instance.ComboCompleted += OnComboCompleted;
        }
        ComboSystem.ComboExecuted += OnLegacyComboExecuted;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        if (SkillComboSystem.Instance != null)
        {
            SkillComboSystem.Instance.ComboCompleted -= OnComboCompleted;
        }
        ComboSystem.ComboExecuted -= OnLegacyComboExecuted;
        _ClearAllTrails();
    }
}
