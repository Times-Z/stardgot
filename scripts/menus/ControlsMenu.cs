using Godot;

using System.Collections.Generic;

/// <summary>
/// UI menu for configuring input controls.
/// Allows players to view and modify their key bindings.
/// </summary>
public partial class ControlsMenu : Control {
    /// <summary>
    /// Container for the control binding UI elements.
    /// </summary>
    [Export] private VBoxContainer _controlsContainer;

    /// <summary>
    /// Button to reset all controls to defaults.
    /// </summary>
    [Export] private Button _resetButton;

    /// <summary>
    /// Button to close the controls menu.
    /// </summary>
    [Export] private Button _backButton;

    /// <summary>
    /// Label to show instructions or status messages.
    /// </summary>
    [Export] private Label _statusLabel;

    /// <summary>
    /// Font size for action name labels.
    /// </summary>
    [Export] private int _labelFontSize = 18;

    /// <summary>
    /// Font size for key binding buttons.
    /// </summary>
    [Export] private int _buttonFontSize = 16;

    /// <summary>
    /// Currently active key binding button (when waiting for input).
    /// </summary>
    private Button _waitingForInputButton;

    /// <summary>
    /// The action name for the button waiting for input.
    /// </summary>
    private string _waitingForInputAction;

    /// <summary>
    /// Dictionary to keep track of action buttons for updates.
    /// </summary>
    private Dictionary<string, Button> _actionButtons = new();

    public override void _Ready() {
        GD.Print("ControlsMenu _Ready");

        _controlsContainer = GetNode<VBoxContainer>("VBoxContainer/ScrollContainer/ControlsContainer");
        _statusLabel = GetNode<Label>("VBoxContainer/StatusLabel");
        _resetButton = GetNode<Button>("VBoxContainer/ButtonContainer/ResetButton");
        _backButton = GetNode<Button>("VBoxContainer/ButtonContainer/BackButton");

        var parent = GetParent();
        if (parent is SettingsMenu) {
            GD.Print("ControlsMenu: Running as overlay on SettingsMenu");
            ProcessMode = ProcessModeEnum.Always;
            SetProcessInput(true);
        } else {
            GD.Print("ControlsMenu: Running as standalone scene");
        }

        PopulateControls();

        if (_resetButton != null) {
            _resetButton.Pressed += OnResetPressed;
        }

        if (_backButton != null) {
            _backButton.Pressed += OnBackPressed;
        }
    }

    /// <summary>
    /// Clean up signal connections when the node is removed from the tree.
    /// </summary>
    public override void _ExitTree() {
        if (_resetButton != null) {
            _resetButton.Pressed -= OnResetPressed;
        }

        if (_backButton != null) {
            _backButton.Pressed -= OnBackPressed;
        }
    }

    /// <summary>
    /// Populates the controls list with all configurable actions.
    /// </summary>
    private void PopulateControls() {
        if (InputManager.Instance == null) return;

        var actions = InputManager.Instance.GetAllActions();

        foreach (var action in actions) {
            CreateControlBinding(action.Key, action.Value);
        }
    }

    /// <summary>
    /// Creates a UI element for a single control binding.
    /// </summary>
    /// <param name="actionName">The action name</param>
    /// <param name="displayName">The human-readable display name</param>
    private void CreateControlBinding(string actionName, string displayName) {
        var container = new HBoxContainer();
        container.AddThemeConstantOverride("separation", 20);

        var nameLabel = new Label();
        nameLabel.Text = displayName + ":";
        nameLabel.CustomMinimumSize = new Vector2(200, 0);

        nameLabel.AddThemeFontSizeOverride("font_size", _labelFontSize);

        container.AddChild(nameLabel);

        var keyButton = new Button();
        keyButton.CustomMinimumSize = new Vector2(150, 40);
        keyButton.AddThemeFontSizeOverride("font_size", _buttonFontSize);

        UpdateKeyButtonText(keyButton, actionName);

        keyButton.Pressed += () => StartKeyRebinding(actionName, keyButton);

        container.AddChild(keyButton);
        _controlsContainer.AddChild(container);

        _actionButtons[actionName] = keyButton;
    }

    /// <summary>
    /// Updates the text on a key button to show current bindings.
    /// </summary>
    /// <param name="button">The button to update</param>
    /// <param name="actionName">The action name</param>
    private void UpdateKeyButtonText(Button button, string actionName) {
        if (InputManager.Instance == null) return;

        var keys = InputManager.Instance.GetKeysForAction(actionName);
        if (keys.Length == 0) {
            button.Text = "None";
        }
        else if (keys.Length == 1) {
            button.Text = InputManager.Instance.GetKeyDisplayName(keys[0]);
        }
        else {
            var keyNames = new string[keys.Length];
            for (int i = 0; i < keys.Length; i++) {
                keyNames[i] = InputManager.Instance.GetKeyDisplayName(keys[i]);
            }
            button.Text = string.Join(", ", keyNames);
        }
    }

