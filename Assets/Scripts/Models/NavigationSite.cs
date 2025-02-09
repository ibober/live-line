using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Inheritants of the class provide navigation system with walkable areas and impassable objects.
/// </summary>
public abstract class NavigationSite : MonoBehaviour
{
    private bool isAnalysed;
    private bool isBusy; // TODO Makes sense once analysis is implemented async way with Jobs or Coroutines.

    private List<NavMeshBuildSource> elements;
    private Bounds bounds;

    [Space]

    // TODO Decide if this property is needed.
    [Tooltip("Don't trigger OnAnalysed delegates. Check if you plan to trigger further path calculations manually.")]
    public bool ignoreSiteAnalysisListeners;

    [Tooltip("Scriptable object which caches generated navigation data.")]
    public NavDataCacher navDataCacher;

    [Tooltip("Man height with a safety hat (when he's crawling under obstacles in the worst case).")]
    [Range(1.5f, 2.1f)]
    public float minimumPassageHeight = 1.7f;

    [Tooltip("Man shaulders width (when he's crab-walking through tiny space).")]
    [Range(0.5f, 1.5f)]
    public float minimumPassageWidth = 0.7f;

    /// <summary>
    /// Name of the navigation site.
    /// </summary>
    public virtual string Name => gameObject.name;

    /// <summary>
    /// Override to collect all walkable areas.
    /// <see cref="PathFinder"/> will utilize all GOs with <see cref="MeshFilter"/> and <see cref="Renderer"/> components.
    /// </summary>
    protected abstract List<GameObject> CollectFloors();

    /// <summary>
    /// Override to collect all impassable objects.
    /// <see cref="PathFinder"/> will utilize all GOs with <see cref="MeshFilter"/> and <see cref="Renderer"/> components.
    /// </summary>
    protected abstract List<GameObject> CollectObstacles();

    /// <summary>
    /// Override to collect walk-through objects, like doors.
    /// <see cref="PathFinder"/> will utilize all GOs with <see cref="MeshFilter"/> and <see cref="Renderer"/> components.
    /// </summary>
    /// <returns></returns>
    protected abstract List<GameObject> CollectPassages();

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
    /// The event is called each time <see cref="NavMeshData"/> is updated.
    /// Listen to update path.
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
    /// Collects <see cref="Floors"/>, <see cref="Obstacles"/> and <see cref="Passages"/> and caches baked NavMeshData.
    /// </summary>
    public void Analyse()
    {
        if (isBusy)
        {
            return;
        }

        isBusy = true;
        IsAnalysed = false;
        try
        {
            Floors = CollectFloors() ?? new List<GameObject>(0);
            Obstacles = CollectObstacles() ?? new List<GameObject>(0);
            Passages = CollectPassages() ?? new List<GameObject>(0);

            elements = new List<NavMeshBuildSource>();
            bounds = new Bounds();
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
                    bounds.Encapsulate(renderer.bounds);

                    // Add to sources.
                    var source = new NavMeshBuildSource
                    {
                        shape = NavMeshBuildSourceShape.Mesh, // TODO Consider limitation in 100K units in size.
                        sourceObject = meshFilter.sharedMesh,
                        transform = floor.transform.localToWorldMatrix,
                        size = renderer.bounds.size,
                        area = 0, // Built-in Walkable Navigation Area
                    };
                    elements.Add(source);
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
                    bounds.Encapsulate(renderer.bounds);

                    var source = new NavMeshBuildSource
                    {
                        shape = NavMeshBuildSourceShape.Mesh, // Walls with holes need to have Mesh shape, Box is enough for columns and simpler objects.
                        sourceObject = meshFilter.sharedMesh,
                        transform = obstacle.transform.localToWorldMatrix,
                        size = renderer.bounds.size,
                        area = 1, // Built-in Non Walkable Navigation Area
                    };
                    elements.Add(source);
                }
            }

            // TODO We could determine minimumPassageHeight and minimumPassageWidth from bounds of the objects below.
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

            //    //var link = new NavMeshLinkData
            //    //{
            //    //    bidirectional = true,
            //    //    width =
            //    //area = 0,
            //    //}
            //}

            BakeNavMesh();

            IsAnalysed = true;
        }
        catch (Exception x)
        {
            Debug.LogError(x.ToString());
        }
        finally
        {
            isBusy = false;
        }
    }

    private void BakeNavMesh()
    {
        var navSettings = NavMesh.GetSettingsCount() == 0
            ? NavMesh.CreateSettings()
            : NavMesh.GetSettingsByIndex(0);
        navSettings.agentHeight = minimumPassageHeight;
        navSettings.agentRadius = minimumPassageWidth / 2;
        //navSettings.overrideVoxelSize = true;
        //navSettings.voxelSize = minimumPassageWidth / 2;
        //navSettings.overrideTileSize = true;
        //navSettings.tileSize = 1;

        // NOTE We have to make minRegionArea small enough so agent could pass door openings in walls.
        // Given that minimum valid door width = 700mm,
        // taking worker (agentWidth) as per input (e.g. 0.7),
        // there is no room left to go through the opening: 700m - 2 * 0.7 / 2 = 0.
        // Even if door is 1m wide, we have only 300mm to pass through, so the minRegionArea has to be twice smaller in that case - 0.15.
        if (!navDataCacher.TryGet(this.Name, out var navData))
        {
            navData = NavMeshBuilder.BuildNavMeshData(
                navSettings,
                this.elements,
                this.bounds,
                Vector3.zero,
                Quaternion.identity);

            navDataCacher.Set(this.Name, navData);
        }
        else
        {
            var updated = NavMeshBuilder.UpdateNavMeshData(
                navData,
                navSettings,
                this.elements,
                this.bounds);

            // TODO What if NavMeshData wasn't updated?
        }

        navData.hideFlags = HideFlags.None;
        
        // TODO If we add each time, how many instances are there?
        //NavMesh.RemoveAllNavMeshData();
        
        var navDataInstance = NavMesh.AddNavMeshData(navData); // TODO Discard instance.
    }

    public void RemoveAllNavMeshData()
    {
        if (TryGetComponent<LineRenderer>(out var lineRenderer))
        {
#if UNITY_EDITOR
            DestroyImmediate(lineRenderer);
#else
            Destroy(lineRenderer);
#endif
        }

        NavMesh.RemoveAllNavMeshData();
        IsAnalysed = false;
    }
}
