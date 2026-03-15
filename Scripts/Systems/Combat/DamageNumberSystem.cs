using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Combat
{
    [GlobalClass]
    public partial class DamageNumberData : Resource
    {
        [Export] public String Name { get; set; } = "DamageNumber";
        [Export] public Color TextColor { get; set; } = Colors.White;
        [Export] public Color CriticalColor { get; set; } = new Color(1f, 0.84f, 0f); // Gold
        [Export] public Color HealColor { get; set; } = new Color(0.2f, 1f, 0.2f); // Green
        [Export] public Color MissColor { get; set; } = new Color(0.7f, 0.7f, 0.7f); // Gray
        [Export] public float FontSize { get; set; } = 24f;
        [Export] public float CriticalFontSize { get; set; } = 36f;
        [Export] public float FloatSpeed { get; set; } = 50f;
        [Export] public float FloatDuration { get; set; } = 1.0f;
        [Export] public float FadeStartTime { get; set; } = 0.7f;
        [Export] public Vector2 RandomOffsetRange { get; set; } = new Vector2(20f, 20f);
        [Export] public bool EnableShake { get; set; } = true;
        [Export] public float ShakeAmount { get; set; } = 3f;
    }

    public class DamageNumberInstance
    {
        public Label Label { get; set; }
        public float LifeTime { get; set; }
        public float TotalDuration { get; set; }
        public Vector2 StartPosition { get; set; }
        public bool IsCritical { get; set; }
        public bool IsHeal { get; set; }
        public bool IsMiss { get; set; }
        public Random Random { get; set; }

        public void Update(float delta)
        {
            LifeTime += delta;
            
            // Float upward
            float progress = LifeTime / TotalDuration;
            float yOffset = -progress * 100f;
            
            // Apply shake
            if (EnableShake && LifeTime < 0.2f)
            {
                float shakeX = (float)(Random.NextDouble() * 2 - 1) * ShakeAmount * (1f - progress);
                float shakeY = (float)(Random.NextDouble() * 2 - 1) * ShakeAmount * (1f - progress);
                Label.Position = StartPosition + new Vector2(shakeX, yOffset + shakeY);
            }
            else
            {
                Label.Position = StartPosition + new Vector2(0, yOffset);
            }
            
            // Fade out
            if (LifeTime > FadeStartTime)
            {
                float fadeProgress = (LifeTime - FadeStartTime) / (TotalDuration - FadeStartTime);
                Label.Modulate = new Color(1f, 1f, 1f, 1f - fadeProgress);
            }
        }
    }

    public partial class DamageNumberSystem : BaseSystem
    {
        private static DamageNumberSystem _instance;
        public static DamageNumberSystem Instance => _instance;

        [Export] public DamageNumberData Data { get; set; }
        
        private CanvasItem _targetCanvas;
        private List<DamageNumberInstance> _activeNumbers = new List<DamageNumberInstance>();
        private Random _random = new Random();
        
        // Statistics
        public int TotalDamageNumbers { get; private set; }
        public int CriticalHits { get; private set; }
        public int TotalDamage { get; private set; }
        public int TotalHealing { get; private set; }

        public override void _Ready()
        {
            _instance = this;
            
            if (Data == null)
            {
                Data = new DamageNumberData();
            }
            
            // Create canvas for damage numbers
            _targetCanvas = new CanvasLayer();
            AddChild(_targetCanvas);
        }

        public override void _Process(double delta)
        {
            float deltaF = (float)delta;
            
            // Update all active damage numbers
            for (int i = _activeNumbers.Count - 1; i >= 0; i--)
            {
                var number = _activeNumbers[i];
                number.Update(deltaF);
                
                if (number.LifeTime >= number.TotalDuration)
                {
                    // Remove the label
                    number.Label.QueueFree();
                    _activeNumbers.RemoveAt(i);
                }
            }
        }

        public void ShowDamage(Vector2 worldPosition, int damage, bool isCritical = false, bool isHeal = false, bool isMiss = false)
        {
            // Create label
            var label = new Label();
            label.Text = isMiss ? "MISS" : (isHeal ? $"+{damage}" : $"-{damage}");
            
            // Set colors
            Color textColor;
            float fontSize;
            
            if (isMiss)
            {
                textColor = Data.MissColor;
                fontSize = Data.FontSize;
            }
            else if (isHeal)
            {
                textColor = Data.HealColor;
                fontSize = isCritical ? Data.CriticalFontSize : Data.FontSize;
            }
            else if (isCritical)
            {
                textColor = Data.CriticalColor;
                fontSize = Data.CriticalFontSize;
            }
            else
            {
                textColor = Data.TextColor;
                fontSize = Data.FontSize;
            }
            
            label.AddThemeColorOverride("font_color", textColor);
            label.AddThemeFontSizeOverride("font_size", (int)fontSize);
            
            // Set position (convert world to screen)
            var viewport = GetViewport();
            var screenPos = viewport.GetCamera2D()?.GetViewport().GetVisibleRect().Size / 2 ?? new Vector2(640, 360);
            var camera = viewport.GetCamera2D();
            
            if (camera != null)
            {
                Vector2 finalPos = camera.UnprojectPosition(new Vector3(worldPosition, 0));
                
                // Add random offset
                float offsetX = (float)(_random.NextDouble() * 2 - 1) * Data.RandomOffsetRange.x;
                float offsetY = (float)(_random.NextDouble() * 2 - 1) * Data.RandomOffsetRange.y;
                finalPos += new Vector2(offsetX, offsetY);
                
                label.Position = finalPos;
            }
            else
            {
                label.Position = worldPosition;
            }
            
            // Add to canvas
            _targetCanvas.AddChild(label);
            
            // Create instance
            var instance = new DamageNumberInstance
            {
                Label = label,
                LifeTime = 0f,
                TotalDuration = Data.FloatDuration,
                StartPosition = label.Position,
                IsCritical = isCritical,
                IsHeal = isHeal,
                IsMiss = isMiss,
                Random = _random
            };
            
            _activeNumbers.Add(instance);
            
            // Update statistics
            TotalDamageNumbers++;
            if (isCritical) CriticalHits++;
            if (!isHeal && !isMiss) TotalDamage += damage;
            if (isHeal) TotalHealing += damage;
        }

        public void ClearAll()
        {
            foreach (var number in _activeNumbers)
            {
                number.Label.QueueFree();
            }
            _activeNumbers.Clear();
        }

        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                { "totalDamageNumbers", TotalDamageNumbers },
                { "criticalHits", CriticalHits },
                { "totalDamage", TotalDamage },
                { "totalHealing", TotalHealing },
                { "activeCount", _activeNumbers.Count }
            };
        }

        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData() {
            var data = new Dictionary();
            data["totalDamageNumbers"] = TotalDamageNumbers;
            data["criticalHits"] = CriticalHits;
            data["totalDamage"] = TotalDamage;
            data["totalHealing"] = TotalHealing;
            return data;
        }

        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data) {
            if (data == null) return;
            
            if (data.Contains("totalDamageNumbers")) TotalDamageNumbers = (int)data["totalDamageNumbers"];
            if (data.Contains("criticalHits")) CriticalHits = (int)data["criticalHits"];
            if (data.Contains("totalDamage")) TotalDamage = (int)data["totalDamage"];
            if (data.Contains("totalHealing")) TotalHealing = (int)data["totalHealing"];
        }
    }
}
