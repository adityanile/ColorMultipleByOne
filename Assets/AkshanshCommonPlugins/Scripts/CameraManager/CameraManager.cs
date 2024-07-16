using UnityEngine;
namespace AkshanshKanojia.Controllers.CameraController
{
    public class CameraManager : MonoBehaviour
    {
        //this class manage transforms based on inputs recived.
        #region SerializeFields

        #endregion

        #region PrivateFields
        Vector3 curtTarget;
        bool isTracking = false, isRotating = false;
        #endregion

        #region PublicFields
        //action properties
        public bool TrackObject = false;

        //behaviour properties
        public bool IsPaused = false;
        [HideInInspector] public GameObject CurrentTargetObj,CurrentRotObject;
        [HideInInspector]public bool SeprateRotationTarget;
        [HideInInspector] public enum AvailableBehaviours { Track, Rotate, TrackAndRotate }
        [HideInInspector] public AvailableBehaviours CameraBehaviour;
        //movement properties
        [HideInInspector] public enum AvailableTrackAxis { X, Y, Z, XY, XZ, YZ, XYZ }
        [HideInInspector] public AvailableTrackAxis curtTrackAxis;
        [HideInInspector] public float moveSpeed = 1.5f, trackBeginDistance = 1f, trackStopDistance = 0.2f;

        //rotation properties
        [HideInInspector] public float RotSpeed = 2f;
        public enum RotationOptions { X, Y, Z, XY, XZ, YZ, XYZ }
        [HideInInspector] public RotationOptions RotationAxis;
        [HideInInspector] public bool ClampRotation = false;
        [HideInInspector] public Vector3 MinRotationClamp, MaxRotationClamp;

        //events
        public delegate void HasReachedDestination();
        public event HasReachedDestination OnTrackFinish;//use this to assign listeners when required. Add a custom class if needed for required inputs on reach
        public delegate void HasFinishedRotation();
        public event HasFinishedRotation OnRotationFinished;
        #endregion

        private void Update()
        {
            if (!IsPaused && TrackObject)
            {
                TargetManager();
            }
        }

        private void LateUpdate()
        {
            if (!IsPaused)
            {
                if (isTracking&&TrackObject)
                    TrackTarget();
                if (isRotating)
                    RotationManager();
            }
        }
        void TargetManager()
        {
            Vector3 _tempDir = CurrentTargetObj.transform.position - transform.position;
            switch (curtTrackAxis)
            {
                case AvailableTrackAxis.X:
                    _tempDir.y = 0;
                    _tempDir.z = 0;
                    break;
                case AvailableTrackAxis.Y:
                    _tempDir.x = 0;
                    _tempDir.z = 0;
                    break;
                case AvailableTrackAxis.Z:
                    _tempDir.x = 0;
                    _tempDir.y = 0;
                    break;
                case AvailableTrackAxis.XY:
                    _tempDir.z = 0;
                    break;
                case AvailableTrackAxis.XZ:
                    _tempDir.y = 0;
                    break;
                case AvailableTrackAxis.YZ:
                    _tempDir.x = 0;
                    break;
                default:
                    break;
            }
            if (_tempDir.magnitude > trackBeginDistance)
            {
                SetTarget(CurrentTargetObj.transform.position);
            }
            if (CameraBehaviour == AvailableBehaviours.Rotate || CameraBehaviour == AvailableBehaviours.TrackAndRotate)
            {
                isRotating = true;
            }
        }
        void TrackTarget()
        {
            Vector3 _tempDir = curtTarget - transform.position;
            if (CameraBehaviour == AvailableBehaviours.Track || CameraBehaviour == AvailableBehaviours.TrackAndRotate)
            {
                switch (curtTrackAxis)
                {
                    case AvailableTrackAxis.X:
                        _tempDir.y = 0;
                        _tempDir.z = 0;
                        break;
                    case AvailableTrackAxis.Y:
                        _tempDir.x = 0;
                        _tempDir.z = 0;
                        break;
                    case AvailableTrackAxis.Z:
                        _tempDir.x = 0;
                        _tempDir.y = 0;
                        break;
                    case AvailableTrackAxis.XY:
                        _tempDir.z = 0;
                        break;
                    case AvailableTrackAxis.XZ:
                        _tempDir.y = 0;
                        break;
                    case AvailableTrackAxis.YZ:
                        _tempDir.x = 0;
                        break;
                    default:
                        break;
                }
                transform.position += moveSpeed * Time.deltaTime * _tempDir.normalized;
                if (_tempDir.magnitude < trackStopDistance)
                {
                    isTracking = false;
                    OnTrackFinish?.Invoke();
                }
            }
        }

        private void RotationManager()
        {
            GameObject _tempObj = (SeprateRotationTarget) ? CurrentRotObject : 
                CurrentTargetObj;
            _tempObj = (_tempObj) ? _tempObj : CurrentTargetObj;//if user forget to assign rotation property
            Vector3 _tempDir = _tempObj.transform.position - transform.position;
            transform.rotation = Quaternion.Slerp(Quaternion.Euler(transform.eulerAngles), Quaternion.LookRotation(_tempDir),
                Time.deltaTime * RotSpeed);
            Vector3 _tempRotAxis = transform.eulerAngles;
            if (ClampRotation)
            {
                _tempRotAxis.x = Mathf.Clamp(_tempRotAxis.x, MinRotationClamp.x, MaxRotationClamp.x);
                _tempRotAxis.y = Mathf.Clamp(_tempRotAxis.y, MinRotationClamp.y, MaxRotationClamp.y);
                _tempRotAxis.z = Mathf.Clamp(_tempRotAxis.z, MinRotationClamp.z, MaxRotationClamp.z);
            }
            switch (RotationAxis)
            {
                case RotationOptions.X:
                    _tempRotAxis.y = 0;
                    _tempRotAxis.z = 0;
                    break;
                case RotationOptions.Y:
                    _tempRotAxis.x = 0;
                    _tempRotAxis.z = 0;
                    break;
                case RotationOptions.Z:
                    _tempRotAxis.x = 0;
                    _tempRotAxis.y = 0;
                    break;
                case RotationOptions.XY:
                    _tempRotAxis.z = 0;
                    break;
                case RotationOptions.XZ:
                    _tempRotAxis.y = 0;
                    break;
                case RotationOptions.YZ:
                    _tempRotAxis.x = 0;
                    break;
                default:
                    break;
            }
            transform.eulerAngles = _tempRotAxis;
            if (Vector3.Angle(transform.forward, _tempDir) < 2)
            {
                isRotating = false;
                OnRotationFinished?.Invoke();
            }
        }

        public void SetTarget(Vector3 _dir)
        {
            curtTarget = _dir;
            isTracking = true;
        }
    }
}
