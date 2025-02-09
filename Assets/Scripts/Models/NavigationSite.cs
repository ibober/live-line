using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Inheritants of the class provide navigation system with walkable areas and impassable objects.
/// </summary>
public abstract class NavigationSite : MonoBehaviour
{
    private bool isAnalysed;
    private bool isAnalysing; // TODO Makes sense once analysis is implemented async way with Jobs or Coroutines.

    // TODO Decide if this property is needed.
    [Tooltip("Don't trigger OnAnalysed delegates. Check if you plan to trigger further path calculations manually.")]
    public bool ignoreSiteAnalysisListeners;

    [Tooltip("Scriptable object which caches generated navigation data.")]
    public NavDataCacher navDataCacher;

    /// <summary>
    /// Name of the navigation site.
    /// </summary>
    public virtual string Name => gameObject.name;

    /// <summary>
    /// Override to collect all walkable areas.
    /// <see cref="PathFinder"/> will utilize all GOs with <see cref="MeshFilter"/> and <see cref="Renderer"/> components.
    /// </summary>
    protected abstract List<GameObject> GetFloors();

    /// <summary>
    /// Override to collect all impassable objects.
    /// <see cref="PathFinder"/> will utilize all GOs with <see cref="MeshFilter"/> and <see cref="Renderer"/> components.
    /// </summary>
    protected abstract List<GameObject> GetObstacles();

    /// <summary>
    /// Override to collect walk-through objects, like doors.
    /// <see cref="PathFinder"/> will utilize all GOs with <see cref="MeshFilter"/> and <see cref="Renderer"/> components.
    /// </summary>
    /// <returns></returns>
    protected abstract List<GameObject> GetPassages();

    /// <summary>
    /// Walkable areas on the site.
    /// </summary>
    public List<GameObject> Floors { get; private set; }

    /// <summary>
    /// Impassable objects.
    /// </summary>
    public List<GameObject> Obstacles { get; private set; }

    /// <summary>
    /// Walk-through objects.
    /// </summary>
    public List<GameObject> Passages { get; private set; }

    /// <summary>
    /// <see cref="NavMeshBuildSource"/>s collection to build <see cref="NavMesh"/>.
    /// </summary>
    public List<NavMeshBuildSource> Elements { get; private set; }

    /// <summary>
    /// <see cref="Bounds"/> which wraps all <see cref="Elements"/> to build <see cref="NavMesh"/>.
    /// </summary>
    public Bounds Bounds { get; private set; }

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
    /// Call this method in inheritor class whenever you want to collect <see cref="Floors"/>, <see cref="Obstacles"/> and <see cref="Passages"/>.
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
            Passages = GetPassages() ?? new List<GameObject>(0);

            Elements = new List<NavMeshBuildSource>();
            Bounds = new Bounds();
            foreach (var floor in Floors)
            {
                var meshFilters = floor.GetComponentsInChildren<MeshFilter>();
                foreach (var meshFilter in meshFilters)
                {
                    // Expand bounds.
                    if (!meshFilter.gameObject.TryGetComponent<Renderer>(out var renderer))
                    {
                        // Ignore invisible objects.
                        continue;
                    }
                    Bounds.Encapsulate(renderer.bounds);

                    // Add to sources.
                    var source = new NavMeshBuildSource
                    {
                        shape = NavMeshBuildSourceShape.Mesh, // TODO Consider limitation in 100K units in size.
                        sourceObject = meshFilter.sharedMesh,
                        transform = floor.transform.localToWorldMatrix,
                        size = renderer.bounds.size,
                        area = 0, // Built-in Walkable Navigation Area
                    };
                    Elements.Add(source);
                }
            }

            foreach (var obstacle in Obstacles)
            {
                var meshFilters = obstacle.GetComponentsInChildren<MeshFilter>();
                foreach (var meshFilter in meshFilters)
                {
                    // Expand bounds.
                    if (!meshFilter.gameObject.TryGetComponent<Renderer>(out var renderer))
                    {
                        // Ignore invisible objects.
                        continue;
                    }
                    Bounds.Encapsulate(renderer.bounds);

                    var source = new NavMeshBuildSource
                    {
                        shape = NavMeshBuildSourceShape.Mesh, // Walls with holes need to have Mesh shape, Box is enough for columns and simpler objects.
                        sourceObject = meshFilter.sharedMesh,
                        transform = obstacle.transform.localToWorldMatrix,
                        size = renderer.bounds.size,
                        area = 1, // Built-in Non Walkable Navigation Area
                    };
                    Elements.Add(source);
                }
            }

            //foreach (var pass in Passages)
            //{
            //    var navModifier = pass.GetOrAddComponent<NavMeshModifier>();
            //    navModifier.applyToChildren = true;
            //    navModifier.overrideArea = true;
            //    navModifier.area = 0;

            //    //var meshFilter = pass.GetComponent<MeshFilter>();
            //    //var source = new NavMeshBuildSource
            //    //{
            //    //    shape = NavMeshBuildSourceShape.Mesh, // TODO Consider limitation in 100K units in size.
            //    //    sourceObject = meshFilter.sharedMesh,
            //    //    transform = pass.transform.localToWorldMatrix,
            //    //    area = 0, // Built-in Non Walkable Navigation Area
            //    //};
            //    //Elements.Add(source);
            //}

            IsAnalysed = true;
        }
        catch (Exception x)
        {
            Debug.LogError(x.ToString());
        }
        finally
        {
            isAnalysing = false;
        }
    }

    /// <summary>
    /// Manually triggers analysis. When baking NavMesh in Editor, e.g.
    /// </summary>
    public virtual bool TriggerSiteAnalysis()
    {
        Debug.Log("\nCollecting walkable areas and impassable objects on the site...");
        Analyse();
        Debug.Log($"\n" +
            $"{Floors.Count} floor{Floors.DecideEnding()}, " +
            $"{Obstacles.Count} obstacle{Obstacles.DecideEnding()} and " +
            $"{Passages.Count} passage{Passages.DecideEnding()} found.");
        return isAnalysed;
    }
}
