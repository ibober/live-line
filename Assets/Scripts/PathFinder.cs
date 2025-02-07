using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Class automagically finds and draws shortest path from the transformation it's assigned to to the destination point at the provided <see cref="NavigationSite"/>.
/// </summary>
public class PathFinder : MonoBehaviour
{
    private const float MasDistanceToMesh = 2f;

    private Transform endingPoint;
    private NavMeshData navData;
    private NavMeshBuildSettings navSettings;

    [Space]
    [Header("Evacuation site settings")]

    [Tooltip("Walkable area and impassable objects provider.")]
    public NavigationSite siteAnalyser;

    [Tooltip("Leave empty to take attached GO's position.")]
    public Transform startingPoint;

    [Tooltip("Alternative destination points. Closest one will be selected.")]
    public Transform[] destinations;

    [Space]
    [Header("Worker settings")]

    [Tooltip("Man height with a safety hat (when he's crawling under obstacles in the worst case).")]
    [Range(1.5f, 2.1f)]
    public float agentHeight = 1.7f;

    [Tooltip("Man shaulders width (when he's crab-walking through tiny space).")]
    [Range(0.5f, 1.5f)]
    public float agentWidth = 0.7f;

    private bool IsSubscribed => siteAnalyser.OnAnalysed?.GetInvocationList().Any(d => d.Method.Name == nameof(Bake)) ?? false;

    // TODO Force analysis.
    // TODO Extend for finding route.

    private void Start()
    {
        if (siteAnalyser.IsAnalysed)
        {
            CalculatePath();
        }
        else
        {
            siteAnalyser.OnAnalysed += CalculatePath;
        }
    }

    private void OnDestroy()
    {
        if (IsSubscribed)
        {
            siteAnalyser.OnAnalysed -= CalculatePath;
        }
    }

    private void CalculatePath()
    {
        CalculatePath(force: false);
    }

    /// <summary>
    /// Manually trigger NavMesh building.
    /// </summary>
    public void CalculatePath(bool force)
    {
        // Validate input.
        if (destinations.Length == 0)
        {
#if UNITY_EDITOR
            // TODO Ctrl+F all Debug.Log and replase with better logging system.
            Debug.Log($"Nowhere to go. No {nameof(destinations)} set.");
#endif
            return;
        }

        if (!siteAnalyser.IsAnalysed)
        {
            if (force)
            {
                siteAnalyser.TriggerSiteAnalysis();
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log($"Site is not analysed yet.");
#endif
                return;
            }
        }

        Prepare();

        Bake();

        DrawPath();
    }

    private void Prepare()
    {
        if ((startingPoint?.ToString() ?? "null") == "null") // Strangely it says startingPoint is not equal null if though it's not filled in Editor.
        {
            startingPoint = transform;
        }

        endingPoint = destinations[0];
        var distance = Vector3.Distance(endingPoint.position, startingPoint.position);
        for (int i = 1; i < destinations.Length; i++)
        {
            var endingPointCandidate = destinations[i];
            var newDistance = Vector3.Distance(endingPointCandidate.position, startingPoint.position);
            if (newDistance < distance)
            {
                endingPoint = endingPointCandidate;
            }
        }

        // TODO In theory directly closest destination might not be at the shortest path to go to.

        navSettings = NavMesh.GetSettingsCount() == 0
            ? NavMesh.CreateSettings()
            : NavMesh.GetSettingsByIndex(0);
        navSettings.agentHeight = agentHeight;
        navSettings.agentRadius = agentWidth / 2;
    }

    private void Bake()
    {
        var bounds = new Bounds();
        var navSources = new List<NavMeshBuildSource>();
        foreach (var floor in siteAnalyser.Floors)
        {
            var bb = floor.GetComponent<Renderer>()?.bounds;
            bounds.Encapsulate(bb.GetValueOrDefault());

            var navSurface = floor.GetOrAddComponent<NavMeshSurface>(); // TODO Do we need to set it since we are making all from code?..

            var meshFilter = floor.GetComponent<MeshFilter>();
            var source = new NavMeshBuildSource
            {
                component = navSurface,
                shape = NavMeshBuildSourceShape.Mesh, // TODO Consider limitation in 100K units in size.
                sourceObject = meshFilter.sharedMesh,
                transform = floor.transform.localToWorldMatrix,
                area = 0, // Built-in Non Walkable Navigation Area
            };
            navSources.Add(source);
        }

        foreach (var obstacle in siteAnalyser.Obstacles)
        {
            // TODO Don't really need box colliders, but NavMeshObstacles.
            var navObstacle = obstacle.GetOrAddComponent<NavMeshObstacle>(); // TODO Do we need to set it since we are making all from code?..

            // TODO Set collider (if there is no one) based on the shape of the mesh.
            //obstacle.GetOrAddComponent<Collider>();
            var collider = obstacle.GetOrAddComponent<BoxCollider>();
            bounds.Encapsulate(collider.bounds);
            var source = new NavMeshBuildSource
            {
                component = navObstacle,
                shape = NavMeshBuildSourceShape.Box, // TODO Determine shape based on the collider set to the GO.
                transform = collider.transform.localToWorldMatrix,
                size = collider.bounds.size,
                area = 1, // Built-in Non Walkable Navigation Area
            };
            navSources.Add(source);
        }

        // TODO Cache NavMesheData per floor or per model. Could even upload it to the cloud to share with collegues?
        //NavMesh.AddNavMeshData(navData1);

        // TODO Decide if it makes sence to have many settings.
        //var navSettingsFloor1 = NavMesh.CreateSettings();

        if ((navData?.ToString() ?? "null") == "null")
        {
            navData = NavMeshBuilder.BuildNavMeshData(
                navSettings,
                navSources,
                bounds,
                Vector3.zero,
                Quaternion.identity);
        }
        else
        {
            NavMesh.RemoveAllNavMeshData();

            // TODO Delete and recreate if it wasn't updated.
            _ = NavMeshBuilder.UpdateNavMeshData(
                navData,
                navSettings,
                navSources,
                bounds);
        }
        _ = NavMesh.AddNavMeshData(navData);

        // TODO Cache baked NavMesh.
    }

    private void DrawPath()
    {
        var path = new NavMeshPath();

        // TODO It also has to be a part of validation.
        var pointsAreOver = NavMesh.SamplePosition(startingPoint.position, out NavMeshHit hitA, MasDistanceToMesh, NavMesh.AllAreas);
        pointsAreOver |= NavMesh.SamplePosition(endingPoint.position, out NavMeshHit hitB, MasDistanceToMesh, NavMesh.AllAreas);
        if (!pointsAreOver)
        {
            Debug.Log("Points are not in a walkable area.");
            return;
        }

        if (NavMesh.CalculatePath(hitA.position, hitB.position, NavMesh.AllAreas, path))
        {
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                // TODO Prepare navigation instructions.
                Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red, 10.0f);
            }
        }
        else
        {
            Debug.LogWarning("\nNo valid path between PointA and PointB." +
                "\nShowing only direction to the destination.");
            Debug.DrawLine(startingPoint.position, endingPoint.position, Color.red, 10.0f, depthTest: false);
        }
    }
}
