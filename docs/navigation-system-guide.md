# Navigation System Architecture Guide

## Overview

The Stardgot project implements a sophisticated dual navigation system that handles both menu navigation within the GameRoot system and scene-level transitions. This architecture provides seamless user experiences across different contexts while maintaining proper state management and resource efficiency.

## Architecture Components

### 1. MenuManager (Primary Navigation)
**Location**: `res://scripts/ui/MenuManager.cs`  
**Scope**: Menu navigation within GameRoot viewport  
**Singleton**: Instance-based (child of GameRoot)

#### Responsibilities
- **Menu Scene Management**: Caching and instantiation of menu scenes
- **Navigation History**: Stack-based back navigation with proper context
- **Visibility Control**: Show/hide menus with proper layering
- **Memory Management**: Efficient menu instance reuse

#### Supported Menus
```csharp
public enum MenuType {
    None,
    MainMenu,        // Main game menu
    PauseMenu,       // In-game pause menu
    SettingsMenu,    // Settings configuration
    ControlsMenu     // Input controls configuration
}
```

#### Key Features
- **Scene Caching**: All menu scenes preloaded for instant switching
- **Navigation Stack**: Proper back button functionality with history
- **Integration**: Works seamlessly with GameRoot's UiLayer
- **Lifecycle Management**: Automatic cleanup and memory management

### 2. NavigationManager (Secondary Navigation)
**Location**: `res://scripts/core/NavigationManager.cs`  
**Scope**: Scene-level navigation and context overlays  
**Singleton**: Autoload singleton

#### Responsibilities
- **Scene Transitions**: Major scene changes (Menu → Game, Game → Menu)
- **Context Overlays**: Settings overlay from pause menu preserving game state
- **Music Management**: Coordinates with MenuMusicManager for audio transitions
- **Player Integration**: Manages player references for pause functionality

#### Scene Management
```csharp
public static class ScenePaths {
    public const string MainMenu = "res://scenes/menus/MainMenu.tscn";
    public const string SettingsMenu = "res://scenes/menus/SettingsMenu.tscn";
    public const string PauseMenu = "res://scenes/menus/PauseMenu.tscn";
    public const string MainMap = "res://scenes/maps/MainMap.tscn";
    public const string GameRoot = "res://scenes/GameRoot.tscn";
}
```

## Navigation Flow Patterns

### 1. Menu Navigation (MenuManager)
```
MainMenu ↔ SettingsMenu ↔ ControlsMenu
    ↓ (NavigationManager)
  Game Scene
```

**Example Usage**:
```csharp
// In MainMenu.cs
var menuManager = GameRoot.Instance?.GetMenuManager();
menuManager?.ShowMenu(MenuManager.MenuType.SettingsMenu);

// Automatic back navigation
menuManager?.GoBack(); // Returns to previous menu in history
```

### 2. Game Transitions (NavigationManager)
```
MainMenu → MainMap (scene change)
MainMap → Settings Overlay (preserves game state)
Settings Overlay → MainMap (resumes game)
```

**Example Usage**:
```csharp
// Start new game
NavigationManager.Instance.NavigateToMainMap();

// Settings from pause (overlay mode)
NavigationManager.Instance.NavigateToSettingsMenuWithContext("PauseMenu");

// Return to main menu
NavigationManager.Instance.NavigateToMainMenu();
```

## Context-Aware Features

### Settings Overlay System
The navigation system provides context-aware settings access:

#### From Main Menu
- **Behavior**: Standard menu navigation within GameRoot
- **Implementation**: MenuManager handles transition
- **State**: No game state to preserve

#### From Pause Menu  
- **Behavior**: Overlay mode that preserves game state
- **Implementation**: NavigationManager creates CanvasLayer overlay
- **State**: Game remains loaded in background, music paused

```csharp
// Context detection in SettingsMenu.cs
var parent = GetParent();
if (parent != null && parent.Name == "SettingsOverlay") {
    // Running as overlay - preserve game state
    ProcessMode = ProcessModeEnum.Always;
} else {
    // Running as standard menu
}
```

### Music Context Management
Integration with MenuMusicManager for seamless audio:

