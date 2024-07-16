// #define XDPAINT_VR_ENABLE

#if XDPAINT_VR_ENABLE
using System.Collections.Generic;
using UnityEngine.XR;
using InputDevice = UnityEngine.XR.InputDevice;
using CommonUsages = UnityEngine.XR.CommonUsages;
#endif

using System;
using UnityEngine;
using XDPaint.Core;
using XDPaint.Tools;
using XDPaint.Utils;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
#endif

namespace XDPaint.Controllers
{
    public class InputController : Singleton<InputController>
    {
        [Header("General")]
        [SerializeField, Min(1)] private int maxTouchesCount = 10;
        
        [Header("Ignore Raycasts Settings")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject[] ignoreForRaycasts;
        
        [Header("VR Settings")]
        public Transform PenTransform;

        public event Action OnUpdate;
        public event Action<int, Vector3> OnMouseHover;
        public event Action<int, Vector3, float> OnMouseDown;
        public event Action<int, Vector3, float> OnMouseButton;
        public event Action<int, Vector3> OnMouseUp;

        public int MaxTouchesCount => maxTouchesCount;
        public Canvas Canvas => canvas;
        public GameObject[] IgnoreForRaycasts => ignoreForRaycasts;

        private bool isVRMode;
        private bool[] isBegan;
        
#if XDPAINT_VR_ENABLE
        private List<InputDevice> leftHandedControllers;
        private bool isPressed;
#endif
        
        void Start()
        {
            isBegan = new bool[maxTouchesCount];
            isVRMode = Settings.Instance.IsVRMode;
            InitVR();
#if ENABLE_INPUT_SYSTEM
            if (!EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Enable();
            }
#endif
        }

        private void InitVR()
        {
#if XDPAINT_VR_ENABLE
            leftHandedControllers = new List<InputDevice>();
            var desiredCharacteristics = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
            InputDevices.GetDevicesWithCharacteristics(desiredCharacteristics, leftHandedControllers);
#endif
        }
        
        void Update()
        {
            //VR
            if (isVRMode)
            {
#if XDPAINT_VR_ENABLE
                OnUpdate?.Invoke();

                OnMouseHover?.Invoke(0, Vector3.zero);

                //VR input
                //next line can be changed for VR device input
                if (leftHandedControllers.Count > 0 && leftHandedControllers[0].TryGetFeatureValue(CommonUsages.triggerButton, out var triggerValue) && triggerValue)
                {
                    if (!isPressed)
                    {
                        isPressed = true;
                        OnMouseDown?.Invoke(0, Vector3.zero, 1f);
                    }
                    else
                    {
                        OnMouseButton?.Invoke(0, Vector3.zero, 1f);
                    }
                }
                else if (isPressed)
                {
                    isPressed = false;
                    OnMouseUp?.Invoke(0, Vector3.zero);
                }
#endif
            }
            else
            {
                //Pen / Touch / Mouse
#if ENABLE_INPUT_SYSTEM
                if (Pen.current != null && (Pen.current.press.isPressed || Pen.current.press.wasReleasedThisFrame))
                {
                    if (Pen.current.press.isPressed)
                    {
                        OnUpdate?.Invoke();

                        var pressure = Settings.Instance.PressureEnabled ? Pen.current.pressure.ReadValue() : 1f;
                        var position = Pen.current.position.ReadValue();

                        if (Pen.current.press.wasPressedThisFrame)
                        {
                            OnMouseDown?.Invoke(0, position, pressure);
                        }

                        if (!Pen.current.press.wasPressedThisFrame)
                        {
                            OnMouseButton?.Invoke(0, position, pressure);
                        }
                    }
                    else if (Pen.current.press.wasReleasedThisFrame)
                    {
                        var position = Pen.current.position.ReadValue();
                        OnMouseUp?.Invoke(0, position);
                    }
                }
                else if (Touchscreen.current != null && Touch.activeTouches.Count > 0)
                {
                    foreach (var touch in Touch.activeTouches)
                    {
                        var fingerId = touch.finger.index;
                        if (fingerId >= Constants.Input.MaxTouchCount)
                            continue;

                        OnUpdate?.Invoke();

                        var pressure = Settings.Instance.PressureEnabled ? touch.pressure : 1f;

                        if (touch.phase == TouchPhase.Began && !isBegan[fingerId])
                        {
                            isBegan[fingerId] = true;
                            OnMouseDown?.Invoke(fingerId, touch.screenPosition, pressure);
                        }

                        if (isBegan[fingerId])
                        {
                            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                            {
                                OnMouseButton?.Invoke(fingerId, touch.screenPosition, pressure);
                            }

                            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                            {
                                isBegan[fingerId] = false;
                                OnMouseUp?.Invoke(fingerId, touch.screenPosition);
                            }
                        }
                    }
                }
                else if (Mouse.current != null)
                {
                    OnUpdate?.Invoke();

                    var mousePosition = Mouse.current.position.ReadValue();
                    OnMouseHover?.Invoke(0, mousePosition);

                    if (Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        OnMouseDown?.Invoke(0, mousePosition, 1f);
                    }

                    if (Mouse.current.leftButton.isPressed)
                    {
                        OnMouseButton?.Invoke(0, mousePosition, 1f);
                    }

                    if (Mouse.current.leftButton.wasReleasedThisFrame)
                    {
                        OnMouseUp?.Invoke(0, mousePosition);
                    }
                }
#elif ENABLE_LEGACY_INPUT_MANAGER
                //Touch / Mouse
                if (Input.touchSupported && Input.touchCount > 0)
                {
                    foreach (var touch in Input.touches)
                    {
                        var fingerId = touch.fingerId;
                        if (fingerId >= maxTouchesCount)
                            continue;
                        
                        OnUpdate?.Invoke();

                        var pressure = Settings.Instance.PressureEnabled ? touch.pressure : 1f;
                        
                        if (touch.phase == TouchPhase.Began && !isBegan[fingerId])
                        {
                            isBegan[fingerId] = true;
                            OnMouseDown?.Invoke(fingerId, touch.position, pressure);
                        }

                        if (touch.fingerId == fingerId)
                        {
                            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                            {
                                OnMouseButton?.Invoke(fingerId, touch.position, pressure);
                            }
                            
                            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                            {
                                isBegan[fingerId] = false;
                                OnMouseUp?.Invoke(fingerId, touch.position);
                            }
                        }
                    }
                }
                else
                {
                    OnUpdate?.Invoke();

                    OnMouseHover?.Invoke(0, Input.mousePosition);

                    if (Input.GetMouseButtonDown(0))
                    {
                        OnMouseDown?.Invoke(0, Input.mousePosition, 1f);
                    }

                    if (Input.GetMouseButton(0))
                    {
                        OnMouseButton?.Invoke(0, Input.mousePosition, 1f);
                    }

                    if (Input.GetMouseButtonUp(0))
                    {
                        OnMouseUp?.Invoke(0, Input.mousePosition);
                    }
                }
#endif
            }
        }
    }
}