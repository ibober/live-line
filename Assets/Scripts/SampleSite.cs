using System;
using UnityEngine;

/// <summary>
/// Example class which provides <see cref="PathFinder"/> with objects to navigate between
/// based on the BIM models provided by the <a href="https://apps.autodesk.com/RVT/en/Detail/Index?id=4018039617950599215">glTF Exporter for Revit</a> and loaded into the scene.
/// </summary>
public class SampleSite : NavigationSite
{
    private GameObject[] floors;
    private GameObject[] obstacles;
    private bool analized;


    public override GameObject[] GetFloors()
    {
        if (!analized)
        {
            throw new InvalidOperationException("Site wasn't analized yet.");
        }

        return floors;
    }

    public override GameObject[] GetObstacles()
    {
        if (!analized)
        {
            throw new InvalidOperationException("Site wasn't analized yet.");
        }

        return obstacles;
    }

    private void Awake()
    {
        // TODO Collect object.
        floors = new GameObject[0];
        obstacles = new GameObject[0];

        OnAnalized?.Invoke();
    }
}
