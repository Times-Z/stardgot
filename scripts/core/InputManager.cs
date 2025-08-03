using Godot;

using System.Collections.Generic;

/// <summary>
/// Manages input remapping and provides functionality for players to customize their controls.
/// This class handles saving/loading input mappings and provides methods to change key bindings at runtime.
/// </summary>
public partial class InputManager : Node {
    /// <summary>
    /// Singleton instance of InputManager.
    /// </summary>
    public static InputManager Instance { get; private set; }

    /// <summary>
    /// Configuration file path for saving custom input mappings.
    /// </summary>
    private const string InputConfigPath = "user://input_config.cfg";

    /// <summary>
    /// Default key mappings for all game actions.
    /// Supports both QWERTY and AZERTY keyboard layouts.
    /// </summary>
    private readonly Dictionary<string, Key[]> _defaultKeyMappings = new() {
        // Supports both QWERTY and AZERTY layouts
        { "move_left", new[] { Key.Left, Key.A, Key.Q } },
        { "move_right", new[] { Key.Right, Key.D } },
        { "move_up", new[] { Key.Up, Key.W, Key.Z } },
        { "move_down", new[] { Key.Down, Key.S } },
        { "interact", new[] { Key.E } },
        { "ui_cancel", new[] { Key.Escape } }
    };

    /// <summary>
    /// Human-readable names for actions (for UI display).
    /// </summary>
    private readonly Dictionary<string, string> _actionDisplayNames = new() {
        { "move_left", "Move Left" },
        { "move_right", "Move Right" },
        { "move_up", "Move Up" },
        { "move_down", "Move Down" },
        { "interact", "Interact" },
        { "ui_cancel", "Pause/Cancel" }
    };

    public override void _Ready() {
        GD.Print("InputManager _Ready");
        GD.Print($"Config dir {OS.GetUserDataDir()}");

        if (Instance == null) {
            Instance = this;
            LoadInputConfig();
        }
        else {
            QueueFree();
        }
    }

    /// <summary>
    /// Gets the display name for an action.
    /// </summary>
    /// <param name="actionName">The action name</param>
    /// <returns>Human-readable display name</returns>
    public string GetActionDisplayName(string actionName) {
        return _actionDisplayNames.TryGetValue(actionName, out string displayName) ? displayName : actionName;
    }

    /// <summary>
    /// Gets all configurable actions.
    /// </summary>
    /// <returns>Dictionary of action names and their display names</returns>
    public Dictionary<string, string> GetAllActions() {
        return new Dictionary<string, string>(_actionDisplayNames);
    }

    /// <summary>
    /// Gets the current key(s) assigned to an action.
    /// </summary>
    /// <param name="actionName">The action name</param>
    /// <returns>Array of keys assigned to the action</returns>
    public Key[] GetKeysForAction(string actionName) {
        if (!InputMap.HasAction(actionName)) return new Key[0];

        var events = InputMap.ActionGetEvents(actionName);
        var keys = new List<Key>();

        foreach (var inputEvent in events) {
            if (inputEvent is InputEventKey keyEvent) {
                Key key = keyEvent.PhysicalKeycode != Key.None ? keyEvent.PhysicalKeycode : keyEvent.Keycode;
                keys.Add(key);
            }
        }

        return keys.ToArray();
    }

    /// <summary>
    /// Assigns a new key to an action, replacing all existing keys.
    /// </summary>
    /// <param name="actionName">The action name</param>
    /// <param name="newKey">The new key to assign</param>
    /// <returns>True if successful, false if the key is already used by another action</returns>
    public bool AssignKeyToAction(string actionName, Key newKey) {

        if (!InputMap.HasAction(actionName)) {
            GD.PrintErr($"InputManager: Action '{actionName}' not found in InputMap");
            return false;
        }

        if (IsSystemKey(newKey)) {
            return false;
        }

        foreach (var action in _actionDisplayNames.Keys) {
            if (action == actionName) continue;

            var currentKeys = GetKeysForAction(action);
            if (System.Array.Exists(currentKeys, k => k == newKey)) {
                return false;
            }
        }

        InputMap.ActionEraseEvents(actionName);

        var keyEvent = new InputEventKey();
        keyEvent.PhysicalKeycode = newKey;
        keyEvent.Keycode = newKey;
        InputMap.ActionAddEvent(actionName, keyEvent);

        return true;
    }

