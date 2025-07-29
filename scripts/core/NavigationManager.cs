using Godot;

/// <summary>
/// Centralized navigation manager that handles scene transitions throughout the game.
/// This singleton (autoload) provides a clean way to navigate between scenes without
/// circular references and maintains consistent navigation behavior.
/// </summary>
public partial class NavigationManager : Node {
	/// <summary>
	/// Scene paths for easy management and consistency.
	/// </summary>
	public static class ScenePaths {
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
	public override void _Ready() {
		Instance = this;
		GD.Print("NavigationManager _Ready");
	}

	/// <summary>
	/// Navigate to a scene using PackedScene with caching.
	/// </summary>
	/// <param name="scenePath">The path to the scene to load</param>
	/// <returns>True if navigation was successful, false otherwise</returns>
	public bool NavigateToScene(string scenePath) {
		GD.Print($"NavigationManager: Navigating to {scenePath}");

		var packedScene = GetOrLoadScene(scenePath);

		if (packedScene != null) {
			GD.Print($"Successfully loaded scene: {packedScene.ResourcePath}");
			GetTree().ChangeSceneToPacked(packedScene);
			return true;
		}
		else {
			GD.PrintErr($"Failed to load scene: {scenePath}");
			return false;
		}
	}

	/// <summary>
	/// Navigate to the main menu.
	/// </summary>
	public void NavigateToMainMenu() {
		NavigateToScene(ScenePaths.MainMenu);
	}

	/// <summary>
	/// Navigate to the settings menu.
	/// </summary>
	public void NavigateToSettingsMenu() {
		NavigateToScene(ScenePaths.SettingsMenu);
	}

	/// <summary>
	/// Navigate to the settings menu with context for proper back navigation.
	/// </summary>
	/// <param name="fromContext">The context from which settings is being opened (e.g., "MainMenu", "PauseMenu")</param>
	public void NavigateToSettingsMenuWithContext(string fromContext) {
		_navigationStack.Push(fromContext);
		
		// If coming from PauseMenu, use overlay instead of scene change to preserve game state
		if (fromContext == "PauseMenu") {
			ShowSettingsMenuOverlay();
		}
		else {
			NavigateToScene(ScenePaths.SettingsMenu);
		}
	}

	/// <summary>
	/// Navigate back to the previous context or default to main menu.
	/// </summary>
	public void NavigateBack() {
		if (_navigationStack.Count > 0) {
			var previousContext = _navigationStack.Pop();
			switch (previousContext) {
				case "MainMenu":
					NavigateToMainMenu();
					break;
				case "PauseMenu":
					// If we're coming back from settings overlay, close it and show pause menu again
					CloseSettingsOverlayAndShowPauseMenu();
					break;
				default:
					NavigateToMainMenu();
					break;
			}
		}
		else {
			NavigateToMainMenu();
		}
	}

