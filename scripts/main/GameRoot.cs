using Godot;

/// <summary>
/// Root node of the game that manages the viewport and UI layers.
/// This class serves as the entry point for the game and handles high-level operations
/// like displaying menus and managing different game layers.
/// </summary>
public partial class GameRoot : Control
{
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
    /// Called when the node enters the scene tree for the first time.
    /// Shows the main menu.
    /// </summary>
    public override void _Ready()
    {
        // Preload common scenes for better navigation performance
        if (NavigationManager.Instance != null)
        {
            NavigationManager.Instance.PreloadCommonScenes();
        }

        ShowMainMenu();
    }

    /// <summary>
    /// Shows the main menu in the UI layer.
    /// </summary>
    private void ShowMainMenu()
    {
        if (MainMenuScene != null)
        {
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
        else
        {
            GD.PrintErr("MainMenuScene is not assigned in the editor!");
        }
    }

    /// <summary>
    /// Hides the main menu.
    /// </summary>
    public void HideMainMenu()
    {
        if (_mainMenuInstance != null)
            _mainMenuInstance.Visible = false;
    }

    /// <summary>
    /// Gets the UI layer for adding menu content.
    /// </summary>
    public CanvasLayer GetUiLayer()
    {
        return _uiLayer;
    }

    /// <summary>
    /// Gets the game layer for adding game content.
    /// </summary>
    public CanvasLayer GetGameLayer()
    {
        return _gameLayer;
    }

    /// <summary>
    /// Gets the viewport for adding 3D or game content.
    /// </summary>
    public new SubViewport GetViewport()
    {
        return _viewport;
    }
}
