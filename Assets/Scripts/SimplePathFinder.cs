using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.AI;

public class SimplePathFinder : MonoBehaviour
{
    private const float MasDistanceToMesh = 2f;

    [SerializeField]
    private GameObject pointA;

    private GameObject pointB;

    public NavigationSite siteAnalyser;

    public void BakeNavMesh()
    {
        var navSettings = NavMesh.GetSettingsCount() == 0
            ? NavMesh.CreateSettings()
            : NavMesh.GetSettingsByIndex(0);
        navSettings.agentHeight = 1f;
        navSettings.agentRadius = 0.1f;

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
            var navObstacle = obstacle.GetOrAddComponent<NavMeshObstacle>();
            navObstacle.carving = true;

            // TODO Set collider (if there is no one) based on the shape of the mesh.
            //obstacle.GetOrAddComponent<Collider>();
            var collider = obstacle.GetOrAddComponent<BoxCollider>();
            bounds.Encapsulate(collider.bounds);
            var source = new NavMeshBuildSource
            {
                component = navObstacle,
                shape = NavMeshBuildSourceShape.Mesh, // TODO Determine shape based on the collider set to the GO.
                transform = obstacle.transform.localToWorldMatrix,
                size = navObstacle.size,// collider.bounds.size,
                area = 1, // Built-in Non Walkable Navigation Area
            };
            navSources.Add(source);
        }

        foreach (var pass in siteAnalyser.Passages)
        {
            var navModifier = pass.GetOrAddComponent<NavMeshModifier>();
            navModifier.applyToChildren = true;
            navModifier.overrideArea = true;
            navModifier.area = 0;

            //var meshFilter = pass.GetComponent<MeshFilter>();
            //var source = new NavMeshBuildSource
            //{
            //    shape = NavMeshBuildSourceShape.Mesh, // TODO Consider limitation in 100K units in size.
            //    sourceObject = meshFilter.sharedMesh,
            //    transform = pass.transform.localToWorldMatrix,
            //    area = 0, // Built-in Non Walkable Navigation Area
            //};
            //navSources.Add(source);
        }

        NavMesh.RemoveAllNavMeshData();
        var navData = NavMeshBuilder.BuildNavMeshData(
            navSettings,
            navSources,
            bounds,
            Vector3.zero,
            Quaternion.identity);
        _ = NavMesh.AddNavMeshData(navData);
    }

    public void ShowPath()
    {
        var path = new NavMeshPath();
        var pointsAreOver = NavMesh.SamplePosition(pointA.transform.position, out NavMeshHit hitA, MasDistanceToMesh, NavMesh.AllAreas);
        pointsAreOver |= NavMesh.SamplePosition(pointB.transform.position, out NavMeshHit hitB, MasDistanceToMesh, NavMesh.AllAreas);

        if (pointsAreOver && NavMesh.CalculatePath(hitA.position, hitB.position, NavMesh.AllAreas, path))
        {
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red, 10.0f);
            }
        }
        else
        {
            Debug.DrawLine(pointA.transform.position, pointB.transform.position, Color.red, 10.0f, depthTest: false);
        }
    }
}
