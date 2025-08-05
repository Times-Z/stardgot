using Godot;

/// <summary>
/// Simple animation controller component for AnimatedSprite2D.
/// Provides basic animation playback functionality.
/// </summary>
public partial class AnimationControllerComponent : Node {
	/// <summary>
	/// Path to the AnimatedSprite2D node.
	/// </summary>
	[Export] public NodePath AnimatedSpritePath { get; set; }

	/// <summary>
	/// Default animation to play when the controller starts.
	/// </summary>
	[Export] public string DefaultAnimation { get; set; } = "idle";

	/// <summary>
	/// Whether to play the default animation automatically on ready.
	/// </summary>
	[Export] public bool AutoPlayDefault { get; set; } = true;

	/// <summary>
	/// Debug name for logging purposes.
	/// </summary>
	[Export] public string ComponentName { get; set; } = "AnimationController";

	/// <summary>
	/// Reference to the AnimatedSprite2D.
	/// </summary>
	private AnimatedSprite2D _animatedSprite;

	/// <summary>
	/// Called when the node enters the scene tree.
	/// Finds and configures the AnimatedSprite2D node.
	/// </summary>
	public override void _Ready() {
        GD.Print($"{ComponentName} _Ready");
		SetupAnimatedSprite();

		if (AutoPlayDefault && !string.IsNullOrEmpty(DefaultAnimation)) {
			PlayAnimation(DefaultAnimation);
		}
	}

	/// <summary>
	/// Sets up the AnimatedSprite2D reference.
	/// </summary>
	private void SetupAnimatedSprite() {
		if (!AnimatedSpritePath.IsEmpty) {
			_animatedSprite = GetNode<AnimatedSprite2D>(AnimatedSpritePath);
		} else {
			_animatedSprite = GetParent()?.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
			_animatedSprite ??= GetParent()?.FindChild("*", recursive: true, owned: false) as AnimatedSprite2D;
		}

		if (_animatedSprite == null) {
			GD.PrintErr($"{ComponentName}: Could not find AnimatedSprite2D");
		} else {
			GD.Print($"{ComponentName}: Found AnimatedSprite2D at {_animatedSprite.GetPath()}");
		}
	}

	/// <summary>
	/// Plays the specified animation.
	/// </summary>
	/// <param name="animationName">Name of the animation to play</param>
	/// <param name="force">Whether to restart the animation if it's already playing</param>
	/// <returns>True if the animation was successfully started</returns>
	public virtual bool PlayAnimation(string animationName, bool force = false) {
		if (string.IsNullOrEmpty(animationName)) {
			GD.PrintErr($"{ComponentName}: Animation name cannot be empty");
			return false;
		}

		bool success = PlayAnimatedSpriteAnimation(animationName);

		// if (success) {
		// 	_currentAnimation = animationName;
		// 	GD.Print($"{ComponentName}: Playing animation '{animationName}'");
		// } else {
		// 	GD.PrintErr($"{ComponentName}: Failed to play animation '{animationName}'");
		// }

		return success;
	}

	/// <summary>
	/// Plays an animation using AnimatedSprite2D.
	/// </summary>
	/// <param name="animationName">Animation name</param>
	/// <returns>True if successful</returns>
	private bool PlayAnimatedSpriteAnimation(string animationName) {
		if (_animatedSprite == null) return false;

		if (_animatedSprite.SpriteFrames == null) {
			GD.PrintErr($"{ComponentName}: AnimatedSprite2D has no SpriteFrames resource");
			return false;
		}

		if (!_animatedSprite.SpriteFrames.HasAnimation(animationName)) {
			GD.PrintErr($"{ComponentName}: Animation '{animationName}' not found in SpriteFrames");
			return false;
		}

		_animatedSprite.Play(animationName);
		return true;
	}
}
