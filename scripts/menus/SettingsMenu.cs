using Godot;

/// <summary>
/// Manages the settings menu interface and navigation.
/// This class handles the settings menu UI and provides functionality
/// to return to the main menu from the settings screen.
/// </summary>
public partial class SettingsMenu : Control {
	/// <summary>
	/// Node path to the back button that returns to the main menu.
	/// Should be set in the Godot editor.
	/// </summary>
	[Export] private NodePath BackButtonPath = "VBoxContainer/BackButton";

	/// <summary>
	/// Called when the node enters the scene tree for the first time.
	/// </summary>
	public override void _Ready() {
		GD.Print("SettingsMenu _Ready");

		// Check if we're in an overlay by looking at our parent hierarchy
		var parent = GetParent();
		if (parent != null && parent.Name == "SettingsOverlay") {
			GD.Print("SettingsMenu: Running as overlay to preserve game state");
			// Enable input processing and set process mode to always for overlay
			ProcessMode = ProcessModeEnum.Always;
			SetProcessInput(true);
		}
		else {
			GD.Print("SettingsMenu: Running as full scene");
		}

		var backButton = GetNodeOrNull<Button>(BackButtonPath);

		if (backButton != null) {
			backButton.GrabFocus();
		}
	}

	/// <summary>
	/// Handle input events when in overlay mode.
	/// </summary>
	public override void _Input(InputEvent @event) {
		var parent = GetParent();
		if (parent != null && parent.Name == "SettingsOverlay") {
			if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape) {
				// Avoid triggering pause menu
				GetViewport().SetInputAsHandled();
			}
		}
	}

	/// <summary>
	/// Handles the back button press event.
	/// Uses the centralized NavigationManager to return to the previous context.
	/// This approach maintains PackedScene usage while avoiding circular references.
	/// </summary>
	public void _on_back_button_pressed() {
		NavigationManager.Instance.NavigateBack();
	}
}
