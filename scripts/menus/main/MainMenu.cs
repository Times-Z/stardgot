using Godot;
using System;

public partial class MainMenu : Control
{
    public override void _Ready()
    {
        GD.Print("MainMenu _Ready");
        var quitButton = GetNode<Button>("VBoxContainer/QuitButton");
        quitButton.Pressed += OnQuitButtonPressed;
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
