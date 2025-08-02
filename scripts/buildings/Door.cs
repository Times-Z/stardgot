using Godot;

/// <summary>
/// Represents a door that can be opened and closed by the player.
/// When opened, the door removes collision so the player can pass through.
/// When closed, the door blocks the player's path.
/// </summary>
public partial class Door : Interactable {
    /// <summary>
    /// Whether the door is currently open or closed.
    /// </summary>
    [Export] public bool IsOpen = false;

    /// <summary>
    /// The collision shape that blocks the player when the door is closed.
    /// This should be assigned in the Godot editor.
    /// </summary>
    [Export] public CollisionShape2D DoorCollision;

    /// <summary>
    /// Optional sprite or visual representation of the door.
    /// Can be used to change the door's appearance when opened/closed.
    /// </summary>
    [Export] public AnimatedSprite2D DoorSprite;

    /// <summary>
    /// Sound effect to play when the door opens.
    /// </summary>
    [Export] public AudioStreamPlayer2D OpenSound;

    /// <summary>
    /// Sound effect to play when the door closes.
    /// </summary>
    [Export] public AudioStreamPlayer2D CloseSound;

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// Initializes the door state and finds necessary child nodes if not assigned.
    /// </summary>
    public override void _Ready() {
        base._Ready();

        // Auto-find collision shape if not assigned
        if (DoorCollision == null) {
            // Try to find DoorBlocker/DoorCollision in parent
            var parent = GetParent();
            var doorBlocker = parent.GetNodeOrNull<StaticBody2D>("DoorBlocker");
            if (doorBlocker != null) {
                DoorCollision = doorBlocker.GetNodeOrNull<CollisionShape2D>("DoorCollision");
            }

            // Fallback: try direct child
            if (DoorCollision == null) {
                DoorCollision = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
            }
        }

        if (DoorCollision == null) {
            GD.PrintErr("Door: Could not find DoorCollision CollisionShape2D!");
        }

        // Set initial state
        UpdateDoorState();
        UpdateInteractionPrompt();
    }

    /// <summary>
    /// Called when the player interacts with the door.
    /// Toggles the door between open and closed states.
    /// </summary>
    /// <param name="player">The player interacting with the door</param>
    public override void Interact(Player player) {
        if (!IsInteractable) {
            GD.Print("Door is not interactable");
            return;
        }

        // Toggle door state
        if (IsOpen) {
            CloseDoor();
        } else {
            OpenDoor();
        }
    }

    /// <summary>
    /// Opens the door, removing collision and updating visual state.
    /// </summary>
    public void OpenDoor() {
        IsOpen = true;
        UpdateDoorState();
        UpdateInteractionPrompt();

        // Play open sound
        if (OpenSound != null) {
            OpenSound.Play();
        }

        GD.Print("Door opened");
    }

    /// <summary>
    /// Closes the door, enabling collision and updating visual state.
    /// </summary>
    public void CloseDoor() {
        IsOpen = false;
        UpdateDoorState();
        UpdateInteractionPrompt();

        // Play close sound
        if (CloseSound != null) {
            CloseSound.Play();
        }

        GD.Print("Door closed");
    }

    /// <summary>
    /// Updates the door's collision state based on whether it's open or closed.
    /// </summary>
    private void UpdateDoorState() {
        // Update visual appearance if sprite is assigned
        if (DoorSprite != null) {
            DoorSprite.Play(IsOpen ? "open_animation" : "close_animation");
        }
        if (DoorCollision != null) {
            // DoorSprite.AnimationFinished()
            DoorCollision.SetDeferred("disabled", IsOpen);
        }
    }

    /// <summary>
    /// Updates the interaction prompt text based on the door state.
    /// </summary>
    private void UpdateInteractionPrompt() {
        InteractionPrompt = IsOpen ? "Press E to close door" : "Press E to open door";
        
        // If a player is in range, update the displayed prompt immediately
        if (_playerInRange != null) {
            _playerInRange.SetCurrentInteractable(this);
        }
    }

    /// <summary>
    /// Called when the player enters the interaction range.
    /// Updates the prompt and calls the base implementation.
    /// </summary>
    /// <param name="player">The player that entered the range</param>
    protected override void OnPlayerEntered(Player player) {
        UpdateInteractionPrompt();
        base.OnPlayerEntered(player);
    }
}
