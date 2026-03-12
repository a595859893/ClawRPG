using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI
{
    public class PetSkillTreeUI : Control
    {
        private static PetSkillTreeUI _instance;
        public static PetSkillTreeUI Instance => _instance;

        private Control panel;
        private TabContainer tabContainer;
        private Label titleLabel;
        private Label skillPointsLabel;
        private Button closeButton;
        
        private PetSkillTreeData.SkillTreeType currentTreeType = PetSkillTreeData.SkillTreeType.Offensive;
        private string currentPetId = "";
        
        public bool IsVisible => panel?.Visible ?? false;

        public override void _Ready()
        {
            _instance = this;
            SetupUI();
        }

        private void SetupUI()
        {
            panel = new Control();
            panel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(panel);
            
            var bg = new ColorRect();
            bg.Color = new Color(0, 0, 0, 0.7f);
            bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            panel.AddChild(bg);
            
            var mainPanel = new PanelContainer();
            mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            mainPanel.CustomMinimumSize = new Vector2(900, 600);
            panel.AddChild(mainPanel);
            
            var mainVBox = new VBoxContainer();
            mainPanel.AddChild(mainVBox);
            
            // Header
            var header = new HBoxContainer();
            mainVBox.AddChild(header);
            
            titleLabel = new Label();
            titleLabel.Text = "Pet Skill Tree";
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            header.AddChild(titleLabel);
            
            header.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
            
            skillPointsLabel = new Label();
            skillPointsLabel.Text = "Skill Points: 0";
            skillPointsLabel.AddThemeFontSizeOverride("font_size", 18);
            header.AddChild(skillPointsLabel);
            
            closeButton = new Button();
            closeButton.Text = "X";
            closeButton.CustomMinimumSize = new Vector2(40, 40);
            closeButton.Pressed += () => Hide();
            header.AddChild(closeButton);
            
            // Tab container
            tabContainer = new TabContainer();
            tabContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
            mainVBox.AddChild(tabContainer);
            
            // Create tabs for each tree type
            CreateTreeTab(PetSkillTreeData.SkillTreeType.Offensive, "Offensive");
            CreateTreeTab(PetSkillTreeData.SkillTreeType.Defensive, "Defensive");
            CreateTreeTab(PetSkillTreeData.SkillTreeType.Support, "Support");
            CreateTreeTab(PetSkillTreeData.SkillTreeType.Special, "Special");
            
            // Stats section
            var statsPanel = new HBoxContainer();
            mainVBox.AddChild(statsPanel);
            
            var statsLabel = new Label();
            statsLabel.Text = "Statistics: ";
            statsPanel.AddChild(statsLabel);
            
            UpdateStats();
            
            panel.Visible = false;
        }

        private void CreateTreeTab(PetSkillTreeData.SkillTreeType treeType, string tabName)
        {
            var scroll = new ScrollContainer();
            scroll.Name = tabName;
            tabContainer.AddChild(scroll);
            
            var grid = new GridContainer();
            grid.Columns = 3;
            grid.AddThemeConstantOverride("separation", 10);
            scroll.AddChild(grid);
            
            PopulateTreeGrid(grid, treeType);
        }

        private void PopulateTreeGrid(GridContainer grid, PetSkillTreeData.SkillTreeType treeType)
        {
            var system = PetSkillTreeSystem.Instance;
            var db = PetSkillTreeDatabase.Instance;
            
            var nodes = db.GetSkillTree("Fire", treeType); // Using Fire as default display
            
            foreach (var node in nodes)
            {
                var nodePanel = CreateNodePanel(node, currentPetId);
                grid.AddChild(nodePanel);
            }
        }

        private Control CreateNodePanel(PetSkillTreeData.SkillNode node, string petId)
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(180, 120);
            
            var vbox = new VBoxContainer();
            panel.AddChild(vbox);
            
            var nameLabel = new Label();
            nameLabel.Text = node.Name;
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            nameLabel.AddThemeFontSizeOverride("font_size", 14);
            vbox.AddChild(nameLabel);
            
            var descLabel = new Label();
            descLabel.Text = node.Description;
            descLabel.HorizontalAlignment = HorizontalAlignment.Center;
            descLabel.AutowrapMode = TextServer.AutowrapWord;
            vbox.AddChild(descLabel);
            
            var costLabel = new Label();
            costLabel.Text = $"Cost: {node.Cost}";
            costLabel.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(costLabel);
            
            if (node.IsUltimate)
            {
                var ultLabel = new Label();
                ultLabel.Text = "ULTIMATE";
                ultLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f, 1f));
                ultLabel.HorizontalAlignment = HorizontalAlignment.Center;
                vbox.AddChild(ultLabel);
            }
            
            // Color based on status
            var status = PetSkillTreeSystem.Instance.GetNodeStatus(petId, node.NodeId);
            switch (status)
            {
                case PetSkillTreeData.SkillNodeStatus.Unlocked:
                    panel.AddThemeColorOverride("border_color", new Color(0f, 1f, 0f, 1f));
                    break;
                case PetSkillTreeData.SkillNodeStatus.Available:
                    panel.AddThemeColorOverride("border_color", new Color(0f, 0.5f, 1f, 1f));
                    break;
                case PetSkillTreeData.SkillNodeStatus.Locked:
                    panel.AddThemeColorOverride("border_color", new Color(0.3f, 0.3f, 0.3f, 1f));
                    break;
            }
            
            return panel;
        }

        public void Show(string petId = "")
        {
            currentPetId = petId;
            if (!string.IsNullOrEmpty(petId))
            {
                PetSkillTreeSystem.Instance.InitializePetSkillTree(petId, "Fire");
            }
            
            UpdateSkillPoints();
            UpdateStats();
            panel.Visible = true;
        }

        public void Hide()
        {
            panel.Visible = false;
        }

        public void Toggle()
        {
            if (IsVisible)
                Hide();
            else
                Show(currentPetId);
        }

        private void UpdateSkillPoints()
        {
            var points = PetSkillTreeSystem.Instance.GetAvailableSkillPoints(currentPetId);
            skillPointsLabel.Text = $"Skill Points: {points}";
        }

        private void UpdateStats()
        {
            var stats = PetSkillTreeSystem.Instance.GetStatistics();
            GD.Print($"Pet Skill Tree Stats: {stats["nodes_unlocked"]} nodes unlocked, {stats["ultimates_unlocked"]} ultimates");
        }

        public override void _Input(InputEvent e)
        {
            if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
            {
                if (IsVisible)
                    Hide();
            }
        }
    }
}
