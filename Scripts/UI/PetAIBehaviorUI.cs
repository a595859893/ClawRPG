using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Database;
using ClawRPG.Scripts.Data;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// Pet AI Behavior UI - configure and view pet AI behavior
    /// </summary>
    public partial class PetAIBehaviorUI : Control
    {
        private PetAIBehaviorSystem _aiSystem;
        
        private VBoxContainer _mainContainer;
        private OptionButton _petSelect;
        private OptionButton _behaviorSelect;
        private Label _currentStateLabel;
        private Label _behaviorNameLabel;
        private Label _statisticsLabel;
        private Button _closeButton;
        
        private string _currentPetId = "";
        private List<string> _availablePets = new List<string>();

        public override void _Ready()
        {
            _aiSystem = PetAIBehaviorSystem.Instance;
            if (_aiSystem == null)
            {
                GD.PushWarning("PetAIBehaviorSystem not found!");
                return;
            }

            SetupUI();
            LoadPets();
            VisibilityChanged += OnVisibilityChanged;
        }

        private void SetupUI()
        {
            // Main container
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorPreset(ControlPreset.CenterCenter);
            _mainContainer.CustomMinimumSize = new Vector2(500, 450);
            AddChild(_mainContainer);

            // Title
            var titleLabel = new Label();
            titleLabel.Text = "🐾 宠物AI行为系统";
            titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _mainContainer.AddChild(titleLabel);

            _mainContainer.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });

            // Pet selection
            var petLabel = new Label();
            petLabel.Text = "选择宠物:";
            _mainContainer.AddChild(petLabel);

            _petSelect = new OptionButton();
            _petSelect.ItemSelected += OnPetSelected;
            _mainContainer.AddChild(_petSelect);

            _mainContainer.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });

            // Behavior selection
            var behaviorLabel = new Label();
            behaviorLabel.Text = "AI行为模式:";
            _mainContainer.AddChild(behaviorLabel);

            _behaviorSelect = new OptionButton();
            _behaviorSelect.ItemSelected += OnBehaviorSelected;
            LoadBehaviorOptions();
            _mainContainer.AddChild(_behaviorSelect);

            // Current state display
            var stateContainer = new HBoxContainer();
            _mainContainer.AddChild(stateContainer);

            var stateTitleLabel = new Label();
            stateTitleLabel.Text = "当前状态: ";
            stateTitleLabel.AddThemeFontSizeOverride("font_size", 18);
            stateContainer.AddChild(stateTitleLabel);

            _currentStateLabel = new Label();
            _currentStateLabel.Text = "待机中";
            _currentStateLabel.AddThemeFontSizeOverride("font_size", 18);
            _currentStateLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.8f, 0.3f));
            stateContainer.AddChild(_currentStateLabel);

            // Behavior info
            _mainContainer.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });
            
            var infoFrame = new PanelContainer();
            infoFrame.CustomMinimumSize = new Vector2(0, 80);
            _mainContainer.AddChild(infoFrame);

            var infoLabel = new Label();
            infoLabel.Text = GetBehaviorDescription();
            infoLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            infoFrame.AddChild(infoLabel);

            _behaviorNameLabel = infoLabel;

            // Statistics
            var statsLabel = new Label();
            statsLabel.Text = "📊 战斗统计";
            statsLabel.AddThemeFontSizeOverride("font_size", 16);
            _mainContainer.AddChild(statsLabel);

            _statisticsLabel = new Label();
            _statisticsLabel.Text = "击败: 0 | 闪避: 0 | 格挡: 0\n伤害: 0 | 避免伤害: 0";
            _mainContainer.AddChild(_statisticsLabel);

            _mainContainer.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });

            // Close button
            _closeButton = new Button();
            _closeButton.Text = " 关闭 (ESC) ";
            _closeButton.Pressed += OnClosePressed;
            _mainContainer.AddChild(_closeButton);

            // Background
            var bgPanel = new PanelContainer();
            bgPanel.SetAnchorPreset(ControlPreset.FullRect);
            bgPanel.ZIndex = -1;
            MoveChild(bgPanel, 0);
        }

        private void LoadPets()
        {
            _petSelect.Clear();
            _availablePets.Clear();

            // Get pets from PetSystem
            var petSystem = PetSystem.Instance;
            if (petSystem != null)
            {
                var pets = petSystem.GetOwnedPets();
                foreach (var pet in pets)
                {
                    _availablePets.Add(pet.Id);
                    _petSelect.AddItem($"{pet.Name} (Lv.{pet.Level})", _availablePets.Count - 1);
                }
            }

            if (_availablePets.Count > 0)
            {
                _currentPetId = _availablePets[0];
                _petSelect.Selected = 0;
                _aiSystem.InitializePetAI(_currentPetId);
                UpdateUI();
            }
            else
            {
                _petSelect.AddItem("无宠物", 0);
            }
        }

        private void LoadBehaviorOptions()
        {
            _behaviorSelect.Clear();
            var behaviors = PetAIDatabase.GetAllBehaviors();
            
            for (int i = 0; i < behaviors.Count; i++)
            {
                var behavior = behaviors[i];
                _behaviorSelect.AddItem($"[{behavior.BehaviorType}] {behavior.BehaviorName}", i);
            }
            
            _behaviorSelect.Selected = 2; // Default to Aggressive
        }

        private void OnPetSelected(long index)
        {
            if (index >= 0 && index < _availablePets.Count)
            {
                _currentPetId = _availablePets[(int)index];
                _aiSystem.InitializePetAI(_currentPetId);
                UpdateUI();
            }
        }

        private void OnBehaviorSelected(long index)
        {
            if (_currentPetId == "") return;
            
            var behaviors = PetAIDatabase.GetAllBehaviors();
            if (index >= 0 && index < behaviors.Count)
            {
                var behavior = behaviors[(int)index];
                _aiSystem.SetBehavior(_currentPetId, behavior.BehaviorType);
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            if (_currentPetId == "") return;

            // Update state
            var state = _aiSystem.GetCurrentState(_currentPetId);
            _currentStateLabel.Text = state.ToString();

            // Update behavior description
            _behaviorNameLabel.Text = GetBehaviorDescription();

            // Update statistics
            var stats = _aiSystem.GetStatistics(_currentPetId);
            _statisticsLabel.Text = $"击败: {stats["enemies_attacked"]} | 闪避: {stats["dodges_successful"]} | 格挡: {stats["blocks_successful"]}\n" +
                                  $"伤害: {stats["total_damage_dealt"]:F0} | 避免伤害: {stats["total_damage_avoided"]:F0}";
        }

        private string GetBehaviorDescription()
        {
            if (_currentPetId == "") return "请选择宠物";
            
            var config = _aiSystem.GetBehaviorConfig(_currentPetId);
            
            string behaviorText = config.BehaviorType switch
            {
                PetAIBehavior.Passive => "🐕 被动: 只在非常接近敌人时才会攻击",
                PetAIBehavior.Defensive => "🛡️ 防御: 保护主人，优先攻击威胁主人的敌人",
                PetAIBehavior.Aggressive => "⚔️ 激进: 始终攻击最近的敌人",
                PetAIBehavior.Tactical => "🎯 战术: 智能位置选择，优先攻击低血量敌人",
                PetAIBehavior.Support => "💚 辅助: 专注于治疗和辅助主人",
                _ => ""
            };

            return $"{behaviorText}\n\n" +
                   $"攻击范围: {config.AttackRange:F0} | 追击范围: {config.ChaseRange:F0}\n" +
                   $"闪避几率: {config.DodgeChance:P0} | 格挡几率: {config.BlockChance:P0}\n" +
                   $"逃跑血线: {config.FleeThreshold:P0}";
        }

        private void OnVisibilityChanged()
        {
            if (Visible)
            {
                LoadPets();
                UpdateUI();
            }
        }

        private void OnClosePressed()
        {
            Hide();
        }

        public override void _Input(InputEvent e)
        {
            if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
            {
                Hide();
            }
        }

        public void Toggle()
        {
            if (Visible)
                Hide();
            else
                Show();
        }
    }
}
