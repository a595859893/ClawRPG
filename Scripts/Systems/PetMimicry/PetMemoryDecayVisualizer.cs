using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Events;
using ClawRPG.Scripts.Managers;
using ClawRPG.Scripts.Systems.Pets;

namespace ClawRPG.Scripts.Systems.PetMimicry
{
    /// <summary>
    /// 宠物记忆衰减可视化 — 宠物印记强度衰减时，外形逐渐回退到原始外观
    /// 
    /// 职责：
    /// 1. 订阅 MimicryLevelTracker 的印记等级变化信号
    /// 2. 根据各印记的 ImprintStrength (基于 DecayTimer 和等级) 计算外观强度
    /// 3. 平滑调整宠物 Sprite 颜色/调制/覆盖层效果
    /// 4. 同步宠物 UI 印记条颜色与衰减状态
    /// 
    /// 衰减阈值（REQ-147 规格）：
    /// - ImprintStrength > 70%: 完全强化外观
    /// - 50% < ImprintStrength <= 70%: 开始出现原始颜色斑块
    /// - 30% < ImprintStrength <= 50%: 混合状态，原始颜色占主导
    /// - ImprintStrength <= 30%: 基本回到原始外观
    /// </summary>
    public class PetMemoryDecayVisualizer : Node
    {
        public static PetMemoryDecayVisualizer Instance { get; private set; }

        // ── Visual Config ──────────────────────────────────────────────────
        [Export] private float _decayWarningThreshold = 0.30f;   // 30% 以下危险
        [Export] private float _decayCautionThreshold = 0.50f;   // 50% 以下开始混合
        [Export] private float _decayMildThreshold = 0.70f;      // 70% 以下出现斑块
        [Export] private float _tweenDuration = 0.8f;             // 平滑过渡时长（秒）

        // 每种印记类型对应的视觉颜色调制
        private static readonly Dictionary<PlayerBehaviorType, Color> _elementColors = new Dictionary<PlayerBehaviorType, Color>()
        {
            { PlayerBehaviorType.UseFireSkill,     new Color(1.0f, 0.35f, 0.1f)  }, // 火焰橙红
            { PlayerBehaviorType.UseIceSkill,      new Color(0.3f, 0.7f,  1.0f)  }, // 冰蓝
            { PlayerBehaviorType.UseElectricSkill, new Color(1.0f, 0.9f,  0.2f)  }, // 电黄
            { PlayerBehaviorType.UseShadowSkill,   new Color(0.4f, 0.1f,  0.6f)  }, // 暗紫
            { PlayerBehaviorType.UseHolySkill,     new Color(1.0f, 0.85f, 0.4f)  }, // 神圣金
            { PlayerBehaviorType.UseNatureSkill,  new Color(0.2f, 0.8f,  0.3f)  }, // 自然绿
        };

        // 覆盖层节点名称（预制件）
        private const string OVERLAY_NODE_NAME = "ImprintOverlay";

        // ── State ──────────────────────────────────────────────────────────
        /// <summary>当前各印记的视觉强度（0.0-1.0）</summary>
        private Dictionary<PlayerBehaviorType, float> _imprintVisualStrength = new Dictionary<PlayerBehaviorType, float>();

        /// <summary>覆盖层节点引用</summary>
        private Node2D _overlayNode;

        /// <summary>宠物主 Sprite 引用</summary>
        private Sprite _petSprite;

        /// <summary>原始 Sprite 调制颜色</summary>
        private Color _originalModulate = Colors.White;

        /// <summary>当前活跃的印记类型列表</summary>
        private HashSet<PlayerBehaviorType> _activeImprintTypes = new HashSet<PlayerBehaviorType>();

        /// <summary>各行为类型的当前叠加色</summary>
        private Dictionary<PlayerBehaviorType, Color> _currentOverlayColors = new Dictionary<PlayerBehaviorType, Color>();

        // ── Signals ─────────────────────────────────────────────────────────
        [Signal]
        public delegate void ImprintVisualDecayedEventHandler(PlayerBehaviorType behavior, float strength);

        [Signal]
        public delegate void ImprintVisualRefreshedEventHandler(PlayerBehaviorType behavior, int newLevel);

        public override void _Ready()
        {
            Instance = this;
            SubscribeToSignals();
            ConnectToPetSprite();
            GD.Print("[PetMemoryDecayVisualizer] Initialized");
        }

        private void SubscribeToSignals()
        {
            var tracker = MimicryLevelTracker.Instance;
            if (tracker != null)
            {
                tracker.ImprintLevelChanged += OnImprintLevelChanged;
                tracker.ImprintXpGained += OnImprintXpGained;
            }
        }

