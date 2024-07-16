using UnityEditor;
using UnityEngine;

namespace AkshanshKanojia.Controllers.PointClick
{
    [CustomEditor(typeof(PointClickManager))]
    public class PointClickEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var _tempMang = (PointClickManager)target;
            #region Location Properties
            _tempMang.ClampLocation = EditorGUILayout.Toggle("Clamp Location",_tempMang.ClampLocation);
            if(_tempMang.ClampLocation)
            {
                _tempMang.MinPosClamp = EditorGUILayout.Vector3Field("Min Pos Clamp", _tempMang.MinPosClamp);
                _tempMang.MaxPosClamp = EditorGUILayout.Vector3Field("Max Pos Clamp", _tempMang.MaxPosClamp);
            }
            #endregion
            #region Rotation Properties
            if (_tempMang.RotatePlayerWhileMoving)
            {
                GUILayout.Label("Rotation Properties");
                _tempMang.RotSpeed = EditorGUILayout.FloatField("Rotation Speed", _tempMang.RotSpeed);
                _tempMang.RotationAxis = (PointClickManager.RotationOptions)EditorGUILayout.EnumPopup("Rotation Axis", _tempMang.RotationAxis);
                _tempMang.ClampRotation = EditorGUILayout.Toggle("Clam Rotation", _tempMang.ClampRotation);
                if(_tempMang.ClampRotation)
                {
                    _tempMang.MinRotationClamp = EditorGUILayout.Vector3Field("MinRotationClamp", _tempMang.MinRotationClamp);
                    _tempMang.MaxRotationClamp = EditorGUILayout.Vector3Field("MaxRotationClamp", _tempMang.MaxRotationClamp);
                }
                _tempMang.SeprateRotBody = EditorGUILayout.Toggle("Seprate Rotation Object", _tempMang.SeprateRotBody);
                if(_tempMang.SeprateRotBody)
                {
                    _tempMang.RotObj = (GameObject)EditorGUILayout.ObjectField("Rotation Body",_tempMang.RotObj,typeof(GameObject)
                        , true);
                }
            }
            #endregion
        }
    }
}