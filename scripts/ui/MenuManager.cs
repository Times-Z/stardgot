using Godot;

using System.Collections.Generic;

public partial class MenuManager : Node {
    /// <summary>
    /// Types de menus disponibles
    /// </summary>
    public enum MenuType {
        None,
        MainMenu,
        PauseMenu,
        SettingsMenu,
        ControlsMenu
    }

    /// <summary>
    /// Chemins vers les scènes des menus
    /// </summary>
    private static readonly Dictionary<MenuType, string> MenuScenePaths = new() {
        { MenuType.MainMenu, "res://scenes/menus/MainMenu.tscn" },
        { MenuType.PauseMenu, "res://scenes/menus/PauseMenu.tscn" },
        { MenuType.SettingsMenu, "res://scenes/menus/SettingsMenu.tscn" },
        { MenuType.ControlsMenu, "res://scenes/menus/ControlsMenu.tscn" }
    };

    /// <summary>
    /// Cache des scènes de menus chargées
    /// </summary>
    private readonly Dictionary<MenuType, PackedScene> _menuSceneCache = new();

    /// <summary>
    /// Cache des instances de menus actives
    /// </summary>
    private readonly Dictionary<MenuType, Control> _activeMenus = new();

    /// <summary>
    /// Pile de navigation pour gérer l'historique des menus
    /// </summary>
    private readonly Stack<MenuType> _menuHistory = new();

    /// <summary>
    /// Menu actuellement affiché
    /// </summary>
    public MenuType CurrentMenu { get; private set; } = MenuType.None;

    /// <summary>
    /// Référence vers la couche UI du GameRoot
    /// </summary>
    private CanvasLayer _uiLayer;

    /// <summary>
    /// Signal émis quand un menu change
    /// </summary>
    [Signal]
    public delegate void MenuChangedEventHandler(MenuType oldMenu, MenuType newMenu);

    /// <summary>
    /// Singleton instance
    /// </summary>
    public static MenuManager Instance { get; private set; }

    /// <summary>
    /// Initialise le gestionnaire de menus
    /// </summary>
    public override void _Ready() {
        Instance = this;
        _uiLayer = GameRoot.Instance?.GetUiLayer();

        if (_uiLayer == null) {
            GD.PrintErr("MenuManager: Impossible d'obtenir la couche UI du GameRoot!");
        }

        PreloadMenuScenes();
    }

    /// <summary>
    /// Précharge toutes les scènes de menus pour améliorer les performances
    /// </summary>
    private void PreloadMenuScenes() {
        foreach (var kvp in MenuScenePaths) {
            var scene = GD.Load<PackedScene>(kvp.Value);
            if (scene != null) {
                _menuSceneCache[kvp.Key] = scene;
                GD.Print($"MenuManager: Scene preloaded - {kvp.Key}");
            }
            else {
                GD.PrintErr($"MenuManager: Failed to preload {kvp.Value}");
            }
        }
    }

    /// <summary>
    /// Affiche un menu spécifique
    /// </summary>
    /// <param name="menuType">Type de menu à afficher</param>
    /// <param name="hideOthers">Si true, cache tous les autres menus</param>
    public void ShowMenu(MenuType menuType, bool hideOthers = true) {
        if (menuType == MenuType.None) {
            HideAllMenus();
            return;
        }

        if (CurrentMenu == menuType) {
            GD.Print($"MenuManager: Menu {menuType} already displayed, ignored");
            return;
        }

        if (_uiLayer == null) {
            GD.PrintErr("MenuManager: UI layer not available!");
            return;
        }

        var currentMenuToSave = CurrentMenu;

        if (hideOthers) {
            HideAllMenus();
        }

        if (currentMenuToSave != MenuType.None && currentMenuToSave != menuType) {
            _menuHistory.Push(currentMenuToSave);
            GD.Print($"MenuManager: Added {currentMenuToSave} to history");
        }

        var oldMenu = currentMenuToSave;
        CurrentMenu = menuType;

        if (_activeMenus.TryGetValue(menuType, out Control existingMenu)) {
            existingMenu.Visible = true;
            existingMenu.MoveToFront();
        }
        else {
            if (_menuSceneCache.TryGetValue(menuType, out PackedScene menuScene)) {
                var menuInstance = menuScene.Instantiate<Control>();
                if (menuInstance != null) {
                    _uiLayer.AddChild(menuInstance);
                    _activeMenus[menuType] = menuInstance;
                    menuInstance.Visible = true;

                    GD.Print($"MenuManager: Menu {menuType} created and displayed");
                }
                else {
                    GD.PrintErr($"MenuManager: Failed to instantiate menu {menuType}");
                    return;
                }
            }
            else {
                GD.PrintErr($"MenuManager: Menu scene {menuType} not found in cache");
                return;
            }
        }

        EmitSignal(SignalName.MenuChanged, (int)oldMenu, (int)menuType);
        GD.Print($"MenuManager: Menu change - {oldMenu} -> {menuType}");
        GD.Print($"MenuManager: Menu history: [{string.Join(", ", _menuHistory)}]");
    }

