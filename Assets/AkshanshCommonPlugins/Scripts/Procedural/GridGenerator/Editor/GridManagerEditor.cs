using UnityEditor;
using UnityEngine;

namespace AkshanshKanojia.LevelEditors
{
    [CustomEditor(typeof(GridManager))]
    public class GridManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var _tempGrid = (GridManager)target;
            if (GUILayout.Button("Generate Grid"))
            {
                _tempGrid.GenerateGrid();
            }
            if (GUILayout.Button("Clear Grid"))
            {
                _tempGrid.ClearGrid();
            }

            if (EditorGUI.EndChangeCheck())
            {
                _tempGrid.GenerateGrid();
            }
        }
    }
}
