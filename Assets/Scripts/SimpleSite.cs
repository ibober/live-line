using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Another simple example of class which provides <see cref="PathFinder"/> with objects to navigate.
/// </summary>
public class SimpleSite : NavigationSite
{
    [Tooltip("Set walkable areas.")]
    public List<GameObject> floors;

    [Tooltip("Set impassable objects.")]
    public List<GameObject> obstacles;

    /// <inheritdoc/>
    protected override List<GameObject> GetFloors() => floors;

    /// <inheritdoc/>
    protected override List<GameObject> GetObstacles() => obstacles;

    private void Start()
    {
        Analyse();
    }
}
