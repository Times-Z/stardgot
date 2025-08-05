# Reusable Components Guide

This document explains how to use the new components created for the Stardgot project, inspired by the pattern established by `InteractionPromptComponent.cs`.

## Table of Contents

- [1. InteractionPromptComponent](#1-interactionprompt-component) - UI component for interaction prompts
- [2. MusicPlayerComponent](#2-musicplayercomponent) - Reusable background music management
- [3. MenuMusicManager (Singleton)](#3-menumusicmanager-singleton) - Persistent menu music across scenes
- [4. DepthSortableComponent](#4-depthsortablecomponent) - Automatic depth sorting for Node2D
- [5. BackgroundComponent](#5-backgroundcomponent) - Reusable UI backgrounds with effects
- [6. AnimationControllerComponent](#6-animationcontrollercomponent) - Standardized animation management
- [7. Door Component](#7-door-component) - Interactive door system
- [8. PixelPerfectViewportConfigurator](#8-pixelperfectviewportconfigurator) - Automatic pixel-perfect viewport setup
- [9. MenuManager](#9-menumanager) - Centralized menu navigation system
- [10. FPSDisplayComponent](#10-fpsdisplaycomponent) - Configurable FPS display

---

## 1. InteractionPromptComponent Component

**Path**: `res://scripts/components/ui/InteractionPromptComponent.cs`  
**Scene**: `res://scenes/components/ui/InteractionPromptComponent.tscn`

### Description
UI component for displaying interaction prompts above the player or at a fixed screen position. This is the foundational component that inspired the component architecture.

### Configurable Properties
- `PromptText`: Default text to display
- `BackgroundColor`: Background color of the prompt (default: black with 50% opacity)
- `TextColor`: Text color (default: white)
- `CornerRadius`: Corner radius for rounded background
- `LabelOffset`: Offset position relative to parent (default: -75, -80)
- `LabelSize`: Size of the label (default: 150x40)
- `FontSize`: Font size for the text (default: 18)

### Usage
```csharp
// Get the component
var interactionPrompt = GetNode<InteractionPromptComponent>("InteractionPromptComponent");

// Show and hide prompts
interactionPrompt.ShowPrompt("Press E to interact");
interactionPrompt.HidePrompt();

// Update styling
interactionPrompt.UpdateStyling(
    backgroundColor: new Color(0.2f, 0.2f, 0.8f, 0.7f),
    textColor: Colors.Yellow,
    cornerRadius: 5
);

// Position management
interactionPrompt.SetLabelPosition(new Vector2(100, 50));
interactionPrompt.SetLabelSize(new Vector2(200, 50));
```

## 2. MusicPlayerComponent

**Path**: `res://scripts/components/audio/MusicPlayerComponent.cs`  
**Scene**: `res://scenes/components/audio/MusicPlayerComponent.tscn`

### Description
Reusable component for background music management in different contexts (menu, game, etc.).

### Configurable Properties
- `MusicStream`: Audio file to play
- `AutoPlay`: Automatic start (true for menus, false for game)
- `VolumeDb`: Volume in decibels (default: -10)
- `Loop`: Loop playback (default: true)
- `PlayerName`: Name for debugging

### Usage
```csharp
### Usage Example
```csharp
// Get the component
var musicPlayer = GetNode<MusicPlayerComponent>("MusicPlayer");

// Control music
musicPlayer.PlayMusic();
musicPlayer.PauseMusic();
musicPlayer.StopMusic();
musicPlayer.SetVolume(-5.0f);

// Change music
var newStream = GD.Load<AudioStream>("res://assets/audio/background/02.mp3");
musicPlayer.SetMusicStream(newStream, true);
```

## 3. MenuMusicManager (Singleton)

**Path**: `res://scripts/core/MenuMusicManager.cs`  
**Autoload**: `MenuMusicManager="*res://scripts/core/MenuMusicManager.cs"`

### Description
Singleton music manager for persistent menu music across different menu scenes. Ensures menu music continues playing when navigating between MainMenu, SettingsMenu, etc.

### Features
- **Persistent Music**: Continues playing across menu scene changes
- **Context Awareness**: Distinguishes between menu context and game context
- **Automatic Management**: Integrates with NavigationManager for seamless transitions
- **Component-Based**: Uses MusicPlayerComponent internally
- **Smart Playback**: Won't restart music if already playing or in wrong context

### Usage
```csharp
// Direct access (usually handled automatically by NavigationManager)
MenuMusicManager.Instance.PlayMenuMusic();
MenuMusicManager.Instance.StopMenuMusic();
MenuMusicManager.Instance.PauseMenuMusic();
MenuMusicManager.Instance.ResumeMenuMusic();

// Context management
MenuMusicManager.Instance.ResumeMenuContext(); // Sets menu context + plays music
bool inGame = MenuMusicManager.Instance.IsInGameContext();

// Volume control
MenuMusicManager.Instance.SetVolume(-5.0f);

// State checking
if (MenuMusicManager.Instance.IsPlaying()) {
    GD.Print("Menu music is playing");
}
```

### Integration Points
- **GameRoot.cs**: Sets menu context on game launch
- **NavigationManager.cs**: Manages context transitions between menu/game
- **ShowSettingsMenuOverlay()**: Respects game context + pauses game music
- **CloseSettingsOverlay()**: Resumes game music when returning to game
- **Automatic**: No manual calls needed in menu scene scripts

## 4. DepthSortableComponent

### Description
Component to make any Node2D automatically sortable by depth.

### Configurable Properties
- `AutoRegister`: Automatic registration with DepthSorter
- `SortingOffset`: Position offset for sorting
- `UseCustomSortPosition`: Use custom position for sorting
- `CustomSortPosition`: Custom position for sorting
- `SortingPriority`: Sorting priority (higher values = in front)
- `ComponentName`: Name for debugging

### Usage
```csharp
// Add to an existing Node2D
var player = GetNode<Node2D>("Player");
var depthComponent = preload("res://scenes/components/core/DepthSortableComponent.tscn").Instantiate();
player.AddChild(depthComponent);

// Manual control
var depthSortable = GetNode<DepthSortableComponent>("DepthSortableComponent");
depthSortable.SetCustomSortPosition(new Vector2(100, 200));
depthSortable.ForceDepthUpdate();
```

### Replacing existing code
Instead of:
```csharp
// In _Ready()
_depthSorter = GetNode<DepthSorter>("../DepthSorter");
_depthSorter.RegisterObject(this);
```

Use:
```csharp
// Add the component as child, that's it!
// It registers automatically
```

## 5. BackgroundComponent

**Path**: `res://scripts/components/ui/BackgroundComponent.cs`  
**Scene**: `res://scenes/components/ui/BackgroundComponent.tscn`

### Description  
Reusable component for UI backgrounds with support for textures, colors, and blur effects.

### Configurable Properties
- `BackgroundTexture`: Background texture
- `BackgroundColor`: Background color or tint
- `UseSolidColor`: Use solid color instead of texture
- `ExpandMode`: Texture expansion mode
- `StretchMode`: Texture stretch mode
- `ApplyBlur`: Apply blur effect
- `BlurMaterial`: Blur material (ShaderMaterial)
- `Opacity`: Opacity (0.0 to 1.0)
- `FillContainer`: Fill entire parent container

### Usage
```csharp
var background = GetNode<BackgroundComponent>("Background");

// Change texture
var newTexture = GD.Load<Texture2D>("res://assets/background/new_bg.png");
background.SetBackgroundTexture(newTexture);

// Effects
background.SetOpacity(0.8f);
background.SetBlurEnabled(true);

// Animations
background.FadeIn(1.0f);
background.AnimateOpacity(0.5f, 2.0f);
```

### Replacement in menus
Instead of manually creating:
```gdscript
[node name="Background" type="TextureRect" parent="."]
# ... manual configuration
```

Use:
```gdscript
[node name="Background" parent="." instance=ExtResource("BackgroundComponent.tscn")]
BackgroundTexture = ExtResource("background.png")
```

### Current Usage
Now used in:
- **MainMenu**: Texture background with placeholder image
- **SettingsMenu**: Texture background with placeholder image  
- **PauseMenu**: Uses traditional TextureRect + ColorRect for blur effect (BackgroundComponent not suitable for this use case)

## 6. AnimationControllerComponent

**Path**: `res://scripts/components/animation/AnimationControllerComponent.cs`  
**Scene**: `res://scenes/components/animation/AnimationControllerComponent.tscn`

### Description
Standardized component for animation management (AnimatedSprite2D or AnimationPlayer).

### Configurable Properties
- `ControllerType`: Animation type (AnimatedSprite2D, AnimationPlayer, Custom)
- `AnimatedSpritePath`: Path to AnimatedSprite2D
- `AnimationPlayerPath`: Path to AnimationPlayer  
- `DefaultAnimation`: Default animation
- `AutoPlayDefault`: Play default animation automatically
- `AnimationAliases`: Animation aliases (e.g., "walk" -> "walk_down")
- `ComponentName`: Name for debugging

### Usage
```csharp
var animController = GetNode<AnimationControllerComponent>("AnimationController");

// Control animations
animController.PlayAnimation("walk");
animController.PlayAnimation("idle", force: true);
animController.StopAnimation();
animController.PauseAnimation();

// Aliases
animController.SetAnimationAlias("move", "walk_down");
animController.PlayAnimation("move"); // Plays "walk_down"

// Debug
GD.Print(animController.GetDebugInfo());
```

### Replacement in Player.cs
Instead of:
```csharp
private AnimatedSprite2D _animatedSprite;

// In _Ready()
_animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

// To play an animation
_animatedSprite.Play("walk_" + _lastDirection);
```

Use:
```csharp
private AnimationControllerComponent _animationController;

// In _Ready()
_animationController = GetNode<AnimationControllerComponent>("AnimationController");

// To play an animation
_animationController.PlayAnimation("walk_" + _lastDirection);
```

## 7. Door Component

**Path**: `res://scripts/buildings/Door.cs`  
**Scene**: `res://scenes/components/building/WoodDoor.tscn`

### Description
Interactive door component that can be opened and closed by the player. Extends the `Interactable` base class.

### Configurable Properties
- `IsOpen`: Whether the door is currently open or closed
- `DoorCollision`: Collision shape that blocks the player when closed
- `DoorSprite`: Optional animated sprite for visual representation
- `OpenSound`: Sound effect played when door opens
- `CloseSound`: Sound effect played when door closes

### Usage
```csharp
var door = GetNode<Door>("Door");

// Control door state
door.OpenDoor();
door.CloseDoor();
door.ToggleDoor();

// Check state
if (door.IsOpen) {
    GD.Print("Door is open");
}

// Override interaction behavior
public override void OnInteract(Node interactor) {
    ToggleDoor();
    base.OnInteract(interactor);
}
```

---

## 8. PixelPerfectViewportConfigurator

**Path**: `res://scripts/components/core/PixelPerfectViewportConfigurator.cs`  
**Scene**: Attach directly to SubViewport nodes  
**Global Class**: Available in Godot editor

### Description
Component that automatically configures a SubViewport for pixel-perfect rendering. This component ensures crisp pixel art display by setting appropriate viewport properties for 2D pixel art games.

### Configurable Properties
- `ConfigureOnReady`: Whether to auto-configure on _Ready() (default: true)
- `SnapTransformsToPixel`: Enable transform snapping to pixels (default: true)
- `SnapVerticesToPixel`: Enable vertex snapping to pixels (default: true)
- `DisablePhysicsInterpolation`: Disable physics interpolation for pixel-perfect movement (default: true)
- `TextureFilter`: Texture filtering mode (default: Nearest for pixel art)
- `RenderUpdateMode`: Viewport update mode (default: Always)

### Usage
```csharp
// Attach to any SubViewport as a child node
// Configuration is automatic on _Ready() if ConfigureOnReady = true

// Manual configuration
var configurator = GetNode<PixelPerfectViewportConfigurator>("PixelPerfectViewportConfigurator");
configurator.ConfigureViewport();

// Custom configuration
configurator.SnapTransformsToPixel = true;
configurator.TextureFilter = Viewport.DefaultCanvasItemTextureFilter.Nearest;
configurator.ConfigureViewport();
```

### Integration with GameRoot
```csharp
// GameRoot.tscn structure:
// GameRoot/ViewportContainer/Viewport/PixelPerfectViewportConfigurator
// The configurator automatically sets up the viewport for pixel-perfect rendering
```

---

## 9. MenuManager

**Path**: `res://scripts/ui/MenuManager.cs`  
**Integration**: Child node of GameRoot  
**Singleton**: Instance-based (GameRoot.Instance.GetMenuManager())

### Description
Centralized menu navigation system that manages all menu scenes within the GameRoot architecture. Provides efficient menu caching, navigation history, and seamless transitions between different menu screens.

### Supported Menus
```csharp
public enum MenuType {
    None,
    MainMenu,        // Main game menu
    PauseMenu,       // In-game pause menu  
    SettingsMenu,    // Settings configuration
    ControlsMenu     // Input controls configuration
}
```

### Key Features
- **Scene Caching**: All menu scenes preloaded for instant switching
- **Navigation History**: Stack-based back navigation with proper context
- **Memory Management**: Efficient menu instance reuse and cleanup
- **Integration**: Works seamlessly with GameRoot's UiLayer

### Usage
```csharp
// Get MenuManager instance
var menuManager = GameRoot.Instance?.GetMenuManager();

// Show menus
menuManager.ShowMenu(MenuManager.MenuType.MainMenu);
menuManager.ShowMenu(MenuManager.MenuType.SettingsMenu);

// Navigation
menuManager.GoBack(); // Returns to previous menu in history

// Hide specific menu
menuManager.HideMenu(MenuManager.MenuType.SettingsMenu);
menuManager.HideAllMenus();

// Check menu state
if (menuManager.IsMenuVisible(MenuManager.MenuType.MainMenu)) {
    GD.Print("Main menu is visible");
}

// Get menu instance
var mainMenu = menuManager.GetMenuInstance(MenuManager.MenuType.MainMenu);
```

### Signals
```csharp
// Emitted when menu changes
[Signal]
public delegate void MenuChangedEventHandler(MenuType oldMenu, MenuType newMenu);

// Connect to signal
menuManager.MenuChanged += OnMenuChanged;
```

### Menu Scene Requirements
Menu scenes should implement standard patterns:
```csharp
// In menu scripts, prefer MenuManager over NavigationManager
var menuManager = GameRoot.Instance?.GetMenuManager();
if (menuManager != null) {
    menuManager.ShowMenu(MenuManager.MenuType.SettingsMenu);
} else {
    // Fallback to NavigationManager
    NavigationManager.Instance.NavigateToSettingsMenu();
}
```

---

## 10. FPSDisplayComponent

**Path**: `res://scripts/components/ui/FPSDisplayComponent.cs`  
**Scene**: `res://scenes/components/ui/FPSDisplayComponent.tscn`  
**Integration**: Managed by ConfigurationManager

### Description
Configurable FPS display component that shows current framerate. Automatically integrates with the global settings system and can be toggled via the settings menu.

### Configurable Properties
- **Display Options**: Position, font size, colors
- **Update Frequency**: How often FPS is recalculated
- **Visibility**: Controlled by global ShowFPS setting

### Usage
```csharp
// FPS display is automatically managed by ConfigurationManager
// Toggle via settings or programmatically:

ConfigurationManager.ShowFPS = true;  // Show FPS counter
ConfigurationManager.ShowFPS = false; // Hide FPS counter

// The component automatically responds to configuration changes
```

### Integration Points
- **ConfigurationManager**: Automatically shows/hides based on global setting
- **SettingsMenu**: Provides toggle button for user control
- **GameRoot**: Positioned in UI layer for consistent display

### Settings Integration
```csharp
// In SettingsMenu.cs - automatic integration
var showFPSButton = GetNodeOrNull<CheckButton>("ShowFPSButton");
if (showFPSButton != null) {
    showFPSButton.ButtonPressed = ConfigurationManager.ShowFPS;
    showFPSButton.Toggled += OnShowFPSToggled;
}

private void OnShowFPSToggled(bool buttonPressed) {
    ConfigurationManager.ShowFPS = buttonPressed;
}
```

## Component Best Practices

### 1. Consistent API Design
- Use `[Export]` properties for editor configuration
- Provide both automatic and manual initialization options
- Include comprehensive null safety checks

### 2. Global Class Pattern
```csharp
[GlobalClass]
public partial class YourComponent : Node
{
    // Makes component available in Godot editor
}
```

### 3. Integration with Core Systems
- Components should integrate with ConfigurationManager for settings
- Use GameRoot.Instance for accessing core systems
- Implement proper singleton patterns where appropriate

### 4. Resource Management
- Implement proper cleanup in _ExitTree()
- Use scene caching for frequently accessed resources
- Provide Clear/Reset methods for memory management

### 5. Error Handling
- Always check for null references before use
- Provide meaningful error messages with GD.PrintErr()
- Implement graceful fallbacks for missing dependencies
