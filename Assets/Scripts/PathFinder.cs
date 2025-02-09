using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Class automagically finds and draws shortest path from the transformation it's assigned to to the destination point at the provided <see cref="NavigationSite"/>.
/// </summary>
public partial class PathFinder : MonoBehaviour
{
    private const float MaxDistanceToMesh = 2f;

    private bool isBusy;
    private Transform endingPoint;
    private NavMeshBuildSettings navSettings;

    [Space]
    [Header("Site settings")]

    [Tooltip("Walkable area, passages and impassable objects provider.")]
    public NavigationSite siteAnalyser;

    [Tooltip("Scriptable object which caches generated navigation data.")]
    public NavDataCacher navDataCacher;

    [Tooltip("Leave empty to take attached GO's tranfsorm.")]
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


    // TODO Extend for finding route.
    private bool IsSubscribed => siteAnalyser.OnAnalysed?.GetInvocationList().Any(d => d.Method.Name == nameof(BakeNavMesh)) ?? false;

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
        if (isBusy)
        {
            return;
        }

        isBusy = true;
        try
        {
            if (!siteAnalyser.IsAnalysed)
            {
                if (force)
                {
                    var analysed = siteAnalyser.TriggerSiteAnalysis();
                    if (!analysed)
                    {
#if UNITY_EDITOR
                        Debug.Log($"Couldn't force-analyse navigation site.");
#endif
                        return;
                    }
                }
                else
                {
#if UNITY_EDITOR
                    Debug.Log($"Site is not analysed yet.");
#endif
                    return;
                }
            }

            BakeNavMesh();

            DrawPath();
        }
        finally
        {
            isBusy = false;
        }
    }

    private void BakeNavMesh()
    {
        navSettings = NavMesh.GetSettingsCount() == 0
            ? NavMesh.CreateSettings()
            : NavMesh.GetSettingsByIndex(0);
        navSettings.agentHeight = agentHeight;
        navSettings.agentRadius = agentWidth / 2;
        navSettings.minRegionArea = 0.1f; // This setting won't make sence if we set up MeshLinks through the doors.

        // NOTE We have to make minRegionArea small enough so agent could pass door openings in walls.
        // Given that minimum valid door width = 700mm,
        // taking worker (agentWidth) as per input (e.g. 0.7),
        // there is no room left to go through the opening: 700m - 2 * 0.7 / 2 = 0.
        // Even if door is 1m wide, we have only 300mm to pass through, so the minRegionArea has to be twice smaller in that case - 0.15.

        if (navDataCacher.TryGet(siteAnalyser.Name, out var navData))
        {
            navData = NavMeshBuilder.BuildNavMeshData(
                navSettings,
                siteAnalyser.Elements,
                siteAnalyser.Bounds,
                Vector3.zero,
                Quaternion.identity);

            navDataCacher.Set(siteAnalyser.Name, navData);
        }
        else
        {
            var updated = NavMeshBuilder.UpdateNavMeshData(
                navData,
                navSettings,
                siteAnalyser.Elements,
                siteAnalyser.Bounds);
            // TODO What if NavMeshData wasn't updated?
        }

        //NavMesh.RemoveAllNavMeshData();

        //navData.hideFlags = HideFlags.None;

        // TODO If we add each time, how many instances are there?
        var navDataInstance = NavMesh.AddNavMeshData(navData);
        var test = navDataInstance.valid;

        // TODO Cache baked NavMesh.
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
    }

    private void DrawPath()
    {
        ValidatePoints();

        var path = new NavMeshPath();

        // TODO It also has to be a part of validation.
        var pointsAreOver = NavMesh.SamplePosition(startingPoint.position, out NavMeshHit hitA, MaxDistanceToMesh, NavMesh.AllAreas);
        pointsAreOver |= NavMesh.SamplePosition(endingPoint.position, out NavMeshHit hitB, MaxDistanceToMesh, NavMesh.AllAreas);
        if (!pointsAreOver)
        {
            //TODO Probably NavMesh.FindClosestEdge could be used to find at least something.
            Debug.Log("Points are not in a walkable area.");
            return;
        }

        if (NavMesh.CalculatePath(hitA.position, hitB.position, NavMesh.AllAreas, path))
        {
            VisualisePath(path);
            return;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                // TODO Prepare navigation instructions.
                // TODO Use LineRenderer.
                Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red, 10.0f);
            }
        }
        else
        {
            Debug.LogWarning("\nNo valid path between PointA and PointB." +
                "\nShowing only direction to the destination.");

            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                // TODO Prepare navigation instructions.
                // TODO Use LineRenderer.
                Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red, 10.0f);
            }
        }
    }

    private bool ValidatePoints()
    {
        // Validate start and destination points.
        if (destinations.Length == 0)
        {
#if UNITY_EDITOR
            // TODO Ctrl+F all Debug.Log and replase with better logging system.
            Debug.Log($"Nowhere to go. No {nameof(destinations)} set.");
#endif
            return false;
        }

        if ((startingPoint?.ToString() ?? "null") == "null") // Strangely it says startingPoint is not equal null if though it's not assigned in Editor.
        {
            startingPoint = transform;
        }

        // TODO Directly closest destination might not be at the shortest path to go to.
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

        return true;
    }

    private void VisualisePath(NavMeshPath path)
    {
        var lineRenderer = this.GetOrAddComponent<LineRenderer>();
        lineRenderer.positionCount = path.corners.Length;
        lineRenderer.SetPositions(path.corners);
        lineRenderer.material.color = Color.red;
        lineRenderer.widthMultiplier = 0.7f;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.Simplify(0); // TODO Do I need it really? May be for the navigation instructions.
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
    }



    internal void CacheNavData(string siteName, NavMeshData navData)
    {
        if (navDataCacher == null)
        {
            Debug.LogWarning($"{nameof(navDataCacher)} is not set.");
            return;
        }

        navDataCacher.Set(siteName, navData);
    }
}
