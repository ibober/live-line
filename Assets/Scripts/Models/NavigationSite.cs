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
    private bool isBusy;

    private List<NavMeshBuildSource> elements;
    private Bounds bounds;

    [Tooltip("Man's height with a safety hat (when he's crawling under obstacles in the worst case).")]
    [Range(1.2f, 2.1f)]
    public float minimumPassageHeight = 1.7f;

    [Tooltip("Man's shaulders width (when he's crab-walking through tiny space in the worst case)." +
        "\nThis value should not be big otherwise door holes will become impassable." +
        "\nIMO it affects precision more notisably than setting voxel size.")]
    [Range(0, 1.5f)]
    public float minimumPassageWidth = 0.5f;

    [Tooltip("Don't trigger OnAnalysed delegates, don't draw path immediately after NavMesh is baked.")]
    public bool ignoreSiteAnalysisListeners;

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
    /// Removes all <see cref="NavMeshData"/> from the scene.
    /// </summary>
    public void RemoveAllNavMeshData()
    {
        NavMesh.RemoveAllNavMeshData();
        IsAnalysed = false;
    }

    /// <summary>
    /// Collects <see cref="Floors"/>, <see cref="Obstacles"/> and <see cref="Passages"/> and bakes NavMeshData.
    /// <para>As a result <see cref="NavMeshSurface"/> is created and path gets ready to be calculated.</para>
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

                    // Add to sources.
                    var source = new NavMeshBuildSource
                    {
                        shape = NavMeshBuildSourceShape.Mesh, // TODO Walls with holes need to have Mesh shape, Box would be enough for columns and simpler objects.
                        sourceObject = meshFilter.sharedMesh,
                        transform = obstacle.transform.localToWorldMatrix,
                        size = renderer.bounds.size,
                        area = 1, // Built-in Non Walkable Navigation Area
                    };
                    elements.Add(source);
                }
            }

            // TODO In case doors don't cut holes in walls it might be a good idea to create NavMeshLinks.
            //foreach (var pass in Passages)
            //{
            //    //var navModifier = pass.GetOrAddComponent<NavMeshModifier>();
            //    //navModifier.applyToChildren = true;
            //    //navModifier.overrideArea = true;
            //    //navModifier.area = 0;
            //    var link = new NavMeshLinkData
            //    {
            //        bidirectional = true,
            //        width = Constants.PathWidth,
            //        area = 0,
            //    };
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

        // NOTE Playing with values below gives different paths.
        // The smaller the voxel size the better the precision.
        // But it's hard to guess and usually Unity is seems to be good enough to set the values automatically.
        //navSettings.overrideVoxelSize = true;
        //navSettings.voxelSize = minimumPassageWidth / 2;
        //navSettings.overrideTileSize = true;
        //navSettings.tileSize = 1;

        var navData = NavMeshBuilder.BuildNavMeshData(
                navSettings,
                this.elements,
                this.bounds,
                Vector3.zero,
                Quaternion.identity);

        NavMesh.RemoveAllNavMeshData(); // If we don't remove, Unity adds more NavMeshSurfaces to the scene.
        _ = NavMesh.AddNavMeshData(navData);
    }
}
