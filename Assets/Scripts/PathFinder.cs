using UnityEngine;

/// <summary>
/// Class automagically finds and draws shortest path from point A to point B in the provided <see cref="NavigationSite"/>.
/// </summary>
public class PathFinder : MonoBehaviour
{
    [Tooltip("Floors and obstacles provider.")]
    public NavigationSite site;

    [Tooltip("Start point of the path.")]
    public Transform pointA;

    [Tooltip("Finish point of the path.")]
    public Transform pointB;

    [Tooltip("Alternative finish points. Closest one will be selected.")]
    public Transform[] alternativeExits;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
