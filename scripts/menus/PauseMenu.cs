using Godot;

/// <summary>
/// Handles the pause menu functionality and input processing during paused game state.
/// This class manages the display and dismissal of the pause menu, typically triggered
/// by the Escape key, and ensures proper cleanup when the menu is closed.
/// Integrates with NavigationManager for scene transitions.
/// </summary>
public partial class PauseMenu : Control {

    /// <summary>
    /// Reference to the blur background ColorRect for applying screen capture.
    /// </summary>
    private TextureRect _blurBackground;

    /// <summary>
    /// Processes input events, specifically handling the Escape key press to close the pause menu.
    /// When Escape is pressed, removes the CanvasLayer parent (if present) or the PauseMenu itself,
    /// and unpauses the game by setting GetTree().Paused to false.
    /// Only processes input if there's no settings overlay active.
    /// </summary>
    /// <param name="event">The input event to process</param>
    public override void _Input(InputEvent @event) {
        // Check if settings overlay is active - if so, don't handle escape
        var root = GetTree().Root;
        var settingsOverlay = root?.GetNodeOrNull<CanvasLayer>("SettingsOverlay");
        if (settingsOverlay != null) {
            return; // Let the settings menu handle input instead
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape) {
            ClosePauseMenu();
        }
    }

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// Configures the pause menu to process input even when the game is paused
    /// by setting ProcessMode to Always and enabling input processing.
    /// </summary>
    public override void _Ready() {
        GD.Print("PauseMenu _Ready");
        ProcessMode = ProcessModeEnum.Always;
        SetProcessInput(true);

        // Get reference to blur background
        _blurBackground = GetNode<TextureRect>("BlurBackground");

        // Set focus on button by default
        // Needed to make the menu accessible via keyboard
        var resumeButton = GetNodeOrNull<Button>("CenterContainer/PausePanel/VBoxContainer/ButtonContainer/ResumeButton");
        if (resumeButton != null) {
            resumeButton.GrabFocus();
        }
    }

    /// <summary>
    /// Sets the screen texture for the blur background.
    /// Called by NavigationManager with a pre-captured screen texture.
    /// </summary>
    /// <param name="texture">The screen texture to apply</param>
    public void SetScreenTexture(Texture2D texture) {
        if (_blurBackground != null && texture != null) {
            _blurBackground.Texture = texture;
        }
    }

    /// <summary>
    /// Closes the pause menu and resumes the game.
    /// </summary>
    public void ClosePauseMenu() {
        // Remove the CanvasLayer parent instead of just the PauseMenu
        var canvasLayer = GetParent();
        if (canvasLayer != null && canvasLayer.GetType().Name == "CanvasLayer") {
            canvasLayer.QueueFree();
        }
        else {
            QueueFree();
        }
        GetTree().Paused = false;
    }

    /// <summary>
    /// Returns to the main menu using NavigationManager.
    /// Unpauses the game and navigates to the main menu scene.
    /// </summary>
    public void ReturnToMainMenu() {
        GetTree().Paused = false;

        // Use NavigationManager to navigate to main menu
        if (NavigationManager.Instance != null) {
            NavigationManager.Instance.NavigateToMainMenu();
        }
        else {
            GD.PrintErr("NavigationManager instance not found");
        }
    }

    /// <summary>
    /// Opens the settings menu using NavigationManager.
    /// </summary>
    public void OpenSettingsMenu() {
        // Close pause menu first
        ClosePauseMenu();

        // Use NavigationManager to navigate to settings with pause context
        if (NavigationManager.Instance != null) {
            NavigationManager.Instance.NavigateToSettingsMenuWithContext("PauseMenu");
        }
        else {
            GD.PrintErr("NavigationManager instance not found");
        }
    }

    /// <summary>
    /// Called when the Resume button is pressed.
    /// Closes the pause menu and resumes the game.
    /// </summary>
    private void _on_resume_button_pressed() {
        ClosePauseMenu();
    }

    /// <summary>
    /// Called when the Settings button is pressed.
    /// Opens the settings menu.
    /// </summary>
    private void _on_settings_button_pressed() {
        OpenSettingsMenu();
    }

    /// <summary>
    /// Called when the Main Menu button is pressed.
    /// Returns to the main menu.
    /// </summary>
    private void _on_main_menu_button_pressed() {
        ReturnToMainMenu();
    }
    
    private void _on_exit_button_pressed() {
        GetTree().Quit();
    }
}
