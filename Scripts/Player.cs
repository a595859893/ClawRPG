using Godot;
using ClawRPG.Systems;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed = 200f;

    // Stub properties matching Character interface
    public float CurrentHealth { get; set; } = 100f;
    public float MaxHealth { get; set; } = 100f;
    public float Attack { get; set; } = 10f;
    public float Defense { get; set; } = 5f;

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
}
