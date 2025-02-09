using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Class automagically finds and draws shortest path from the transformation it's assigned to to the destination point at the provided <see cref="NavigationSite"/>.
/// </summary>
[ExecuteInEditMode]
public partial class PathFinder : MonoBehaviour
{
    private const float MaxDistanceToMesh = 2f;

    private bool isBusy;
    private Transform endingPoint;
    private LineRenderer lineRenderer;

    [Space]

    [Tooltip("Walkable area, passages and impassable objects provider.")]
    public NavigationSite site;

    [Tooltip("Alternative destination points. Closest one will be selected.")]
    public Transform[] destinations;

    public void DrawPath()
    {
        if (!ValidatePoints())
        {
            return;
        }

        if (isBusy)
        {
            return;
        }

        isBusy = true;
        try
        {
            if (!site.IsAnalysed)
            {
#if UNITY_EDITOR
                Debug.Log($"Site is not analysed yet.");
#endif
                return;
            }

            CalculatePath();
        }
        finally
        {
            isBusy = false;
        }
    }

    private void CalculatePath()
    {
        var pointAisOver = NavMesh.SamplePosition(transform.position, out NavMeshHit hitA, MaxDistanceToMesh, NavMesh.AllAreas);
        if (!pointAisOver)
        {
            Debug.LogWarning("Off the site location.");
            return;
        }

        var pointBisOver = NavMesh.SamplePosition(endingPoint.position, out NavMeshHit hitB, MaxDistanceToMesh, NavMesh.AllAreas);
        var edgeFound = NavMesh.FindClosestEdge(transform.position, out var hitBedge, NavMesh.AllAreas);
        if (!pointBisOver)
        {
            Debug.Log("Points are not in a walkable area.");
            return;
        }

        var path = new NavMeshPath();
        if (NavMesh.CalculatePath(hitA.position, hitB.position, NavMesh.AllAreas, path))
        {
            VisualisePath(path);
        }
        else
        {
            Debug.LogWarning("\nNo valid path between PointA and PointB." +
                "\nShowing only direction to the destination.");

            // TODO Prepare navigation instructions.
            // TODO Use LineRenderer.
            Debug.DrawLine(transform.position, endingPoint.position, Color.red, 10.0f);
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

        // TODO Directly closest destination might not be at the shortest path to go to.
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

    private void VisualisePath(NavMeshPath path)
    {
        if (!this.gameObject.TryGetComponent(out lineRenderer))
        {
            lineRenderer = this.gameObject.AddComponent<LineRenderer>();
        }
        lineRenderer.positionCount = path.corners.Length;
        lineRenderer.SetPositions(path.corners);

        var material = new Material(Shader.Find("Standard"));
        material.color = Color.red;
#if UNITY_EDITOR
        lineRenderer.sharedMaterial = material;
#else
        lineRenderer.material = material;
#endif
        //(lineRenderer.sharedMaterial ?? lineRenderer.material).color = Color.red;

        lineRenderer.widthMultiplier = 0.3f; //NavMesh.CreateSettings()
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        //lineRenderer.Simplify(1f); // Not sure if it worth ommiting corners which are closer to each other than this value.
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
    }

    #region Synchronize this script with LineRenderer component.

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

    private void OnDestroy()
    {
        if (lineRenderer != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(lineRenderer);
#else
            Destroy(lineRenderer);
#endif
        }
    }

    #endregion
}
