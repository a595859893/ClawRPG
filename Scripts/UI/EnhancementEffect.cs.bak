using Godot;
using System;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 装备强化特效系统
    /// </summary>
    public partial class EnhancementEffect : Control {
        // 特效节点
        private Control _effectContainer;
        private ColorRect _successOverlay;
        private ColorRect _failOverlay;
        private Label _resultLabel;
        private Control _particleContainer;
        
        // 动画
        private Tween _mainTween;
        
        // 颜色
        private Color ColorGold = new Color(1.0f, 0.84f, 0.0f);
        private Color ColorSuccess = new Color(0.2f, 1.0f, 0.4f, 0.3f);
        private Color ColorFail = new Color(1.0f, 0.3f, 0.3f, 0.3f);
        private Color ColorPurple = new Color(0.6f, 0.3f, 0.9f, 0.5f);
        
        public override void _Ready() {
            SetupEffectContainer();
            SetupOverlays();
            SetupParticleContainer();
            SetupResultLabel();
            
            Visible = false; 
        }
        
        private void SetupEffectContainer() {
            _effectContainer = new Control();
            _effectContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(_effectContainer);
        }
        
        private void SetupOverlays() {
            // 成功覆盖层
            _successOverlay = new ColorRect();
            _successOverlay.Color = ColorSuccess;
            _successOverlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _successOverlay.Visible = false; 
            _effectContainer.AddChild(_successOverlay);
            
            // 失败覆盖层
            _failOverlay = new ColorRect();
            _failOverlay.Color = ColorFail;
            _failOverlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _failOverlay.Visible = false; 
            _effectContainer.AddChild(_failOverlay);
        }
        
        private void SetupParticleContainer() {
            _particleContainer = new Control();
            _particleContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _effectContainer.AddChild(_particleContainer);
        }
        
        private void SetupResultLabel() {
            _resultLabel = new Label();
            _resultLabel.SetAnchorsPreset(Control.LayoutPreset.CenterBottom);
            _resultLabel.OffsetTop = -200;
            _resultLabel.OffsetBottom = -100;
            _resultLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _resultLabel.AddThemeFontSizeOverride("font_size", 48);
            _resultLabel.Modulate = ColorGold;
            _resultLabel.Visible = false; 
            _effectContainer.AddChild(_resultLabel);
        }
        
        /// <summary>
        /// 播放强化成功特效
        /// </summary>
        public void PlaySuccessEffect(int newLevel) {
            Visible = true;
            
            // 清理之前的tween
            if (_mainTween != null && _mainTween.IsValid()) {
                _mainTween.Kill();
            }
            _mainTween = CreateTween();
            _mainTween.SetParallel(false);
            
            // 重置状态
            _failOverlay.Visible = false; 
            _resultLabel.Visible = true;
            _resultLabel.Modulate = ColorGold;
            _resultLabel.Text = $"+{newLevel} 强化成功!";
            
            // 成功闪光
            _successOverlay.Modulate = new Color(ColorSuccess.R, ColorSuccess.G, ColorSuccess.B, 0);
            _successOverlay.Visible = true;
            _mainTween.TweenProperty(_successOverlay, "modulate:a", 0.5f, 0.1f);
            _mainTween.TweenProperty(_successOverlay, "modulate:a", 0f, 0.3f);
            
            // 播放金色粒子
            SpawnGoldParticles();
            
            // 震动效果
            ShakeScreen(5.0f, 0.2f);
            
            // 结果文字动画
            _resultLabel.Scale = new Vector2(0.5f, 0.5f);
            _mainTween.TweenProperty(_resultLabel, "scale", new Vector2(1.2f, 1.2f), 0.15f).SetTrans(Tween.TransitionType.Elastic);
            _mainTween.TweenProperty(_resultLabel, "scale", Vector2.One, 0.1f);
            
            // 延迟隐藏
            _mainTween.TweenInterval(1.5f);
            _mainTween.TweenCallback(() => {
                Visible = false; 
                ClearParticles();
            });
        }
        
        /// <summary>
        /// 播放强化失败特效
        /// </summary>
        public void PlayFailEffect(int newLevel) {
            Visible = true;
            
            // 清理之前的tween
            if (_mainTween != null && _mainTween.IsValid()) {
                _mainTween.Kill();
            }
            _mainTween = CreateTween();
            _mainTween.SetParallel(false);
            
            // 重置状态
            _successOverlay.Visible = false; 
            _resultLabel.Visible = true;
            _resultLabel.Modulate = new Color(1.0f, 0.5f, 0.5f);
            _resultLabel.Text = $"+{newLevel} 强化失败...";
            
            // 失败闪光
            _failOverlay.Modulate = new Color(ColorFail.R, ColorFail.G, ColorFail.B, 0);
            _failOverlay.Visible = true;
            _mainTween.TweenProperty(_failOverlay, "modulate:a", 0.4f, 0.15f);
            _mainTween.TweenProperty(_failOverlay, "modulate:a", 0f, 0.4f);
            
            // 播放灰色粒子
            SpawnGrayParticles();
            
            // 震动效果
            ShakeScreen(8.0f, 0.3f);
            
            // 结果文字动画 - 摇晃
            _resultLabel.Scale = Vector2.One;
            var shakeTween = CreateTween();
            for (int i = 0; i < 6; i++) {
                float offset = (i % 2 == 0) ? 10f : -10f;
                shakeTween.TweenProperty(_resultLabel, "offset_x", offset, 0.05f);
            }
            shakeTween.TweenProperty(_resultLabel, "offset_x", 0f, 0.05f);
            
            // 延迟隐藏
            _mainTween.TweenInterval(1.5f);
            _mainTween.TweenCallback(() => {
                Visible = false; 
                ClearParticles();
            });
        }
        
        /// <summary>
        /// 播放最大等级特效
        /// </summary>
        public void PlayMaxLevelEffect() {
            Visible = true;
            
            // 清理之前的tween
            if (_mainTween != null && _mainTween.IsValid()) {
                _mainTween.Kill();
            }
            _mainTween = CreateTween();
            _mainTween.SetParallel(false);
            
            // 重置状态
            _successOverlay.Visible = false; 
            _failOverlay.Visible = false; 
            _resultLabel.Visible = true;
            _resultLabel.Modulate = ColorPurple;
            _resultLabel.Text = "已达最大强化等级!";
            
            // 播放紫色光环粒子
            SpawnPurpleParticles();
            
            // 延迟隐藏
            _mainTween.TweenInterval(2.0f);
            _mainTween.TweenCallback(() => {
                Visible = false; 
                ClearParticles();
            });
        }
        
        /// <summary>
        /// 播放强化进行中动画
        /// </summary>
        public void PlayEnhancingAnimation() {
            Visible = true;
            
            // 清理之前的tween
            if (_mainTween != null && _mainTween.IsValid()) {
                _mainTween.Kill();
            }
            _mainTween = CreateTween();
            _mainTween.SetParallel(false);
            
            // 显示进行中
            _successOverlay.Visible = false; 
            _failOverlay.Visible = false; 
            _resultLabel.Visible = true;
            _resultLabel.Modulate = new Color(0.8f, 0.8f, 1.0f);
            _resultLabel.Text = "强化中...";
            
            // 闪烁效果
            _mainTween.TweenProperty(_resultLabel, "modulate:a", 0.3f, 0.3f);
            _mainTween.TweenProperty(_resultLabel, "modulate:a", 1.0f, 0.3f);
            _mainTween.TweenProperty(_resultLabel, "modulate:a", 0.3f, 0.3f);
            _mainTween.TweenProperty(_resultLabel, "modulate:a", 1.0f, 0.3f);
            
            // 旋转效果
            float startRotation = _resultLabel.Rotation;
            _mainTween.TweenProperty(_resultLabel, "rotation", startRotation + 0.2f, 0.6f).SetTrans(Tween.TransitionType.Sine);
            _mainTween.TweenProperty(_resultLabel, "rotation", startRotation - 0.2f, 0.6f).SetTrans(Tween.TransitionType.Sine);
            _mainTween.TweenProperty(_resultLabel, "rotation", startRotation, 0.3f);
        }
        
        private void SpawnGoldParticles() {
            var viewportSize = GetViewportRect().Size;
            Vector2 center = viewportSize / 2;
            
            for (int i = 0; i < 20; i++) {
                var particle = CreateParticle(ColorGold);
                _particleContainer.AddChild(particle);
                
                // 随机起始位置（中心周围）
                Vector2 startPos = center + new Vector2(
                    (float)GD.RandRange(-100, 100),
                    (float)GD.RandRange(-50, 50)
                );
                particle.Position = startPos;
                
                // 随机目标位置（向上散开）
                Vector2 targetPos = startPos + new Vector2(
                    (float)GD.RandRange(-200, 200),
                    (float)GD.RandRange(-300, -100)
                );
                
                // 动画
                var tween = CreateTween();
                tween.SetParallel(true);
                tween.TweenProperty(particle, "position", targetPos, 0.8f).SetTrans(Tween.TransitionType.Quad);
                tween.TweenProperty(particle, "modulate:a", 0f, 0.8f);
                tween.TweenProperty(particle, "scale", Vector2.Zero, 0.8f);
                tween.TweenCallback(() => {
                    if (IsInstanceValid(particle)) {
                        particle.QueueFree();
                    }
                });
                
                // 延迟产生
                tween.TweenInterval(i * 0.03f);
            }
            
            // 添加光环效果
            var halo = new ColorRect();
            halo.Color = new Color(1.0f, 0.9f, 0.5f, 0.4f);
            halo.CustomMinimumSize = new Vector2(200, 200);
            halo.Position = center - new Vector2(100, 100);
            _particleContainer.AddChild(halo);
            
            var haloTween = CreateTween();
            haloTween.TweenProperty(halo, "scale", new Vector2(1.5f, 1.5f), 0.5f);
            haloTween.TweenProperty(halo, "modulate:a", 0f, 0.5f);
            haloTween.TweenCallback(() => halo.QueueFree());
        }
        
        private void SpawnGrayParticles() {
            var viewportSize = GetViewportRect().Size;
            Vector2 center = viewportSize / 2;
            
            for (int i = 0; i < 15; i++) {
                var particle = CreateParticle(new Color(0.5f, 0.5f, 0.5f, 0.8f));
                _particleContainer.AddChild(particle);
                
                // 随机起始位置
                Vector2 startPos = center + new Vector2(
                    (float)GD.RandRange(-80, 80),
                    (float)GD.RandRange(-40, 40)
                );
                particle.Position = startPos;
                
                // 向下飘散
                Vector2 targetPos = startPos + new Vector2(
                    (float)GD.RandRange(-100, 100),
                    (float)GD.RandRange(100, 200)
                );
                
                // 动画
                var tween = CreateTween();
                tween.SetParallel(true);
                tween.TweenProperty(particle, "position", targetPos, 1.0f).SetTrans(Tween.TransitionType.Quad);
                tween.TweenProperty(particle, "modulate:a", 0f, 1.0f);
                tween.TweenCallback(() => {
                    if (IsInstanceValid(particle)) {
                        particle.QueueFree();
                    }
                });
                
                tween.TweenInterval(i * 0.04f);
            }
        }
        
        private void SpawnPurpleParticles() {
            var viewportSize = GetViewportRect().Size;
            Vector2 center = viewportSize / 2;
            
            // 紫色光环
            var halo = new ColorRect();
            halo.Color = ColorPurple;
            halo.CustomMinimumSize = new Vector2(150, 150);
            halo.Position = center - new Vector2(75, 75);
            halo.Modulate = new Color(1, 1, 1, 0);
            _particleContainer.AddChild(halo);
            
            var haloTween = CreateTween();
            haloTween.TweenProperty(halo, "modulate:a", 0.6f, 0.3f);
            haloTween.TweenProperty(halo, "scale", new Vector2(2.0f, 2.0f), 1.0f).SetTrans(Tween.TransitionType.Sine);
            haloTween.TweenProperty(halo, "modulate:a", 0f, 1.0f);
            haloTween.TweenCallback(() => halo.QueueFree());
            
            // 星星粒子
            for (int i = 0; i < 12; i++) {
                var angle = i * Mathf.Tau / 12;
                var star = CreateStar();
                star.Position = center;
                _particleContainer.AddChild(star);
                
                var targetPos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 150f;
                
                var tween = CreateTween();
                tween.TweenProperty(star, "position", targetPos, 0.6f).SetTrans(Tween.TransitionType.Back);
                tween.TweenProperty(star, "modulate:a", 0f, 0.6f);
                tween.TweenCallback(() => {
                    if (IsInstanceValid(star)) star.QueueFree();
                });
            }
        }
        
        private ColorRect CreateParticle(Color color) {
            var rect = new ColorRect();
            rect.Color = color;
            rect.CustomMinimumSize = new Vector2(8, 8);
            return rect;
        }
        
        private ColorRect CreateStar() {
            var rect = new ColorRect();
            rect.Color = ColorGold;
            rect.CustomMinimumSize = new Vector2(12, 12);
            return rect;
        }
        
        private void ShakeScreen(float intensity, float duration) {
            var viewport = GetViewport();
            if (viewport == null) return;
            
            var camera = viewport.GetCamera3d();
            if (camera == null) return;
            
            // 简单的屏幕震动（通过相机位置）
            Vector3 originalPos = camera.GlobalPosition;
            
            var shakeTween = CreateTween();
            for (int i = 0; i < 10; i++) {
                Vector3 offset = new Vector3(
                    (float)GD.RandRange(-intensity, intensity),
                    (float)GD.RandRange(-intensity, intensity),
                    0
                );
                shakeTween.TweenProperty(camera, "global_position", originalPos + offset, duration / 10);
            }
            shakeTween.TweenProperty(camera, "global_position", originalPos, 0.05f);
        }
        
        private void ClearParticles() {
            foreach (var child in _particleContainer.GetChildren()) {
                if (child is Control) {
                    child.QueueFree();
                }
            }
            _resultLabel.OffsetLeft = 0;
            _resultLabel.OffsetRight = 0;
        }
    }
}
