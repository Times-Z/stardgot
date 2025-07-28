using Godot;

/// <summary>
/// Manages background music playback for menu scenes.
/// This class handles starting, stopping, and controlling audio streams
/// that play during menu navigation and interactions.
/// </summary>
public partial class MenuMusicPlayer : Node
{
	/// <summary>
	/// The audio stream player component that handles actual audio playback.
	/// </summary>
	private AudioStreamPlayer _player;
	
	/// <summary>
	/// The audio stream resource containing the background music for menus.
	/// Should be assigned in the Godot editor.
	/// </summary>
	[Export] public AudioStream MusicStream;

	/// <summary>
	/// Called when the node enters the scene tree for the first time.
	/// Creates an audio stream player, assigns the music stream, and starts playback automatically.
	/// </summary>
	public override void _Ready()
	{
		GD.Print("MusicPlayer _Ready");
		_player = new AudioStreamPlayer();
		AddChild(_player);
		if (MusicStream != null)
		{
			_player.Stream = MusicStream;
			_player.Autoplay = true;
			_player.Play();
		}
	}

	/// <summary>
	/// Stops the currently playing background music.
	/// This method is typically called when transitioning away from menu scenes.
	/// </summary>
	public void StopMusic()
	{
		_player?.Stop();
	}

	/// <summary>
	/// Starts playing the background music.
	/// Assigns the music stream to the player and begins playback.
	/// Logs a debug message when music playback starts.
	/// </summary>
	public void PlayMusic()
	{
		GD.Print("MusicPlayer PlayMusic");
		if (_player != null && MusicStream != null)
		{
			_player.Stream = MusicStream;
			_player.Play();
		}
	}
}
