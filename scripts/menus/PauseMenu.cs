using Godot;

/// <summary>
/// Handles the pause menu functionality and input processing during paused game state.
/// This class manages the display and dismissal of the pause menu, typically triggered
/// by the Escape key, and ensures proper cleanup when the menu is closed.
/// Integrates with GameRoot for scene transitions.
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
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape) {
            var root = GetTree().Root;
            var settingsOverlay = root?.GetNodeOrNull<CanvasLayer>("SettingsOverlay");
            if (settingsOverlay != null) {
                return; 
            }
            
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

        _blurBackground = GetNode<TextureRect>("BlurBackground");

        var resumeButton = GetNodeOrNull<Button>("CenterContainer/PausePanel/VBoxContainer/ButtonContainer/ResumeButton");
        if (resumeButton != null) {
            resumeButton.GrabFocus();
        }
    }

    /// <summary>
    /// Sets the screen texture for the blur background.
    /// Called by the pause system with a pre-captured screen texture.
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
        var parent = GetParent();
        bool isInOverlay = parent != null && (parent.Name == "PauseOverlay" || parent is CanvasLayer);
        
        if (isInOverlay) {
            var menuManager = GameRoot.Instance.GetMenuManager();
            menuManager.CloseOverlay("PauseOverlay");
        } else {
            var canvasLayer = GetParent();
            if (canvasLayer != null && canvasLayer.GetType().Name == "CanvasLayer") {
                canvasLayer.QueueFree();
            } else {
                QueueFree();
            }
            GetTree().Paused = false;
        }
    }

    /// <summary>
    /// Returns to the main menu using GameRoot.
    /// Unpauses the game and navigates to the main menu scene.
    /// </summary>
    public void ReturnToMainMenu() {
        ClosePauseMenu();
        GameRoot.Instance?.ReturnToMainMenu();
    }

    /// <summary>
    /// Opens the settings menu using MenuManager.
    /// </summary>
    public void OpenSettingsMenu() {;
        var parent = GetParent();
        if (parent is CanvasLayer canvasLayer) {
            canvasLayer.Visible = false;
        }
        GameRoot.Instance.GetMenuManager().ShowMenuAsOverlay(MenuManager.MenuType.SettingsMenu, "SettingsOverlay");
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
