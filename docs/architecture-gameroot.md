# GameRoot / Viewport Architecture

## Overview

The GameRoot architecture uses a hierarchical structure with a viewport and layers to separate the user interface from game content. The architecture has been enhanced with state management, performance optimizations, robust error handling, and a centralized MenuManager system for seamless navigation.

## Current Node Structure

```
GameRoot (Control)
├── ViewportContainer (SubViewportContainer)
│   └── Viewport (SubViewport)
│       └── UiLayer (CanvasLayer) [Layer: 10]
└── MenuManager
```

**Key Change**: The GameLayer has been simplified - game content is now added directly to the Viewport, while UI elements use the UiLayer. The MenuManager handles all menu navigation within the GameRoot system.

## Enhanced Features

### State Management
- **GameState enum**: Tracks application state (MainMenu, Loading, InGame, Paused, Settings)
- **State transitions**: Managed through `ChangeGameState()` with signals
- **State-based logic**: Different behaviors based on current state

### MenuManager Integration
- **Centralized menu system**: All menus managed through a dedicated MenuManager singleton
- **Menu caching**: Preloaded scenes with intelligent memory management
- **Navigation history**: Stack-based navigation with proper back button support
- **Context-aware transitions**: Proper menu overlays for pause/settings scenarios

### Performance Optimizations
- **Scene caching**: Preloaded scenes stored in memory for faster access
- **Resource management**: Proper cleanup of menu instances
- **Pixel snapping**: Enabled for crisp 2D graphics with PixelPerfectViewportConfigurator
- **Layer optimization**: Streamlined layer structure for better performance

### Error Handling & Validation
- **Node reference validation**: Checks all required references on startup  
- **Null safety**: All getter methods validate references before returning
- **Safe operations**: `AddToUiLayer()` and `AddToViewport()` with error checking
- **Graceful fallbacks**: NavigationManager provides fallback scene navigation

### Music & Audio Management
- **MenuMusicManager**: Persistent background music across menu scenes
- **Context-aware audio**: Distinguishes between menu and game contexts
- **Automatic transitions**: Seamless audio management during scene changes

## Node Structure

```
GameRoot (Control)
├── ViewportContainer (SubViewportContainer)
│   └── Viewport (SubViewport)
│       ├── PixelPerfectViewportConfigurator
│       └── UiLayer (CanvasLayer) [Layer: 10]
└── MenuManager
```

## Visual Diagram

