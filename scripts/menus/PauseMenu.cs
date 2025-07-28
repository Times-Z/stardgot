using Godot;

public partial class PauseMenu : Control
{
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            // Remove the CanvasLayer parent instead of just the PauseMenu
            var canvasLayer = GetParent();
            if (canvasLayer != null && canvasLayer.GetType().Name == "CanvasLayer")
            {
                canvasLayer.QueueFree();
            }
            else
            {
                QueueFree();
            }
            GetTree().Paused = false;
        }
    }

    public override void _Ready()
    {
        GD.Print("PauseMenu _Ready");
        ProcessMode = ProcessModeEnum.Always;
        SetProcessInput(true);
    }
}
