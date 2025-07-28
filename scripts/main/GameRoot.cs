using Godot;

/// <summary>
/// Root node of the game that manages the main viewport and overall game state.
/// This class serves as the entry point for the game and handles high-level operations
/// like pausing the game and managing the main viewport.
/// </summary>
public partial class GameRoot : Control
{
    /// <summary>
    /// The packed scene for the main viewport that will be instantiated at runtime.
    /// This should be assigned in the Godot editor.
    /// </summary>
    [Export] public PackedScene MainViewportScene;
    
    /// <summary>
    /// Reference to the instantiated main viewport instance.
    /// </summary>
    private MainViewport _mainViewport;

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// Instantiates the main viewport scene and adds it as a child.
    /// Also preloads common scenes for better performance.
    /// </summary>
    public override void _Ready()
    {
        // Preload common scenes for better navigation performance
        if (NavigationManager.Instance != null)
        {
            NavigationManager.Instance.PreloadCommonScenes();
        }

        if (MainViewportScene != null)
        {
            _mainViewport = MainViewportScene.Instantiate<MainViewport>();
            AddChild(_mainViewport);
        }
        else
        {
            GD.PrintErr("MainViewportScene is not assigned in the editor!");
        }
    }

    /// <summary>
    /// Pauses or unpauses the game by showing/hiding the pause menu.
    /// </summary>
    /// <param name="pause">True to pause the game and show the pause menu, false to unpause</param>
    public void PauseGame(bool pause)
    {
        if (_mainViewport != null)
            _mainViewport.ShowPauseMenu(pause);
    }
}
