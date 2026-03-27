using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.Pets.AI;

namespace ClawRPG.Scripts.UI.Pet
{
    /// <summary>
    /// Observer 叙事气泡 UI（REQ-138）
    /// 在屏幕边缘显示 Observer 的提问气泡（叙事风格，不打断操作）
    /// </summary>
    public partial class ObserverBubbleUI : Control
    {
        // ===== 配置 =====
        
        private float _bubbleDisplayDuration = 5.0f;      // 气泡显示时长
        private float _bubbleFadeOutDuration = 1.0f;      // 淡出时长
        private int _maxBubbles = 3;                       // 最多同时显示气泡数
        private Vector2 _bubbleSize = new Vector2(300, 80);

        // ===== 状态 =====
        
        private List<ObserverBubble> _activeBubbles = new List<ObserverBubble>();
        private AdversarialObserverSystem _observerSystem;

        // ===== 节点引用 =====
        
        private PanelContainer _bubbleContainer;
        private VBoxContainer _bubbleList;

        // ===== 颜色 =====
        
        private Color _bubbleBgColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        private Color _textColor = new Color(0.9f, 0.85f, 0.7f, 1.0f);  // 暖色调叙事文字
        private Color _confidenceHighColor = new Color(1.0f, 0.8f, 0.2f);  // 高置信度金色
        private Color _confidenceLowColor = new Color(0.6f, 0.6f, 0.7f);   // 低置信度灰色

        // ===== 宠物名字（Narrative voice）=====
        
        private string _observerPetName = "小家伙";  // 可以从宠物系统获取真实名字

        public override void _Ready()
        {
            // 尝试获取 Observer 系统
            _observerSystem = AdversarialObserverSystem.Instance;

            SetupUI();

            // 订阅 Observer 信号
            if (_observerSystem != null)
            {
                _observerSystem.OnObserverChallenge += OnObserverChallenge;
                _observerSystem.OnConfidenceChanged += OnConfidenceChanged;
            }

            GD.Print("[ObserverBubbleUI] Initialized");
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;

            // 更新气泡
            for (int i = _activeBubbles.Count - 1; i >= 0; i--)
            {
                var bubble = _activeBubbles[i];
                bubble.TimeRemaining -= dt;

                if (bubble.TimeRemaining <= _bubbleFadeOutDuration)
                {
                    // 淡出
                    float alpha = bubble.TimeRemaining / _bubbleFadeOutDuration;
                    bubble.Panel.Modulate = new Color(1, 1, 1, alpha);
                }

                if (bubble.TimeRemaining <= 0)
                {
                    RemoveBubble(bubble);
                    _activeBubbles.RemoveAt(i);
                }
            }
        }

        public override void _ExitTree()
        {
            if (_observerSystem != null)
            {
                _observerSystem.OnObserverChallenge -= OnObserverChallenge;
                _observerSystem.OnConfidenceChanged -= OnConfidenceChanged;
            }
        }

        // ===== UI 构建 =====

        private void SetupUI()
        {
            // 外层容器
            _bubbleContainer = new PanelContainer
            {
                Name = "ObserverBubbleContainer",
                AnchorLeft = 0.02f,
                AnchorTop = 0.1f,
                AnchorRight = 0.02f,
                AnchorBottom = 0.9f,
                GrowHorizontal = GrowDirection.Begin,
                GrowVertical = GrowDirection.Both
            };
            AddChild(_bubbleContainer);

            var styleBox = new StyleBoxFlat
            {
                BgColor = new Color(0, 0, 0, 0),
                BorderWidthLeft = 0,
                BorderWidthTop = 0,
                BorderWidthRight = 0,
                BorderWidthBottom = 0
            };
            _bubbleContainer.AddThemeStyleboxOverride("panel", styleBox);

            _bubbleList = new VBoxContainer
            {
                Name = "BubbleList",
                Alignment = BoxContainer.AlignmentMode.End  // 从下往上排列
            };
            _bubbleContainer.AddChild(_bubbleList);
        }

        // ===== 信号处理 =====

        private void OnObserverChallenge(ObserverChallenge challenge)
        {
            // 叙事化包装：用宠物口吻说出来
            string narrativeText = WrapInNarrativeVoice(challenge);

            CreateBubble(narrativeText, challenge.Confidence);
        }

