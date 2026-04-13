using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.PetSoul
{
    /// <summary>
    /// 宠物灵魂 UI — Safe House 中的灵魂可视化
    /// 负责显示游荡的灵魂节点、独白气泡、升华光环
    /// </summary>
    public partial class PetSoulGhostUI : CanvasLayer
    {
        /// <summary>灵魂节点容器</summary>
        private Control _ghostContainer;

        /// <summary>独白气泡标签</summary>
        private Label _monologueBubble;

        /// <summary>升华光环节点（围绕玩家）</summary>
        private Control _guardianAura;

        /// <summary>灵魂节点缓存 PetId -> Node</summary>
        private Dictionary<int, Control> _ghostNodes = new Dictionary<int, Control>();

        /// <summary>升华光环节点缓存</summary>
        private Dictionary<int, Control> _auraNodes = new Dictionary<int, Control>();

        /// <summary>独白气泡计时器</summary>
        private float _monologueTimer = 0f;
        private const float MONOLOGUE_DISPLAY_TIME = 4f;
        private int _currentMonologuePetId = -1;
        private string _currentMonologueText = "";

        public override void _Ready()
        {
            base._Ready();

            SetupGhostContainer();
            SetupMonologueBubble();
            SetupGuardianAura();
            SubscribeToSignals();

            // 初始同步：显示已有的升华守护灵
            if (PetSoulGhostSystem.Instance != null)
            {
                foreach (var ghost in PetSoulGhostSystem.Instance.GetTranscendedGhosts())
                {
                    AddGuardianAura(ghost);
                }
            }
        }

        private void SetupGhostContainer()
        {
            _ghostContainer = new Control();
            _ghostContainer.Name = "GhostContainer";
            _ghostContainer.SetAnchorsPreset(Control.LayoutPreset.Wide);
            _ghostContainer.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            _ghostContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
            AddChild(_ghostContainer);
        }

        private void SetupMonologueBubble()
        {
            _monologueBubble = new Label();
            _monologueBubble.Name = "MonologueBubble";
            _monologueBubble.HorizontalAlignment = HorizontalAlignment.Center;
            _monologueBubble.VerticalAlignment = VerticalAlignment.Center;
            _monologueBubble.Modulate = new Color(1f, 0.9f, 0.95f, 0f);
            _monologueBubble.AddThemeColorOverride("font_color", new Color(0.9f, 0.85f, 0.95f));
            _monologueBubble.AddThemeFontSizeOverride("font_size", 14);
            _monologueBubble.Position = new Vector2(0, -60);
            _monologueBubble.Size = new Vector2(300, 40);

            // 气泡背景
            var panel = new Panel();
            panel.Name = "BubbleBackground";
            panel.Modulate = new Color(0.15f, 0.1f, 0.2f, 0.85f);
            panel.SetAnchorsPreset(Control.LayoutPreset.Center);
            panel.Position = new Vector2(-150, -85);
            panel.Size = new Vector2(300, 50);
            _monologueBubble.AddSibling(panel);
            panel.MoveChild(_monologueBubble, 0);

            AddChild(_monologueBubble);
        }

        private void SetupGuardianAura()
        {
            _guardianAura = new Control();
            _guardianAura.Name = "GuardianAura";
            _guardianAura.SetAnchorsPreset(Control.LayoutPreset.Wide);
            _guardianAura.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            _guardianAura.SizeFlagsVertical = Control.SizeFlags.Expand;
            _guardianAura.MouseFilter = Control.MouseFilterEnum.Ignore;
            AddChild(_guardianAura);
        }

        private void SubscribeToSignals()
        {
            if (PetSoulGhostSystem.Instance == null)
                return;

            PetSoulGhostSystem.Instance.OnSoulAdded += OnSoulAdded;
            PetSoulGhostSystem.Instance.OnSoulStateChanged += OnSoulStateChanged;
            PetSoulGhostSystem.Instance.OnSoulTranscended += OnSoulTranscended;
            PetSoulGhostSystem.Instance.OnSoulMonologue += OnSoulMonologue;
            PetSoulGhostSystem.Instance.OnSoulInteracted += OnSoulInteracted;
        }

        private void OnSoulAdded(PetSoulGhostEntry soul)
        {
            if (soul.IsTranscended)
                AddGuardianAura(soul);
            else
                AddGhostNode(soul);
        }

        private void OnSoulStateChanged(int petId, SoulState oldState, SoulState newState)
        {
            // 状态变化时可以更新视觉效果
            if (newState == SoulState.NearPlayer)
            {
                UpdateGhostNodeForNearPlayer(petId);
            }
            else if (newState == SoulState.Wandering)
            {
                ResetGhostNodeFromNearPlayer(petId);
            }
        }

        private void OnSoulTranscended(int petId)
        {
            // 移除游荡灵魂节点，添加升华光环
            RemoveGhostNode(petId);
            var soul = PetSoulGhostSystem.Instance?.GetGhost(petId);
            if (soul != null)
                AddGuardianAura(soul);
        }

        private void OnSoulMonologue(int petId, string text)
        {
            ShowMonologueBubble(petId, text);
        }

        private void OnSoulInteracted(int petId)
        {
            PlayInteractionAnimation(petId);
        }

        /// <summary>
        /// 添加灵魂节点到 Safe House
        /// </summary>
        private void AddGhostNode(PetSoulGhostEntry soul)
        {
            if (_ghostNodes.ContainsKey(soul.PetId))
                return;

            var ghostNode = CreateGhostNode(soul);
            _ghostContainer.AddChild(ghostNode);
            _ghostNodes[soul.PetId] = ghostNode;
            UpdateGhostNodePosition(soul.PetId, soul.WanderPosition);
        }

        private void RemoveGhostNode(int petId)
        {
            if (_ghostNodes.TryGetValue(petId, out var node))
            {
                node.QueueFree();
                _ghostNodes.Remove(petId);
            }
        }

        private Control CreateGhostNode(PetSoulGhostEntry soul)
        {
            var container = new Control();
            container.Name = $"Ghost_{soul.PetId}";
            container.Size = new Vector2(60, 80);
            container.Position = new Vector2(0, 0);

            // 灵魂主体 — 圆形渐变精灵（程序化）
            var ghostBody = new ColorRect
            {
                Name = "GhostBody",
                Size = new Vector2(40, 40),
                Position = new Vector2(10, 10),
                Color = ParseColor(soul.PetColor).WithAlpha(0.5f),
                MouseFilter = Control.MouseFilterEnum.Ignore
            };

            // 灵魂外发光
            var glow = new ColorRect
            {
                Name = "GhostGlow",
                Size = new Vector2(50, 50),
                Position = new Vector2(5, 5),
                Color = ParseColor(soul.PetColor).WithAlpha(0.2f),
                MouseFilter = Control.MouseFilterEnum.Ignore
            };

            // 灵魂名称标签
            var nameLabel = new Label
            {
                Name = "NameLabel",
                Text = soul.PetName,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Position = new Vector2(-20, 55),
                Size = new Vector2(100, 20),
                Modulate = new Color(0.85f, 0.8f, 0.95f, 0.8f)
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 11);

            container.AddChild(glow);
            container.AddChild(ghostBody);
            container.AddChild(nameLabel);

            // 飘浮动画
            var tween = CreateTween();
            tween.SetLoops(true);
            tween.TweenProperty(ghostBody, "position:y", ghostBody.Position.Y - 8f, 1.5f);
            tween.TweenProperty(ghostBody, "position:y", ghostBody.Position.Y, 1.5f);

            return container;
        }

        private void UpdateGhostNodePosition(int petId, Vector2 worldPosition)
        {
            if (!_ghostNodes.TryGetValue(petId, out var node))
                return;
            // 转换为屏幕坐标（Safe House 中的相对位置）
            // 这里使用相对于容器中心的位置
            node.Position = worldPosition + new Vector2(500, 300); // 假设 Safe House 中心在 (500, 300)
        }

        private void UpdateGhostNodeForNearPlayer(int petId)
        {
            if (!_ghostNodes.TryGetValue(petId, out var node))
                return;
            // 靠近玩家时光芒变亮
            var glow = node.GetNodeOrNull<ColorRect>("GhostGlow");
            if (glow != null)
            {
                var tween = CreateTween();
                tween.TweenProperty(glow, "color:a", 0.4f, 0.3f);
            }
        }

        private void ResetGhostNodeFromNearPlayer(int petId)
        {
            if (!_ghostNodes.TryGetValue(petId, out var node))
                return;
            var glow = node.GetNodeOrNull<ColorRect>("GhostGlow");
            if (glow != null)
            {
                var tween = CreateTween();
                tween.TweenProperty(glow, "color:a", 0.2f, 0.3f);
            }
        }

        private void AddGuardianAura(PetSoulGhostEntry soul)
        {
            if (_auraNodes.ContainsKey(soul.PetId))
                return;

            var auraNode = CreateGuardianAuraNode(soul);
            _guardianAura.AddChild(auraNode);
            _auraNodes[soul.PetId] = auraNode;
        }

        private Control CreateGuardianAuraNode(PetSoulGhostEntry soul)
        {
            var container = new Control();
            container.Name = $"GuardianAura_{soul.PetId}";
            container.Size = new Vector2(60, 60);
            container.Position = new Vector2(0, 0);
            container.MouseFilter = Control.MouseFilterEnum.Ignore;

            // 光环 — 环形
            var aura = new ColorRect
            {
                Name = "AuraRing",
                Size = new Vector2(50, 50),
                Position = new Vector2(5, 5),
                Color = ParseColor(soul.PetColor).WithAlpha(0.3f),
                MouseFilter = Control.MouseFilterEnum.Ignore
            };

            // 内核光球
            var core = new ColorRect
            {
                Name = "AuraCore",
                Size = new Vector2(20, 20),
                Position = new Vector2(20, 20),
                Color = ParseColor(soul.PetColor).WithAlpha(0.6f),
                MouseFilter = Control.MouseFilterEnum.Ignore
            };

            // 守护灵名称
            var label = new Label
            {
                Name = "AuraLabel",
                Text = $"✨{soul.PetName}",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Position = new Vector2(-25, 55),
                Size = new Vector2(110, 20),
                Modulate = new Color(0.9f, 0.85f, 1f, 0.9f)
            };
            label.AddThemeFontSizeOverride("font_size", 12);

            container.AddChild(aura);
            container.AddChild(core);
            container.AddChild(label);

            // 旋转动画
            var tween = CreateTween();
            tween.SetLoops(true);
            tween.TweenProperty(aura, "rotation", Mathf.Tau, 4f);
            tween.SetTrans(Tween.TransitionType.Linear);

            // 脉冲动画
            var tween2 = CreateTween();
            tween2.SetLoops(true);
            tween2.TweenProperty(core, "size", new Vector2(24, 24), 1f);
            tween2.TweenProperty(core, "size", new Vector2(20, 20), 1f);

            return container;
        }

        private void ShowMonologueBubble(int petId, string text)
        {
            if (_monologueTimer > 0f)
                return; // 防止覆盖

            if (!_ghostNodes.TryGetValue(petId, out var ghostNode))
                return;

            _currentMonologuePetId = petId;
            _currentMonologueText = text;
            _monologueTimer = MONOLOGUE_DISPLAY_TIME;

            // 设置气泡文本和位置
            _monologueBubble.Text = $"\"{text}\"";
            Vector2 ghostWorldPos = ghostNode.GlobalPosition;
            _monologueBubble.Position = ghostWorldPos + new Vector2(-150, -80);

            // 淡入动画
            var tween = CreateTween();
            tween.TweenProperty(_monologueBubble, "modulate:a", 1f, 0.4f);
            var bg = _monologueBubble.GetParent() as Panel;
            if (bg != null)
                tween.TweenProperty(bg, "modulate:a", 0.85f, 0.4f);
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            // 独白气泡计时
            if (_monologueTimer > 0f)
            {
                _monologueTimer -= (float)delta;
                if (_monologueTimer <= 0f)
                {
                    HideMonologueBubble();
                }
            }

            // 更新游荡灵魂位置
            if (PetSoulGhostSystem.Instance != null)
            {
                foreach (var ghost in PetSoulGhostSystem.Instance.GetWanderingGhosts())
                {
                    UpdateGhostNodePosition(ghost.PetId, ghost.WanderPosition);
                }
            }
        }

        private void HideMonologueBubble()
        {
            var tween = CreateTween();
            tween.TweenProperty(_monologueBubble, "modulate:a", 0f, 0.4f);
            var bg = _monologueBubble.GetParent() as Panel;
            if (bg != null)
                tween;
            _monologueTimer = 0f;
            _currentMonologuePetId = -1;
        }

        private void PlayInteractionAnimation(int petId)
        {
            if (!_ghostNodes.TryGetValue(petId, out var ghostNode))
                return;

            var ghostBody = ghostNode.GetNodeOrNull<ColorRect>("GhostBody");
            if (ghostBody == null)
                return;

            // 旋转一圈动画
            var tween = CreateTween();
            tween.TweenProperty(ghostNode, "rotation", Mathf.Tau, 1.5f);
            tween.SetTrans(Tween.TransitionType.Cubic);

            // 缩放弹跳
            var tween2 = CreateTween();
            tween2.TweenProperty(ghostNode, "scale", new Vector2(1.2f, 1.2f), 0.2f);
            tween2.TweenProperty(ghostNode, "scale", new Vector2(1f, 1f), 0.3f);
        }

        private Color ParseColor(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return new Color(1f, 1f, 1f);
            try
            {
                return new Color(hex);
            }
            catch
            {
                return new Color(1f, 1f, 1f);
            }
        }

        /// <summary>
        /// 显示灵魂面板（用于基地中的查看）
        /// </summary>
        public void ShowSoulPanel()
        {
            Visible = true;
        }

        /// <summary>
        /// 隐藏灵魂面板
        /// </summary>
        public void HideSoulPanel()
        {
            Visible = false;
        }
    }
}