    /// <summary>
    /// Cache un menu spécifique
    /// </summary>
    /// <param name="menuType">Type de menu à cacher</param>
    public void HideMenu(MenuType menuType) {
        if (_activeMenus.TryGetValue(menuType, out Control menu)) {
            menu.Visible = false;

            if (CurrentMenu == menuType) {
                CurrentMenu = MenuType.None;
            }
        }
    }

    /// <summary>
    /// Cache tous les menus actifs
    /// </summary>
    public void HideAllMenus() {
        foreach (var menu in _activeMenus.Values) {
            if (menu != null && IsInstanceValid(menu)) {
                menu.Visible = false;
            }
        }
        CurrentMenu = MenuType.None;
    }

    /// <summary>
    /// Supprime définitivement un menu de la mémoire
    /// </summary>
    /// <param name="menuType">Type de menu à supprimer</param>
    public void DestroyMenu(MenuType menuType) {
        if (_activeMenus.TryGetValue(menuType, out Control menu)) {
            if (menu != null && IsInstanceValid(menu)) {
                menu.QueueFree();
            }
            _activeMenus.Remove(menuType);
        }
    }

    /// <summary>
    /// Supprime tous les menus de la mémoire
    /// </summary>
    public void DestroyAllMenus() {
        foreach (var kvp in _activeMenus) {
            if (kvp.Value != null && IsInstanceValid(kvp.Value)) {
                kvp.Value.QueueFree();
            }
        }
        _activeMenus.Clear();
        _menuHistory.Clear();
        CurrentMenu = MenuType.None;
    }

    /// <summary>
    /// Retourne au menu précédent dans l'historique
    /// </summary>
    public void GoBack() {
        GD.Print($"MenuManager: GoBack called, history: [{string.Join(", ", _menuHistory)}]");

        if (_menuHistory.Count > 0) {
            var previousMenu = _menuHistory.Pop();
            GD.Print($"MenuManager: Return to menu {previousMenu}");
            ShowMenuWithoutHistory(previousMenu);
        }
        else {
            GD.Print("MenuManager: No history, return to main menu");
            ShowMenuWithoutHistory(MenuType.MainMenu);
        }
    }

    /// <summary>
    /// Affiche un menu sans l'ajouter à l'historique (utilisé pour GoBack)
    /// </summary>
    /// <param name="menuType">Type de menu à afficher</param>
    private void ShowMenuWithoutHistory(MenuType menuType) {
        if (menuType == MenuType.None) {
            HideAllMenus();
            return;
        }

        if (CurrentMenu == menuType) {
            GD.Print($"MenuManager: Menu {menuType} already displayed, ignored");
            return;
        }

        if (_uiLayer == null) {
            GD.PrintErr("MenuManager: UI layer not available!");
            return;
        }

        foreach (var menu in _activeMenus.Values) {
            if (menu != null && IsInstanceValid(menu)) {
                menu.Visible = false;
            }
        }

        var oldMenu = CurrentMenu;
        CurrentMenu = menuType;

        if (_activeMenus.TryGetValue(menuType, out Control existingMenu)) {
            existingMenu.Visible = true;
            existingMenu.MoveToFront();
        }
        else {
            if (_menuSceneCache.TryGetValue(menuType, out PackedScene menuScene)) {
                var menuInstance = menuScene.Instantiate<Control>();
                if (menuInstance != null) {
                    _uiLayer.AddChild(menuInstance);
                    _activeMenus[menuType] = menuInstance;
                    menuInstance.Visible = true;

                    GD.Print($"MenuManager: Menu {menuType} created and displayed (without history)");
                }
                else {
                    GD.PrintErr($"MenuManager: Failed to instantiate menu {menuType}");
                    return;
                }
            }
            else {
                GD.PrintErr($"MenuManager: Menu scene {menuType} not found in cache");
                return;
            }
        }

        EmitSignal(SignalName.MenuChanged, (int)oldMenu, (int)menuType);
        GD.Print($"MenuManager: Menu change (GoBack) - {oldMenu} -> {menuType}");
        GD.Print($"MenuManager: Menu history: [{string.Join(", ", _menuHistory)}]");
    }

