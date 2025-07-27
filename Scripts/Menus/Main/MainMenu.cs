using Godot;

public partial class MainMenu : Control
{
    [Export] private NodePath NewGameButtonPath = "VBoxContainer/NewGameButton";
    [Export] private NodePath QuitButtonPath = "VBoxContainer/QuitButton";

    public override void _Ready()
    {
        GD.Print("MainMenu _Ready");

        var newGameButton = GetNode<Button>(NewGameButtonPath);
        var quitButton = GetNode<Button>(QuitButtonPath);

        newGameButton.Pressed += OnNewGameButtonPressed;
        quitButton.Pressed += OnQuitButtonPressed;
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }

    private void OnNewGameButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/MainMap.tscn");
    }
}
