using System.Collections.Generic;
using UnityEngine;

namespace AkshanshKanojia.Controllers.ObjectManager
{
    public class ObjectController : MonoBehaviour
    {
        [SerializeField] float stoppingDist = 0.2f;

        public enum AvailableUpdateModes { Normal, Late, Fixed }
        [System.Serializable]
        class ActionHolder
        {
            public GameObject TargetObject;
            public Vector3 targetLocation,targetScale;
            public Quaternion targetRotation;
            public float MovementSpeed;
            public bool ReachedPos = true, ReachedRot = true,ReachedScale = true, IsLocal;
            public ActionHolder(GameObject _obj, Vector3 _targetPos, float _speed, bool _isLocal)
            {
                TargetObject = _obj;
                targetLocation = _targetPos;
                MovementSpeed = _speed;
                ReachedPos = false;
                IsLocal = _isLocal;
            }
            public ActionHolder(GameObject _obj, Vector3 _targetPos,Vector3 _targetScale ,float _speed, bool _isLocal)
            {
                TargetObject = _obj;
                targetLocation = _targetPos;
                targetScale = _targetScale;
                MovementSpeed = _speed;
                ReachedPos = false;
                ReachedScale = false;
                IsLocal = _isLocal;
            }
            public ActionHolder(GameObject _obj, Quaternion _targetRot, float _speed, bool _isLocal)
            {
                TargetObject = _obj;
                targetRotation = _targetRot;
                MovementSpeed = _speed;
                ReachedRot = false;
                IsLocal = _isLocal;
            }
            public ActionHolder(GameObject _obj, Quaternion _targetRot,Vector3 _targetScale, float _speed, bool _isLocal)
            {
                TargetObject = _obj;
                targetRotation = _targetRot;
                targetScale = _targetScale;
                MovementSpeed = _speed;
                ReachedRot = false;
                ReachedScale = false;
                IsLocal = _isLocal;
            }
            public ActionHolder(GameObject _obj, Vector3 _targetPos, Quaternion _targetRot, float _speed, bool _isLocal)
            {
                TargetObject = _obj;
                targetLocation = _targetPos;
                targetRotation = _targetRot;
                MovementSpeed = _speed;
                IsLocal = _isLocal;
                ReachedPos = false;
                ReachedRot = false;
            }
            public ActionHolder(GameObject _obj, Vector3 _targetPos, Quaternion _targetRot,Vector3 _targetScale, float _speed, bool _isLocal)
            {
                TargetObject = _obj;
                targetLocation = _targetPos;
                targetRotation = _targetRot;
                targetScale = _targetScale;
                MovementSpeed = _speed;
                IsLocal = _isLocal;
                ReachedPos = false;
                ReachedRot = false;
                ReachedScale = false;
            }
        }

        List<ActionHolder> FixedActiveEvents;
        List<ActionHolder> ActiveEvents;
        List<ActionHolder> LateActiveEvents;

        //events
        public delegate void RotationFinished(GameObject _obj);
        public delegate void MovementFinished(GameObject _obj);
        public delegate void ScaleFinished(GameObject _obj);
        public event RotationFinished OnRotationEnd;
        public event ScaleFinished OnScaleEnd;
        public event MovementFinished OnMovementEnd;

        private void Awake()
        {
            FixedActiveEvents = new List<ActionHolder>();
            ActiveEvents = new List<ActionHolder>();
            LateActiveEvents = new List<ActionHolder>();
        }

        private void FixedUpdate()
        {
            FixedActionManager();
        }
        private void Update()
        {
            ActionManager();
        }
        private void LateUpdate()
        {
            LateActionManager();
        }