    /// <summary>
    /// Starts the key rebinding process for an action.
    /// </summary>
    /// <param name="actionName">The action to rebind</param>
    /// <param name="button">The button that was pressed</param>
    private void StartKeyRebinding(string actionName, Button button) {
        _waitingForInputButton = button;
        _waitingForInputAction = actionName;
        button.Text = "Press any key...";
        _statusLabel.Text = $"Press a key to bind to {InputManager.Instance.GetActionDisplayName(actionName)}";
    }

    /// <summary>
    /// Handles input events when waiting for key rebinding.
    /// </summary>
    /// <param name="event">The input event</param>
    public override void _Input(InputEvent @event) {
        // Handle escape to close menu when not rebinding
        if (_waitingForInputButton == null && @event is InputEventKey keyEvent &&
            keyEvent.Pressed && keyEvent.Keycode == Key.Escape) {
            GetViewport().SetInputAsHandled();
            OnBackPressed();
            return;
        }

        if (_waitingForInputButton == null || _waitingForInputAction == null) return;

        if (@event is InputEventKey rebindKeyEvent && rebindKeyEvent.Pressed) {

            if (rebindKeyEvent.Keycode == Key.Shift || rebindKeyEvent.Keycode == Key.Ctrl ||
                rebindKeyEvent.Keycode == Key.Alt || rebindKeyEvent.Keycode == Key.Meta) {
                return;
            }

            // if (rebindKeyEvent.Keycode == Key.Escape) {
            //     CancelKeyRebinding();
            //     return;
            // }

            Key keyToUse = rebindKeyEvent.PhysicalKeycode != Key.None ? rebindKeyEvent.PhysicalKeycode : rebindKeyEvent.Keycode;
            bool success = InputManager.Instance.AssignKeyToAction(_waitingForInputAction, keyToUse);

            if (success) {
                _statusLabel.Text = $"Key binding updated successfully!";
                _statusLabel.Modulate = Colors.Green;
                InputManager.Instance.SaveInputConfig();
                UpdateKeyButtonText(_waitingForInputButton, _waitingForInputAction);
            }
            else {
                _statusLabel.Text = "Key already in use or invalid!";
                _statusLabel.Modulate = Colors.Red;
                UpdateKeyButtonText(_waitingForInputButton, _waitingForInputAction);
            }

            _waitingForInputButton = null;
            _waitingForInputAction = null;

            GetTree().CreateTimer(2.0).Timeout += () => {
                if (!IsInstanceValid(this) || _statusLabel == null) return;
                _statusLabel.Text = "Click on a key binding to change it";
                _statusLabel.Modulate = Colors.White;
            };

            GetViewport().SetInputAsHandled();
        }
    }

    /// <summary>
    /// Cancels the current key rebinding process.
    /// </summary>
    private void CancelKeyRebinding() {
        if (_waitingForInputButton != null && _waitingForInputAction != null) {
            UpdateKeyButtonText(_waitingForInputButton, _waitingForInputAction);
        }

        _waitingForInputButton = null;
        _waitingForInputAction = null;
        _statusLabel.Text = "Key binding cancelled";

        GetTree().CreateTimer(2.0).Timeout += () => {
            if (!IsInstanceValid(this) || _statusLabel == null) return;
            _statusLabel.Text = "Click on a key binding to change it";
        };
    }

    /// <summary>
    /// Handles the reset button press.
    /// </summary>
    private void OnResetPressed() {
        InputManager.Instance?.ResetToDefaults();

        foreach (var actionButton in _actionButtons) {
            UpdateKeyButtonText(actionButton.Value, actionButton.Key);
        }

        _statusLabel.Text = "Controls reset to defaults";

        GetTree().CreateTimer(2.0).Timeout += () => {
            if (!IsInstanceValid(this) || _statusLabel == null) return;
            _statusLabel.Text = "Click on a key binding to change it";
        };
    }

    /// <summary>
    /// Handles the back button press.
    /// </summary>
    private void OnBackPressed() {
        var parent = GetParent();
        if (parent is SettingsMenu) {
            GD.Print("ControlsMenu: Closing overlay");
            QueueFree();
        }
        else {
            GD.Print("ControlsMenu: Using NavigationManager to go back");
            CallDeferred(nameof(DeferredNavigateBack));
        }
    }

    /// <summary>
    /// Deferred navigation back to avoid signal handling issues.
    /// </summary>
    private void DeferredNavigateBack() {
        NavigationManager.Instance?.NavigateBack();
    }
}
