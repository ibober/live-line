using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathFinder))]
public partial class PathFinderComponent : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var component = (PathFinder)target;
        if (GUILayout.Button("Cache NavMesh"))
        {
            // TODO Implement caching NavMeshSurfaceInstance.
        }
        if (GUILayout.Button("Remove all NavMeshData"))
        {
            component.RemoveAllNavMeshData();
            component.CalculatePath(force: true);
            //component.CacheNavData(string.Empty, null);
        }
    }
}