	/// <summary>
	/// Navigate back to the game and trigger pause menu display.
	/// This is used when returning from settings while in-game.
	/// </summary>
	private void NavigateBackToPauseMenu() {
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
	private void ShowPauseMenuFromSettings() {
		Node player = _currentPlayer;
		if (player == null || !IsInstanceValid(player)) {
			player = GetTree().CurrentScene?.FindChild("Player", true, false);
			if (player == null)
				return;
			_currentPlayer = player;
		}

		var playerCamera = player.GetNodeOrNull<Camera2D>("PlayerCamera");
		var pauseMenuScene = GetOrLoadScene(ScenePaths.PauseMenu);

		if (pauseMenuScene != null && playerCamera != null) {
			ShowPauseMenu(playerCamera, pauseMenuScene);
		}
	}

	/// <summary>
	/// Navigate to the main game scene.
	/// Automatically stops the menu music if available.
	/// </summary>
	public void NavigateToMainMap() {
		// Stop menu music when starting the game
		var musicPlayer = GetNodeOrNull("/root/MenuMusicPlayer") as MenuMusicPlayer;
		musicPlayer?.StopMusic();

		NavigateToScene(ScenePaths.MainMap);
	}

	/// <summary>
	/// Set the current player reference for pause menu functionality.
	/// </summary>
	/// <param name="player">The player node</param>
	public void SetCurrentPlayer(Node player) {
		_currentPlayer = player;
	}

	/// <summary>
	/// Create and show a pause menu instance on the specified parent node.
	/// </summary>
	/// <param name="parent">The parent node where the pause menu will be added</param>
	/// <param name="pauseMenuScene">The PackedScene for the pause menu</param>
	/// <returns>True if pause menu was successfully created, false otherwise</returns>
	public bool ShowPauseMenu(Node parent, PackedScene pauseMenuScene) {
		if (parent == null || pauseMenuScene == null) {
			GD.PrintErr("NavigationManager: Cannot show pause menu - parent or scene is null");
			return false;
		}

		// Important : ensure we are not creating multiple pause menus
		if (parent.FindChild("PauseMenu", true, false) != null) {
			GD.Print("NavigationManager: Pause menu already exists");
			return false;
		}

		var viewport = GetTree().Root;
		ImageTexture screenCopy = null;
		var screenTexture = viewport?.GetTexture();
		var image = screenTexture?.GetImage();
		if (image != null)
			screenCopy = ImageTexture.CreateFromImage(image);

		var canvasLayer = new CanvasLayer();
		parent.AddChild(canvasLayer);

		var pauseMenuInstance = pauseMenuScene.Instantiate();
		canvasLayer.AddChild(pauseMenuInstance);

		if (pauseMenuInstance is Control pauseMenuControl && screenCopy != null)
			pauseMenuControl.Call("SetScreenTexture", screenCopy);

		GetTree().Paused = true;
		GD.Print("NavigationManager: Pause menu created successfully");
		return true;
	}

	/// <summary>
	/// Shows the settings menu as an overlay without changing the current scene.
	/// This preserves the game state when accessing settings from the pause menu.
	/// </summary>
	private void ShowSettingsMenuOverlay() {
		var settingsScene = GetOrLoadScene(ScenePaths.SettingsMenu);
		if (settingsScene == null) {
			GD.PrintErr("Failed to load settings scene for overlay");
			return;
		}

		var root = GetTree().Root;
		if (root == null) {
			GD.PrintErr("Cannot find root node for settings overlay");
			return;
		}

		// Important: ensure we are not creating multiple settings overlays
		if (root.GetNodeOrNull<CanvasLayer>("SettingsOverlay") != null) {
			GD.Print("Settings overlay already exists");
			return;
		}

		var overlayLayer = new CanvasLayer {
			Name = "SettingsOverlay",
			Layer = 200
		};
		root.AddChild(overlayLayer);

		var settingsInstance = settingsScene.Instantiate<Control>();
		overlayLayer.AddChild(settingsInstance);
		settingsInstance.SetAnchorsPreset(Control.LayoutPreset.FullRect);

		GD.Print("NavigationManager: Settings overlay created successfully");
	}

	/// <summary>
	/// Closes the settings overlay and returns to the previous state.
	/// </summary>
	public void CloseSettingsOverlay() {
		var root = GetTree().Root;
		var overlayLayer = root?.GetNodeOrNull<CanvasLayer>("SettingsOverlay");
		
		if (overlayLayer != null) {
			overlayLayer.QueueFree();
			GD.Print("NavigationManager: Settings overlay closed");
		}
	}

	/// <summary>
	/// Closes the settings overlay and shows the pause menu again.
	/// Used when returning from settings to pause menu.
	/// </summary>
	private void CloseSettingsOverlayAndShowPauseMenu() {
		CloseSettingsOverlay();
		
		// Find the current player and show pause menu
		var currentScene = GetTree().CurrentScene;
		var player = currentScene?.FindChild("Player", true, false);
		
		if (player != null) {
			var playerCamera = player.GetNodeOrNull<Camera2D>("PlayerCamera");
			var pauseMenuScene = GetOrLoadScene(ScenePaths.PauseMenu);
			
			if (playerCamera != null && pauseMenuScene != null) {
				// need a small delay to ensure overlay is properly removed
				GetTree().CreateTimer(0.1f).Timeout += () => {
					ShowPauseMenu(playerCamera, pauseMenuScene);
				};
			}
		}
	}

	/// <summary>
	/// Get a PackedScene from cache or load it if not cached.
	/// </summary>
	/// <param name="scenePath">The path to the scene</param>
	/// <returns>The PackedScene or null if loading failed</returns>
	private PackedScene GetOrLoadScene(string scenePath) {
		// Check cache first
		if (_sceneCache.TryGetValue(scenePath, out var cachedScene)) {
			GD.Print($"Using cached scene: {scenePath}");
			return cachedScene;
		}

		// Load the scene
		var scene = GD.Load<PackedScene>(scenePath);
		if (scene != null) {
			_sceneCache[scenePath] = scene;
			GD.Print($"Cached new scene: {scenePath}");
		}

		return scene;
	}

	/// <summary>
	/// Clear the scene cache. Useful for memory management or when scenes are updated.
	/// </summary>
	public void ClearSceneCache() {
		_sceneCache.Clear();
		GD.Print("Scene cache cleared");
	}

	/// <summary>
	/// Preload commonly used scenes for faster navigation.
	/// </summary>
	public void PreloadCommonScenes() {
		GetOrLoadScene(ScenePaths.MainMenu);
		GetOrLoadScene(ScenePaths.SettingsMenu);
	}
}
