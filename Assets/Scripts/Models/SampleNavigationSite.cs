using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Example class which provides <see cref="PathFinder"/> with objects to navigate
/// based on the English language BIM models from the <a href="https://apps.autodesk.com/RVT/en/Detail/Index?id=4018039617950599215">glTF Exporter for Revit</a> loaded into the scene.
/// </summary>
public class SampleNavigationSite : NavigationSite
{
    protected override List<GameObject> GetFloors()
    {
        return FindElementsOfCategory(transform, "Topography", "Floors", "Stairs");
    }

    protected override List<GameObject> GetObstacles()
    {
        var obstacles = FindElementsOfCategory(transform, "Walls");
        var furniture = FindElementsOfCategory(transform, "Furniture");
        var columns = FindElementsOfCategory(transform, "Structural Columns");
        obstacles.AddRange(furniture);
        obstacles.AddRange(columns);
        return obstacles;
    }

    protected override List<GameObject> GetPassages()
    {
        return FindElementsOfCategory(transform, "Doors");
    }

    private void Start()
    {
        Analyse();
    }

    /// <summary>
    /// Collects all children of the nodes named as categories supplied as parameter.
    /// </summary>
    /// <param name="transform">Root of search node in the hierarchy.</param>
    /// <param name="categories">List of categories.</param>
    /// <returns>List of child GameObjects.</returns>
    private List<GameObject> FindElementsOfCategory(Transform transform, params string[] categories)
    {
        var result = new List<GameObject>();
        if (categories.Contains(transform.name))
        {
            var elements = transform
                .GetComponentsInChildren<Transform>(includeInactive: false)
                .Select(tr => tr.gameObject);
            result.AddRange(elements);
            return result;
        }

        foreach (Transform child in transform)
        {
            result.AddRange(FindElementsOfCategory(child, categories));
        }

        return result;
    }
}
