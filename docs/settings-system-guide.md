# ConfigurationManager - Unified Configuration System Guide

## Overview

The `ConfigurationManager` is a robust and extensible system that manages all application settings. It provides a unified solution for controls, display settings, audio, and gameplay parameters with automatic validation and configuration repair.

## Features

### Implemented

1. **Unified and robust configuration**
   - Automatic saving to `user://config.cfg`
   - Automatic validation and repair of configuration on startup
   - Automatic loading on startup

2. **Available settings**
   - **Display**: FPS, Fullscreen, VSync
   - **Audio**: Master volume, music, sound effects
   - **Gameplay**: Auto-save, difficulty
   - **Controls**: Customizable key mappings

3. **Advanced architecture**
   - Centralized default values
   - Generic API for adding new settings
   - Automatic configuration file validation
   - Robust error handling
   - Supported data types: bool, float, int, string

## File Structure

### User configuration
Settings are saved to `user://config.cfg` with the following structure:

```ini
[input]
move_left=[65,81,16777231]
move_right=[68,16777233]
move_up=[87,90,16777232]
move_down=[83,16777234]
interact=[69]
ui_cancel=[16777217]

[display]
show_fps=false
fullscreen=false
vsync=true

[audio]
master_volume=1.0
music_volume=0.8
sfx_volume=1.0

; not currently implemented
[gameplay]
auto_save=true
difficulty="normal"
```

### Modified files

1. **`scripts/core/ConfigurationManager.cs`** (renamed and significantly improved)
   - Formerly `InputManager.cs`
   - Centralized default values system with `_defaultSettings`
   - Static properties for all settings: `ShowFPS`, `MasterVolume`, `Fullscreen`
   - Extended `SaveConfiguration()` and `LoadConfiguration()` methods
   - Automatic validation and repair with `ValidateAndRepairConfiguration()`
   - Generic API with `GetSetting<T>()` and `SetSetting()`
   - Unified management of settings and controls

2. **`scripts/menus/SettingsMenu.cs`** (modified)
   - Now uses `ConfigurationManager.ShowFPS`
   - Automatic saving when modifications are made

3. **`scripts/menus/ControlsMenu.cs`** (modified)
   - Updated to use `ConfigurationManager` instead of `InputManager`

4. **`scripts/core/InputManager.cs`** (deleted/renamed)
   - Renamed to `ConfigurationManager.cs` to reflect its extended function

## Usage

### Accessing existing settings
```csharp
// Display settings
bool showFPS = ConfigurationManager.ShowFPS;
bool fullscreen = ConfigurationManager.Fullscreen;

// Audio settings
float masterVolume = ConfigurationManager.MasterVolume;

// Modify a setting (automatic saving)
ConfigurationManager.ShowFPS = true;
ConfigurationManager.MasterVolume = 0.8f;
```

### Using the generic API
```csharp
// Read any setting
bool autoSave = ConfigurationManager.Instance.GetSetting<bool>("gameplay", "auto_save");
float musicVolume = ConfigurationManager.Instance.GetSetting<float>("audio", "music_volume");

// Set any setting
ConfigurationManager.Instance.SetSetting("gameplay", "difficulty", "hard");
ConfigurationManager.Instance.SetSetting("display", "vsync", false);
```

### Adding new settings

1. **Add the default value** in `_defaultSettings`
```csharp
private readonly Dictionary<string, object> _defaultSettings = new() {
    // Existing settings...
    { "new_setting", defaultValue }
};
```

2. **Add a static property** (optional, for easy access)
```csharp
public static bool NewSetting {
    get => _newSetting;
    set {
        _newSetting = value;
        Instance?.SaveConfiguration();
    }
}
private static bool _newSetting = false;
```

3. **Update `SaveConfiguration()`** to include the new setting
```csharp
config.SetValue("section", "new_setting", _newSetting);
```

4. **Update `LoadConfiguration()`** to load the new setting
```csharp
_newSetting = config.GetValue("section", "new_setting", (bool)_defaultSettings["new_setting"]).AsBool();
```

5. **Update `ResetSettingsToDefaults()`** if necessary
```csharp
_newSetting = (bool)_defaultSettings["new_setting"];
```
