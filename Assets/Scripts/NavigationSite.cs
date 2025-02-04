using System;
using UnityEngine;

/// <summary>
/// Inheritants of the class provide navigation system with walkable areas and impassable objects.
/// </summary>
public abstract class NavigationSite : MonoBehaviour
{
    /// <summary>
    /// Walkable areas.
    /// </summary>
    /// <returns>Array of GameObjects.</returns>
    public abstract GameObject[] GetFloors();

    /// <summary>
    /// Impassable objects.
    /// </summary>
    /// <returns>Array of GameObjects.</returns>
    public abstract GameObject[] GetObstacles();

    /// <summary>
    /// The event is called when <see cref="GetFloors"/> and <see cref="GetObstacles"/> methods both are ready to provide valid information.
    /// </summary>
    protected Action OnAnalized;
}