        // updates rotation and position of object based on lists which hold specified data
        #region Action managers
        void FixedActionManager()
        {
            foreach (var tempItem in FixedActiveEvents)
            {
                if (tempItem == null)
                    return;
                if (!tempItem.ReachedPos)
                {
                    //update pos
                    Vector3 _tempDir = tempItem.targetLocation - tempItem.TargetObject.transform.position;
                    if (tempItem.IsLocal)
                    {
                        _tempDir = tempItem.targetLocation - tempItem.TargetObject.transform.localPosition;
                        tempItem.TargetObject.transform.localPosition += tempItem.MovementSpeed * Time.deltaTime * _tempDir.normalized;
                    }
                    else
                    {
                        tempItem.TargetObject.transform.position += tempItem.MovementSpeed * Time.deltaTime * _tempDir.normalized;
                    }
                    if (_tempDir.magnitude < stoppingDist)
                    {
                        tempItem.ReachedPos = true;
                        OnMovementEnd?.Invoke(tempItem.TargetObject);
                    }
                }
                //update rot
                if (!tempItem.ReachedRot)
                {
                    if (tempItem.IsLocal)
                    {
                        tempItem.TargetObject.transform.localRotation = Quaternion.Slerp(tempItem.TargetObject.transform.rotation,
                            tempItem.targetRotation, Time.deltaTime * tempItem.MovementSpeed);
                    }
                    else
                    {
                        tempItem.TargetObject.transform.rotation = Quaternion.Slerp(tempItem.TargetObject.transform.rotation,
                            tempItem.targetRotation, Time.deltaTime * tempItem.MovementSpeed);
                    }
                    if (Quaternion.Angle(tempItem.TargetObject.transform.rotation,
                        tempItem.targetRotation) < stoppingDist)
                    {
                        tempItem.ReachedRot = true;
                        OnRotationEnd?.Invoke(tempItem.TargetObject);
                    }
                }
                //update scale
                if(!tempItem.ReachedScale)
                {
                    if(Vector3.Distance(tempItem.TargetObject.transform.localScale,tempItem.targetScale)<stoppingDist)
                    {
                        tempItem.ReachedScale = true;
                        OnScaleEnd?.Invoke(tempItem.TargetObject);
                    }
                    else
                    {
                        tempItem.TargetObject.transform.localScale = Vector3.Lerp(tempItem.TargetObject.transform.localScale,
                            tempItem.targetScale,tempItem.MovementSpeed*Time.deltaTime);
                    }
                }
                if (tempItem.ReachedRot && tempItem.ReachedPos&& tempItem.ReachedScale)
                {
                    RemoveUsedObejct(tempItem, FixedActiveEvents);
                    break;
                }
            }
        }

        void ActionManager()
        {
            foreach (var tempItem in ActiveEvents)
            {
                if (tempItem == null)
                    return;
                if (!tempItem.ReachedPos)
                {
                    //update pos
                    Vector3 _tempDir = tempItem.targetLocation - tempItem.TargetObject.transform.position;
                    if (tempItem.IsLocal)
                    {
                        _tempDir = tempItem.targetLocation - tempItem.TargetObject.transform.localPosition;
                        tempItem.TargetObject.transform.localPosition += tempItem.MovementSpeed * Time.deltaTime * _tempDir.normalized;
                    }
                    else
                    {
                        tempItem.TargetObject.transform.position += tempItem.MovementSpeed * Time.deltaTime * _tempDir.normalized;
                    }
                    if (_tempDir.magnitude < stoppingDist)
                    {
                        tempItem.ReachedPos = true;
                        OnMovementEnd?.Invoke(tempItem.TargetObject);
                    }
                }
                //update rot

                if (!tempItem.ReachedRot)
                {
                    if (tempItem.IsLocal)
                    {
                        tempItem.TargetObject.transform.localRotation = Quaternion.Slerp(tempItem.TargetObject.transform.rotation,
                            tempItem.targetRotation, Time.deltaTime * tempItem.MovementSpeed);
                    }
                    else
                    {
                        tempItem.TargetObject.transform.rotation = Quaternion.Slerp(tempItem.TargetObject.transform.rotation,
                            tempItem.targetRotation, Time.deltaTime * tempItem.MovementSpeed);
                    }
                    if (Quaternion.Angle(tempItem.TargetObject.transform.rotation,
                        tempItem.targetRotation) < stoppingDist)
                    {
                        tempItem.ReachedRot = true;
                        OnRotationEnd?.Invoke(tempItem.TargetObject);
                    }
                }
                //update scale
                if (!tempItem.ReachedScale)
                {
                    if (Vector3.Distance(tempItem.TargetObject.transform.localScale, tempItem.targetScale) < stoppingDist)
                    {
                        tempItem.ReachedScale = true;
                        OnScaleEnd?.Invoke(tempItem.TargetObject);
                    }
                    else
                    {
                        tempItem.TargetObject.transform.localScale = Vector3.Lerp(tempItem.TargetObject.transform.localScale,
                            tempItem.targetScale, tempItem.MovementSpeed * Time.deltaTime);
                    }
                }
                if (tempItem.ReachedRot && tempItem.ReachedPos&& tempItem.ReachedScale)
                {
                    RemoveUsedObejct(tempItem, ActiveEvents);
                    break;
                }
            }
        }

