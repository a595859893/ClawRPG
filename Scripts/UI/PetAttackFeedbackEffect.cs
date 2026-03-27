using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI;

/// <summary>
/// 宠物攻击屏幕边缘划痕特效 (REQ-139)
/// - 宠物每次攻击时，在屏幕对应边缘闪爪痕
/// - 每N次攻击触发"宠物协同"增益提示（边缘光晕）
/// </summary>
public partial class PetAttackFeedbackEffect : CanvasLayer
{
    public static PetAttackFeedbackEffect Instance { get; private set; }

    // 配置
    [Export] public int SynergyThreshold { get; set; } = 5;
    [Export] public float ScratchDuration { get; set; } = 0.3f;
    [Export] public float ScratchFadeDelay { get; set; } = 0.1f;
    [Export] public Color ScratchColor { get; set; } = new Color(1f, 0.95f, 0.9f, 0.7f);
    [Export] public Color SynergyGlowColor { get; set; } = new Color(1f, 0.85f, 0.4f, 0.6f);
    [Export] public float SynergyGlowDuration { get; set; } = 0.5f;

    // 内部状态
    private int _attackCountSinceSynergy = 0;
    private Control _scratchContainer;
    private List<Line2D> _activeScratches = new();
    private ColorRect _synergyGlowRect;
    private Tween _synergyGlowTween;

    public override void _Ready()
    {
        Instance = this;
        SetupScratchContainer();
        SetupSynergyGlow();
        ConnectSignals();
    }

    private void SetupScratchContainer()
    {
        _scratchContainer = new Control
        {
            Name = "ScratchContainer",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            AnchorsPreset = Control.LayoutPreset.FullRect
        };
        AddChild(_scratchContainer);
    }

