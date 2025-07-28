using Godot;

/// <summary>
/// Centralized navigation manager that handles scene transitions throughout the game.
/// This singleton (autoload) provides a clean way to navigate between scenes without
/// circular references and maintains consistent navigation behavior.
/// </summary>
public partial class NavigationManager : Node
{
	/// <summary>
	/// Scene paths for easy management and consistency.
	/// </summary>
	public static class ScenePaths
	{
		public const string MainMenu = "res://scenes/menus/MainMenu.tscn";
		public const string SettingsMenu = "res://scenes/menus/SettingsMenu.tscn";
		public const string PauseMenu = "res://scenes/menus/PauseMenu.tscn";
		public const string MainMap = "res://scenes/main/MainMap.tscn";
		public const string GameRoot = "res://scenes/main/GameRoot.tscn";
	}

	/// <summary>
	/// Cache for loaded PackedScenes to avoid reloading the same scenes multiple times.
	/// </summary>
	private static readonly System.Collections.Generic.Dictionary<string, PackedScene> _sceneCache = new();

	/// <summary>
	/// Stack to keep track of navigation context for proper back navigation.
	/// </summary>
	private static readonly System.Collections.Generic.Stack<string> _navigationStack = new();

	/// <summary>
	/// Reference to the current player for pause menu functionality.
	/// </summary>
	private static Node _currentPlayer;

	/// <summary>
	/// Singleton instance for easy access.
	/// </summary>
	public static NavigationManager Instance { get; private set; }

	/// <summary>
	/// Initialize the singleton instance.
	/// </summary>
	public override void _Ready()
	{
		Instance = this;
		GD.Print("NavigationManager _Ready");
	}

	/// <summary>
	/// Navigate to a scene using PackedScene with caching.
	/// </summary>
	/// <param name="scenePath">The path to the scene to load</param>
	/// <returns>True if navigation was successful, false otherwise</returns>
	public bool NavigateToScene(string scenePath)
	{
		GD.Print($"NavigationManager: Navigating to {scenePath}");

		var packedScene = GetOrLoadScene(scenePath);
		
		if (packedScene != null)
		{
			GD.Print($"Successfully loaded scene: {packedScene.ResourcePath}");
			GetTree().ChangeSceneToPacked(packedScene);
			return true;
		}
		else
		{
			GD.PrintErr($"Failed to load scene: {scenePath}");
			return false;
		}
	}

	/// <summary>
	/// Navigate to the main menu.
	/// </summary>
	public void NavigateToMainMenu()
	{
		NavigateToScene(ScenePaths.MainMenu);
	}

	/// <summary>
	/// Navigate to the settings menu.
	/// </summary>
	public void NavigateToSettingsMenu()
	{
		NavigateToScene(ScenePaths.SettingsMenu);
	}

	/// <summary>
	/// Navigate to the settings menu with context for proper back navigation.
	/// </summary>
	/// <param name="fromContext">The context from which settings is being opened (e.g., "MainMenu", "PauseMenu")</param>
	public void NavigateToSettingsMenuWithContext(string fromContext)
	{
		_navigationStack.Push(fromContext);
		NavigateToScene(ScenePaths.SettingsMenu);
	}

	/// <summary>
	/// Navigate back to the previous context or default to main menu.
	/// </summary>
	public void NavigateBack()
	{
		if (_navigationStack.Count > 0)
		{
			var previousContext = _navigationStack.Pop();
			switch (previousContext)
			{
				case "MainMenu":
					NavigateToMainMenu();
					break;
				case "PauseMenu":
					NavigateBackToPauseMenu();
					break;
				default:
					NavigateToMainMenu();
					break;
			}
		}
		else
		{
			NavigateToMainMenu();
		}
	}

	/// <summary>
	/// Navigate back to the game and trigger pause menu display.
	/// This is used when returning from settings while in-game.
	/// </summary>
	private void NavigateBackToPauseMenu()
	{
		// First navigate to the main map scene
		NavigateToScene(ScenePaths.MainMap);
		
		// Then show the pause menu after a short delay to ensure the scene is loaded
		GetTree().CreateTimer(0.1f).Timeout += () => {
			ShowPauseMenuFromSettings();
		};
	}

