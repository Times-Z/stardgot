using Godot;

public partial class Player : CharacterBody2D
{
    [Export] private int speed = 100;
    private AnimatedSprite2D animatedSprite;
    private Camera2D playerCamera;
    private Vector2 minZoom = new Vector2(3, 3);
    private Vector2 maxZoom = new Vector2(6, 6);
    private float zoomStep = 0.2f;

    public override void _Ready()
    {
        GD.Print("Player _Ready");

        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animatedSprite.Play("idle");
        playerCamera = FindChild("PlayerCamera", true, false) as Camera2D;
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

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
            {
                ZoomCamera(zoomStep);
            }
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
            {
                ZoomCamera(-zoomStep);
            }
        }
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

    private void ZoomCamera(float delta)
    {
        if (playerCamera == null) return;
        Vector2 newZoom = playerCamera.Zoom + new Vector2(delta, delta);
        newZoom.X = Mathf.Clamp(newZoom.X, minZoom.X, maxZoom.X);
        newZoom.Y = Mathf.Clamp(newZoom.Y, minZoom.Y, maxZoom.Y);
        playerCamera.Zoom = newZoom;
    }
}
