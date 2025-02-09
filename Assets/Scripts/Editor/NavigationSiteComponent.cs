using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NavigationSite), editorForChildClasses: true)]
public class NavigationSiteComponent : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox(
            "This is an implementation of NavigationSite component, which is a mandatory parameter for PathFinder component to build paths.",
            MessageType.Info);

        DrawPropertiesExcluding(serializedObject, nameof(NavigationSite.ignoreSiteAnalysisListeners));

        var ignoreDelegatesField = serializedObject.FindProperty(nameof(NavigationSite.ignoreSiteAnalysisListeners));
        EditorGUILayout.PropertyField(ignoreDelegatesField);
        serializedObject.ApplyModifiedProperties();

        var component = (NavigationSite)target;
        if (GUILayout.Button("Cache"))
        {
            component.Analyse();
        }

        if (GUILayout.Button("Remove all NavMeshData"))
        {
            component.RemoveAllNavMeshData();
        }
    }
}