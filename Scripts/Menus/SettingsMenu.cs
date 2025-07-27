using Godot;

public partial class SettingsMenu : Control
{
	[Export] private NodePath BackButtonPath = "VBoxContainer/BackButton";

	// Todo : need to be set in the editor but not fonctional yet
	[Export] private string MainMenuScenePath = "res://Scenes/Menus/MainMenu.tscn";

	public override void _Ready()
	{
		GD.Print("SettingsMenu _Ready");

		var backButton = GetNode<Button>(BackButtonPath);

		backButton.Pressed += OnBackButtonPressed;
	}

	private void OnBackButtonPressed()
	{
		if (!string.IsNullOrEmpty(MainMenuScenePath))
		{
			GetTree().ChangeSceneToFile(MainMenuScenePath);
		}
		else
		{
			GD.PrintErr("MainMenuScenePath is not assigned or is empty!");
		}
	}
}
