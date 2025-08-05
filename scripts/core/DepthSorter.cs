using Godot;

using System.Collections.Generic;

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
						SetLayerZIndexWithChildren(building.BaseStructureLayer, playerZIndex - 1);
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

						SetLayerZIndexWithChildren(building.FrontStructureLayer, frontStructureZIndex);
					}

					// Decorations should always be behind player
					if (building.DecorationLayer != null) {
						SetLayerZIndexWithChildren(building.DecorationLayer, playerZIndex - 1);
					}

					// RoofStructures should always be on top when visible
					if (building.RoofStructureLayer != null && building.RoofDecorationLayer != null) {
						SetLayerZIndexWithChildren(building.RoofStructureLayer, building.BaseZIndex + 1);
						SetLayerZIndexWithChildren(building.RoofDecorationLayer, building.BaseZIndex + 1);
					}
				}
				else {
					if (obj.GetType().Name == "Player") {
						// Fixed Z-index for players - they don't change depth
						SetNodeZIndexWithChildren(obj, BaseZIndex + 500);
					} else {
						// Other objects still use normal depth sorting
						int newZIndex = BaseZIndex + (int)(sortPosition.Y * DepthMultiplier);
						newZIndex = Mathf.Max(0, newZIndex);
						SetNodeZIndexWithChildren(obj, newZIndex);
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
	public IReadOnlyList<Node2D> GetRegisteredObjects() => _sortableObjects.AsReadOnly();

	/// <summary>
	/// Clears all registered objects.
	/// </summary>
	public void ClearAllObjects() {
		_sortableObjects.Clear();
		GD.Print("Cleared all registered objects from depth sorter");
	}

	/// <summary>
	/// Sets the Z-index of a layer and propagates it to all its children recursively.
	/// This ensures that child nodes inherit the same Z-index as their parent layer.
	/// </summary>
	/// <param name="layer">The layer node to set Z-index for</param>
	/// <param name="zIndex">The Z-index value to set</param>
	private void SetLayerZIndexWithChildren(Node2D layer, int zIndex) {
		if (layer == null) return;
		
		// GD.Print($"SetLayerZIndexWithChildren: Setting Z-index {zIndex} for layer {layer.Name} (type: {layer.GetType().Name})");
		layer.ZIndex = zIndex;

		PropagateZIndexToChildren(layer, zIndex);
	}

	/// <summary>
	/// Sets the Z-index of a single node and propagates it to all its children recursively.
	/// This is a wrapper around SetLayerZIndexWithChildren for consistency.
	/// </summary>
	/// <param name="node">The node to set Z-index for</param>
	/// <param name="zIndex">The Z-index value to set</param>
	private void SetNodeZIndexWithChildren(Node2D node, int zIndex) {
		SetLayerZIndexWithChildren(node, zIndex);
	}

	/// <summary>
	/// Recursively propagates Z-index to all child nodes of a given parent.
	/// This method traverses the entire node tree under the parent and sets
	/// the Z-index for any Node2D children.
	/// </summary>
	/// <param name="parent">The parent node to start propagation from</param>
	/// <param name="zIndex">The Z-index value to propagate</param>
	private void PropagateZIndexToChildren(Node parent, int zIndex) {
		foreach (Node child in parent.GetChildren()) {
			if (child is Node2D child2D) {
				int oldZIndex = child2D.ZIndex;
				bool oldZAsRelative = child2D.ZAsRelative;
				child2D.ZAsRelative = false;
				child2D.ZIndex = zIndex;

				if (child2D.ZIndex != zIndex) {
					GD.PrintErr($"WARNING: Failed to set Z-index for {child2D.Name}! Expected: {zIndex}, Got: {child2D.ZIndex}");
					// Try to force it again
					child2D.ZIndex = zIndex;
				}
			}
			
			// Always recurse to children, regardless of their type
			PropagateZIndexToChildren(child, zIndex);
		}
	}
}