	/// <summary>
	/// Show pause menu after returning from settings.
	/// This method finds the player camera and shows the pause menu.
	/// </summary>
	private void ShowPauseMenuFromSettings()
	{
		if (_currentPlayer != null && IsInstanceValid(_currentPlayer))
		{
			// Get the pause menu scene and player camera
			var pauseMenuScene = GD.Load<PackedScene>(ScenePaths.PauseMenu);
			var playerCamera = _currentPlayer.GetNodeOrNull<Camera2D>("PlayerCamera");
			
			if (pauseMenuScene != null && playerCamera != null)
			{
				ShowPauseMenu(playerCamera, pauseMenuScene);
			}
		}
		else
		{
			// Fallback: try to find player in the current scene
			var currentScene = GetTree().CurrentScene;
			var player = currentScene?.FindChild("Player", true, false);
			if (player != null)
			{
				_currentPlayer = player;
				ShowPauseMenuFromSettings(); // Recursive call with found player
			}
		}
	}

	/// <summary>
	/// Navigate to the main game scene.
	/// Automatically stops the menu music if available.
	/// </summary>
	public void NavigateToMainMap()
	{
		// Stop menu music when starting the game
		var musicPlayer = GetNodeOrNull("/root/MenuMusicPlayer") as MenuMusicPlayer;
		musicPlayer?.StopMusic();
		
		NavigateToScene(ScenePaths.MainMap);
	}

	/// <summary>
	/// Set the current player reference for pause menu functionality.
	/// </summary>
	/// <param name="player">The player node</param>
	public void SetCurrentPlayer(Node player)
	{
		_currentPlayer = player;
	}

	/// <summary>
	/// Create and show a pause menu instance on the specified parent node.
	/// </summary>
	/// <param name="parent">The parent node where the pause menu will be added</param>
	/// <param name="pauseMenuScene">The PackedScene for the pause menu</param>
	/// <returns>True if pause menu was successfully created, false otherwise</returns>
	public bool ShowPauseMenu(Node parent, PackedScene pauseMenuScene)
	{
		if (parent == null || pauseMenuScene == null)
		{
			GD.PrintErr("NavigationManager: Cannot show pause menu - parent or scene is null");
			return false;
		}

		// Check if pause menu already exists
		foreach (var child in parent.GetChildren())
		{
			if (child is CanvasLayer existingCanvas)
			{
				foreach (var canvasChild in existingCanvas.GetChildren())
				{
					if (canvasChild.GetType().Name == "PauseMenu")
					{
						GD.Print("NavigationManager: Pause menu already exists");
						return false;
					}
				}
			}
		}

		// Create pause menu
		var canvasLayer = new CanvasLayer();
		parent.AddChild(canvasLayer);

		var pauseMenuInstance = pauseMenuScene.Instantiate();
		canvasLayer.AddChild(pauseMenuInstance);
		GetTree().Paused = true;

		GD.Print("NavigationManager: Pause menu created successfully");
		return true;
	}

	/// <summary>
	/// Get a PackedScene from cache or load it if not cached.
	/// </summary>
	/// <param name="scenePath">The path to the scene</param>
	/// <returns>The PackedScene or null if loading failed</returns>
	private PackedScene GetOrLoadScene(string scenePath)
	{
		// Check cache first
		if (_sceneCache.TryGetValue(scenePath, out var cachedScene))
		{
			GD.Print($"Using cached scene: {scenePath}");
			return cachedScene;
		}

		// Load the scene
		var scene = GD.Load<PackedScene>(scenePath);
		if (scene != null)
		{
			_sceneCache[scenePath] = scene;
			GD.Print($"Cached new scene: {scenePath}");
		}

		return scene;
	}

	/// <summary>
	/// Clear the scene cache. Useful for memory management or when scenes are updated.
	/// </summary>
	public void ClearSceneCache()
	{
		_sceneCache.Clear();
		GD.Print("Scene cache cleared");
	}

	/// <summary>
	/// Preload commonly used scenes for faster navigation.
	/// </summary>
	public void PreloadCommonScenes()
	{
		GetOrLoadScene(ScenePaths.MainMenu);
		GetOrLoadScene(ScenePaths.SettingsMenu);
	}
}
