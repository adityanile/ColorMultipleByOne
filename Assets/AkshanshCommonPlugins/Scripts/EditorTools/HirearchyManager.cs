using UnityEngine;

namespace AkshanshKanojia.EditorTools
{
    public class HirearchyManager : MonoBehaviour
    {
        #region PublicFields
        public enum AvailableActions { SortChildrenSpacing }
        public AvailableActions CurtAction;

        //childrend sort property
        [HideInInspector] public float ChildSpacing = 2f, ChildstartingPos, CenterPosRef;
        public enum SortingAxis { X, Y, Z }
        [HideInInspector] public SortingAxis ChildSortAxis;
        [HideInInspector] public bool ArrangeChildInLocalSpace = false, UseNegetiveAxis = false, AutoAlignInCenter = false;
        #endregion

        #region SerializeFields
        [SerializeField, Tooltip("Gameobjects on which action will be performed")] GameObject[] ObjectsToActOn;
        #endregion

        #region PrivateFields
        #endregion


        /// <summary>
        /// adjusts children of the object(s) in an equidistant order based on values passed.
        /// </summary>
        public void SortChildrenSpacing()
        {
            if (ObjectsToActOn == null)
            {
                return;
            }
            if (ObjectsToActOn.Length == 0)
            {
                return;
            }
            foreach (GameObject _obj in ObjectsToActOn)
            {
                float _tempStartPos = 0f;
                if (AutoAlignInCenter)
                {
                    _tempStartPos = ((ChildSpacing * _obj.transform.childCount) / 2 + CenterPosRef)-ChildSpacing/2;//subtracting it bcz for an object to be in center it should be 0.5 on both side if scale is 1 ie equally on both sides of center
                    UseNegetiveAxis = true;
                    ArrangeChildInLocalSpace = true;
                }
                float _startPos = (AutoAlignInCenter) ? _tempStartPos : ChildstartingPos;
                //aplly pos loop
                for (int i = 0; i < _obj.transform.childCount; i++)
                {
                    var _tempChild = _obj.transform.GetChild(i).transform;
                    Vector3 _pos = (ArrangeChildInLocalSpace) ? _tempChild.localPosition : _tempChild.position;
                    var _valueToAdd = (UseNegetiveAxis) ? -(i * ChildSpacing) : (i * ChildSpacing);
                    switch (ChildSortAxis)
                    {
                        case SortingAxis.X:
                            _pos.x = _startPos + _valueToAdd;
                            break;
                        case SortingAxis.Y:
                            _pos.y = _startPos + _valueToAdd;
                            break;
                        case SortingAxis.Z:
                            _pos.z = _startPos + _valueToAdd;
                            break;
                        default:
                            return;
                    }
                    if (!ArrangeChildInLocalSpace)
                    {
                        _tempChild.position = _pos;
                    }
                    else
                    {
                        _tempChild.localPosition = _pos;
                    }
                }
            }
        }

        // reset local transform of children
        public void ResetChildPos()
        {
            if (ObjectsToActOn == null)
            {
                return;
            }
            if (ObjectsToActOn.Length == 0)
            {
                return;
            }
            foreach (GameObject _obj in ObjectsToActOn)
            {
                for (int i = 0; i < _obj.transform.childCount; i++)
                {
                    _obj.transform.GetChild(i).transform.localPosition = Vector3.zero;
                }
            }
        }
    }
}
