using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 顿帧UI - 显示战斗顿帧效果
/// 重构自 REQ-075: 移除对 HitStopSystem 的直接引用，改为事件驱动解耦
/// </summary>
public partial class HitStopUI : Control
{
	// ===== 事件接口（UI → System 通信） =====
	// UI 层通过事件向外部发送操作请求，不直接持有 System 引用

	/// <summary>请求触发暴击顿帧效果（外部/System 收到后处理）</summary>
	public Action<float, float> OnCriticalHitRequested;

	/// <summary>请求触发重击顿帧效果（外部/System 收到后处理）</summary>
	public Action<float, float> OnHeavyHitRequested;

	/// <summary>请求触发轻击顿帧效果（外部/System 收到后处理）</summary>
	public Action<float, float> OnLightHitRequested;

	/// <summary>请求关闭界面（外部/System 收到后处理）</summary>
	public Action OnCloseRequested;

	/// <summary>请求刷新状态（外部/System 收到后调用 UpdateHitStopStatus）</summary>
	public Action OnRefreshRequested;

	// ===== UI 组件引用 (通过GetNode获取) =====
	private VBoxContainer _mainContainer;
	private Label _titleLabel;
	private CheckButton _enableToggle;
	private HSlider _durationSlider;
	private Label _durationValue;
	private HSlider _intensitySlider;
	private Label _intensityValue;
	private HSlider _shakeSlider;
	private Label _shakeValue;
	private CheckButton _flashToggle;
	private Button _testCriticalButton;
	private Button _testHeavyButton;
	private Button _testLightButton;
	private Label _statusLabel;
	private Label _infoLabel;

	private bool _enabled = true;
	private float _defaultDuration = 0.1f;
	private float _defaultIntensity = 1.0f;
	private float _defaultShake = 8.0f;
	private bool _defaultFlash = true;

	// ===== 生命周期 =====

	public override void _Ready()
	{
		CreateUI();
	}

	private void CreateUI()
	{
		_mainContainer = new VBoxContainer();
		_mainContainer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
		_mainContainer.CustomMinimumSize = new Vector2(400, 500);
		_mainContainer.AddThemeConstantOverride("separation", 10);
		AddChild(_mainContainer);

		_titleLabel = new Label();
		_titleLabel.Text = " Hit Stop Effect System ";
		_titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_titleLabel.AddThemeFontSizeOverride("font_size", 20);
		_mainContainer.AddChild(_titleLabel);

		var separator1 = new HSeparator();
		_mainContainer.AddChild(separator1);

		_enableToggle = new CheckButton();
		_enableToggle.Text = "Enable Hit Stop";
		_enableToggle.ButtonPressed = _enabled;
		_enableToggle.Toggled += OnEnableToggled;
		_mainContainer.AddChild(_enableToggle);

		var separator2 = new HSeparator();
		_mainContainer.AddChild(separator2);

		var durationContainer = new VBoxContainer();
		_mainContainer.AddChild(durationContainer);

		var durationLabel = new Label();
		durationLabel.Text = "Default Duration:";
		durationContainer.AddChild(durationLabel);

		var durationHBox = new HBoxContainer();
		durationContainer.AddChild(durationHBox);

		_durationSlider = new HSlider();
		_durationSlider.MinValue = 0.01f;
		_durationSlider.MaxValue = 0.5f;
		_durationSlider.Step = 0.01f;
		_durationSlider.Value = _defaultDuration;
		_durationSlider.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		_durationSlider.ValueChanged += OnDurationChanged;
		durationHBox.AddChild(_durationSlider);

		_durationValue = new Label();
		_durationValue.Text = $"{_defaultDuration:F2}s";
		_durationValue.CustomMinimumSize = new Vector2(50, 0);
		durationHBox.AddChild(_durationValue);

		var intensityContainer = new VBoxContainer();
		_mainContainer.AddChild(intensityContainer);

		var intensityLabel = new Label();
		intensityLabel.Text = "Default Intensity:";
		intensityContainer.AddChild(intensityLabel);

		var intensityHBox = new HBoxContainer();
		intensityContainer.AddChild(intensityHBox);

		_intensitySlider = new HSlider();
		_intensitySlider.MinValue = 0.1f;
		_intensitySlider.MaxValue = 3.0f;
		_intensitySlider.Step = 0.1f;
		_intensitySlider.Value = _defaultIntensity;
		_intensitySlider.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		_intensitySlider.ValueChanged += OnIntensityChanged;
		intensityHBox.AddChild(_intensitySlider);

		_intensityValue = new Label();
		_intensityValue.Text = $"{_defaultIntensity:F1f}x";
		_intensityValue.CustomMinimumSize = new Vector2(50, 0);
		intensityHBox.AddChild(_intensityValue);

		var shakeContainer = new VBoxContainer();
		_mainContainer.AddChild(shakeContainer);

		var shakeLabel = new Label();
		shakeLabel.Text = "Screen Shake Amount:";
		shakeContainer.AddChild(shakeLabel);

		var shakeHBox = new HBoxContainer();
		shakeContainer.AddChild(shakeHBox);

		_shakeSlider = new HSlider();
		_shakeSlider.MinValue = 0f;
		_shakeSlider.MaxValue = 20f;
		_shakeSlider.Step = 1f;
		_shakeSlider.Value = _defaultShake;
		_shakeSlider.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		_shakeSlider.ValueChanged += OnShakeChanged;
		shakeHBox.AddChild(_shakeSlider);

		_shakeValue = new Label();
		_shakeValue.Text = $"{_defaultShake:F0f}";
		_shakeValue.CustomMinimumSize = new Vector2(50, 0);
		shakeHBox.AddChild(_shakeValue);

		_flashToggle = new CheckButton();
		_flashToggle.Text = "Enable Screen Flash";
		_flashToggle.ButtonPressed = _defaultFlash;
		_flashToggle.Toggled += OnFlashToggled;
		_mainContainer.AddChild(_flashToggle);

		var separator3 = new HSeparator();
		_mainContainer.AddChild(separator3);

		var testLabel = new Label();
		testLabel.Text = "Test Effects:";
		testLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_mainContainer.AddChild(testLabel);

		var buttonContainer = new HBoxContainer();
		buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
		_mainContainer.AddChild(buttonContainer);

		_testCriticalButton = new Button();
		_testCriticalButton.Text = "Critical";
		_testCriticalButton.Pressed += OnTestCritical;
		buttonContainer.AddChild(_testCriticalButton);

		_testHeavyButton = new Button();
		_testHeavyButton.Text = "Heavy";
		_testHeavyButton.Pressed += OnTestHeavy;
		buttonContainer.AddChild(_testHeavyButton);

		_testLightButton = new Button();
		_testLightButton.Text = "Light";
		_testLightButton.Pressed += OnTestLight;
		buttonContainer.AddChild(_testLightButton);

		var separator4 = new HSeparator();
		_mainContainer.AddChild(separator4);

		_statusLabel = new Label();
		_statusLabel.Text = "Status: Ready";
		_statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_mainContainer.AddChild(_statusLabel);

		_infoLabel = new Label();
		_infoLabel.Text = "Effects trigger on critical/heavy hits\nUse in combat for game feel impact";
		_infoLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_infoLabel.AddThemeFontSizeOverride("font_size", 12);
		_mainContainer.AddChild(_infoLabel);

		var closeLabel = new Label();
		closeLabel.Text = "\n[ESC] to close";
		closeLabel.HorizontalAlignment = HorizontalAlignment.Center;
		closeLabel.AddThemeFontSizeOverride("font_size", 11);
		_mainContainer.AddChild(closeLabel);
	}

