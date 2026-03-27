using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Framework;

namespace ClawRPG.Scripts.Systems.WeaponResonance
{
    /// <summary>
    /// 武器共鸣系统 — 检测双持同类型武器并应用共鸣效果
    /// </summary>
    public partial class WeaponResonanceSystem : BaseSystem
    {
        public static WeaponResonanceSystem Instance { get; private set; }

        /// <summary>共鸣数据</summary>
        public WeaponResonanceData ResonanceData { get; private set; } = new();

        /// <summary>
        /// 武器提供者接口 — 允许不同装备系统接入
        /// 默认实现：查询 InventoryManager 或 PlayerInventoryData
        /// </summary>
        public interface IWeaponProvider
        {
            /// <summary>获取当前主手武器类型，无武器返回 null</summary>
            string GetMainWeaponType();
            /// <summary>获取当前副手武器类型，无武器返回 null</summary>
            string GetOffhandWeaponType();
        }

        private IWeaponProvider _weaponProvider;

        // Signals
        public Signal<string> ResonanceActivated { get; } = new Signal<string>();
        public Signal<string, bool> ResonanceChanged { get; } = new Signal<string, bool>();
        public Signal<string, ResonanceEffect> ResonanceEffectUpdated { get; } = new Signal<string, ResonanceEffect>();

        // 共鸣特效节点引用（运行时动态创建）
        private Label _resonanceLabel;
        private Timer _resonanceLabelTimer;
        private bool _labelVisible = false;

        // 副手武器类型缓存（检测变化）
        private string _lastMainWeaponType = "";
        private string _lastOffhandWeaponType = "";

        public override void _Ready()
        {
            Instance = this;

            // 设置默认武器提供者
            SetWeaponProvider(new DefaultWeaponProvider());

            // 初始化 UI
            InitializeResonanceUI();

            GD.Print("[WeaponResonanceSystem] 初始化完成");
        }

        public override void _Process(double delta)
        {
            ResonanceData.Update(delta);

            // 检测武器变化
            CheckWeaponResonance();
        }

        /// <summary>
        /// 设置武器类型提供者（可注入自定义实现）
        /// </summary>
        public void SetWeaponProvider(IWeaponProvider provider)
        {
            _weaponProvider = provider;
        }

        /// <summary>
        /// 检测共鸣状态并触发/关闭共鸣
        /// </summary>
        private void CheckWeaponResonance()
        {
            if (_weaponProvider == null) return;

            string mainType = _weaponProvider.GetMainWeaponType() ?? "";
            string offhandType = _weaponProvider.GetOffhandWeaponType() ?? "";

            // 如果武器未变化，跳过检测
            if (mainType == _lastMainWeaponType && offhandType == _lastOffhandWeaponType)
                return;

            _lastMainWeaponType = mainType;
            _lastOffhandWeaponType = offhandType;

            // 检测双持：主手和副手都有武器且类型相同
            if (!string.IsNullOrEmpty(mainType) &&
                !string.IsNullOrEmpty(offhandType) &&
                mainType.Equals(offhandType, StringComparison.OrdinalIgnoreCase))
            {
                // 激活共鸣
                if (!ResonanceData.IsActive || ResonanceData.ActiveWeaponType != mainType)
                {
                    ResonanceData.Activate(mainType);
                    var effect = ResonanceData.GetActiveEffect();

                    ResonanceActivated.Emit(mainType);
                    ResonanceChanged.Emit(mainType, true);
                    if (effect != null)
                        ResonanceEffectUpdated.Emit(mainType, effect);

                    ShowResonanceLabel(mainType, effect);
                    GD.Print($"[WeaponResonance] 共鸣激活: {mainType} - {effect?.Name}");
                }
            }
            else
            {
                // 关闭共鸣
                if (ResonanceData.IsActive)
                {
                    string deactivatedType = ResonanceData.ActiveWeaponType;
                    ResonanceData.Deactivate();

                    ResonanceChanged.Emit(deactivatedType, false);
                    HideResonanceLabel();
                    GD.Print($"[WeaponResonance] 共鸣关闭: {deactivatedType}");
                }
            }
        }

        /// <summary>
        /// 获取当前伤害加成（直接用于 SkillModules）
        /// </summary>
        public float GetDamageBonus()
        {
            return ResonanceData.GetDamageBonus();
        }

        /// <summary>
        /// 获取当前暴击率加成
        /// </summary>
        public float GetCritBonus()
        {
            return ResonanceData.GetCritBonus();
        }

        /// <summary>
        /// 获取当前暴击伤害加成
        /// </summary>
        public float GetCritDamageBonus()
        {
            return ResonanceData.GetCritDamageBonus();
        }

        /// <summary>
        /// 获取当前攻击速度加成
        /// </summary>
        public float GetAttackSpeedBonus()
        {
            return ResonanceData.GetAttackSpeedBonus();
        }

        /// <summary>
        /// 获取共鸣是否激活
        /// </summary>
        public bool IsResonanceActive()
        {
            return ResonanceData.IsActive;
        }

        /// <summary>
        /// 获取当前激活的共鸣类型名称
        /// </summary>
        public string GetActiveTypeName()
        {
            return ResonanceData.ActiveWeaponType;
        }

        /// <summary>
        /// 获取共鸣描述
        /// </summary>
        public string GetResonanceDescription()
        {
            return ResonanceData.GetResonanceDescription();
        }

        /// <summary>
        /// 获取当前共鸣效果配置
        /// </summary>
        public ResonanceEffect GetActiveEffect()
        {
            return ResonanceData.GetActiveEffect();
        }

