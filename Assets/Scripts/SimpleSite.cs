using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Another simple example of class which provides <see cref="PathFinder"/> with objects to navigate.
/// </summary>
public class SimpleSite : NavigationSite
{
    [Tooltip("Manually set walkable areas.")]
    public List<GameObject> floors;

    [Tooltip("Manually set impassable objects.")]
    public List<GameObject> obstacles;

    // In both overrides return manually set game objects.
    protected override List<GameObject> GetFloors() => floors;

    protected override List<GameObject> GetObstacles() => obstacles;

    private void Start()
    {
        // We still need to call this method to set IsActive.
        Analyse();
    }
}
