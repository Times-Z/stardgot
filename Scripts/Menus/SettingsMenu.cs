using Godot;

public partial class SettingsMenu : Control
{
	[Export] private NodePath BackButtonPath = "VBoxContainer/BackButton";
	// [Export] public PackedScene MainMenuScene;

	public override void _Ready()
	{
		GD.Print("SettingsMenu _Ready");

		var backButton = GetNode<Button>(BackButtonPath);

		backButton.Pressed += OnBackButtonPressed;
	}

	private void OnBackButtonPressed()
	{
		// if (MainMenuScene != null)
		// 	GetTree().ChangeSceneToPacked(MainMenuScene);
		// else
			GD.PrintErr("MainMenuScene is not assigned in the editor!");
	}
}
