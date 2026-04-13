using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Combat;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// REQ-180: Ghost Archive UI — Narrative Fragment Viewer
    ///
    /// Displays the player's Ghost Archive, showing unlocked narrative fragments
    /// earned through completing ghost combos.
    ///
    /// Fragments are grouped by tier (1=10pts, 2=25pts, 3=50pts).
    /// Each tier has a distinct color theme.
    ///
    /// Usage: Add to scene and call ShowArchive() / HideArchive().
    /// </summary>
    public partial class GhostArchiveUI : PanelContainer
    {
        // Tier colors
        private static readonly Color TIER1_COLOR = new Color(0.55f, 0.45f, 0.80f, 1.0f);  // Violet
        private static readonly Color TIER2_COLOR = new Color(0.80f, 0.55f, 0.25f, 1.0f);  // Amber
        private static readonly Color TIER3_COLOR = new Color(0.25f, 0.80f, 0.65f, 1.0f);  // Teal

        private VBoxContainer _mainContent;
        private Label _titleLabel;
        private Label _pointsLabel;
        private Label _pointsNeededLabel;
        private ScrollContainer _scroll;
        private VBoxContainer _fragmentContainer;
        private Label _emptyLabel;
        private Label _reconciledLabel;

        private bool _subscribed = false;

        public override void _Ready()
        {
            // Panel styling — dark, ghostly theme
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.04f, 0.03f, 0.10f, 0.92f);
            style.BorderColorLeft = new Color(0.40f, 0.30f, 0.70f, 0.7f);
            style.BorderColorRight = new Color(0.40f, 0.30f, 0.70f, 0.7f);
            style.BorderColorTop = new Color(0.40f, 0.30f, 0.70f, 0.7f);
            style.BorderColorBottom = new Color(0.40f, 0.30f, 0.70f, 0.7f);
            style.BorderWidthLeft = 1;
            style.BorderWidthRight = 1;
            style.BorderWidthTop = 1;
            style.BorderWidthBottom = 1;
            style.CornerRadiusTopLeft = 8;
            style.CornerRadiusTopRight = 8;
            style.CornerRadiusBottomLeft = 8;
            style.CornerRadiusBottomRight = 8;
            style.ContentMarginLeft = 16;
            style.ContentMarginTop = 12;
            style.ContentMarginRight = 16;
            style.ContentMarginBottom = 12;
            AddThemeStyleboxOverride("panel", style);

            _BuildLayout();
            Visible = false;
        }

        private void _BuildLayout()
        {
            _mainContent = new VBoxContainer
            {
                CustomMinimumSize = new Vector2(0, 0)
            };
            AddChild(_mainContent);

            // Title
            _titleLabel = new Label
            {
                Text = "👻 幽灵档案",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 16);
            _titleLabel.AddThemeColorOverride("font_color", new Color(0.75f, 0.65f, 1.0f, 1.0f));
            _mainContent.AddChild(_titleLabel);

            // Points summary
            _pointsLabel = new Label
            {
                Text = "执念点数: 0",
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            _pointsLabel.AddThemeFontSizeOverride("font_size", 12);
            _pointsLabel.AddThemeColorOverride("font_color", new Color(0.65f, 0.60f, 0.85f, 0.9f));
            _mainContent.AddChild(_pointsLabel);

            _pointsNeededLabel = new Label
            {
                Text = "距离下一片段: —",
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            _pointsNeededLabel.AddThemeFontSizeOverride("font_size", 11);
            _pointsNeededLabel.AddThemeColorOverride("font_color", new Color(0.50f, 0.45f, 0.70f, 0.7f));
            _mainContent.AddChild(_pointsNeededLabel);

            // Separator
            var sep = new HSeparator();
            sep.AddThemeColorOverride("separator", new Color(0.40f, 0.30f, 0.70f, 0.4f));
            _mainContent.AddChild(sep);

            // Scrollable fragment list
            _scroll = new ScrollContainer
            {
                HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
                CustomMinimumSize = new Vector2(0, 200),
            };
            _mainContent.AddChild(_scroll);

            _fragmentContainer = new VBoxContainer { };
            _scroll.AddChild(_fragmentContainer);

            _emptyLabel = new Label
            {
                Text = "尚未解锁任何片段...\n完成幽灵连招来获得执念点数。",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Modulate = new Color(0.50f, 0.45f, 0.65f, 0.7f),
            };
            _emptyLabel.AddThemeFontSizeOverride("font_size", 11);
            _fragmentContainer.AddChild(_emptyLabel);

            _reconciledLabel = new Label
            {
                Text = "你已与幽灵和解。",
                HorizontalAlignment = HorizontalAlignment.Center,
                Modulate = TIER3_COLOR,
            };
            _reconciledLabel.AddThemeFontSizeOverride("font_size", 12);
            _reconciledLabel.Visible = false;
            _mainContent.AddChild(_reconciledLabel);
        }

        private void _EnsureSubscription()
        {
            if (_subscribed) return;
            _subscribed = true;

            if (GhostConvictionSystem.Instance != null)
            {
                GhostConvictionSystem.OnConvictionPointsChanged += _OnPointsChanged;
                GhostConvictionSystem.OnFragmentsUnlocked += _OnFragmentsUnlocked;
                GhostConvictionSystem.OnAllFragmentsUnlocked += _OnAllUnlocked;
            }
        }

        /// <summary>
        /// Show the Ghost Archive panel with current state.
        /// </summary>
        public void ShowArchive()
        {
            _EnsureSubscription();
            RefreshDisplay();
            Visible = true;

            var tween = CreateTween();
            Modulate = new Color(1, 1, 1, 0);
            tween.TweenProperty(this, "modulate:a", 1.0f, 0.35f);
        }

        /// <summary>
        /// Hide the Ghost Archive panel.
        /// </summary>
        public void HideArchive()
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0f, 0.25f);
            tween.TweenCallback(Callable.From(() => Visible = false));
        }

        /// <summary>
        /// Refresh the displayed fragments and points.
        /// </summary>
        public void RefreshDisplay()
        {
            if (!IsInstanceValid(_mainContent)) return;

            if (GhostConvictionSystem.Instance == null) return;

            int points = GhostConvictionSystem.Instance.GetConvictionPoints();
            int toNext = GhostConvictionSystem.Instance.GetPointsToNextFragment();

            _pointsLabel.Text = $"执念点数: {points}";
            _pointsNeededLabel.Text = toNext > 0
                ? $"距离下一片段: {toNext} 点"
                : "所有片段已解锁";

            // Clear existing fragment items (keep empty/ reconciled labels)
            foreach (var child in _fragmentContainer.GetChildren())
            {
                if (child == _emptyLabel) continue;
                child.QueueFree();
            }

            var fragments = GhostConvictionSystem.Instance.GetUnlockedFragmentsWithContent();
            _emptyLabel.Visible = fragments.Count == 0;

            foreach (var (id, content, tier) in fragments)
            {
                var fragmentItem = _CreateFragmentItem(id, content, tier);
                _fragmentContainer.AddChild(fragmentItem);
            }

            bool allUnlocked = fragments.Count >= GhostConvictionDatabase.FragmentPool.Count;
            _reconciledLabel.Visible = allUnlocked;
        }

        private PanelContainer _CreateFragmentItem(string id, string content, int tier)
        {
            var color = tier switch
            {
                1 => TIER1_COLOR,
                2 => TIER2_COLOR,
                3 => TIER3_COLOR,
                _ => TIER1_COLOR,
            };

            var container = new PanelContainer();
            var itemStyle = new StyleBoxFlat();
            itemStyle.BgColor = new Color(color.R, color.G, color.B, 0.08f);
            itemStyle.BorderColorLeft = new Color(color.R, color.G, color.B, 0.5f);
            itemStyle.BorderColorRight = new Color(color.R, color.G, color.B, 0.5f);
            itemStyle.BorderColorTop = new Color(color.R, color.G, color.B, 0.5f);
            itemStyle.BorderColorBottom = new Color(color.R, color.G, color.B, 0.5f);
            itemStyle.BorderWidthLeft = 1;
            itemStyle.BorderWidthRight = 1;
            itemStyle.BorderWidthTop = 1;
            itemStyle.BorderWidthBottom = 1;
            itemStyle.CornerRadiusTopLeft = 4;
            itemStyle.CornerRadiusTopRight = 4;
            itemStyle.CornerRadiusBottomLeft = 4;
            itemStyle.CornerRadiusBottomRight = 4;
            itemStyle.ContentMarginLeft = 10;
            itemStyle.ContentMarginTop = 8;
            itemStyle.ContentMarginRight = 10;
            itemStyle.ContentMarginBottom = 8;
            itemStyle.ContentMarginBottom = 8;
            container.AddThemeStyleboxOverride("panel", itemStyle);

            var vbox = new VBoxContainer();
            container.AddChild(vbox);

            var tierLabel = new Label
            {
                Text = tier switch
                {
                    1 => "◆ 第一片段 (10点)",
                    2 => "◆◆ 第二片段 (25点)",
                    3 => "◆◆◆ 最终片段 (50点)",
                    _ => "◆ 片段"
                },
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            tierLabel.AddThemeFontSizeOverride("font_size", 10);
            tierLabel.AddThemeColorOverride("font_color", new Color(color.R, color.G, color.B, 0.8f));
            vbox.AddChild(tierLabel);

            var contentLabel = new Label
            {
                Text = content,
                HorizontalAlignment = HorizontalAlignment.Left,
                AutowrapMode = TextServer.AutowrapMode.Word,
            };
            contentLabel.AddThemeFontSizeOverride("font_size", 11);
            contentLabel.AddThemeColorOverride("font_color", new Color(0.82f, 0.78f, 0.95f, 0.92f));
            vbox.AddChild(contentLabel);

            return container;
        }

        private void _OnPointsChanged(int newTotal)
        {
            // Silent update — user can check archive at any time
        }

        private void _OnFragmentsUnlocked(List<string> newlyUnlocked)
        {
            // Optionally flash a small notification
            GD.Print($"[GhostArchiveUI] New fragments unlocked: {string.Join(", ", newlyUnlocked)}");
            RefreshDisplay();
        }

        private void _OnAllUnlocked()
        {
            GD.Print("[GhostArchiveUI] Player has reconciled with their ghosts!");
            RefreshDisplay();
        }

        public override void _ExitTree()
        {
            if (_subscribed && GhostConvictionSystem.Instance != null)
            {
                GhostConvictionSystem.OnConvictionPointsChanged -= _OnPointsChanged;
                GhostConvictionSystem.OnFragmentsUnlocked -= _OnFragmentsUnlocked;
                GhostConvictionSystem.OnAllFragmentsUnlocked -= _OnAllUnlocked;
            }
            base._ExitTree();
        }
    }
}
