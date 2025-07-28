using Godot;

public partial class MainMenuOverlay : Control
{
    [Export] public PackedScene MainMenuScene;
    private Control _mainMenuInstance;

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

    public void HideMenu()
    {
        if (_mainMenuInstance != null)
            _mainMenuInstance.Visible = false;
    }
}
