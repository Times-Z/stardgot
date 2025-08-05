using Godot;

/// <summary>
/// Component that automatically configures a SubViewport for pixel-perfect rendering.
/// This can be attached to any SubViewport to apply pixel art settings automatically.
/// </summary>
[GlobalClass]
public partial class PixelPerfectViewportConfigurator : Node
{
    [Export] public bool ConfigureOnReady { get; set; } = true;
    [Export] public bool SnapTransformsToPixel { get; set; } = true;
    [Export] public bool SnapVerticesToPixel { get; set; } = true;
    [Export] public bool DisablePhysicsInterpolation { get; set; } = true;
    [Export] public Viewport.DefaultCanvasItemTextureFilter TextureFilter { get; set; } = Viewport.DefaultCanvasItemTextureFilter.Nearest;
    [Export] public SubViewport.UpdateMode RenderUpdateMode { get; set; } = SubViewport.UpdateMode.Always;

    public override void _Ready()
    {
        if (ConfigureOnReady)
        {
            ConfigureViewport();
        }
    }

    public void ConfigureViewport()
    {
        var viewport = GetParent<SubViewport>();
        if (viewport == null)
        {
            GD.PrintErr("PixelPerfectViewportConfigurator: Parent must be a SubViewport!");
            return;
        }

        if (SnapTransformsToPixel)
        {
            viewport.Snap2DTransformsToPixel = true;
        }

        if (SnapVerticesToPixel)
        {
            viewport.Snap2DVerticesToPixel = true;
        }

        viewport.CanvasItemDefaultTextureFilter = TextureFilter;
        viewport.RenderTargetUpdateMode = RenderUpdateMode;
        viewport.PhysicsObjectPicking = true;

        if (DisablePhysicsInterpolation)
        {
            viewport.SetPhysicsInterpolationMode(Node.PhysicsInterpolationModeEnum.Off);
        }

        GD.Print("PixelPerfectViewportConfigurator: Viewport configured for pixel-perfect rendering");
    }
}
