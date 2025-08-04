using Godot;

/// <summary>
/// UI component for displaying FPS counter in the top-left corner of the screen.
/// Similar to InteractionPrompt, can be added to game scenes to show performance metrics.
/// </summary>
public partial class FPSDisplay : CanvasLayer {
	[Export] public Color BackgroundColor { get; set; } = new Color(0, 0, 0, 0.7f);
	[Export] public Color TextColor { get; set; } = new Color(1, 1, 1);
	[Export] public int CornerRadius { get; set; } = 5;
	[Export] public Vector2 LabelOffset { get; set; } = new Vector2(10, 10);
	[Export] public Vector2 LabelSize { get; set; } = new Vector2(120, 30);
	[Export] public int FontSize { get; set; } = 16;

	private Label _fpsLabel;
	private static bool _showFPS = false;

	/// <summary>
	/// Gets or sets whether the FPS display should be visible.
	/// This is a global setting that affects all FPSDisplay instances.
	/// </summary>
	public static bool ShowFPS {
		get => _showFPS;
		set {
			_showFPS = value;
			// Notify all instances to update their visibility
			var tree = Engine.GetMainLoop() as SceneTree;
			if (tree != null) {
				var instances = tree.GetNodesInGroup("fps_display");
				foreach (FPSDisplay instance in instances) {
					instance.UpdateVisibility();
				}
			}
		}
	}

	/// <summary>
	/// Initializes the FPS label and sets up default styling.
	/// </summary>
	public override void _Ready() {
		// Add this instance to the fps_display group
		AddToGroup("fps_display");

		_fpsLabel = new Label {
			Text = "FPS: 60",
			Visible = _showFPS,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			Modulate = TextColor
		};
		AddChild(_fpsLabel);
		ConfigureLabelPosition();
		ApplyLabelStyling();
		UpdateVisibility();
	}

	/// <summary>
	/// Updates the FPS display every frame.
	/// </summary>
	public override void _Process(double delta) {
		if (_showFPS && _fpsLabel != null) {
			var fps = Engine.GetFramesPerSecond();
			_fpsLabel.Text = $"FPS: {fps}";
		}
	}

	/// <summary>
	/// Updates the visibility of this FPS display based on the global ShowFPS setting.
	/// </summary>
	public void UpdateVisibility() {
		if (_fpsLabel != null) {
			_fpsLabel.Visible = _showFPS;
		}
	}

	/// <summary>
	/// Updates the styling of the FPS display with new parameters.
	/// Call this after changing Export properties to apply the changes.
	/// </summary>
	public void UpdateStyling() {
		if (_fpsLabel != null) {
			_fpsLabel.Modulate = TextColor;
			ConfigureLabelPosition();
			ApplyLabelStyling();
		}
	}

	/// <summary>
	/// Configures the FPS display with specific parameters. Useful for runtime configuration.
	/// </summary>
	/// <param name="textColor">Color of the text</param>
	/// <param name="backgroundColor">Background color of the display</param>
	/// <param name="fontSize">Font size</param>
	public void ConfigureDisplay(Color? textColor = null, Color? backgroundColor = null, int? fontSize = null) {
		if (textColor.HasValue) TextColor = textColor.Value;
		if (backgroundColor.HasValue) BackgroundColor = backgroundColor.Value;
		if (fontSize.HasValue) FontSize = fontSize.Value;
		
		UpdateStyling();
	}

	private void ConfigureLabelPosition() {
		// Position in top-left corner
		_fpsLabel.AnchorLeft = 0.0f;
		_fpsLabel.AnchorRight = 0.0f;
		_fpsLabel.AnchorTop = 0.0f;
		_fpsLabel.AnchorBottom = 0.0f;
		_fpsLabel.OffsetLeft = LabelOffset.X;
		_fpsLabel.OffsetRight = LabelOffset.X + LabelSize.X;
		_fpsLabel.OffsetTop = LabelOffset.Y;
		_fpsLabel.OffsetBottom = LabelOffset.Y + LabelSize.Y;
	}

	private void ApplyLabelStyling() {
		var theme = GD.Load<Theme>("res://assets/themes/maintheme.tres");
		if (theme != null) {
			_fpsLabel.Theme = theme;
		}

		var styleBox = new StyleBoxFlat {
			BgColor = BackgroundColor,
			CornerRadiusTopLeft = CornerRadius,
			CornerRadiusTopRight = CornerRadius,
			CornerRadiusBottomLeft = CornerRadius,
			CornerRadiusBottomRight = CornerRadius,
			ContentMarginLeft = 8,
			ContentMarginRight = 8,
			ContentMarginTop = 4,
			ContentMarginBottom = 4
		};
		_fpsLabel.AddThemeStyleboxOverride("normal", styleBox);
		_fpsLabel.AddThemeFontSizeOverride("font_size", FontSize);
	}

	/// <summary>
	/// Clean up when the node is removed from the tree.
	/// </summary>
	public override void _ExitTree() {
		RemoveFromGroup("fps_display");
	}
}
