using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 宠物故事界面
    /// </summary>
    public class PetStoryUI : Control {
        private static PetStoryUI Instance;
        
        // UI Components
        private PanelContainer mainPanel;
        private VBoxContainer mainVBox;
        private Label titleLabel;
        private HBoxContainer petInfoContainer;
        private Label petNameLabel;
        private Label petTypeLabel;
        private Label storyCountLabel;
        private ScrollContainer storyListContainer;
        private VBoxContainer storyListVBox;
        private Button closeButton;
        
        // Story detail panel
        private PanelContainer detailPanel;
        private Label storyTitleLabel;
        private RichTextLabel storyBackstoryLabel;
        private RichTextLabel storyPersonalityLabel;
        private Label storyDialogueLabel;
        private Button backButton;
        
        // State
        private bool isVisible = false;
        private int currentPetId = -1;
        private int currentPetTypeId = -1;
        private List<PetStory> currentStories = new List<PetStory>();
        
        public override void _Ready() {
            Instance = this;
            SetupUI();
            Visible = false;
        }
        
        private void SetupUI() {
            // Main Panel
            mainPanel = new PanelContainer();
            mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            mainPanel.CustomMinimumSize = new Vector2(700, 500);
            AddChild(mainPanel);
            
            // Style
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(8);
            mainPanel.AddThemeStyleboxOverride("panel", style);
            
            // Main VBox
            mainVBox = new VBoxContainer();
            mainVBox.SetCustomMinimumSize(new Vector2(680, 480));
            mainVBox.AddThemeConstantOverride("separation", 10);
            mainPanel.AddChild(mainVBox);
            
            // Title
            titleLabel = new Label();
            titleLabel.Text = "  🐾 宠物背景故事  🐾";
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.6f));
            mainVBox.AddChild(titleLabel);
            
            // Pet Info Container
            petInfoContainer = new HBoxContainer();
            petInfoContainer.AddThemeConstantOverride("separation", 30);
            mainVBox.AddChild(petInfoContainer);
            
            petNameLabel = new Label();
            petNameLabel.Text = "宠物: -";
            petNameLabel.AddThemeFontSizeOverride("font_size", 18);
            petInfoContainer.AddChild(petNameLabel);
            
            petTypeLabel = new Label();
            petTypeLabel.Text = "类型: -";
            petTypeLabel.AddThemeFontSizeOverride("font_size", 18);
            petInfoContainer.AddChild(petTypeLabel);
            
            storyCountLabel = new Label();
            storyCountLabel.Text = "已解锁: 0/0";
            storyCountLabel.AddThemeFontSizeOverride("font_size", 18);
            storyCountLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.8f, 1f));
            petInfoContainer.AddChild(storyCountLabel);
            
            // Separator
            var hsep1 = new HSeparator();
            mainVBox.AddChild(hsep1);
            
            // Story List Container
            storyListContainer = new ScrollContainer();
            storyListContainer.SetCustomMinimumSize(new Vector2(660, 350));
            storyListContainer.HScrollEnabled = false;
            mainVBox.AddChild(storyListContainer);
            
            storyListVBox = new VBoxContainer();
            storyListVBox.AddThemeConstantOverride("separation", 8);
            storyListContainer.AddChild(storyListVBox);
            
            // Close Button
            closeButton = new Button();
            closeButton.Text = "  关闭  ";
            closeButton.Pressed += OnClosePressed;
            mainVBox.AddChild(closeButton);
            
            // Detail Panel (initially hidden)
            SetupDetailPanel();
        }
        
        private void SetupDetailPanel() {
            detailPanel = new PanelContainer();
            detailPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            detailPanel.CustomMinimumSize = new Vector2(700, 500);
            detailPanel.Visible = false;
            AddChild(detailPanel);
            
            var detailStyle = new StyleBoxFlat();
            detailStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            detailStyle.BorderColor = new Color(0.5f, 0.3f, 0.1f);
            detailStyle.SetBorderWidthAll(3);
            detailStyle.SetCornerRadiusAll(8);
            detailPanel.AddThemeStyleboxOverride("panel", detailStyle);
            
            var detailVBox = new VBoxContainer();
            detailVBox.SetCustomMinimumSize(new Vector2(680, 480));
            detailVBox.AddThemeConstantOverride("separation", 10);
            detailPanel.AddChild(detailVBox);
            
            // Story Title
            storyTitleLabel = new Label();
            storyTitleLabel.AddThemeFontSizeOverride("font_size", 22);
            storyTitleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.4f));
            storyTitleLabel.Align = Label.AlignEnum.Center;
            detailVBox.AddChild(storyTitleLabel);
            
            // Backstory
            var backstoryTitle = new Label();
            backstoryTitle.Text = "背景故事:";
            backstoryTitle.AddThemeFontSizeOverride("font_size", 16);
            backstoryTitle.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
            detailVBox.AddChild(backstoryTitle);
            
            storyBackstoryLabel = new RichTextLabel();
            storyBackstoryLabel.BbcodeEnabled = true;
            storyBackstoryLabel.SetCustomMinimumSize(new Vector2(660, 120));
            storyBackstoryLabel.AddThemeColorOverride("default_color", new Color(0.9f, 0.9f, 0.95f));
            detailVBox.AddChild(storyBackstoryLabel);
            
            // Personality
            var personalityTitle = new Label();
            personalityTitle.Text = "性格特点:";
            personalityTitle.AddThemeFontSizeOverride("font_size", 16);
            personalityTitle.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
            detailVBox.AddChild(personalityTitle);
            
            storyPersonalityLabel = new RichTextLabel();
            storyPersonalityLabel.BbcodeEnabled = true;
            storyPersonalityLabel.SetCustomMinimumSize(new Vector2(660, 80));
            storyPersonalityLabel.AddThemeColorOverride("default_color", new Color(0.8f, 0.9f, 0.8f));
            detailVBox.AddChild(storyPersonalityLabel);
            
            // Dialogue
            var dialogueTitle = Label.New();
            dialogueTitle.Text = "常用对话:";
            dialogueTitle.AddThemeFontSizeOverride("font_size", 16);
            dialogueTitle.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
            detailVBox.AddChild(dialogueTitle);
            
            storyDialogueLabel = new Label();
            storyDialogueLabel.SetCustomMinimumSize(new Vector2(660, 60));
            storyDialogueLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            detailVBox.AddChild(storyDialogueLabel);
            
            // Back Button
            backButton = new Button();
            backButton.Text = "  返回列表  ";
            backButton.Pressed += OnBackPressed;
            detailVBox.AddChild(backButton);
        }
        
        /// <summary>
        /// 显示宠物故事界面
        /// </summary>
        public void ShowPetStories(int petId, int petTypeId, string petName, string petType) {
            currentPetId = petId;
            currentPetTypeId = petTypeId;
            
            // Update info
            petNameLabel.Text = $"宠物: {petName}";
            petTypeLabel.Text = $"类型: {petType}";
            
            // Get stories
            if (PetStorySystem.Instance != null) {
                currentStories = PetStorySystem.Instance.GetPetStories(petId, petTypeId);
                
                int unlockedCount = 0;
                int totalCount = currentStories.Count;
                
                foreach (var story in currentStories) {
                    if (PetStorySystem.Instance != null) {
                        // Check read status
                        bool isRead = false;
                        if (PetStorySystem.Instance.HasUnreadStories(petId)) {
                            // Get read status from system
                        }
                    }
                }
                
                storyCountLabel.Text = $"已解锁: {PetStorySystem.Instance.GetUnlockedStoryCount(petId)}/{totalCount}";
            }
            
            // Populate list
            PopulateStoryList();
            
            Visible = true;
            isVisible = true;
            mainPanel.Visible = true;
            detailPanel.Visible = false;
        }
        
        private void PopulateStoryList() {
            // Clear existing
            foreach (var child in storyListVBox.GetChildren()) {
                child.QueueFree();
            }
            
            // Add story items
            foreach (var story in currentStories) {
                var item = CreateStoryItem(story);
                storyListVBox.AddChild(item);
            }
        }
        
        private Control CreateStoryItem(PetStory story) {
            var container = new PanelContainer();
            container.SetCustomMinimumSize(new Vector2(640, 70));
            
            var style = new StyleBoxFlat();
            if (story.IsUnlocked) {
                style.BgColor = new Color(0.15f, 0.15f, 0.2f);
                style.BorderColor = new Color(0.3f, 0.5f, 0.3f);
            } else {
                style.BgColor = new Color(0.1f, 0.1f, 0.12f);
                style.BorderColor = new Color(0.25f, 0.25f, 0.3f);
            }
            style.SetBorderWidthAll(1);
            style.SetCornerRadiusAll(4);
            container.AddThemeStyleboxOverride("panel", style);
            
            var hbox = new HBoxContainer();
            hbox.AddThemeConstantOverride("separation", 15);
            container.AddChild(hbox);
            
            // Lock/Unlock icon
            var iconLabel = new Label();
            iconLabel.Text = story.IsUnlocked ? "📖" : "🔒";
            iconLabel.AddThemeFontSizeOverride("font_size", 24);
            hbox.AddChild(iconLabel);
            
            // Story info
            var infoVBox = new VBoxContainer();
            infoVBox.AddThemeConstantOverride("separation", 2);
            hbox.AddChild(infoVBox);
            
            var titleText = new Label();
            titleText.Text = story.Title;
            titleText.AddThemeFontSizeOverride("font_size", 16);
            titleText.AddThemeColorOverride("font_color", story.IsUnlocked ? new Color(1f, 0.9f, 0.6f) : new Color(0.5f, 0.5f, 0.5f));
            infoVBox.AddChild(titleText);
            
            var descText = new Label();
            descText.Text = story.Description;
            descText.AddThemeFontSizeOverride("font_size", 13);
            descText.AddThemeColorOverride("font_color", story.IsUnlocked ? new Color(0.7f, 0.7f, 0.8f) : new Color(0.4f, 0.4f, 0.45f));
            infoVBox.AddChild(descText);
            
            // Unlock condition
            var conditionText = new Label();
            if (!story.IsUnlocked) {
                string condition = GetUnlockConditionText(story.UnlockCondition);
                conditionText.Text = $"解锁条件: {condition}";
                conditionText.AddThemeFontSizeOverride("font_size", 12);
                conditionText.AddThemeColorOverride("font_color", new Color(0.6f, 0.4f, 0.3f));
            } else {
                conditionText.Text = "✓ 已解锁";
                conditionText.AddThemeFontSizeOverride("font_size", 12);
                conditionText.AddThemeColorOverride("font_color", new Color(0.3f, 0.7f, 0.4f));
            }
            infoVBox.AddChild(conditionText);
            
            // View button (if unlocked)
            if (story.IsUnlocked) {
                var viewBtn = new Button();
                viewBtn.Text = "阅读";
                viewBtn.Pressed += () => OnViewStoryPressed(story);
                hbox.AddChild(viewBtn);
            }
            
            return container;
        }
        
        private string GetUnlockConditionText(PetStoryUnlockCondition condition) {
            switch (condition.Type) {
                case PetStoryUnlockType.AffectionLevel:
                    return $"亲密度达到 {condition.RequiredValue} 级";
                case PetStoryUnlockType.EvolutionStage:
                    string[] stages = { "", "基础", "高级", "精英", "史诗", "传奇" };
                    int stageIdx = Mathf.Min(condition.RequiredValue, stages.Length - 1);
                    return $"进化到 {stages[stageIdx]} 形态";
                case PetStoryUnlockType.BattleCount:
                    return $"参与 {condition.RequiredValue} 次战斗";
                case PetStoryUnlockType.ExpeditionSuccess:
                    return $"成功完成 {condition.RequiredValue} 次探险";
                case PetStoryUnlockType.BreedingCount:
                    return $"繁殖 {condition.RequiredValue} 次";
                default:
                    return condition.CustomCondition ?? "未知条件";
            }
        }
        
        private void OnViewStoryPressed(PetStory story) {
            ShowStoryDetail(story);
        }
        
        private void ShowStoryDetail(PetStory story) {
            storyTitleLabel.Text = story.Title;
            
            storyBackstoryLabel.Text = $"[color=#CCCCCC]{story.Backstory}[/color]";
            
            storyPersonalityLabel.Text = $"[color=#CCFFCC]{story.Personality}[/color]";
            
            string dialogues = "";
            for (int i = 0; i < story.DialogueLines.Length; i++) {
                dialogues += $"• {story.DialogueLines[i]}";
                if (i < story.DialogueLines.Length - 1) dialogues += "\n";
            }
            storyDialogueLabel.Text = dialogues;
            
            mainPanel.Visible = false;
            detailPanel.Visible = true;
            
            // Mark as read
            if (PetStorySystem.Instance != null && currentPetId >= 0) {
                PetStorySystem.Instance.MarkStoryAsRead(currentPetId, story.StoryId);
            }
        }
        
        private void OnBackPressed() {
            mainPanel.Visible = true;
            detailPanel.Visible = false;
            
            // Refresh list to show updated read status
            PopulateStoryList();
        }
        
        private void OnClosePressed() {
            Visible = false;
            isVisible = false;
        }
        
        /// <summary>
        /// 切换显示
        /// </summary>
        public static void Toggle(int petId, int petTypeId, string petName, string petType) {
            if (Instance != null) {
                if (Instance.isVisible && Instance.currentPetId == petId) {
                    Instance.Visible = false;
                    Instance.isVisible = false;
                } else {
                    Instance.ShowPetStories(petId, petTypeId, petName, petType);
                }
            }
        }
        
        public override void _Input(InputEvent e) {
            if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape) {
                if (isVisible) {
                    if (detailPanel.Visible) {
                        OnBackPressed();
                    } else {
                        Visible = false;
                        isVisible = false;
                    }
                }
            }
        }
    }
}
