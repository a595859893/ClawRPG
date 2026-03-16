using Godot;
using System;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Display logic for CombatStatsPanel
    /// Handles animations, rating display, and visual effects
    /// </summary>
    public class CombatStatsPanelDisplay
    {
        private readonly CombatStatsPanel _owner;
        
        public CombatStatsPanelDisplay(CombatStatsPanel owner)
        {
            _owner = owner;
        }
        
        /// <summary>
        /// Setup the rating panel - delegates to components
        /// </summary>
        public void SetupRatingPanel()
        {
            // Access the components through owner's internal reference
            // This is handled via the owner passing itself
            var componentsField = typeof(CombatStatsPanel).GetField("_components", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (componentsField != null)
            {
                var components = componentsField.GetValue(_owner) as CombatStatsPanelComponents;
                components?.SetupRatingPanel();
            }
        }
        
        /// <summary>
        /// Show combat rating popup
        /// </summary>
        public void ShowRating(Func<float> calculateRating, Func<float, (string letter, string detail, Color color)> getRatingInfo)
        {
            float score = calculateRating();
            var (letter, detail, color) = getRatingInfo(score);
            
            // Get rating panel from components
            var componentsField = typeof(CombatStatsPanel).GetField("_components", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (componentsField == null) return;
            
            var components = componentsField.GetValue(_owner) as CombatStatsPanelComponents;
            if (components == null) return;
            
            components.RatingLabel.Text = letter;
            components.RatingLabel.AddThemeColorOverride("font_color", color);
            components.RatingDetailLabel.Text = detail;
            
            components.RatingPanel.Visible = true;
            
            // Animate rating panel
            components.RatingPanel.Modulate = new Color(1f, 1f, 1f, 0f);
            components.RatingPanel.Scale = new Vector2(0.5f, 0.5f);
            
            var tween = _owner.CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(components.RatingPanel, "modulate:a", 1f, 0.3f);
            tween.TweenProperty(components.RatingPanel, "scale", new Vector2(1.1f, 1.1f), 0.3f);
            tween.TweenCallback(new Callable(_owner, nameof(OnRatingShowComplete)));
        }
        
        private void OnRatingShowComplete()
        {
            // Get rating panel from components
            var componentsField = typeof(CombatStatsPanel).GetField("_components", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (componentsField == null) return;
            
            var components = componentsField.GetValue(_owner) as CombatStatsPanelComponents;
            if (components?.RatingPanel == null) return;
            
            // Bounce effect
            var tween = _owner.CreateTween();
            tween.TweenProperty(components.RatingPanel, "scale", new Vector2(1f, 1f), 0.1f);
        }
        
        /// <summary>
        /// Hide rating panel
        /// </summary>
        public void HideRating()
        {
            var componentsField = typeof(CombatStatsPanel).GetField("_components", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (componentsField == null) return;
            
            var components = componentsField.GetValue(_owner) as CombatStatsPanelComponents;
            if (components?.RatingPanel != null)
            {
                components.RatingPanel.Visible = false;
            }
        }
        
        /// <summary>
        /// Play appear animation for the panel
        /// </summary>
        public void PlayAppearAnimation(Control panel)
        {
            panel.Modulate = new Color(1f, 1f, 1f, 0f);
            var tween = panel.CreateTween();
            tween.TweenProperty(panel, "modulate:a", 1f, 0.3f);
        }
        
        /// <summary>
        /// Pulse a label to highlight it
        /// </summary>
        public void PulseLabel(Label label)
        {
            var tween = label.CreateTween();
            tween.TweenProperty(label, "modulate", new Color(1.5f, 1.5f, 1.5f, 1f), 0.1f);
            tween.TweenProperty(label, "modulate", Colors.White, 0.2f);
        }
    }
}
