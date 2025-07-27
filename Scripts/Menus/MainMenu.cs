using Godot;

public partial class MainMenu : Control
{
    [Export] private NodePath NewGameButtonPath = "VBoxContainer/NewGameButton";
    [Export] private NodePath QuitButtonPath = "VBoxContainer/QuitButton";
    [Export] private NodePath SettingsButtonPath = "VBoxContainer/SettingsButton";
    [Export] public PackedScene MainMapScene;
    [Export] public PackedScene SettingsMenuScene;

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
        var musicPlayer = GetNodeOrNull("/root/MenuMusicPlayer") as MenuMusicPlayer;
        musicPlayer?.StopMusic();
        if (MainMapScene != null)
            GetTree().ChangeSceneToPacked(MainMapScene);
        else
            GD.PrintErr("MainMapScene is not assigned in the editor!");
    }

    private void OnSettingsButtonPressed()
    {
        if (SettingsMenuScene != null)
            GetTree().ChangeSceneToPacked(SettingsMenuScene);
        else
            GD.PrintErr("SettingsMenuScene is not assigned in the editor!");
    }
}
