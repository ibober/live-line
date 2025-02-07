using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inheritants of the class provide navigation system with walkable areas and impassable objects.
/// </summary>
public abstract class NavigationSite : MonoBehaviour
{
    private bool isAnalysed;
    private bool isAnalysing; // TODO Makes sense once analysis is implemented with Jobs.

    [Tooltip("Don't trigger OnAnalysed delegates. Check if you plan to trigger further path calculations manually, e.g. in Editor mode.")]
    public bool ignoreSiteAnalysisListeners;

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
            if (isAnalysed && !ignoreSiteAnalysisListeners)
            {
                OnAnalysed?.Invoke();
            }
        }
    }

    /// <summary>
    /// Call this method in inheritor class whenever you want to collect <see cref="Floors"/> and <see cref="Obstacles"/>.
    /// </summary>
    protected void Analyse()
    {
        if (isAnalysing)
        {
            return;
        }

        isAnalysing = true;
        IsAnalysed = false;
        try
        {
            Floors = GetFloors() ?? new List<GameObject>(0);
            Obstacles = GetObstacles() ?? new List<GameObject>(0);
            IsAnalysed = true;
        }
        catch (Exception x)
        {
            Debug.Log(x.ToString());
        }
        finally
        {
            isAnalysing = false;
        }
    }

    /// <summary>
    /// Manually trigger analysis. When baking NavMesh in Editor, e.g.
    /// </summary>
    public virtual void TriggerSiteAnalysis()
    {
        Debug.Log("\nCollecting walkable areas and impassable objects on the site...");
        Analyse();
        Debug.Log($"\n{Floors.Count} floor{Floors.DecideEnding()} and {Obstacles.Count} obstacle{Obstacles.DecideEnding()} found.");
    }

    // TODO Validate floors and obstacles adding colliders or checking sizes.
}
