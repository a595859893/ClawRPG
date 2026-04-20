using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class EconomicWarningUI : Control
{
    private EconomicWarningSystem _system;
    private TabContainer _tabContainer;
    private VBoxContainer _activeWarningsContainer;
    private VBoxContainer _historyContainer;
    private VBoxContainer _indicatorsContainer;
    private VBoxContainer _statisticsContainer;
    private Label _statusLabel;

    private Color _infoColor = new Color(0.4f, 0.6f, 1f);
    private Color _warningColor = new Color(1f, 0.8f, 0.2f);
    private Color _criticalColor = new Color(1f, 0.3f, 0.3f);

    public override void _Ready()
    {
        _system = GetNode<EconomicWarningSystem>("/root/EconomicWarningSystem");
        SetupUI();
    }

    private void SetupUI()
    {
        // Main panel
        var panel = new PanelContainer
        {
            AnchorRight = Vector2.One,
            AnchorBottom = Vector2.One,
            Margin = new Margin(50, 50, -50, -50)
        };
        AddChild(panel);

        var mainVBox = new VBoxContainer();
        panel.AddChild(mainVBox);

        // Header
        var header = new HBoxContainer();
        mainVBox.AddChild(header);

        var titleLabel = new Label
        {
            Text = "Economic Warning System",
            Align = Label.AlignEnum.Center
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        header.AddChild(titleLabel);

        header.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });

        var closeBtn = new Button { Text = "X" };
        closeBtn.Pressed += () => Hide();
        header.AddChild(closeBtn);

        // Status
        _statusLabel = new Label
        {
            Text = "Monitoring...",
            Align = Label.AlignEnum.Center
        };
        _statusLabel.AddThemeFontSizeOverride("font_size", 14);
        mainVBox.AddChild(_statusLabel);

        // Tab container
        _tabContainer = new TabContainer
        {
            SizeFlagsVertical = Control.SizeFlags.Expand
        };
        mainVBox.AddChild(_tabContainer);

        // Active Warnings tab
        var activeTab = new Control();
        _tabContainer.AddChild(activeTab);
        _tabContainer.SetTabTitle(0, "Active Warnings");
        
        var activeScroll = new ScrollContainer { SizeFlagsVertical = Control.SizeFlags.Expand };
        activeTab.AddChild(activeScroll);
        
        _activeWarningsContainer = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.Expand };
        activeScroll.AddChild(_activeWarningsContainer);

        // History tab
        var historyTab = new Control();
        _tabContainer.AddChild(historyTab);
        _tabContainer.SetTabTitle(1, "History");
        
        var historyScroll = new ScrollContainer { SizeFlagsVertical = Control.SizeFlags.Expand };
        historyTab.AddChild(historyScroll);
        
        _historyContainer = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.Expand };
        historyScroll.AddChild(_historyContainer);

        // Indicators tab
        var indicatorsTab = new Control();
        _tabContainer.AddChild(indicatorsTab);
        _tabContainer.SetTabTitle(2, "Indicators");
        
        var indicatorsScroll = new ScrollContainer { SizeFlagsVertical = Control.SizeFlags.Expand };
        indicatorsTab.AddChild(indicatorsScroll);
        
        _indicatorsContainer = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.Expand };
        indicatorsScroll.AddChild(_indicatorsContainer);

        // Statistics tab
        var statsTab = new Control();
        _tabContainer.AddChild(statsTab);
        _tabContainer.SetTabTitle(3, "Statistics");
        
        var statsScroll = new ScrollContainer { SizeFlagsVertical = Control.SizeFlags.Expand };
        statsTab.AddChild(statsScroll);
        
        _statisticsContainer = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.Expand };
        statsScroll.AddChild(_statisticsContainer);

        // Control buttons
        var buttonBar = new HBoxContainer();
        mainVBox.AddChild(buttonBar);

        var refreshBtn = new Button { Text = "Refresh" };
        refreshBtn.Pressed += RefreshData;
        buttonBar.AddChild(refreshBtn);

        var checkBtn = new Button { Text = "Run Check" };
        checkBtn.Pressed += () => _system.ManualCheck();
        buttonBar.AddChild(checkBtn);

        var clearBtn = new Button { Text = "Clear All" };
        clearBtn.Pressed += () => _system.ClearAllWarnings();
        buttonBar.AddChild(clearBtn);

        // Initial load
        RefreshData();

        // Auto-refresh
        var timer = new Timer { WaitTime = 5f, Autostart = true };
        AddChild(timer);
        timer.Timeout += RefreshData;
    }

    private void RefreshData()
    {
        RefreshActiveWarnings();
        RefreshHistory();
        RefreshIndicators();
        RefreshStatistics();
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var warnings = _system.GetActiveWarnings();
        int critical = warnings.Count(w => w.Severity == WarningSeverity.Critical);
        int warning = warnings.Count(w => w.Severity == WarningSeverity.Warning);
        int info = warnings.Count(w => w.Severity == WarningSeverity.Info);

        string status = $"Active: {warnings.Count}";
        if (critical > 0) status += $" | Critical: {critical}";
        if (warning > 0) status += $" | Warning: {warning}";
        if (info > 0) status += $" | Info: {info}";

        _statusLabel.Text = status;
        
        if (critical > 0)
            _statusLabel.Modulate = _criticalColor;
        else if (warning > 0)
            _statusLabel.Modulate = _warningColor;
        else
            _statusLabel.Modulate = _infoColor;
    }

    private void RefreshActiveWarnings()
    {
        foreach (var child in _activeWarningsContainer.GetChildren())
            child.QueueFree();

        var warnings = _system.GetActiveWarnings();

        if (warnings.Count == 0)
        {
            var emptyLabel = new Label { Text = "No active warnings" };
            _activeWarningsContainer.AddChild(emptyLabel);
            return;
        }

        foreach (var warning in warnings.OrderByDescending(w => w.Severity))
        {
            var card = CreateWarningCard(warning);
            _activeWarningsContainer.AddChild(card);
        }
    }

    private void RefreshHistory()
    {
        foreach (var child in _historyContainer.GetChildren())
            child.QueueFree();

        var warnings = _system.GetAllWarnings().Take(50).ToList();

        if (warnings.Count == 0)
        {
            var emptyLabel = new Label { Text = "No warning history" };
            _historyContainer.AddChild(emptyLabel);
            return;
        }

        foreach (var warning in warnings)
        {
            var card = CreateWarningCard(warning);
            _historyContainer.AddChild(card);
        }
    }

    private Control CreateWarningCard(WarningRecord warning)
    {
        var panel = new PanelContainer { Margin = new Margin(5, 5, 5, 5) };
        
        var color = warning.Severity switch
        {
            WarningSeverity.Critical => _criticalColor,
            WarningSeverity.Warning => _warningColor,
            _ => _infoColor
        };
        
        var style = new StyleBoxFlat { BorderColor = color, BorderWidthLeft = 4 };
        panel.AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer();
        panel.AddChild(vbox);

        var header = new HBoxContainer();
        vbox.AddChild(header);

        var severityIcon = new Label
        {
            Text = warning.Severity switch
            {
                WarningSeverity.Critical => "🔴",
                WarningSeverity.Warning => "🟡",
                _ => "🔵"
            }
        };
        header.AddChild(severityIcon);

        var title = new Label { Text = warning.Title };
        title.AddThemeFontSizeOverride("font_size", 16);
        header.AddChild(title);

        header.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });

        if (warning.IsActive)
        {
            var ackBtn = new Button { Text = "Acknowledge" };
            ackBtn.Pressed += () => _system.AcknowledgeWarning(warning.WarningId);
            header.AddChild(ackBtn);
        }

        var desc = new Label { Text = warning.Description };
        desc.Autowrap = true;
        vbox.AddChild(desc);

        var stats = new Label
        {
            Text = $"Value: {warning.Value:F2} | Threshold: {warning.Severity:F2} | {warning.WarningType}"
        };
        stats.AddThemeFontSizeOverride("font_size", 12);
        vbox.AddChild(stats);

        if (!string.IsNullOrEmpty(warning.RecommendedAction))
        {
            var action = new Label
            {
                Text = $"💡 {warning.RecommendedAction}",
                Autowrap = true
            };
            action.AddThemeFontSizeOverride("font_size", 12);
            vbox.AddChild(action);
        }

        return panel;
    }

    private void RefreshIndicators()
    {
        foreach (var child in _indicatorsContainer.GetChildren())
            child.QueueFree();

        var values = _system.GetIndicatorValues();
        var db = EconomicWarningDatabase.Instance;

        foreach (var config in db.IndicatorConfigs)
        {
            var panel = new PanelContainer { Margin = new Margin(5, 5, 5, 5) };
            _indicatorsContainer.AddChild(panel);

            var vbox = new VBoxContainer();
            panel.AddChild(vbox);

            var header = new HBoxContainer();
            vbox.AddChild(header);

            var name = new Label { Text = config.Name };
            name.AddThemeFontSizeOverride("font_size", 14);
            header.AddChild(name);

            header.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });

            float value = values.TryGetValue(config.IndicatorId, out float v) ? v : 0f;
            var valueLabel = new Label { Text = $"Value: {value:F2}" };
            header.AddChild(valueLabel);

            var desc = new Label { Text = config.Description };
            desc.AddThemeFontSizeOverride("font_size", 12);
            vbox.AddChild(desc);

            var range = new Label
            {
                Text = $"Healthy Range: {config.HealthyRange.x:F2} - {config.HealthyRange.y:F2}"
            };
            range.AddThemeFontSizeOverride("font_size", 12);
            vbox.AddChild(range);

            // Progress bar
            var progress = new ProgressBar
            {
                MinValue = config.MinValue,
                MaxValue = config.MaxValue,
                Value = value,
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            progress.CustomMinimumSize = new Vector2(0, 20);
            vbox.AddChild(progress);

            bool isHealthy = value >= config.HealthyRange.x && value <= config.HealthyRange.y;
            var status = new Label
            {
                Text = isHealthy ? "✅ Healthy" : "⚠️ Outside Range",
                Align = Label.AlignEnum.Center
            };
            status.Modulate = isHealthy ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.5f, 0.5f);
            vbox.AddChild(status);
        }
    }

    private void RefreshStatistics()
    {
        foreach (var child in _statisticsContainer.GetChildren())
            child.QueueFree();

        var stats = _system.GetStatistics();

        var grid = new GridContainer { Columns = 2 };
        _statisticsContainer.AddChild(grid);

        AddStatRow(grid, "Total Warnings Generated", stats.TotalWarningsGenerated.ToString());
        AddStatRow(grid, "Warnings Triggered", stats.WarningsTriggered.ToString());
        AddStatRow(grid, "Warnings Resolved", stats.WarningsResolved.ToString());
        AddStatRow(grid, "Critical Warnings", stats.CriticalWarnings.ToString());
        AddStatRow(grid, "Avg Resolution Time", $"{stats.AverageResolutionTime:F1}s");

        var typeHeader = new Label { Text = "By Type:" };
        typeHeader.AddThemeFontSizeOverride("font_size", 14);
        grid.AddChild(typeHeader);
        grid.AddChild(new Control());

        foreach (var kvp in stats.WarningTypeCounts)
        {
            AddStatRow(grid, kvp.Key, kvp.Value.ToString());
        }

        var resetBtn = new Button { Text = "Reset Statistics" };
        resetBtn.Pressed += () => _system.ResetStatistics();
        _statisticsContainer.AddChild(resetBtn);
    }

    private void AddStatRow(GridContainer grid, string label, string value)
    {
        var labelNode = new Label { Text = label };
        grid.AddChild(labelNode);
        
        var valueNode = new Label { Text = value };
        valueNode.HorizontalAlignment = HorizontalAlignment.Right;
        grid.AddChild(valueNode);
    }

    public override void _Input(InputEvent ev)
    {
        if (ev is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Scancode == KeyList.Escape)
        {
            Hide();
        }
    }
}
