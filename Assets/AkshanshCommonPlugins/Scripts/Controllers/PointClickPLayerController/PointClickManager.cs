using UnityEngine;

namespace AkshanshKanojia.Controllers.PointClick
{
    public class PointClickManager : MonoBehaviour
    {
        //this class manage player transforms based on inputs recived. Use this to extend for animators and collisions
        #region SerializeFields
        [SerializeField] float moveSpeed = 1.5f;
        public enum AvailableTrackAaxis { X, Y, Z, XY, XZ, YZ, XYZ }
        public AvailableTrackAaxis curtTrackAxis;
        [SerializeField, Tooltip("Detrmine if can change direction while moving")] bool overrideTargetWhileMoving = true;
        #endregion

        #region PrivateFields
        Vector3 curtTarget, tempRotationTarget;
        bool isTracking = false;
        #endregion

        #region PublicFields
        public bool RotatePlayerWhileMoving;
        [HideInInspector] public float RotSpeed = 2f;
        public enum RotationOptions { X, Y, Z, XY, XZ, YZ, XYZ }
        [HideInInspector] public GameObject RotObj;
        [HideInInspector] public RotationOptions RotationAxis;
        [HideInInspector] public bool ClampRotation = false, ClampLocation = false, SeprateRotBody = false;
        [HideInInspector] public Vector3 MinRotationClamp, MaxRotationClamp, MinPosClamp, MaxPosClamp;

        //events
        public delegate void HasReachedDestination();
        public event HasReachedDestination OnReached;//use this to assign listeners when required. Add a custom class if needed for required inputs on reach
        #endregion

        private void FixedUpdate()
        {
            if (isTracking)
            {
                TrackTarget();
            }
        }
        void TrackTarget()
        {
            Vector3 _tempDir = curtTarget - transform.position;
            switch (curtTrackAxis)
            {
                case AvailableTrackAaxis.X:
                    _tempDir.y = 0;
                    _tempDir.z = 0;
                    break;
                case AvailableTrackAaxis.Y:
                    _tempDir.x = 0;
                    _tempDir.z = 0;
                    break;
                case AvailableTrackAaxis.Z:
                    _tempDir.x = 0;
                    _tempDir.y = 0;
                    break;
                case AvailableTrackAaxis.XY:
                    _tempDir.z = 0;
                    break;
                case AvailableTrackAaxis.XZ:
                    _tempDir.y = 0;
                    break;
                case AvailableTrackAaxis.YZ:
                    _tempDir.x = 0;
                    break;
                default:
                    break;
            }
            transform.position += moveSpeed * Time.deltaTime * _tempDir.normalized;
            if (ClampLocation)
            {
                Vector3 _tempPlayerPos = transform.position;
                switch (curtTrackAxis)
                {
                    case AvailableTrackAaxis.X:
                        _tempPlayerPos.x = CustomClamp(_tempPlayerPos.x, MinPosClamp.x, MaxPosClamp.x, 0);
                        break;
                    case AvailableTrackAaxis.Y:
                        _tempPlayerPos.y = CustomClamp(_tempPlayerPos.y, MinPosClamp.y, MaxPosClamp.y, 1);
                        break;
                    case AvailableTrackAaxis.Z:
                        _tempPlayerPos.z = CustomClamp(_tempPlayerPos.z, MinPosClamp.z, MaxPosClamp.z, 2);
                        break;
                    case AvailableTrackAaxis.XY:
                        _tempPlayerPos.x = CustomClamp(_tempPlayerPos.x, MinPosClamp.x, MaxPosClamp.x, 0);
                        _tempPlayerPos.y = CustomClamp(_tempPlayerPos.y, MinPosClamp.y, MaxPosClamp.y, 1);
                        break;
                    case AvailableTrackAaxis.XZ:
                        _tempPlayerPos.x = CustomClamp(_tempPlayerPos.x, MinPosClamp.x, MaxPosClamp.x, 0);
                        _tempPlayerPos.z = CustomClamp(_tempPlayerPos.z, MinPosClamp.z, MaxPosClamp.z, 2);
                        break;
                    case AvailableTrackAaxis.YZ:
                        _tempPlayerPos.y = CustomClamp(_tempPlayerPos.y, MinPosClamp.y, MaxPosClamp.y, 1);
                        _tempPlayerPos.z = CustomClamp(_tempPlayerPos.z, MinPosClamp.z, MaxPosClamp.z, 2);
                        break;
                    default:
                        _tempPlayerPos.x = CustomClamp(_tempPlayerPos.x, MinPosClamp.x, MaxPosClamp.x, 0);
                        _tempPlayerPos.y = CustomClamp(_tempPlayerPos.y, MinPosClamp.y, MaxPosClamp.y, 1);
                        _tempPlayerPos.z = CustomClamp(_tempPlayerPos.z, MinPosClamp.z, MaxPosClamp.z, 2);
                        break;
                }
                transform.position = _tempPlayerPos;
            }
            if (_tempDir.magnitude < 0.2f)
            {
                isTracking = false;
                OnReached?.Invoke();
            }

            //rotation
            if (RotatePlayerWhileMoving)
                RotationManager(_tempDir);
        }

        void ClampedValue(int _axisIndex)
        {
            switch (_axisIndex)
            {
                case 0:
                    curtTarget.x = transform.position.x;
                    break;
                case 1:
                    curtTarget.y = transform.position.y;
                    break;
                case 2:
                    curtTarget.z = transform.position.z;
                    break;
                default:
                    break;
            }
        }
        float CustomClamp(float _pos, float _min, float _max, int _axisIndex)//for getting event when clamped
        {
            if (_pos < _min)
            {
                ClampedValue(_axisIndex);
                return _min;
            }
            if (_pos > _max)
            {
                ClampedValue(_axisIndex);
                return _max;
            }
            return _pos;
        }

        private void RotationManager(Vector3 _tempDir)
        {
            GameObject _tempRotTarget = (SeprateRotBody) ? RotObj : gameObject;
            if (!_tempRotTarget && SeprateRotBody)
            {
                print("No rotation body assigned for " + gameObject.name + " Point Click Manager! Using self as target.");
                _tempRotTarget = gameObject;
            }

            _tempRotTarget.transform.rotation = Quaternion.Slerp(Quaternion.Euler(_tempRotTarget.transform.eulerAngles), Quaternion.LookRotation(_tempDir),
                Time.deltaTime * RotSpeed);
            Vector3 _tempRotAxis = _tempRotTarget.transform.eulerAngles;
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
            _tempRotTarget.transform.eulerAngles = _tempRotAxis;
        }

        public void SetTarget(Vector3 _dir)
        {
            if (!overrideTargetWhileMoving && isTracking)
                return;
            curtTarget = _dir;
            isTracking = true;
        }
    }
}
