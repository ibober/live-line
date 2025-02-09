using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SimplePathFinder))]
public class SimplePathFinderComponent : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var component = (SimplePathFinder)target;
        if (GUILayout.Button("Bake NavMesh"))
        {
            component.BakeNavMesh();
        }
        if(GUILayout.Button("Show path"))
        {
            component.ShowPath();
        }
    }
}
