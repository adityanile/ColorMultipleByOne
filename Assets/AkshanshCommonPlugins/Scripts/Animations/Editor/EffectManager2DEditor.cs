using UnityEditor;
using UnityEngine;

namespace AkshanshKanojia.Animations
{
    [CustomEditor(typeof(EffectsManger2D))]
    public class EffectManager2DEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var _tempMang = (EffectsManger2D)target;
            switch (_tempMang.CurrentEffect)
            {
                case EffectsManger2D.AvailableEffects.FlipPage:
                    //flip options
                    EditorGUILayout.LabelField("Page Flip Properties");
                    _tempMang.FlipObject = (EffectsManger2D.FlipObjectTypes)EditorGUILayout.EnumPopup("Flipping Obejct Type", _tempMang.FlipObject);
                    //sprite options
                    if(_tempMang.FlipObject == EffectsManger2D.FlipObjectTypes.Sprite)
                    {
                        _tempMang.RightFlipTrigger = (GameObject)EditorGUILayout.ObjectField("Right Page Trigger",_tempMang.RightFlipTrigger,typeof(GameObject),true);
                        _tempMang.LeftFlipTrigger = (GameObject)EditorGUILayout.ObjectField("Left Page Trigger",_tempMang.LeftFlipTrigger,typeof(GameObject),true);
                        _tempMang.BookCover = (GameObject)EditorGUILayout.ObjectField("Book Cover Prefab",_tempMang.BookCover,typeof(GameObject),true);
                        _tempMang.BookCover = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Page Prefab","Must have a sprite renderer child as target and origin at bottom right."),
                            _tempMang.BookPagePref,typeof(GameObject),true);  
                    }
                    _tempMang.BookCenter = EditorGUILayout.Vector3Field("Book Center", _tempMang.BookCenter);
                    _tempMang.FlipSpeed = EditorGUILayout.FloatField("Page Flip Speed", _tempMang.FlipSpeed);
                    break;
                default:
                    break;
            }
        }
    }
}
