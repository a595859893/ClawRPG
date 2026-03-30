using Godot;
using System;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// HUDUI - 游戏内HUD管理
    /// 处理血条、技能栏、状态信息等游戏HUD元素的显示与更新
    /// </summary>
    public partial class HUDUI : BaseUI
    {
        public static new HUDUI Instance { get; protected set; }

        // 场景引用
        private Main _main;
        private Player _player;

        // HUD 节点
        private ProgressBar _healthBar;
        private ProgressBar _manaBar;
        private ProgressBar _expBar;
        private Label _healthLabel;
        private Label _manaLabel;
        private Label _expLabel;
        private Label _levelLabel;
        private Label _goldLabel;
        private Control _skillBar;
        private Control _quickSlotBar;
        private Control _statusEffects;

        // 刷新间隔
        private float _updateInterval = 0.1f;
        private float _updateTimer = 0f;

        public override void _Ready()
        {
            base._Ready();
            Instance = this;
            LoadNodes();
        }

        private void LoadNodes()
        {
            var canvasLayer = GetTree()?.CurrentScene?.GetNodeOrNull<CanvasLayer>("CanvasLayer");
            if (canvasLayer != null)
            {
                var hud = canvasLayer.GetNodeOrNull<Control>("HUDUI");
                if (hud != null)
                {
                    _healthBar = hud.GetNodeOrNull<ProgressBar>("VBox/HealthBar");
                    _manaBar = hud.GetNodeOrNull<ProgressBar>("VBox/ManaBar");
                    _expBar = hud.GetNodeOrNull<ProgressBar>("VBox/ExpBar");
                    _healthLabel = hud.GetNodeOrNull<Label>("VBox/HealthLabel");
                    _manaLabel = hud.GetNodeOrNull<Label>("VBox/ManaLabel");
                    _expLabel = hud.GetNodeOrNull<Label>("VBox/ExpLabel");
                    _levelLabel = hud.GetNodeOrNull<Label>("VBox/LevelLabel");
                    _goldLabel = hud.GetNodeOrNull<Label>("VBox/GoldLabel");
                    _skillBar = hud.GetNodeOrNull<Control>("SkillBar");
                    _quickSlotBar = hud.GetNodeOrNull<Control>("QuickSlotBar");
                    _statusEffects = hud.GetNodeOrNull<Control>("StatusEffects");
                }
            }
        }

        public void Initialize(Main main)
        {
            _main = main;
            _player = GetTree()?.GetFirstNodeInGroup("player") as Player;
            Refresh();
        }

        public override void _Process(double delta)
        {
            if (!IsVisible) return;

            _updateTimer += (float)delta;
            if (_updateTimer >= _updateInterval)
            {
                _updateTimer = 0f;
                Refresh();
            }
        }

        protected override void OnRefresh()
        {
            UpdatePlayerStats();
        }

        private void UpdatePlayerStats()
        {
            if (_player == null)
            {
                _player = GetTree()?.GetFirstNodeInGroup("player") as Player;
                if (_player == null) return;
            }

            // 更新生命值
            if (_healthBar != null)
            {
                _healthBar.MaxValue = _player.MaxHealth;
                _healthBar.Value = _player.Health;
            }

            if (_healthLabel != null)
            {
                _healthLabel.Text = $"HP: {_player.Health}/{_player.MaxHealth}";
            }

            // 更新魔法值
            if (_manaBar != null)
            {
                _manaBar.MaxValue = _player.MaxMana;
                _manaBar.Value = _player.Mana;
            }

            if (_manaLabel != null)
            {
                _manaLabel.Text = $"MP: {_player.Mana}/{_player.MaxMana}";
            }

            // 更新经验值
            if (_expBar != null)
            {
                _expBar.MaxValue = _player.ExpToNextLevel;
                _expBar.Value = _player.CurrentExp;
            }

            if (_expLabel != null)
            {
                _expLabel.Text = $"EXP: {_player.CurrentExp}/{_player.ExpToNextLevel}";
            }

            // 更新等级
            if (_levelLabel != null)
            {
                _levelLabel.Text = $"Lv.{_player.Level}";
            }

            // 更新金币
            if (_goldLabel != null && _player.HasMethod("GetGold"))
            {
                _goldLabel.Text = $"Gold: {_player.GetGold()}";
            }
        }

        public void ShowSkillCooldown(int skillIndex, float cooldownRemaining)
        {
            if (_skillBar == null) return;

            var slot = _skillBar.GetNodeOrNull<Control>($"Slot{skillIndex}");
            if (slot != null && slot.HasMethod("SetCooldown"))
            {
                slot.SetCooldown(cooldownRemaining);
            }
        }

        public void AddStatusEffect(string effectName, float duration)
        {
            GD.Print($"[HUDUI] Adding status effect: {effectName}");
        }

        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary
            {
                ["UIName"] = UIName,
                ["IsVisible"] = IsVisible
            };
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            base.ImportSaveData(data);
            Refresh();
        }

        public override void _ExitTree()
        {
            Instance = null;
        }
    }
}
