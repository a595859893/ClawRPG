using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed = 200f;
    
    public override void _PhysicsProcess(double delta)
    {
        Vector2 input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        Velocity = input * Speed;
        MoveAndSlide();
    }
}