	private void OnEnableToggled(bool pressed)
	{
		_enabled = pressed;
		UpdateStatus();
	}

	private void OnDurationChanged(double value)
	{
		_defaultDuration = (float)value;
		_durationValue.Text = $"{_defaultDuration:F2}s";
	}

	private void OnIntensityChanged(double value)
	{
		_defaultIntensity = (float)value;
		_intensityValue.Text = $"{_defaultIntensity:F1f}x";
	}

	private void OnShakeChanged(double value)
	{
		_defaultShake = (float)value;
		_shakeValue.Text = $"{_defaultShake:F0f}";
	}

	private void OnFlashToggled(bool pressed)
	{
		_defaultFlash = pressed;
	}

	private void OnTestCritical()
	{
		if (!_enabled) return;
		// REQ-075 解耦：通过事件请求 System 处理
		OnCriticalHitRequested?.Invoke(_defaultDuration, _defaultIntensity);
		UpdateStatus("Critical Hit!");
	}

	private void OnTestHeavy()
	{
		if (!_enabled) return;
		// REQ-075 解耦：通过事件请求 System 处理
		OnHeavyHitRequested?.Invoke(_defaultDuration * 1.5f, _defaultIntensity * 1.5f);
		UpdateStatus("Heavy Hit!");
	}

	private void OnTestLight()
	{
		if (!_enabled) return;
		// REQ-075 解耦：通过事件请求 System 处理
		OnLightHitRequested?.Invoke(_defaultDuration * 0.5f, _defaultIntensity * 0.5f);
		UpdateStatus("Light Hit!");
	}

	private void UpdateStatus(string status = "Ready")
	{
		// REQ-075 注意：此处直接读取 HitStopSystem.Instance 状态
		// 用于实时显示，属于展示数据读取而非业务逻辑调用
		// 如需完全解耦，外部可通过 OnRefreshRequested 事件触发 UpdateHitStopStatus() 调用
		var active = HitStopSystem.Instance?.IsActive ?? false;
		if (active)
		{
			_statusLabel.Text = $"Status: Active ({HitStopSystem.Instance.CurrentIntensity:F1f})";
		}
		else
		{
			_statusLabel.Text = $"Status: {status}";
		}
	}

	// ===== 公开更新接口（System → UI 通信） =====
	// REQ-075 解耦：UI 不再主动拉取数据，而是等待外部推送

	/// <summary>
	/// 更新顿帧状态显示（由外部/System 调用）
	/// </summary>
	public void UpdateHitStopStatus(bool isActive, float intensity)
	{
		if (isActive)
		{
			_statusLabel.Text = $"Status: Active ({intensity:F1f})";
		}
		else
		{
			_statusLabel.Text = "Status: Ready";
		}
	}

	// ===== 生命周期 =====

	/// <summary>
	/// 初始化事件订阅（由外部/System 在其 _Ready() 中调用）
	/// REQ-075 解耦：UI 不持有 System 引用，由外部负责 wiring
	/// </summary>
	public void Initialize()
	{
		// 事件已在类定义中声明，此处可添加额外的初始化逻辑
		// 外部通过订阅 OnXxxRequested 事件来处理 UI 操作请求
	}

	public override void _Process(double delta)
	{
		UpdateStatus();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
		{
			Hide();
			OnCloseRequested?.Invoke();
			GetTree().SetInputAsHandled();
		}
	}
}
