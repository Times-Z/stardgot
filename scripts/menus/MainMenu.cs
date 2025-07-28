using Godot;

/// <summary>
/// Main menu control that handles the primary navigation options for the game.
/// Provides buttons for starting a new game, accessing settings, and quitting the application.
/// This class manages scene transitions and coordinates with the background music system.
/// </summary>
public partial class MainMenu : Control
{
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
	/// Initializes button references and connects their pressed signals to respective handlers.
	/// </summary>
	public override void _Ready()
	{
		GD.Print("MainMenu _Ready");

		var newGameButton = GetNode<Button>(NewGameButtonPath);
		var quitButton = GetNode<Button>(QuitButtonPath);
		var settingsButton = GetNode<Button>(SettingsButtonPath);

		newGameButton.Pressed += OnNewGameButtonPressed;
		quitButton.Pressed += OnQuitButtonPressed;
		settingsButton.Pressed += OnSettingsButtonPressed;
	}

	/// <summary>
	/// Handles the quit button press event.
	/// Immediately exits the application.
	/// </summary>
	private void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}

	/// <summary>
	/// Handles the new game button press event.
	/// Uses the centralized NavigationManager to transition to the main game scene.
	/// Falls back to PackedScene if NavigationManager is not available.
	/// </summary>
	private void OnNewGameButtonPressed()
	{
		NavigationManager.Instance.NavigateToMainMap();
	}

	/// <summary>
	/// Handles the settings button press event.
	/// Uses the centralized NavigationManager to transition to the settings menu.
	/// Falls back to PackedScene if NavigationManager is not available.
	/// </summary>
	private void OnSettingsButtonPressed()
	{
		NavigationManager.Instance.NavigateToSettingsMenuWithContext("MainMenu");
	}
}
