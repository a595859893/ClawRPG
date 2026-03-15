using Godot;
using System;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// 神器UI - 显示神器界面
    /// </summary>
    public class ArtifactUI : Control
    {
        private Control container;
        private VBoxContainer mainVBox;
        private TabContainer tabContainer;
        
        // UI Elements
        private Label titleLabel;
        private ItemList artifactList;
        private ItemList equippedList;
        private Label artifactInfoLabel;
        private Label statsLabel;
        
        private string selectedArtifactId = null;
        private bool visible = false;

        public override void _Ready()
        {
            SetupUI();
            ConnectSignals();
            RefreshLists();
            
            Visible = false;
        }

        private void SetupUI()
        {
            // Main container
            container = new Control();
            container.SetAnchor(AnchorPreset.FullRect);
            AddChild(container);

            // Background panel
            Panel bgPanel = new Panel();
            bgPanel.SetAnchor(AnchorPreset.FullRect);
            bgPanel.Modulate = new Color(0, 0, 0, 0.85f);
            container.AddChild(bgPanel);

            // Main VBox
            mainVBox = new VBoxContainer();
            mainVBox.SetAnchor(AnchorPreset.FullRect);
            mainVBox.AddThemeConstantOverride("separation", 20);
            container.AddChild(mainVBox);

            // Title
            titleLabel = new Label();
            titleLabel.Text = "   ✦ 神器系统 ✦";
            titleLabel.AddThemeFontSizeOverride("font_size", 28);
            titleLabel.AddThemeColorOverride("font_color", new Color(1, 0.84f, 0));
            mainVBox.AddChild(titleLabel);

            // Tab container
            tabContainer = new TabContainer();
            tabContainer.SetSize(new Vector2(900, 550));
            tabContainer.Position = new Vector2(50, 80);
            container.AddChild(tabContainer);

            // Create tabs
            CreateCollectionTab();
            CreateEquippedTab();
            CreateStatisticsTab();
            CreateSetsTab();
        }

        private void CreateCollectionTab()
        {
            VBoxContainer tab = new VBoxContainer();
            tab.Name = "收藏";
            tab.AddThemeConstantOverride("separation", 10);
            tabContainer.AddChild(tab);

            HBoxContainer hbox = new HBoxContainer();
            hbox.AddThemeConstantOverride("separation", 20);
            tab.AddChild(hbox);

            // Artifact list
            VBoxContainer leftBox = new VBoxContainer();
            leftBox.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            hbox.AddChild(leftBox);

            Label listLabel = new Label();
            listLabel.Text = "神器列表:";
            listLabel.AddThemeFontSizeOverride("font_size", 18);
            leftBox.AddChild(listLabel);

            artifactList = new ItemList();
            artifactList.Size = new Vector2(350, 400);
            artifactList.ItemSelected += OnArtifactSelected;
            leftBox.AddChild(artifactList);

            // Info panel
            VBoxContainer rightBox = new VBoxContainer();
            rightBox.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            hbox.AddChild(rightBox);

            Label infoLabel = new Label();
            infoLabel.Text = "神器详情:";
            infoLabel.AddThemeFontSizeOverride("font_size", 18);
            rightBox.AddChild(infoLabel);

            artifactInfoLabel = new Label();
            artifactInfoLabel.Text = "选择一个神器查看详情";
            artifactInfoLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            artifactInfoLabel.Size = new Vector2(400, 400);
            rightBox.AddChild(artifactInfoLabel);
        }

        private void CreateEquippedTab()
        {
            VBoxContainer tab = new VBoxContainer();
            tab.Name = "已装备";
            tab.AddThemeConstantOverride("separation", 10);
            tabContainer.AddChild(tab);

            HBoxContainer hbox = new HBoxContainer();
            hbox.AddThemeConstantOverride("separation", 20);
            tab.AddChild(hbox);

            // Equipped list
            VBoxContainer leftBox = new VBoxContainer();
            leftBox.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            hbox.AddChild(leftBox);

            Label listLabel = new Label();
            listLabel.Text = "已装备神器:";
            listLabel.AddThemeFontSizeOverride("font_size", 18);
            leftBox.AddChild(listLabel);

            equippedList = new ItemList();
            equippedList.Size = new Vector2(350, 400);
            equippedList.ItemSelected += OnEquippedSelected;
            leftBox.AddChild(equippedList);

            // Action buttons
            VBoxContainer rightBox = new VBoxContainer();
            rightBox.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            hbox.AddChild(rightBox);

            Label actionLabel = new Label();
            actionLabel.Text = "操作:";
            actionLabel.AddThemeFontSizeOverride("font_size", 18);
            rightBox.AddChild(actionLabel);

            Button equipBtn = new Button();
            equipBtn.Text = "装备神器";
            equipBtn.Pressed += OnEquipPressed;
            equipBtn.CustomMinimumSize = new Vector2(200, 50);
            rightBox.AddChild(equipBtn);

            Button unequipBtn = new Button();
            unequipBtn.Text = "卸下神器";
            unequipBtn.Pressed += OnUnequipPressed;
            unequipBtn.CustomMinimumSize = new Vector2(200, 50);
            rightBox.AddChild(unequipBtn);

            Button generateBtn = new Button();
            generateBtn.Text = "随机获取神器";
            generateBtn.Pressed += OnGeneratePressed;
            generateBtn.CustomMinimumSize = new Vector2(200, 50);
            rightBox.AddChild(generateBtn);
        }

        private void CreateStatisticsTab()
        {
            VBoxContainer tab = new VBoxContainer();
            tab.Name = "统计";
            tab.AddThemeConstantOverride("separation", 15);
            tabContainer.AddChild(tab);

            Label statsTitle = new Label();
            statsTitle.Text = "收集统计";
            statsTitle.AddThemeFontSizeOverride("font_size", 22);
            statsTitle.AddThemeColorOverride("font_color", new Color(1, 0.84f, 0));
            tab.AddChild(statsTitle);

            statsLabel = new Label();
            statsLabel.Text = "正在加载统计...";
            statsLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            tab.AddChild(statsLabel);

            // Progress bar
            Label progressTitle = new Label();
            progressTitle.Text = "收集进度:";
            progressTitle.AddThemeFontSizeOverride("font_size", 18);
            tab.AddChild(progressTitle);

            ProgressBar progressBar = new ProgressBar();
            progressBar.CustomMinimumSize = new Vector2(600, 30);
            tab.AddChild(progressBar);
        }

        private void CreateSetsTab()
        {
            VBoxContainer tab = new VBoxContainer();
            tab.Name = "套装";
            tab.AddThemeConstantOverride("separation", 10);
            tabContainer.AddChild(tab);

            Label setsTitle = new Label();
            setsTitle.Text = "神器套装";
            setsTitle.AddThemeFontSizeOverride("font_size", 22);
            setsTitle.AddThemeColorOverride("font_color", new Color(1, 0.84f, 0));
            tab.AddChild(setsTitle);

            Label descLabel = new Label();
            descLabel.Text = "收集同一套装的多个神器可获得额外加成";
            descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            tab.AddChild(descLabel);

            ScrollContainer scroll = new ScrollContainer();
            scroll.CustomMinimumSize = new Vector2(800, 400);
            tab.AddChild(scroll);

            VBoxContainer setList = new VBoxContainer();
            setList.AddThemeConstantOverride("separation", 10);
            scroll.AddChild(setList);

            // Add set info
            var sets = ArtifactDatabase.GetAllSets();
            foreach (var set in sets)
            {
                HBoxContainer setBox = new HBoxContainer();
                setBox.AddThemeConstantOverride("separation", 20);
                setList.AddChild(setBox);

                Label setName = new Label();
                setName.Text = $"■ {set.Key.ToUpper()}";
                setName.AddThemeFontSizeOverride("font_size", 16);
                setName.AddThemeColorOverride("font_color", new Color(0.8f, 0.6f, 0.2f));
                setBox.AddChild(setName);

                Label setCount = new Label();
                setCount.Text = $"{set.Value.Count} 件神器";
                setBox.AddChild(setCount);
            }
        }

        private void ConnectSignals()
        {
            // Connect to artifact system signals
            var artifactSystem = GetNode<ArtifactSystem>("/root/ArtifactSystem");
            if (artifactSystem != null)
            {
                artifactSystem.Connect(ArtifactSystem.SignalArtifactUnlocked, this, nameof(OnArtifactUnlocked));
                artifactSystem.Connect(ArtifactSystem.SignalArtifactEquipped, this, nameof(OnArtifactEquipped));
                artifactSystem.Connect(ArtifactSystem.SignalArtifactUnequipped, this, nameof(OnArtifactUnequipped));
            }

            // Input
            var input = GetTree().Root.GetNode<Control>("Main");
            if (input != null)
            {
                input.Connect("gui_input", this, nameof(OnGuiInput));
            }
        }

        public override void _Input(InputEvent e)
        {
            if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
            {
                ToggleVisible();
            }
        }

        public void ToggleVisible()
        {
            visible = !visible;
            Visible = visible;
            
            if (visible)
            {
                RefreshLists();
            }
        }

        private void RefreshLists()
        {
            // Refresh collection list
            artifactList.Clear();
            var unlocked = ArtifactSystem.Instance.GetUnlockedArtifacts();
            
            foreach (var artifact in unlocked)
            {
                string text = $"[{GetRarityIcon(artifact.Rarity)}] {artifact.Name}";
                artifactList.AddItem(text);
            }

            // Refresh equipped list
            equippedList.Clear();
            var equipped = ArtifactSystem.Instance.GetEquippedArtifacts();
            foreach (var artifact in equipped)
            {
                string text = $"[{GetRarityIcon(artifact.Rarity)}] {artifact.Name}";
                equippedList.AddItem(text);
            }

            // Refresh statistics
            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            var stats = ArtifactSystem.Instance.GetStatistics();
            float progress = ArtifactSystem.Instance.GetCollectionProgress();
            
            string statsText = $"已解锁神器: {stats["total_unlocked"]}\n";
            statsText += $"已装备神器: {stats["total_equipped"]}\n\n";
            statsText += $"📦 稀有度统计:\n";
            statsText += $"  普通: {stats["common_count"]}\n";
            statsText += $"  优秀: {stats["uncommon_count"]}\n";
            statsText += $"  稀有: {stats["rare_count"]}\n";
            statsText += $"  史诗: {stats["epic_count"]}\n";
            statsText += $"  传说: {stats["legendary_count"]}\n";
            statsText += $"  神器: {stats["mythical_count"]}\n\n";
            statsText += $"⚔️ 类型统计:\n";
            statsText += $"  武器: {stats["weapon_count"]}\n";
            statsText += $"  护甲: {stats["armor_count"]}\n";
            statsText += $"  饰品: {stats["accessory_count"]}\n";
            statsText += $"  遗物: {stats["relic_count"]}\n";

            statsLabel.Text = statsText;

            // Update progress bar in statistics tab
            var tabs = tabContainer.GetTabContainer();
            if (tabs != null)
            {
                foreach (Node child in tabs.GetChildren())
                {
                    if (child.Name == "统计")
                    {
                        foreach (Node tabChild in child.GetChildren())
                        {
                            if (tabChild is ProgressBar pb)
                            {
                                pb.Value = progress * 100;
                                pb.CustomMinimumSize = new Vector2(600, 30);
                            }
                        }
                    }
                }
            }
        }

        private string GetRarityIcon(ArtifactRarity rarity)
        {
            switch (rarity)
            {
                case ArtifactRarity.Common: return "⚪";
                case ArtifactRarity.Uncommon: return "🟢";
                case ArtifactRarity.Rare: return "🔵";
                case ArtifactRarity.Epic: return "🟣";
                case ArtifactRarity.Legendary: return "🟠";
                case ArtifactRarity.Mythical: return "🔴";
                default: return "⚪";
            }
        }

        private void OnArtifactSelected(int index)
        {
            var unlocked = ArtifactSystem.Instance.GetUnlockedArtifacts();
            if (index >= 0 && index < unlocked.Count)
            {
                var artifact = unlocked[index];
                selectedArtifactId = artifact.Id;
                ShowArtifactInfo(artifact);
            }
        }

        private void OnEquippedSelected(int index)
        {
            // Similar logic for equipped
        }

        private void ShowArtifactInfo(Artifact artifact)
        {
            string color = ArtifactDatabase.GetRarityColor(artifact.Rarity);
            string info = $"[{artifact.Name}]\n";
            info += $"稀有度: {artifact.Rarity}\n";
            info += $"类型: {artifact.Type}\n\n";
            
            info += $"📜 描述:\n{artifact.Description}\n\n";
            
            if (!string.IsNullOrEmpty(artifact.Lore))
            {
                info += $"📖 背景:\n{artifact.Lore}\n\n";
            }
            
            info += $"✨ 效果:\n";
            foreach (var effect in artifact.Effects)
            {
                info += $"  • {effect.Description}\n";
            }

            if (!string.IsNullOrEmpty(artifact.Origin))
            {
                info += $"\n📍 来源: {artifact.Origin}";
            }

            artifactInfoLabel.Text = info;
        }

        private void OnEquipPressed()
        {
            if (!string.IsNullOrEmpty(selectedArtifactId))
            {
                ArtifactSystem.Instance.EquipArtifact(selectedArtifactId);
                RefreshLists();
            }
        }

        private void OnUnequipPressed()
        {
            int selected = equippedList.GetSelectedItems()[0];
            var equipped = ArtifactSystem.Instance.GetEquippedArtifacts();
            if (selected >= 0 && selected < equipped.Count)
            {
                ArtifactSystem.Instance.UnequipArtifact(equipped[selected].Id);
                RefreshLists();
            }
        }

        private void OnGeneratePressed()
        {
            string newId = ArtifactSystem.Instance.GenerateRandomArtifact(50);
            if (!string.IsNullOrEmpty(newId))
            {
                var artifact = ArtifactDatabase.GetArtifact(newId);
                GD.Print($"获得了新神器: {artifact.Name} ({artifact.Rarity})");
                RefreshLists();
            }
        }

        private void OnArtifactUnlocked(string artifactId)
        {
            RefreshLists();
        }

        private void OnArtifactEquipped(string artifactId)
        {
            RefreshLists();
        }

        private void OnArtifactUnequipped(string artifactId)
        {
            RefreshLists();
        }

        private void OnGuiInput(InputEvent e)
        {
            if (e is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.K)
                {
                    ToggleVisible();
                }
            }
        }
    }
}