        void LateActionManager()
        {
            foreach (var tempItem in LateActiveEvents)
            {
                if (tempItem == null)
                    return;
                if (!tempItem.ReachedPos)
                {
                    //update pos
                    Vector3 _tempDir = tempItem.targetLocation - tempItem.TargetObject.transform.position;
                    if (tempItem.IsLocal)
                    {
                        _tempDir = tempItem.targetLocation - tempItem.TargetObject.transform.localPosition;
                        tempItem.TargetObject.transform.localPosition += tempItem.MovementSpeed * Time.deltaTime * _tempDir.normalized;
                    }
                    else
                    {
                        tempItem.TargetObject.transform.position += tempItem.MovementSpeed * Time.deltaTime * _tempDir.normalized;
                    }
                    if (_tempDir.magnitude < stoppingDist)
                    {
                        tempItem.ReachedPos = true;
                        OnMovementEnd?.Invoke(tempItem.TargetObject);
                    }
                }
                //update rot

                if (!tempItem.ReachedRot)
                {
                    if (tempItem.IsLocal)
                    {
                        tempItem.TargetObject.transform.localRotation = Quaternion.Slerp(tempItem.TargetObject.transform.rotation,
                            tempItem.targetRotation, Time.deltaTime * tempItem.MovementSpeed);
                    }
                    else
                    {
                        tempItem.TargetObject.transform.rotation = Quaternion.Slerp(tempItem.TargetObject.transform.rotation,
                            tempItem.targetRotation, Time.deltaTime * tempItem.MovementSpeed);
                    }
                    if (Quaternion.Angle(tempItem.TargetObject.transform.rotation,
                        tempItem.targetRotation) < stoppingDist)
                    {
                        tempItem.ReachedRot = true;
                        OnRotationEnd?.Invoke(tempItem.TargetObject);
                    }
                }
                //update scale
                if (!tempItem.ReachedScale)
                {
                    if (Vector3.Distance(tempItem.TargetObject.transform.localScale, tempItem.targetScale) < stoppingDist)
                    {
                        tempItem.ReachedScale = true;
                        OnScaleEnd?.Invoke(tempItem.TargetObject);
                    }
                    else
                    {
                        tempItem.TargetObject.transform.localScale = Vector3.Lerp(tempItem.TargetObject.transform.localScale,
                            tempItem.targetScale, tempItem.MovementSpeed * Time.deltaTime);
                    }
                }
                if (tempItem.ReachedRot && tempItem.ReachedPos&&tempItem.ReachedScale)
                {
                    RemoveUsedObejct(tempItem, LateActiveEvents);
                    break;
                }
            }
        }

        #endregion

