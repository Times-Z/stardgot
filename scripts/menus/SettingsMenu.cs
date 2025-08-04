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
	/// Node path to the show FPS toggle button.
	/// </summary>
	[Export] private NodePath ShowFPSButtonPath = "VBoxContainer/ShowFPSButton";

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
		var parent = GetParent();
		if (parent != null && parent.Name == "SettingsOverlay") {
			GD.Print("SettingsMenu: Running as overlay to preserve game state");
			// Enable input processing and set process mode to always for overlay
			ProcessMode = ProcessModeEnum.Always;
			SetProcessInput(true);
		}
		else {
			GD.Print("SettingsMenu: Running as full scene");
		}

		var backButton = GetNodeOrNull<Button>(BackButtonPath);
		var controlsButton = GetNodeOrNull<Button>(ControlsButtonPath);
		var showFPSButton = GetNodeOrNull<CheckButton>(ShowFPSButtonPath);

		if (backButton != null) {
			backButton.GrabFocus();
		}

		if (controlsButton != null) {
			controlsButton.Pressed += OnControlsButtonPressed;
		}

		if (showFPSButton != null) {
			// Initialize the button state from the FPSDisplay setting
			showFPSButton.ButtonPressed = FPSDisplay.ShowFPS;
			showFPSButton.Toggled += OnShowFPSToggled;
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

		var showFPSButton = GetNodeOrNull<CheckButton>(ShowFPSButtonPath);
		if (showFPSButton != null) {
			showFPSButton.Toggled -= OnShowFPSToggled;
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
		} else {
			GD.PrintErr("SettingsMenu: ControlsMenuScene is null!");
		}
	}

	/// <summary>
	/// Handles the show FPS toggle button event.
	/// Updates the global FPS display setting.
	/// </summary>
	private void OnShowFPSToggled(bool buttonPressed) {
		FPSDisplay.ShowFPS = buttonPressed;
		GD.Print($"FPS Display toggled: {buttonPressed}");
	}
}
