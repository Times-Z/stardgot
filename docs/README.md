# Documentation Index

## Core Architecture Documents

### [GameRoot Architecture Guide](architecture-gameroot.md)
**Essential reading for understanding the foundation of the project**

- **GameRoot system** with viewport-based rendering
- **MenuManager integration** for centralized navigation
- **Pixel-perfect rendering** setup with automatic configuration
- **State management** with signals and proper lifecycle handling
- **Scene caching** and performance optimizations

**Key Topics**: GameRoot structure, viewport configuration, MenuManager, navigation architecture, pixel-perfect rendering

---

### [Navigation System Guide](navigation-system-guide.md)
**Complete overview of the dual navigation architecture**

- **MenuManager** for menu-to-menu navigation within GameRoot
- **NavigationManager** for scene-level transitions and overlays
- **Context-aware behavior** for settings overlays vs. full navigation
- **Music integration** with seamless audio context management
- **Scene caching** and memory management strategies

**Key Topics**: Dual navigation patterns, context preservation, audio management, caching strategies

---

## System-Specific Guides

### [Components Guide](components-guide.md)
**Complete reference for all reusable components**

Covers all major components including:
- **UI Components**: InteractionPrompt, FPSDisplay, BackgroundComponent
- **Audio Components**: MusicPlayer, MenuMusicManager
- **Game Components**: DepthSortable, AnimationController, Door system
- **Core Components**: PixelPerfectViewportConfigurator, MenuManager
- **Best practices** for component development and integration

**Key Topics**: Component patterns, reusable systems, integration guidelines

---

### [Controls System Guide](controls-system-guide.md)
**Input remapping and controls management**

- **Complete key remapping** system with conflict detection
- **Settings persistence** with automatic save/load
- **User interface** for control customization
- **Protection mechanisms** against remapping system keys
- **Integration** with ConfigurationManager

**Key Topics**: Input management, key conflict resolution, settings UI

---

### [Settings System Guide](settings-system-guide.md)
**Unified configuration management**

- **ConfigurationManager** for centralized settings
- **Automatic validation** and configuration repair
- **Multi-category settings**: Display, Audio, Gameplay, Controls
- **Robust error handling** with graceful fallbacks
- **Settings UI integration** across all menus

**Key Topics**: Configuration architecture, settings categories, validation, persistence
