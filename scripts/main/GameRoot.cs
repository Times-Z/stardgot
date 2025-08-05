using Godot;

using System.Linq;

/// <summary>
/// Game state enum for managing different application states
/// </summary>
public enum GameState {
    MainMenu,
    Loading,
    InGame,
    Paused,
    Settings
}

/// <summary>
/// Root node of the game that manages the viewport and UI layers.
/// This class serves as the entry point for the game and handles high-level operations
/// like displaying menus and managing different game layers.
/// </summary>
public partial class GameRoot : AutoSingleton<GameRoot> {
    /// <summary>
    /// Reference to the main viewport container.
    /// </summary>
    [Export] private SubViewportContainer _viewportContainer;

    /// <summary>
    /// Reference to the main viewport.
    /// </summary>
    [Export] private SubViewport _viewport;

    /// <summary>
    /// Reference to the UI layer for menus.
    /// </summary>
    [Export] private CanvasLayer _uiLayer;

    /// <summary>
    /// Reference to the menu manager
    /// </summary>
    [Export] private MenuManager _menuManager;

    /// <summary>
    /// Current game state
    /// </summary>
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    /// <summary>
    /// Signal emitted when game state changes
    /// </summary>
    [Signal]
    public delegate void GameStateChangedEventHandler(GameState oldState, GameState newState);

    /// <summary>
    /// Cache for frequently used scenes to improve performance
    /// </summary>
    private readonly Godot.Collections.Dictionary<string, PackedScene> _sceneCache = new();

    /// <summary>
    /// Maximum number of cached scenes to prevent memory issues
    /// </summary>
    private const int MaxCachedScenes = 10;

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// Shows the main menu and starts menu music.
    /// </summary>
    public override void _Ready() {
        base._Ready();

        PreloadCommonScenes();

        MenuMusicManager.Instance?.ResumeMenuContext();

        CurrentState = GameState.MainMenu;
        ShowMainMenu();
    }

    /// <summary>
    /// Shows the main menu in the UI layer using the menu manager.
    /// </summary>
    private void ShowMainMenu() {
        _menuManager?.ShowMenu(MenuManager.MenuType.MainMenu);
    }

    /// <summary>
    /// Hides the main menu.
    /// </summary>
    public void HideMainMenu() {
        _menuManager?.HideMenu(MenuManager.MenuType.MainMenu);
        ChangeGameState(GameState.Loading);
    }

    /// <summary>
    /// Changes the current game state and emits the signal
    /// </summary>
    /// <param name="newState">The new game state</param>
    public void ChangeGameState(GameState newState) {
        if (CurrentState != newState) {
            GameState oldState = CurrentState;
            CurrentState = newState;
            EmitSignal(SignalName.GameStateChanged, (int)oldState, (int)newState);
            GD.Print($"GameState changed from {oldState} to {newState}");
        }
    }

    /// <summary>
    /// Gets the UI layer for adding menu content.
    /// </summary>
    public CanvasLayer GetUiLayer() {
        return _uiLayer;
    }

    /// <summary>
    /// Gets the viewport for adding game content directly.
    /// </summary>
    public new SubViewport GetViewport() {
        return _viewport;
    }

    /// <summary>
    /// Gets the menu manager instance
    /// </summary>
    /// <returns>The menu manager or null if not initialized</returns>
    public MenuManager GetMenuManager() {
        return _menuManager;
    }

    /// <summary>
    /// Ensures that a specific Camera2D becomes the current camera for the viewport
    /// </summary>
    /// <param name="camera">The Camera2D to make current</param>
    public void SetCurrentCamera(Camera2D camera) {
        if (_viewport != null && camera != null) {
            camera.Enabled = true;
            camera.MakeCurrent();
            
            camera.PositionSmoothingEnabled = false;
            camera.RotationSmoothingEnabled = false;
            
            CallDeferred(nameof(UpdateViewportCamera));
            
            GD.Print($"GameRoot: Set camera {camera.GetPath()} as current with pixel-perfect movement");
        }
    }

    /// <summary>
    /// Forces the viewport to update its current camera
    /// </summary>
    private void UpdateViewportCamera() {
        if (_viewport != null) {
            _viewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
            _viewport.RenderTargetUpdateMode = SubViewport.UpdateMode.WhenVisible;
        }
    }

    /// <summary>
    /// Safely adds a child to the UI layer with error checking
    /// </summary>
    /// <param name="child">The control to add</param>
    /// <param name="forceReadableUniqueName">Whether to force readable unique name</param>
    public void AddToUiLayer(Node child, bool forceReadableUniqueName = false) {
        _uiLayer?.AddChild(child, forceReadableUniqueName);
    }

    /// <summary>
    /// Adds game content directly to the viewport (not in a CanvasLayer)
    /// </summary>
    /// <param name="child">The node to add</param>
    /// <param name="forceReadableUniqueName">Whether to force readable unique name</param>
    public void AddToViewport(Node child, bool forceReadableUniqueName = false) {
        _viewport?.AddChild(child, forceReadableUniqueName);
    }

    /// <summary>
    /// Clean up the scene cache when the node is removed.
    /// </summary>
    public override void _ExitTree() {
        base._ExitTree(); // Call AutoSingleton cleanup

        ClearSceneCache();
    }

