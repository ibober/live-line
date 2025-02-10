using System.Linq;
using Unity.VisualScripting;
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

    /// <summary>
    /// Navigation instructions to follow to get from current position to the closest destination.
    /// </summary>
    // TODO Calculate navigation instructions.
    public NavigationInstructions Instructions { get; private set; }

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
#if UNITY_EDITOR
                // TODO Ctrl+F all Debug.Log and replase with better logging system,
                // or wrap in #if UNITY_EDITOR compilation directive.
                Debug.Log($"{Site.GetType().Name} is not analysed yet.");
#endif
                return;
            }

            if (!CalculatePath())
                return;

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
        var distance = Vector3.Distance(endingPoint.position, transform.position);
        for (int i = 1; i < destinations.Length; i++)
        {
            var endingPointCandidate = destinations[i];
            var newDistance = Vector3.Distance(endingPointCandidate.position, transform.position);
            if (newDistance < distance)
            {
                endingPoint = endingPointCandidate;
            }
        }

        return true;
    }

    private bool CalculatePath()
    {
        var pointAisOver = NavMesh.SamplePosition(transform.position, out var hitA, Constants.MaxDistanceToNavSerface, NavMesh.AllAreas);
        if (!pointAisOver)
        {
            Debug.LogWarning("Off the site location.");
            return false;
        }

        var pointBisOver = NavMesh.SamplePosition(endingPoint.position, out var hitB, Constants.MaxDistanceToNavSerface, NavMesh.AllAreas);
        if (!pointBisOver)
        {
            // TODO Try to find path to another destination, or
            // try finding at least the closest point at the border of walkable area.
            //var edgeFound = NavMesh.FindClosestEdge(transform.position, out var hitBedge, NavMesh.AllAreas);
            Debug.Log("Destination is not in a walkable area.");
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
        if (!this.gameObject.TryGetComponent(out lineRenderer))
        {
            lineRenderer = this.gameObject.AddComponent<LineRenderer>();
            lineRenderer.enabled = true;
            lineRenderer.widthMultiplier = Constants.PathWidth;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;

            var material = new Material(Shader.Find("Standard"));
            material.color = Color.red;
#if UNITY_EDITOR
            lineRenderer.sharedMaterial = material;
#else
            lineRenderer.material = material;
#endif
        }

        // TODO Highlight the path and direction (when the path is not found) differently.

        var corners = default(Vector3[]);
        if (path.status == NavMeshPathStatus.PathInvalid)
        {
            corners = new Vector3[] { startingPoint, endingPoint.position };
        }
        else
        {
            corners = path.corners.Select(c => c + new Vector3(0, Constants.PathElevation, 0)).ToArray();
        }

        lineRenderer.positionCount = corners.Length;
        lineRenderer.SetPositions(corners);

        // TODO I'm not sure if it worth ommiting corners which are too closer to each other.
        // If performance on mobile is OK - don't do it, prefer accuracy.
        //lineRenderer.Simplify(1f); 
    }

    #region Lifecycle methods.

    private void Awake()
    {
        startingPoint = transform.position;
        WatchOut();
    }

    private void OnEnable()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = true;
    }

    private void OnDisable()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, startingPoint) > updateRange)
        {
            DrawPath();
        }
    }

    private void OnDestroy()
    {
        Relax();

        if (lineRenderer != null && !lineRenderer.IsDestroyed())
        {
#if UNITY_EDITOR
            DestroyImmediate(lineRenderer);
#else
            Destroy(lineRenderer);
#endif
        }
    }

    /// <summary>
    /// Subscribes to the <see cref="site.OnAnalized"/>.
    /// </summary>
    private void WatchOut()
    {
        if (site == null)
            return;

        site.OnAnalysed += DrawPath;
    }

    /// <summary>
    /// Unsubscribes from <see cref="site.OnAnalized"/>.
    /// </summary>
    private void Relax()
    {
        if (site == null)
            return;

        if (site.OnAnalysed?.GetInvocationList().Any(m => m.Method.Name == nameof(DrawPath)) ?? false)
        {
            site.OnAnalysed -= DrawPath;
        }
    }

    #endregion
}
