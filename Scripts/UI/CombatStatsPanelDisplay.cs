using Godot;
using System;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Display utilities for CombatStatsPanel
    /// Handles animations, formatting, and visual effects
    /// </summary>
    public static class CombatStatsPanelDisplay
    {
        /// <summary>
        /// Play appear animation for the panel
        /// </summary>
        public static void PlayAppearAnimation(Control panel)
        {
            panel.Modulate = new Color(1f, 1f, 1f, 0f);
            var tween = panel.CreateTween();
            tween.TweenProperty(panel, "modulate:a", 1f, 0.3f);
        }
        
        /// <summary>
        /// Pulse a label to highlight it
        /// </summary>
        public static void PulseLabel(Label label)
        {
            var tween = label.CreateTween();
            tween.TweenProperty(label, "modulate", new Color(1.5f, 1.5f, 1.5f, 1f), 0.1f);
            tween.TweenProperty(label, "modulate", Colors.White, 0.2f);
        }
        
        /// <summary>
        /// Play rating show animation
        /// </summary>
        public static void PlayRatingShowAnimation(PanelContainer ratingPanel, Control owner, string callbackMethod)
        {
            // Animate rating panel
            ratingPanel.Modulate = new Color(1f, 1f, 1f, 0f);
            ratingPanel.Scale = new Vector2(0.5f, 0.5f);
            
            var tween = owner.CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(ratingPanel, "modulate:a", 1f, 0.3f);
            tween.TweenProperty(ratingPanel, "scale", new Vector2(1.1f, 1.1f), 0.3f);
            tween.TweenCallback(new Callable(owner, nameof(callbackMethod)));
        }
        
        /// <summary>
        /// Play rating bounce animation
        /// </summary>
        public static void PlayRatingBounceAnimation(PanelContainer ratingPanel, Control owner)
        {
            var tween = owner.CreateTween();
            tween.TweenProperty(ratingPanel, "scale", new Vector2(1f, 1f), 0.1f);
        }
        
        /// <summary>
        /// Format combat time from seconds to MM:SS
        /// </summary>
        public static string FormatCombatTime(float elapsedSeconds)
        {
            int minutes = (int)(elapsedSeconds / 60);
            int seconds = (int)(elapsedSeconds % 60);
            return $"{minutes}:{seconds:D2}";
        }
        
        /// <summary>
        /// Format number with thousand separators
        /// </summary>
        public static string FormatNumber(int value)
        {
            return value.ToString("N0");
        }
    }
}
