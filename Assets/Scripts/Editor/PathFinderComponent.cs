using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathFinder))]
public partial class PathFinderComponent : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(
            $"This script draws the shortest path on the '{nameof(PathFinder.Site)}' to the closes destination from the list of '{nameof(PathFinder.destinations)}'." +
            $"\n\nIt's updated automatically by '{nameof(NavigationSite)}.{nameof(NavigationSite.OnAnalysed)}' " +
            $"or when GameObject moves away for a certain distance specified by '{nameof(PathFinder.updateRange)}' field, " +
            $"or manually by '{nameof(PathFinder)}.{nameof(PathFinder.DrawPath)}' method.",
            MessageType.Info);

        DrawDefaultInspector();

        var component = (PathFinder)target;
        if (GUILayout.Button("Draw Path"))
        {
            component.DrawPath();
        }
    }
}
