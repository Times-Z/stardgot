using Godot;

/// <summary>
/// Manages the settings menu interface and navigation.
/// This class handles the settings menu UI and provides functionality
/// to return to the main menu from the settings screen.
/// </summary>
public partial class SettingsMenu : Control {
	/// <summary>
	/// Node path to the back button that returns to the main menu.
	/// Should be set in the Godot editor.
	/// </summary>
	[Export] private NodePath BackButtonPath = "VBoxContainer/BackButton";

	/// <summary>
	/// Node path to the controls button that opens the controls menu.
	/// </summary>
	[Export] private NodePath ControlsButtonPath = "VBoxContainer/ControlsButton";

	/// <summary>
	/// Node path to the reset button that resets all settings to defaults.
	/// </summary>
	[Export] private NodePath ResetButtonPath = "VBoxContainer/ResetButton";

	/// <summary>
	/// Node path to the show FPS toggle button.
	/// </summary>
	[Export] private NodePath ShowFPSButtonPath = "VBoxContainer/ShowFPSButton";

	/// <summary>
	/// Node path to the fullscreen toggle button.
	/// </summary>
	[Export] private NodePath FullscreenButtonPath = "VBoxContainer/FullscreenButton";

	/// <summary>
	/// Node path to the vsync toggle button.
	/// </summary>
	[Export] private NodePath VsyncButtonPath = "VBoxContainer/VsyncButton";

	/// <summary>
	/// Node path to the master volume slider.
	/// </summary>
	[Export] private NodePath MasterVolumeSliderPath = "VBoxContainer/VolumeContainer/MasterVolumeSlider";

	/// <summary>
	/// Reference to the controls menu scene.
	/// </summary>
	[Export] private PackedScene ControlsMenuScene = GD.Load<PackedScene>("res://scenes/menus/ControlsMenu.tscn");

	/// <summary>
	/// Called when the node enters the scene tree for the first time.
	/// </summary>
	public override void _Ready() {
		GD.Print("SettingsMenu _Ready");

		// Check if we're in an overlay by looking at our parent hierarchy
		// avoiding circular references
		var parent = GetParent();
		if (parent != null && parent.Name == "SettingsOverlay") {
			GD.Print("SettingsMenu: Running as overlay to preserve game state");
			ProcessMode = ProcessModeEnum.Always;
			SetProcessInput(true);
		}
		else {
			GD.Print("SettingsMenu: Running as full scene");
		}

		var backButton = GetNodeOrNull<Button>(BackButtonPath);
		var controlsButton = GetNodeOrNull<Button>(ControlsButtonPath);
		var resetButton = GetNodeOrNull<Button>(ResetButtonPath);
		var showFPSButton = GetNodeOrNull<CheckButton>(ShowFPSButtonPath);
		var fullscreenButton = GetNodeOrNull<CheckButton>(FullscreenButtonPath);
		var vsyncButton = GetNodeOrNull<CheckButton>(VsyncButtonPath);
		var masterVolumeSlider = GetNodeOrNull<HSlider>(MasterVolumeSliderPath);

		if (backButton != null) {
			backButton.GrabFocus();
		}

		if (controlsButton != null) {
			controlsButton.Pressed += OnControlsButtonPressed;
		}

		if (resetButton != null) {
			resetButton.Pressed += OnResetButtonPressed;
		}

		if (showFPSButton != null) {
			showFPSButton.ButtonPressed = ConfigurationManager.ShowFPS;
			showFPSButton.Toggled += OnShowFPSToggled;
		}

		if (fullscreenButton != null) {
			fullscreenButton.ButtonPressed = ConfigurationManager.Fullscreen;
			fullscreenButton.Toggled += OnFullscreenToggled;
		}

		if (vsyncButton != null) {
			vsyncButton.ButtonPressed = ConfigurationManager.VSync;
			vsyncButton.Toggled += OnVsyncToggled;
		}

		if (masterVolumeSlider != null) {
			masterVolumeSlider.Value = ConfigurationManager.MasterVolume;
			masterVolumeSlider.ValueChanged += OnMasterVolumeChanged;
		}
	}

	/// <summary>
	/// Clean up signal connections when the node is removed from the tree.
	/// </summary>
	public override void _ExitTree() {
		var controlsButton = GetNodeOrNull<Button>(ControlsButtonPath);
		if (controlsButton != null) {
			controlsButton.Pressed -= OnControlsButtonPressed;
		}

		var resetButton = GetNodeOrNull<Button>(ResetButtonPath);
		if (resetButton != null) {
			resetButton.Pressed -= OnResetButtonPressed;
		}

		var showFPSButton = GetNodeOrNull<CheckButton>(ShowFPSButtonPath);
		if (showFPSButton != null) {
			showFPSButton.Toggled -= OnShowFPSToggled;
		}

		var fullscreenButton = GetNodeOrNull<CheckButton>(FullscreenButtonPath);
		if (fullscreenButton != null) {
			fullscreenButton.Toggled -= OnFullscreenToggled;
		}

		var vsyncButton = GetNodeOrNull<CheckButton>(VsyncButtonPath);
		if (vsyncButton != null) {
			vsyncButton.Toggled -= OnVsyncToggled;
		}

		var masterVolumeSlider = GetNodeOrNull<HSlider>(MasterVolumeSliderPath);
		if (masterVolumeSlider != null) {
			masterVolumeSlider.ValueChanged -= OnMasterVolumeChanged;
		}
	}

