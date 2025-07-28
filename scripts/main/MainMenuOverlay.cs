using Godot;

/// <summary>
/// Manages the main menu overlay that appears on top of other game content.
/// This class handles the instantiation and display of the main menu scene
/// within a canvas layer for proper UI layering.
/// </summary>
public partial class MainMenuOverlay : Control
{
    /// <summary>
    /// The packed scene containing the main menu UI.
    /// This should be assigned in the Godot editor.
    /// </summary>
    [Export] public PackedScene MainMenuScene;
    
    /// <summary>
    /// Reference to the instantiated main menu control instance.
    /// </summary>
    private Control _mainMenuInstance;

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// Creates a canvas layer and instantiates the main menu scene within it.
    /// Provides debug information about the menu's properties.
    /// </summary>
    public override void _Ready()
    {
        var canvasLayer = new CanvasLayer();
        AddChild(canvasLayer);
        if (MainMenuScene != null)
        {
            _mainMenuInstance = MainMenuScene.Instantiate<Control>();
            canvasLayer.AddChild(_mainMenuInstance);
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
    /// Hides the main menu by setting its visibility to false.
    /// The menu instance remains in memory but becomes invisible.
    /// </summary>
    public void HideMenu()
    {
        if (_mainMenuInstance != null)
            _mainMenuInstance.Visible = false;
    }
}
