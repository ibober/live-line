using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Another example of class which provides <see cref="PathFinder"/> with objects to navigate.
/// </summary>
public class SimpleNavigationSite : NavigationSite
{
    [Tooltip("Set walkable areas.")]
    public List<GameObject> floors;

    [Tooltip("Set impassable objects.")]
    public List<GameObject> obstacles;

    [Tooltip("Set walk-through object.")]
    public List<GameObject> doors;

    /// <inheritdoc/>
    protected override List<GameObject> GetFloors() => floors;

    /// <inheritdoc/>
    protected override List<GameObject> GetObstacles() => obstacles;

    /// <inheritdoc/>
    protected override List<GameObject> GetPassages() => doors;

    private void Start()
    {
        Analyse();
    }
}
