using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// Game Message System - Displays in-game messages and notifications
    /// </summary>
    public partial class GameMessageSystem : Control
    {
        public static GameMessageSystem Instance { get; private set; }

        [Export] private Vector2 messagePosition = new Vector2(100, 200);
        [Export] private float messageSpacing = 45f;
        [Export] private float messageDuration = 3f;
        [Export] private float fadeDuration = 0.5f;

        private VBoxContainer messageContainer;
        private List<MessageEntry> activeMessages = new List<MessageEntry>();
        private int maxMessages = 8;

        private class MessageEntry
        {
            public Label Label;
            public float Timer;
            public float Lifetime;
            public bool IsFading;
        }

        public override void _Ready()
        {
            Instance = this;
            SetupUI();
        }

        private void SetupUI()
        {
            messageContainer = new VBoxContainer
            {
                Name = "MessageContainer",
                ZIndex = 100
            };
            messageContainer.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
            messageContainer.Position = messagePosition;
            AddChild(messageContainer);
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;
            
            for (int i = activeMessages.Count - 1; i >= 0; i--)
            {
                var entry = activeMessages[i];
                entry.Timer += dt;
                
                // Start fading
                if (!entry.IsFading && entry.Timer >= messageDuration - fadeDuration)
                {
                    entry.IsFading = true;
                    var tween = CreateTween();
                    tween.TweenProperty(entry.Label, "modulate:a", 0f, fadeDuration);
                }
                
                // Remove expired messages
                if (entry.Timer >= messageDuration)
                {
                    entry.Label.QueueFree();
                    activeMessages.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Show a simple text message
        /// </summary>
        public void ShowMessage(string text, Color? color = null)
        {
            ShowMessage(text, MessageType.Info, color);
        }

        /// <summary>
        /// Show a message with specific type
        /// </summary>
        public void ShowMessage(string text, MessageType type, Color? overrideColor = null)
        {
            // Remove oldest if at max
            if (activeMessages.Count >= maxMessages && activeMessages.Count > 0)
            {
                var oldest = activeMessages[0];
                oldest.Label.QueueFree();
                activeMessages.RemoveAt(0);
            }

            var label = new Label();
            label.Text = GetMessagePrefix(type) + text;
            label.AddThemeFontSizeOverride("font_size", 16);
            
            Color textColor = overrideColor ?? GetMessageColor(type);
            label.AddThemeColorOverride("font_color", textColor);
            
            // Add shadow for readability
            label.Set("outline_size", 2);
            label.Set("outline_color", new Color(0, 0, 0, 0.8f));
            
            messageContainer.AddChild(label);

            var entry = new MessageEntry
            {
                Label = label,
                Timer = 0f,
                Lifetime = messageDuration,
                IsFading = false
            };
            
            // Animate in
            label.Modulate = new Color(1, 1, 1, 0);
            var tween = CreateTween();
            tween.TweenProperty(label, "modulate:a", 1f, 0.2f);
            
            activeMessages.Add(entry);
        }

        /// <summary>
        /// Show a positive message (gains, rewards)
        /// </summary>
        public void ShowPositive(string text)
        {
            ShowMessage(text, MessageType.Success);
        }

        /// <summary>
        /// Show a negative message (damage, losses)
        /// </summary>
        public void ShowNegative(string text)
        {
            ShowMessage(text, MessageType.Danger);
        }

        /// <summary>
        /// Show a warning message
        /// </summary>
        public void ShowWarning(string text)
        {
            ShowMessage(text, MessageType.Warning);
        }

        /// <summary>
        /// Show an achievement message
        /// </summary>
        public void ShowAchievement(string achievementName)
        {
            ShowMessage($"🏆 成就解锁: {achievementName}", MessageType.Achievement);
        }

        /// <summary>
        /// Show quest update
        /// </summary>
        public void ShowQuestUpdate(string questName)
        {
            ShowMessage($"📜 任务更新: {questName}", MessageType.Quest);
        }

        /// <summary>
        /// Show level up message
        /// </summary>
        public void ShowLevelUp(int newLevel)
        {
            ShowMessage($"⬆️ 升级! 当前等级: {newLevel}", MessageType.LevelUp);
        }

        private Color GetMessageColor(MessageType type)
        {
            return type switch
            {
                MessageType.Info => new Color(0.9f, 0.9f, 1f, 1f),
                MessageType.Success => new Color(0.3f, 1f, 0.3f, 1f),
                MessageType.Warning => new Color(1f, 0.8f, 0.2f, 1f),
                MessageType.Danger => new Color(1f, 0.3f, 0.3f, 1f),
                MessageType.Quest => new Color(0.4f, 0.7f, 1f, 1f),
                MessageType.Achievement => new Color(1f, 0.84f, 0f, 1f),
                MessageType.LevelUp => new Color(1f, 0.6f, 0.2f, 1f),
                _ => new Color(1f, 1f, 1f, 1f)
            };
        }

        private string GetMessagePrefix(MessageType type)
        {
            return type switch
            {
                MessageType.Info => "ℹ️ ",
                MessageType.Success => "✅ ",
                MessageType.Warning => "⚠️ ",
                MessageType.Danger => "❌ ",
                MessageType.Quest => "",
                MessageType.Achievement => "",
                MessageType.LevelUp => "",
                _ => ""
            };
        }

        public enum MessageType
        {
            Info,
            Success,
            Warning,
            Danger,
            Quest,
            Achievement,
            LevelUp
        }
    }
}
