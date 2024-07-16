using UnityEngine;
using UnityEditor;

namespace AkshanshKanojia.Controllers
{
    [CustomEditor(typeof(CharacterSetupManager))]
    public class CharacterSetupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var _tempMang = (CharacterSetupManager)target;
            if(_tempMang.GenerateOverSelectedObject)
            {
                _tempMang.CharacterParent = (GameObject)EditorGUILayout.ObjectField("Character Object",_tempMang.CharacterParent, typeof(GameObject),true);
            }
            if (GUILayout.Button("Generate Template"))
            {
                _tempMang.GenerateController();
            }
        }
    }
}