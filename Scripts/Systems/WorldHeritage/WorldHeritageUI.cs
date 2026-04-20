using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.WorldHeritage
{
    /// <summary>
    /// 世界遗产 UI — 在主界面显示遗产视觉装饰
    /// 不影响游戏数值，纯视觉叙事层
    /// </summary>
    public partial class WorldHeritageUI : Control
    {
        // ============================================================
        // UI 结构
        // 遗产展示面板（主界面背景装饰层）
        // ============================================================

        [Export]
        private bool _enabled = true;

        [Export]
        private NodePath _heritageContainerPath;

        private VBoxContainer _heritageContainer;

        // 各区域的遗产装饰节点
        private Control _forestHeritage;
        private Control _dungeonHeritage;
        private Control _caveHeritage;
        private Control _towerHeritage;
        private Control _castleHeritage;

        // 主界面中央铭刻显示
        private Label _inscriptionLabel;
        private int _displayedInscriptionCount = 0;

        public override void _Ready()
        {
            base._Ready();

            _heritageContainer = GetNodeOrNull<VBoxContainer>(_heritageContainerPath);

            // 初始化视觉装饰节点
            SetupHeritageDecorations();

            // 订阅遗产激活事件
            if (WorldHeritageSystem.Instance != null)
            {
                WorldHeritageSystem.Instance.OnHeritageActivated += OnHeritageActivated;
                WorldHeritageSystem.Instance.OnHeritageReady += OnHeritageReady;
                RefreshAllHeritageDisplays();
            }
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            if (WorldHeritageSystem.Instance != null)
            {
                WorldHeritageSystem.Instance.OnHeritageActivated -= OnHeritageActivated;
                WorldHeritageSystem.Instance.OnHeritageReady -= OnHeritageReady;
            }
        }

        #region Setup

        private void SetupHeritageDecorations()
        {
            // 各区域遗产容器 — 实际节点需要在场景中创建
            // 这里通过程序方式添加基础结构

            if (_heritageContainer == null)
            {
                // 如果没有预设容器，创建一个
                _heritageContainer = new VBoxContainer();
                _heritageContainer.Name = "HeritageContainer";
                AddChild(_heritageContainer);
            }

            // 创建区域装饰容器（仅视觉占位，实际资源由美术提供）
            CreateRegionDecoration(RegionId.Forest, "ForestHeritage", _forestHeritage);
            CreateRegionDecoration(RegionId.Dungeon, "DungeonHeritage", _dungeonHeritage);
            CreateRegionDecoration(RegionId.Cave, "CaveHeritage", _caveHeritage);
            CreateRegionDecoration(RegionId.Tower, "TowerHeritage", _towerHeritage);
            CreateRegionDecoration(RegionId.Castle, "CastleHeritage", _castleHeritage);

            // 铭刻标签
            _inscriptionLabel = new Label();
            _inscriptionLabel.Name = "InscriptionLabel";
            _inscriptionLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _inscriptionLabel.Modulate = new Color(1, 0.85f, 0.4f, 0.7f); // 暖金色，半透明
            AddChild(_inscriptionLabel);
        }

        private void CreateRegionDecoration(RegionId region, string nodeName, Control existingNode)
        {
            if (existingNode != null)
                return;

            var container = new HBoxContainer();
            container.Name = nodeName;
            container.Alignment = BoxContainer.AlignMode.Center;

            // 区域标题标签
            var regionLabel = new Label();
            regionLabel.Name = "RegionLabel";
            regionLabel.Text = GetRegionEmoji(region);
            regionLabel.Modulate = new Color(1, 1, 1, 0.3f); // 默认半透明
            container.AddChild(regionLabel);

            // 遗产图标容器
            var iconBox = new HBoxContainer();
            iconBox.Name = "Icons";
            container.AddChild(iconBox);

            _heritageContainer.AddChild(container);
        }

        private string GetRegionEmoji(RegionId region)
        {
            switch (region)
            {
                case RegionId.Forest: return "🌲";
                case RegionId.Dungeon: return "⚔️";
                case RegionId.Cave: return "🔥";
                case RegionId.Tower: return "🗼";
                case RegionId.Castle: return "👑";
                default: return "✨";
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// 新遗产激活时调用 — 更新对应区域显示
        /// </summary>
        private void OnHeritageActivated(string recordId, HeritageRecord record)
        {
            if (!_enabled)
                return;

            UpdateRegionDisplay(record.Region, record);
            UpdateInscriptionDisplay();
        }

        /// <summary>
        /// 遗产数据就绪时调用 — 刷新所有显示
        /// </summary>
        private void OnHeritageReady(List<HeritageRecord> activeHeritages)
        {
            RefreshAllHeritageDisplays();
        }

        #endregion

        #region Display Updates

        /// <summary>
        /// 刷新所有区域的遗产显示
        /// </summary>
        private void RefreshAllHeritageDisplays()
        {
            var system = WorldHeritageSystem.Instance;
            if (system == null)
                return;

            // 按区域分组显示
            foreach (RegionId region in System.Enum.GetValues(typeof(RegionId)))
            {
                if (region == RegionId.None)
                    continue;

                var regionHeritages = system.GetHeritagesForRegion(region);
                if (regionHeritages.Count > 0)
                {
                    RefreshRegionDisplay(region, regionHeritages);
                }
            }

            UpdateInscriptionDisplay();
        }

        /// <summary>
        /// 更新指定区域的遗产显示
        /// </summary>
        private void UpdateRegionDisplay(RegionId region, HeritageRecord newRecord)
        {
            if (region == RegionId.None)
                return;

            var regionNode = FindRegionNode(region);
            if (regionNode == null)
                return;

            // 添加新遗产图标
            var iconBox = regionNode.GetNodeOrNull<HBoxContainer>("Icons");
            if (iconBox == null)
                return;

            var icon = new Label();
            icon.Text = GetHeritageIcon(newRecord);
            icon.Modulate = new Color(1, 1, 1, 0.8f);

            // 入场动画
            icon.Scale = new Vector2(0.1f, 0.1f);
            iconBox.AddChild(icon);

            // Tween 入场动画
            var tween = CreateTween();
            tween.TweenProperty(icon, "scale", new Vector2(1f, 1f), 0.4f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
            tween.TweenProperty(icon, "modulate:a", 0.8f, 0.2f);

            // 提升区域标题亮度
            var regionLabel = regionNode.GetNodeOrNull<Label>("RegionLabel");
            if (regionLabel != null)
            {
                var currentAlpha = regionLabel.Modulate.a;
                tween.TweenProperty(regionLabel, "modulate:a", Mathf.Min(currentAlpha + 0.15f, 1f), 0.3f);
            }
        }

        private void RefreshRegionDisplay(RegionId region, List<HeritageRecord> records)
        {
            var regionNode = FindRegionNode(region);
            if (regionNode == null)
                return;

            var iconBox = regionNode.GetNodeOrNull<HBoxContainer>("Icons");
            if (iconBox == null)
                return;

            // 清空现有图标
            foreach (var child in iconBox.GetChildren())
            {
                child.QueueFree();
            }

            // 添加所有已激活遗产的图标
            foreach (var record in records)
            {
                var icon = new Label();
                icon.Text = GetHeritageIcon(record);
                icon.Modulate = new Color(1, 1, 1, 0.8f);
                iconBox.AddChild(icon);
            }

            // 提升区域标题亮度
            var regionLabel = regionNode.GetNodeOrNull<Label>("RegionLabel");
            if (regionLabel != null)
            {
                float targetAlpha = 0.3f + (records.Count * 0.15f);
                regionLabel.Modulate = new Color(1, 1, 1, Mathf.Min(targetAlpha, 1f));
            }
        }

        /// <summary>
        /// 更新主界面铭刻文字
        /// </summary>
        private void UpdateInscriptionDisplay()
        {
            var system = WorldHeritageSystem.Instance;
            if (system == null || _inscriptionLabel == null)
                return;

            var inscriptions = system.GetHeritagesForRegion(RegionId.None);
            int count = inscriptions.Count + system.GetActiveHeritageCount();

            if (count == 0)
            {
                _inscriptionLabel.Text = "";
                return;
            }

            string text = $"世界遗产 × {system.GetActiveHeritageCount()}";
            if (system.GetTotalVictories() > 0)
            {
                text += $"  |  胜利 {system.GetTotalVictories()} 次";
            }
            text += $"  |  探索 {system.GetTotalRunsCompleted()} 局";

            // 淡入动画
            _inscriptionLabel.Text = text;
            _inscriptionLabel.Modulate = new Color(1, 0.85f, 0.4f, 0f);
            var tween = CreateTween();
            tween.TweenProperty(_inscriptionLabel, "modulate:a", 0.7f, 0.5f)
                .SetTrans(Tween.TransitionType.Linear);

            _displayedInscriptionCount = system.GetActiveHeritageCount();
        }

        private string GetHeritageIcon(HeritageRecord record)
        {
            switch (record.Type)
            {
                case HeritageType.BossConquest:
                    return "💀";
                case HeritageType.SecretDiscovery:
                    return "🔮";
                case HeritageType.AchievementInscription:
                    return "🏆";
                default:
                    return "✨";
            }
        }

        private Control FindRegionNode(RegionId region)
        {
            string nodeName = region switch
            {
                RegionId.Forest => "ForestHeritage",
                RegionId.Dungeon => "DungeonHeritage",
                RegionId.Cave => "CaveHeritage",
                RegionId.Tower => "TowerHeritage",
                RegionId.Castle => "CastleHeritage",
                _ => null
            };

            if (nodeName == null)
                return null;

            return _heritageContainer?.GetNodeOrNull<Control>(nodeName);
        }

        #endregion

        #region Public API

        /// <summary>
        /// 运行时开关
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            Visible = enabled;
        }

        /// <summary>
        /// 获取当前显示的遗产数量
        /// </summary>
        public int GetDisplayedHeritageCount()
        {
            return _displayedInscriptionCount;
        }

        #endregion
    }
}
