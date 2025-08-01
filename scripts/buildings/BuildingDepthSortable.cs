using Godot;

/// <summary>
/// Specialized depth sortable component for buildings.
/// Handles the special case where buildings are split into foundation and structure layers.
/// Uses collision detection to determine if players are inside or outside the building.
/// </summary>
public partial class BuildingDepthSortable : Node2D {

    /// <summary>
    /// Reference to the foundation TileMapLayer (used for position reference).
    /// </summary>
    [Export] public TileMapLayer FoundationLayer { get; set; }

    /// <summary>
    /// Reference to the base structure TileMapLayer (always behind the player).
    /// </summary>
    [Export] public TileMapLayer BaseStructureLayer { get; set; }

    /// <summary>
    /// Reference to the front structure TileMapLayer (behind player when outside, in front when inside).
    /// </summary>
    [Export] public TileMapLayer FrontStructureLayer { get; set; }

    /// <summary>
    /// Reference to the decoration TileMapLayer (will follow the same depth as structures but +1).
    /// </summary>
    [Export] public TileMapLayer DecorationLayer { get; set; }

    /// <summary>
    /// Reference to the roof structures TileMapLayer (always visible when outside, hidden when inside).
    /// </summary>
    [Export] public TileMapLayer RoofStructureLayer { get; set; }

    /// <summary>
    /// Reference to the roof structures TileMapLayer (always visible when outside, hidden when inside).
    /// </summary>
    [Export] public TileMapLayer RoofDecorationLayer { get; set; }

    /// <summary>
    /// Area2D used to detect if players are inside the building.
    /// </summary>
    [Export] public Area2D BuildingArea { get; set; }

    /// <summary>
    /// The depth sorter managing this building.
    /// </summary>
    private DepthSorter _depthSorter;

    /// <summary>
    /// Set of players currently inside the building.
    /// </summary>
    private readonly Godot.Collections.Array<Node2D> _playersInside = new();

    /// <summary>
    /// The base Z-index for this building (calculated from its Y position).
    /// </summary>
    public int BaseZIndex { get; private set; }

    /// <summary>
    /// Called when the node enters the scene tree.
    /// </summary>
    public override void _Ready() {
        if (FoundationLayer == null) {
            FoundationLayer = GetNode<TileMapLayer>("Foundations");
        }
        if (BaseStructureLayer == null) {
            BaseStructureLayer = GetNode<TileMapLayer>("BaseStructures");
        }
        if (FrontStructureLayer == null) {
            FrontStructureLayer = GetNodeOrNull<TileMapLayer>("FrontStructures");
        }
        if (DecorationLayer == null) {
            DecorationLayer = GetNodeOrNull<TileMapLayer>("Decorations");
        }
        if (RoofStructureLayer == null) {
            RoofStructureLayer = GetNodeOrNull<TileMapLayer>("RoofStructures");
        }
        if (RoofDecorationLayer == null) {
            RoofDecorationLayer = GetNodeOrNull<TileMapLayer>("RoofDecorations");
        }

        if (FoundationLayer == null || BaseStructureLayer == null) {
            GD.PrintErr($"BuildingDepthSortable {Name}: Could not find Foundation or BaseStructure layers");
            return;
        }

        CalculateBaseZIndex();

        SetupBuildingArea();

        // Initialize roof visibility (visible when no players inside)
        if (RoofStructureLayer != null && RoofDecorationLayer != null) {
            RoofStructureLayer.Visible = _playersInside.Count == 0;
            RoofDecorationLayer.Visible = _playersInside.Count == 0;
        }

        RegisterWithDepthSorter();
    }

    /// <summary>
    /// Calculates the base Z-index for this building based on its Y position.
    /// </summary>
    private void CalculateBaseZIndex() {
        var baseY = GlobalPosition.Y;
        BaseZIndex = 2000 + (int)(baseY * 10.0f);
    }

