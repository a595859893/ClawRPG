using Godot;
using System;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 相机特效设置UI - 允许玩家配置相机效果参数
    /// </summary>
    public partial class CameraEffectUI : Control {
        public static CameraEffectUI Instance { get; private set; }

        [Export] private CheckBox enableDynamicFOVCheck;
        [Export] private CheckBox enableShakeCheck;
        [Export] private CheckBox enableVignetteCheck;
        [Export] private HSlider fovSlider;
        [Export] private HSlider shakeSlider;
        [Export] private HSlider vignetteSlider;
        [Export] private Label fovValueLabel;
        [Export] private Label shakeValueLabel;
        [Export] private Label vignetteValueLabel;
        
        private bool isVisible = false; 
        
        // 设置数据
        private bool enableDynamicFOV = true;
        private bool enableShake = true;
        private bool enableVignette = false; 
        private float fovIntensity = 0.5f;
        private float shakeIntensity = 0.5f;
        private float vignetteIntensity = 0.3f;
        
        public override void _Ready() {
            Instance = this;
            Visible = false; 
            
            // 尝试自动获取UI元素
            TryAutoConnect();
        }
        
        private void TryAutoConnect() {
            // 查找子节点
            foreach (var child in GetChildren()) {
                if (child is CheckBox checkBox) {
                    string name = child.Name.ToString().ToLower();
                    if (name.ContainsKey("dynamicfov") || name.ContainsKey("fov")) {
                        enableDynamicFOVCheck = checkBox;
                    } else if (name.ContainsKey("shake")) {
                        enableShakeCheck = checkBox;
                    } else if (name.ContainsKey("vignette")) {
                        enableVignetteCheck = checkBox;
                    }
                } else if (child is HSlider slider) {
                    string name = child.Name.ToString().ToLower();
                    if (name.ContainsKey("fov")) {
                        fovSlider = slider;
                    } else if (name.ContainsKey("shake")) {
                        shakeSlider = slider;
                    } else if (name.ContainsKey("vignette")) {
                        vignetteSlider = slider;
                    }
                } else if (child is Label label) {
                    string name = child.Name.ToString().ToLower();
                    if (name.ContainsKey("fov")) {
                        fovValueLabel = label;
                    } else if (name.ContainsKey("shake")) {
                        shakeValueLabel = label;
                    } else if (name.ContainsKey("vignette")) {
                        vignetteValueLabel = label;
                    }
                }
            }
            
            // 连接信号
            if (enableDynamicFOVCheck != null) {
                enableDynamicFOVCheck.Toggled += OnDynamicFOVToggled;
            }
            if (enableShakeCheck != null) {
                enableShakeCheck.Toggled += OnShakeToggled;
            }
            if (enableVignetteCheck != null) {
                enableVignetteCheck.Toggled += OnVignetteToggled;
            }
            if (fovSlider != null) {
                fovSlider.ValueChanged += OnFOVSliderChanged;
            }
            if (shakeSlider != null) {
                shakeSlider.ValueChanged += OnShakeSliderChanged;
            }
            if (vignetteSlider != null) {
                vignetteSlider.ValueChanged += OnVignetteSliderChanged;
            }
            
            // 初始化滑块值
            // 初始化滑块值
            if (fovSlider != null) fovSlider.Value = fovIntensity;
            if (shakeSlider != null) shakeSlider.Value = shakeIntensity;
            if (vignetteSlider != null) vignetteSlider.Value = vignetteIntensity;
            
            UpdateLabels();
        }
        
        public override void _Input(InputEvent eventArgs) {
            if (eventArgs.IsActionPressed("ui_cancel") && isVisible) {
                Toggle();
            }
        }
        
        /// <summary>
        /// 切换UI显示
        /// </summary>
        public void Toggle() {
            isVisible = !isVisible;
            Visible = isVisible;
            
            if (isVisible) {
                GetTree().Paused = true;
            } else {
                GetTree().Paused = false; 
            }
        }
        
        /// <summary>
        /// 显示UI
        /// </summary>
        public void Show() {
            if (!isVisible) {
                Toggle();
            }
        }
        
        /// <summary>
        /// 隐藏UI
        /// </summary>
        public void Hide() {
            if (isVisible) {
                Toggle();
            }
        }
        
        private void OnDynamicFOVToggled(bool toggled) {
            enableDynamicFOV = toggled;
            ApplySettings();
        }
        
        private void OnShakeToggled(bool toggled) {
            enableShake = toggled;
            ApplySettings();
        }
        
        private void OnVignetteToggled(bool toggled) {
            enableVignette = toggled;
            ApplySettings();
        }
        
        private void OnFOVSliderChanged(float value) {
            fovIntensity = value;
            UpdateLabels();
            ApplySettings();
        }
        
        private void OnShakeSliderChanged(float value) {
            shakeIntensity = value;
            UpdateLabels();
            ApplySettings();
        }
        
        private void OnVignetteSliderChanged(float value) {
            vignetteIntensity = value;
            UpdateLabels();
            ApplySettings();
        }
        
        private void UpdateLabels() {
            if (fovValueLabel != null) {
                fovValueLabel.Text = $"{(int)(fovIntensity * 100)}%";
            }
            if (shakeValueLabel != null) {
                shakeValueLabel.Text = $"{(int)(shakeIntensity * 100)}%";
            }
            if (vignetteValueLabel != null) {
                vignetteValueLabel.Text = $"{(int)(vignetteIntensity * 100)}%";
            }
        }
        
        private void ApplySettings() {
            var cameraSystem = CameraEffectSystem.Instance;
            if (cameraSystem == null) return;
            
            // 应用设置（这里可以保存到设置文件）
        }
        
        /// <summary>
        /// 获取设置数据（用于存档）
        /// </summary>
        public Dictionary GetSettingsData() {
            return new Dictionary {
                { "enableDynamicFOV", enableDynamicFOV },
                { "enableShake", enableShake },
                { "enableVignette", enableVignette },
                { "fovIntensity", fovIntensity },
                { "shakeIntensity", shakeIntensity },
                { "vignetteIntensity", vignetteIntensity }
            };
        }
        
        /// <summary>
        /// 加载设置数据（用于读档）
        /// </summary>
        public void LoadSettingsData(Dictionary data) {
            if (data.ContainsKey("enableDynamicFOV")) {
                enableDynamicFOV = (bool)data["enableDynamicFOV"];
                if (enableDynamicFOVCheck != null) enableDynamicFOVCheck.ButtonPressed = enableDynamicFOV;
            }
            if (data.ContainsKey("enableShake")) {
                enableShake = (bool)data["enableShake"];
                if (enableShakeCheck != null) enableShakeCheck.ButtonPressed = enableShake;
            }
            if (data.ContainsKey("enableVignette")) {
                enableVignette = (bool)data["enableVignette"];
                if (enableVignetteCheck != null) enableVignetteCheck.ButtonPressed = enableVignette;
            }
            if (data.ContainsKey("fovIntensity")) {
                fovIntensity = (float)data["fovIntensity"];
                if (fovSlider != null) fovSlider.Value = fovIntensity;
            }
            if (data.ContainsKey("shakeIntensity")) {
                shakeIntensity = (float)data["shakeIntensity"];
                if (shakeSlider != null) shakeSlider.Value = shakeIntensity;
            }
            if (data.ContainsKey("vignetteIntensity")) {
                vignetteIntensity = (float)data["vignetteIntensity"];
                if (vignetteSlider != null) vignetteSlider.Value = vignetteIntensity;
            }
            
            UpdateLabels();
        }
    }
}
