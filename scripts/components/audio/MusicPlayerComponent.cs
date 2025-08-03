using Godot;

/// <summary>
/// Reusable music player component that can be configured for different contexts (menu, game, etc.).
/// Provides standardized music playback functionality with configurable autoplay and volume settings.
/// </summary>
public partial class MusicPlayerComponent : Node {
	/// <summary>
	/// The audio stream resource containing the background music.
	/// Should be assigned in the Godot editor or via code.
	/// </summary>
	[Export] public AudioStream MusicStream { get; set; }

	/// <summary>
	/// Whether the music should start playing automatically when the node is ready.
	/// </summary>
	[Export] public bool AutoPlay { get; set; } = false;

	/// <summary>
	/// The volume level in decibels. Default is -10 dB for comfortable listening.
	/// </summary>
	[Export] public float VolumeDb { get; set; } = -10.0f;

	/// <summary>
	/// Whether the music should loop continuously.
	/// </summary>
	[Export] public bool Loop { get; set; } = true;

	/// <summary>
	/// Debug name for this music player instance (helps with logging).
	/// </summary>
	[Export] public string PlayerName { get; set; } = "MusicPlayer";

	/// <summary>
	/// The audio stream player component that handles actual audio playback.
	/// </summary>
	private AudioStreamPlayer _player;

	/// <summary>
	/// Called when the node enters the scene tree for the first time.
	/// Creates an audio stream player and configures it based on export properties.
	/// </summary>
	public override void _Ready() {
		GD.Print($"{PlayerName} _Ready");
		
		_player = new AudioStreamPlayer();
		AddChild(_player);
		
		ConfigurePlayer();
		
		if (AutoPlay && MusicStream != null) {
			PlayMusic();
		}
	}

	/// <summary>
	/// Configures the audio player with the current settings.
	/// </summary>
	private void ConfigurePlayer() {
		if (_player == null) return;

		_player.VolumeDb = VolumeDb;
		
		if (MusicStream != null) {
			_player.Stream = MusicStream;
		}
	}

	/// <summary>
	/// Starts playing the background music.
	/// Only starts playback if music is not already playing.
	/// </summary>
	public void PlayMusic() {
		GD.Print($"{PlayerName} PlayMusic");
		if (_player != null && MusicStream != null && !_player.Playing) {
			_player.Stream = MusicStream;
			_player.Play();
		}
	}

	/// <summary>
	/// Stops the currently playing background music.
	/// </summary>
	public void StopMusic() {
		_player?.Stop();
		GD.Print($"{PlayerName} StopMusic");
	}

	/// <summary>
	/// Pauses the currently playing background music.
	/// The music can be resumed later without restarting from the beginning.
	/// </summary>
	public void PauseMusic() {
		if (_player != null && _player.Playing) {
			_player.StreamPaused = true;
			GD.Print($"{PlayerName} PauseMusic");
		}
	}

	/// <summary>
	/// Resumes the previously paused background music.
	/// The music will continue from where it was paused.
	/// </summary>
	public void ResumeMusic() {
		if (_player != null && _player.StreamPaused) {
			_player.StreamPaused = false;
			GD.Print($"{PlayerName} ResumeMusic");
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
	/// Checks if music is currently paused.
	/// </summary>
	/// <returns>True if music is paused, false otherwise</returns>
	public bool IsPaused() {
		return _player?.StreamPaused ?? false;
	}

	/// <summary>
	/// Updates the music stream at runtime.
	/// </summary>
	/// <param name="newStream">The new audio stream to use</param>
	/// <param name="playImmediately">Whether to start playing the new stream immediately</param>
	public void SetMusicStream(AudioStream newStream, bool playImmediately = false) {
		MusicStream = newStream;
		
		if (_player != null) {
			bool wasPlaying = _player.Playing;
			_player.Stop();
			_player.Stream = newStream;
			
			if (playImmediately || wasPlaying) {
				_player.Play();
			}
		}
	}

	/// <summary>
	/// Updates the volume at runtime.
	/// </summary>
	/// <param name="volumeDb">New volume in decibels</param>
	public void SetVolume(float volumeDb) {
		VolumeDb = volumeDb;
		if (_player != null) {
			_player.VolumeDb = volumeDb;
		}
	}

	/// <summary>
	/// Gets the current playback position in seconds.
	/// </summary>
	/// <returns>Current position in seconds</returns>
	public float GetPlaybackPosition() {
		return _player?.GetPlaybackPosition() ?? 0.0f;
	}

	/// <summary>
	/// Clean up when the node is about to be removed.
	/// </summary>
	public override void _ExitTree() {
		StopMusic();
		base._ExitTree();
	}
}
