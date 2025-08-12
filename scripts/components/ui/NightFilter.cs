using Godot;

public partial class NightFilter : ColorRect {

	public override void _Ready() {
		Visible = false;
	}

	/// <summary>
	/// Updates the visibility of the night filter based on whether it is currently night.
	/// </summary>
	public void UpdateVisibility(bool isNight) {
		Visible = isNight;
	}
}