    /// <summary>
    /// Vérifie si un menu est actuellement visible
    /// </summary>
    /// <param name="menuType">Type de menu à vérifier</param>
    /// <returns>True si le menu est visible</returns>
    public bool IsMenuVisible(MenuType menuType) {
        return _activeMenus.TryGetValue(menuType, out Control menu) &&
               menu != null &&
               IsInstanceValid(menu) &&
               menu.Visible;
    }

    /// <summary>
    /// Obtient l'instance d'un menu actif
    /// </summary>
    /// <param name="menuType">Type de menu</param>
    /// <returns>L'instance du menu ou null si non trouvé</returns>
    public Control GetMenuInstance(MenuType menuType) {
        _activeMenus.TryGetValue(menuType, out Control menu);
        return menu;
    }

    /// <summary>
    /// Affiche un menu en overlay (par-dessus le jeu en cours)
    /// </summary>
    /// <param name="menuType">Type de menu à afficher en overlay</param>
    /// <param name="layerName">Nom de la couche overlay</param>
    /// <param name="layer">Niveau de la couche (défaut: 200)</param>
    public void ShowMenuAsOverlay(MenuType menuType, string layerName = "MenuOverlay", int layer = 200) {
        if (menuType == MenuType.None) {
            return;
        }

        var root = GetTree().Root;
        if (root == null) {
            GD.PrintErr("MenuManager: Cannot get root for overlay");
            return;
        }

        var existingOverlay = root.GetNodeOrNull<CanvasLayer>(layerName);
        if (existingOverlay != null) {
            GD.Print($"MenuManager: Overlay {layerName} already exists, ignored");
            return;
        }

        var overlayLayer = new CanvasLayer {
            Name = layerName,
            Layer = layer
        };
        root.AddChild(overlayLayer);

        if (_menuSceneCache.TryGetValue(menuType, out PackedScene menuScene)) {
            var menuInstance = menuScene.Instantiate<Control>();
            if (menuInstance != null) {
                overlayLayer.AddChild(menuInstance);
                menuInstance.SetAnchorsPreset(Control.LayoutPreset.FullRect);

                menuInstance.ProcessMode = ProcessModeEnum.Always;
                if (menuInstance.HasMethod("SetProcessInput")) {
                    menuInstance.Call("SetProcessInput", true);
                }

                CurrentMenu = menuType;

                GD.Print($"MenuManager: Menu {menuType} created in overlay {layerName}");
            }
            else {
                GD.PrintErr($"MenuManager: Failed to instantiate menu {menuType} in overlay");
                overlayLayer.QueueFree();
            }
        }
        else {
            GD.PrintErr($"MenuManager: Menu scene {menuType} not found for overlay");
            overlayLayer.QueueFree();
        }
    }

    /// <summary>
    /// Ferme un overlay spécifique
    /// </summary>
    /// <param name="layerName">Nom de la couche overlay à fermer</param>
    public void CloseOverlay(string layerName) {
        var root = GetTree().Root;
        var overlayLayer = root?.GetNodeOrNull<CanvasLayer>(layerName);

        if (overlayLayer != null) {
            overlayLayer.QueueFree();
            GD.Print($"MenuManager: Overlay {layerName} closed");

            if (layerName == "PauseOverlay") {
                GetTree().Paused = false;
                GD.Print("MenuManager: Game resumed after PauseOverlay closed");
            }

            if (CurrentMenu != MenuType.None) {
                CurrentMenu = MenuType.None;
            }
        }
    }

    /// <summary>
    /// Méthode spéciale pour ouvrir le menu de pause depuis le jeu
    /// </summary>
    public void ShowPauseMenu() {
        var root = GetTree().Root;
        var existingPauseOverlay = root?.GetNodeOrNull<CanvasLayer>("PauseOverlay");

        if (existingPauseOverlay != null) {
            GD.Print("MenuManager: PauseMenu already open, ignored");
            return;
        }

        if (GetTree().Paused) {
            GD.Print("MenuManager: Game already paused, ignored");
            return;
        }

        GetTree().Paused = true;
        ShowMenuAsOverlay(MenuType.PauseMenu, "PauseOverlay");
    }

    /// <summary>
    /// Nettoie les ressources lors de la destruction
    /// </summary>
    public override void _ExitTree() {
        DestroyAllMenus();

        if (Instance == this) {
            Instance = null;
        }
    }
}
