using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.PetInteraction;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 宠物互动 UI
    /// </summary>
    public class PetInteractionUI : Control {
        private PetInteractionSystem _system = PetInteractionSystem.Instance;
        
        // UI 组件
        private Label _titleLabel;
        private TabContainer _tabContainer;
        
        // 互动面板
        private VBoxContainer _interactionPanel;
        private OptionButton _petSelect;
        private OptionButton _interactionTypeSelect;
        private Button _performButton;
        private Label _resultLabel;
        private Label _dialogueLabel;
        private Label _cooldownLabel;
        
        // 统计面板
        private VBoxContainer _statsPanel;
        private Label _totalInteractionsLabel;
        private Label _specialInteractionsLabel;
        private Label _petCountLabel;
        
        // 历史面板
        private VBoxContainer _historyPanel;
        private ScrollContainer _historyScroll;
        private VBoxContainer _historyList;

        private bool _isVisible = false;

        public override void _Ready() {
            SetupUI();
            Hide();
        }

        private void SetupUI() {
            // 主容器
            var mainContainer = VBoxContainer.New();
            mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
            mainContainer.CustomMinimumSize = new Vector2(600, 500);
            AddChild(mainContainer);

            // 标题
            _titleLabel = Label.New();
            _titleLabel.Text = "🐾 宠物互动系统";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.Modulate = new Color(1f, 0.85f, 0.4f);
            mainContainer.AddChild(_titleLabel);

            // Tab 容器
            _tabContainer = TabContainer.New();
            _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
            _tabContainer.CustomMinimumSize = new Vector2(0, 450);
            mainContainer.AddChild(_tabContainer);

            // 互动面板
            SetupInteractionPanel();

            // 统计面板
            SetupStatsPanel();

            // 历史面板
            SetupHistoryPanel();
        }

        private void SetupInteractionPanel() {
            _interactionPanel = VBoxContainer.New();
            _interactionPanel.Name = "互动";
            _tabContainer.AddChild(_interactionPanel);

            // 宠物选择
            var petLabel = Label.New();
            petLabel.Text = "选择宠物:";
            _interactionPanel.AddChild(petLabel);

            _petSelect = OptionButton.New();
            _petSelect.CustomMinimumSize = new Vector2(200, 30);
            _petSelect.AddItem("默认宠物", 0);
            _interactionPanel.AddChild(_petSelect);

            // 互动类型选择
            var typeLabel = Label.New();
            typeLabel.Text = "互动方式:";
            _interactionPanel.AddChild(typeLabel);

            _interactionTypeSelect = OptionButton.New();
            _interactionTypeSelect.CustomMinimumSize = new Vector2(200, 30);
            _interactionTypeSelect.AddItem("抚摸 🖐️", (int)InteractionType.Pet);
            _interactionTypeSelect.AddItem("玩耍 🎾", (int)InteractionType.Play);
            _interactionTypeSelect.AddItem("对话 💬", (int)InteractionType.Talk);
            _interactionTypeSelect.AddItem("喂食 🍖", (int)InteractionType.Feed);
            _interactionTypeSelect.AddItem("梳理 🧹", (int)InteractionType.Groom);
            _interactionTypeSelect.AddItem("训练 🎯", (int)InteractionType.Train);
            _interactionTypeSelect.AddItem("抱抱 🤗", (int)InteractionType.Cuddle);
            _interactionTypeSelect.AddItem("治疗 💊", (int)InteractionType.Heal);
            _interactionPanel.AddChild(_interactionTypeSelect);

            // 按钮
            _performButton = Button.New();
            _performButton.Text = "执行互动";
            _performButton.CustomMinimumSize = new Vector2(200, 40);
            _performButton.Pressed += OnPerformPressed;
            _interactionPanel.AddChild(_performButton);

            // 结果显示
            _resultLabel = Label.New();
            _resultLabel.Text = "";
            _resultLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _resultLabel.Modulate = new Color(1f, 1f, 0.4f);
            _interactionPanel.AddChild(_resultLabel);

            // 对话显示
            _dialogueLabel = Label.New();
            _dialogueLabel.Text = "";
            _dialogueLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _dialogueLabel.Modulate = new Color(0.6f, 1f, 0.6f);
            _dialogueLabel.CustomMinimumSize = new Vector2(0, 60);
            _interactionPanel.AddChild(_dialogueLabel);

            // 冷却显示
            _cooldownLabel = Label.New();
            _cooldownLabel.Text = "";
            _cooldownLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _cooldownLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            _interactionPanel.AddChild(_cooldownLabel);
        }

        private void SetupStatsPanel() {
            _statsPanel = VBoxContainer.New();
            _statsPanel.Name = "统计";
            _tabContainer.AddChild(_statsPanel);

            _totalInteractionsLabel = Label.New();
            _totalInteractionsLabel.Text = "总互动次数: 0";
            _statsPanel.AddChild(_totalInteractionsLabel);

            _specialInteractionsLabel = Label.New();
            _specialInteractionsLabel.Text = "特殊互动次数: 0";
            _statsPanel.AddChild(_specialInteractionsLabel);

            _petCountLabel = Label.New();
            _petCountLabel.Text = "互动宠物数: 0";
            _statsPanel.AddChild(_petCountLabel);

            // 添加空标签用于间距
            for (int i = 0; i < 10; i++) {
                _statsPanel.AddChild(Label.New());
            }

            // 重置按钮
            var resetButton = Button.New();
            resetButton.Text = "重置统计";
            resetButton.Pressed += OnResetPressed;
            _statsPanel.AddChild(resetButton);
        }

        private void SetupHistoryPanel() {
            _historyPanel = VBoxContainer.New();
            _historyPanel.Name = "历史";
            _tabContainer.AddChild(_historyPanel);

            _historyScroll = ScrollContainer.New();
            _historyScroll.CustomMinimumSize = new Vector2(0, 400);
            _historyPanel.AddChild(_historyScroll);

            _historyList = VBoxContainer.New();
            _historyScroll.AddChild(_historyList);

            RefreshHistory();
        }

        private void OnPerformPressed() {
            var petId = "default_pet";
            var petName = "宠物";
            var petType = "Dog";
            
            var interactionType = (InteractionType)_interactionTypeSelect.GetSelectedId();

            // 检查冷却
            if (_system.IsOnCooldown(petId, interactionType)) {
                var remaining = _system.GetCooldownRemaining(petId, interactionType);
                _cooldownLabel.Text = $"⏳ 冷却中... {remaining:F1}秒";
                return;
            }

            // 执行互动
            var result = _system.PerformInteraction(petId, petName, petType, interactionType);
            
            // 显示结果
            string resultText = "";
            Color resultColor = Colors.White;

            switch (result) {
                case InteractionResult.Success:
                    resultText = "✅ 互动成功！";
                    resultColor = new Color(0.4f, 1f, 0.4f);
                    break;
                case InteractionResult.Special:
                    resultText = "✨ 特殊互动！好感度UP！";
                    resultColor = new Color(1f, 0.85f, 0.4f);
                    break;
                case InteractionResult.Critical:
                    resultText = "💖 完美互动！双倍收获！";
                    resultColor = new Color(1f, 0.4f, 0.7f);
                    break;
                case InteractionResult.Failed:
                    resultText = "❌ 互动失败";
                    resultColor = new Color(1f, 0.4f, 0.4f);
                    break;
            }

            _resultLabel.Text = resultText;
            _resultLabel.Modulate = resultColor;

            // 显示对话
            var dialogue = _system.GetRandomDialogue(interactionType);
            if (!string.IsNullOrEmpty(dialogue)) {
                _dialogueLabel.Text = $"💬 {petName}: \"{dialogue}\"";
            }

            // 更新冷却显示
            var effect = _system.GetInteractionEffect(interactionType);
            _cooldownLabel.Text = $"⏰ 冷却时间: {effect.cooldown}秒";

            // 刷新统计和历史
            RefreshStats();
            RefreshHistory();
        }

        private void OnResetPressed() {
            _system.ResetStatistics();
            RefreshStats();
            RefreshHistory();
            _resultLabel.Text = "✅ 统计已重置";
        }

        private void RefreshStats() {
            var stats = _system.GetStatistics();
            _totalInteractionsLabel.Text = $"总互动次数: {stats["totalInteractions"]}";
            _specialInteractionsLabel.Text = $"特殊互动次数: {stats["specialInteractions"]}";
            _petCountLabel.Text = $"互动宠物数: {stats["uniquePets"]}";
        }

        private void RefreshHistory() {
            // 清空历史列表
            foreach (Node child in _historyList.GetChildren()) {
                child.QueueFree();
            }

            // 添加历史记录
            var historyLabel = Label.New();
            historyLabel.Text = "最近互动记录:";
            historyLabel.Modulate = new Color(0.8f, 0.8f, 0.8f);
            _historyList.AddChild(historyLabel);

            var emptyLabel = Label.New();
            emptyLabel.Text = "暂无记录";
            emptyLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
            _historyList.AddChild(emptyLabel);
        }

        public override void _Input(InputEvent @event) {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
                // I 键切换显示
                if (keyEvent.Keycode == Key.I) {
                    ToggleVisibility();
                }
                // ESC 关闭
                else if (keyEvent.Keycode == Key.Escape && _isVisible) {
                    Hide();
                    _isVisible = false;
                }
            }
        }

        public void ToggleVisibility() {
            if (_isVisible) {
                Hide();
                _isVisible = false;
            } else {
                Show();
                _isVisible = true;
                RefreshStats();
                RefreshHistory();
            }
        }
    }
}
