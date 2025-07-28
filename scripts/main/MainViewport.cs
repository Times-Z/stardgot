using Godot;

/// <summary>
/// Manages the main viewport of the game, handling the pause menu display
/// and visual effects like blur when the game is paused.
/// This class coordinates between the game view and the pause menu overlay.
/// </summary>
public partial class MainViewport : Control
{
    /// <summary>
    /// Node path to the pause menu control. Should be set in the Godot editor.
    /// </summary>
    [Export] public NodePath PauseMenuPath;
    
    /// <summary>
    /// Node path to the texture rectangle that displays the game view.
    /// Should be set in the Godot editor.
    /// </summary>
    [Export] public NodePath TextureRectPath;
    
    /// <summary>
    /// Shader material used to apply blur effect when the game is paused.
    /// Should be assigned in the Godot editor.
    /// </summary>
    [Export] public ShaderMaterial BlurMaterial;

    /// <summary>
    /// Reference to the pause menu control instance.
    /// </summary>
    private Control _pauseMenu;
    
    /// <summary>
    /// Reference to the texture rectangle that displays the game view.
    /// </summary>
    private TextureRect _textureRect;
    
    /// <summary>
    /// The default material of the texture rectangle (before blur is applied).
    /// </summary>
    private ShaderMaterial _defaultMaterial;

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// Initializes references to child nodes and sets up the viewport texture.
    /// </summary>
    public override void _Ready()
    {
        GD.Print("MainViewport _Ready");
        _pauseMenu = GetNode<Control>(PauseMenuPath);
        _textureRect = GetNode<TextureRect>(TextureRectPath);
        _defaultMaterial = _textureRect.Material as ShaderMaterial;

        var viewport = GetNode<Viewport>("Viewport");
        _textureRect.Texture = viewport.GetTexture();
    }

    /// <summary>
    /// Shows or hides the pause menu and applies/removes the blur effect.
    /// When showing the pause menu, applies a blur material to the background.
    /// When hiding it, restores the default material.
    /// </summary>
    /// <param name="show">True to show the pause menu with blur effect, false to hide it</param>
    public void ShowPauseMenu(bool show)
    {
        _pauseMenu.Visible = show;
        if (show)
        {
            _textureRect.Material = BlurMaterial;
        }
        else
        {
            _textureRect.Material = _defaultMaterial;
        }
    }
}