    /// <summary>
    /// Sets up the Area2D for collision detection if not already present.
    /// </summary>
    private void SetupBuildingArea() {
        if (BuildingArea == null) {
            GD.Print($"Building {Name}: Using existing BuildingArea: {BuildingArea != null}");
            BuildingArea = GetNodeOrNull<Area2D>("BuildingArea");
        }

        if (BuildingArea == null) {
            GD.Print($"No BuildingArea found for {Name}, creating a dynamic one");
            CreateBuildingArea();
        }

        if (BuildingArea != null) {
            BuildingArea.BodyEntered += OnPlayerEntered;
            BuildingArea.BodyExited += OnPlayerExited;
        }
    }

    /// <summary>
    /// Creates an Area2D based on foundation, base structures, and front structure tiles.
    /// </summary>
    private void CreateBuildingArea() {
        if (FoundationLayer == null || BaseStructureLayer == null) return;

        var foundationCells = FoundationLayer.GetUsedCells();
        var baseStructureCells = BaseStructureLayer.GetUsedCells();

        var allCells = new Godot.Collections.Array<Vector2I>();
        foreach (Vector2I cell in foundationCells) {
            allCells.Add(cell);
        }
        foreach (Vector2I cell in baseStructureCells) {
            if (!allCells.Contains(cell)) {
                allCells.Add(cell);
            }
        }

        if (FrontStructureLayer != null) {
            var frontStructureCells = FrontStructureLayer.GetUsedCells();
            foreach (Vector2I cell in frontStructureCells) {
                if (!allCells.Contains(cell)) {
                    allCells.Add(cell);
                }
            }
        }

        if (RoofStructureLayer != null) {
            var roofStructureCells = RoofStructureLayer.GetUsedCells();
            foreach (Vector2I cell in roofStructureCells) {
                if (!allCells.Contains(cell)) {
                    allCells.Add(cell);
                }
            }
        }

        if (RoofDecorationLayer != null) {
            var roofDecorationCells = RoofDecorationLayer.GetUsedCells();
            foreach (Vector2I cell in roofDecorationCells) {
                if (!allCells.Contains(cell)) {
                    allCells.Add(cell);
                }
            }
        }

        if (allCells.Count == 0) return;

        BuildingArea = new Area2D();
        BuildingArea.Name = "BuildingArea";
        AddChild(BuildingArea);

        var collisionShape = new CollisionShape2D();
        var rectangleShape = new RectangleShape2D();

        var minX = float.MaxValue;
        var maxX = float.MinValue;
        var minY = float.MaxValue;
        var maxY = float.MinValue;

        var tileSet = FoundationLayer.TileSet;
        var tileSize = tileSet.TileSize;

        foreach (Vector2I cell in allCells) {
            var worldPos = FoundationLayer.MapToLocal(cell);
            var halfTileSize = new Vector2(tileSize.X / 2.0f, tileSize.Y / 2.0f);

            minX = Mathf.Min(minX, worldPos.X - halfTileSize.X);
            maxX = Mathf.Max(maxX, worldPos.X + halfTileSize.X);
            minY = Mathf.Min(minY, worldPos.Y - halfTileSize.Y);
            maxY = Mathf.Max(maxY, worldPos.Y + halfTileSize.Y);
        }

        var width = maxX - minX;
        var height = maxY - minY;
        var centerX = (minX + maxX) / 2.0f;

        // Control both front and back extensions separately
        // God that was a pain to figure out
        var frontExtension = height * 0.11f;
        // I think sweet spot is 14%
        var backReduction = height * 0.14f;
        // Side reduction is 10% sweet spot atm
        var sideReduction = width * 0.1f;

        var newMinY = minY + backReduction;
        var newMaxY = maxY + frontExtension;
        var newMinX = minX + sideReduction;
        var newMaxX = maxX - sideReduction;
        
        var extendedHeight = newMaxY - newMinY;
        var extendedWidth = newMaxX - newMinX;
        var extendedCenterY = (newMinY + newMaxY) / 2.0f;
        var extendedCenterX = (newMinX + newMaxX) / 2.0f;

        rectangleShape.Size = new Vector2(extendedWidth, extendedHeight);
        collisionShape.Shape = rectangleShape;
        collisionShape.Position = new Vector2(extendedCenterX, extendedCenterY);

        BuildingArea.AddChild(collisionShape);

        var totalStructureCells = baseStructureCells.Count + (FrontStructureLayer?.GetUsedCells().Count ?? 0) + (RoofStructureLayer?.GetUsedCells().Count ?? 0) + (RoofDecorationLayer?.GetUsedCells().Count ?? 0);
        GD.Print($"Building {Name}: Auto-created collision area including {foundationCells.Count} foundation + {totalStructureCells} structure tiles");
        GD.Print($"Building {Name}: Extended collision area at ({extendedCenterX:F1}, {extendedCenterY:F1}) size ({extendedWidth:F1} x {extendedHeight:F1})");
        GD.Print($"Building {Name}: Front extension: {frontExtension:F1}, Back reduction: {backReduction:F1}, Side reduction: {sideReduction:F1} units");
    }

