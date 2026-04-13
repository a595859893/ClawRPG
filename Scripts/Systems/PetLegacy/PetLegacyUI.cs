using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.PetLegacy
{
    /// <summary>
    /// 宠物遗产UI — 显示场上遗产标记的状态和交互
    /// </summary>
    public partial class PetLegacyUI : CanvasLayer
    {
        private VBoxContainer _markerContainer;
        private Label _bonusLabel;
        private PetLegacyMarkerData _selectedMarker;
        private PopupPanel _biographyPopup;

        public override void _Ready()
        {
            base._Ready();

            // 创建主容器
            _markerContainer = new VBoxContainer();
            _markerContainer.Name = "MarkerContainer";
            _markerContainer.SetAnchorsPreset(Control.LayoutPreset.TopRight);
            _markerContainer.OffsetLeft = -220;
            _markerContainer.OffsetTop = 10;
            _markerContainer.OffsetRight = -10;
            _markerContainer.OffsetBottom = 200;
            AddChild(_markerContainer);

            // 创建增益显示标签
            _bonusLabel = new Label();
            _bonusLabel.Name = "BonusLabel";
            _bonusLabel.Text = "Legacy Bonus: +0%";
            _bonusLabel.HorizontalAlignment = HorizontalAlignment.Right;
            _markerContainer.AddChild(_bonusLabel);

            // 创建标记按钮容器
            var buttonContainer = new HBoxContainer();
            buttonContainer.Name = "ButtonContainer";
            _markerContainer.AddChild(buttonContainer);

            // 订阅系统信号
            if (PetLegacySystem.Instance != null)
            {
                PetLegacySystem.Instance.OnLegacyMarkerAdded += OnMarkerAdded;
                PetLegacySystem.Instance.OnLegacyBonusChanged += OnBonusChanged;
                PetLegacySystem.Instance.OnLegacyMarkerClicked += OnMarkerClicked;
            }

            // 创建生物信息弹窗
            CreateBiographyPopup();
        }

        private void CreateBiographyPopup()
        {
            _biographyPopup = new PopupPanel();
            _biographyPopup.Name = "BiographyPopup";
            _biographyPopup.Size = new Vector2(300, 200);
            AddChild(_biographyPopup);

            var container = new VBoxContainer();
            container.SetAnchorsPreset(Control.LayoutPreset.Full);
            container.MarginLeft = 10;
            container.MarginTop = 10;
            container.MarginRight = -10;
            container.MarginBottom = -10;
            _biographyPopup.AddChild(container);

            // 宠物名称
            var nameLabel = new Label();
            nameLabel.Name = "NameLabel";
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            nameLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            container.AddChild(nameLabel);

            // 墓碑类型
            var typeLabel = new Label();
            typeLabel.Name = "TypeLabel";
            typeLabel.HorizontalAlignment = HorizontalAlignment.Center;
            container.AddChild(typeLabel);

            // 友谊等级
            var friendshipLabel = new Label();
            friendshipLabel.Name = "FriendshipLabel";
            friendshipLabel.HorizontalAlignment = HorizontalAlignment.Center;
            container.AddChild(friendshipLabel);

            // 累计战斗次数
            var battlesLabel = new Label();
            battlesLabel.Name = "BattlesLabel";
            battlesLabel.HorizontalAlignment = HorizontalAlignment.Center;
            container.AddChild(battlesLabel);

            // 死亡日期
            var deathLabel = new Label();
            deathLabel.Name = "DeathLabel";
            deathLabel.HorizontalAlignment = HorizontalAlignment.Center;
            container.AddChild(deathLabel);

            // 关闭按钮
            var closeBtn = new Button();
            closeBtn.Text = "Close";
            closeBtn.Align = Button.TextAlign.Center;
            closeBtn.Pressed += () => _biographyPopup.Hide();
            container.AddChild(closeBtn);
        }

        private void OnMarkerAdded(PetLegacyMarkerData marker)
        {
            RefreshMarkerDisplay();
        }

        private void OnBonusChanged(int activeCount, float bonus)
        {
            _bonusLabel.Text = $"Legacy Bonus: +{bonus * 100:F0}% ({activeCount}/{PetLegacySystem.MAX_ACTIVE_MARKERS})";
        }

        private void OnMarkerClicked(int petId)
        {
            var marker = PetLegacySystem.Instance.GetMarker(petId);
            if (marker != null)
            {
                ShowBiography(marker);
            }
        }

        /// <summary>
        /// 显示宠物小传弹窗
        /// </summary>
        public void ShowBiography(PetLegacyMarkerData marker)
        {
            _selectedMarker = marker;

            var container = _biographyPopup.GetNode<VBoxContainer>(".");

            container.GetNode<Label>("NameLabel").Text = $"🐾 {marker.PetName}";
            container.GetNode<Label>("TypeLabel").Text = $"Type: {GetMarkerTypeText(marker.MarkerType)}";
            container.GetNode<Label>("FriendshipLabel").Text = $"Friendship at death: {marker.FriendshipLevel}";
            container.GetNode<Label>("BattlesLabel").Text = $"Battles together: {marker.TotalBattles}";
            container.GetNode<Label>("DeathLabel").Text = $"Died at: {GetDeathDateText(marker.DeathTimestamp)}";

            _biographyPopup.PopupCentered();
        }

        private string GetMarkerTypeText(LegacyType type)
        {
            return type switch
            {
                LegacyType.Soul => "💜 Soul Orb",
                LegacyType.Banner => "⚔️ Battle Banner",
                _ => "🪦 Tombstone"
            };
        }

        private string GetDeathDateText(float timestamp)
        {
            try
            {
                var dateTime = DateTimeOffset.FromUnixTimeSeconds((long)timestamp).LocalDateTime;
                return dateTime.ToString("yyyy-MM-dd HH:mm");
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// 刷新场上标记显示
        /// </summary>
        public void RefreshMarkerDisplay()
        {
            // 清除旧按钮
            foreach (var child in _markerContainer.GetChildren())
            {
                if (child.Name != "BonusLabel" && child.Name != "ButtonContainer")
                    child.QueueFree();
            }

            var buttonContainer = _markerContainer.GetNodeOrNull<HBoxContainer>("ButtonContainer");
            if (buttonContainer == null)
            {
                buttonContainer = new HBoxContainer();
                buttonContainer.Name = "ButtonContainer";
                _markerContainer.AddChild(buttonContainer);
            }
            else
            {
                foreach (var child in buttonContainer.GetChildren())
                    child.QueueFree();
            }

            // 创建新按钮
            var activeMarkers = PetLegacySystem.Instance.GetActiveMarkers();
            foreach (var marker in activeMarkers)
            {
                var btn = CreateMarkerButton(marker);
                buttonContainer.AddChild(btn);
            }

            // 显示休眠标记（灰色）
            var dormantMarkers = PetLegacySystem.Instance.GetDormantMarkers();
            foreach (var marker in dormantMarkers)
            {
                var btn = CreateMarkerButton(marker);
                btn.Modulate = new Color(0.5f, 0.5f, 0.5f);  // 灰色表示休眠
                buttonContainer.AddChild(btn);
            }
        }

        private Button CreateMarkerButton(PetLegacyMarkerData marker)
        {
            var btn = new Button();
            btn.CustomMinimumSize = new Vector2(40, 40);

            // 根据类型设置图标
            string icon = GetMarkerIcon(marker.MarkerType);
            btn.Text = icon;
            btn.TooltipText = $"{marker.PetName} ({GetMarkerTypeText(marker.MarkerType)})";

            btn.Pressed += () => OnMarkerButtonPressed(marker);

            return btn;
        }

        private string GetMarkerIcon(LegacyType type)
        {
            return type switch
            {
                LegacyType.Soul => "💜",
                LegacyType.Banner => "⚔️",
                _ => "🪦"
            };
        }

        private void OnMarkerButtonPressed(PetLegacyMarkerData marker)
        {
            PetLegacySystem.Instance.OnMarkerClicked(marker.PetId);
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            if (PetLegacySystem.Instance != null)
            {
                PetLegacySystem.Instance.OnLegacyMarkerAdded -= OnMarkerAdded;
                PetLegacySystem.Instance.OnLegacyBonusChanged -= OnBonusChanged;
                PetLegacySystem.Instance.OnLegacyMarkerClicked -= OnMarkerClicked;
            }
        }
    }
}
