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
    /// Reference to the animation controller component for handling animations.
    /// </summary>
    private AnimationControllerComponent _animationController;

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
    /// Cache for the last animation played to avoid redundant calls
    /// </summary>
    private string _lastAnimation = "";

    /// <summary>
    /// Reference to the scene's depth sorter for managing object layering.
    /// The player needs to be registered with the depth sorter to appear correctly layered with other objects.
    /// </summary>
    private DepthSortableComponent _depthSortableComponent;

    /// <summary>
    /// The interactable object currently in range for interaction.
    /// Null when no interactable is available.
    /// </summary>
    private Interactable _currentInteractable;

    /// <summary>
    /// UI label to display interaction prompts on screen.
    /// </summary>
    [Export] private InteractionPromptComponent _interactionPrompt;

    /// <summary>
    /// Canvas layer for UI elements that should not be affected by camera zoom.
    /// </summary>
    private CanvasLayer _uiLayer;

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// Initializes references to child components and starts the default idle animation.
    /// </summary>
    public override void _Ready() {
        GD.Print("Player _Ready");
        
        SetPhysicsInterpolationMode(Node.PhysicsInterpolationModeEnum.Off);
        
        _animationController = GetNodeOrNull<AnimationControllerComponent>("AnimationController");
        if (_animationController == null) {
            GD.PrintErr("Player: AnimationControllerComponent not found. Please add it as a child node.");
        }
        else {
            _animationController.PlayAnimation("idle_down");
        }
        _playerCamera = GetNodeOrNull<Camera2D>("PlayerCamera");

        if (_playerCamera != null) {
            _playerCamera.Enabled = true;
            _playerCamera.MakeCurrent();
            
            _playerCamera.PositionSmoothingEnabled = false;
            
            GD.Print("Player camera enabled with pixel-perfect movement");

            _interactionPrompt = GameRoot.Instance?.GetUiLayer()?.GetNodeOrNull<InteractionPromptComponent>("InteractionPromptComponent");
            if (_interactionPrompt != null) {
                GD.Print("InteractionPromptComponent found in GameRoot UiLayer");
            }
            else {
                GD.Print("InteractionPromptComponent not found in GameRoot UiLayer");
            }
        }

        CallDeferred(nameof(EnsureCameraSetup));

        _depthSortableComponent = GetNodeOrNull<DepthSortableComponent>("DepthSortableComponent");
        if (_depthSortableComponent == null) {
            GD.PrintErr("Player: DepthSortableComponent not found. Please add it as a child node.");
        }
    }

    /// <summary>
    /// Ensures the player camera is properly set up in the new GameRoot system.
    /// Called deferred to ensure the scene tree is fully built.
    /// </summary>
    private void EnsureCameraSetup() {
        if (_playerCamera != null) {
            if (GameRoot.Instance != null) {
                GameRoot.Instance.SetCurrentCamera(_playerCamera);
                GD.Print("Player: Camera set via GameRoot system");
            } else {
                _playerCamera.Enabled = true;
                _playerCamera.MakeCurrent();
                GD.Print("Player: Camera set via direct activation");
            }
            
            _playerCamera.PositionSmoothingEnabled = false;
            _playerCamera.RotationSmoothingEnabled = false;
            
            var viewport = GetViewport();
            if (viewport != null) {
                GD.Print($"Player camera setup in viewport: {viewport.GetPath()}");
            }
        }
    }

    /// <summary>
    /// Called every frame during the process phase.
    /// Reserved for non-physics related updates.
    /// </summary>
    /// <param name="delta">Time elapsed since the last frame</param>
    public override void _Process(double delta) {
    }

    /// <summary>
    /// Called during the physics processing phase for movement calculations.
    /// Handles player movement based on input and applies it using MoveAndSlide().
    /// </summary>
    /// <param name="delta">Time elapsed since the last physics frame</param>
    public override void _PhysicsProcess(double delta) {
        Vector2 inputDirection = GetInputDirection();
        
        Velocity = inputDirection * speed;
        MoveAndSlide();
        
        GlobalPosition = new Vector2(
            Mathf.Round(GlobalPosition.X),
            Mathf.Round(GlobalPosition.Y)
        );
        
        UpdateAnimation();
    }

    /// <summary>
    /// Processes input events for pause menu activation and camera zoom controls.
    /// Handles pause action and interaction action using Input Map actions.
    /// Prevents input processing when the game is already paused.
    /// </summary>
    /// <param name="event">The input event to process</param>
    public override void _Input(InputEvent @event) {
        if (GetTree().Paused) return;

        if (@event.IsActionPressed("ui_cancel")) {
            ShowPauseMenu();
            return;
        }

        if (@event.IsActionPressed("interact")) {
            TryInteract();
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
    /// Uses standard Godot input actions that are configurable in the Input Map.
    /// </summary>
    /// <returns>A normalized Vector2 representing the desired movement direction, or Vector2.Zero if no input</returns>
    private Vector2 GetInputDirection() {
        var direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        return direction;
    }

    /// <summary>
    /// Updates the player's animation based on current velocity and movement direction.
    /// Plays idle animations when stationary and walking animations when moving.
    /// Determines animation direction based on the dominant axis of movement.
    /// Optimized to avoid redundant animation calls.
    /// </summary>
    private void UpdateAnimation() {
        if (_animationController == null) return;

        string targetAnimation;
        
        if (Velocity.Length() == 0) {
            targetAnimation = $"idle_{_lastDirection}";
        } else {
            string direction = Mathf.Abs(Velocity.X) > Mathf.Abs(Velocity.Y)
                ? (Velocity.X < 0 ? "left" : "right")
                : (Velocity.Y < 0 ? "up" : "down");

            targetAnimation = $"walk_{direction}";
            _lastDirection = direction;
        }

        if (targetAnimation != _lastAnimation) {
            _animationController.PlayAnimation(targetAnimation);
            _lastAnimation = targetAnimation;
        }
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
    /// Shows the pause menu using the MenuManager.
    /// Creates a pause menu instance and adds it as an overlay.
    /// </summary>
    private void ShowPauseMenu() {
        GameRoot.Instance.GetMenuManager().ShowPauseMenu();
    }

    /// <summary>
    /// Called when the node is about to be removed from the scene tree.
    /// </summary>
    public override void _ExitTree() {
        if (_interactionPrompt != null) {
            _interactionPrompt.QueueFree();
            _interactionPrompt = null;
        }
        base._ExitTree();
    }

    /// <summary>
    /// Attempts to interact with the currently available interactable object.
    /// </summary>
    private void TryInteract() {
        if (_currentInteractable != null && _currentInteractable.CanInteract()) {
            _currentInteractable.Interact(this);
        }
    }

    /// <summary>
    /// Sets the current interactable object that the player can interact with.
    /// Called by interactable objects when the player enters/exits their range.
    /// </summary>
    /// <param name="interactable">The interactable to set as current, or null to clear</param>
    public void SetCurrentInteractable(Interactable interactable) {
        _currentInteractable = interactable;

        if (_interactionPrompt == null) {
            _interactionPrompt = GameRoot.Instance?.GetUiLayer()?.GetNodeOrNull<InteractionPromptComponent>("InteractionPromptComponent");
        }

        if (interactable != null) {
            _interactionPrompt?.ShowPrompt(interactable.InteractionPromptComponent);
        }
        else {
            _interactionPrompt?.HidePrompt();
        }
    }

}
