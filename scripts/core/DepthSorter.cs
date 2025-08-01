using Godot;

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages depth sorting for game objects based on their Y position.
/// This ensures proper visual layering where objects further down appear in front of objects above them.
/// Designed to work with moving cameras and dynamic object positions.
/// That's hard to implement
/// </summary>
public partial class DepthSorter : Node2D {

	/// <summary>
	/// List of all objects that should be depth sorted.
	/// </summary>
	private readonly List<Node2D> _sortableObjects = new();

	/// <summary>
	/// How often to update the sorting (in seconds). Lower values = more responsive but more expensive.
	/// </summary>
	[Export] public float UpdateInterval { get; set; } = 0.1f;

	/// <summary>
	/// Base Z-index for depth sorting calculations.
	/// </summary>
	[Export] public int BaseZIndex { get; set; } = 0;

	/// <summary>
	/// Multiplier for converting Y position to Z-index. Higher values = more separation between layers.
	/// </summary>
	[Export] public float DepthMultiplier { get; set; } = 10.0f;

	private double _timeSinceLastUpdate = 0.0;

	/// <summary>
	/// Called when the node enters the scene tree.
	/// </summary>
	public override void _Ready() {
		GD.Print("DepthSorter _Ready");
		UpdateDepthSorting();
	}

	/// <summary>
	/// Called every frame to check if sorting update is needed.
	/// </summary>
	/// <param name="delta">Time elapsed since last frame</param>
	public override void _Process(double delta) {
		_timeSinceLastUpdate += delta;

		if (_timeSinceLastUpdate >= UpdateInterval) {
			UpdateDepthSorting();
			_timeSinceLastUpdate = 0.0;
		}
	}

	/// <summary>
	/// Registers an object for depth sorting.
	/// </summary>
	/// <param name="obj">The Node2D object to be sorted</param>
	public void RegisterObject(Node2D obj) {
		if (obj != null && !_sortableObjects.Contains(obj)) {
			_sortableObjects.Add(obj);
			GD.Print($"Registered object for depth sorting: {obj.Name}");
		}
	}

	/// <summary>
	/// Unregisters an object from depth sorting.
	/// </summary>
	/// <param name="obj">The Node2D object to stop sorting</param>
	public void UnregisterObject(Node2D obj) {
		if (obj != null && _sortableObjects.Contains(obj)) {
			_sortableObjects.Remove(obj);
			GD.Print($"Unregistered object from depth sorting: {obj.Name}");
		}
	}

	/// <summary>
	/// Updates the Z-index of all registered objects based on their Y position.
	/// Objects with higher Y values (further down) will have higher Z-index (appear in front).
	/// </summary>
	private void UpdateDepthSorting() {
		_sortableObjects.RemoveAll(obj => !IsInstanceValid(obj));

		foreach (var obj in _sortableObjects) {
			if (IsInstanceValid(obj)) {
				Vector2 sortPosition = obj.GlobalPosition;

				if (obj is BuildingDepthSortable building) {
					bool hasPlayersInside = false;
					int playerZIndex = BaseZIndex + 500; // Fixed player Z-index
					
					// Check if players are inside a building
					foreach (var otherObj in _sortableObjects) {
						if (otherObj != obj && otherObj.GetType().Name == "Player") {
							if (building.IsPlayerInside(otherObj)) {
								hasPlayersInside = true;
								break;
							}
						}
					}

					// BaseStructures should always be behind player on exterior
					if (building.BaseStructureLayer != null) {
						building.BaseStructureLayer.ZIndex = playerZIndex - 1;
					}

					// FrontStructures behavior depends on player location
					if (building.FrontStructureLayer != null) {
						int frontStructureZIndex;
						if (hasPlayersInside) {
							// Players inside: front structures go above player
							frontStructureZIndex = playerZIndex + 1;
						} else {
							frontStructureZIndex = playerZIndex - 1;
						}

						building.FrontStructureLayer.ZIndex = frontStructureZIndex;
					}

					// Decorations should always be behind player
					if (building.DecorationLayer != null) {
						building.DecorationLayer.ZIndex = playerZIndex - 1;
					}

					// RoofStructures should always be on top when visible
					if (building.RoofStructureLayer != null && building.RoofDecorationLayer != null) {
						building.RoofStructureLayer.ZIndex = building.BaseZIndex + 1;
						building.RoofDecorationLayer.ZIndex = building.BaseZIndex + 1;
					}
				}
				else {
					if (obj.GetType().Name == "Player") {
						// Fixed Z-index for players - they don't change depth
						obj.ZIndex = BaseZIndex + 500;
					} else {
						// Other objects still use normal depth sorting
						int newZIndex = BaseZIndex + (int)(sortPosition.Y * DepthMultiplier);
						newZIndex = Mathf.Max(0, newZIndex);
						obj.ZIndex = newZIndex;
					}
				}
			}
		}
	}

	/// <summary>
	/// Forces an immediate depth sorting update.
	/// Useful when objects change position significantly.
	/// </summary>
	public void ForceUpdate() {
		UpdateDepthSorting();
	}

	/// <summary>
	/// Gets all currently registered objects.
	/// </summary>
	/// <returns>Read-only list of registered objects</returns>
	public IReadOnlyList<Node2D> GetRegisteredObjects() {
		return _sortableObjects.AsReadOnly();
	}

	/// <summary>
	/// Clears all registered objects.
	/// </summary>
	public void ClearAllObjects() {
		_sortableObjects.Clear();
		GD.Print("Cleared all registered objects from depth sorter");
	}
}