    /// <summary>
    /// Called when a player enters the building area.
    /// </summary>
    private void OnPlayerEntered(Node2D body) {
        if (body.GetType().Name == "Player" && !_playersInside.Contains(body)) {
            _playersInside.Add(body);
            GD.Print($"Player entered building {Name}");
            UpdatePlayerDepthSorting();
        }
    }

    /// <summary>
    /// Called when a player exits the building area.
    /// </summary>
    private void OnPlayerExited(Node2D body) {
        if (body.GetType().Name == "Player" && _playersInside.Contains(body)) {
            _playersInside.Remove(body);
            GD.Print($"Player exited building {Name}");
            UpdatePlayerDepthSorting();
        }
    }

    /// <summary>
    /// Updates the depth sorting for players relative to this building.
    /// </summary>
    private void UpdatePlayerDepthSorting() {
        if (_depthSorter == null) return;

        if (RoofStructureLayer != null && RoofDecorationLayer != null) {
            bool shouldBeVisible = _playersInside.Count == 0;
            if (RoofStructureLayer.Visible != shouldBeVisible) {
                RoofStructureLayer.Visible = shouldBeVisible;
                GD.Print($"Building {Name}: Roof visibility changed to {shouldBeVisible} (players inside: {_playersInside.Count})");
            }
            if (RoofDecorationLayer.Visible != shouldBeVisible) {
                RoofDecorationLayer.Visible = shouldBeVisible;
                GD.Print($"Building {Name}: Roof decoration visibility changed to {shouldBeVisible} (players inside: {_playersInside.Count})");
            }
        }

        // Force a depth sorter update
        _depthSorter.ForceUpdate();
    }

    /// <summary>
    /// Checks if a specific player is inside this building.
    /// </summary>
    public bool IsPlayerInside(Node2D player) {
        return _playersInside.Contains(player);
    }

    /// <summary>
    /// Register this building with the depth sorter.
    /// </summary>
    private void RegisterWithDepthSorter() {
        _depthSorter = GetNode<DepthSorter>("../DepthSorter");
        if (_depthSorter == null) {
            var mainScene = GetTree().CurrentScene;
            _depthSorter = mainScene?.GetNode<DepthSorter>("DepthSorter");
        }

        if (_depthSorter != null) {
            _depthSorter.RegisterObject(this);
        }
        else {
            GD.PrintErr($"BuildingDepthSortable {Name}: Could not find DepthSorter in scene");
        }
    }

    /// <summary>
    /// Gets the effective position for depth sorting (center of the building).
    /// </summary>
    public Vector2 DepthSortPosition {
        get {
            return GlobalPosition;
        }
    }

    /// <summary>
    /// Called when the node is about to be removed from the scene tree.
    /// </summary>
    public override void _ExitTree() {
        if (_depthSorter != null) {
            _depthSorter.UnregisterObject(this);
        }
        base._ExitTree();
    }
}
