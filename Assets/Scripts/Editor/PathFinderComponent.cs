using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathFinder))]
public class PathFinderComponent : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var component = (PathFinder)target;
        if (GUILayout.Button("Calculate path"))
        {
            component.CalculatePath(force: true);
        }
    }
}
