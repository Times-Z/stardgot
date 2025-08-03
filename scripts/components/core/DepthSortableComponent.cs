using Godot;

/// <summary>
/// Component that makes any Node2D depth sortable.
/// Automatically registers with the scene's DepthSorter and provides customizable sorting behavior.
/// </summary>
public partial class DepthSortableComponent : Node {
	/// <summary>
	/// Whether this object should be automatically registered with the depth sorter on ready.
	/// </summary>
	[Export] public bool AutoRegister { get; set; } = true;

	/// <summary>
	/// Custom offset to add to the sorting position. Useful for fine-tuning object layering.
	/// </summary>
	[Export] public Vector2 SortingOffset { get; set; } = Vector2.Zero;

	/// <summary>
	/// Whether to use a custom sorting position instead of the parent's global position.
	/// </summary>
	[Export] public bool UseCustomSortPosition { get; set; } = false;

	/// <summary>
	/// Custom sorting position (only used if UseCustomSortPosition is true).
	/// </summary>
	[Export] public Vector2 CustomSortPosition { get; set; } = Vector2.Zero;

	/// <summary>
	/// Priority modifier for depth sorting. Higher values appear in front of lower values at the same Y position.
	/// </summary>
	[Export] public int SortingPriority { get; set; } = 0;

	/// <summary>
	/// Name for debugging and logging purposes.
	/// </summary>
	[Export] public string ComponentName { get; set; } = "DepthSortable";

	/// <summary>
	/// The parent Node2D that will be depth sorted.
	/// </summary>
	private Node2D _parentNode;

	/// <summary>
	/// Reference to the scene's depth sorter.
	/// </summary>
	private DepthSorter _depthSorter;

	/// <summary>
	/// Called when the node enters the scene tree.
	/// Finds the parent Node2D and registers with the depth sorter if AutoRegister is enabled.
	/// </summary>
	public override void _Ready() {
		_parentNode = GetParent<Node2D>();
		
		if (_parentNode == null) {
			GD.PrintErr($"DepthSortableComponent: Parent must be a Node2D. Found: {GetParent()?.GetType().Name ?? "null"}");
			return;
		}

		if (string.IsNullOrEmpty(ComponentName)) {
			ComponentName = _parentNode.Name;
		}

		if (AutoRegister) {
			RegisterWithDepthSorter();
		}
	}

	/// <summary>
	/// Manually registers this object with the depth sorter.
	/// Useful if AutoRegister is disabled or if you need to re-register after unregistering.
	/// </summary>
	public void RegisterWithDepthSorter() {
		if (_depthSorter != null) {
			GD.Print($"{ComponentName}: Already registered with depth sorter");
			return;
		}

		// Try to find the depth sorter in common locations
		_depthSorter = FindDepthSorter();

		if (_depthSorter != null) {
			_depthSorter.RegisterObject(_parentNode);
			GD.Print($"{ComponentName}: Registered with depth sorter");
		} else {
			GD.PrintErr($"{ComponentName}: Could not find DepthSorter in scene");
		}
	}

	/// <summary>
	/// Unregisters this object from the depth sorter.
	/// </summary>
	public void UnregisterFromDepthSorter() {
		if (_depthSorter != null && _parentNode != null) {
			_depthSorter.UnregisterObject(_parentNode);
			_depthSorter = null;
			GD.Print($"{ComponentName}: Unregistered from depth sorter");
		}
	}

	/// <summary>
	/// Gets the effective position for depth sorting.
	/// Takes into account custom positions and offsets.
	/// </summary>
	public Vector2 GetSortingPosition() {
		if (UseCustomSortPosition) {
			return CustomSortPosition + SortingOffset;
		}

		if (_parentNode != null) {
			return _parentNode.GlobalPosition + SortingOffset;
		}

		return Vector2.Zero;
	}

	/// <summary>
	/// Sets a custom sorting position at runtime.
	/// </summary>
	/// <param name="position">The new custom sorting position</param>
	/// <param name="useCustom">Whether to enable custom position mode</param>
	public void SetCustomSortPosition(Vector2 position, bool useCustom = true) {
		CustomSortPosition = position;
		UseCustomSortPosition = useCustom;
	}

	/// <summary>
	/// Forces an immediate depth sorting update.
	/// Useful when the object changes position significantly.
	/// </summary>
	public void ForceDepthUpdate() {
		_depthSorter?.ForceUpdate();
	}

	/// <summary>
	/// Searches for a DepthSorter in the scene hierarchy.
	/// </summary>
	/// <returns>The found DepthSorter or null if not found</returns>
	private DepthSorter FindDepthSorter() {
		// First, try to find it as a sibling
		Node parent = _parentNode.GetParent();
		if (parent != null) {
			var sorter = parent.GetNodeOrNull<DepthSorter>("DepthSorter");
			if (sorter != null) return sorter;
		}

		// Then try the current scene root
		var mainScene = GetTree().CurrentScene;
		if (mainScene != null) {
			var sorter = mainScene.GetNodeOrNull<DepthSorter>("DepthSorter");
			if (sorter != null) return sorter;

			// Also try looking in common locations
			sorter = mainScene.FindChild("DepthSorter", recursive: true, owned: false) as DepthSorter;
			if (sorter != null) return sorter;
		}

		// Finally, try searching the entire tree
		foreach (Node node in GetTree().GetNodesInGroup("depth_sorters")) {
			if (node is DepthSorter depthSorter) {
				return depthSorter;
			}
		}

		return null;
	}

	/// <summary>
	/// Gets information about the current depth sorting state.
	/// </summary>
	/// <returns>Debug information string</returns>
	public string GetDebugInfo() {
		var info = $"DepthSortableComponent '{ComponentName}':\n";
		info += $"  Parent: {_parentNode?.Name ?? "null"}\n";
		info += $"  Registered: {_depthSorter != null}\n";
		info += $"  Sort Position: {GetSortingPosition()}\n";
		info += $"  Priority: {SortingPriority}\n";
		info += $"  Auto Register: {AutoRegister}\n";
		info += $"  Use Custom Position: {UseCustomSortPosition}";
		
		return info;
	}

	/// <summary>
	/// Clean up when the node is about to be removed.
	/// </summary>
	public override void _ExitTree() {
		UnregisterFromDepthSorter();
		base._ExitTree();
	}
}
