using Godot;

/// <summary>
/// UI component for displaying an interaction prompt at a fixed screen position.
/// Can be added to any scene that needs to show interaction instructions.
/// Similar to FPSDisplayComponent, uses a Control node with a pre-configured Label child.
/// Style and positioning are configured directly in the Godot editor.
/// </summary>
public partial class InteractionPromptComponent : Control {

	private Label _promptLabel;

	/// <summary>
	/// Initializes the label reference.
	/// </summary>
	public override void _Ready() {
		_promptLabel = GetNode<Label>("PromptLabel");
		GD.Print($"InteractionPromptComponent._Ready() - Label found: {_promptLabel != null}");
	}

	/// <summary>
	/// Shows the prompt with the specified text.
	/// </summary>
	/// <param name="text">The text to display. If null or empty, keeps current text.</param>
	public void ShowPrompt(string text = null) {
		if (_promptLabel != null) {
			if (!string.IsNullOrEmpty(text)) {
				_promptLabel.Text = text;
			}
			_promptLabel.Visible = true;
			GD.Print($"Interaction prompt shown: {_promptLabel.Text}");
		}
	}

	/// <summary>
	/// Hides the prompt.
	/// </summary>
	public void HidePrompt() {
		if (_promptLabel != null) {
			_promptLabel.Visible = false;
			GD.Print("Interaction prompt hidden");
		}
	}
}