	/// <summary>
	/// Handle input events when in overlay mode.
	/// </summary>
	public override void _Input(InputEvent @event) {
		var parent = GetParent();
		if (parent != null && parent.Name == "SettingsOverlay") {
			if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape) {
				GetViewport().SetInputAsHandled();
			}
		}
	}

	/// <summary>
	/// Handles the back button press event.
	/// Uses the centralized NavigationManager to return to the previous context.
	/// This approach maintains PackedScene usage while avoiding circular references.
	/// </summary>
	public void _on_back_button_pressed() {
		CallDeferred(nameof(DeferredNavigateBack));
	}

	/// <summary>
	/// Deferred navigation back to avoid signal handling issues.
	/// </summary>
	private void DeferredNavigateBack() {
		NavigationManager.Instance.NavigateBack();
	}

	/// <summary>
	/// Handles the controls button press event.
	/// Shows the controls menu as an overlay.
	/// </summary>
	private void OnControlsButtonPressed() {
		if (ControlsMenuScene != null) {
			if (GetNodeOrNull("ControlsMenu") != null) {
				GD.Print("ControlsMenu already open, ignoring button press");
				return;
			}

			var controlsMenu = ControlsMenuScene.Instantiate<ControlsMenu>();
			controlsMenu.Name = "ControlsMenu";

			if (!IsInstanceValid(this)) {
				controlsMenu.QueueFree();
				return;
			}

			AddChild(controlsMenu);
			controlsMenu.ZIndex = 200;

			controlsMenu.SetAnchorsPreset(Control.LayoutPreset.FullRect);

			GD.Print("ControlsMenu overlay created successfully");
		}
		else {
			GD.PrintErr("SettingsMenu: ControlsMenuScene is null!");
		}
	}

	/// <summary>
	/// Handles the show FPS toggle button event.
	/// Updates the global FPS display setting through ConfigurationManager.
	/// </summary>
	private void OnShowFPSToggled(bool buttonPressed) {
		ConfigurationManager.ShowFPS = buttonPressed;
		GD.Print($"FPS Display toggled: {buttonPressed}");
	}

	/// <summary>
	/// Handles the fullscreen toggle button event.
	/// Updates the global fullscreen setting through ConfigurationManager.
	/// </summary>
	private void OnFullscreenToggled(bool buttonPressed) {
		ConfigurationManager.Fullscreen = buttonPressed;
		GD.Print($"Fullscreen toggled: {buttonPressed}");
	}

	/// <summary>
	/// Handles the VSync toggle button event.
	/// Updates the VSync setting through ConfigurationManager.
	/// </summary>
	private void OnVsyncToggled(bool buttonPressed) {
		ConfigurationManager.VSync = buttonPressed;
		GD.Print($"VSync toggled: {buttonPressed}");
	}

	/// <summary>
	/// Handles the master volume slider change event.
	/// Updates the master volume setting through ConfigurationManager.
	/// </summary>
	private void OnMasterVolumeChanged(double value) {
		ConfigurationManager.MasterVolume = (float)value;
		GD.Print($"Master volume changed: {value:F2}");
	}

	/// <summary>
	/// Handles the reset button press event.
	/// Resets all settings to their default values and updates the UI.
	/// </summary>
	private void OnResetButtonPressed() {
		ConfigurationManager.Instance.ResetToDefaults();

		var showFPSButton = GetNodeOrNull<CheckButton>(ShowFPSButtonPath);
		var fullscreenButton = GetNodeOrNull<CheckButton>(FullscreenButtonPath);
		var vsyncButton = GetNodeOrNull<CheckButton>(VsyncButtonPath);
		var masterVolumeSlider = GetNodeOrNull<HSlider>(MasterVolumeSliderPath);

		if (showFPSButton != null) {
			showFPSButton.ButtonPressed = ConfigurationManager.ShowFPS;
		}

		if (fullscreenButton != null) {
			fullscreenButton.ButtonPressed = ConfigurationManager.Fullscreen;
		}

		if (vsyncButton != null) {
			vsyncButton.ButtonPressed = ConfigurationManager.VSync;
		}

		if (masterVolumeSlider != null) {
			masterVolumeSlider.Value = ConfigurationManager.MasterVolume;
		}

		GD.Print("Settings reset to defaults");
	}
}
