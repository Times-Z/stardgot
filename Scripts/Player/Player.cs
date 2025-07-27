using Godot;

public partial class Player : CharacterBody2D
{
    [Export] private int speed = 100;
    private AnimatedSprite2D animatedSprite;

    public override void _Ready()
    {
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animatedSprite.Play("idle");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 inputDirection = GetInputDirection();

        Velocity = inputDirection * speed;
        MoveAndSlide();
    }

    private Vector2 GetInputDirection()
    {
        float x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
        float y = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");
        Vector2 direction = new Vector2(x, y);
        return direction.Length() > 0 ? direction.Normalized() : Vector2.Zero;
    }
}
