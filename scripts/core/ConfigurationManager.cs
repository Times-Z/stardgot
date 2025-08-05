using Godot;

using System.Collections.Generic;

/// <summary>
/// Manages input remapping and application settings. Provides functionality for players to customize 
/// their controls and configure display/audio preferences. This class handles saving/loading both 
/// input mappings and general settings to provide a unified configuration system.
/// </summary>
public partial class ConfigurationManager : Node {

    /// <summary>
    /// Singleton instance of ConfigurationManager.
    /// </summary>
    public static ConfigurationManager Instance { get; private set; }

    /// <summary>
    /// Configuration file path for saving custom input mappings and settings.
    /// </summary>
    private const string ConfigPath = "user://config.cfg";

    /// <summary>
    /// Default settings values for the application.
    /// These values are used when no configuration file exists or when resetting settings.
    /// </summary>
    private static bool _showFPS = false;
    private static float _masterVolume = 1.0f;
    private static bool _fullscreen = false;
    private static bool _vsync = true;

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

    /// <summary>
    /// Default settings values for all application settings.
    /// </summary>
    private readonly Dictionary<string, object> _defaultSettings = new() {
        { "show_fps", false },
        { "fullscreen", false },
        { "vsync", true },

        { "master_volume", 1.0f },
        { "music_volume", 0.8f },
        { "sfx_volume", 1.0f },

        { "auto_save", true },
        { "difficulty", "normal" }
    };