    /// <summary>
    /// Preloads a scene and stores it in cache
    /// </summary>
    /// <param name="scenePath">Path to the scene file</param>
    /// <param name="cacheKey">Optional cache key (defaults to scenePath)</param>
    public void PreloadScene(string scenePath, string cacheKey = null) {
        cacheKey ??= scenePath;

        if (_sceneCache.ContainsKey(cacheKey)) {
            return;
        }

        if (_sceneCache.Count >= MaxCachedScenes) {
            var firstKey = _sceneCache.Keys.FirstOrDefault();
            if (firstKey != null) {
                _sceneCache.Remove(firstKey);
            }
        }

        var scene = GD.Load<PackedScene>(scenePath);
        if (scene != null) {
            _sceneCache[cacheKey] = scene;
            GD.Print($"Preloaded scene: {scenePath}");
        }
        else {
            GD.PrintErr($"Failed to preload scene: {scenePath}");
        }
    }

    /// <summary>
    /// Gets a cached scene or loads it if not cached
    /// </summary>
    /// <param name="scenePath">Path to the scene file</param>
    /// <param name="cacheKey">Optional cache key</param>
    /// <returns>The loaded scene or null if failed</returns>
    public PackedScene GetOrLoadScene(string scenePath, string cacheKey = null) {
        cacheKey ??= scenePath;

        if (_sceneCache.TryGetValue(cacheKey, out PackedScene cachedScene)) {
            return cachedScene;
        }

        var scene = GD.Load<PackedScene>(scenePath);
        if (scene != null && _sceneCache.Count < MaxCachedScenes) {
            _sceneCache[cacheKey] = scene;
        }

        return scene;
    }

    /// <summary>
    /// Clears the scene cache to free memory
    /// </summary>
    public void ClearSceneCache() {
        _sceneCache.Clear();
        GD.Print("Scene cache cleared");
    }

    /// <summary>
    /// Returns from the game to the main menu
    /// </summary>
    public void ReturnToMainMenu() {
        GetTree().Paused = false;
        
        ClosePauseMenus();
        
        // Clear viewport content except UiLayer
        if (_viewport != null) {
            foreach (Node child in _viewport.GetChildren()) {
                if (child.Name != "UiLayer") {
                    child.QueueFree();
                }
            }
        }

        _menuManager?.DestroyAllMenus();
        _menuManager?.ShowMenu(MenuManager.MenuType.MainMenu);
        MenuMusicManager.Instance?.ResumeMenuContext();
        ChangeGameState(GameState.MainMenu);
        
        GD.Print("GameRoot: Returned to main menu");
    }

    /// <summary>
    /// Closes all active pause menus and their overlays
    /// </summary>
    private void ClosePauseMenus() {
        var pauseMenuLayers = FindChildren("PauseMenuLayer", "", false);
        foreach (Node layer in pauseMenuLayers) {
            if (layer != null && IsInstanceValid(layer)) {
                layer.QueueFree();
                GD.Print("GameRoot: Closed pause menu layer");
            }
        }
        
        var pauseMenus = FindChildren("PauseMenu", "", true);
        foreach (Node menu in pauseMenus) {
            if (menu != null && IsInstanceValid(menu)) {
                var parent = menu.GetParent();
                if (parent != null && parent.GetType().Name == "CanvasLayer") {
                    parent.QueueFree();
                } else {
                    menu.QueueFree();
                }
                GD.Print("GameRoot: Closed pause menu");
            }
        }
    }

    /// <summary>
    /// Navigate to the main map scene.
    /// Stops menu music and loads the game scene.
    /// </summary>
    public void NavigateToMainMap() {
        MenuMusicManager.Instance?.StopMenuMusic();

        if (_viewport != null && _menuManager != null) {
            _menuManager.HideAllMenus();

            foreach (Node child in _viewport.GetChildren()) {
                if (child.Name != "UiLayer") {
                    child.QueueFree();
                }
            }
            
            var mainMapScene = GD.Load<PackedScene>("res://scenes/maps/MainMap.tscn");
            if (mainMapScene != null) {
                var mainMapInstance = mainMapScene.Instantiate();
                _viewport.AddChild(mainMapInstance);
                
                ChangeGameState(GameState.InGame);
                
                CallDeferred(nameof(EnsurePlayerCameraActive), mainMapInstance);
                
                GD.Print("GameRoot: MainMap loaded directly into Viewport");
            } else {
                GD.PrintErr("GameRoot: Failed to load MainMap scene");
            }
        }
    }

    /// <summary>
    /// Ensures the player camera is active after MainMap is loaded
    /// </summary>
    /// <param name="mainMapInstance">The instantiated MainMap scene</param>
    private void EnsurePlayerCameraActive(Node mainMapInstance) {
        var player = FindPlayerInNode(mainMapInstance);
        if (player != null) {
            var playerCamera = player.GetNodeOrNull<Camera2D>("PlayerCamera");
            if (playerCamera != null) {
                SetCurrentCamera(playerCamera);
                GD.Print("GameRoot: Player camera set as current");
            }
        }
    }

    /// <summary>
    /// Recursively searches for a Player node in the given node and its children
    /// </summary>
    /// <param name="node">The node to search in</param>
    /// <returns>The Player node if found, null otherwise</returns>
    private Node FindPlayerInNode(Node node) {
        if (node.Name == "Player" && node.HasMethod("_Ready")) {
            return node;
        }

        foreach (Node child in node.GetChildren()) {
            var result = FindPlayerInNode(child);
            if (result != null) {
                return result;
            }
        }

        return null;
    }

    /// <summary>
    /// Preloads commonly used scenes for better performance.
    /// </summary>
    public void PreloadCommonScenes() {
        PreloadScene("res://scenes/menus/MainMenu.tscn");
        PreloadScene("res://scenes/menus/SettingsMenu.tscn");
        PreloadScene("res://scenes/menus/PauseMenu.tscn");
        PreloadScene("res://scenes/menus/ControlsMenu.tscn");
        GD.Print("GameRoot: Common scenes preloaded");
    }
}
