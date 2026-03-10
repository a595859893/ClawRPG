using Godot;
using System;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// Screen Flash Effect - Visual feedback for damage, attacks, etc.
    /// </summary>
    public partial class ScreenFlashEffect : CanvasLayer
    {
        public static ScreenFlashEffect Instance { get; private set; }

        [Export] private Color damageColor = new Color(1f, 0f, 0f, 0.4f);
        [Export] private Color healColor = new Color(0f, 1f, 0f, 0.3f);
        [Export] private Color perfectBlockColor = new Color(1f, 1f, 1f, 0.5f);
        [Export] private Color levelUpColor = new Color(1f, 0.84f, 0f, 0.4f);
        [Export] private Color enemyHitColor = new Color(1f, 0.5f, 0f, 0.3f);

        private ColorRect flashRect;
        private Tween tween;

        public override void _Ready()
        {
            Instance = this;
            SetupFlashRect();
        }

        private void SetupFlashRect()
        {
            flashRect = new ColorRect
            {
                Name = "FlashRect",
                Color = new Color(0, 0, 0, 0),
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            
            flashRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(flashRect);
        }

        /// <summary>
        /// Flash screen with damage effect (red)
        /// </summary>
        public void FlashDamage(float intensity = 1f)
        {
            var color = damageColor;
            color.A *= intensity;
            Flash(color, 0.15f);
        }

        /// <summary>
        /// Flash screen with heal effect (green)
        /// </summary>
        public void FlashHeal(float intensity = 1f)
        {
            var color = healColor;
            color.A *= intensity;
            Flash(color, 0.2f);
        }

        /// <summary>
        /// Flash screen with perfect block effect (white)
        /// </summary>
        public void FlashPerfectBlock(float intensity = 1f)
        {
            var color = perfectBlockColor;
            color.A *= intensity;
            Flash(color, 0.1f);
        }

        /// <summary>
        /// Flash screen with level up effect (gold)
        /// </summary>
        public void FlashLevelUp(float intensity = 1f)
        {
            var color = levelUpColor;
            color.A *= intensity;
            Flash(color, 0.4f);
        }

        /// <summary>
        /// Flash screen with enemy hit effect (orange)
        /// </summary>
        public void FlashEnemyHit(float intensity = 1f)
        {
            var color = enemyHitColor;
            color.A *= intensity;
            Flash(color, 0.1f);
        }

        /// <summary>
        /// Custom color flash
        /// </summary>
        public void Flash(Color color, float duration)
        {
            if (tween != null && tween.IsValid())
                tween.Kill();

            tween = CreateTween();
            flashRect.Color = color;
            
            tween.TweenProperty(flashRect, "color:a", 0f, duration)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransType.Quad);
        }

        /// <summary>
        /// Pulse effect - quick flash in and out
        /// </summary>
        public void Pulse(Color color, int pulses = 2, float duration = 0.1f)
        {
            if (tween != null && tween.IsValid())
                tween.Kill();

            tween = CreateTween();
            tween.SetLoops(pulses * 2);
            
            flashRect.Color = color;
            
            tween.TweenProperty(flashRect, "color:a", 0f, duration)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransType.Quad);
            tween.TweenProperty(flashRect, "color:a", color.A, duration)
                .SetEase(Tween.EaseType.In)
                .SetTrans(Tween.TransType.Quad);
        }
    }
}
