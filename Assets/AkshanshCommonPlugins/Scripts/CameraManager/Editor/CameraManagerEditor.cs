using UnityEditor;
using UnityEngine;

namespace AkshanshKanojia.Controllers.CameraController
{
    public class CameraManagerEditor : MonoBehaviour
    {
        [CustomEditor(typeof(CameraManager))]
        public class PointClickEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                var _tempMang = (CameraManager)target;
                if (_tempMang.TrackObject)
                {
                    _tempMang.CameraBehaviour = (CameraManager.AvailableBehaviours)EditorGUILayout.EnumPopup(new GUIContent
                        ("Camera Behaviour", "Detrmines how camera will react toward target"), _tempMang.CameraBehaviour);
                    _tempMang.CurrentTargetObj = (GameObject)EditorGUILayout.ObjectField("Target Object", _tempMang.CurrentTargetObj,
                    typeof(GameObject), true);
                    _tempMang.SeprateRotationTarget = EditorGUILayout.Toggle("Seprate Rotation Target",_tempMang.SeprateRotationTarget);
                    if(_tempMang.SeprateRotationTarget)
                    {
                        _tempMang.CurrentRotObject = (GameObject)EditorGUILayout.ObjectField("Rotation Target", _tempMang.CurrentRotObject,
                    typeof(GameObject), true);
                    }
                }
                #region Movement Properties
                GUILayout.Label("Movement Properties");
                if ((_tempMang.CameraBehaviour == CameraManager.AvailableBehaviours.Track ||
                   _tempMang.CameraBehaviour == CameraManager.AvailableBehaviours.TrackAndRotate) && _tempMang.TrackObject)
                {
                    _tempMang.curtTrackAxis = (CameraManager.AvailableTrackAxis)EditorGUILayout.EnumPopup("Track Axis",
                        _tempMang.curtTrackAxis);
                    _tempMang.moveSpeed = EditorGUILayout.FloatField("Tracking Speed", _tempMang.moveSpeed);
                    _tempMang.trackBeginDistance = EditorGUILayout.FloatField("Track Begin Dist", _tempMang.trackBeginDistance);
                    _tempMang.trackStopDistance = EditorGUILayout.FloatField("Track Stop Dist", _tempMang.trackStopDistance);
                }
                #endregion

                #region Rotation Properties
                if ((_tempMang.CameraBehaviour == CameraManager.AvailableBehaviours.Rotate ||
                   _tempMang.CameraBehaviour == CameraManager.AvailableBehaviours.TrackAndRotate)&&_tempMang.TrackObject)
                {
                    GUILayout.Label("Rotation Properties");
                    _tempMang.RotSpeed = EditorGUILayout.FloatField("Rotation Speed", _tempMang.RotSpeed);
                    _tempMang.RotationAxis = (CameraManager.RotationOptions)EditorGUILayout.EnumPopup("Rotation Axis", _tempMang.RotationAxis);
                    _tempMang.ClampRotation = EditorGUILayout.Toggle("Clam Rotation", _tempMang.ClampRotation);
                    if (_tempMang.ClampRotation)
                    {
                        _tempMang.MinRotationClamp = EditorGUILayout.Vector3Field("MinRotationClamp", _tempMang.MinRotationClamp);
                        _tempMang.MaxRotationClamp = EditorGUILayout.Vector3Field("MaxRotationClamp", _tempMang.MaxRotationClamp);
                    }
                }
                #endregion
            }
        }
    }
}
