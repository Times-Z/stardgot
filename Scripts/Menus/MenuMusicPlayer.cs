using Godot;

public partial class MenuMusicPlayer : Node
{
	private AudioStreamPlayer _player;
	[Export] public AudioStream MusicStream;

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

	public void StopMusic()
	{
		_player?.Stop();
	}

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
