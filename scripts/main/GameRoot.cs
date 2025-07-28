using Godot;

public partial class GameRoot : Control
{
    [Export] public PackedScene MainViewportScene;
    private MainViewport _mainViewport;

    public override void _Ready()
    {
        // Instancie le viewport principal
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

    public void PauseGame(bool pause)
    {
        if (_mainViewport != null)
            _mainViewport.ShowPauseMenu(pause);
    }
}