        private void ConnectToPetSprite()
        {
            // 尝试从 PetCombatAI 获取宠物节点
            try
            {
                if (PetCombatAI.Instance != null)
                {
                    var petNode = PetCombatAI.Instance.GetPetNode();
                    if (petNode != null)
                    {
                        // 查找 Sprite 节点
                        _petSprite = petNode.GetNodeOrNull<Sprite>("Sprite") 
                                  ?? petNode.GetNodeOrNull<Sprite>("sprite")
                                  ?? petNode.GetNodeOrNull<Sprite>("Sprite2D");

                        if (_petSprite != null)
                        {
                            _originalModulate = _petSprite.Modulate;
                            GD.Print($"[PetMemoryDecayVisualizer] Connected to pet sprite: {_petSprite.Name}");
                        }

                        // 添加覆盖层节点
                        _overlayNode = new Node2D();
                        _overlayNode.Name = OVERLAY_NODE_NAME;
                        _overlayNode.ZIndex = 10; // 在宠物上方
                        petNode.AddChild(_overlayNode);
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[PetMemoryDecayVisualizer] Failed to connect to pet sprite: {ex.Message}");
            }
        }

        public override void _Process(double delta)
        {
            // 被动衰减：每帧微小衰减（视觉上更平滑）
            bool anyChanged = false;
            foreach (var behavior in new List<PlayerBehaviorType>(_activeImprintTypes))
            {
                if (_imprintVisualStrength.TryGetValue(behavior, out float strength))
                {
                    // 被动衰减速率：每分钟约 1%（极缓慢，在衰减计时器外作为视觉补充）
                    float passiveDecay = (float)delta / 60f * 0.01f;
                    float newStrength = Mathf.Max(0f, strength - passiveDecay);
                    if (Mathf.Abs(newStrength - strength) > 0.001f)
                    {
                        _imprintVisualStrength[behavior] = newStrength;
                        anyChanged = true;
                    }
                }
            }

            if (anyChanged)
                UpdatePetVisuals();
        }

        // ── Signal Handlers ─────────────────────────────────────────────────

        /// <summary>
        /// 印记等级变化时：更新视觉强度
        /// </summary>
        private void OnImprintLevelChanged(PlayerBehaviorType behavior, RoomEnvironmentType environment, int oldLevel, int newLevel)
        {
            GD.Print($"[PetMemoryDecayVisualizer] Level changed: {behavior} {oldLevel}→{newLevel}");

            if (newLevel == 0)
            {
                // 印记消失：移除视觉效果
                RemoveImprintVisual(behavior);
            }
            else
            {
                // 印记等级变化：重新计算强度
                // 强度 = 等级 / 5 * (1 - DecayTimer/(Grace+Interval))
                RefreshImprintVisual(behavior);
            }
        }

        /// <summary>
        /// 印记 XP 增加时：轻微提升视觉强度
        /// </summary>
        private void OnImprintXpGained(PlayerBehaviorType behavior, RoomEnvironmentType environment, float xpGained, float xpTotal, float xpForNextLevel)
        {
            // XP 增加时视觉上轻微闪烁一下（反馈感）
            if (_imprintVisualStrength.TryGetValue(behavior, out float strength))
            {
                _imprintVisualStrength[behavior] = Mathf.Min(1f, strength + 0.05f);
                UpdatePetVisuals();
            }
        }

        // ── Core Visual Logic ───────────────────────────────────────────────

        /// <summary>
        /// 刷新单个印记的视觉强度
        /// </summary>
        private void RefreshImprintVisual(PlayerBehaviorType behavior)
        {
            if (_mimicryData == null) return;

            var imprints = _mimicryData.GetAllImprints();
            BehaviorImprint target = null;
            foreach (var imprint in imprints)
            {
                if (imprint.BehaviorType == behavior)
                {
                    target = imprint;
                    break;
                }
            }

            if (target == null) return;

            // 计算视觉强度
            // 基于等级（60%权重）和衰减计时器（40%权重）
            float levelStrength = target.ImprintLevel / 5f;
            
            // 衰减进度：0 = 无衰减，1 = 即将衰减
            float graceSeconds = 7f * 86400f;
            float decayIntervalSeconds = 14f * 86400f;
            float decayProgress = 0f;
            if (target.DecayTimer > graceSeconds)
            {
                decayProgress = Mathf.Min(1f, (target.DecayTimer - graceSeconds) / decayIntervalSeconds);
            }

            float visualStrength = levelStrength * (1f - decayProgress * 0.5f);
            visualStrength = Mathf.Clamp(visualStrength, 0f, 1f);

            _imprintVisualStrength[behavior] = visualStrength;
            _activeImprintTypes.Add(behavior);

            if (!_currentOverlayColors.ContainsKey(behavior))
                _currentOverlayColors[behavior] = _elementColors.GetValueOrDefault(behavior, Colors.White);

            // 应用视觉效果
            UpdatePetVisuals();

            EmitSignal(SignalName.ImprintVisualDecayed, behavior, visualStrength);
        }

        /// <summary>
        /// 移除印记视觉效果
        /// </summary>
        private void RemoveImprintVisual(PlayerBehaviorType behavior)
        {
            _imprintVisualStrength.Remove(behavior);
            _activeImprintTypes.Remove(behavior);
            _currentOverlayColors.Remove(behavior);
            UpdatePetVisuals();
        }

        /// <summary>
        /// 更新宠物外观：综合所有印记的视觉强度
        /// </summary>
        private void UpdatePetVisuals()
        {
            if (_petSprite == null) return;

            // 计算加权平均视觉强度（以主要印记类型为主）
            float dominantStrength = 0f;
            PlayerBehaviorType? dominantType = null;

            foreach (var kvp in _imprintVisualStrength)
            {
                if (kvp.Value > dominantStrength)
                {
                    dominantStrength = kvp.Value;
                    dominantType = kvp.Key;
                }
            }

            // 计算混合叠加色
            Color overlayColor = Colors.Transparent;
            float totalWeight = 0f;

            foreach (var kvp in _imprintVisualStrength)
            {
                if (_elementColors.TryGetValue(kvp.Key, out Color elemColor))
                {
                    float weight = kvp.Value;
                    overlayColor += elemColor * weight;
                    totalWeight += weight;
                }
            }

            if (totalWeight > 0f)
                overlayColor = new Color(overlayColor.R / totalWeight, overlayColor.G / totalWeight, overlayColor.B / totalWeight);
            else
                overlayColor = Colors.Transparent;

            // 应用 Sprite 调制变化
            ApplySpriteModulation(dominantStrength, dominantType);

            // 应用/更新覆盖层效果
            ApplyOverlayEffect(overlayColor, dominantStrength);

            // 更新 UI 印记条
            UpdateImprintBars();
        }

        /// <summary>
        /// 根据视觉强度应用 Sprite 调制
        /// </summary>
        private void ApplySpriteModulation(float strength, PlayerBehaviorType? dominantType)
        {
            if (_petSprite == null) return;

            Color targetModulate = _originalModulate;

            if (dominantType.HasValue && _elementColors.TryGetValue(dominantType.Value, out Color elemColor))
            {
                // 强度越高，元素色越浓
                float blendFactor = strength * 0.6f; // 最高 60% 染色
                targetModulate = _originalModulate.LinearInterpolate(elemColor, blendFactor);
            }

            // 创建平滑过渡 Tween
            var tween = CreateTween();
            tween.SetTrans(Tween.TransitionType.Sine);
            tween.SetEase(Tween.EaseType.InOut);
            tween.TweenProperty(_petSprite, "modulate:r", targetModulate.R, _tweenDuration);
            tween.Parallel().TweenProperty(_petSprite, "modulate:g", targetModulate.G, _tweenDuration);
            tween.Parallel().TweenProperty(_petSprite, "modulate:b", targetModulate.B, _tweenDuration);
        }

        /// <summary>
        /// 应用/更新覆盖层节点（元素光晕/粒子效果）
        /// </summary>
        private void ApplyOverlayEffect(Color overlayColor, float strength)
        {
            if (_overlayNode == null) return;

            // 清空现有覆盖层
            foreach (Node child in _overlayNode.GetChildren())
            {
                child.QueueFree();
            }

            if (strength < _decayMildThreshold) return; // 太弱不显示

            // 根据强度决定覆盖层透明度
            float alpha = (strength - _decayMildThreshold) / (1f - _decayMildThreshold);
            alpha *= 0.7f; // 最大 70% 透明度

            // 根据强度决定是否显示警告效果
            bool isWarning = strength <= _decayWarningThreshold;
            bool isCaution = strength <= _decayCautionThreshold;

            if (isWarning)
            {
                // 危险状态：红色脉冲
                CreatePulseOverlay(new Color(1f, 0f, 0f, alpha * 0.8f), 0.5f);
            }
            else if (isCaution)
            {
                // 警告状态：橙色淡色覆盖
                CreateFlatOverlay(new Color(1f, 0.5f, 0f, alpha * 0.4f));
            }
            else
            {
                // 正常：元素色光晕
                if (overlayColor.A > 0f)
                {
                    Color c = new Color(overlayColor.R, overlayColor.G, overlayColor.B, alpha * 0.5f);
                    CreateFlatOverlay(c);
                }
            }
        }

        /// <summary>
        /// 创建平面颜色覆盖层
        /// </summary>
        private void CreateFlatOverlay(Color color)
        {
            if (_overlayNode == null) return;

            // 使用 ColorRect 作为简单覆盖层（放在宠物精灵上方）
            var rect = new ColorRect();
            rect.Name = "FlatOverlay";
            rect.Modulate = color;
            rect.ZIndex = 5;

            // 连接到宠物精灵大小
            if (_petSprite != null)
            {
                rect.Size = _petSprite.Texture != null 
                    ? _petSprite.Texture.GetSize() * _petSprite.Scale 
                    : new Vector2(64, 64);
                rect.Position = -rect.Size / 2f;
            }

            _overlayNode.AddChild(rect);
        }

        /// <summary>
        /// 创建脉冲动画覆盖层
        /// </summary>
        private void CreatePulseOverlay(Color color, float pulseSpeed)
        {
            if (_overlayNode == null) return;

            var rect = new ColorRect();
            rect.Name = "PulseOverlay";
            rect.Modulate = color;
            rect.ZIndex = 5;

            if (_petSprite != null)
            {
                rect.Size = _petSprite.Texture != null 
                    ? _petSprite.Texture.GetSize() * _petSprite.Scale 
                    : new Vector2(64, 64);
                rect.Position = -rect.Size / 2f;
            }

            _overlayNode.AddChild(rect);

            // 脉冲动画（透明度 0 → 当前alpha → 0）
            var tween = CreateTween();
            tween.SetLoops(-1); // 无限循环

            float baseAlpha = color.A;
            tween.TweenProperty(rect, "modulate:a", 0f, pulseSpeed * 0.5f)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.InOut);
            tween.TweenProperty(rect, "modulate:a", baseAlpha, pulseSpeed * 0.5f)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.InOut);
        }

