using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI {
    public partial class StoryUI : Control {
        private VBoxContainer mainContainer;
        private VBoxContainer chapterList;
        private Label titleLabel;
        private Label currentChapterLabel;
        private TextureRect chapterIcon;
        
        private Button closeButton;
        private ScrollContainer scrollContainer;
        
        private Color unlockedColor = new Color(1f, 0.84f, 0f); // Gold
        private Color completedColor = new Color(0.2f, 0.8f, 0.2f); // Green
        private Color lockedColor = new Color(0.5f, 0.5f, 0.5f); // Gray
        private Color inProgressColor = new Color(0.3f, 0.6f, 1f); // Blue
        
        public override void _Ready() {
            Visible = false; 
            _InitializeUI();
            
            // Connect to story manager signals
            if (StoryManager.Instance != null) {
                StoryManager.Instance.ChapterUnlocked += OnChapterUnlocked;
                StoryManager.Instance.ChapterCompleted += OnChapterCompleted;
                StoryManager.Instance.ObjectiveProgressUpdated += OnObjectiveProgressUpdated;
            }
        }
        
        private void _InitializeUI() {
            // Main panel
            var panel = new Panel {
                AnchorRight = 0.6f,
                AnchorBottom = 0.8f,
                AnchorLeft = 0.2f,
                AnchorTop = 0.1f,
                SelfModulate = new Color(0.1f, 0.1f, 0.15f, 0.95f)
            };
            AddChild(panel);
            
            var panelStyle = new StyleBoxFlat {
                BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f),
                BorderColor = new Color(0.3f, 0.3f, 0.4f, 1f),
                BorderWidthLeft = 2,
                BorderWidthRight = 2,
                BorderWidthTop = 2,
                BorderWidthBottom = 2,
                CornerRadiusTopLeft = 10,
                CornerRadiusTopRight = 10,
                CornerRadiusBottomLeft = 10,
                CornerRadiusBottomRight = 10
            };
            panel.AddStyleboxOverride("panel", panelStyle);
            
            // Title
            titleLabel = new Label {
                Text = "📖 故事章节",
                AnchorTop = 0.02f,
                AnchorLeft = 0.02f,
                AnchorRight = 0.98f,
                Align = Label.AlignEnum.Center,
                SelfModulate = new Color(1f, 0.9f, 0.5f, 1f)
            };
            titleLabel.AddColorOverride("font_color", new Color(1f, 0.9f, 0.5f, 1f));
            panel.AddChild(titleLabel);
            
            // Close button
            closeButton = new Button {
                Text = "✕",
                AnchorTop = 0.02f,
                AnchorLeft = 0.9f,
                AnchorRight = 0.98f,
                AnchorBottom = 0.08f
            };
            closeButton.Pressed += OnClosePressed;
            panel.AddChild(closeButton);
            
            // Current chapter display
            currentChapterLabel = new Label {
                Text = "当前章节: ",
                AnchorTop = 0.08f,
                AnchorLeft = 0.05f,
                AnchorRight = 0.95f,
                Align = Label.AlignEnum.Center
            };
            currentChapterLabel.AddColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f, 1f));
            panel.AddChild(currentChapterLabel);
            
            // Chapter list scroll container
            scrollContainer = new ScrollContainer {
                AnchorTop = 0.14f,
                AnchorLeft = 0.03f,
                AnchorRight = 0.97f,
                AnchorBottom = 0.95f
            };
            panel.AddChild(scrollContainer);
            
            chapterList = new VBoxContainer {
                CustomMinimumSize = new Vector2(0, 400)
            };
            scrollContainer.AddChild(chapterList);
            
            RefreshChapterList();
        }
        
        public void RefreshChapterList() {
            // Clear existing
            foreach (Node child in chapterList.GetChildren()) {
                child.QueueFree();
            }
            
            var chapters = StoryManager.Instance?.GetAllChapters();
            if (chapters == null) return;
            
            var currentChapter = StoryManager.Instance?.GetCurrentChapter();
            int currentId = currentChapter?.ChapterId ?? 1;
            
            currentChapterLabel.Text = $"当前章节: 第{currentId}章 - {(currentChapter?.Title ?? "无")}";
            
            foreach (var chapter in chapters) {
                var chapterPanel = _CreateChapterPanel(chapter, chapter.ChapterId == currentId);
                chapterList.AddChild(chapterPanel);
            }
        }
        
        private Control _CreateChapterPanel(StoryChapter chapter, bool isCurrent) {
            var panel = new PanelContainer {
                CustomMinimumSize = new Vector2(0, 120),
                MarginBottom = 10
            };
            
            var style = new StyleBoxFlat {
                BgColor = isCurrent ? new Color(0.2f, 0.25f, 0.35f, 1f) : new Color(0.15f, 0.15f, 0.2f, 1f),
                BorderWidthLeft = 2,
                BorderWidthRight = 2,
                BorderWidthTop = 2,
                BorderWidthBottom = 2,
                BorderColor = isCurrent ? inProgressColor : (chapter.IsCompleted ? completedColor : (chapter.IsUnlocked ? unlockedColor : lockedColor)),
                CornerRadiusTopLeft = 8,
                CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8,
                CornerRadiusBottomRight = 8
            };
            panel.AddStyleboxOverride("panel", style);
            
            var vbox = new VBoxContainer {
                MarginLeft = 10,
                MarginRight = 10,
                MarginTop = 8,
                MarginBottom = 8
            };
            panel.AddChild(vbox);
            
            // Chapter header
            var header = new HBoxContainer();
            vbox.AddChild(header);
            
            var chapterNum = new Label {
                Text = $"第{chapter.ChapterId}章",
                SelfModulate = chapter.IsCompleted ? completedColor : (chapter.IsUnlocked ? unlockedColor : lockedColor)
            };
            chapterNum.AddColorOverride("font_color", chapter.IsCompleted ? completedColor : (chapter.IsUnlocked ? unlockedColor : lockedColor));
            header.AddChild(chapterNum);
            
            var title = new Label {
                Text = $" {chapter.Title}",
                SelfModulate = chapter.IsCompleted ? completedColor : (chapter.IsUnlocked ? Colors.White : lockedColor)
            };
            title.AddColorOverride("font_color", chapter.IsCompleted ? completedColor : (chapter.IsUnlocked ? Colors.White : lockedColor));
            header.AddChild(title);
            
            var statusLabel = new Label {
                Text = chapter.IsCompleted ? " ✓ 已完成" : (isCurrent ? " 进行中" : " 🔒 未解锁"),
                Align = Label.AlignEnum.Right,
                SizeFlagsHorizontal = SizeFlags.Expand
            };
            statusLabel.AddColorOverride("font_color", chapter.IsCompleted ? completedColor : (isCurrent ? inProgressColor : lockedColor));
            header.AddChild(statusLabel);
            
            // Description
            var desc = new Label {
                Text = chapter.Description,
                SelfModulate = new Color(0.7f, 0.7f, 0.8f, 1f)
            };
            desc.AddColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f, 1f));
            desc.Autowrap = true;
            vbox.AddChild(desc);
            
            // Objectives (only show for current/unlocked chapters)
            if (chapter.IsUnlocked && chapter.Objectives.Count > 0) {
                var objHeader = new Label {
                    Text = "目标:",
                    SelfModulate = new Color(0.6f, 0.6f, 0.7f, 1f)
                };
                objHeader.AddColorOverride("font_color", new Color(0.6f, 0.6f, 0.7f, 1f));
                vbox.AddChild(objHeader);
                
                foreach (var obj in chapter.Objectives) {
                    var objText = $"  • {obj.Description}";
                    if (obj.Type != StoryObjectiveType.ReachLevel) {
                        objText += $" ({obj.CurrentCount}/{obj.TargetCount})";
                    }
                    
                    var objLabel = new Label {
                        Text = objText,
                        SelfModulate = obj.CurrentCount >= obj.TargetCount ? completedColor : new Color(0.8f, 0.8f, 0.85f, 1f)
                    };
                    objLabel.AddColorOverride("font_color", obj.CurrentCount >= obj.TargetCount ? completedColor : new Color(0.8f, 0.8f, 0.85f, 1f));
                    vbox.AddChild(objLabel);
                }
                
                // Rewards
                if (chapter.Rewards.Count > 0) {
                    var rewardText = "奖励: ";
                    foreach (var reward in chapter.Rewards) {
                        switch (reward.Type) {
                            case RewardType.Gold:
                                rewardText += $"💰{reward.Amount} ";
                                break;
                            case RewardType.Experience:
                                rewardText += $"✨{reward.Amount} ";
                                break;
                            case RewardType.SkillPoints:
                                rewardText += $"⭐{reward.Amount} ";
                                break;
                        }
                    }
                    
                    var rewardLabel = new Label {
                        Text = rewardText,
                        SelfModulate = new Color(1f, 0.85f, 0.4f, 1f)
                    };
                    rewardLabel.AddColorOverride("font_color", new Color(1f, 0.85f, 0.4f, 1f));
                    vbox.AddChild(rewardLabel);
                }
            }
            
            return panel;
        }
        
        private void OnClosePressed() {
            Visible = false; 
        }
        
        private void OnChapterUnlocked(StoryChapter chapter) {
            RefreshChapterList();
        }
        
        private void OnChapterCompleted(StoryChapter chapter) {
            RefreshChapterList();
        }
        
        private void OnObjectiveProgressUpdated(StoryObjective objective) {
            RefreshChapterList();
        }
        
        public override void _Input(InputEvent @event) {
            if (@event.IsActionPressed("ui_cancel")) {
                if (Visible) {
                    Visible = false; 
                    GetTree().SetInputAsHandled();
                }
            }
        }
    }
}
