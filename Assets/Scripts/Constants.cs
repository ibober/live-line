using Unity.AI.Navigation;

public struct Constants
{
    /// <summary>
    /// Basically, the maximum distance from AR camera to the floor
    /// (from current <see cref="PathFinder"/> position to the <see cref="NavMeshSurface"/>)
    /// </summary>
    public const float MaxDistanceToNavSerface = 2.1f;

    /// <summary>
    /// Width of the path in meters.
    /// </summary>
    public const float PathWidth = 0.3f;
}