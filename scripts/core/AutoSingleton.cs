using Godot;

/// <summary>
/// Base class for automatic singleton management.
/// Inherit from this class to automatically handle singleton pattern
/// without manual initialization in _Ready().
/// </summary>
/// <typeparam name="T">The type of the singleton</typeparam>
public abstract partial class AutoSingleton<T> : Control where T : AutoSingleton<T>
{
    /// <summary>
    /// Singleton instance
    /// </summary>
    public static T Instance { get; private set; }

    /// <summary>
    /// Automatically initializes the singleton when the node enters the tree
    /// </summary>
    public override void _EnterTree()
    {
        if (Instance == null)
        {
            Instance = (T)this;
            GD.Print($"{typeof(T).Name}: Singleton initialized");
        }
        else
        {
            GD.PrintErr($"{typeof(T).Name}: Multiple instances detected! This should not happen.");
        }

        base._EnterTree();
    }

    /// <summary>
    /// Automatically cleans up the singleton when the node exits the tree
    /// </summary>
    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
            GD.Print($"{typeof(T).Name}: Singleton cleaned up");
        }

        base._ExitTree();
    }
}