```
┌─────────────────────────────────────────────────────────────┐
│ GameRoot (Control)                                          │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ ViewportContainer (SubViewportContainer)                │ │
│ │ ┌─────────────────────────────────────────────────────┐ │ │
│ │ │ Viewport (SubViewport)                              │ │ │
│ │ │ ┌─────────────────────────────────────────────────┐ │ │ │
│ │ │ │ PixelPerfectViewportConfigurator                │ │ │ │
│ │ │ └─────────────────────────────────────────────────┘ │ │ │
│ │ │ ┌─────────────────────────────────────────────────┐ │ │ │
│ │ │ │ UiLayer (CanvasLayer) - Layer: 10               │ │ │ │
│ │ │ │ ┌─────────────────────────────────────────────┐ │ │ │ │
│ │ │ │ │ MainMenu / UI Elements                      │ │ │ │ │
│ │ │ │ └─────────────────────────────────────────────┘ │ │ │ │
│ │ │ └─────────────────────────────────────────────────┘ │ │ │
│ │ │ ┌─────────────────────────────────────────────────┐ │ │ │
│ │ │ │ Game Content (Direct Viewport Children)         │ │ │ │
│ │ │ │ ┌─────────────────────────────────────────────┐ │ │ │ │
│ │ │ │ │ MainMap / Player / Game Scenes              │ │ │ │ │
│ │ │ │ └─────────────────────────────────────────────┘ │ │ │ │
│ │ │ └─────────────────────────────────────────────────┘ │ │ │
│ │ └─────────────────────────────────────────────────────┘ │ │
│ └─────────────────────────────────────────────────────────┘ │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ MenuManager                                             │ │
│ │ ┌─────────────────────────────────────────────────────┐ │ │
│ │ │ Menu Scene Cache & Instance Management              │ │ │
│ │ └─────────────────────────────────────────────────────┘ │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## Component Responsibilities

### GameRoot (Control)
- **Role**: Main entry point of the game
- **Responsibilities**:
  - Application lifecycle management
  - Layer initialization
  - Menu coordination with MenuManager
  - Common scene preloading and caching
  - Game state management with signals

### ViewportContainer (SubViewportContainer)
- **Role**: Container to make the viewport visible
- **Configuration**:
  - Anchors: Full Rect (0, 0, 1, 1)
  - Stretch: true
  - Fills the entire window

### Viewport (SubViewport)
- **Role**: Separate rendering context for pixel-perfect rendering
- **Configuration**:
  - Snap2DTransformsToPixel: true
  - Snap2DVerticesToPixel: true
  - CanvasItemDefaultTextureFilter: Nearest
- **Advantages**:
  - Rendering isolation
  - Independent layer management
  - Pixel-perfect 2D graphics
  - Direct game content hosting

### PixelPerfectViewportConfigurator
- **Role**: Automatic pixel-perfect configuration component
- **Responsibilities**:
  - Configures viewport for crisp pixel art rendering
  - Manages texture filtering and transform snapping
  - Handles physics interpolation settings

### UiLayer (CanvasLayer)
- **Role**: Layer for user interface elements
- **Configuration**:
  - Layer: 10 (high priority, always on top)
- **Content**:
  - Main menus (managed by MenuManager)
  - Game HUD
  - Interface overlays
  - Settings/pause menus

### MenuManager (Node)
- **Role**: Centralized menu system controller
- **Responsibilities**:
  - Menu scene caching and instantiation
  - Navigation history management
  - Menu visibility and transitions
  - Integration with GameRoot's UiLayer

## Editor Configuration

### Configuration steps in Godot:

1. **GameRoot**:
   - Attach the `GameRoot.cs` script
   - Configure Export references in the inspector:
     - `_viewportContainer`: NodePath to ViewportContainer
     - `_viewport`: NodePath to ViewportContainer/Viewport  
     - `_uiLayer`: NodePath to ViewportContainer/Viewport/UiLayer
     - `_menuManager`: NodePath to MenuManager

2. **ViewportContainer**:
   - Layout → Full Rect
   - Stretch = true

3. **Viewport**:
   - Add PixelPerfectViewportConfigurator as child
   - Snap2DTransformsToPixel = true (set by configurator)
   - CanvasItemDefaultTextureFilter = Nearest (set by configurator)

4. **UiLayer**:
   - CanvasLayer with Layer = 10

5. **MenuManager**:
   - Attach `MenuManager.cs` script
   - Will automatically find UiLayer reference on _Ready()

## Navigation Architecture

The project now uses a dual navigation system:

### MenuManager (Primary)
- **Scope**: Menu navigation within GameRoot
- **Usage**: MainMenu ↔ SettingsMenu ↔ ControlsMenu
- **Features**: Navigation history, menu caching, proper cleanup

### NavigationManager (Secondary)  
- **Scope**: Scene-level navigation and overlays
- **Usage**: MainMenu → Game, Pause menu overlays, settings overlays from pause
- **Features**: Scene caching, context-aware transitions, music management

## Architecture Advantages

- **Separation of concerns**: UI and Game content are isolated
- **Easy maintenance**: Visual configuration in the editor with export properties
- **Flexibility**: Easy to add new menus through MenuManager
- **Performance**: Optimized rendering with pixel-perfect viewport configuration
- **Debugging**: Clear and hierarchical structure with proper singleton pattern
- **State Management**: Robust game state tracking with signals
- **Navigation**: Dual navigation system for different use cases
- **Audio Integration**: Seamless music management across contexts
- **Memory Efficiency**: Scene caching with automatic cleanup

## Key API Methods

### GameRoot Public Interface

```csharp
// State management
void ChangeGameState(GameState newState)
GameState CurrentState { get; }

// Access to core components
CanvasLayer GetUiLayer()
SubViewport GetViewport()
MenuManager GetMenuManager()

// Game content management
void AddToUiLayer(Node child, bool forceReadableUniqueName = false)
void AddToViewport(Node child, bool forceReadableUniqueName = false)

// Scene management
void PreloadScene(string scenePath, string cacheKey = null)
PackedScene GetOrLoadScene(string scenePath, string cacheKey = null)
void ClearSceneCache()

// Navigation
void ReturnToMainMenu()
void SetCurrentCamera(Camera2D camera)
```

### MenuManager Public Interface

```csharp
// Menu management
void ShowMenu(MenuType menuType, bool hideOthers = true)
void HideMenu(MenuType menuType)
void HideAllMenus()
void GoBack()

// State queries
bool IsMenuVisible(MenuType menuType)
Control GetMenuInstance(MenuType menuType)
MenuType CurrentMenu { get; }
```

## Simplified C# Code

The GameRoot.cs code uses `[Export]` annotations to expose references in the editor:

```csharp
[Export] private SubViewportContainer _viewportContainer;
[Export] private SubViewport _viewport;
[Export] private CanvasLayer _uiLayer;
[Export] private MenuManager _menuManager;
```

This approach avoids manual node searching and makes the configuration more robust. The MenuManager operates as a child of GameRoot and automatically manages menu scenes within the UiLayer.