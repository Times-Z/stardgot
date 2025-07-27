using Godot;

public partial class Player : CharacterBody2D
{
    [Export] private int speed = 100;
    private AnimatedSprite2D animatedSprite;

    public override void _Ready()
    {
        GD.Print("Player _Ready");
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animatedSprite.Play("idle");
    }

    public override void _Process(double delta)
    {
        UpdateAnimation();
    }

    public override void _PhysicsProcess(double delta)
    {
        Velocity = GetInputDirection() * speed;
        MoveAndSlide();
    }

    private Vector2 GetInputDirection()
    {
        float x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
        float y = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");
        Vector2 direction = new Vector2(x, y);
        return direction.Length() > 0 ? direction.Normalized() : Vector2.Zero;
    }

    private void UpdateAnimation()
    {
        if (Velocity.Length() == 0)
        {
            animatedSprite.Play("idle");
            return;
        }

        if (Mathf.Abs(Velocity.X) > Mathf.Abs(Velocity.Y))
        {
            animatedSprite.Play(Velocity.X < 0 ? "walk_left" : "walk_right");
        }
        else
        {
            animatedSprite.Play(Velocity.Y < 0 ? "walk_up" : "walk_down");
        }
    }
}
