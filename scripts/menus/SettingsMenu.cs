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

		var backButton = GetNodeOrNull<Button>(BackButtonPath);

		if (backButton != null) {
			backButton.GrabFocus();
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
