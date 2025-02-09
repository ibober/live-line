using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathFinder))]
public partial class PathFinderComponent : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var component = (PathFinder)target;
        if (GUILayout.Button("Visualise the Path"))
        {
            component.DrawPath();
        }
    }
}
