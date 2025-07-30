using Godot;

/// <summary>
/// Manages background music playback for game scenes.
/// This class handles starting, stopping, and controlling audio streams
/// that play during actual gameplay.
/// </summary>
public partial class GameMusicPlayer : Node {
	/// <summary>
	/// The audio stream player component that handles actual audio playback.
	/// </summary>
	private AudioStreamPlayer _player;

	/// <summary>
	/// The audio stream resource containing the background music for gameplay.
	/// Should be assigned in the Godot editor.
	/// </summary>
	[Export] public AudioStream MusicStream;

	/// <summary>
	/// Called when the node enters the scene tree for the first time.
	/// Creates an audio stream player and assigns the music stream.
	/// Does not auto-play - music is started when the game begins.
	/// </summary>
	public override void _Ready() {
		GD.Print("GameMusicPlayer _Ready");
		_player = new AudioStreamPlayer();
		AddChild(_player);
		if (MusicStream != null) {
			_player.Stream = MusicStream;
			_player.VolumeDb = -10;
		}
	}

	/// <summary>
	/// Stops the currently playing background music.
	/// This method is typically called when transitioning back to menu scenes.
	/// </summary>
	public void StopMusic() {
		_player?.Stop();
	}

	/// <summary>
	/// Starts playing the background music.
	/// Assigns the music stream to the player and begins playback with looping enabled.
	/// Logs a debug message when music playback starts.
	/// Only starts playback if music is not already playing.
	/// </summary>
	public void PlayMusic() {
		GD.Print("GameMusicPlayer PlayMusic");
		if (_player != null && MusicStream != null && !_player.Playing) {
			_player.Stream = MusicStream;
			_player.Play();
		}
	}

	/// <summary>
	/// Checks if music is currently playing.
	/// </summary>
	/// <returns>True if music is playing, false otherwise</returns>
	public bool IsPlaying() {
		return _player?.Playing ?? false;
	}

	/// <summary>
	/// Pauses the currently playing background music.
	/// The music can be resumed later without restarting from the beginning.
	/// </summary>
	public void PauseMusic() {
		if (_player != null && _player.Playing) {
			_player.StreamPaused = true;
		}
	}

	/// <summary>
	/// Resumes the previously paused background music.
	/// The music will continue from where it was paused.
	/// </summary>
	public void ResumeMusic() {
		if (_player != null && _player.StreamPaused) {
			_player.StreamPaused = false;
		}
	}
}
