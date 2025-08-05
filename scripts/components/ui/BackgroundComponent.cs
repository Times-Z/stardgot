using Godot;

/// <summary>
/// Reusable background component for UI screens and menus.
/// Provides configurable background textures, colors, and effects like blur.
/// </summary>
public partial class BackgroundComponent : Control {
	/// <summary>
	/// The background texture to display.
	/// </summary>
	[Export] public Texture2D BackgroundTexture { get; set; }

	/// <summary>
	/// Background color to display if no texture is set, or to tint the texture.
	/// </summary>	
	[Export] public Color BackgroundColor { get; set; } = Colors.Black;

	/// <summary>
	/// Whether to use a solid color background instead of texture.
	/// </summary>
	[Export] public bool UseSolidColor { get; set; } = false;

	/// <summary>
	/// How the background texture should be stretched/expanded.
	/// </summary>
	[Export] public TextureRect.ExpandModeEnum ExpandMode { get; set; } = TextureRect.ExpandModeEnum.FitWidthProportional;

	/// <summary>
	/// How the background texture should be stretched.
	/// </summary>
	[Export] public TextureRect.StretchModeEnum StretchMode { get; set; } = TextureRect.StretchModeEnum.KeepAspectCentered;

	/// <summary>
	/// Whether to apply a blur effect to the background.
	/// </summary>
	[Export] public bool ApplyBlur { get; set; } = false;

	/// <summary>
	/// The blur material to use (should be a ShaderMaterial with blur shader).
	/// </summary>
	[Export] public ShaderMaterial BlurMaterial { get; set; }

	/// <summary>
	/// Opacity/transparency of the background (0.0 = fully transparent, 1.0 = fully opaque).
	/// </summary>
	[Export] public float Opacity { get; set; } = 1.0f;

	/// <summary>
	/// Whether the background should fill the entire parent container.
	/// </summary>
	[Export] public bool FillContainer { get; set; } = true;

	/// <summary>
	/// The background control (TextureRect or ColorRect).
	/// </summary>
	private Control _backgroundControl;

	/// <summary>
	/// Called when the node enters the scene tree.
	/// Creates and configures the background based on settings.
	/// </summary>
	public override void _Ready() {
		if (FillContainer) {
			SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		}

		CreateBackground();
		ApplySettings();
	}

	/// <summary>
	/// Creates the appropriate background control based on settings.
	/// </summary>
	private void CreateBackground() {
		// Remove existing background if any
		if (_backgroundControl != null) {
			_backgroundControl.QueueFree();
			_backgroundControl = null;
		}

		if (UseSolidColor || BackgroundTexture == null) {
			CreateColorBackground();
		} else {
			CreateTextureBackground();
		}
	}

	/// <summary>
	/// Creates a solid color background using ColorRect.
	/// </summary>
	private void CreateColorBackground() {
		var colorRect = new ColorRect();
		colorRect.Color = BackgroundColor;
		
		if (FillContainer) {
			colorRect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		}

		AddChild(colorRect);
		_backgroundControl = colorRect;
	}

	/// <summary>
	/// Creates a textured background using TextureRect.
	/// </summary>
	private void CreateTextureBackground() {
		var textureRect = new TextureRect();
		textureRect.Texture = BackgroundTexture;
		textureRect.ExpandMode = ExpandMode;
		textureRect.StretchMode = StretchMode;
		textureRect.Modulate = BackgroundColor; // Use color as tint
		
		if (FillContainer) {
			textureRect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		}

		AddChild(textureRect);
		_backgroundControl = textureRect;
	}

	/// <summary>
	/// Applies current settings to the background control.
	/// </summary>
	private void ApplySettings() {
		if (_backgroundControl == null) return;

		// Apply opacity
		var modulate = _backgroundControl.Modulate;
		modulate.A = Opacity;
		_backgroundControl.Modulate = modulate;

		// Apply blur if enabled and material is provided
		if (ApplyBlur && BlurMaterial != null) {
			_backgroundControl.Material = BlurMaterial;
		} else {
			_backgroundControl.Material = null;
		}
	}

	/// <summary>
	/// Updates the background texture at runtime.
	/// </summary>
	/// <param name="newTexture">The new texture to use</param>
	/// <param name="forceTextureMode">Whether to switch to texture mode even if using solid color</param>
	public void SetBackgroundTexture(Texture2D newTexture, bool forceTextureMode = false) {
		BackgroundTexture = newTexture;
		
		if (forceTextureMode) {
			UseSolidColor = false;
		}

		if (IsInsideTree()) {
			CreateBackground();
			ApplySettings();
		}
	}
}
