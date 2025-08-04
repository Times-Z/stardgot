using Godot;

/// <summary>
/// Singleton music manager for menu navigation.
/// Ensures menu music continues playing across different menu scenes.
/// </summary>
public partial class MenuMusicManager : Node
{
    /// <summary>
    /// Singleton instance of the menu music manager.
    /// </summary>
    public static MenuMusicManager Instance { get; private set; }

    /// <summary>
    /// The music player component that handles menu audio.
    /// </summary>
    private MusicPlayerComponent _musicPlayer;

    /// <summary>
    /// Tracks if we're currently in game context (true) or menu context (false).
    /// When in game context, menu music should not play automatically.
    /// </summary>
    private bool _isInGameContext = false;

    private const string MenuMusicPath = "res://assets/audio/background/03.ogg";

    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// Initializes the singleton and creates the music player.
    /// </summary>
    public override void _Ready() {
        GD.Print("MenuMusicManager _Ready");

        Instance = this;

        _musicPlayer = new MusicPlayerComponent();
        AddChild(_musicPlayer);

        _musicPlayer.MusicStream = GD.Load<AudioStream>(MenuMusicPath);
        _musicPlayer.AutoPlay = true;
        _musicPlayer.PlayerName = "MenuMusicManager";

        if (ConfigurationManager.Instance != null) {
            var volumeDb = ConfigurationManager.MasterVolume <= 0.0f ? -80.0f : Mathf.LinearToDb(ConfigurationManager.MasterVolume);
            _musicPlayer.SetVolume(volumeDb);
        }

        GD.Print("MenuMusicManager initialized");
    }

    /// <summary>
    /// Starts playing the menu music.
    /// Safe to call multiple times - won't restart if already playing.
    /// Only plays if not in game context.
    /// </summary>
    public void PlayMenuMusic()
    {
        if (_isInGameContext)
        {
            GD.Print("MenuMusicManager: Skipping menu music - in game context");
            return;
        }

        if (_musicPlayer != null && !_musicPlayer.IsPlaying())
        {
            _musicPlayer.PlayMusic();
            GD.Print("MenuMusicManager: Started menu music");
        }
    }

    /// <summary>
    /// Stops the menu music and sets game context.
    /// Called when transitioning to game scenes.
    /// </summary>
    public void StopMenuMusic()
    {
        _isInGameContext = true;
        if (_musicPlayer != null && _musicPlayer.IsPlaying())
        {
            _musicPlayer.StopMusic();
            GD.Print("MenuMusicManager: Stopped menu music - entering game context");
        }
    }

    /// <summary>
    /// Resumes menu context and starts menu music.
    /// Called when returning to menu from game.
    /// </summary>
    public void ResumeMenuContext()
    {
        _isInGameContext = false;
        PlayMenuMusic();
        GD.Print("MenuMusicManager: Resumed menu context and music");
    }

    /// <summary>
    /// Pauses the menu music.
    /// </summary>
    public void PauseMenuMusic()
    {
        _musicPlayer?.PauseMusic();
        GD.Print("MenuMusicManager: Paused menu music");
    }

    /// <summary>
    /// Resumes the menu music.
    /// </summary>
    public void ResumeMenuMusic()
    {
        _musicPlayer?.ResumeMusic();
        GD.Print("MenuMusicManager: Resumed menu music");
    }

    /// <summary>
    /// Sets the volume of the menu music.
    /// </summary>
    /// <param name="volumeDb">Volume in decibels</param>
    public void SetVolume(float volumeDb)
    {
        _musicPlayer?.SetVolume(volumeDb);
    }

    /// <summary>
    /// Checks if menu music is currently playing.
    /// </summary>
    /// <returns>True if playing, false otherwise</returns>
    public bool IsPlaying()
    {
        return _musicPlayer?.IsPlaying() ?? false;
    }

    /// <summary>
    /// Checks if we're currently in game context.
    /// </summary>
    /// <returns>True if in game context, false if in menu context</returns>
    public bool IsInGameContext()
    {
        return _isInGameContext;
    }
}
