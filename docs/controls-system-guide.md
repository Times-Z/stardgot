# Controls Remapping System - User Guide

## Overview

The controls remapping system allows players to fully customize their game keys. It includes:

- **InputManager**: Central manager for key mappings
- **ControlsMenu**: User interface for modifying controls
- Automatic save/load of preferences  
- Protection against key conflicts

## Features

### Implemented

1. **Basic key remapping**
   - Movement: `move_left`, `move_right`, `move_up`, `move_down`
   - Interaction: `interact`
   - Pause: `ui_cancel`

2. **Complete user interface**
   - Menu accessible from Settings > Controls
   - Real-time display of assigned keys
   - Visual status messages (success/error)
   - "Reset to Defaults" button

3. **Data persistence**
   - Automatic save to `user://input_config.cfg`
   - Automatic load at startup

4. **System protection**
   - Prevention of remapping system keys (Alt, Ctrl, etc.)
   - Key conflict detection
   - Cancel with Escape

### Current configurable actions

- **Move Left**: Left arrow + A (default)
- **Move Right**: Right arrow + D (default)
- **Move Up**: Up arrow + W (default)
- **Move Down**: Down arrow + S (default)
- **Interact**: E (default)
- **Pause/Cancel**: Escape (default)

## How to use

### For players

1. Go to **Settings > Controls**
2. Click on any key button
3. Press the desired new key
4. Configuration is automatically saved
5. Use "Reset to Defaults" if needed

### For developers

#### Adding a new action

1. **In project.godot**, add the action to `[input]`:
```ini
new_action={
"deadzone": 0.5,
"events": [Object(InputEventKey,"resource_local_to_scene":false,...)]
}
```

2. **In InputManager.cs**, add to dictionaries:
```csharp
// In _defaultKeyMappings
{ "new_action", new[] { Key.F } },

// In _actionDisplayNames  
{ "new_action", "New Action" }
```

3. **In game code**, use:
```csharp
if (Input.IsActionJustPressed("new_action")) {
    // Action logic
}
```

#### Using in code

```csharp
// Movement (recommended)
var direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");

// Single actions
if (Input.IsActionJustPressed("interact")) {
    TryInteract();
}

if (Input.IsActionPressed("ui_cancel")) {
    ShowPauseMenu();
}
```
