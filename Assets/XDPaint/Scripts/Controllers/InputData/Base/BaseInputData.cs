using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XDPaint.Tools;
using XDPaint.Tools.Raycast;
using IDisposable = XDPaint.Core.IDisposable;
using Object = UnityEngine.Object;

namespace XDPaint.Controllers.InputData.Base
{
    public abstract class BaseInputData : IDisposable
    {
        public event Action<int, Vector3, Triangle> OnHoverSuccessHandler;
        public event Action<int, Vector3, Triangle> OnHoverFailedHandler;
        public event Action<int, Vector3, float, Triangle> OnDownHandler;
        public event Action<int, Vector3, float, Triangle> OnPressHandler;
        public event Action<int, Vector3> OnUpHandler;

        protected Camera Camera;
        protected bool IsOnDownSuccess;
        protected bool IsShouldRaycast;
        protected PaintManager PaintManager;
        private List<CanvasGraphicRaycaster> raycasters;
        private Dictionary<CanvasGraphicRaycaster, List<RaycastResult>> raycastResults;
        private bool canHover = true;
        
        public virtual void Init(PaintManager paintManagerInstance, Camera camera)
        {
            Camera = camera;
            PaintManager = paintManagerInstance;
            raycasters = new List<CanvasGraphicRaycaster>();
            raycastResults = new Dictionary<CanvasGraphicRaycaster, List<RaycastResult>>();
            if (Settings.Instance.CheckCanvasRaycasts)
            {
                if (PaintManager.ObjectForPainting.TryGetComponent<RawImage>(out var rawImage) && rawImage.canvas != null)
                {
                    if (!rawImage.canvas.TryGetComponent<CanvasGraphicRaycaster>(out var graphicRaycaster))
                    {
                        graphicRaycaster = rawImage.canvas.gameObject.AddComponent<CanvasGraphicRaycaster>();
                    }
                    if (!raycasters.Contains(graphicRaycaster))
                    {
                        raycasters.Add(graphicRaycaster);
                    }
                }

                var canvas = InputController.Instance.Canvas;
                if (canvas == null)
                {
                    canvas = Object.FindObjectOfType<Canvas>();
                }

                if (canvas != null)
                {
                    if (!canvas.TryGetComponent<CanvasGraphicRaycaster>(out var graphicRaycaster))
                    {
                        graphicRaycaster = canvas.gameObject.AddComponent<CanvasGraphicRaycaster>();
                    }
                    if (!raycasters.Contains(graphicRaycaster))
                    {
                        raycasters.Add(graphicRaycaster);
                    }
                }
            }
        }
        
        public void DoDispose()
        {
            raycasters.Clear();
            raycastResults.Clear();
        }

        public virtual void OnUpdate()
        {
            
        }

        public void OnHover(int fingerId, Vector3 position)
        {
            if (!PaintManager.PaintObject.ProcessInput || !PaintManager.enabled)
            {
                OnHoverFailed(fingerId);
                return;
            }
            
            if (Settings.Instance.CheckCanvasRaycasts && raycasters.Count > 0)
            {
                raycastResults.Clear();
                foreach (var raycaster in raycasters)
                {
                    var result = raycaster.GetRaycasts(position);
                    if (result != null)
                    {
                        raycastResults.Add(raycaster, result);
                    }
                }

                if (canHover && (raycastResults.Count == 0 || CheckRaycasts()))
                {
                    OnHoverSuccess(fingerId, position, null);
                }
                else
                {
                    OnHoverFailed(fingerId);
                }
            }
            else
            {
                OnHoverSuccess(fingerId, position, null);
            }
        }
        
        protected virtual void OnHoverSuccess(int fingerId, Vector3 position, Triangle triangle)
        {
            OnHoverSuccessHandler?.Invoke(fingerId, position, triangle);
        }
        
        protected virtual void OnHoverFailed(int fingerId)
        {
            OnHoverFailedHandler?.Invoke(fingerId, Vector4.zero, null);
        }

        public void OnDown(int fingerId, Vector3 position, float pressure = 1.0f)
        {
            if (!PaintManager.PaintObject.ProcessInput || !PaintManager.enabled)
            {
                OnDownFailed(fingerId, position, pressure);
                return;
            }

            if (Settings.Instance.CheckCanvasRaycasts && raycasters.Count > 0)
            {
                raycastResults.Clear();
                foreach (var raycaster in raycasters)
                {
                    var result = raycaster.GetRaycasts(position);
                    if (result != null)
                    {
                        raycastResults.Add(raycaster, result);
                    }
                }
                if (raycastResults.Count == 0 || CheckRaycasts())
                {
                    OnDownSuccess(fingerId, position, pressure);
                }
                else
                {
                    canHover = false;
                    OnDownFailed(fingerId, position, pressure);
                }
            }
            else
            {
                OnDownSuccess(fingerId, position, pressure);
            }
        }
        
        protected virtual void OnDownSuccess(int fingerId, Vector3 position, float pressure = 1.0f)
        {
            OnDownSuccessInvoke(fingerId, position, pressure);
            IsOnDownSuccess = true;
        }

        protected virtual void OnDownSuccessInvoke(int fingerId, Vector3 position, float pressure = 1.0f, Triangle triangle = null)
        {
            OnDownHandler?.Invoke(fingerId, position, pressure, triangle);
        }
        
        protected virtual void OnDownFailed(int fingerId, Vector3 position, float pressure = 1.0f)
        {
            IsOnDownSuccess = false;
        }

        public virtual void OnPress(int fingerId, Vector3 position, float pressure = 1.0f)
        {
            if (!PaintManager.PaintObject.ProcessInput || !PaintManager.enabled)
                return;

            if (IsOnDownSuccess)
            {
                OnPressInvoke(fingerId, position, pressure);
            }
        }

        protected void OnPressInvoke(int fingerId, Vector3 position, float pressure = 1.0f, Triangle triangle = null)
        {
            OnPressHandler?.Invoke(fingerId, position, pressure, triangle);
        }

        public virtual void OnUp(int fingerId, Vector3 position)
        {
            if (!PaintManager.PaintObject.ProcessInput || !PaintManager.enabled)
                return;

            if (IsOnDownSuccess)
            {
                OnUpHandler?.Invoke(fingerId, position);
            }
            canHover = true;
        }

        private bool CheckRaycasts()
        {
            var result = true;
            if (raycastResults.Count > 0)
            {
                var ignoreRaycasts = InputController.Instance.IgnoreForRaycasts;
                foreach (var raycaster in raycastResults.Keys)
                {
                    if (raycastResults[raycaster].Count > 0)
                    {
                        var raycast = raycastResults[raycaster][0];
                        if (raycast.gameObject == PaintManager.ObjectForPainting && PaintManager.Initialized)
                        {
                            continue;
                        }

                        if (!ignoreRaycasts.Contains(raycast.gameObject))
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }
            return result;
        }
    }
}