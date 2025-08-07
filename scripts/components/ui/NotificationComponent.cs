using Godot;

partial class NotificationComponent : Control {

	public static NotificationComponent Instance { get; private set; }

	/// <summary>
	/// The RichTextLabel node used to display notifications to the player.
	/// This label will show messages like "You have received a new item!".
	/// </summary>
	private RichTextLabel _notificationLabel;

	/// <summary>
	/// The AnimationPlayer node used to handle animations for the notification label.
	/// This can be used to animate the appearance and disappearance of notifications.
	/// </summary>
	private AnimationPlayer _animPlayer;

	/// <summary>
	/// Called when the node is added to the scene. Initializes the singleton instance of the NotificationComponent.
	/// If an instance already exists, the current node is freed. Otherwise, sets up the notification label and hides it.
	/// </summary>
	public override void _Ready() {
		if (Instance == null) {
			Instance = this;
			_notificationLabel = GetNode<RichTextLabel>("NotificationLabel");
			_animPlayer = GetNode<AnimationPlayer>("NotificationAnimation");
			_notificationLabel.Visible = false;
			_notificationLabel.Position = new Vector2(0, -100);
		}
		else {
			QueueFree();
		}
	}

	/// <summary>
	/// Displays a notification message to the player.
	/// The message will be shown in the notification label and will automatically hide after a delay.
	/// </summary>
	/// <param name="message">The message to display in the notification label.</param>
	public void ShowNotify(string message) {
		_notificationLabel.Text = message;
		_animPlayer.Play("ShowNotify");
		_notificationLabel.Visible = true;

		CallDeferred(nameof(Instance.HideNotification), 3.0f);
	}

	/// <summary>
	/// Hides the notification label after a specified delay.
	/// This method uses a Timer to wait before clearing the notification text and hiding the label.
	/// </summary>
	public void HideNotification(float delay = 1.0f) {
		if (delay <= 0) {
			GD.PushError("NotificationComponent.HideNotification: Delay must be greater than 0.");
			return;
		}

		var timer = new Timer();
		timer.WaitTime = delay;
		timer.OneShot = true;
		timer.Timeout += () => {
			_animPlayer.Play("HideNotification");
		};

		AddChild(timer);
		timer.Start();

		return;
	}
}
