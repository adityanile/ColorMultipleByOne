using UnityEngine;

namespace AkshanshKanojia.Inputs.Mobile
{
    public class MobileInputManager : MonoBehaviour
    {
        // Handles all inputs related to touch

        // Events for touch inputs
        #region PublicFields
        public delegate void OnTapped(TouchData _data);
        public event OnTapped HasTapped;
        public delegate void OnTouchMove(TouchData _data);
        public event OnTouchMove HasMoved;
        public delegate void OnTouchHeld(TouchData _data);
        public event OnTouchHeld HasHeld;
        public delegate void OnTouchEnd(TouchData _data);
        public event OnTouchEnd HasEnded;
        public delegate void OnPinchStarted(PinchData _data);
        public event OnPinchStarted PinchStarted;
        public delegate void OnPinch(PinchData _data);
        public event OnPinch HasPinched;
        public delegate void OnPinchEnded(PinchData _data);
        public event OnPinchEnded PinchEnded;

        // Event data holder for touch inputs
        [System.Serializable]
        public class TouchData
        {
            public int TouchIndex;
            public Vector3 TouchPosition;
        }

        // Event data holder for pinch gestures
        [System.Serializable]
        public class PinchData
        {
            public float PinchDistance;
            public int NormalizedDirection;
        }

        // Mouse drag sensitivity
        [HideInInspector] public float mouseDragSensitivity = 2f;

        // Support cross-platform testing
        public bool supportCrossPlatformTesting = true;
        #endregion

        #region SerializedFields
        [SerializeField] bool supportMultiTouch = false;
        [SerializeField] float pinchDelta = 1f;
        #endregion

        #region PrivateFields
        bool isOnPc = false;
        Vector3 tempTouchPos;
        float initialPinchDistance, updatedPinchDIstance;
        #endregion

        private void Start()
        {
            // Determine the platform
            if (supportCrossPlatformTesting)
            {
#if UNITY_EDITOR
                isOnPc = true; // Running in editor
#elif PLATFORM_ANDROID
                isOnPc = false; // Running on Android device
#endif
            }
        }

        private void Update()
        {
            InputHandler();
        }

        // Checks for different types of inputs and invokes the appropriate events
        void InputHandler()
        {
            if (isOnPc)
            {
                // PC input handling
                if (Input.GetMouseButtonDown(0))
                {
                    // Tapped
                    TouchData _tempdata = new TouchData()
                    {
                        TouchPosition = Input.mousePosition,
                        TouchIndex = 0
                    };
                    tempTouchPos = Input.mousePosition;
                    HasTapped?.Invoke(_tempdata);
                }

                if (Input.GetMouseButton(0))
                {
                    TouchData _tempdata = new TouchData()
                    {
                        TouchPosition = Input.mousePosition,
                        TouchIndex = 0
                    };
                    if (Vector3.Distance(tempTouchPos, Input.mousePosition) > mouseDragSensitivity)
                    {
                        // Dragged
                        tempTouchPos = Input.mousePosition;
                        HasMoved?.Invoke(_tempdata);
                    }
                    // Held
                    HasHeld?.Invoke(_tempdata);
                }

                if (Input.GetMouseButtonUp(0))
                {
                    // Released
                    TouchData _tempdata = new TouchData()
                    {
                        TouchPosition = Input.mousePosition,
                        TouchIndex = 0
                    };
                    HasEnded?.Invoke(_tempdata);
                }
            }
            // Mobile input handling
            if (Input.touchCount > 0)
            {
                if (supportMultiTouch)
                {
                    // Multi-touch support enabled
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        ManageTouchEvents(i);
                    }
                }
                else
                {
                    // Single touch input
                    ManageTouchEvents(0);
                }
            }

            if (Input.touchCount == 2)
            {
                // Pinch gesture detected
                ManagePinchEvents();
            }
        }

        // Manages touch events based on touch index
        private void ManageTouchEvents(int _tempIndex)
        {
            TouchData _tempdata = new TouchData()
            {
                TouchPosition = Input.GetTouch(_tempIndex).position,
                TouchIndex = _tempIndex
            };
            if (Input.GetTouch(_tempIndex).phase == TouchPhase.Began)
            {
                // Tapped
                HasTapped?.Invoke(_tempdata);
            }
            if (Input.GetTouch(_tempIndex).phase == TouchPhase.Stationary)
            {
                // Held
                HasHeld?.Invoke(_tempdata);
            }
            if (Input.GetTouch(_tempIndex).phase == TouchPhase.Moved)
            {
                // Dragged
                HasMoved?.Invoke(_tempdata);
            }
            if (Input.GetTouch(_tempIndex).phase == TouchPhase.Ended)
            {
                // Stopped
                HasEnded?.Invoke(_tempdata);
            }
        }

        // Manages pinch events for pinch gestures
        private void ManagePinchEvents()
        {
            Vector2 touch0 = Input.GetTouch(0).position;
            Vector2 touch1 = Input.GetTouch(1).position;

            float pinchDistance = Vector2.Distance(touch0, touch1);

            if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(1).phase == TouchPhase.Began)
            {
                // Pinch Started
                initialPinchDistance = pinchDistance;
                updatedPinchDIstance = initialPinchDistance;
                pinchDistance = updatedPinchDIstance - initialPinchDistance;
                PinchData _tempdata = new PinchData()
                {
                    PinchDistance = pinchDistance,
                    NormalizedDirection = 0
                };
                PinchStarted?.Invoke(_tempdata);
            }

            if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
            {
                updatedPinchDIstance = pinchDistance;
                if (Mathf.Abs(initialPinchDistance - updatedPinchDIstance) < pinchDelta)
                    return;
                // Pinch
                pinchDistance = updatedPinchDIstance - initialPinchDistance;
                PinchData _tempdata = new PinchData()
                {
                    PinchDistance = pinchDistance,
                    NormalizedDirection = pinchDistance > 0 ? 1 : -1
                };
                initialPinchDistance = updatedPinchDIstance;
                HasPinched?.Invoke(_tempdata);
            }

            if (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(1).phase == TouchPhase.Ended)
            {
                // Pinch Ended
                pinchDistance = updatedPinchDIstance - initialPinchDistance;
                PinchData _tempdata = new PinchData()
                {
                    PinchDistance = pinchDistance,
                    NormalizedDirection = pinchDistance > 0 ? 1 : -1
                };
                PinchEnded?.Invoke(_tempdata);
            }
        }
    }
}