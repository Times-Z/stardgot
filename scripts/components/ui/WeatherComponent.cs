using Godot;

public partial class WeatherComponent : Control {
	/// <summary>
	/// GPU particle system for rain effect.
	/// </summary>
	private GpuParticles2D _rainParticles;

	/// <summary>
	/// Indicates if the WeatherChanged signal is already connected.
	/// </summary>
	private bool _isSignalConnected = false;

	[Export]
	public float DefaultLifetimeRandomness = 1.5f;

	[Export]
	public float DefaultInitialVelocityMin = 400;

	[Export]
	public float DefaultInitialVelocityMax = 600;

	[Export]
	public int DefaultAmount = 100;


	/// <summary>
	/// Initializes the rain particle system and adds it to the scene.
	/// </summary>
	private void InitRainEffect() {
		_rainParticles = new GpuParticles2D();
		var rainTexture = GD.Load<Texture2D>("res://assets/others/rain/drops/0.png");
		var rainMaterial = new ParticleProcessMaterial {
			LifetimeRandomness = DefaultLifetimeRandomness,
			InitialVelocityMin = DefaultInitialVelocityMin,
			InitialVelocityMax = DefaultInitialVelocityMax,
			Gravity = new Vector3(0, 800, 0),
			Direction = new Vector3(0, 1, 0)
		};

		_rainParticles.Texture = rainTexture;
		_rainParticles.ProcessMaterial = rainMaterial;
		_rainParticles.Amount = DefaultAmount;
		_rainParticles.Emitting = false;

		AddChild(_rainParticles);

		UpdateParticlesRect();
	}

	/// <summary>
	/// Called every frame. Connects to WeatherManager if needed and updates rain effect.
	/// </summary>
	public override void _Process(double delta) {
		if (WeatherManager.Instance != null && !_isSignalConnected) {
			WeatherManager.Instance.WeatherChanged += OnWeatherChanged;
			_isSignalConnected = true;
			UpdateWeatherDisplay(WeatherManager.Instance.CurrentWeather);
		}
		UpdateParticlesRect();
	}

	/// <summary>
	/// Updates the position and emission area of the rain particles to match the camera viewport.
	/// </summary>
	private void UpdateParticlesRect() {
		var camera = GetViewport().GetCamera2D();
		if (_rainParticles == null || _rainParticles.ProcessMaterial == null || camera == null)
			return;

		var viewportSize = GetViewportRect().Size;
		var zoom = camera.Zoom;
		var half = viewportSize * zoom / 2f;

		GlobalPosition = camera.GlobalPosition - half;

		_rainParticles.Position = half;
		_rainParticles.Scale = Vector2.One;
		_rainParticles.LocalCoords = false;

		if (_rainParticles.ProcessMaterial is ParticleProcessMaterial mat) {
			mat.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box;
			mat.EmissionBoxExtents = new Vector3(half.X, half.Y, 0);
		}
	}

	/// <summary>
	/// Handles notifications (e.g., window resize) to update rain particle area.
	/// </summary>
	public override void _Notification(int what) {
		if (_rainParticles != null)
			UpdateParticlesRect();
	}

	/// <summary>
	/// Callback for when the weather changes. Updates the rain effect.
	/// </summary>
	private void OnWeatherChanged() {
		if (WeatherManager.Instance != null)
			UpdateWeatherDisplay(WeatherManager.Instance.CurrentWeather);
	}

	/// <summary>
	/// Enables or disables the rain effect based on the current weather type.
	/// </summary>
	/// <param name="weather">The current weather type from WeatherManager.</param>
	private void UpdateWeatherDisplay(WeatherManager.WeatherType weather) {
		switch (weather) {
			case WeatherManager.WeatherType.Rainy:
			case WeatherManager.WeatherType.Stormy:
				SetRainActive(true);
				break;
			case WeatherManager.WeatherType.Cloudy:
				SetRainActive(false);
				break;
			case WeatherManager.WeatherType.Sunny:
			default:
				SetRainActive(false);
				break;
		}
	}

	private void SetRainActive(bool active) {
		if (_rainParticles != null)
			_rainParticles.Emitting = active;
	}

	public override void _Ready() {
		InitRainEffect();
	}
}
