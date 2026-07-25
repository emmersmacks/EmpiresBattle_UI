using EmpiresBattle.Grid;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EmpiresBattle.EditorTools
{
    /// <summary>
    /// Lets designers generate/clear the hex grid directly in the Scene view,
    /// instead of only seeing it appear at runtime.
    /// </summary>
    [CustomEditor(typeof(HexGrid))]
    public class HexGridEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var grid = (HexGrid)target;

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Grid"))
            {
                grid.GenerateGrid();
                EditorSceneManager.MarkSceneDirty(grid.gameObject.scene);
            }

            if (GUILayout.Button("Clear Grid"))
            {
                grid.ClearGrid();
                EditorSceneManager.MarkSceneDirty(grid.gameObject.scene);
            }
        }
    }
}
