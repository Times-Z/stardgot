using Godot;

using System;

public partial class WeatherManager : Node {
    public static WeatherManager Instance { get; private set; }

    public enum WeatherType {
        Sunny,
        Rainy,
        Cloudy,
        Stormy
    }

    public WeatherType CurrentWeather { get; private set; } = WeatherType.Sunny;

    [Signal]
    public delegate void WeatherChangedEventHandler();

    private int _lastWeatherChangeHour = 0;
    private int _nextWeatherChangeInHours = 1;
    private Random _random = new Random();
    private int _sameWeatherCount = 1;

    public override void _Ready() {
        if (Instance == null) {
            Instance = this;
            _lastWeatherChangeHour = GameTimeManager.Instance.Hours;
            SetNextWeatherChange();
            ChangeWeather();
        }
        else {
            QueueFree();
        }
    }

    public override void _Process(double delta) {
        if (GameRoot.Instance.CurrentState != GameState.InGame)
            return;

        int currentHour = GameTimeManager.Instance.Hours;
        int hoursPassed = (currentHour - _lastWeatherChangeHour + 24) % 24;

        if (hoursPassed >= _nextWeatherChangeInHours) {
            ChangeWeather();
            _lastWeatherChangeHour = currentHour;
            SetNextWeatherChange();
        }
    }

    private void SetNextWeatherChange() {
        _nextWeatherChangeInHours = _random.Next(1, 5);
    }

    private void ChangeWeather() {
        bool shouldChange = _random.NextDouble() < 0.6 || _sameWeatherCount >= 2;
        WeatherType newWeather = CurrentWeather;

        if (shouldChange) {
            Array values = Enum.GetValues(typeof(WeatherType));
            do {
                newWeather = (WeatherType)values.GetValue(_random.Next(values.Length));
            } while (newWeather == CurrentWeather);
        }

        if (newWeather == CurrentWeather) {
            _sameWeatherCount++;
        }
        else {
            _sameWeatherCount = 1;
        }

        if (newWeather != CurrentWeather || _lastWeatherChangeHour == GameTimeManager.Instance.Hours) {
            CurrentWeather = newWeather;
            EmitSignal("WeatherChanged");
        }
        else {
            CurrentWeather = newWeather;
        }

        GD.Print($"[Day {GameTimeManager.Instance.Day}, {GameTimeManager.Instance.Hours}:00] Weather changed to: {CurrentWeather} (next in {_nextWeatherChangeInHours}h)");
    }
}