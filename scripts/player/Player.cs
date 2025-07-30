using Godot;

/// <summary>
/// Main player character controller that handles movement, animations, camera controls, and pause menu interactions.
/// This class manages the player's CharacterBody2D movement, sprite animations based on direction,
/// camera zoom functionality, and integrates with the pause menu system.
/// </summary>
public partial class Player : CharacterBody2D {
    /// <summary>
    /// Packed scene for the pause menu that will be instantiated when the player pauses the game.
    /// Should be assigned in the Godot editor.
    /// </summary>
    [Export] public PackedScene PauseMenuScene;

    /// <summary>
    /// Movement speed of the player character in pixels per second.
    /// </summary>
    private int speed = 80;

    /// <summary>
    /// Reference to the player's animated sprite component for handling animations.
    /// </summary>
    private AnimatedSprite2D _animatedSprite;

    /// <summary>
    /// Reference to the player's camera for handling zoom and view controls.
    /// </summary>
    private Camera2D _playerCamera;

    /// <summary>
    /// Minimum zoom level allowed for the player camera.
    /// </summary>
    private readonly Vector2 _minZoom = new(3, 3);

    /// <summary>
    /// Maximum zoom level allowed for the player camera.
    /// </summary>
    private readonly Vector2 _maxZoom = new(10, 10);

    /// <summary>
    /// The amount by which zoom changes with each scroll wheel input.
    /// </summary>
    private const float ZoomStep = 0.5f;

    /// <summary>
    /// Stores the last movement direction for maintaining consistent idle animations.
    /// </summary>
    private string _lastDirection = "down";

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// Initializes references to child components and starts the default idle animation.
    /// </summary>
    public override void _Ready() {
        GD.Print("Player _Ready");
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _animatedSprite.Play("idle_down");
        _playerCamera = GetNodeOrNull<Camera2D>("PlayerCamera");

        // Register with NavigationManager for pause menu functionality
        if (NavigationManager.Instance != null) {
            NavigationManager.Instance.SetCurrentPlayer(this);
        }
    }

    /// <summary>
    /// Called every frame during the process phase.
    /// Updates the player's animation based on current movement state.
    /// </summary>
    /// <param name="delta">Time elapsed since the last frame</param>
    public override void _Process(double delta) {
        UpdateAnimation();
    }

    /// <summary>
    /// Called during the physics processing phase for movement calculations.
    /// Handles player movement based on input and applies it using MoveAndSlide().
    /// </summary>
    /// <param name="delta">Time elapsed since the last physics frame</param>
    public override void _PhysicsProcess(double delta) {
        Velocity = GetInputDirection() * speed;
        MoveAndSlide();
    }

    /// <summary>
    /// Processes input events for pause menu activation and camera zoom controls.
    /// Handles Escape key for pause menu and mouse wheel for camera zoom.
    /// Prevents input processing when the game is already paused.
    /// </summary>
    /// <param name="event">The input event to process</param>
    public override void _Input(InputEvent @event) {
        if (GetTree().Paused) return;
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape) {
            ShowPauseMenu();
            return;
        }
        if (@event is not InputEventMouseButton mouseEvent || !mouseEvent.Pressed) return;

        switch (mouseEvent.ButtonIndex) {
            case MouseButton.WheelUp:
                ZoomCamera(ZoomStep);
                break;
            case MouseButton.WheelDown:
                ZoomCamera(-ZoomStep);
                break;
        }
    }

    /// <summary>
    /// Gets the normalized input direction vector based on player input.
    /// Combines horizontal and vertical input to create a movement vector.
    /// </summary>
    /// <returns>A normalized Vector2 representing the desired movement direction, or Vector2.Zero if no input</returns>
    private Vector2 GetInputDirection() {
        float x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
        float y = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");
        var direction = new Vector2(x, y);
        return direction.Length() > 0 ? direction.Normalized() : Vector2.Zero;
    }

    /// <summary>
    /// Updates the player's animation based on current velocity and movement direction.
    /// Plays idle animations when stationary and walking animations when moving.
    /// Determines animation direction based on the dominant axis of movement.
    /// </summary>
    private void UpdateAnimation() {
        if (Velocity.Length() == 0) {
            _animatedSprite.Play($"idle_{_lastDirection}");
            return;
        }

        string direction = Mathf.Abs(Velocity.X) > Mathf.Abs(Velocity.Y)
            ? (Velocity.X < 0 ? "left" : "right")
            : (Velocity.Y < 0 ? "up" : "down");

        _animatedSprite.Play($"walk_{direction}");
        _lastDirection = direction;
    }

    /// <summary>
    /// Adjusts the camera zoom level by the specified delta amount.
    /// Clamps the zoom level between the defined minimum and maximum values.
    /// </summary>
    /// <param name="delta">The amount to change the zoom by (positive for zoom in, negative for zoom out)</param>
    private void ZoomCamera(float delta) {
        if (_playerCamera == null) return;
        Vector2 newZoom = _playerCamera.Zoom + new Vector2(delta, delta);
        newZoom.X = Mathf.Clamp(newZoom.X, _minZoom.X, _maxZoom.X);
        newZoom.Y = Mathf.Clamp(newZoom.Y, _minZoom.Y, _maxZoom.Y);
        _playerCamera.Zoom = newZoom;
    }

    /// <summary>
    /// Shows the pause menu using the NavigationManager.
    /// Creates a pause menu instance and adds it to the camera's UI layer.
    /// </summary>
    private void ShowPauseMenu() {
        if (NavigationManager.Instance != null && _playerCamera != null && PauseMenuScene != null) {
            NavigationManager.Instance.ShowPauseMenu(_playerCamera, PauseMenuScene);
        }
        else {
            GD.PrintErr("Player: Cannot show pause menu - NavigationManager, camera, or pause menu scene is null");
        }
    }
}
