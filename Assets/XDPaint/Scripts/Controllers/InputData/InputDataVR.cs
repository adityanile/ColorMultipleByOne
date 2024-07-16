using UnityEngine;
using XDPaint.Controllers.InputData.Base;
using XDPaint.Tools.Raycast;

namespace XDPaint.Controllers.InputData
{
    public class InputDataVR : BaseInputData
    {
        private class InputData
        {
            public Ray? Ray;
            public Triangle Triangle;
            public Vector3 ScreenPosition = -Vector3.one;
        }
        
        private InputData[] inputData;
        private Transform penTransform;

        public override void Init(PaintManager paintManager, Camera camera)
        {
            base.Init(paintManager, camera);
            inputData = new InputData[InputController.Instance.MaxTouchesCount];
            for (var i = 0; i < inputData.Length; i++)
            {
                inputData[i] = new InputData();
            }
            penTransform = InputController.Instance.PenTransform;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            foreach (var data in inputData)
            {
                data.Ray = null;
                data.Triangle = null;
            }
        }

        protected override void OnHoverSuccess(int fingerId, Vector3 position, Triangle triangleData)
        {
            var data = inputData[fingerId];
            data.ScreenPosition = -Vector3.one;
            data.Ray = new Ray(penTransform.position, penTransform.forward);
            data.Triangle = RaycastController.Instance.Raycast(PaintManager, data.Ray.Value, fingerId, data.ScreenPosition);
            if (data.Triangle != null)
            { 
                data.ScreenPosition = Camera.WorldToScreenPoint(data.Triangle.WorldHit);
                base.OnHoverSuccess(fingerId, data.ScreenPosition, data.Triangle);
            }
            else
            {
                base.OnHoverFailed(fingerId);
            }
        }

        protected override void OnDownSuccess(int fingerId, Vector3 position, float pressure = 1.0f)
        {
            IsOnDownSuccess = true;
            var data = inputData[fingerId];
            if (data.Ray == null)
            {
                data.Ray = new Ray(penTransform.position, penTransform.forward);
            }
            
            if (data.Triangle == null)
            {
                data.Triangle = RaycastController.Instance.Raycast(PaintManager, data.Ray.Value, fingerId, data.ScreenPosition);
            }
            
            if (data.Triangle != null)
            {
                data.ScreenPosition = Camera.WorldToScreenPoint(data.Triangle.WorldHit);
            }
            
            OnDownSuccessInvoke(fingerId, data.ScreenPosition, pressure, data.Triangle);
        }

        public override void OnPress(int fingerId, Vector3 position, float pressure = 1.0f)
        {
            if (!PaintManager.PaintObject.ProcessInput || !PaintManager.enabled)
                return;
            
            if (IsOnDownSuccess)
            {
                var data = inputData[fingerId];
                if (data.Ray == null)
                {
                    data.Ray = new Ray(penTransform.position, penTransform.forward);
                }
                
                if (data.Triangle == null)
                {
                    data.Triangle = RaycastController.Instance.Raycast(PaintManager, data.Ray.Value, fingerId, data.ScreenPosition);
                }
                
                if (data.Triangle != null)
                {
                    data.ScreenPosition = Camera.WorldToScreenPoint(data.Triangle.WorldHit);
                }
                
                OnPressInvoke(fingerId, data.ScreenPosition, pressure, data.Triangle);
            }
        }
    }
}