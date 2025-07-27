using Godot;

public partial class MainMenu : Control
{
    [Export] private NodePath NewGameButtonPath = "VBoxContainer/NewGameButton";
    [Export] private NodePath QuitButtonPath = "VBoxContainer/QuitButton";
    [Export] private NodePath SettingsButtonPath = "VBoxContainer/SettingsButton";

    public override void _Ready()
    {
        GD.Print("MainMenu _Ready");

        var newGameButton = GetNode<Button>(NewGameButtonPath);
        var quitButton = GetNode<Button>(QuitButtonPath);
        var settingsButton = GetNode<Button>(SettingsButtonPath);

        newGameButton.Pressed += OnNewGameButtonPressed;
        quitButton.Pressed += OnQuitButtonPressed;
        settingsButton.Pressed += OnSettingsButtonPressed;
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }

    private void OnNewGameButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/MainMap.tscn");
    }

    private void OnSettingsButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Menus/SettingsMenu.tscn");
    }
}