        private void OnConfidenceChanged(float confidence)
        {
            // 可以在这里更新 UI 显示 Observer 的"心态"
        }

        // ===== 气泡管理 =====

        private void CreateBubble(string text, float confidence)
        {
            // 限制同时显示的气泡数量
            if (_activeBubbles.Count >= _maxBubbles)
            {
                RemoveBubble(_activeBubbles[0]);
                _activeBubbles.RemoveAt(0);
            }

            var bubble = new ObserverBubble
            {
                TimeRemaining = _bubbleDisplayDuration + _bubbleFadeOutDuration,
                Panel = CreateBubblePanel(text, confidence)
            };

            _bubbleList.AddChild(bubble.Panel);
            _activeBubbles.Add(bubble);
        }

        private PanelContainer CreateBubblePanel(string text, float confidence)
        {
            var panel = new PanelContainer
            {
                SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
                SizeFlagsVertical = SizeFlags.ShrinkBegin,
                CustomMinimumSize = _bubbleSize
            };

            // 气泡样式
            var styleBox = new StyleBoxFlat
            {
                BgColor = _bubbleBgColor,
                CornerRadiusTopLeft = 8,
                CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8,
                CornerRadiusBottomRight = 8,
                BorderWidthLeft = 2,
                BorderWidthTop = 2,
                BorderWidthRight = 2,
                BorderWidthBottom = 2,
                BorderColor = confidence > 0.7f ? _confidenceHighColor : _confidenceLowColor
            };
            panel.AddThemeStyleboxOverride("panel", styleBox);

            // 内边距容器
            var margin = new MarginContainer
            {
                ThemeConstantSeparation = 8
            };
            panel.AddChild(margin);

            // 气泡内容
            var vbox = new VBoxContainer();
            margin.AddChild(vbox);

            // 头部（宠物名 + 置信度指示）
            var header = new HBoxContainer();
            vbox.AddChild(header);

            var nameLabel = new Label
            {
                Text = $"【{_observerPetName}】",
                ThemeColorFontSize = 14,
                SizeFlagsHorizontal = SizeFlags.ShrinkBegin
            };
            nameLabel.AddThemeColorOverride("font_color", _confidenceHighColor);
            header.AddChild(nameLabel);

            header.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });

            // 置信度指示
            var confidenceIndicator = new Label
            {
                Text = confidence > 0.75f ? "◆◆◆" : (confidence > 0.5f ? "◆◆" : "◆"),
                ThemeColorFontSize = 10,
                SizeFlagsHorizontal = SizeFlags.ShrinkEnd
            };
            confidenceIndicator.AddThemeColorOverride("font_color", confidence > 0.7f ? _confidenceHighColor : _confidenceLowColor);
            header.AddChild(confidenceIndicator);

            // 内容
            var contentLabel = new Label
            {
                Text = text,
                ThemeColorFontSize = 14,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            contentLabel.AddThemeColorOverride("font_color", _textColor);
            vbox.AddChild(contentLabel);

            return panel;
        }

        private void RemoveBubble(ObserverBubble bubble)
        {
            if (bubble.Panel != null && bubble.Panel.IsInsideTree())
            {
                bubble.Panel.QueueFree();
            }
        }

        // ===== 叙事化包装 =====

        /// <summary>
        /// 将挑战用宠物的叙事口吻包装（不是直接说"你的HP很低"）
        /// </summary>
        private string WrapInNarrativeVoice(ObserverChallenge challenge)
        {
            switch (challenge.Type)
            {
                case DisagreementType.ThreatIgnored:
                    return $"我有点担心...{challenge.QuestionPrompt}";

                case DisagreementType.MissedOpportunity:
                    return $"嘿，{challenge.QuestionPrompt}";

                case DisagreementType.GoalDrift:
                    return $"...{challenge.QuestionPrompt}";

                case DisagreementType.ResourceMismatch:
                    return $"我有个疑问：{challenge.QuestionPrompt}";

                default:
                    return challenge.QuestionPrompt;
            }
        }

        /// <summary>
        /// 设置 Observer 宠物名字
        /// </summary>
        public void SetObserverPetName(string name)
        {
            _observerPetName = name;
        }

        // ===== 内部类 =====

        private class ObserverBubble
        {
            public PanelContainer Panel { get; set; }
            public float TimeRemaining { get; set; }
        }
    }
}
