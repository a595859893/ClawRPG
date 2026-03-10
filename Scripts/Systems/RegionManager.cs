using Godot;
using System;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Manages region transitions and environmental effects
    /// </summary>
    [GodotClass]
    public class RegionManager : Node
    {
        public static RegionManager Instance { get; private set; }

        [Signal]
        public signal void RegionChanged(string newRegionId, string regionName);

        [Signal]
        public signal void EnvironmentalDamage(float damage);

        private RegionDatabase _regionDb;
        private Player _player;
        private string _currentRegionId = "forest";
        private float _environmentTimer = 0f;

        public string CurrentRegionId => _currentRegionId;
        public RegionType CurrentRegion => _regionDb?.GetRegion(_currentRegionId);

        public override void _Ready()
        {
            Instance = this;
            _regionDb = RegionDatabase.Instance;
            
            // Wait for player to be ready
            CallDeferred(nameof(InitializePlayer));
        }

        private async void InitializePlayer()
        {
            // Wait for player to be available
            await ToSignal(GetTree(), "process_frame");
            await ToSignal(GetTree(), "process_frame");
            
            _player = GetTree().GetFirstNodeInGroup("player") as Player;
            if (_player == null)
            {
                GD.Warning("[RegionManager] Player not found, retrying...");
                CallDeferred(nameof(InitializePlayer));
                return;
            }
            
            GD.Print($"[RegionManager] Initialized with player: {_player.Name}");
            UpdateRegionEffects();
        }

        public override void _Process(float delta)
        {
            if (_player == null || CurrentRegion == null)
                return;

            // Handle environmental damage
            float envDamage = CurrentRegion.EnvironmentalDamagePerSecond;
            if (envDamage > 0)
            {
                _environmentTimer += delta;
                if (_environmentTimer >= 1.0f)
                {
                    _environmentTimer = 0f;
                    ApplyEnvironmentalDamage(envDamage);
                }
            }
        }

        private void ApplyEnvironmentalDamage(float damage)
        {
            if (_player == null)
                return;

            // Apply damage based on region type
            float finalDamage = damage;
            
            if (CurrentRegion.HasPoisonFog)
            {
                // Poison fog damage
                _player.ApplyStatusEffect("poison", 5f, 3f);
            }
            
            if (CurrentRegion.HasFireDamage)
            {
                // Direct fire damage
                _player.TakeDamage(Mathf.RoundToInt(finalDamage), Vector2.Zero);
                EmitSignal(EnvironmentalDamage.Name, finalDamage);
                GD.Print($"[RegionManager] Fire damage: {finalDamage}");
            }
            
            if (CurrentRegion.HasIceDamage)
            {
                // Ice damage and slow
                _player.ApplyStatusEffect("frozen", 2f, 1f);
                _player.ApplyStatusEffect("slow", 0.5f, 2f);
                EmitSignal(EnvironmentalDamage.Name, finalDamage);
            }
        }

        public void ChangeRegion(string regionId)
        {
            if (_regionDb == null)
            {
                GD.Warning("[RegionManager] RegionDatabase not initialized");
                return;
            }

            var newRegion = _regionDb.GetRegion(regionId);
            if (newRegion == null)
            {
                GD.Warning($"[RegionManager] Invalid region: {regionId}");
                return;
            }

            // Check level requirement
            if (_player != null && newRegion.RequiredLevel > _player.Level)
            {
                GD.Warning($"[RegionManager] Player level too low. Required: {newRegion.RequiredLevel}, Current: {_player.Level}");
                return;
            }

            string oldRegionId = _currentRegionId;
            _currentRegionId = regionId;

            GD.Print($"[RegionManager] Region changed: {oldRegionId} -> {regionId}");
            
            EmitSignal(RegionChanged.Name, regionId, newRegion.RegionName);
            UpdateRegionEffects();
        }

        private void UpdateRegionEffects()
        {
            if (CurrentRegion == null)
                return;

            // Apply region multipliers to player if needed
            GD.Print($"[RegionManager] Entered region: {CurrentRegion.RegionName} (Lv.{CurrentRegion.RequiredLevel})");
            GD.Print($"  - Damage: x{CurrentRegion.DamageMultiplier}, Defense: x{CurrentRegion.DefenseMultiplier}");
            GD.Print($"  - EXP: x{CurrentRegion.ExpMultiplier}, Drop: x{CurrentRegion.DropRateMultiplier}");
            
            if (CurrentRegion.HasPoisonFog)
                GD.Print($"  - Environmental: Poison Fog ({CurrentRegion.EnvironmentalDamagePerSecond} DPS)");
            if (CurrentRegion.HasFireDamage)
                GD.Print($"  - Environmental: Fire Damage ({CurrentRegion.EnvironmentalDamagePerSecond} DPS)");
            if (CurrentRegion.HasIceDamage)
                GD.Print($"  - Environmental: Ice Damage ({CurrentRegion.EnvironmentalDamagePerSecond} DPS)");
        }

        public RegionType GetCurrentRegion()
        {
            return CurrentRegion;
        }

        public float GetExpMultiplier()
        {
            return CurrentRegion?.ExpMultiplier ?? 1.0f;
        }

        public float GetDropRateMultiplier()
        {
            return CurrentRegion?.DropRateMultiplier ?? 1.0f;
        }

        public float GetDamageMultiplier()
        {
            return CurrentRegion?.DamageMultiplier ?? 1.0f;
        }

        public float GetDefenseMultiplier()
        {
            return CurrentRegion?.DefenseMultiplier ?? 1.0f;
        }

        public bool CanAccessRegion(string regionId, int playerLevel)
        {
            var region = _regionDb?.GetRegion(regionId);
            if (region == null)
                return false;
            return region.RequiredLevel <= playerLevel;
        }

        public string[] GetAvailableRegions(int playerLevel)
        {
            return _regionDb?.GetUnlockedRegionIds(playerLevel) ?? Array.Empty<string>();
        }
    }
}
