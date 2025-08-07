using Godot;

public partial class GameTimeManager : Node {

    /// <summary>
    /// Gets the singleton instance of the GameTimeManager.
    /// This property provides global access to the single instance of the GameTimeManager class.
    /// </summary>
    /// <value>The singleton instance of GameTimeManager, or null if not yet initialized.</value>
    public static GameTimeManager Instance { get; private set; }

    /// <summary>
    /// The time scale multiplier that controls how fast game time progresses relative to real time.
    /// A value of 1.0 means game time matches real time, values greater than 1.0 speed up time,
    /// and values less than 1.0 slow down time.
    /// </summary>
    [Export] public float TimeScale = 50.0f;

    /// <summary>
    /// The hour (0-23) at which the game day starts when the game begins or resets.
    /// </summary>
    [Export] public int StartHour = 8;

    /// <summary>
    /// The starting minute value for the game time when the game begins.
    /// This property is exported and can be configured in the Godot editor.
    /// </summary>
    [Export] public int StartMinute = 0;

    /// <summary>
    /// The starting day value when the game begins.
    /// This property is exported and can be configured in the Godot editor.
    /// </summary>
    [Export] public int StartDay = 1;

    /// <summary>
    /// The notification string used to inform the player when a new day starts.
    /// This string is formatted with the current day number and can be customized in the Godot editor.
    /// </summary>
    [Export] public string NotificationString = "Day {0} has started !";

    /// <summary>
    /// The current game time in seconds since the game started or was last reset.
    /// </summary>
    private float _gameTimeSeconds = 0f;

    /// <summary>
    /// The current day in the game timeline, starting from day 1.
    /// </summary>
    private int _currentDay;

    /// <summary>
    /// Gets the current hour of the day based on the total game time in seconds.
    /// The value is between 0 and 23, representing the hour in a 24-hour format.
    /// </summary>
    public int Hours => Mathf.FloorToInt(_gameTimeSeconds / 3600) % 24;

    /// <summary>
    /// Gets the current minute value of the in-game time, calculated from the total game time in seconds.
    /// The value is always between 0 and 59.
    /// </summary>
    public int Minutes => Mathf.FloorToInt(_gameTimeSeconds / 60) % 60;

    /// <summary>
    /// Gets the current day in the game timeline.
    /// This value starts at the configured StartDay and increments as time progresses.
    /// </summary>
    public int Day => _currentDay;

    /// <summary>
    /// Gets the total game time in seconds since the game started or was last reset.
    /// This value is updated every frame based on the TimeScale and the delta time.
    /// </summary>
    public override void _Ready() {
        if (Instance == null) {
            Instance = this;
            _gameTimeSeconds = (StartHour * 3600) + (StartMinute * 60);
            _currentDay = StartDay;
            NotificationComponent.Instance.ShowNotify(string.Format(NotificationString, _currentDay));
        }
        else {
            QueueFree();
        }
    }

    /// <summary>
    /// Processes the game time every frame, updating the total game time in seconds
    /// and checking if a new day has started based on the elapsed time.
    /// The game time is scaled by the TimeScale factor, and the current day is updated
    /// accordingly when a full day (86400 seconds) has passed.
    /// </summary>
    public override void _Process(double delta) {
        if (GameRoot.Instance.CurrentState == GameState.InGame) {
            float previousTimeSeconds = _gameTimeSeconds;
            _gameTimeSeconds += (float)delta * TimeScale;

            int previousDay = Mathf.FloorToInt(previousTimeSeconds / 86400);
            int currentDay = Mathf.FloorToInt(_gameTimeSeconds / 86400);

            if (currentDay > previousDay) {
                _currentDay += currentDay - previousDay;
                NotificationComponent.Instance.ShowNotify(string.Format(NotificationString, _currentDay));
            }
        }
    }

    /// <summary>
    /// Gets the formatted time string for display in the UI, including the rich text image and the
    /// current time in HH:MM format.
    /// </summary>
    public string GetFormattedTime() => $"{Hours:D2}:{Minutes:D2}";
}