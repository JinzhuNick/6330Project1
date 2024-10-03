#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    private SerializedProperty gridPrefabProp;
    private SerializedProperty gridWidthProp;
    private SerializedProperty gridHeightProp;
    private SerializedProperty cellSizeProp;
    private SerializedProperty gridCellsListProp;

    void OnEnable()
    {
        gridPrefabProp = serializedObject.FindProperty("cellPrefab");
        gridWidthProp = serializedObject.FindProperty("gridWidth");
        gridHeightProp = serializedObject.FindProperty("gridHeight");
        cellSizeProp = serializedObject.FindProperty("cellSize");
        gridCellsListProp = serializedObject.FindProperty("gridCellsList");
    }

    public override void OnInspectorGUI()
    {
        // 更新序列化对象
        serializedObject.Update();

        // 绘制网格属性
        EditorGUILayout.PropertyField(gridPrefabProp);
        EditorGUILayout.PropertyField(gridWidthProp);
        EditorGUILayout.PropertyField(gridHeightProp);
        EditorGUILayout.PropertyField(cellSizeProp);

        GridManager gridManager = (GridManager)target;

        if (GUILayout.Button("生成网格"))
        {
            gridManager.CreateGrid();
            // 更新序列化对象，以反映新的格子列表
            serializedObject.Update();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("格子列表", EditorStyles.boldLabel);

        if (gridCellsListProp != null && gridCellsListProp.isArray)
        {
            // 遍历格子列表
            for (int i = 0; i < gridCellsListProp.arraySize; i++)
            {
                SerializedProperty cellProp = gridCellsListProp.GetArrayElementAtIndex(i);

                SerializedProperty xProp = cellProp.FindPropertyRelative("x");
                SerializedProperty yProp = cellProp.FindPropertyRelative("y");
                SerializedProperty isWalkableProp = cellProp.FindPropertyRelative("isWalkable");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"格子 ({xProp.intValue}, {yProp.intValue})", GUILayout.Width(100));

                // 使用 PropertyField 绘制 isWalkable 属性
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(isWalkableProp, GUIContent.none, GUILayout.Width(60));
                if (EditorGUI.EndChangeCheck())
                {
                    // 当 isWalkable 修改后，更新格子视觉
                    GridCell cell = gridManager.gridCellsList[i];
                    cell.isWalkable = isWalkableProp.boolValue;
                    cell.UpdateVisual();
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        // 应用属性修改
        serializedObject.ApplyModifiedProperties();
    }
}
#endif