    /// <summary>
    /// Gets or sets whether the FPS display should be visible.
    /// This setting is automatically saved and loaded.
    /// </summary>
    public static bool ShowFPS {
        get => _showFPS;
        set {
            _showFPS = value;

            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree != null) {
                var instances = tree.GetNodesInGroup("fps_display");
                foreach (FPSDisplayComponent instance in instances) {
                    instance.UpdateVisibility();
                }
            }
            
            Instance?.SaveConfiguration();
        }
    }

    /// <summary>
    /// Gets or sets the master volume (0.0 to 1.0).
    /// </summary>
    public static float MasterVolume {
        get => _masterVolume;
        set {
            _masterVolume = Mathf.Clamp(value, 0.0f, 1.0f);
            ApplyMasterVolume();
            Instance?.SaveConfiguration();
        }
    }

    /// <summary>
    /// Gets or sets whether the game should run in fullscreen mode.
    /// </summary>
    public static bool Fullscreen {
        get => _fullscreen;
        set {
            _fullscreen = value;
            if (value) {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            }
            else {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            }
            Instance?.SaveConfiguration();
        }
    }

    /// <summary>
    /// Gets or sets whether VSync should be enabled.
    /// </summary>
    public static bool VSync {
        get => _vsync;
        set {
            _vsync = value;
            DisplayServer.WindowSetVsyncMode(value ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
            Instance?.SaveConfiguration();
        }
    }

    /// <summary>
    /// Applies the master volume setting to the audio system.
    /// Converts the 0.0-1.0 range to decibels and updates all audio players.
    /// </summary>
    private static void ApplyMasterVolume() {
        // convert 0.0-1.0 range to dB
        // 1 = 0 dB that is the maximum volume
        float volumeDb = _masterVolume <= 0.0f ? -80.0f : Mathf.LinearToDb(_masterVolume);

        var masterBusIndex = AudioServer.GetBusIndex("Master");
        AudioServer.SetBusVolumeDb(masterBusIndex, volumeDb);

        if (MenuMusicManager.Instance != null) {
            MenuMusicManager.Instance.SetVolume(volumeDb);
        }

        GD.Print($"Applied master volume: {_masterVolume:F2} ({volumeDb:F1}dB)");
    }

    /// <summary>
    /// Public method to force volume application. Useful for newly created audio components.
    /// </summary>
    public static void RefreshAudioVolume() {
        ApplyMasterVolume();
    }

    public override void _Ready() {
        GD.Print("ConfigurationManager _Ready");
        GD.Print($"Config dir {OS.GetUserDataDir()}");

        if (Instance == null) {
            Instance = this;
            ValidateAndRepairConfiguration();
            LoadConfiguration();
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
    public string GetActionDisplayName(string actionName) => 
        _actionDisplayNames.TryGetValue(actionName, out string displayName) ? displayName : actionName;

    /// <summary>
    /// Gets all configurable actions.
    /// </summary>
    /// <returns>Dictionary of action names and their display names</returns>
    public Dictionary<string, string> GetAllActions() => new Dictionary<string, string>(_actionDisplayNames);

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
            GD.PrintErr($"ConfigurationManager: Action '{actionName}' not found in InputMap");
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
    /// Resets all settings and actions to their default values.
    /// </summary>
    public void ResetToDefaults() {
        // Reset input mappings
        foreach (var mapping in _defaultKeyMappings) {
            if (!InputMap.HasAction(mapping.Key)) {
                GD.PrintErr($"ConfigurationManager: Action '{mapping.Key}' not found in InputMap during reset");
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

        ResetSettingsToDefaults();

        SaveConfiguration();
        GD.Print("ConfigurationManager: All settings reset to defaults");
    }

    /// <summary>
    /// Resets only the application settings to their default values (not input mappings).
    /// </summary>
    public void ResetSettingsToDefaults() {
        _showFPS = (bool)_defaultSettings["show_fps"];
        _masterVolume = (float)_defaultSettings["master_volume"];
        _fullscreen = (bool)_defaultSettings["fullscreen"];
        _vsync = (bool)_defaultSettings["vsync"];

        // Notify all FPS display components to update visibility
        var tree = Engine.GetMainLoop() as SceneTree;
        if (tree != null) {
            var instances = tree.GetNodesInGroup("fps_display");
            foreach (FPSDisplayComponent instance in instances) {
                instance.UpdateVisibility();
            }
        }

        if (_fullscreen) {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        }
        else {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }

        DisplayServer.WindowSetVsyncMode(_vsync ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);

        ApplyMasterVolume();

        GD.Print("ConfigurationManager: Settings reset to defaults");
    }

    /// <summary>
    /// Saves the current input configuration and settings to file.
    /// </summary>
    public void SaveConfiguration() {
        var config = new ConfigFile();

        foreach (var actionName in _actionDisplayNames.Keys) {
            var keys = GetKeysForAction(actionName);
            var keyInts = new int[keys.Length];
            for (int i = 0; i < keys.Length; i++) {
                keyInts[i] = (int)keys[i];
            }
            config.SetValue("input", actionName, keyInts);
        }

        config.SetValue("display", "show_fps", _showFPS);
        config.SetValue("display", "fullscreen", _fullscreen);
        config.SetValue("display", "vsync", _vsync);

        config.SetValue("audio", "master_volume", _masterVolume);

        var error = config.Save(ConfigPath);
        if (error != Error.Ok) {
            GD.PrintErr($"ConfigurationManager: Failed to save configuration - {error}");
        }
        else {
            GD.Print("ConfigurationManager: Configuration saved successfully");
        }
    }

    /// <summary>
    /// Loads input configuration and settings from file.
    /// </summary>
    private void LoadConfiguration() {
        EnsureActionsExist();

        var config = new ConfigFile();
        var error = config.Load(ConfigPath);

        if (error != Error.Ok) {
            GD.Print("ConfigurationManager: No config file found, using defaults");
            ResetSettingsToDefaults();
            return;
        }

        foreach (var actionName in _actionDisplayNames.Keys) {
            if (!InputMap.HasAction(actionName)) {
                GD.PrintErr($"ConfigurationManager: Action '{actionName}' not found in InputMap");
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

        _showFPS = config.GetValue("display", "show_fps", (bool)_defaultSettings["show_fps"]).AsBool();
        _fullscreen = config.GetValue("display", "fullscreen", (bool)_defaultSettings["fullscreen"]).AsBool();
        _vsync = config.GetValue("display", "vsync", (bool)_defaultSettings["vsync"]).AsBool();

        _masterVolume = config.GetValue("audio", "master_volume", (float)_defaultSettings["master_volume"]).AsSingle();

        var tree = Engine.GetMainLoop() as SceneTree;
        if (tree != null) {
            var instances = tree.GetNodesInGroup("fps_display");
            foreach (FPSDisplayComponent instance in instances) {
                instance.UpdateVisibility();
            }
        }

        GD.Print($"ConfigurationManager: ShowFPS loaded as {_showFPS}");

        if (_fullscreen) {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        }
        else {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }

        DisplayServer.WindowSetVsyncMode(_vsync ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);

        ApplyMasterVolume();

        GD.Print($"ConfigurationManager: Configuration loaded - ShowFPS: {_showFPS}, Fullscreen: {_fullscreen}, VSync: {_vsync}, MasterVolume: {_masterVolume}");
    }

    /// <summary>
    /// Ensures all required actions exist in the InputMap.
    /// Creates them if they don't exist.
    /// </summary>
    private void EnsureActionsExist() {
        foreach (var mapping in _defaultKeyMappings) {
            if (!InputMap.HasAction(mapping.Key)) {
                GD.Print($"ConfigurationManager: Creating missing action '{mapping.Key}'");
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

    /// <summary>
    /// Gets a setting value with a fallback to default.
    /// </summary>
    /// <typeparam name="T">The type of the setting</typeparam>
    /// <param name="section">Configuration section</param>
    /// <param name="key">Setting key</param>
    /// <returns>The setting value or default</returns>
    public T GetSetting<T>(string section, string key) where T : struct {
        var config = new ConfigFile();
        var error = config.Load(ConfigPath);

        if (error != Error.Ok || !config.HasSectionKey(section, key)) {
            string defaultKey = key;
            if (_defaultSettings.ContainsKey(defaultKey)) {
                return (T)_defaultSettings[defaultKey];
            }
            return default(T);
        }

        var value = config.GetValue(section, key);
        if (typeof(T) == typeof(bool)) {
            return (T)(object)value.AsBool();
        }
        else if (typeof(T) == typeof(float)) {
            return (T)(object)value.AsSingle();
        }
        else if (typeof(T) == typeof(int)) {
            return (T)(object)value.AsInt32();
        }

        return default(T);
    }

    /// <summary>
    /// Sets a setting value and saves the configuration.
    /// </summary>
    /// <param name="section">Configuration section</param>
    /// <param name="key">Setting key</param>
    /// <param name="value">Setting value</param>
    public void SetSetting(string section, string key, object value) {
        var config = new ConfigFile();
        config.Load(ConfigPath);

        switch (value) {
            case bool boolValue:
                config.SetValue(section, key, boolValue);
                break;
            case float floatValue:
                config.SetValue(section, key, floatValue);
                break;
            case int intValue:
                config.SetValue(section, key, intValue);
                break;
            case string stringValue:
                config.SetValue(section, key, stringValue);
                break;
            default:
                GD.PrintErr($"ConfigurationManager: Unsupported value type for setting {section}.{key}");
                return;
        }

        var error = config.Save(ConfigPath);
        if (error != Error.Ok) {
            GD.PrintErr($"ConfigurationManager: Failed to save setting {section}.{key} - {error}");
        }
    }

    /// <summary>
    /// Validates the configuration file and repairs any missing or invalid entries.
    /// </summary>
    public void ValidateAndRepairConfiguration() {
        var config = new ConfigFile();
        var error = config.Load(ConfigPath);
        bool needsSave = false;

        if (error != Error.Ok) {
            GD.Print("ConfigurationManager: Creating new configuration file");
            config = new ConfigFile();
            needsSave = true;
        }

        foreach (var defaultSetting in _defaultSettings) {
            string section = GetSectionForSetting(defaultSetting.Key);
            if (config.HasSectionKey(section, defaultSetting.Key)) {
                continue;
            }

            switch (defaultSetting.Value) {
                case bool boolValue:
                    config.SetValue(section, defaultSetting.Key, boolValue);
                    break;
                case float floatValue:
                    config.SetValue(section, defaultSetting.Key, floatValue);
                    break;
                case int intValue:
                    config.SetValue(section, defaultSetting.Key, intValue);
                    break;
                case string stringValue:
                    config.SetValue(section, defaultSetting.Key, stringValue);
                    break;
            }

            needsSave = true;
            GD.Print($"ConfigurationManager: Repaired missing setting {section}.{defaultSetting.Key}");
        }

        if (needsSave) {
            config.Save(ConfigPath);
            GD.Print("ConfigurationManager: Configuration file validated and repaired");
        }
    }

    /// <summary>
    /// Gets the appropriate configuration section for a setting key.
    /// </summary>
    /// <param name="settingKey">The setting key</param>
    /// <returns>The configuration section name</returns>
    private string GetSectionForSetting(string settingKey) {
        return settingKey switch {
            "show_fps" or "fullscreen" or "vsync" => "display",
            "master_volume" or "music_volume" or "sfx_volume" => "audio",
            "auto_save" or "difficulty" => "gameplay",
            _ => "general"
        };
    }
}