    private void SetupSynergyGlow()
    {
        _synergyGlowRect = new ColorRect
        {
            Name = "SynergyGlow",
            Color = new Color(SynergyGlowColor.R, SynergyGlowColor.G, SynergyGlowColor.B, 0f),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        _synergyGlowRect.Ready += () =>
        {
            _synergyGlowRect.Size = GetViewportRect().Size;
            _synergyGlowRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        };
        AddChild(_synergyGlowRect);
    }

    private void ConnectSignals()
    {
        // 监听宠物攻击信号 (PetCombatAI.PetAttacked — 包含 enemy 信息)
        if (PetCombatAI.Instance != null)
        {
            PetCombatAI.Instance.PetAttacked += OnPetAttacked;
        }

        // 也监听 SynergyAttackTriggered 作为后备/补充
        if (PetCombatCompanionSystem.Instance != null)
        {
            PetCombatCompanionSystem.Instance.SynergyAttackTriggered += OnSynergyAttackTriggered;
        }
    }

    private void OnPetAttacked(Node2D enemy, int damage)
    {
        if (enemy == null) return;
        ShowScratchEffectForEnemy(enemy);
    }

    private void OnSynergyAttackTriggered(string petId, string attackType, float syncLevel)
    {
        // 如果 PetAttacked 信号没触发（直接协同攻击），用 PetCombatAI 获取目标
        if (PetCombatAI.Instance != null)
        {
            var target = PetCombatAI.Instance.GetCurrentTarget();
            if (target != null)
            {
                ShowScratchEffectForEnemy(target);
            }
            else
            {
                // 没有目标时，屏幕中央闪烁（兜底）
                ShowCenteredScratch();
            }
        }
        else
        {
            ShowCenteredScratch();
        }
    }

    private void ShowScratchEffectForEnemy(Node2D enemy)
    {
        // 计算屏幕边缘方向：从玩家视角看，敌人大概在哪个屏幕边缘
        var viewportSize = GetViewportRect().Size;
        var screenCenter = viewportSize / 2;

        // 获取宠物位置和敌人位置
        Vector2 enemyScreenPos = GetViewport().GetVisibleRect().Size / 2; // fallback

        if (PetCombatAI.Instance != null && PetCombatAI.Instance.GetPetNode() is Node2D petNode)
        {
            // 转换到屏幕坐标
            var camera = GetTree().CurrentScene?.GetNodeOrNull<Camera2D>("Camera2D");
            if (camera != null)
            {
                enemyScreenPos = camera.UnprojectPosition(enemy.GlobalPosition);
            }
            else
            {
                // fallback: 使用相对偏移
                Vector2 offset = enemy.GlobalPosition - petNode.GlobalPosition;
                enemyScreenPos = screenCenter + offset;
            }
        }

        // 确定边缘：基于敌人屏幕位置相对于中心的方向
        EdgeDirection edge = GetEdgeDirection(enemyScreenPos, screenCenter);

        // 绘制爪痕
        ShowScratchOnEdge(edge);

        // 更新计数
        _attackCountSinceSynergy++;

        // 检查是否达到协同阈值
        if (_attackCountSinceSynergy >= SynergyThreshold)
        {
            TriggerSynergyGlow();
            _attackCountSinceSynergy = 0;
        }
    }

    private EdgeDirection GetEdgeDirection(Vector2 screenPos, Vector2 screenCenter)
    {
        Vector2 dir = screenPos - screenCenter;
        float angle = dir.Angle();

        // 将角度映射到四个边缘
        // angle: -PI to PI, 从右(0), 下(PI/2), 左(PI), 上(-PI/2)
        if (angle > -Mathf.Pi / 4 && angle <= Mathf.Pi / 4)
            return EdgeDirection.Right;
        if (angle > Mathf.Pi / 4 && angle <= 3 * Mathf.Pi / 4)
            return EdgeDirection.Bottom;
        if (angle > 3 * Mathf.Pi / 4 || angle <= -3 * Mathf.Pi / 4)
            return EdgeDirection.Left;
        return EdgeDirection.Top;
    }

    private enum EdgeDirection { Top, Bottom, Left, Right }

    private void ShowScratchOnEdge(EdgeDirection edge)
    {
        var viewportSize = GetViewportRect().Size;

        // 创建爪痕线条
        var scratch = new Line2D
        {
            Name = "Scratch",
            DefaultColor = ScratchColor,
            Width = 3f,
            Antialiased = true
        };

        // 生成爪痕形状（随机曲线）
        GenerateScratchGeometry(scratch, edge, viewportSize);

        _scratchContainer.AddChild(scratch);
        _activeScratches.Add(scratch);

        // 淡出动画
        var tween = CreateTween();
        tween.TweenProperty(scratch, "modulate:a", 0f, ScratchDuration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
        tween.TweenCallback(Callable.From(() => OnScratchFadeComplete(scratch)));
    }

    private void GenerateScratchGeometry(Line2D scratch, EdgeDirection edge, Vector2 screenSize)
    {
        var random = Random.Shared;
        float margin = 20f;

        switch (edge)
        {
            case EdgeDirection.Top:
                {
                    float y = margin;
                    float x1 = margin + (float)random.NextDouble() * (screenSize.X * 0.2f);
                    float x2 = x1 + 40f + (float)random.NextDouble() * 60f;
                    float x3 = x2 + 20f + (float)random.NextDouble() * 40f;
                    scratch.AddPoint(new Vector2(x1, y));
                    scratch.AddPoint(new Vector2(x1 + 15f, y + 30f));
                    scratch.AddPoint(new Vector2(x2, y + 25f));
                    scratch.AddPoint(new Vector2(x2 + 10f, y + 45f));
                    scratch.AddPoint(new Vector2(x3, y + 40f));
                    break;
                }
            case EdgeDirection.Bottom:
                {
                    float y = screenSize.Y - margin;
                    float x1 = margin + (float)random.NextDouble() * (screenSize.X * 0.2f);
                    float x2 = x1 + 40f + (float)random.NextDouble() * 60f;
                    float x3 = x2 + 20f + (float)random.NextDouble() * 40f;
                    scratch.AddPoint(new Vector2(x1, y));
                    scratch.AddPoint(new Vector2(x1 + 15f, y - 30f));
                    scratch.AddPoint(new Vector2(x2, y - 25f));
                    scratch.AddPoint(new Vector2(x2 + 10f, y - 45f));
                    scratch.AddPoint(new Vector2(x3, y - 40f));
                    break;
                }
            case EdgeDirection.Left:
                {
                    float x = margin;
                    float y1 = margin + (float)random.NextDouble() * (screenSize.Y * 0.2f);
                    float y2 = y1 + 40f + (float)random.NextDouble() * 60f;
                    float y3 = y2 + 20f + (float)random.NextDouble() * 40f;
                    scratch.AddPoint(new Vector2(x, y1));
                    scratch.AddPoint(new Vector2(x + 30f, y1 + 15f));
                    scratch.AddPoint(new Vector2(x + 25f, y2));
                    scratch.AddPoint(new Vector2(x + 45f, y2 + 10f));
                    scratch.AddPoint(new Vector2(x + 40f, y3));
                    break;
                }
            case EdgeDirection.Right:
                {
                    float x = screenSize.X - margin;
                    float y1 = margin + (float)random.NextDouble() * (screenSize.Y * 0.2f);
                    float y2 = y1 + 40f + (float)random.NextDouble() * 60f;
                    float y3 = y2 + 20f + (float)random.NextDouble() * 40f;
                    scratch.AddPoint(new Vector2(x, y1));
                    scratch.AddPoint(new Vector2(x - 30f, y1 + 15f));
                    scratch.AddPoint(new Vector2(x - 25f, y2));
                    scratch.AddPoint(new Vector2(x - 45f, y2 + 10f));
                    scratch.AddPoint(new Vector2(x - 40f, y3));
                    break;
                }
        }
    }

    private void ShowCenteredScratch()
    {
        // 没有目标时，在屏幕中央偏下显示爪痕
        var viewportSize = GetViewportRect().Size;
        var scratch = new Line2D
        {
            Name = "Scratch",
            DefaultColor = ScratchColor,
            Width = 3f,
            Antialiased = true
        };

        float cx = viewportSize.X / 2;
        float cy = viewportSize.Y * 0.7f;
        scratch.AddPoint(new Vector2(cx - 50f, cy));
        scratch.AddPoint(new Vector2(cx - 30f, cy - 20f));
        scratch.AddPoint(new Vector2(cx, cy - 15f));
        scratch.AddPoint(new Vector2(cx + 30f, cy - 20f));
        scratch.AddPoint(new Vector2(cx + 50f, cy));

        _scratchContainer.AddChild(scratch);
        _activeScratches.Add(scratch);

        var tween = CreateTween();
        tween.TweenProperty(scratch, "modulate:a", 0f, ScratchDuration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
        tween.TweenCallback(Callable.From(() => OnScratchFadeComplete(scratch)));
    }

    private void OnScratchFadeComplete(Line2D scratch)
    {
        if (scratch.IsInsideTree())
        {
            scratch.QueueFree();
        }
        _activeScratches.Remove(scratch);
    }

    private void TriggerSynergyGlow()
    {
        // 边缘金色光晕
        if (_synergyGlowTween != null && _synergyGlowTween.IsValid())
        {
            _synergyGlowTween.Kill();
        }

        _synergyGlowRect.Modulate = new Color(SynergyGlowColor.R, SynergyGlowColor.G, SynergyGlowColor.B, 0.8f);

        _synergyGlowTween = CreateTween();
        _synergyGlowTween.TweenProperty(_synergyGlowRect, "modulate:a", 0f, SynergyGlowDuration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        GD.Print($"[PetAttackFeedback] Synergy glow triggered! ({SynergyThreshold} attacks reached)");
    }

    public override void _EnterTree()
    {
        // 确保屏幕尺寸变化时同步
        GetTree().Connect("screen_resized", Callable.From(OnScreenResized));
    }

    private void OnScreenResized()
    {
        if (_synergyGlowRect != null && _synergyGlowRect.IsInsideTree())
        {
            _synergyGlowRect.Size = GetViewportRect().Size;
        }
    }

    public int GetAttackCountSinceSynergy() => _attackCountSinceSynergy;

    public void ResetSynergyCount()
    {
        _attackCountSinceSynergy = 0;
    }
}
