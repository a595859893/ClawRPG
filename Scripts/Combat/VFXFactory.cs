using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Combat {
    /// <summary>
    /// VFX Factory - Creates and configures visual effects
    /// Part of CombatVFXSystem refactoring
    /// </summary>
    public partial class VFXFactory : BaseSystem
    {
        private CombatVFXSystem _vfxSystem;
        
        public VFXFactory(CombatVFXSystem vfxSystem)
        {
            _vfxSystem = vfxSystem;
        }
        
        /// <summary>
        /// Create damage number visual
        /// </summary>
        public Label CreateDamageNumberUI(DamageNumber dn)
        {
            var label = new Label();
            string text;
            
            if (dn.Type == DamageNumberType.Miss || dn.Type == DamageNumberType.Dodge) {
                text = "MISS";
            } else {
                text = Mathf.RoundToInt(dn.Value).ToString();
            }
            
            label.Text = text;
            label.AddThemeFontSizeOverride("font_size", (int)CombatVFXDatabase.DamageNumberSizes[dn.Type]);
            label.Modulate = CombatVFXDatabase.DamageNumberColors[dn.Type];
            
            // Set position
            Vector2 screenPos = WorldToScreen(dn.Position);
            label.Position = screenPos;
            
            // Add shadow effect
            label.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.5f));
            label.AddThemeConstantOverride("font_shadow_size", 2);
            
            return label;
        }
        
        /// <summary>
        /// Create VFX visual (particle effect)
        /// </summary>
        public MeshInstance3D CreateVFXVisual(VFXInstance vfx)
        {
            var meshInstance = new MeshInstance3D();
            var sphere = new SphereMesh();
            sphere.Radius = 0.3f * vfx.Scale;
            sphere.Height = 0.6f * vfx.Scale;
            meshInstance.Mesh = sphere;
            
            // Create emissive material
            var material = new StandardMaterial3D();
            material.AlbedoColor = vfx.Color;
            material.EmissionEnabled = true;
            material.Emission = vfx.Color;
            material.EmissionEnergyMultiplier = 2f;
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            material.AlbedoColor = new Color(vfx.Color.R, vfx.Color.G, vfx.Color.B, 0.8f);
            meshInstance.MaterialOverride = material;
            
            meshInstance.Position = vfx.Position;
            
            return meshInstance;
        }
        
        /// <summary>
        /// Create screen effect overlay
        /// </summary>
        public ColorRect CreateScreenEffectOverlay(ScreenEffect effect)
        {
            var colorRect = new ColorRect();
            colorRect.Color = effect.Color;
            colorRect.Color.A = effect.Intensity;
            colorRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            colorRect.MouseFilter = Control.MouseFilterEnum.Ignore;
            
            return colorRect;
        }
        
        /// <summary>
        /// Create combo effect UI
        /// </summary>
        public Label CreateComboUI(ComboEffect effect)
        {
            var label = new Label();
            label.Text = $"{effect.ComboCount} COMBO!";
            label.AddThemeFontSizeOverride("font_size", (int)CombatVFXDatabase.GetComboSize(effect.ComboCount));
            label.Modulate = CombatVFXDatabase.GetComboColor(effect.ComboCount);
            
            // Set position (above screen center)
            Vector2 screenPos = WorldToScreen(effect.Position);
            screenPos.y -= 100;
            label.Position = screenPos;
            
            // Add shadow
            label.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.7f));
            label.AddThemeConstantOverride("font_shadow_size", 3);
            
            return label;
        }
        
        /// <summary>
        /// Create critical glow visual
        /// </summary>
        public MeshInstance3D CreateCriticalGlowVisual(CriticalGlow glow)
        {
            if (glow.Target == null || !IsInstanceValid(glow.Target)) return null;
            
            var meshInstance = new MeshInstance3D();
            var box = new BoxMesh();
            box.Size = new Vector3(1.5f, 1.5f, 1.5f);
            meshInstance.Mesh = box;
            
            var material = new StandardMaterial3D();
            material.AlbedoColor = glow.GlowColor;
            material.EmissionEnabled = true;
            material.Emission = glow.GlowColor;
            material.EmissionEnergyMultiplier = glow.Intensity;
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            material.AlbedoColor = new Color(glow.GlowColor.R, glow.GlowColor.G, glow.GlowColor.B, 0.3f);
            meshInstance.MaterialOverride = material;
            
            meshInstance.Position = glow.Target.Position;
            
            return meshInstance;
        }
        
        /// <summary>
        /// Convert world position to screen coordinates
        /// </summary>
        private Vector2 WorldToScreen(Vector3 worldPos)
        {
            var camera = _vfxSystem?.GetMainCamera();
            if (camera == null) return Vector2.Zero;
            
            var screenPos = camera.UnprojectPosition(worldPos);
            return new Vector2(screenPos.x, screenPos.y);
        }
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            // No persistent data needed
        }
    }
}
