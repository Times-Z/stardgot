using Godot;
using System;

public partial class MainMenu : Control
{
    public override void _Ready()
    {
        GD.Print("MainMenu _Ready");
        var quitButton = GetNode<Button>("VBoxContainer/QuitButton");
        var newGameButton = GetNode<Button>("VBoxContainer/NewGameButton");
        newGameButton.Pressed += OnNewGameButtonPressed;
        quitButton.Pressed += OnQuitButtonPressed;
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }

    private void OnNewGameButtonPressed()
    {
        GD.Print("New Game button pressed");
        GetTree().ChangeSceneToFile("res://Scenes/MainMap.tscn");
    }
}
