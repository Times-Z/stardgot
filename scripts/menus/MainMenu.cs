using Godot;

/// <summary>
/// Main menu control that handles the primary navigation options for the game.
/// Provides buttons for starting a new game, accessing settings, and quitting the application.
/// This class manages scene transitions and coordinates with the background music system.
/// </summary>
public partial class MainMenu : Control {
	/// <summary>
	/// Node path to the "New Game" button. Should be set in the Godot editor.
	/// </summary>
	[Export] private NodePath NewGameButtonPath = "VBoxContainer/NewGameButton";

	/// <summary>
	/// Node path to the "Quit" button. Should be set in the Godot editor.
	/// </summary>
	[Export] private NodePath QuitButtonPath = "VBoxContainer/QuitButton";

	/// <summary>
	/// Node path to the "Settings" button. Should be set in the Godot editor.
	/// </summary>
	[Export] private NodePath SettingsButtonPath = "VBoxContainer/SettingsButton";

	/// <summary>
	/// Called when the node enters the scene tree for the first time.
	/// </summary>
	public override void _Ready() {
		GD.Print("MainMenu _Ready");

		var newGameButton = GetNodeOrNull<Button>(NewGameButtonPath);
		if (newGameButton != null) {
			newGameButton.CallDeferred("grab_focus");
		}
	}

	/// <summary>
	/// Handles the quit button press event.
	/// Immediately exits the application.
	/// </summary>
	public void _on_quit_button_pressed() {
		GetTree().Quit();
	}

	/// <summary>
	/// Handles the new game button press event.
	/// Uses the centralized NavigationManager to transition to the main game scene.
	/// Falls back to PackedScene if NavigationManager is not available.
	/// </summary>
	public void _on_new_game_button_pressed() {
		NavigationManager.Instance.NavigateToMainMap();
	}

	/// <summary>
	/// Handles the settings button press event.
	/// Uses the centralized NavigationManager to transition to the settings menu.
	/// Falls back to PackedScene if NavigationManager is not available.
	/// </summary>
	public void _on_settings_button_pressed() {
		NavigationManager.Instance.NavigateToSettingsMenuWithContext("MainMenu");
	}
}
