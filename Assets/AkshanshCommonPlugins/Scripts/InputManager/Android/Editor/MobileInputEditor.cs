using UnityEditor;

namespace AkshanshKanojia.Inputs.Mobile
{
    [CustomEditor(typeof(MobileInputManager))]
    public class MobileInputEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            //mouse properties
            var _tempMang = (MobileInputManager)target;
            if(_tempMang.supportCrossPlatformTesting)
            {
                _tempMang.mouseDragSensitivity = EditorGUILayout.FloatField("MouseDragSenstivity", 2);
            }
        }
    }
}
