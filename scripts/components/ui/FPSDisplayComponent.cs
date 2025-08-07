using Godot;

/// <summary>
/// UI component for displaying FPS counter in the top-left corner of the screen.
/// Similar to InteractionPromptComponent, can be added to game scenes to show performance metrics.
/// </summary>
public partial class FPSDisplayComponent : Control {

	private Label _fpsLabel;

	/// <summary>
	/// Initializes the FPS label and sets up default styling.
	/// </summary>
	public override void _Ready() {
		AddToGroup("fps_display");

		_fpsLabel = GetNode<Label>("FPSLabel");
		GD.Print($"FPSDisplayComponent._Ready() - Label found: {_fpsLabel != null}, ShowFPS: {ConfigurationManager.ShowFPS}");
		UpdateVisibility();
	}

	/// <summary>
	/// Updates the FPS display every frame.
	/// </summary>
	public override void _Process(double delta) {
		if (ConfigurationManager.ShowFPS && _fpsLabel != null && GameRoot.Instance.CurrentState == GameState.InGame) {
			var fps = Engine.GetFramesPerSecond();
			_fpsLabel.Text = $"FPS: {fps}";
		}
	}

	/// <summary>
	/// Updates the visibility of this FPS display based on the global ShowFPS setting.
	/// </summary>
	public void UpdateVisibility() {
		if (_fpsLabel != null) {
			_fpsLabel.Visible = ConfigurationManager.ShowFPS;
			GD.Print($"FPS Display visibility updated: {ConfigurationManager.ShowFPS}");
		}
	}

	/// <summary>
	/// Clean up when the node is removed from the tree.
	/// </summary>
	public override void _ExitTree() {
		RemoveFromGroup("fps_display");
	}
}
