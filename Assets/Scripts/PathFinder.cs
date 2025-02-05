using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Class automagically finds and draws shortest path from the transformation it's assigned to to the destination point at the provided <see cref="NavigationSite"/>.
/// </summary>
public class PathFinder : MonoBehaviour
{
    private bool unsubscribe;
    private NavMeshAgent navAgent;

    [Tooltip("Walkable area and impassable objects provider.")]
    public NavigationSite site;

    [Tooltip("Alternative destination points. Closest one will be selected.")]
    public Transform[] alternativeDestinations;

    // TODO Set current GO as NavMeshAgent.

    private void Start()
    {
        if (site.IsAnalysed)
        {
            PrepareNavigationSystem();
        }
        else
        {
            unsubscribe = true;
            site.OnAnalysed += PrepareNavigationSystem;
        }
    }

    private void PrepareNavigationSystem()
    {
        // TODO Should we clear previously baked NavMesh?

        foreach (var floor in site.Floors)
        {
            var navSurface = floor.AddComponent<NavMeshSurface>();
            
        }

        foreach (var obstacle in site.Obstacles)
        {
            var navObstacle = obstacle.AddComponent<NavMeshObstacle>();

        }

    }

    private void Update()
    {
        // TODO Update visual path.
    }

    private void OnDestroy()
    {
        if (unsubscribe)
        {
            site.OnAnalysed -= PrepareNavigationSystem;
        }
    }
}
