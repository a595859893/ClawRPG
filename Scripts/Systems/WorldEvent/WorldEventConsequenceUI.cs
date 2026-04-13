// WorldEventConsequenceUI.cs
// REQ-197: WorldEvent因果事件链 — SafeHouse 印记视觉层
// 在 SafeHouse 中显示成功事件留下的视觉印记

using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Core.Systems
{
    /// <summary>
    /// SafeHouse 印记视觉 UI
    /// 显示所有成功事件留下的视觉叠加层（光芒、旗帜、地形变化）
    /// 纯视觉层，不影响战斗数值平衡
    /// </summary>
    public partial class WorldEventConsequenceUI : CanvasLayer
    {
        // 印记容器
        private VBoxContainer _marksContainer;

        // 印记节点池
        private Dictionary<WorldEventType, Control> _markNodes;

        // 债务警告面板
        private PanelContainer _debtWarningPanel;
        private Label _debtWarningLabel;

        // 因果叙事文字
        private Label _consequenceNarrativeLabel;

        // 当前活跃债务（显示警告）
        private List<DebtRecord> _currentDebts;

        // ============ 导出配置 ============
        [Export]
        private bool _showMarks = true;

        [Export]
        private bool _showDebtWarning = true;

        [Export]
        private bool _showNarrativeText = true;

        [Export]
        private float _markIconSize = 32f;

        [Export]
        private Color _merchantMarkColor = new Color(0.9f, 0.7f, 0.3f, 0.8f); // 金色
        [Export]
        private Color _blessingMarkColor = new Color(0.6f, 0.9f, 0.6f, 0.8f); // 绿色
        [Export]
        private Color _invasionMarkColor = new Color(0.8f, 0.3f, 0.3f, 0.8f); // 红色
        [Export]
        private Color _portalMarkColor = new Color(0.5f, 0.3f, 0.9f, 0.8f); // 紫色
        [Export]
        private Color _debtWarningColor = new Color(0.9f, 0.2f, 0.2f, 0.9f); // 深红

        public override void _Ready()
        {
            _markNodes = new Dictionary<WorldEventType, Control>();
            _currentDebts = new List<DebtRecord>();

            SetupUINodes();
            SubscribeSignals();
            RefreshAllMarks();

            // 初始隐藏
            Visible = false;
        }

        private void SetupUINodes()
        {
            // 主容器 - 右下角
            _marksContainer = new VBoxContainer();
            _marksContainer.Name = "MarksContainer";
            _marksContainer.SetAnchorsPreset(Control.LayoutPreset.BottomRight);
            _marksContainer.OffsetLeft = -200;
            _marksContainer.OffsetRight = -16;
            _marksContainer.OffsetTop = -300;
            _marksContainer.OffsetBottom = -16;
            _marksContainer.Alignment = BoxContainer.AlignMode.End;
            AddChild(_marksContainer);

            // 债务警告面板
            _debtWarningPanel = new PanelContainer();
            _debtWarningPanel.Name = "DebtWarningPanel";
            _debtWarningPanel.Modulate = new Color(1f, 1f, 1f, 0f); // 初始透明
            _debtWarningPanel.CustomMinimumSize = new Vector2(200, 60);

            var debtPanelStyle = new StyleBoxFlat();
            debtPanelStyle.BgColor = new Color(0.2f, 0.05f, 0.05f, 0.95f);
            debtPanelStyle.CornerRadiusTopLeft = 8;
            debtPanelStyle.CornerRadiusTopRight = 8;
            debtPanelStyle.CornerRadiusBottomLeft = 8;
            debtPanelStyle.CornerRadiusBottomRight = 8;
            debtPanelStyle.SetBorderRadiusAll(8);
            debtPanelStyle.SetBorderColorAll(_debtWarningColor);
            debtPanelStyle.BorderWidthLeft = 2;
            debtPanelStyle.BorderWidthTop = 2;
            debtPanelStyle.BorderWidthRight = 2;
            debtPanelStyle.BorderWidthBottom = 2;
            _debtWarningPanel.AddThemeStyleboxOverride("panel", debtPanelStyle);

            var debtVBox = new VBoxContainer();
            debtVBox.Alignment = BoxContainer.AlignMode.Center;

            var debtTitle = new Label();
            debtTitle.Text = "⚠️ 债务警告";
            debtTitle.HorizontalAlignment = HorizontalAlignment.Center;
            debtTitle.AddThemeColorOverride("font_color", _debtWarningColor);

            _debtWarningLabel = new Label();
            _debtWarningLabel.Text = "";
            _debtWarningLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _debtWarningLabel.autowrap_mode = TextServer.AutowrapMode.WordSmart;

            debtVBox.AddChild(debtTitle);
            debtVBox.AddChild(_debtWarningLabel);
            _debtWarningPanel.AddChild(debtVBox);
            AddChild(_debtWarningPanel);

            // 因果叙事文字（屏幕底部中央）
            _consequenceNarrativeLabel = new Label();
            _consequenceNarrativeLabel.Name = "ConsequenceNarrative";
            _consequenceNarrativeLabel.SetAnchorsPreset(Control.LayoutPreset.CenterBottom);
            _consequenceNarrativeLabel.OffsetLeft = -300;
            _consequenceNarrativeLabel.OffsetRight = 300;
            _consequenceNarrativeLabel.OffsetTop = -100;
            _consequenceNarrativeLabel.OffsetBottom = -50;
            _consequenceNarrativeLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _consequenceNarrativeLabel.VerticalAlignment = VerticalAlignment.Center;
            _consequenceNarrativeLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _consequenceNarrativeLabel.Modulate = new Color(1f, 0.9f, 0.7f, 0f); // 初始透明
            _consequenceNarrativeLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.6f));
            AddChild(_consequenceNarrativeLabel);

            // 设置 SafeHouse 位置
            _marksContainer.SetAnchorsPreset(Control.LayoutPreset.BottomRight);
            _debtWarningPanel.SetAnchorsPreset(Control.LayoutPreset.TopRight);
            _debtWarningPanel.OffsetLeft = -220;
            _debtWarningPanel.OffsetRight = -16;
            _debtWarningPanel.OffsetTop = 60;
            _debtWarningPanel.OffsetBottom = 120;
        }

        private void SubscribeSignals()
        {
            // 订阅因果系统信号
            var consequenceSystem = WorldEventConsequenceSystem.Instance;
            if (consequenceSystem != null)
            {
                consequenceSystem.OnMarkEarned += OnMarkEarned;
                consequenceSystem.OnDebtTriggered += OnDebtTriggered;
                consequenceSystem.OnDebtResolved += OnDebtResolved;
                consequenceSystem.OnGrudgeEscalated += OnGrudgeEscalated;
            }
        }

        // ============ 公开 API ============

        /// <summary>
        /// 显示 SafeHouse 印记界面
        /// </summary>
        public void ShowMarks()
        {
            Visible = true;
            RefreshAllMarks();
            CheckActiveDebts();
        }

        /// <summary>
        /// 隐藏 SafeHouse 印记界面
        /// </summary>
        public void HideMarks()
        {
            Visible = false;
        }

        /// <summary>
        /// 刷新所有印记显示
        /// </summary>
        public void RefreshAllMarks()
        {
            if (!_showMarks) return;

            var consequenceSystem = WorldEventConsequenceSystem.Instance;
            if (consequenceSystem == null) return;

            // 清理旧节点
            foreach (var node in _markNodes.Values)
            {
                node.QueueFree();
            }
            _markNodes.Clear();

            // 创建新节点
            var marks = consequenceSystem.GetActiveMarks();
            foreach (var mark in marks)
            {
                if (mark.Intensity <= 0) continue;
                CreateMarkNode(mark.EventType, mark.Intensity);
            }

            // 根据印记数量调整容器大小
            AdjustContainerSize();
        }

        /// <summary>
        /// 显示一条因果叙事文字（淡入淡出，3秒后自动消失）
        /// </summary>
        public void ShowNarrative(string text, float durationSeconds = 3f)
        {
            if (!_showNarrativeText) return;

            _consequenceNarrativeLabel.Text = text;

            // Tween 淡入淡出
            var tween = CreateTween();
            tween.SetLoops(1);

            // 淡入
            tween.TweenProperty(_consequenceNarrativeLabel, "modulate:a", 0.95f, 0.5f);
            tween.Chain();

            // 保持
            tween.TweenInterval(durationSeconds - 1f);

            // 淡出
            tween.TweenProperty(_consequenceNarrativeLabel, "modulate:a", 0f, 0.5f);
        }

        // ============ 内部处理 ============

        private void CreateMarkNode(WorldEventType eventType, int intensity)
        {
            var markNode = new HBoxContainer();
            markNode.Name = $"Mark_{eventType}";
            markNode.Alignment = BoxContainer.AlignMode.End;

            // 印记图标
            var iconLabel = new Label();
            iconLabel.Text = GetMarkIcon(eventType);
            iconLabel.AddThemeFontSizeOverride("font_size", (int)_markIconSize);

            // 印记名称
            var nameLabel = new Label();
            nameLabel.Text = $"{GetEventTypeShortName(eventType)} ×{intensity}";
            nameLabel.AddThemeColorOverride("font_color", GetMarkColor(eventType));

            // 强度指示（星星）
            var starsLabel = new Label();
            starsLabel.Text = GetStarsString(intensity);
            starsLabel.AddThemeColorOverride("font_color", GetMarkColor(eventType));

            markNode.AddChild(iconLabel);
            markNode.AddChild(nameLabel);
            markNode.AddChild(new Control()); // spacer
            markNode.AddChild(starsLabel);

            _marksContainer.AddChild(markNode);
            _markNodes[eventType] = markNode;

            // 淡入动画
            markNode.Modulate = new Color(1f, 1f, 1f, 0f);
            var tween = CreateTween();
            tween.TweenProperty(markNode, "modulate:a", 1f, 0.3f);
        }

        private void OnMarkEarned(WorldEventType eventType, int totalMarks)
        {
            // 刷新该类型印记
            if (_markNodes.TryGetValue(eventType, out var existingNode))
            {
                existingNode.QueueFree();
                _markNodes.Remove(eventType);
            }

            CreateMarkNode(eventType, totalMarks);
            AdjustContainerSize();

            // 显示叙事文字
            var narrative = GetMarkEarnedNarrative(eventType, totalMarks);
            ShowNarrative(narrative, 2.5f);
        }

        private void OnGrudgeEscalated(WorldEventType eventType, int grudgeLevel)
        {
            // 怨念升级时显示警告（但印记 UI 不直接显示怨念）
            // 怨念的反馈通过事件描述文字体现
        }

        private void OnDebtTriggered(WorldEventType eventType, int totalDebts)
        {
            CheckActiveDebts();

            // 显示债务警告
            var debtWarning = $"你欠这个世界一笔债。";
            _debtWarningLabel.Text = $"{GetEventTypeShortName(eventType)}: {totalDebts}笔债务待还";

            ShowDebtWarningPanel();
        }

        private void OnDebtResolved(WorldEventType eventType, int remainingDebts)
        {
            if (remainingDebts <= 0)
            {
                HideDebtWarningPanel();
            }
            else
            {
                _debtWarningLabel.Text = $"{GetEventTypeShortName(eventType)}: {remainingDebts}笔债务待还";
            }

            CheckActiveDebts();
        }

        private void CheckActiveDebts()
        {
            if (!_showDebtWarning) return;

            var consequenceSystem = WorldEventConsequenceSystem.Instance;
            if (consequenceSystem == null) return;

            var currentLevel = 1; // 默认最低等级
            try
            {
                var gameState = GetNodeOrNull("/root/GameState");
                if (gameState != null && gameState.Has("Level"))
                {
                    currentLevel = (int)gameState.Get("Level");
                }
            }
            catch { /* ignore */ }

            var activeDebts = consequenceSystem.CheckActiveDebts(currentLevel);
            _currentDebts = activeDebts;

            if (activeDebts.Count > 0)
            {
                ShowDebtWarningPanel();
            }
            else
            {
                HideDebtWarningPanel();
            }
        }

        private void ShowDebtWarningPanel()
        {
            if (!_showDebtWarning) return;

            var tween = CreateTween();
            tween.TweenProperty(_debtWarningPanel, "modulate:a", 1f, 0.3f);
        }

        private void HideDebtWarningPanel()
        {
            var tween = CreateTween();
            tween.TweenProperty(_debtWarningPanel, "modulate:a", 0f, 0.3f);
        }

        private void AdjustContainerSize()
        {
            // 根据印记数量调整容器
            var markCount = _markNodes.Count;
            var height = Mathf.Max(60, markCount * 40 + 20);
            _marksContainer.CustomMinimumSize = new Vector2(180, height);
        }

        // ============ 辅助方法 ============

        private string GetMarkIcon(WorldEventType eventType)
        {
            return eventType switch
            {
                WorldEventType.MerchantVisit => "🏪",
                WorldEventType.Blessing => "✨",
                WorldEventType.MonsterSurge => "⚔️",
                WorldEventType.TreasureSpawn => "💎",
                WorldEventType.Curse => "💀",
                WorldEventType.Portal => "🌀",
                WorldEventType.RareSpawn => "🐉",
                WorldEventType.ResourceBurst => "💰",
                WorldEventType.WeatherChange => "🌤️",
                WorldEventType.NPCrescue => "🆘",
                _ => "🌍"
            };
        }

        private string GetEventTypeShortName(WorldEventType eventType)
        {
            return eventType switch
            {
                WorldEventType.MerchantVisit => "商人",
                WorldEventType.Blessing => "祝福",
                WorldEventType.MonsterSurge => "侵袭",
                WorldEventType.TreasureSpawn => "宝藏",
                WorldEventType.Curse => "诅咒",
                WorldEventType.Portal => "传送门",
                WorldEventType.RareSpawn => "稀有",
                WorldEventType.ResourceBurst => "资源",
                WorldEventType.WeatherChange => "天气",
                WorldEventType.NPCrescue => "营救",
                _ => "事件"
            };
        }

        private Color GetMarkColor(WorldEventType eventType)
        {
            return eventType switch
            {
                WorldEventType.MerchantVisit => _merchantMarkColor,
                WorldEventType.Blessing => _blessingMarkColor,
                WorldEventType.MonsterSurge => _invasionMarkColor,
                WorldEventType.Portal => _portalMarkColor,
                WorldEventType.Curse => new Color(0.7f, 0.4f, 0.9f, 0.8f),
                _ => new Color(0.7f, 0.7f, 0.7f, 0.8f)
            };
        }

        private string GetStarsString(int intensity)
        {
            var stars = Mathf.Min(intensity, 5);
            return new string('★', stars) + new string('☆', Mathf.Max(0, 5 - stars));
        }

        private string GetMarkEarnedNarrative(WorldEventType eventType, int totalMarks)
        {
            var name = GetEventTypeShortName(eventType);
            if (totalMarks == 1)
            {
                return $"世界的记忆里，多了一个关于{name}的印记。";
            }
            return $"关于{name}的印记，变得更加清晰了。（{totalMarks}次）";
        }

        public override void _ExitTree()
        {
            // 取消信号订阅
            var consequenceSystem = WorldEventConsequenceSystem.Instance;
            if (consequenceSystem != null)
            {
                consequenceSystem.OnMarkEarned -= OnMarkEarned;
                consequenceSystem.OnDebtTriggered -= OnDebtTriggered;
                consequenceSystem.OnDebtResolved -= OnDebtResolved;
                consequenceSystem.OnGrudgeEscalated -= OnGrudgeEscalated;
            }

            base._ExitTree();
        }
    }
}
