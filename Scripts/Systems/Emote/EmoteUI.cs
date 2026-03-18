using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Systems.Emote {
    public partial class EmoteUI : Control {
        private PanelContainer mainPanel;
        private VBoxContainer contentVBox;
        private TabContainer tabContainer;
        
        // Shop tab
        private VBoxContainer shopVBox;
        private ScrollContainer shopScroll;
        private GridContainer shopGrid;
        
        // My Emotes tab
        private VBoxContainer myEmotesVBox;
        private ScrollContainer myEmotesScroll;
        private GridContainer myEmotesGrid;
        
        // Favorites tab
        private VBoxContainer favoritesVBox;
        private ScrollContainer favoritesScroll;
        private GridContainer favoritesGrid;
        
        // Stats tab
        private VBoxContainer statsVBox;
        private Label statsLabel;
        
        private bool isVisible = false;
        private int selectedCategory = -1; // -1 = all

        public override void _Ready() {
            SetupUI();
            Hide();
        }

        private void SetupUI() {
            // Main panel
            mainPanel = new PanelContainer();
            mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            mainPanel.CustomMinimumSize = new Vector2(800, 600);
            AddChild(mainPanel);
            
            // Create tabs
            tabContainer = new TabContainer();
            tabContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            mainPanel.AddChild(tabContainer);
            
            // ===== Shop Tab =====
            shopVBox = new VBoxContainer();
            shopVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            shopVBox.AddThemeConstantOverride("separation", 10);
            tabContainer.AddChild(shopVBox);
            tabContainer.SetTabTitle(0, "🛒 Shop");
            
            // Category filter for shop
            HBoxContainer shopFilter = new HBoxContainer();
            shopFilter.AddThemeConstantOverride("separation", 5);
            shopVBox.AddChild(shopFilter);
            
            Button allBtn = new Button();
            allBtn.Text = "All";
            allBtn.Pressed += () => { selectedCategory = -1; RefreshShop(); };
            shopFilter.AddChild(allBtn);
            
            string[] categories = { "Happy", "Sad", "Angry", "Excited", "Thinking", "Greeting", "Victory", "Defeat", "Love", "Misc" };
            for (int i = 0; i < categories.Length; i++) {
                int catIndex = i;
                Button catBtn = new Button();
                catBtn.Text = categories[i];
                catBtn.Pressed += () => { selectedCategory = catIndex; RefreshShop(); };
                shopFilter.AddChild(catBtn);
            }
            
            shopScroll = new ScrollContainer();
            shopScroll.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            shopScroll.VScrollRelativeOffset = new Vector2(0, 0);
            shopVBox.AddChild(shopScroll);
            
            shopGrid = new GridContainer();
            shopGrid.Columns = 4;
            shopGrid.AddThemeConstantOverride("h_separation", 10);
            shopGrid.AddThemeConstantOverride("v_separation", 10);
            shopScroll.AddChild(shopGrid);
            
            // ===== My Emotes Tab =====
            myEmotesVBox = new VBoxContainer();
            myEmotesVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            tabContainer.AddChild(myEmotesVBox);
            tabContainer.SetTabTitle(1, "✨ My Emotes");
            
            myEmotesScroll = new ScrollContainer();
            myEmotesScroll.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            myEmotesVBox.AddChild(myEmotesScroll);
            
            myEmotesGrid = new GridContainer();
            myEmotesGrid.Columns = 4;
            myEmotesGrid.AddThemeConstantOverride("h_separation", 10);
            myEmotesGrid.AddThemeConstantOverride("v_separation", 10);
            myEmotesScroll.AddChild(myEmotesGrid);
            
            // ===== Favorites Tab =====
            favoritesVBox = new VBoxContainer();
            favoritesVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            tabContainer.AddChild(favoritesVBox);
            tabContainer.SetTabTitle(2, "❤️ Favorites");
            
            favoritesScroll = new ScrollContainer();
            favoritesScroll.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            favoritesVBox.AddChild(favoritesScroll);
            
            favoritesGrid = new GridContainer();
            favoritesGrid.Columns = 4;
            favoritesGrid.AddThemeConstantOverride("h_separation", 10);
            favoritesGrid.AddThemeConstantOverride("v_separation", 10);
            favoritesScroll.AddChild(favoritesGrid);
            
            // ===== Stats Tab =====
            statsVBox = new VBoxContainer();
            statsVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            statsVBox.AddThemeConstantOverride("separation", 10);
            tabContainer.AddChild(statsVBox);
            tabContainer.SetTabTitle(3, "📊 Stats");
            
            statsLabel = new Label();
            statsLabel.Text = "Emote Statistics";
            statsVBox.AddChild(statsLabel);
            
            // Info label
            Label infoLabel = new Label();
            infoLabel.Text = "Press E to open Emote Menu | Click emote to use it";
            infoLabel.HorizontalAlignment = HorizontalAlignment.Center;
            statsVBox.AddChild(infoLabel);
            
            // Close button
            Button closeBtn = new Button();
            closeBtn.Text = "Close (ESC)";
            closeBtn.Pressed += Toggle;
            statsVBox.AddChild(closeBtn);
            
            RefreshAll();
        }

        public void Toggle() {
            if (isVisible) {
                Hide();
                isVisible = false;
            } else {
                Show();
                isVisible = true;
                RefreshAll();
            }
        }

        private void RefreshAll() {
            RefreshShop();
            RefreshMyEmotes();
            RefreshFavorites();
            RefreshStats();
        }

        private void RefreshShop() {
            // Clear existing
            foreach (Node child in shopGrid.GetChildren()) {
                child.QueueFree();
            }
            
            var shopEmotes = EmoteSystem.Instance.GetShopEmotes();
            if (selectedCategory >= 0) {
                shopEmotes = shopEmotes.Where(e => (int)e.Category == selectedCategory).ToList();
            }
            
            foreach (var emote in shopEmotes) {
                shopGrid.AddChild(CreateEmoteCard(emote, true));
            }
        }

        private void RefreshMyEmotes() {
            foreach (Node child in myEmotesGrid.GetChildren()) {
                child.QueueFree();
            }
            
            var unlockedEmotes = EmoteSystem.Instance.GetUnlockedEmotes();
            foreach (var emote in unlockedEmotes) {
                myEmotesGrid.AddChild(CreateEmoteCard(emote, false, true));
            }
        }

        private void RefreshFavorites() {
            foreach (Node child in favoritesGrid.GetChildren()) {
                child.QueueFree();
            }
            
            var favorites = EmoteSystem.Instance.GetFavoriteEmotes();
            foreach (var emote in favorites) {
                favoritesGrid.AddChild(CreateEmoteCard(emote, false, true, true));
            }
        }

        private void RefreshStats() {
            var stats = EmoteSystem.Instance.GetUsageStatistics();
            var mostUsed = EmoteSystem.Instance.GetMostUsedEmote();
            
            string statsText = "=== Emote Statistics ===\n\n";
            statsText += $"Total Emotes Unlocked: {EmoteSystem.Instance.GetUnlockedEmotes().Count}\n";
            statsText += $"Favorite Emotes: {EmoteSystem.Instance.GetFavoriteEmotes().Count}\n\n";
            
            if (mostUsed != null) {
                var emote = ClawRPG.Systems.Emote.EmoteDatabase.Instance.GetEmote(mostUsed);
                statsText += $"Most Used: {emote?.Name ?? "Unknown"}\n";
            }
            
            statsText += "\n=== Usage Count ===\n";
            foreach (var kvp in stats.OrderByDescending(k => k.Value).Take(10)) {
                var e = ClawRPG.Systems.Emote.EmoteDatabase.Instance.GetEmote(kvp.Key);
                statsText += $"{e?.Name ?? kvp.Key}: {kvp.Value} uses\n";
            }
            
            statsLabel.Text = statsText;
        }

        private Control CreateEmoteCard(Emote emote, bool showBuy = false, bool showUse = false, bool showFavorite = false) {
            var card = new PanelContainer();
            card.CustomMinimumSize = new Vector2(150, 120);
            
            VBoxContainer cardVBox = new VBoxContainer();
            cardVBox.AddThemeConstantOverride("separation", 5);
            card.AddChild(cardVBox);
            
            // Name
            Label nameLabel = new Label();
            nameLabel.Text = emote.Name;
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            nameLabel.AddThemeFontSizeOverride("font_size", 14);
            cardVBox.AddChild(nameLabel);
            
            // Rarity color
            Color rarityColor = GetRarityColor(emote.Rarity);
            card.AddThemeStyleboxOverride("panel", CreateRarityStyle(rarityColor));
            
            // Category
            Label categoryLabel = new Label();
            categoryLabel.Text = emote.Category.ToString();
            categoryLabel.HorizontalAlignment = HorizontalAlignment.Center;
            categoryLabel.AddThemeFontSizeOverride("font_size", 10);
            cardVBox.AddChild(categoryLabel);
            
            if (showBuy) {
                // Price
                Label priceLabel = new Label();
                priceLabel.Text = $"💰 {emote.Cost}";
                priceLabel.HorizontalAlignment = HorizontalAlignment.Center;
                cardVBox.AddChild(priceLabel);
                
                // Buy button
                Button buyBtn = new Button();
                buyBtn.Text = "Buy";
                buyBtn.Pressed += () => {
                    if (EmoteSystem.Instance.UnlockEmote(emote.Id)) {
                        RefreshAll();
                    } else {
                        GD.Print("Not enough gold!");
                    }
                };
                cardVBox.AddChild(buyBtn);
            }
            
            if (showUse) {
                // Use button
                Button useBtn = new Button();
                useBtn.Text = "Use";
                useBtn.Pressed += () => {
                    EmoteSystem.Instance.UseEmote(emote.Id);
                };
                cardVBox.AddChild(useBtn);
            }
            
            if (showFavorite) {
                // Remove from favorites
                Button favBtn = new Button();
                favBtn.Text = "❤️";
                favBtn.Pressed += () => {
                    EmoteSystem.Instance.RemoveFavorite(emote.Id);
                    RefreshAll();
                };
                cardVBox.AddChild(favBtn);
            } else if (showUse) {
                // Add to favorites
                Button addFavBtn = new Button();
                addFavBtn.Text = "🤍";
                addFavBtn.Pressed += () => {
                    EmoteSystem.Instance.AddFavorite(emote.Id);
                    RefreshAll();
                };
                cardVBox.AddChild(addFavBtn);
            }
            
            return card;
        }

        private Color GetRarityColor(EmoteRarity rarity) {
            return rarity switch {
                EmoteRarity.Common => Colors.Gray,
                EmoteRarity.Uncommon => Colors.Green,
                EmoteRarity.Rare => Colors.Blue,
                EmoteRarity.Epic => Colors.Purple,
                EmoteRarity.Legendary => Colors.Orange,
                _ => Colors.White
            };
        }

        private StyleBoxFlat CreateRarityStyle(Color color) {
            var style = new StyleBoxFlat();
            style.BgColor = new Color(color.R, color.G, color.B, 0.2f);
            style.BorderColor = color;
            style.BorderWidthBottom = 2;
            style.BorderWidthLeft = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthTop = 2;
            style.CornerRadiusBottomLeft = 5;
            style.CornerRadiusBottomRight = 5;
            style.CornerRadiusTopLeft = 5;
            style.CornerRadiusTopRight = 5;
            return style;
        }

        public override void _Input(InputEvent evt) {
            if (evt.IsActionPressed("ui_cancel")) {
                if (isVisible) {
                    Toggle();
                    GetTree().SetInputAsHandled();
                }
            }
            
            if (evt.IsActionPressed("emote_menu")) {
                Toggle();
                GetTree().SetInputAsHandled();
            }
        }
    }
}
