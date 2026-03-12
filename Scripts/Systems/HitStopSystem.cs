using Godot;
using System;

[GlobalClass]
public partial class HitStopEffectData : Resource
{
	[Export] public float Duration { get; set; } = 0.1f;
	[Export] public float Intensity { get; set; } = 1.0f;
	[Export] public bool EnableScreenShake { get; set; } = true;
	[Export] public float ShakeAmount { get; set; } = 5.0f;
	[Export] public Color FlashColor { get; set; } = new Color(1f, 0f, 0f, 0.3f);
	[Export] public bool EnableFlash { get; set; } = true;
}

[GlobalClass]
public partial class HitStopSystem : Node
{
	public static HitStopSystem Instance { get; private set; }
	
	private float _timeScale = 1.0f;
	private float _targetTimeScale = 1.0f;
	private float _currentDuration = 0f;
	private float _shakeIntensity = 0f;
	private Vector2 _shakeOffset = Vector2.Zero;
	private Color _flashColor = Colors.Transparent;
	private Control _flashOverlay;
	
	public override void _Ready()
	{
		Instance = this;
		CreateFlashOverlay();
	}
	
	private void CreateFlashOverlay()
	{
		_flashOverlay = new Control();
		_flashOverlay.Name = "HitStopFlashOverlay";
		_flashOverlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		_flashOverlay.MouseFilter = Control.MouseFilterEnum.Ignore;
		
		var colorRect = new ColorRect();
		colorRect.Name = "FlashColor";
		colorRect.Color = Colors.Transparent;
		colorRect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		
		_flashOverlay.AddChild(colorRect);
		
		var canvas = GetTree().Root.GetCanvasLayer();
		if (canvas != null)
		{
			canvas.AddChild(_flashOverlay);
		}
	}
	
	public void TriggerHitStop(HitStopEffectData effect)
	{
		if (effect == null) return;
		
		_targetTimeScale = 0.0f;
		_currentDuration = effect.Duration * effect.Intensity;
		_shakeIntensity = effect.EnableScreenShake ? effect.ShakeAmount * effect.Intensity : 0f;
		
		if (effect.EnableFlash)
		{
			_flashColor = effect.FlashColor;
			var colorRect = _flashOverlay?.GetNode<ColorRect>("FlashColor");
			if (colorRect != null)
			{
				colorRect.Color = _flashColor;
			}
		}
	}
	
	public void TriggerCriticalHit(float baseDuration = 0.1f, float intensity = 1.0f)
	{
		var effect = new HitStopEffectData
		{
			Duration = baseDuration,
			Intensity = intensity,
			EnableScreenShake = true,
			ShakeAmount = 8.0f,
			EnableFlash = true,
			FlashColor = new Color(1f, 0.3f, 0f, 0.4f)
		};
		TriggerHitStop(effect);
	}
	
	public void TriggerHeavyHit(float baseDuration = 0.15f, float intensity = 1.5f)
	{
		var effect = new HitStopEffectData
		{
			Duration = baseDuration,
			Intensity = intensity,
			EnableScreenShake = true,
			ShakeAmount = 12.0f,
			EnableFlash = true,
			FlashColor = new Color(0.8f, 0f, 0f, 0.5f)
		};
		TriggerHitStop(effect);
	}
	
	public void TriggerLightHit(float baseDuration = 0.05f, float intensity = 0.5f)
	{
		var effect = new HitStopEffectData
		{
			Duration = baseDuration,
			Intensity = intensity,
			EnableScreenShake = false,
			EnableFlash = false
		};
		TriggerHitStop(effect);
	}
	
	public override void _Process(double delta)
	{
		if (_currentDuration > 0)
		{
			_currentDuration -= (float)delta;
			
			if (_currentDuration <= 0)
			{
				_targetTimeScale = 1.0f;
				_shakeIntensity = 0f;
				_flashColor = Colors.Transparent;
				
				var colorRect = _flashOverlay?.GetNode<ColorRect>("FlashColor");
				if (colorRect != null)
				{
					colorRect.Color = Colors.Transparent;
				}
			}
		}
		
		_timeScale = Mathf.Lerp(_timeScale, _targetTimeScale, 0.3f);
		Engine.TimeScale = Math.Max(0.01f, _timeScale);
		
		if (_shakeIntensity > 0)
		{
			_shakeOffset = new Vector2(
				(float)GD.RandRange(-_shakeIntensity, _shakeIntensity),
				(float)GD.RandRange(-_shakeIntensity, _shakeIntensity)
			);
			
			var camera = GetViewport().GetCamera2D();
			if (camera != null)
			{
				camera.Offset = _shakeOffset;
			}
		}
		else
		{
			var camera = GetViewport().GetCamera2D();
			if (camera != null)
			{
				camera.Offset = Vector2.Zero;
			}
		}
	}
	
	public override void _ExitTree()
	{
		Engine.TimeScale = 1.0f;
		if (Instance == this)
		{
			Instance = null;
		}
	}
	
	public bool IsActive => _currentDuration > 0;
	public float CurrentIntensity => _shakeIntensity;
}
