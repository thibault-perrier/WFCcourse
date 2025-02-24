using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WFCWorldGenerator))]
public class WorldGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WFCWorldGenerator generator = (WFCWorldGenerator)target;

        if (GUILayout.Button("Generate Map"))
        {
            generator.Init();
        }

        if (GUILayout.Button("Delete Map"))
        {
            generator.DeleteWorld();
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("_mapWidth"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_mapHeight"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("_tileRules"));

        serializedObject.ApplyModifiedProperties();
    }
}
