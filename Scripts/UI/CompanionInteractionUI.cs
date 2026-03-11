using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// 伴侣互动系统界面
    /// </summary>
    public partial class CompanionInteractionUI : Control
    {
        private VBoxContainer _mainContainer;
        private HBoxContainer _typeSelector;
        private OptionButton _typeOption;
        private OptionButton _entityOption;
        private OptionButton _actionOption;
        private Label _actionDescription;
        private Label _actionInfo;
        private Button _startButton;
        private ProgressBar _progressBar;
        private Label _progressLabel;
        private VBoxContainer _statisticsContainer;
        private Label _totalInteractions;
        private Label _totalAffection;
        private Label _favoriteEntity;
        private List<OptionButton> _actionButtons;

        private CompanionInteractionSystem _interactionSystem;
        private InteractionType _currentType = InteractionType.Pet;
        private string _currentEntityId = "";

        public override void _Ready()
        {
            _interactionSystem = CompanionInteractionSystem.Instance;
            SetupUI();
            ConnectSignals();
            RefreshActions();
        }

        private void SetupUI()
        {
            // 设置窗口属性
            RectMinSize = new Vector2(500, 600);
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorsPreset = Control.LayoutPreset.FullRect;
            _mainContainer.MarginLeft = 20;
            _mainContainer.MarginTop = 20;
            _mainContainer.MarginRight = -20;
            _mainContainer.MarginBottom = -20;
            _mainContainer.AddThemeConstantOverride("separation", 15);
            AddChild(_mainContainer);

            // 标题
            var titleLabel = new Label();
            titleLabel.Text = "🐾 伴侣互动系统";
            titleLabel.Align = Label.AlignEnum.Center;
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _mainContainer.AddChild(titleLabel);

            // 类型选择器
            _typeSelector = new HBoxContainer();
            _typeSelector.Alignment = BoxContainer.AlignmentMode.Center;
            _typeSelector.CustomMinimumSize = new Vector2(0, 40);
            
            var typeLabel = new Label();
            typeLabel.Text = "类型: ";
            typeLabel.MarginRight = 10;
            _typeSelector.AddChild(typeLabel);

            _typeOption = new OptionButton();
            _typeOption.CustomMinimumSize = new Vector2(150, 0);
            _typeOption.AddItem("宠物互动", (int)InteractionType.Pet);
            _typeOption.AddItem("坐骑互动", (int)InteractionType.Mount);
            _typeOption.Selected = 0;
            _typeOption.ItemSelected += OnTypeSelected;
            _typeSelector.AddChild(_typeOption);

            _mainContainer.AddChild(_typeSelector);

            // 实体选择
            var entityContainer = new HBoxContainer();
            entityContainer.Alignment = BoxContainer.AlignmentMode.Center;
            
            var entityLabel = new Label();
            entityLabel.Text = "选择对象: ";
            entityLabel.MarginRight = 10;
            entityContainer.AddChild(entityLabel);

            _entityOption = new OptionButton();
            _entityOption.CustomMinimumSize = new Vector2(200, 0);
            _entityOption.AddItem("请选择...", 0);
            _entityOption.Selected = 0;
            _entityOption.ItemSelected += OnEntitySelected;
            entityContainer.AddChild(_entityOption);

            _mainContainer.AddChild(entityContainer);

            // 动作选择
            var actionContainer = new HBoxContainer();
            actionContainer.Alignment = BoxContainer.AlignmentMode.Center;

            var actionLabel = new Label();
            actionLabel.Text = "互动动作: ";
            actionLabel.MarginRight = 10;
            actionContainer.AddChild(actionLabel);

            _actionOption = new OptionButton();
            _actionOption.CustomMinimumSize = new Vector2(200, 0);
            _actionOption.AddItem("请选择...", 0);
            _actionOption.Selected = 0;
            _actionOption.ItemSelected += OnActionSelected;
            actionContainer.AddChild(_actionOption);

            _mainContainer.AddChild(actionContainer);

            // 动作描述
            _actionDescription = new Label();
            _actionDescription.Text = "选择一个互动动作查看详情";
            _actionDescription.Align = Label.AlignEnum.Center;
            _actionDescription.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            _mainContainer.AddChild(_actionDescription);

            // 动作信息
            _actionInfo = new Label();
            _actionInfo.Text = "";
            _actionInfo.Align = Label.AlignEnum.Center;
            _actionInfo.AddThemeFontSizeOverride("font_size", 14);
            _mainContainer.AddChild(_actionInfo);

            // 进度条
            _progressBar = new ProgressBar();
            _progressBar.CustomMinimumSize = new Vector2(0, 30);
            _progressBar.Value = 0;
            _progressBar.MaxValue = 100;
            _progressBar.Visible = false; 
            _mainContainer.AddChild(_progressBar);

            _progressLabel = new Label();
            _progressLabel.Text = "";
            _progressLabel.Align = Label.AlignEnum.Center;
            _progressLabel.Visible = false; 
            _mainContainer.AddChild(_progressLabel);

            // 开始按钮
            _startButton = new Button();
            _startButton.Text = "开始互动";
            _startButton.CustomMinimumSize = new Vector2(200, 45);
            _startButton.Disabled = true;
            _startButton.Pressed += OnStartPressed;
            _mainContainer.AddChild(_startButton);

            // 分隔线
            var separator = new HSeparator();
            _mainContainer.AddChild(separator);

            // 统计信息标题
            var statsTitle = new Label();
            statsTitle.Text = "📊 互动统计";
            statsTitle.AddThemeFontSizeOverride("font_size", 18);
            _mainContainer.AddChild(statsTitle);

            // 统计容器
            _statisticsContainer = new VBoxContainer();
            _statisticsContainer.AddThemeConstantOverride("separation", 8);

            _totalInteractions = new Label();
            _totalInteractions.Text = "总互动次数: 0";
            _statisticsContainer.AddChild(_totalInteractions);

            _totalAffection = new Label();
            _totalAffection.Text = "累计好感度: 0";
            _statisticsContainer.AddChild(_totalAffection);

            _favoriteEntity = new Label();
            _favoriteEntity.Text = "最喜爱对象: 无";
            _statisticsContainer.AddChild(_favoriteEntity);

            _mainContainer.AddChild(_statisticsContainer);

            // 关闭按钮
            var closeButton = new Button();
            closeButton.Text = "关闭 (ESC)";
            closeButton.CustomMinimumSize = new Vector2(0, 40);
            closeButton.Pressed += OnClosePressed;
            _mainContainer.AddChild(closeButton);

            UpdateStatistics();
        }

        private void ConnectSignals()
        {
            if (_interactionSystem != null)
            {
                _interactionSystem.Connect(SignalName.InteractionStarted, Callable.From(OnInteractionStarted));
                _interactionSystem.Connect(SignalName.InteractionCompleted, Callable.From(OnInteractionCompleted));
                _interactionSystem.Connect(SignalName.InteractionFailed, Callable.From(OnInteractionFailed));
            }
        }

        private void OnTypeSelected(long index)
        {
            _currentType = (InteractionType)index;
            RefreshEntities();
            RefreshActions();
        }

        private void OnEntitySelected(long index)
        {
            if (index > 0 && index < _entityOption.ItemCount)
            {
                _currentEntityId = _entityOption.GetItemText((int)index);
            }
            else
            {
                _currentEntityId = "";
            }
            UpdateStartButton();
        }

        private void OnActionSelected(long index)
        {
            UpdateActionInfo();
            UpdateStartButton();
        }

        private void RefreshEntities()
        {
            _entityOption.Clear();
            _entityOption.AddItem("请选择...", 0);

            if (_currentType == InteractionType.Pet)
            {
                // 从宠物系统获取宠物列表
                _entityOption.AddItem("默认宠物", 1);
            }
            else
            {
                // 从坐骑系统获取坐骑列表
                _entityOption.AddItem("默认坐骑", 1);
            }

            _entityOption.Selected = 0;
            _currentEntityId = "";
        }

        private void RefreshActions()
        {
            _actionOption.Clear();
            _actionOption.AddItem("请选择...", 0);

            if (_interactionSystem != null)
            {
                var actions = _interactionSystem.GetAvailableActions(_currentType);
                for (int i = 0; i < actions.Count; i++)
                {
                    var action = actions[i];
                    _actionOption.AddItem($"{action.Name} (Lv.{action.MinLevel})", i + 1);
                }
            }

            _actionOption.Selected = 0;
            _actionDescription.Text = "选择一个互动动作查看详情";
            _actionInfo.Text = "";
        }

        private void UpdateActionInfo()
        {
            var selectedIndex = _actionOption.Selected - 1;
            if (selectedIndex < 0)
            {
                _actionDescription.Text = "选择一个互动动作查看详情";
                _actionInfo.Text = "";
                return;
            }

            var actions = _interactionSystem?.GetAvailableActions(_currentType);
            if (actions == null || selectedIndex >= actions.Count) return;

            var action = actions[selectedIndex];
            _actionDescription.Text = action.Description;

            string requirements = "";
            if (action.RequiresItem)
            {
                requirements = $" [需要道具: {action.RequiredItemId}]";
            }

            _actionInfo.Text = $"好感度 +{action.AffectionGain} | 快乐度 +{action.HappinessGain} | 精力 {(action.EnergyCost >= 0 ? "-" : "+")}{Mathf.Abs(action.EnergyCost)} | 耗时 {action.Duration}秒{requirements}";
        }

        private void UpdateStartButton()
        {
            _startButton.Disabled = _entityOption.Selected <= 0 || _actionOption.Selected <= 0;
        }

        private void OnStartPressed()
        {
            var selectedEntityIndex = _entityOption.Selected - 1;
            var selectedActionIndex = _actionOption.Selected - 1;

            if (selectedEntityIndex < 0 || selectedActionIndex < 0) return;

            // 获取动作
            var actions = _interactionSystem?.GetAvailableActions(_currentType);
            if (actions == null || selectedActionIndex >= actions.Count) return;

            var action = actions[selectedActionIndex];
            var entityId = _entityOption.GetItemText(_entityOption.Selected);

            // 获取实体ID
            string entityKey = _currentType == InteractionType.Pet ? "pet_" : "mount_";
            entityKey += entityId.GetHashCode().ToString();

            _interactionSystem?.StartInteraction(entityKey, _currentType, action.Action);
        }

        private void OnInteractionStarted(string entityId, InteractionType entityType, InteractionAction action)
        {
            _progressBar.Visible = true;
            _progressLabel.Visible = true;
            _startButton.Disabled = true;
            _progressLabel.Text = "互动进行中...";
        }

        private void OnInteractionCompleted(string entityId, InteractionType entityType, InteractionAction action, int affectionGain, int happinessGain)
        {
            _progressBar.Visible = false; 
            _progressLabel.Visible = false; 
            _startButton.Disabled = false; 

            UpdateStatistics();

            // 显示完成通知
            _progressLabel.Text = $"✨ 互动完成！好感度 +{affectionGain}, 快乐度 +{happinessGain}";
            _progressLabel.Visible = true;

            // 3秒后隐藏
            var timer = GetTree().CreateTimer(3f);
            timer.Timeout += () => _progressLabel.Visible = false; 
        }

        private void OnInteractionFailed(string entityId, InteractionType entityType, InteractionAction action, string reason)
        {
            _progressLabel.Text = $"❌ 互动失败: {reason}";
            _progressLabel.Visible = true;

            var timer = GetTree().CreateTimer(2f);
            timer.Timeout += () => _progressLabel.Visible = false; 
        }

        private void UpdateStatistics()
        {
            if (_interactionSystem?.PlayerData == null) return;

            var data = _interactionSystem.PlayerData;
            _totalInteractions.Text = $"总互动次数: {data.TotalInteractions}";
            _totalAffection.Text = $"累计好感度: {data.TotalAffectionGained}";
            _favoriteEntity.Text = $"最喜爱对象: {(string.IsNullOrEmpty(data.FavoriteEntityId) ? "无" : data.FavoriteEntityId)}";
        }

        private void OnClosePressed()
        {
            Visible = false; 
            QueueFree();
        }

        public override void _Input(InputEvent evt)
        {
            if (evt is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
            {
                OnClosePressed();
            }
        }
    }
}
