using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inheritants of the class provide navigation system with walkable areas and impassable objects.
/// </summary>
public abstract class NavigationSite : MonoBehaviour
{
    private bool isAnalysed;

    /// <summary>
    /// Override to collect all walkable areas.
    /// </summary>
    protected abstract List<GameObject> GetFloors();

    /// <summary>
    /// Override to collect all impassable objects.
    /// </summary>
    protected abstract List<GameObject> GetObstacles();

    /// <summary>
    /// Walkable areas on the site.
    /// </summary>
    public List<GameObject> Floors { get; private set; }

    /// <summary>
    /// Impassable objects.
    /// </summary>
    public List<GameObject> Obstacles { get; private set; }

    /// <summary>
    /// The event is called when <see cref="Floors"/> and <see cref="Obstacles"/> properties both are ready to provide valid information.
    /// </summary>
    public Action OnAnalysed;

    /// <summary>
    /// If navigation site was analysed and <see cref="Floors"/> and <see cref="Obstacles"/> properties both provide valid information.
    /// </summary>
    public bool IsAnalysed
    {
        get => isAnalysed;
        set
        {
            isAnalysed = value;
            if (isAnalysed)
            {
                OnAnalysed?.Invoke();
            }
        }
    }

    /// <summary>
    /// Call this method whenever you want to collect <see cref="Floors"/> and <see cref="Obstacles"/>.
    /// </summary>
    protected void Analyse()
    {
        IsAnalysed = false;
        Floors = GetFloors() ?? new List<GameObject>(0);
        Obstacles = GetObstacles() ?? new List<GameObject>(0);
        IsAnalysed = true;
    }
}
