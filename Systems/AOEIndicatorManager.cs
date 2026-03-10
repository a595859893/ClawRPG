using Godot;
using System.Collections.Generic;

public partial class AOEIndicatorManager : Node
{
	public static AOEIndicatorManager Instance { get; private set; }
	
	[Export] public Color playerIndicatorColor = new Color(0.2f, 0.8f, 1.0f, 0.5f);
	[Export] public Color enemyIndicatorColor = new Color(1.0f, 0.2f, 0.2f, 0.6f);
	[Export] public float defaultDuration = 3.0f;
	[Export] public float pulseSpeed = 2.0f;
	
	private Control _indicatorRoot;
	private readonly Dictionary<int, ColorRect> _activeIndicators = new Dictionary<int, ColorRect>();
	private int _nextId = 0;
	
	public override void _Ready()
	{
		Instance = this;
		SetupIndicatorRoot();
	}
	
	private void SetupIndicatorRoot()
	{
		_indicatorRoot = new Control
		{
			Name = "AOEIndicatorRoot",
			MouseFilter = Control.MouseFilterEnum.Ignore
		};
		
		var canvasLayer = new CanvasLayer
		{
			Name = "AOEIndicators",
			Layer = 150
		};
		canvasLayer.AddChild(_indicatorRoot);
		
		// Add to scene tree
		var main = GetTree().CurrentScene;
		if (main != null)
		{
			main.AddChild(canvasLayer);
		}
	}
	
	/// <summary>
	/// Show a circular AOE indicator at screen position
	/// </summary>
	/// <param name="screenPosition">Center position in screen coordinates</param>
	/// <param name="radius">Radius in pixels</param>
	/// <param name="isEnemy">Whether this is for enemy ability</param>
	/// <param name="duration">How long to show (0 = manual dismiss)</param>
	/// <returns>Indicator ID for manual dismissal</returns>
	public int ShowCircularIndicator(Vector2 screenPosition, float radius, bool isEnemy = false, float duration = 0)
	{
		return CreateIndicator(screenPosition, radius, isEnemy, true, duration);
	}
	
	/// <summary>
	/// Show a rectangular AOE indicator at screen position
	/// </summary>
	/// <param name="screenPosition">Top-left corner in screen coordinates</param>
	/// <param name="size">Size in pixels (width, height)</param>
	/// <param name="isEnemy">Whether this is for enemy ability</param>
	/// <param name="duration">How long to show (0 = manual dismiss)</param>
	/// <returns>Indicator ID for manual dismissal</returns>
	public int ShowRectangularIndicator(Vector2 screenPosition, Vector2 size, bool isEnemy = false, float duration = 0)
	{
		return CreateIndicator(screenPosition, size, isEnemy, false, duration);
	}
	
	/// <summary>
	/// Show indicator at world position (converts to screen)
	/// </summary>
	public int ShowAtWorldPosition(Vector3 worldPosition, float radius, bool isEnemy = false, float duration = 0)
	{
		var camera = GetTree().CurrentScene?.GetViewport().GetCamera3D();
		if (camera == null) return -1;
		
		var screenPos = camera.UnprojectPosition(worldPosition);
		screenPos.Y = GetTree().CurrentScene.GetViewport().GetVisibleRect().Size.Y - screenPos.Y;
		
		return ShowCircularIndicator(screenPos, radius, isEnemy, duration);
	}
	
	private int CreateIndicator(Vector2 position, Vector2 size, bool isEnemy, bool isCircular, float duration)
	{
		var indicator = new ColorRect
		{
			Name = $"AOEIndicator_{_nextId}",
			MouseFilter = Control.MouseFilterEnum.Ignore
		};
		
		// Apply shader
		var shader = GD.Load<Shader>("res://Shaders/aoe_indicator.gdshader");
		if (shader != null)
		{
			var material = new ShaderMaterial
			{
				Shader = shader
			};
			material.SetShaderParameter("indicator_color", isEnemy ? enemyIndicatorColor : playerIndicatorColor);
			material.SetShaderParameter("pulse_speed", pulseSpeed);
			material.SetShaderParameter("is_enemy", isEnemy);
			indicator.Material = material;
		}
		
		// Position and size
		if (isCircular)
		{
			indicator.Size = new Vector2(size.X * 2, size.X * 2);
			indicator.Position = position - new Vector2(size.X, size.X);
		}
		else
		{
			indicator.Size = size;
			indicator.Position = position;
		}
		
		_indicatorRoot.AddChild(indicator);
		
		int id = _nextId++;
		_activeIndicators[id] = indicator;
		
		// Auto-dismiss after duration
		if (duration > 0)
		{
			GetTree().CreateTimer(duration).Timeout += () => DismissIndicator(id);
		}
		
		return id;
	}
	
	private int CreateIndicator(Vector2 center, float radius, bool isEnemy, bool isCircular, float duration)
	{
		return CreateIndicator(center, new Vector2(radius, radius), isEnemy, isCircular, duration);
	}
	
	/// <summary>
	/// Dismiss a specific indicator by ID
	/// </summary>
	public void DismissIndicator(int id)
	{
		if (_activeIndicators.TryGetValue(id, out var indicator) && indicator != null)
		{
			indicator.QueueFree();
			_activeIndicators.Remove(id);
		}
	}
	
	/// <summary>
	/// Dismiss all active indicators
	/// </summary>
	public void DismissAll()
	{
		foreach (var kvp in _activeIndicators)
		{
			kvp.Value?.QueueFree();
		}
		_activeIndicators.Clear();
	}
	
	/// <summary>
	/// Get number of active indicators
	/// </summary>
	public int ActiveCount => _activeIndicators.Count;
	
	public override void _ExitTree()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		DismissAll();
	}
}