        /// <summary>
        /// 更新宠物 UI 的印记条颜色（通过信号通知UI）
        /// </summary>
        private void UpdateImprintBars()
        {
            // 计算所有印记的衰减状态
            foreach (var kvp in _imprintVisualStrength)
            {
                float strength = kvp.Value;
                Color barColor;

                if (strength > _decayMildThreshold)
                    barColor = new Color(0.2f, 1f, 0.3f);  // 绿色 — 活跃
                else if (strength > _decayCautionThreshold)
                    barColor = new Color(1f, 0.8f, 0.2f);  // 黄色 — 开始衰减
                else if (strength > _decayWarningThreshold)
                    barColor = new Color(1f, 0.5f, 0f);    // 橙色 — 明显衰减
                else
                    barColor = new Color(1f, 0.1f, 0.1f);  // 红色 — 危险

                // 发射信号通知 UI 更新（UI 订阅 ImprintVisualDecayed）
                EmitSignal(SignalName.ImprintVisualDecayed, kvp.Key, strength);
            }
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// 刷新所有印记的视觉（当宠物进入新环境时调用）
        /// </summary>
        public void RefreshAllVisuals()
        {
            if (_mimicryData == null) return;

            var allImprints = _mimicryData.GetAllImprints();
            foreach (var imprint in allImprints)
            {
                if (imprint.ImprintLevel > 0)
                    RefreshImprintVisual(imprint.BehaviorType);
            }
        }

        /// <summary>
        /// 获取指定印记的当前视觉强度
        /// </summary>
        public float GetImprintVisualStrength(PlayerBehaviorType behavior)
        {
            return _imprintVisualStrength.GetValueOrDefault(behavior, 0f);
        }

        /// <summary>
        /// 获取当前主导印记类型
        /// </summary>
        public PlayerBehaviorType? GetDominantImprintType()
        {
            float maxStrength = 0f;
            PlayerBehaviorType? dominant = null;

            foreach (var kvp in _imprintVisualStrength)
            {
                if (kvp.Value > maxStrength)
                {
                    maxStrength = kvp.Value;
                    dominant = kvp.Key;
                }
            }
            return dominant;
        }

        /// <summary>
        /// 获取衰减警告状态（任何印记是否处于危险水平）
        /// </summary>
        public bool IsAnyImprintAtRisk()
        {
            foreach (var kvp in _imprintVisualStrength)
            {
                if (kvp.Value <= _decayWarningThreshold)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获取总体视觉强度（0.0-1.0），综合所有印记
        /// </summary>
        public float GetOverallVisualStrength()
        {
            if (_imprintVisualStrength.Count == 0) return 0f;

            float total = 0f;
            foreach (var kvp in _imprintVisualStrength)
                total += kvp.Value;
            return total / _imprintVisualStrength.Count;
        }

        private PetMimicryData _mimicryData => PetMimicryData.Instance;
    }
}
