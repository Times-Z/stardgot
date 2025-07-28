using Godot;

public partial class MainViewport : Control
{
    [Export] public NodePath PauseMenuPath;
    [Export] public NodePath TextureRectPath;
    [Export] public ShaderMaterial BlurMaterial;

    private Control _pauseMenu;
    private TextureRect _textureRect;
    private ShaderMaterial _defaultMaterial;

    public override void _Ready()
    {
        GD.Print("MainViewport _Ready");
        _pauseMenu = GetNode<Control>(PauseMenuPath);
        _textureRect = GetNode<TextureRect>(TextureRectPath);
        _defaultMaterial = _textureRect.Material as ShaderMaterial;

        var viewport = GetNode<Viewport>("Viewport");
        _textureRect.Texture = viewport.GetTexture();
    }

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
