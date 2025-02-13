using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Class automagically finds and draws shortest path from the transformation it's assigned to to the destination point at the provided <see cref="NavigationSite"/>.
/// </summary>
[ExecuteInEditMode]
public partial class PathFinder : MonoBehaviour
{
    private bool isBusy;
    private Vector3 startingPoint;
    private Transform endingPoint;
    private NavMeshPath path;
    private LineRenderer lineRenderer;
    private Material lineMaterial;

    [SerializeField]
    [Tooltip("Walkable area, passages and impassable objects provider.")]
    private NavigationSite site;

    /// <summary>
    /// Walkable area, passages and impassable objects provider.
    /// </summary>
    public NavigationSite Site
    {
        get => site;
        set
        {
            Relax();
            site = value;
            WatchOut();
        }
    }

    [Tooltip("Alternative destination points. Closest one will be selected.")]
    public Transform[] destinations;

    [Tooltip("The distance you move before the path is recalculated.")]
    [Min(0)]
    public float updateRange = 1.5f;

    [Tooltip("Speed of symbols running along the path to the destination")]
    public float scrollSpeed = 0.5f;

    /// <summary>
    /// Navigation instructions to follow to get from current position to the closest destination.
    /// </summary>
    internal NavigationInstructions Instructions { get; private set; }

    public void DrawPath()
    {
        if (!ConfirmDestination())
            return;

        if (isBusy)
            return;

        isBusy = true;
        try
        {
            if (!site.IsAnalysed)
            {
                // TODO Ctrl+Shift+F all Debug.Log and replase with better logging system,
                // or wrap in #if UNITY_EDITOR compilation directive.
                Debug.Log($"{Site.gameObject.name} navigation site has not been analysed yet.");
                WatchOut();
                return;
            }

            _ = CalculatePath();

            VisualisePath();
        }
        finally
        {
            isBusy = false;
        }
    }

    private bool ConfirmDestination()
    {
        if (destinations.Length == 0)
        {
            Debug.Log($"Nowhere to go. No {nameof(destinations)} set.");
            return false;
        }

        // TODO Directly closest destination might not be at the shortest path to go to.
        // We might better calculate paths to all destinations (sorting them by the distance to each),
        // and then draw the shortest path.
        endingPoint = destinations[0];
        var closestDistanceSqr = (endingPoint.position - transform.position).sqrMagnitude;
        for (int i = 1; i < destinations.Length; i++)
        {
            var endingPointCandidate = destinations[i];
            var newDistanceSqr = (endingPointCandidate.position - transform.position).sqrMagnitude;
            if (newDistanceSqr < closestDistanceSqr)
            {
                endingPoint = endingPointCandidate;
            }
        }

        return true;
    }

    private bool CalculatePath()
    {
        path = null;
        var pointAisOver = NavMesh.SamplePosition(transform.position, out var hitA, Constants.Path.MaxDistanceToNavSerface, NavMesh.AllAreas);
        if (!pointAisOver)
        {
            Debug.LogWarning("Off the site location.");
            return false;
        }

        var pointBisOver = NavMesh.SamplePosition(endingPoint.position, out var hitB, Constants.Path.MaxDistanceToNavSerface, NavMesh.AllAreas);
        if (!pointBisOver)
        {
            // TODO Try to find path to another destination, or
            // try finding at least the closest point at the border of walkable area.
            //var edgeFound = NavMesh.FindClosestEdge(transform.position, out var hitBedge, NavMesh.AllAreas);
            Debug.Log("Destination is not in a walkable area." +
                "\nShowing only direction to the destination.");
            return false;
        }

        path = new NavMeshPath();
        var pathFound = NavMesh.CalculatePath(hitA.position, hitB.position, NavMesh.AllAreas, path);
        if (pathFound)
        {
            // Remember starting point to update the path only when move away for a decent distance.
            startingPoint = transform.position;
        }
        else
        {
            // TODO Try to find path to another destination.
            Debug.LogWarning("No valid path found." +
                "\nShowing only direction to the destination.");
        }

        return true;
    }

    private void VisualisePath()
    {
        var notPathOnlyDirection = path == null || path.status == NavMeshPathStatus.PathInvalid;
        if (!this.gameObject.TryGetComponent(out lineRenderer))
        {
            lineRenderer = this.gameObject.AddComponent<LineRenderer>();
            lineRenderer.enabled = true;
            lineRenderer.widthMultiplier = Constants.Path.Width;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
        }

        if (lineMaterial == null)
        {
            // TODO Might need to find with AssetDatabase.FindAssets.
            lineMaterial = Resources.Load<Material>(Constants.Path.EvacuationLineMaterialPath);
            if (lineMaterial != null || notPathOnlyDirection)
            {
                lineRenderer.textureMode = LineTextureMode.Tile;
                lineRenderer.textureScale = new Vector2(1 / Constants.Path.Width, 1);
            }
            else
            {
                lineMaterial = new Material(Shader.Find("Standard"));
                lineMaterial.color = Color.red;
            }

#if UNITY_EDITOR
            lineRenderer.sharedMaterial = lineMaterial;
#else
            lineRenderer.material = lineMaterial;
#endif
        }

        Vector3[] corners;
        if (notPathOnlyDirection)
        {
            corners = new Vector3[] { startingPoint, endingPoint.position };
        }
        else
        {
            corners = path.corners.Select(c => c + new Vector3(0, Constants.Path.Elevation, 0)).ToArray();
        }

        lineRenderer.positionCount = corners.Length;
        lineRenderer.SetPositions(corners);

        // TODO I'm not sure if it worth ommiting corners which are too close to each other.
        // If performance on mobile is OK - don't do it, prefer accuracy.
        //lineRenderer.Simplify(1f);

        Instructions = new NavigationInstructions(lineRenderer);
        Instructions.Calculate();
        Instructions.Log();
    }

    #region Lifecycle methods.

    private void Start()
    {
        startingPoint = transform.position;

        if (site.IsAnalysed)
        {
            DrawPath();
        }

        WatchOut();

#if UNITY_EDITOR
        EditorApplication.update += Update; // Meant to help with path scrolling but doen't really work.
#endif
    }

    private void OnEnable()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = true;
    }

    private void Update()
    {
        if (lineMaterial != null && lineMaterial.mainTextureOffset != null)
        {
            var offset =
#if UNITY_EDITOR
                (float)EditorApplication.timeSinceStartup
#else
                Time.time
#endif
                * -scrollSpeed;
            lineMaterial.mainTextureOffset = new Vector2(offset, 0);
        }

        if (Vector3.Distance(transform.position, startingPoint) > updateRange)
        {
            DrawPath();
        }
    }

    private void OnDisable()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(lineRenderer);
            }
#endif
        }
    }

    private void OnDestroy()
    {
        Relax();

        if (lineRenderer != null)
        {
#if !UNITY_EDITOR
            Destroy(lineRenderer);
#endif
        }
    }

    private bool Subscribed => site.OnAnalysed?.GetInvocationList().Any(m => m.Method.Name == nameof(DrawPath)) ?? false;

    /// <summary>
    /// Subscribes to the <see cref="site.OnAnalized"/>.
    /// </summary>
    private void WatchOut()
    {
        if (site == null || Subscribed)
            return;

        site.OnAnalysed += DrawPath;
    }

    /// <summary>
    /// Unsubscribes from <see cref="site.OnAnalized"/>.
    /// </summary>
    private void Relax()
    {
        if (site == null || !Subscribed)
            return;

        site.OnAnalysed -= DrawPath;
    }

    #endregion
}