```csharp
// NavigationManager coordinates music transitions
public void NavigateToMainMap() {
    MenuMusicManager.Instance?.StopMenuMusic(); // Stop menu music
    // Load game scene - game handles its own music
}

public void NavigateToMainMenu() {
    MenuMusicManager.Instance?.ResumeMenuContext(); // Resume menu music
}
```

## Advanced Features

### Scene Caching
Both navigation systems implement intelligent scene caching:

**MenuManager Caching**:
```csharp
// Preloads all menu scenes on startup
private void PreloadMenuScenes() {
    foreach (var kvp in MenuScenePaths) {
        var scene = GD.Load<PackedScene>(kvp.Value);
        if (scene != null) {
            _menuSceneCache[kvp.Key] = scene;
        }
    }
}
```

**NavigationManager Caching**:
```csharp
// Dynamic caching with size limits
public PackedScene GetOrLoadScene(string scenePath) {
    if (_sceneCache.TryGetValue(scenePath, out PackedScene cachedScene)) {
        return cachedScene; // Return cached version
    }
    // Load and cache if space available
}
```

### Navigation History
MenuManager maintains a navigation stack for proper back functionality:

```csharp
public void ShowMenu(MenuType menuType, bool hideOthers = true) {
    if (CurrentMenu != MenuType.None && CurrentMenu != menuType) {
        _menuHistory.Push(CurrentMenu); // Save current menu
    }
    // Show new menu
}

public void GoBack() {
    if (_menuHistory.Count > 0) {
        var previousMenu = _menuHistory.Pop();
        ShowMenu(previousMenu); // Return to previous menu
    }
}
```

## Error Handling & Fallbacks

### Graceful Degradation
```csharp
// MenuManager with NavigationManager fallback
var menuManager = GameRoot.Instance?.GetMenuManager();
if (menuManager != null) {
    menuManager.ShowMenu(MenuType.SettingsMenu);
} else {
    // Fallback to NavigationManager
    NavigationManager.Instance.NavigateToSettingsMenu();
}
```

### Null Safety
Both systems implement comprehensive null checking:
```csharp
// Safe navigation example
if (GameRoot.Instance != null) {
    var menuManager = GameRoot.Instance.GetMenuManager();
    if (menuManager != null) {
        menuManager.ShowMenu(MenuType.MainMenu);
    }
}
```

## Integration Points

### GameRoot Integration
```csharp
// GameRoot exposes MenuManager access
public MenuManager GetMenuManager() {
    return _menuManager;
}

// Coordinates with NavigationManager
public void ReturnToMainMenu() {
    _menuManager?.ShowMenu(MenuManager.MenuType.MainMenu);
    MenuMusicManager.Instance?.ResumeMenuContext();
    ChangeGameState(GameState.MainMenu);
}
```

### Component Integration
- **Player.cs**: Registers with NavigationManager for pause functionality
- **PauseMenu.cs**: Uses NavigationManager for settings overlay
- **All Menu Scripts**: Prefer MenuManager, fallback to NavigationManager

## Best Practices

### When to Use MenuManager
- ✅ Navigation between standard menu screens
- ✅ Back button functionality within menus  
- ✅ Menu state management and history
- ✅ Fast menu switching with caching

### When to Use NavigationManager
- ✅ Major scene transitions (Menu ↔ Game)
- ✅ Context-preserving overlays (Settings from pause)
- ✅ Music context management
- ✅ Scene-level caching and preloading

### Implementation Guidelines
1. **Primary Navigation**: Always try MenuManager first in menu contexts
2. **Fallback Strategy**: Implement NavigationManager fallbacks for robustness
3. **Context Awareness**: Check parent nodes to determine overlay vs. full menu mode
4. **State Management**: Use GameRoot.ChangeGameState() for major transitions
5. **Music Coordination**: Let NavigationManager handle music context changes

## Debugging and Monitoring

Both systems provide extensive logging for debugging:

```csharp
// MenuManager logging
GD.Print($"MenuManager: Menu changed from {oldMenu} to {newMenu}");
GD.Print($"MenuManager: Navigation history: [{string.Join(", ", _menuHistory)}]");

// NavigationManager logging  
GD.Print("NavigationManager: MainMap loaded directly into Viewport");
GD.Print("NavigationManager: Settings overlay created successfully");
```

This dual navigation architecture provides a robust, efficient, and user-friendly navigation experience while maintaining clean separation of concerns between menu-level and scene-level navigation.
