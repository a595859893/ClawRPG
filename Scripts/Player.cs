using Godot;
using System.Collections.Generic;
using ClawRPG.Systems;

public partial class Player : CharacterBody2D
{
    private static Player _instance;
    public static Player Instance => _instance;
    public float Gold { get; set; } = 0f;
    public int playerId { get; set; } = 0;

    [Export] public float Speed = 200f;

    // Stub properties matching Character interface
    public float CurrentHealth { get; set; } = 100f;
    public float MaxHealth { get; set; } = 100f;
    public float Attack { get; set; } = 10f;
    public float Defense { get; set; } = 5f;

    public override void _Ready()
    {
        _instance = this;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        Velocity = input * Speed;
        MoveAndSlide();
    }

    // Status effect stubs for skill system compatibility
    public void ApplyStatusEffect(StatusEffect.EffectType effectType, float value, float duration)
    {
        GD.Print("Player affected by: " + effectType);
    }

    public void ApplyStatusEffect(StatusEffectType effectType, float value, float duration)
    {
        GD.Print("Player affected by: " + effectType);
    }

    public void TakeDamage(int damage) { }
    public void Heal(int amount) { }

    public void LoadPlayerData(Dictionary<string, object> data)
    {
        if (data.ContainsKey("health")) CurrentHealth = Convert.ToSingle(data["health"]);
        if (data.ContainsKey("max_health")) MaxHealth = Convert.ToSingle(data["max_health"]);
        if (data.ContainsKey("attack")) Attack = Convert.ToSingle(data["attack"]);
        if (data.ContainsKey("defense")) Defense = Convert.ToSingle(data["defense"]);
        if (data.ContainsKey("gold")) Gold = Convert.ToSingle(data["gold"]);
    }
}