    /// <summary>
    /// Adds an additional key to an action (for multi-key bindings).
    /// </summary>
    /// <param name="actionName">The action name</param>
    /// <param name="additionalKey">The additional key to add</param>
    /// <returns>True if successful, false if the key is already used</returns>
    public bool AddKeyToAction(string actionName, Key additionalKey) {
        var currentKeys = GetKeysForAction(actionName);
        if (System.Array.Exists(currentKeys, k => k == additionalKey)) {
            return false;
        }

        foreach (var action in _actionDisplayNames.Keys) {
            if (action == actionName) continue;

            var actionKeys = GetKeysForAction(action);
            if (System.Array.Exists(actionKeys, k => k == additionalKey)) {
                return false;
            }
        }

        var keyEvent = new InputEventKey();
        keyEvent.PhysicalKeycode = (Key)additionalKey;
        InputMap.ActionAddEvent(actionName, keyEvent);

        return true;
    }

    /// <summary>
    /// Resets all actions to their default key mappings.
    /// </summary>
    public void ResetToDefaults() {
        foreach (var mapping in _defaultKeyMappings) {
            if (!InputMap.HasAction(mapping.Key)) {
                GD.PrintErr($"InputManager: Action '{mapping.Key}' not found in InputMap during reset");
                continue;
            }

            InputMap.ActionEraseEvents(mapping.Key);

            foreach (var key in mapping.Value) {
                var keyEvent = new InputEventKey();
                keyEvent.PhysicalKeycode = key;
                keyEvent.Keycode = key;
                InputMap.ActionAddEvent(mapping.Key, keyEvent);
            }
        }

        SaveInputConfig();
    }

    /// <summary>
    /// Saves the current input configuration to file.
    /// </summary>
    public void SaveInputConfig() {
        var config = new ConfigFile();

        foreach (var actionName in _actionDisplayNames.Keys) {
            var keys = GetKeysForAction(actionName);
            var keyInts = new int[keys.Length];
            for (int i = 0; i < keys.Length; i++) {
                keyInts[i] = (int)keys[i];
            }
            config.SetValue("input", actionName, keyInts);
        }

        config.Save(InputConfigPath);
    }

    /// <summary>
    /// Loads input configuration from file.
    /// </summary>
    private void LoadInputConfig() {
        EnsureActionsExist();

        var config = new ConfigFile();
        var error = config.Load(InputConfigPath);

        if (error != Error.Ok) {
            GD.Print("InputManager: No config file found, using defaults");
            return;
        }

        foreach (var actionName in _actionDisplayNames.Keys) {
            if (!InputMap.HasAction(actionName)) {
                GD.PrintErr($"InputManager: Action '{actionName}' not found in InputMap");
                continue;
            }

            if (config.HasSectionKey("input", actionName)) {
                var keyInts = config.GetValue("input", actionName).AsInt32Array();

                InputMap.ActionEraseEvents(actionName);

                foreach (var keyInt in keyInts) {
                    var keyEvent = new InputEventKey();
                    keyEvent.PhysicalKeycode = (Key)keyInt;
                    keyEvent.Keycode = (Key)keyInt;
                    InputMap.ActionAddEvent(actionName, keyEvent);
                }
            }
        }
    }

    /// <summary>
    /// Ensures all required actions exist in the InputMap.
    /// Creates them if they don't exist.
    /// </summary>
    private void EnsureActionsExist() {
        foreach (var mapping in _defaultKeyMappings) {
            if (!InputMap.HasAction(mapping.Key)) {
                GD.Print($"InputManager: Creating missing action '{mapping.Key}'");
                InputMap.AddAction(mapping.Key);

                foreach (var key in mapping.Value) {
                    var keyEvent = new InputEventKey();
                    keyEvent.PhysicalKeycode = key;
                    keyEvent.Keycode = key;
                    InputMap.ActionAddEvent(mapping.Key, keyEvent);
                }
            }
        }
    }

    /// <summary>
    /// Checks if a key is a system key that shouldn't be remapped.
    /// </summary>
    /// <param name="key">The key to check</param>
    /// <returns>True if it's a system key</returns>
    private bool IsSystemKey(Key key) {
        return key == Key.Alt || key == Key.Ctrl || key == Key.Shift ||
               key == Key.Meta || key == Key.Tab || key == Key.Enter ||
               key == Key.Backspace || key == Key.Delete;
    }

    /// <summary>
    /// Gets a user-friendly name for a key.
    /// </summary>
    /// <param name="key">The key to get name for</param>
    /// <returns>User-friendly key name</returns>
    public string GetKeyDisplayName(Key key) {
        return key switch {
            Key.Left => "Left Arrow",
            Key.Right => "Right Arrow",
            Key.Up => "Up Arrow",
            Key.Down => "Down Arrow",
            Key.Space => "Space",
            Key.Escape => "Escape",
            Key.Enter => "Enter",
            Key.Backspace => "Backspace",
            Key.Delete => "Delete",
            Key.Shift => "Shift",
            Key.Ctrl => "Ctrl",
            Key.Alt => "Alt",
            _ => key.ToString()
        };
    }
}