        // ===== UI =====

        private void InitializeResonanceUI()
        {
            // 创建 CanvasLayer 用于显示共鸣提示
            var canvasLayer = new CanvasLayer();
            canvasLayer.Name = "ResonanceUILayer";
            GetTree().Root.AddChild(canvasLayer);

            // 创建居中标签
            _resonanceLabel = new Label();
            _resonanceLabel.Name = "ResonanceLabel";
            _resonanceLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _resonanceLabel.VerticalAlignment = VerticalAlignment.Center;
            _resonanceLabel.Position = new Vector2(0, -200); // 屏幕上半部分
            _resonanceLabel.Size = new Vector2(1920, 100);
            _resonanceLabel.Modulate = new Color(1f, 0.85f, 0.2f, 0f); // 金黄色，从透明开始
            _resonanceLabel.AddThemeFontSizeOverride("font_size", 32);
            canvasLayer.AddChild(_resonanceLabel);

            // 创建淡出计时器
            _resonanceLabelTimer = new Timer();
            _resonanceLabelTimer.Name = "ResonanceLabelTimer";
            _resonanceLabelTimer.OneShot = true;
            canvasLayer.AddChild(_resonanceLabelTimer);
            _resonanceLabelTimer.Timeout += OnResonanceLabelTimerTimeout;
        }

        private void ShowResonanceLabel(string weaponType, ResonanceEffect effect)
        {
            if (_resonanceLabel == null || effect == null) return;

            string text = $"⚡ {effect.Name}！{effect.Description}";
            _resonanceLabel.Text = text;
            _resonanceLabel.Modulate = new Color(1f, 0.85f, 0.2f, 1f); // 立即显示

            _labelVisible = true;

            // 重置计时器，3秒后淡出
            _resonanceLabelTimer.Stop();
            _resonanceLabelTimer.Start(3.0f);
        }

        private void HideResonanceLabel()
        {
            if (_resonanceLabel != null)
            {
                _resonanceLabel.Modulate = new Color(1f, 0.85f, 0.2f, 0f);
            }
            _labelVisible = false;
        }

        private async void OnResonanceLabelTimerTimeout()
        {
            // 淡出动画（0.5秒）
            if (_resonanceLabel == null) return;

            float elapsed = 0f;
            float duration = 0.5f;
            Color startColor = new Color(1f, 0.85f, 0.2f, 1f);
            Color endColor = new Color(1f, 0.85f, 0.2f, 0f);

            while (elapsed < duration && _resonanceLabel != null)
            {
                elapsed += 0.05f;
                float t = elapsed / duration;
                _resonanceLabel.Modulate = startColor.Lerp(endColor, t);
                await ToSignal(GetTree().CreateTimer(0.05f), Timer.SignalName.Timeout);
            }

            if (_resonanceLabel != null)
            {
                _resonanceLabel.Modulate = endColor;
            }
        }

        // ===== 持久化 =====

        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            data["isActive"] = ResonanceData.IsActive;
            data["activeWeaponType"] = ResonanceData.ActiveWeaponType;
            data["activeDuration"] = ResonanceData.ActiveDuration;
            return data;
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;

            if (data.Contains("isActive") && (bool)data["isActive"])
            {
                string weaponType = data.Contains("activeWeaponType") ? (string)data["activeWeaponType"] : "";
                if (!string.IsNullOrEmpty(weaponType))
                {
                    ResonanceData.Activate(weaponType);
                    // 触发 UI 显示（如果当前有武器配置）
                    var effect = ResonanceData.GetActiveEffect();
                    ShowResonanceLabel(weaponType, effect);
                }
            }
            else
            {
                ResonanceData.Deactivate();
            }
        }

        // ===== 默认武器提供者 =====

        /// <summary>
        /// 默认武器提供者 — 从 InventoryManager 读取装备的武器类型
        /// 如果主手和副手武器类型相同（需扩展装备系统支持副手武器），
        /// 或者模拟双持检测，则触发共鸣。
        /// 
        /// 当前游戏只有1个武器槽，这里通过模拟方式检测：
        /// - 如果主手武器类型存在且在 ResonanceConfig 中注册，
        ///   则尝试查询 OffhandWeaponType（暂无实现，返回空）
        /// - 未来扩展副手武器槽后，只需改这里
        /// </summary>
        private class DefaultWeaponProvider : IWeaponProvider
        {
            public string GetMainWeaponType()
            {
                // 方案A：从 InventoryManager 获取当前装备的武器类型
                // 当前 InventoryManager 没有直接暴露装备武器的类型查询
                // 这里用 WeaponMasterySystem 的当前武器类型作为替代

                var masterySystem = WeaponMasterySystem.Instance;
                if (masterySystem != null)
                {
                    var currentType = masterySystem.CurrentWeaponType;
                    // 检查该类型是否有共鸣配置
                    if (WeaponResonanceConfig.GetEffect(currentType) != null)
                    {
                        return currentType.ToString();
                    }
                }

                // TODO: 未来从 InventoryManager.Equipment[0] 获取实际武器Item，
                // 再获取 Weapon.WeaponType，转为字符串
                return "";
            }

            public string GetOffhandWeaponType()
            {
                // TODO: 从 InventoryManager.Equipment[1]（副手槽）获取
                // 当前无副手武器槽实现，返回空
                // 如实现双持系统，只需在此返回实际副手武器类型
                return "";
            }
        }
    }
}
