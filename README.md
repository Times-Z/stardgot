# 🌟 Stardgot: The Open Source Game

**Stardgot** is a charming open-source game built with [Godot Engine 4.4](https://godotengine.org/) using **C#**. The game features pixel-art visuals from the **Sprout Lands Pack** created by [Cup Nooble](https://cupnooble.itch.io/).

> 🌱 Grow, explore, and build — Stardgot is designed to be modifiable, accessible, and fully open source.

<div align="center">
  <a href="https://github.com/Times-Z/stardgot"><img src="https://img.shields.io/github/v/release/Times-Z/stardgot?label=Latest%20Version&color=c56a90&style=for-the-badge&logo=star)" alt="Latest Version" /></a>
  <a href="https://github.com/Times-Z/stardgot"><img src="https://img.shields.io/github/actions/workflow/status/Times-Z/stardgot/.github/workflows/build-and-release.yml?branch=main&label=Pipeline%20Status&color=c56a90&style=for-the-badge&logo=star" alt="Latest Version" /></a>
</div>



---

## Features

- Developed in **C#** using **Godot 4.4 (Mono)**
- Pixel-art assets from the **Sprout Lands Pack**
- Modular architecture for easy content addition
- Fully open source and mod-friendly

---

## 🛠 Requirements

- [Godot 4.4 (Mono version)](https://godotengine.org/download)
- .NET SDK 8.0+

---

## Project Structure

- `assets/` : Art, music, sound, fonts, and pixel-art resources
  - `audio/` : Background music and sound effects
  - `fonts/` : Pixel fonts and previews
  - `gfx/` : Backgrounds, characters, tilesets, UI images
  - `imgs/` : Additional images and imports
- `build/` : Build output directory
- `scenes/` : Main game scenes (main map, menus, player, overlays)
  - `main/` : Core game scenes
  - `menus/` : Menu scenes (main menu, settings, pause)
  - `player/` : Player scene
- `scripts/` : C# scripts for game logic
  - `core/` : Core systems (e.g., navigation)
  - `main/` : Main game logic
  - `menus/` : Menu logic
  - `player/` : Player logic
- `shaders/` : Shader files and materials
- `project.godot` : Godot project configuration
- `Stardgot.csproj`, `Stardgot.sln` : C# project and solution files
- `README.md`, `LICENSE`, `icon.svg` : Documentation and project metadata

---

## How to Run

1. Open the project folder in **Godot 4.4 (Mono)**.
2. Make sure you have .NET SDK 8.0 installed.
3. Click **Play** in the Godot editor to start the game.

---

## 📦 Releases

You can download the latest builds and releases from our [GitHub Releases page](https://github.com/Times-Z/stardgot/releases).

---

## Documentation

- [GameRoot / Viewport architecture](docs/architecture-gameroot.md)
