using UnityEditor;

[CustomEditor(typeof(Tile))]
public class TileEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Tile tile = (Tile)target;

        tile.IsDirectional = EditorGUILayout.Toggle("Is Directional", tile.IsDirectional);

        serializedObject.Update();

        if (tile.IsDirectional)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UpNeighbours"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("RightNeighbours"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DownNeighbours"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("LeftNeighbours"));
        }
        else
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Neighbours"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
