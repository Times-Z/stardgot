using Godot;


public partial class TimeDisplayComponent : Control {

	/// <summary>
	/// The Label node used to display the current game time.
	/// This label will show the time in HH:MM format with a clock icon.
	/// </summary>
	private Label _timeLabel;

	/// <summary>
	/// The TextureRect node used to display the weather icon.
	/// This icon will change based on the current weather conditions in the game.
	/// </summary>
	private TextureRect _weatherIcon;

	[Export] private Texture2D _weatherAtlasTexture;

	private bool _isSignalConnected = false;

	/// <summary>
	/// Initializes the TimeDisplayComponent by adding it to the "time_display" group
	/// and retrieving the RichTextLabel node for displaying the time.
	/// </summary>
	public override void _Ready() {
		AddToGroup("time_display");

		_timeLabel = GetNode<Label>("TimePanel/TimeContainer/TimeLabel");
		_weatherIcon = GetNode<TextureRect>("TimePanel/TimeContainer/WeatherIcon");
		GD.Print($"TimeDisplayComponent._Ready() - Label found: {_timeLabel != null}");
	}

	/// <summary>
	/// Updates the displayed time every frame based on the current game state.
	/// If the game is in progress, it retrieves the formatted time from GameTimeManager
	/// and updates the RichTextLabel text.
	/// </summary>
	public override void _Process(double delta) {
		if (GameRoot.Instance.CurrentState == GameState.InGame && GameTimeManager.Instance != null) {
			if (!_isSignalConnected) {
				WeatherManager.Instance.Connect("WeatherChanged", Callable.From(OnWeatherChanged));
				OnWeatherChanged();
				_isSignalConnected = true;
			}
			_timeLabel.Text = GameTimeManager.Instance.GetFormattedTime();
		}
	}

	/// <summary>
	/// Called when the weather changes. Updates the weather icon accordingly.
	/// </summary>
	private void OnWeatherChanged() {
		Rect2 region = new Rect2(64, 992, 32, 32);

		switch (WeatherManager.Instance.CurrentWeather) {
			case WeatherManager.WeatherType.Sunny:
				region = new Rect2(64, 992, 32, 32);
				break;
			case WeatherManager.WeatherType.Rainy:
				region = new Rect2(160, 1056, 32, 32);
				break;
			case WeatherManager.WeatherType.Cloudy:
				region = new Rect2(224, 992, 32, 32);
				break;
			case WeatherManager.WeatherType.Stormy:
				region = new Rect2(224, 1056, 32, 32);
				break;
		}

		var atlasTexture = new AtlasTexture();
		atlasTexture.Atlas = _weatherAtlasTexture;
		atlasTexture.Region = region;

		_weatherIcon.Texture = atlasTexture;
	}
}
