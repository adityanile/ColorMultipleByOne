using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteGen))]
public class SpriteGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Generate Masks"))
        {
            var _Temp = (SpriteGen)target;
            _Temp.GenerateMask();
        }
        if (GUILayout.Button("Clear Masks"))
        {
            var _Temp = (SpriteGen)target;
            _Temp.ClearPrev();
        }
    }
}
