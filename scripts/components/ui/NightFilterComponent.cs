using Godot;

public partial class NightFilterComponent : Control {

    /// <summary>
    /// The ColorRect node that serves as the night filter overlay.
    /// This node is used to darken the screen during night time in the game.
    /// </summary>
    private ColorRect _nightFilter;

    /// <summary>
    /// The maximum alpha value for the night filter, determining how dark the screen gets.
    /// This value is exported to allow configuration in the Godot editor.
    /// </summary>
    [Export] private float _nightAlphaMax = 0.5f;

    /// <summary>
    /// The speed at which the night filter transitions between visible and invisible states.
    /// This value is exported to allow configuration in the Godot editor.
    /// </summary>
    [Export] private float _transitionSpeed = 1f;

    /// <summary>
    /// The target alpha value for the night filter, which determines its visibility.
    /// This value is set based on whether it is currently night in the game.
    /// </summary>
    private float _targetAlpha = 0.5f;

    /// <summary>
    /// Initializes the NightFilterComponent by adding it to the "night_filter" group
    /// and setting the initial visibility of the night filter based on the current game time.
    /// If it is night, the filter will be set to its maximum alpha value; otherwise, it will be invisible.
    /// </summary>
    public override void _Ready() {
        AddToGroup("night_filter");

        _nightFilter = GetNode<ColorRect>("NightFilterRect");
        bool isNight = GameTimeManager.Instance != null && GameTimeManager.Instance.IsNight();
        _targetAlpha = isNight ? _nightAlphaMax : 0f;
        _nightFilter.Modulate = new Color(0, 0, 0.2f, _targetAlpha);
        _nightFilter.Visible = true;
    }

    /// <summary>
    /// Processes the night filter every frame, adjusting its alpha value towards the target alpha.
    /// This creates a smooth transition effect for the night filter overlay.
    /// </summary>
    public override void _Process(double delta) {
        if (_nightFilter == null) return;

        float currentAlpha = _nightFilter.Modulate.A;
        float newAlpha = Mathf.Lerp(currentAlpha, _targetAlpha, (float)delta * _transitionSpeed);
        _nightFilter.Modulate = new Color(0, 0, 0.2f, newAlpha);
    }

    /// <summary>
    /// Sets the target alpha value for the night filter based on whether it is currently night.
    /// If it is night, the target alpha is set to the maximum value; otherwise,
    /// it is set to zero, making the filter invisible.
    /// </summary>
    public void SetNight(bool isNight) {
        _targetAlpha = isNight ? _nightAlphaMax : 0f;
    }
}