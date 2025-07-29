# GameRoot / Viewport Architecture

## Overview

The GameRoot architecture uses a hierarchical structure with a viewport and layers to separate the user interface from game content.

## Node Structure

```
GameRoot (Control)
└── ViewportContainer (SubViewportContainer)
    └── Viewport (SubViewport)
        ├── UiLayer (CanvasLayer) [Layer: 10]
        └── GameLayer (CanvasLayer) [Layer: 0]
```

## Visual Diagram

```
┌─────────────────────────────────────────────────────────────┐
│ GameRoot (Control)                                          │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ ViewportContainer (SubViewportContainer)                │ │
│ │ ┌─────────────────────────────────────────────────────┐ │ │
│ │ │ Viewport (SubViewport)                              │ │ │
│ │ │                                                     │ │ │
│ │ │ ┌─────────────────────────────────────────────────┐ │ │ │
│ │ │ │ UiLayer (CanvasLayer) - Layer: 10               │ │ │ │
│ │ │ │ ┌─────────────────────────────────────────────┐ │ │ │ │
│ │ │ │ │ MainMenu / UI Elements                      │ │ │ │ │
│ │ │ │ └─────────────────────────────────────────────┘ │ │ │ │
│ │ │ └─────────────────────────────────────────────────┘ │ │ │
│ │ │                                                     │ │ │
│ │ │ ┌─────────────────────────────────────────────────┐ │ │ │
│ │ │ │ GameLayer (CanvasLayer) - Layer: 0              │ │ │ │
│ │ │ │ ┌─────────────────────────────────────────────┐ │ │ │ │
│ │ │ │ │ Game Content / 2D Scenes                    │ │ │ │ │
│ │ │ │ └─────────────────────────────────────────────┘ │ │ │ │
│ │ │ └─────────────────────────────────────────────────┘ │ │ │
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
  - Menu display/hiding
  - Common scene preloading

### ViewportContainer (SubViewportContainer)
- **Role**: Container to make the viewport visible
- **Configuration**:
  - Anchors: Full Rect (0, 0, 1, 1)
  - Stretch: true
  - Fills the entire window

### Viewport (SubViewport)
- **Role**: Separate rendering context
- **Advantages**:
  - Rendering isolation
  - Independent layer management
  - Separate post-processing capabilities

### UiLayer (CanvasLayer)
- **Role**: Layer for user interface
- **Configuration**:
  - Layer: 10 (high priority)
- **Content**:
  - Main menus
  - Game HUD
  - Interface overlays

### GameLayer (CanvasLayer)
- **Role**: Layer for game content
- **Configuration**:
  - Layer: 0 (low priority)
- **Content**:
  - 2D game scenes
  - Gameplay content
  - Backgrounds

## Editor Configuration

### Configuration steps in Godot:

1. **GameRoot**:
   - Attach the `GameRoot.cs` script
   - Configure Export references in the inspector

2. **ViewportContainer**:
   - Layout → Full Rect
   - Stretch = true

3. **Layers**:
   - UiLayer: Layer = 10
   - GameLayer: Layer = 0

## Architecture Advantages

- **Separation of concerns**: UI and Game content are isolated
- **Easy maintenance**: Visual configuration in the editor
- **Flexibility**: Easy to add new layers
- **Performance**: Optimized rendering with CanvasLayer
- **Debugging**: Clear and hierarchical structure

## Simplified C# Code

The GameRoot.cs code uses `[Export]` annotations to expose references in the editor:

```csharp
[Export] private SubViewportContainer _viewportContainer;
[Export] private SubViewport _viewport;
[Export] private CanvasLayer _uiLayer;
[Export] private CanvasLayer _gameLayer;
```

This approach avoids manual node searching and makes the configuration more robust.