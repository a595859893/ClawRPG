using Godot;
using System;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 相机特效增强系统 - 提供动态FOV、镜头震动、渐晕等视觉增强
    /// </summary>
    public partial class CameraEffectSystem : BaseSystem {
        public static CameraEffectSystem Instance { get; private set; }

        [Export] private Camera3D playerCamera;
        [Export] private float defaultFOV = 75f;
        [Export] private float maxFOV = 90f;
        [Export] private float fovTransitionSpeed = 5f;
        
        // 镜头震动参数
        [Export] private float shakeIntensity = 0f;
        [Export] private float shakeDuration = 0f;
        [Export] private float shakeFrequency = 30f;
        private Vector3 originalCameraOffset;
        private float currentShakeTime = 0f;
        
        // 渐晕效果
        [Export] private Color vignetteColor = new Color(0, 0, 0, 0.3f);
        [Export] private float vignetteIntensity = 0f;
        [Export] private float targetVignetteIntensity = 0f;
        
        // 动态FOV
        private float currentTargetFOV;
        private float velocityFOV = 0f;
        
        public override void _Ready() {
            Instance = this;
            currentTargetFOV = defaultFOV;
            
            // 尝试自动获取相机
            if (playerCamera == null) {
                var player = GetTree().GetFirstNodeInGroup("Player");
                if (player != null) {
                    playerCamera = player.GetNode<Camera3D>("Camera3D");
                }
            }
        }
        
        /// <summary>
        /// 系统名称
        /// </summary>
        protected override string SystemName => "CameraEffect";
        
        public override void _Ready() {
            Instance = this;
            currentTargetFOV = defaultFOV;
            
            // 尝试自动获取相机
            if (playerCamera == null) {
                var player = GetTree().GetFirstNodeInGroup("Player");
                if (player != null) {
                    playerCamera = player.GetNode<Camera3D>("Camera3D");
                }
            }
            
            if (playerCamera != null) {
                originalCameraOffset = playerCamera.Position;
                playerCamera.Fov = defaultFOV;
            }
        }
        
        public override void _Process(double delta) {
            float dt = (float)delta;
            
            // 更新动态FOV
            UpdateDynamicFOV(dt);
            
            // 更新镜头震动
            UpdateShake(dt);
            
            // 更新渐晕效果
            UpdateVignette(dt);
        }
        
        /// <summary>
        /// 根据玩家速度动态调整FOV
        /// </summary>
        private void UpdateDynamicFOV(float dt) {
            var player = GetTree().GetFirstNodeInGroup("Player");
            if (player == null) return;
            
            // 获取玩家速度（通过Player脚本）
            float playerSpeed = 0f;
            if (player is Characters.Player p) {
                playerSpeed = p.Velocity.Length();
            }
            
            // 根据速度计算目标FOV
            float speedRatio = Mathf.Clamp(playerSpeed / 400f, 0f, 1f);
            float targetFOV = Mathf.Lerp(defaultFOV, maxFOV, speedRatio);
            
            // 平滑过渡
            currentTargetFOV = Mathf.MoveToward(currentTargetFOV, targetFOV, fovTransitionSpeed * dt);
            
            if (playerCamera != null) {
                playerCamera.Fov = Mathf.Lerp(playerCamera.Fov, currentTargetFOV, dt * 3f);
            }
        }
        
        /// <summary>
        /// 更新镜头震动效果
        /// </summary>
        private void UpdateShake(float dt) {
            if (shakeDuration <= 0f) {
                if (playerCamera != null && playerCamera.Position != originalCameraOffset) {
                    playerCamera.Position = originalCameraOffset;
                }
                return;
            }
            
            currentShakeTime += dt;
            float progress = currentShakeTime / shakeDuration;
            
            if (progress >= 1f) {
                shakeDuration = 0f;
                if (playerCamera != null) {
                    playerCamera.Position = originalCameraOffset;
                }
                return;
            }
            
            // 衰减震动强度
            float intensity = shakeIntensity * (1f - progress);
            float x = Mathf.Sin(currentShakeTime * shakeFrequency) * intensity;
            float y = Mathf.Cos(currentShakeTime * shakeFrequency * 1.3f) * intensity;
            
            if (playerCamera != null) {
                playerCamera.Position = originalCameraOffset + new Vector3(x, y, 0);
            }
        }
        
        /// <summary>
        /// 更新渐晕效果
        /// </summary>
        private void UpdateVignette(float dt) {
            // 平滑过渡渐晕强度
            vignetteIntensity = Mathf.MoveToward(vignetteIntensity, targetVignetteIntensity, dt * 2f);
        }
        
        /// <summary>
        /// 触发镜头震动
        /// </summary>
        /// <param name="intensity">震动强度</param>
        /// <param name="duration">持续时间(秒)</param>
        /// <param name="frequency">震动频率</param>
        public void TriggerShake(float intensity = 0.5f, float duration = 0.3f, float frequency = 30f) {
            shakeIntensity = intensity;
            shakeDuration = duration;
            shakeFrequency = frequency;
            currentShakeTime = 0f;
        }
        
        /// <summary>
        /// 触发轻度震动
        /// </summary>
        public void TriggerLightShake() {
            TriggerShake(0.2f, 0.15f, 40f);
        }
        
        /// <summary>
        /// 触发中度震动
        /// </summary>
        public void TriggerMediumShake() {
            TriggerShake(0.4f, 0.3f, 30f);
        }
        
        /// <summary>
        /// 触发强力震动
        /// </summary>
        public void TriggerHeavyShake() {
            TriggerShake(0.8f, 0.5f, 25f);
        }
        
        /// <summary>
        /// 触发剧烈震动（用于Boss攻击）
        /// </summary>
        public void TriggerViolentShake() {
            TriggerShake(1.2f, 0.7f, 20f);
        }
        
        /// <summary>
        /// 设置渐晕强度
        /// </summary>
        /// <param name="intensity">目标强度(0-1)</param>
        public void SetVignette(float intensity) {
            targetVignetteIntensity = Mathf.Clamp(intensity, 0f, 1f);
        }
        
        /// <summary>
        /// 渐晕效果 - 进入战斗时
        /// </summary>
        public void EnableCombatVignette() {
            SetVignette(0.4f);
        }
        
        /// <summary>
        /// 渐晕效果 - 退出战斗时
        /// </summary>
        public void DisableCombatVignette() {
            SetVignette(0f);
        }
        
        /// <summary>
        /// 低血量渐晕效果
        /// </summary>
        public void EnableLowHealthVignette() {
            SetVignette(0.6f);
        }
        
        /// <summary>
        /// 重置所有效果
        /// </summary>
        public void ResetAllEffects() {
            shakeDuration = 0f;
            SetVignette(0f);
            currentTargetFOV = defaultFOV;
            
            if (playerCamera != null) {
                playerCamera.Fov = defaultFOV;
                playerCamera.Position = originalCameraOffset;
            }
        }
        
        /// <summary>
        /// 快速移动特效 - 冲刺时FOV增加
        /// </summary>
        public void TriggerSprintEffect() {
            if (playerCamera != null) {
                playerCamera.Fov = maxFOV;
            }
        }
        
        /// <summary>
        /// 停止快速移动特效
        /// </summary>
        public void EndSprintEffect() {
            currentTargetFOV = defaultFOV;
        }

        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["default_fov"] = defaultFOV;
            data["max_fov"] = maxFOV;
            data["fov_transition_speed"] = fovTransitionSpeed;
            data["shake_intensity"] = shakeIntensity;
            data["shake_duration"] = shakeDuration;
            data["vignette_color"] = vignetteColor.ToHtml();
            data["vignette_intensity"] = vignetteIntensity;
            return data;
        }

        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            if (data.Contains("default_fov")) defaultFOV = (float)data["default_fov"];
            if (data.Contains("max_fov")) maxFOV = (float)data["max_fov"];
            if (data.Contains("fov_transition_speed")) fovTransitionSpeed = (float)data["fov_transition_speed"];
            if (data.Contains("shake_intensity")) shakeIntensity = (float)data["shake_intensity"];
            if (data.Contains("shake_duration")) shakeDuration = (float)data["shake_duration"];
            if (data.Contains("vignette_color")) vignetteColor = Color.FromHtml((string)data["vignette_color"]);
            if (data.Contains("vignette_intensity")) vignetteIntensity = (float)data["vignette_intensity"];
        }
    }
}
