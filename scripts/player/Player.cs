using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public PackedScene PauseMenuScene;
    private int speed = 100;
    private AnimatedSprite2D _animatedSprite;
    private Camera2D _playerCamera;
    private readonly Vector2 _minZoom = new(3, 3);
    private readonly Vector2 _maxZoom = new(6, 6);
    private const float ZoomStep = 0.2f;
    private string _lastDirection = "down";

    public override void _Ready()
    {
        GD.Print("Player _Ready");
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _animatedSprite.Play("idle_down");
        _playerCamera = GetNodeOrNull<Camera2D>("PlayerCamera");
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
        if (GetTree().Paused) return;
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            // Check if pause menu already exists on the camera
            if (_playerCamera != null)
            {
                foreach (var child in _playerCamera.GetChildren())
                {
                    if (child.GetType().Name == "CanvasLayer")
                    {
                        var canvasLayer = child as CanvasLayer;
                        foreach (var canvasChild in canvasLayer.GetChildren())
                        {
                            if (canvasChild.GetType().Name == "PauseMenu") return;
                        }
                    }
                }
            }

            if (PauseMenuScene != null && _playerCamera != null)
            {
                var canvasLayer = new CanvasLayer();
                _playerCamera.AddChild(canvasLayer);

                var pauseMenuInstance = PauseMenuScene.Instantiate();
                canvasLayer.AddChild(pauseMenuInstance);
                GetTree().Paused = true;
            }
            return;
        }
        if (@event is not InputEventMouseButton mouseEvent || !mouseEvent.Pressed) return;

        switch (mouseEvent.ButtonIndex)
        {
            case MouseButton.WheelUp:
                ZoomCamera(ZoomStep);
                break;
            case MouseButton.WheelDown:
                ZoomCamera(-ZoomStep);
                break;
        }
    }

    private Vector2 GetInputDirection()
    {
        float x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
        float y = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");
        var direction = new Vector2(x, y);
        return direction.Length() > 0 ? direction.Normalized() : Vector2.Zero;
    }

    private void UpdateAnimation()
    {
        if (Velocity.Length() == 0)
        {
            _animatedSprite.Play($"idle_{_lastDirection}");
            return;
        }

        string direction = Mathf.Abs(Velocity.X) > Mathf.Abs(Velocity.Y)
            ? (Velocity.X < 0 ? "left" : "right")
            : (Velocity.Y < 0 ? "up" : "down");

        _animatedSprite.Play($"walk_{direction}");
        _lastDirection = direction;
    }

    private void ZoomCamera(float delta)
    {
        if (_playerCamera == null) return;
        Vector2 newZoom = _playerCamera.Zoom + new Vector2(delta, delta);
        newZoom.X = Mathf.Clamp(newZoom.X, _minZoom.X, _maxZoom.X);
        newZoom.Y = Mathf.Clamp(newZoom.Y, _minZoom.Y, _maxZoom.Y);
        _playerCamera.Zoom = newZoom;
    }
}
