using Godot;

/// <summary>
/// Base class for all interactable objects in the game.
/// This abstract class provides the foundation for objects that the player can interact with,
/// such as doors, NPCs, chests, etc.
/// </summary>
public abstract partial class Interactable : Area2D {
    /// <summary>
    /// The text to display when the player is near this interactable object.
    /// This will be shown as a prompt to indicate the player can interact.
    /// </summary>
    [Export] public string InteractionPromptComponent = "Press E to interact";

    /// <summary>
    /// Whether this interactable is currently available for interaction.
    /// Can be used to disable interactions temporarily.
    /// </summary>
    [Export] public bool IsInteractable = true;

    /// <summary>
    /// Reference to the player currently in range for interaction.
    /// Null when no player is in range.
    /// </summary>
    protected Player _playerInRange;

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// Sets up the area signals for detecting when the player enters and exits the interaction range.
    /// </summary>
    public override void _Ready() {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    /// <summary>
    /// Called when a body enters the interaction area.
    /// If the body is a player, stores the reference and calls OnPlayerEntered.
    /// </summary>
    /// <param name="body">The body that entered the area</param>
    private void OnBodyEntered(Node2D body) {
        if (body is Player player) {
            _playerInRange = player;
            OnPlayerEntered(player);
        }
    }

    /// <summary>
    /// Called when a body exits the interaction area.
    /// If the body is the player in range, clears the reference and calls OnPlayerExited.
    /// </summary>
    /// <param name="body">The body that exited the area</param>
    private void OnBodyExited(Node2D body) {
        if (body == _playerInRange) {
            OnPlayerExited(_playerInRange);
            _playerInRange = null;
        }
    }

    /// <summary>
    /// Called when the player enters the interaction range.
    /// Override this method to implement custom behavior when the player gets close.
    /// </summary>
    /// <param name="player">The player that entered the range</param>
    protected virtual void OnPlayerEntered(Player player) {
        GD.Print($"Player entered interaction range of {GetType().Name}");
        // Notify the player that an interactable is available
        player.SetCurrentInteractable(this);
    }

    /// <summary>
    /// Called when the player exits the interaction range.
    /// Override this method to implement custom behavior when the player moves away.
    /// </summary>
    /// <param name="player">The player that exited the range</param>
    protected virtual void OnPlayerExited(Player player) {
        GD.Print($"Player exited interaction range of {GetType().Name}");
        // Notify the player that no interactable is available
        player.SetCurrentInteractable(null);
    }

    /// <summary>
    /// Called when the player attempts to interact with this object.
    /// This is the main interaction method that derived classes should override.
    /// </summary>
    /// <param name="player">The player attempting the interaction</param>
    public virtual void Interact(Player player) {
        if (!IsInteractable) {
            GD.Print($"{GetType().Name} is not currently interactable");
            return;
        }

        GD.Print($"Player interacted with {GetType().Name}");
        // Override this method in derived classes to implement specific interaction behavior
    }

    /// <summary>
    /// Checks if the player is currently in range and able to interact.
    /// </summary>
    /// <returns>True if interaction is possible, false otherwise</returns>
    public bool CanInteract() {
        return _playerInRange != null && IsInteractable;
    }

    /// <summary>
    /// Clean up event connections when the node is about to be removed.
    /// </summary>
    public override void _ExitTree() {
        BodyEntered -= OnBodyEntered;
        BodyExited -= OnBodyExited;
        base._ExitTree();
    }
}
