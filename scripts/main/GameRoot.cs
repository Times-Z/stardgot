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
public partial class GameRoot : Control {
    /// <summary>
    /// The packed scene for the main menu that will be instantiated at runtime.
    /// This should be assigned in the Godot editor.
    /// </summary>
    [Export] public PackedScene MainMenuScene;

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
    /// Reference to the game layer.
    /// </summary>
    [Export] private CanvasLayer _gameLayer;

    /// <summary>
    /// Reference to the instantiated main menu.
    /// </summary>
    private Control _mainMenuInstance;

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
        // Initialize singleton pattern
        InitializeSingleton();

        // Validate node references
        if (!ValidateNodeReferences()) {
            GD.PrintErr("GameRoot: Critical node references are missing!");
            return;
        }

        // Preload common scenes for better navigation performance
        if (NavigationManager.Instance != null) {
            NavigationManager.Instance.PreloadCommonScenes();
        }

        // Start menu music when the game launches (ensure menu context)
        MenuMusicManager.Instance?.ResumeMenuContext();

        // Set initial state and show main menu
        CurrentState = GameState.MainMenu;
        ShowMainMenu();
    }

    /// <summary>
    /// Validates that all required node references are properly set
    /// </summary>
    /// <returns>True if all references are valid, false otherwise</returns>
    private bool ValidateNodeReferences() {
        bool isValid = true;

        if (_viewportContainer == null) {
            GD.PrintErr("GameRoot: _viewportContainer is null");
            isValid = false;
        }
        if (_viewport == null) {
            GD.PrintErr("GameRoot: _viewport is null");
            isValid = false;
        }
        if (_uiLayer == null) {
            GD.PrintErr("GameRoot: _uiLayer is null");
            isValid = false;
        }
        if (_gameLayer == null) {
            GD.PrintErr("GameRoot: _gameLayer is null");
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// Shows the main menu in the UI layer.
    /// </summary>
    private void ShowMainMenu() {
        if (MainMenuScene != null) {
            _mainMenuInstance = MainMenuScene.Instantiate<Control>();
            _uiLayer.AddChild(_mainMenuInstance);
            _mainMenuInstance.Visible = true;
            GD.Print(
                $"MainMenu instantiated! Visible: {_mainMenuInstance.Visible}, " +
                $"Rect: {_mainMenuInstance.GetRect()}, " +
                $"Anchors: L={_mainMenuInstance.AnchorLeft} " +
                $"T={_mainMenuInstance.AnchorTop} " +
                $"R={_mainMenuInstance.AnchorRight} " +
                $"B={_mainMenuInstance.AnchorBottom}"
            );
        }
        else {
            GD.PrintErr("MainMenuScene is not assigned in the editor!");
        }
    }

    /// <summary>
    /// Hides the main menu.
    /// </summary>
    public void HideMainMenu() {
        if (_mainMenuInstance != null) {
            _mainMenuInstance.Visible = false;
            _mainMenuInstance.QueueFree();
            _mainMenuInstance = null;
        }
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
        if (_uiLayer == null) {
            GD.PrintErr("UiLayer reference is null! Check GameRoot configuration.");
        }
        return _uiLayer;
    }

    /// <summary>
    /// Gets the game layer for adding game content.
    /// </summary>
    public CanvasLayer GetGameLayer() {
        if (_gameLayer == null) {
            GD.PrintErr("GameLayer reference is null! Check GameRoot configuration.");
        }
        return _gameLayer;
    }

    /// <summary>
    /// Gets the viewport for adding 3D or game content.
    /// </summary>
    public new SubViewport GetViewport() {
        if (_viewport == null) {
            GD.PrintErr("Viewport reference is null! Check GameRoot configuration.");
        }
        return _viewport;
    }

    /// <summary>
    /// Safely adds a child to the UI layer with error checking
    /// </summary>
    /// <param name="child">The control to add</param>
    /// <param name="forceReadableUniqueName">Whether to force readable unique name</param>
    public void AddToUiLayer(Node child, bool forceReadableUniqueName = false) {
        var uiLayer = GetUiLayer();
        if (uiLayer != null && child != null) {
            uiLayer.AddChild(child, forceReadableUniqueName);
        }
        else {
            GD.PrintErr($"Failed to add child to UI layer. UiLayer: {uiLayer != null}, Child: {child != null}");
        }
    }

    /// <summary>
    /// Safely adds a child to the game layer with error checking
    /// </summary>
    /// <param name="child">The node to add</param>
    /// <param name="forceReadableUniqueName">Whether to force readable unique name</param>
    public void AddToGameLayer(Node child, bool forceReadableUniqueName = false) {
        var gameLayer = GetGameLayer();
        if (gameLayer != null && child != null) {
            gameLayer.AddChild(child, forceReadableUniqueName);
        }
        else {
            GD.PrintErr($"Failed to add child to Game layer. GameLayer: {gameLayer != null}, Child: {child != null}");
        }
    }

    /// <summary>
    /// Singleton instance for easy access throughout the application.
    /// </summary>
    public static GameRoot Instance { get; private set; }

    /// <summary>
    /// Initialize the singleton instance.
    /// </summary>
    private void InitializeSingleton() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            GD.PrintErr("Multiple GameRoot instances detected! This should not happen.");
        }
    }

    /// <summary>
    /// Clean up the singleton instance when the node is removed.
    /// </summary>
    public override void _ExitTree() {
        if (Instance == this) {
            Instance = null;
        }

        ClearSceneCache();

        base._ExitTree();
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
}
