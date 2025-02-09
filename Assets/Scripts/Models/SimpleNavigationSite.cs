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
    protected override List<GameObject> CollectFloors() => floors;

    /// <inheritdoc/>
    protected override List<GameObject> CollectObstacles() => obstacles;

    /// <inheritdoc/>
    protected override List<GameObject> CollectPassages() => doors;

    private void Start()
    {
        Analyse();
    }
}
