using System;
using System.Collections.Generic;
using Godot;

public partial class MomentumSystem : Node
{
    public static MomentumSystem Instance { get; private set; }
    
    [Signal]
    public void MomentumChanged(MomentumData.MomentumType type, MomentumData.MomentumState state, int level);
    
    [Signal]
    public void MomentumCharged(MomentumData.MomentumType type);
    
    [Signal]
    public void MomentumOvercharged(MomentumData.MomentumType type);
    
    [Signal]
    public void MomentumLost(MomentumData.MomentumType type);
    
    private MomentumData.PlayerMomentumData _playerData;
    private Dictionary<MomentumData.MomentumType, MomentumData.MomentumInstance> _activeMomenta;
    private bool _isInitialized = false;
    
    public override void _Ready()
    {
        Instance = this;
        _playerData = new MomentumData.PlayerMomentumData();
        _activeMomenta = new Dictionary<MomentumData.MomentumType, MomentumData.MomentumInstance>();
        InitializeMomenta();
        _isInitialized = true;
    }
    
    private void InitializeMomenta()
    {
        var types = Enum.GetValues(typeof(MomentumData.MomentumType));
        foreach (MomentumData.MomentumType type in types)
        {
            var config = MomentumDatabase.Instance.GetConfig(type);
            if (config != null)
            {
                var instance = new MomentumData.MomentumInstance
                {
                    Type = type,
                    State = MomentumData.MomentumState.Neutral,
                    Level = 0,
                    Charge = 0f,
                    MaxCharge = config.MaxCharge,
                    DecayRate = config.DecayRate,
                    Multiplier = 1.0f,
                    ConsecutiveKills = 0,
                    LastKillTime = DateTime.MinValue
                };
                _activeMomenta[type] = instance;
                _playerData.ActiveMomenta[type] = instance;
            }
        }
    }
    
    public override void _Process(double delta)
    {
        if (!_isInitialized) return;
        
        float dt = (float)delta;
        
        foreach (var kvp in _activeMomenta)
        {
            var momentum = kvp.Value;
            var config = MomentumDatabase.Instance.GetConfig(momentum.Type);
            if (config == null) continue;
            
            // Charge from passive generation
            if (config.ChargePerSecond > 0 && momentum.Charge < momentum.MaxCharge)
            {
                momentum.Charge = Mathf.Min(momentum.Charge + config.ChargePerSecond * dt, momentum.MaxCharge);
            }
            
            // Check for decay (if no kills recently)
            if (momentum.ConsecutiveKills > 0)
            {
                var timeSinceLastKill = (DateTime.Now - momentum.LastKillTime).TotalSeconds;
                if (timeSinceLastKill > 3.0) // 3 seconds window
                {
                    // Start fading
                    if (momentum.State != MomentumData.MomentumState.Fading)
                    {
                        momentum.State = MomentumData.MomentumState.Fading;
                        EmitSignal(nameof(MomentumChanged), momentum.Type, momentum.State, momentum.Level);
                    }
                    
                    // Decay charge
                    momentum.Charge = Mathf.Max(momentum.Charge - momentum.DecayRate * dt * 2, 0);
                    momentum.ConsecutiveKills = 0;
                    
                    // Reset if fully decayed
                    if (momentum.Charge <= 0)
                    {
                        ResetMomentum(momentum);
                        _playerData.MomentumLostToDecay++;
                        EmitSignal(nameof(MomentumLost), momentum.Type);
                    }
                }
            }
            
            // Update state based on charge level
            UpdateMomentumState(momentum);
        }
    }
    
    private void UpdateMomentumState(MomentumData.MomentumInstance momentum)
    {
        var config = MomentumDatabase.Instance.GetConfig(momentum.Type);
        if (config == null) return;
        
        var oldState = momentum.State;
        var chargePercent = momentum.Charge / momentum.MaxCharge;
        
        if (chargePercent >= 0.9f && momentum.Level >= config.MaxLevel - 1)
        {
            if (momentum.State != MomentumData.MomentumState.Overcharged)
            {
                momentum.State = MomentumData.MomentumState.Overcharged;
                momentum.Level = config.MaxLevel;
                _playerData.OverchargeCount++;
                EmitSignal(nameof(MomentumOvercharged), momentum.Type);
            }
        }
        else if (chargePercent >= 0.6f)
        {
            momentum.State = MomentumData.MomentumState.Charged;
            momentum.Level = Mathf.FloorToInt(chargePercent * config.MaxLevel);
        }
        else if (chargePercent >= 0.3f)
        {
            momentum.State = MomentumData.MomentumState.Building;
            momentum.Level = Mathf.FloorToInt(chargePercent * config.MaxLevel);
        }
        else if (chargePercent > 0)
        {
            momentum.State = MomentumData.MomentumState.Building;
            momentum.Level = 0;
        }
        
        // Update multiplier based on state
        momentum.Multiplier = GetMultiplier(momentum.Type, momentum.State);
        
        if (oldState != momentum.State)
        {
            EmitSignal(nameof(MomentumChanged), momentum.Type, momentum.State, momentum.Level);
        }
    }
    
    public void OnEnemyKilled()
    {
        foreach (var kvp in _activeMomenta)
        {
            var momentum = kvp.Value;
            var config = MomentumDatabase.Instance.GetConfig(momentum.Type);
            if (config == null) continue;
            
            momentum.Charge = Mathf.Min(momentum.Charge + config.ChargePerKill, momentum.MaxCharge);
            momentum.ConsecutiveKills++;
            momentum.LastKillTime = DateTime.Now;
            
            _playerData.TotalMomentumGained++;
            if (momentum.Level > _playerData.MaxMomentumReached)
            {
                _playerData.MaxMomentumReached = momentum.Level;
            }
            
            UpdateMomentumState(momentum);
        }
    }
    
