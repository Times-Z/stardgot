using Godot;

/// <summary>
/// UI component for displaying an interaction prompt above the player or at a fixed screen position.
/// Can be added to any scene that needs to show interaction instructions.
/// </summary>
public partial class InteractionPrompt : CanvasLayer {
	[Export] public string PromptText { get; set; } = "";
	[Export] public Color BackgroundColor { get; set; } = new Color(0, 0, 0, 0.5f);
	[Export] public Color TextColor { get; set; } = new Color(1, 1, 1);
	[Export] public int CornerRadius { get; set; } = 3;
	[Export] public Vector2 LabelOffset { get; set; } = new Vector2(-75, -80);
	[Export] public Vector2 LabelSize { get; set; } = new Vector2(150, 40);
	[Export] public int FontSize { get; set; } = 18;

	private Label _label;

	/// <summary>
	/// Initializes the label and sets up default styling.
	/// </summary>
	public override void _Ready() {
		_label = new Label {
			Text = PromptText,
			Visible = false,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			Modulate = TextColor
		};
		AddChild(_label);
		ConfigureLabelPosition();
		ApplyLabelStyling();
	}

	/// <summary>
	/// Shows the prompt with the specified text.
	/// </summary>
	public void ShowPrompt(string text) {
		_label.Text = text;
		_label.Visible = true;
	}

	/// <summary>
	/// Hides the prompt.
	/// </summary>
	public void HidePrompt() {
		_label.Visible = false;
	}

	/// <summary>
	/// Updates the styling of the prompt with new parameters.
	/// Call this after changing Export properties to apply the changes.
	/// </summary>
	public void UpdateStyling() {
		if (_label != null) {
			_label.Modulate = TextColor;
			ConfigureLabelPosition();
			ApplyLabelStyling();
		}
	}

	/// <summary>
	/// Configures the prompt with specific parameters. Useful for runtime configuration.
	/// </summary>
	/// <param name="textColor">Color of the text</param>
	/// <param name="backgroundColor">Background color of the prompt</param>
	/// <param name="fontSize">Font size</param>
	public void ConfigurePrompt(Color? textColor = null, Color? backgroundColor = null, int? fontSize = null) {
		if (textColor.HasValue) TextColor = textColor.Value;
		if (backgroundColor.HasValue) BackgroundColor = backgroundColor.Value;
		if (fontSize.HasValue) FontSize = fontSize.Value;
		
		UpdateStyling();
	}

	private void ConfigureLabelPosition() {
		_label.AnchorLeft = 0.5f;
		_label.AnchorRight = 0.5f;
		_label.AnchorTop = 1.0f;
		_label.AnchorBottom = 1.0f;
		_label.OffsetLeft = LabelOffset.X;
		_label.OffsetRight = LabelOffset.X + LabelSize.X;
		_label.OffsetTop = LabelOffset.Y;
		_label.OffsetBottom = LabelOffset.Y + LabelSize.Y;
	}

	private void ApplyLabelStyling() {
		var theme = GD.Load<Theme>("res://assets/themes/maintheme.tres");
		if (theme != null) {
			_label.Theme = theme;
		}
		var styleBox = new StyleBoxFlat {
			BgColor = BackgroundColor,
			CornerRadiusTopLeft = CornerRadius,
			CornerRadiusTopRight = CornerRadius,
			CornerRadiusBottomLeft = CornerRadius,
			CornerRadiusBottomRight = CornerRadius,
			ContentMarginLeft = 6,
			ContentMarginRight = 6,
			ContentMarginTop = 3,
			ContentMarginBottom = 3
		};
		_label.AddThemeStyleboxOverride("normal", styleBox);
		_label.AddThemeFontSizeOverride("font_size", FontSize);
	}
}
