using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Combat Stats Panel Display - Data formatting and display logic
    /// Handles display updates, animations, and rating system
    /// </summary>
    public partial class CombatStatsPanel : Control
    {
        // Rating constants
        private const float RATING_S_THRESHOLD = 95f;  // S rank: top 5%
        private const float RATING_A_THRESHOLD = 85f;  // A rank: top 15%
        private const float RATING_B_THRESHOLD = 70f;  // B rank: top 30%
        private const float RATING_C_THRESHOLD = 50f;  // C rank: top 50%
        // Below C is D rank
        
        #region Display Updates
        
        private void UpdateDisplay()
        {
            _damageDealtLabel.Text = _totalDamageDealt.ToString("N0");
            _damageTakenLabel.Text = _totalDamageTaken.ToString("N0");
            _killsLabel.Text = _totalKills.ToString();
            _dodgesLabel.Text = _totalDodges.ToString();
            _blocksLabel.Text = _totalBlocks.ToString();
            _critsLabel.Text = _totalCrits.ToString();
            _comboLabel.Text = _maxCombo.ToString();
            
            // Update combat time
            float elapsed = (Time.GetTicksMsec() / 1000f) - _combatStartTime;
            int minutes = (int)(elapsed / 60);
            int seconds = (int)(elapsed % 60);
            _combatTimeLabel.Text = $"{minutes}:{seconds:D2}";
        }
        
        private void PulseLabel(Label label)
        {
            _pulseTween?.Kill();
            _pulseTween = CreateTween();
            _pulseTween.TweenProperty(label, "modulate", new Color(1.5f, 1.5f, 1.5f, 1f), 0.1f);
            _pulseTween.TweenProperty(label, "modulate", Colors.White, 0.2f);
        }
        
        private void PlayAppearAnimation()
        {
            Modulate = new Color(1f, 1f, 1f, 0f);
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 1f, 0.3f);
        }
        
        #endregion
        
        #region Rating System
        
        /// <summary>
        /// Calculate combat rating based on performance metrics
        /// </summary>
        private float CalculateRating()
        {
            if (_totalKills == 0) return 0f;
            
            float score = 0f;
            
            // 1. Damage efficiency (40% weight)
            // Higher damage per kill = better
            float damagePerKill = _totalDamageDealt / (float)_totalKills;
            float damageScore = Math.Min(damagePerKill / 500f, 1f) * 40f;
            score += damageScore;
            
            // 2. Survival (30% weight)
            // Less damage taken = better
            float survivalScore = 0f;
            if (_totalDamageTaken == 0)
            {
                survivalScore = 30f; // Perfect survival
            }
            else
            {
                float damagePerKillTaken = _totalDamageDealt / (float)Math.Max(_totalDamageTaken, 1);
                survivalScore = Math.Min(damagePerKillTaken / 10f, 1f) * 30f;
            }
            score += survivalScore;
            
            // 3. Skill usage (20% weight)
            // Dodges, blocks, crits show player skill
            float totalSkillActions = _totalDodges + _totalBlocks + _totalCrits;
            float skillScore = Math.Min(totalSkillActions / (float)Math.Max(_totalKills, 1) * 2f, 1f) * 20f;
            score += skillScore;
            
            // 4. Combat efficiency (10% weight)
            // Fast kills = better
            float combatTime = (Time.GetTicksMsec() / 1000f) - _combatStartTime;
            if (combatTime > 0)
            {
                float killsPerSecond = _totalKills / combatTime;
                float efficiencyScore = Math.Min(killsPerSecond * 5f, 1f) * 10f;
                score += efficiencyScore;
            }
            
            return Math.Min(score, 100f);
        }
        
        /// <summary>
        /// Get rating letter based on score
        /// </summary>
        private (string letter, string detail, Color color) GetRatingInfo(float score)
        {
            if (score >= RATING_S_THRESHOLD)
                return ("S", "完美表现！", new Color(1f, 0.84f, 0f, 1f)); // Gold
            if (score >= RATING_A_THRESHOLD)
                return ("A", "出色发挥！", new Color(0.4f, 1f, 0.4f, 1f)); // Green
            if (score >= RATING_B_THRESHOLD)
                return ("B", "良好水平", new Color(0.4f, 0.8f, 1f, 1f)); // Blue
            if (score >= RATING_C_THRESHOLD)
                return ("C", "还需练习", new Color(1f, 0.7f, 0.4f, 1f)); // Orange
            return ("D", "继续努力", new Color(0.8f, 0.5f, 0.5f, 1f)); // Red
        }
        
        /// <summary>
        /// Show combat rating popup
        /// </summary>
        private void ShowRating()
        {
            float score = CalculateRating();
            var (letter, detail, color) = GetRatingInfo(score);
            
            _ratingLabel.Text = letter;
            _ratingLabel.AddThemeColorOverride("font_color", color);
            _ratingDetailLabel.Text = detail;
            
            _ratingPanel.Visible = true;
            
            // Animate rating panel
            _ratingPanel.Modulate = new Color(1f, 1f, 1f, 0f);
            _ratingPanel.Scale = new Vector2(0.5f, 0.5f);
            
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(_ratingPanel, "modulate:a", 1f, 0.3f);
            tween.TweenProperty(_ratingPanel, "scale", new Vector2(1.1f, 1.1f), 0.3f);
            tween.TweenCallback(new Callable(this, nameof(_OnRatingShowComplete)));
        }
        
        private void _OnRatingShowComplete()
        {
            // Bounce effect
            var tween = CreateTween();
            tween.TweenProperty(_ratingPanel, "scale", new Vector2(1f, 1f), 0.1f);
        }
        
        /// <summary>
        /// Hide rating panel
        /// </summary>
        public void HideRating()
        {
            _ratingPanel.Visible = false; 
        }
        
        /// <summary>
        /// Get current combat rating (call after combat ends)
        /// </summary>
        public string GetCurrentRating()
        {
            float score = CalculateRating();
            var (letter, _, _) = GetRatingInfo(score);
            return letter;
        }
        
        /// <summary>
        /// Get detailed rating info
        /// </summary>
        public (string letter, string detail, float score) GetRatingDetails()
        {
            float score = CalculateRating();
            var (letter, detail, _) = GetRatingInfo(score);
            return (letter, detail, score);
        }
        
        #endregion
    }
}