    public void OnDamageDealt(float damage)
    {
        foreach (var kvp in _activeMomenta)
        {
            var momentum = kvp.Value;
            if (momentum.Type == MomentumData.MomentumType.Attack || 
                momentum.Type == MomentumData.MomentumType.Critical)
            {
                var config = MomentumDatabase.Instance.GetConfig(momentum.Type);
                if (config == null) continue;
                
                momentum.Charge = Mathf.Min(momentum.Charge + config.ChargePerHit, momentum.MaxCharge);
                momentum.LastKillTime = DateTime.Now;
                UpdateMomentumState(momentum);
            }
        }
    }
    
    public void OnDamageTaken(float damage)
    {
        if (_activeMomenta.ContainsKey(MomentumData.MomentumType.Defense))
        {
            var momentum = _activeMomenta[MomentumData.MomentumType.Defense];
            var config = MomentumDatabase.Instance.GetConfig(momentum.Type);
            if (config == null) return;
            
            momentum.Charge = Mathf.Min(momentum.Charge + config.ChargePerHit, momentum.MaxCharge);
            momentum.LastKillTime = DateTime.Now;
            UpdateMomentumState(momentum);
        }
    }
    
    private void ResetMomentum(MomentumData.MomentumInstance momentum)
    {
        momentum.State = MomentumData.MomentumState.Neutral;
        momentum.Level = 0;
        momentum.Charge = 0f;
        momentum.Multiplier = 1.0f;
        momentum.ConsecutiveKills = 0;
    }
    
    public float GetMultiplier(MomentumData.MomentumType type, MomentumData.MomentumState state)
    {
        return MomentumDatabase.Instance.GetStateMultiplier(type, state, "multiplier");
    }
    
    public float GetAttributeMultiplier(MomentumData.MomentumType type, string attribute)
    {
        if (!_activeMomenta.ContainsKey(type)) return 1.0f;
        
        var momentum = _activeMomenta[type];
        var config = MomentumDatabase.Instance.GetConfig(type);
        if (config == null || !config.AttributeBonuses.ContainsKey(attribute)) return 1.0f;
        
        return 1.0f + (config.AttributeBonuses[attribute] * momentum.Level);
    }
    
    public float GetDamageMultiplier()
    {
        if (!_activeMomenta.ContainsKey(MomentumData.MomentumType.Attack)) return 1.0f;
        return _activeMomenta[MomentumData.MomentumType.Attack].Multiplier;
    }
    
    public float GetDefenseMultiplier()
    {
        if (!_activeMomenta.ContainsKey(MomentumData.MomentumType.Defense)) return 1.0f;
        return _activeMomenta[MomentumData.MomentumType.Defense].Multiplier;
    }
    
    public float GetAttackSpeedMultiplier()
    {
        if (!_activeMomenta.ContainsKey(MomentumData.MomentumType.Speed)) return 1.0f;
        return _activeMomenta[MomentumData.MomentumType.Speed].Multiplier;
    }
    
    public float GetLuckMultiplier()
    {
        if (!_activeMomenta.ContainsKey(MomentumData.MomentumType.Luck)) return 1.0f;
        return _activeMomenta[MomentumData.MomentumType.Luck].Multiplier;
    }
    
    public float GetCritDamageMultiplier()
    {
        if (!_activeMomenta.ContainsKey(MomentumData.MomentumType.Critical)) return 1.0f;
        return _activeMomenta[MomentumData.MomentumType.Critical].Multiplier;
    }
    
    public Dictionary<MomentumData.MomentumType, MomentumData.MomentumInstance> GetAllMomenta()
    {
        return new Dictionary<MomentumData.MomentumType, MomentumData.MomentumInstance>(_activeMomenta);
    }
    
    public MomentumData.PlayerMomentumData GetStatistics()
    {
        return _playerData;
    }
    
    public void Save(Dictionary<string, Variant> saveData)
    {
        var momentumData = new Dictionary<string, Variant>();
        
        foreach (var kvp in _activeMomenta)
        {
            var key = kvp.Key.ToString();
            var instance = kvp.Value;
            momentumData[key] = new Dictionary<string, Variant>
            {
                { "charge", instance.Charge },
                { "level", instance.Level },
                { "state", (int)instance.State },
                { "consecutiveKills", instance.ConsecutiveKills }
            };
        }
        
        saveData["momentum_system"] = momentumData;
    }
    
    public void Load(Dictionary<string, Variant> saveData)
    {
        if (!saveData.ContainsKey("momentum_system")) return;
        
        var momentumData = (Dictionary<string, Variant>)saveData["momentum_system"];
        
        foreach (var kvp in momentumData)
        {
            if (Enum.TryParse<MomentumData.MomentumType>(kvp.Key, out var type) && 
                _activeMomenta.ContainsKey(type))
            {
                var data = (Dictionary<string, Variant>)kvp.Value;
                var instance = _activeMomenta[type];
                instance.Charge = (float)data["charge"];
                instance.Level = (int)data["level"];
                instance.State = (MomentumData.MomentumState)(int)data["state"];
                instance.ConsecutiveKills = (int)data["consecutiveKills"];
                instance.Multiplier = GetMultiplier(type, instance.State);
            }
        }
    }
}