        // removes item which has completed all actions specified in that list
        private void RemoveUsedObejct(ActionHolder tempItem, List<ActionHolder> _tempList)
        {
            for (int i = 0; i < _tempList.Count; i++)
            {
                if (tempItem == _tempList[i])
                {
                    _tempList.RemoveAt(i);
                }
            }
        }
        //functions to add different types of object behviour to specific list
        #region Add Event Overloads
        public void AddEvent(GameObject _obj, Vector3 _targetPos, float _speed, bool _isLocal)
        {
            FixedActiveEvents.Add(new ActionHolder(_obj, _targetPos, _speed, _isLocal));
        }
        public void AddEvent(GameObject _obj, Vector3 _targetPos,Vector3 _scale, float _speed, bool _isLocal)
        {
            FixedActiveEvents.Add(new ActionHolder(_obj, _targetPos,_scale, _speed, _isLocal));
        }
        public void AddEvent(GameObject _obj, Quaternion _targetRot, float _speed, bool _isLocal)
        {
            FixedActiveEvents.Add(new ActionHolder(_obj, _targetRot, _speed, _isLocal));
        }
        public void AddEvent(GameObject _obj, Quaternion _targetRot, Vector3 _scale,float _speed, bool _isLocal)
        {
            FixedActiveEvents.Add(new ActionHolder(_obj, _targetRot,_scale, _speed, _isLocal));
        }
        public void AddEvent(GameObject _obj, Vector3 _targetPos, Quaternion _targetRot, float _speed, bool _isLocal)
        {
            FixedActiveEvents.Add(new ActionHolder(_obj, _targetPos, _targetRot, _speed, _isLocal));
        }
        public void AddEvent(GameObject _obj, Vector3 _targetPos, Quaternion _targetRot,Vector3 _scale, float _speed, bool _isLocal)
        {
            FixedActiveEvents.Add(new ActionHolder(_obj, _targetPos,_targetRot, _scale, _speed, _isLocal));
        }
        //
        public void AddEvent(GameObject _obj, Vector3 _targetPos, float _speed, bool _isLocal, AvailableUpdateModes _updateMode)
        {
            switch (_updateMode)
            {
                case AvailableUpdateModes.Fixed:
                    FixedActiveEvents.Add(new ActionHolder(_obj, _targetPos, _speed, _isLocal));
                    break;
                case AvailableUpdateModes.Normal:
                    ActiveEvents.Add(new ActionHolder(_obj, _targetPos, _speed, _isLocal));
                    break;
                case AvailableUpdateModes.Late:
                    LateActiveEvents.Add(new ActionHolder(_obj, _targetPos, _speed, _isLocal));
                    break;
            }
        }
        public void AddEvent(GameObject _obj, Vector3 _targetPos,Vector3 _scale, float _speed, bool _isLocal, AvailableUpdateModes _updateMode)
        {
            switch (_updateMode)
            {
                case AvailableUpdateModes.Fixed:
                    FixedActiveEvents.Add(new ActionHolder(_obj, _targetPos,_scale, _speed, _isLocal));
                    break;
                case AvailableUpdateModes.Normal:
                    ActiveEvents.Add(new ActionHolder(_obj, _targetPos,_scale, _speed, _isLocal));
                    break;
                case AvailableUpdateModes.Late:
                    LateActiveEvents.Add(new ActionHolder(_obj, _targetPos,_scale, _speed, _isLocal));
                    break;
            }
        }
        public void AddEvent(GameObject _obj, Quaternion _targetRot, float _speed, bool _isLocal, AvailableUpdateModes _updateMode)
        {
            switch (_updateMode)
            {
                case AvailableUpdateModes.Fixed:
                    FixedActiveEvents.Add(new ActionHolder(_obj, _targetRot, _speed, _isLocal));
                    break;
                case AvailableUpdateModes.Normal:
                    ActiveEvents.Add(new ActionHolder(_obj, _targetRot, _speed, _isLocal));
                    break;
                case AvailableUpdateModes.Late:
                    LateActiveEvents.Add(new ActionHolder(_obj, _targetRot, _speed, _isLocal));
                    break;
            }
        }
        public void AddEvent(GameObject _obj, Quaternion _targetRot,Vector3 _scale, float _speed, bool _isLocal, AvailableUpdateModes _updateMode)
        {
            switch (_updateMode)
            {
                case AvailableUpdateModes.Fixed:
                    FixedActiveEvents.Add(new ActionHolder(_obj, _targetRot,_scale, _speed, _isLocal));
                    break;
                case AvailableUpdateModes.Normal:
                    ActiveEvents.Add(new ActionHolder(_obj, _targetRot, _scale, _speed, _isLocal));
                    break;
                case AvailableUpdateModes.Late:
                    LateActiveEvents.Add(new ActionHolder(_obj, _targetRot, _scale, _speed, _isLocal));
                    break;
            }
        }
        public void AddEvent(GameObject _obj, Vector3 _targetPos, Quaternion _targetRot, float _speed, bool _isLocal, AvailableUpdateModes _updateMode)
        {
            switch (_updateMode)
            {
                case AvailableUpdateModes.Fixed:
                    FixedActiveEvents.Add(new ActionHolder(_obj, _targetPos, _targetRot, _speed, _isLocal));
                    break;
                case AvailableUpdateModes.Normal:
                    ActiveEvents.Add(new ActionHolder(_obj, _targetPos, _targetRot, _speed, _isLocal));
                    break;
                case AvailableUpdateModes.Late:
                    LateActiveEvents.Add(new ActionHolder(_obj, _targetPos, _targetRot, _speed, _isLocal));
                    break;
            }
        }
        public void AddEvent(GameObject _obj, Vector3 _targetPos, Quaternion _targetRot,Vector3 _scale, float _speed, bool _isLocal, AvailableUpdateModes _updateMode)
        {
            switch (_updateMode)
            {
                case AvailableUpdateModes.Fixed:
                    FixedActiveEvents.Add(new ActionHolder(_obj, _targetPos, _targetRot, _scale, _speed, _isLocal));
                    break;
                case AvailableUpdateModes.Normal:
                    ActiveEvents.Add(new ActionHolder(_obj, _targetPos, _targetRot, _scale, _speed, _isLocal));
                    break;
                case AvailableUpdateModes.Late:
                    LateActiveEvents.Add(new ActionHolder(_obj, _targetPos, _targetRot, _scale, _speed, _isLocal));
                    break;
            }
        }
        #endregion
    }
}
