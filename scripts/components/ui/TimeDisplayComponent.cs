using Godot;


public partial class TimeDisplayComponent : Control {

    /// <summary>
    /// The Label node used to display the current game time.
    /// This label will show the time in HH:MM format with a clock icon.
    /// </summary>
    private Label _timeLabel;

    /// <summary>
    /// Initializes the TimeDisplayComponent by adding it to the "time_display" group
    /// and retrieving the RichTextLabel node for displaying the time.
    /// </summary>
    public override void _Ready() {
        AddToGroup("time_display");

        _timeLabel = GetNode<Label>("TimePanel/TimeContainer/TimeLabel");
        GD.Print($"TimeDisplayComponent._Ready() - Label found: {_timeLabel != null}");
    }

    /// <summary>
    /// Updates the displayed time every frame based on the current game state.
    /// If the game is in progress, it retrieves the formatted time from GameTimeManager
    /// and updates the RichTextLabel text.
    /// </summary>
    public override void _Process(double delta) {
        if (GameRoot.Instance.CurrentState == GameState.InGame && GameTimeManager.Instance != null) {
            _timeLabel.Text = GameTimeManager.Instance.GetFormattedTime();
        }
    }
}
