using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// NPC关系UI - 显示和管理玩家与NPC的关系
    /// </summary>
    public partial class NPCRelationshipUI : Control
    {
        private Label _titleLabel;
        private VBoxContainer _npcListContainer;
        private PanelContainer _detailPanel;
        private Label _detailNameLabel;
        private Label _detailLevelLabel;
        private Label _detailFavorLabel;
        private Label _detailDescriptionLabel;
        private ProgressBar _favorProgressBar;
        private Label _statsLabel;
        
        private string _selectedNPCId = "";
        
        // 颜色配置
        private Color _strangerColor = new Color(0.6f, 0.6f, 0.6f);
        private Color _acquaintanceColor = new Color(0.7f, 0.7f, 0.5f);
        private Color _friendColor = new Color(0.5f, 0.8f, 0.5f);
        private Color _closeFriendColor = new Color(0.4f, 0.9f, 0.6f);
        private Color _bestFriendColor = new Color(0.6f, 0.5f, 0.9f);
        private Color _soulmateColor = new Color(1.0f, 0.6f, 0.8f);
        
        public override void _Ready()
        {
            SetupUI();
            RefreshNPCList();
            
            // 初始化NPC数据
            var db = new NPCRelationshipDatabase();
            var npcIds = db.GetAllNPCIds();
            NPCRelationshipSystem.Instance.InitializeNPCs(npcIds);
            
            // 监听关系变化
            NPCRelationshipSystem.Instance.OnRelationshipChanged += OnRelationshipChanged;
        }

        private void SetupUI()
        {
            // 主容器
            var mainContainer = new HBoxContainer();
            mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            mainContainer.AddThemeConstantOverride("separation", 20);
            AddChild(mainContainer);
            
            // 左侧NPC列表
            var listPanel = new PanelContainer();
            listPanel.SetCustomMinimumSize(new Vector2(300, 0));
            mainContainer.AddChild(listPanel);
            
            var listVBox = new VBoxContainer();
            listVBox.AddThemeConstantOverride("separation", 10);
            listPanel.AddChild(listVBox);
            
            // 标题
            _titleLabel = new Label();
            _titleLabel.Text = "NPC 关系";
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));
            listVBox.AddChild(_titleLabel);
            
            // NPC列表容器
            _npcListContainer = new VBoxContainer();
            _npcListContainer.AddThemeConstantOverride("separation", 8);
            listVBox.AddChild(_npcListContainer);
            
            // 右侧详情面板
            _detailPanel = new PanelContainer();
            _detailPanel.SetCustomMinimumSize(new Vector2(400, 0));
            mainContainer.AddChild(_detailPanel);
            
            var detailVBox = new VBoxContainer();
            detailVBox.AddThemeConstantOverride("separation", 15);
            _detailPanel.AddChild(detailVBox);
            
            // NPC名称
            _detailNameLabel = new Label();
            _detailNameLabel.AddThemeFontSizeOverride("font_size", 28);
            _detailNameLabel.AddThemeColorOverride("font_color", new Color(1f, 0.95f, 0.8f));
            detailVBox.AddChild(_detailNameLabel);
            
            // 等级
            _detailLevelLabel = new Label();
            _detailLevelLabel.AddThemeFontSizeOverride("font_size", 20);
            detailVBox.AddChild(_detailLevelLabel);
            
            // 好感度进度条
            var progressLabel = new Label();
            progressLabel.Text = "好感度进度";
            progressLabel.AddThemeFontSizeOverride("font_size", 16);
            detailVBox.AddChild(progressLabel);
            
            _favorProgressBar = new ProgressBar();
            _favorProgressBar.SetCustomMinimumSize(new Vector2(0, 30));
            _favorProgressBar.PercentVisible = true;
            detailVBox.AddChild(_favorProgressBar);
            
            // 好感度数值
            _detailFavorLabel = new Label();
            _detailFavorLabel.AddThemeFontSizeOverride("font_size", 18);
            detailVBox.AddChild(_detailFavorLabel);
            
            // 描述
            _detailDescriptionLabel = new Label();
            _detailDescriptionLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _detailDescriptionLabel.AddThemeFontSizeOverride("font_size", 14);
            detailVBox.AddChild(_detailDescriptionLabel);
            
            // 统计信息
            var statsTitle = new Label();
            statsTitle.Text = "互动统计";
            statsTitle.AddThemeFontSizeOverride("font_size", 18);
            statsTitle.AddThemeColorOverride("font_color", new Color(0.8f, 0.9f, 1f));
            detailVBox.AddChild(statsTitle);
            
            _statsLabel = new Label();
            _statsLabel.AddThemeFontSizeOverride("font_size", 14);
            _statsLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            detailVBox.AddChild(_statsLabel);
            
            // 商店折扣信息
            var discountTitle = new Label();
            discountTitle.Text = "商店折扣";
            discountTitle.AddThemeFontSizeOverride("font_size", 18);
            discountTitle.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.6f));
            detailVBox.AddChild(discountTitle);
            
            var discountLabel = new Label();
            discountLabel.Name = "DiscountLabel";
            discountLabel.AddThemeFontSizeOverride("font_size", 16);
            detailVBox.AddChild(discountLabel);
            
            // 关闭按钮
            var closeButton = new Button();
            closeButton.Text = "关闭 (ESC)";
            closeButton.Pressed += () => Hide();
            detailVBox.AddChild(closeButton);
        }

        private void RefreshNPCList()
        {
            // 清除现有列表
            foreach (var child in _npcListContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            var db = NPCRelationshipDatabase.Instance;
            var npcIds = db.GetAllNPCIds();
            
            foreach (var npcId in npcIds)
            {
                var npcData = db.GetNPCData(npcId);
                var relationship = NPCRelationshipSystem.Instance.GetRelationship(npcId);
                var level = NPCRelationshipSystem.Instance.GetRelationshipLevel(npcId);
                
                var npcButton = CreateNPCButton(npcId, npcData.DisplayName, level, relationship?.Favor ?? 0);
                _npcListContainer.AddChild(npcButton);
            }
        }

        private Button CreateNPCButton(string npcId, string name, NPCRelationshipSystem.RelationshipLevel level, int favor)
        {
            var button = new Button();
            button.Text = $"{name} - {NPCRelationshipSystem.Instance.GetLevelName(level)} ({favor})";
            button.Pressed += () => SelectNPC(npcId);
            
            // 根据等级设置颜色
            Color levelColor = GetLevelColor(level);
            button.AddThemeColorOverride("font_color", levelColor);
            
            return button;
        }

        private void SelectNPC(string npcId)
        {
            _selectedNPCId = npcId;
            
            var db = NPCRelationshipDatabase.Instance;
            var npcData = db.GetNPCData(npcId);
            var relationship = NPCRelationshipSystem.Instance.GetRelationship(npcId);
            var level = NPCRelationshipSystem.Instance.GetRelationshipLevel(npcId);
            
            // 更新详情面板
            _detailNameLabel.Text = npcData.DisplayName;
            
            string levelName = NPCRelationshipSystem.Instance.GetLevelName(level);
            _detailLevelLabel.Text = $"关系等级: {levelName}";
            _detailLevelLabel.AddThemeColorOverride("font_color", GetLevelColor(level));
            
            int favor = relationship?.Favor ?? 0;
            _detailFavorLabel.Text = $"好感度: {favor}";
            
            // 进度条
            float progress = NPCRelationshipSystem.Instance.GetFavorProgress(npcId);
            _favorProgressBar.Value = progress * 100;
            _favorProgressBar.AddThemeColorOverride("font_color", GetLevelColor(level));
            
            // 描述
            _detailDescriptionLabel.Text = npcData.Description;
            
            // 统计
            string stats = $"送礼次数: {relationship?.TotalGiftsGiven ?? 0}\n";
            stats += $"对话次数: {relationship?.ConversationsHad ?? 0}\n";
            stats += $"完成任务: {relationship?.QuestsCompleted ?? 0}";
            _statsLabel.Text = stats;
            
            // 商店折扣
            float discount = NPCRelationshipSystem.Instance.GetShopDiscount(npcId);
            int discountPercent = (int)((1 - discount) * 100);
            var discountLabel = _detailPanel.GetNode<Label>("DiscountLabel");
            if (discountLabel != null)
            {
                if (discountPercent > 0)
                    discountLabel.Text = $"购买商品享受 {discountPercent}% 折扣";
                else
                    discountLabel.Text = "暂无折扣";
                discountLabel.AddThemeColorOverride("font_color", discountPercent > 0 ? new Color(1f, 0.8f, 0.4f) : new Color(0.6f, 0.6f, 0.6f));
            }
        }

        private Color GetLevelColor(NPCRelationshipSystem.RelationshipLevel level)
        {
            switch (level)
            {
                case NPCRelationshipSystem.RelationshipLevel.Stranger: return _strangerColor;
                case NPCRelationshipSystem.RelationshipLevel.Acquaintance: return _acquaintanceColor;
                case NPCRelationshipSystem.RelationshipLevel.Friend: return _friendColor;
                case NPCRelationshipSystem.RelationshipLevel.CloseFriend: return _closeFriendColor;
                case NPCRelationshipSystem.RelationshipLevel.BestFriend: return _bestFriendColor;
                case NPCRelationshipSystem.RelationshipLevel.Soulmate: return _soulmateColor;
                default: return _strangerColor;
            }
        }

        private void OnRelationshipChanged(string npcId, NPCRelationshipSystem.RelationshipLevel oldLevel, NPCRelationshipSystem.RelationshipLevel newLevel)
        {
            RefreshNPCList();
            if (_selectedNPCId == npcId)
            {
                SelectNPC(npcId);
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
            {
                Hide();
            }
        }
    }
}
