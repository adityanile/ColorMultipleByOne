using UnityEngine;
using XDPaint.Controllers.InputData.Base;
using XDPaint.Tools.Raycast;

namespace XDPaint.Controllers.InputData
{
    public class InputDataMesh : BaseInputData
    {
        private class InputData
        {
            public Ray? Ray;
            public Triangle Triangle;
        }

        private InputData[] inputData;

        public override void Init(PaintManager paintManagerInstance, Camera camera)
        {
            base.Init(paintManagerInstance, camera);
            inputData = new InputData[InputController.Instance.MaxTouchesCount];
            for (var i = 0; i < inputData.Length; i++)
            {
                inputData[i] = new InputData();
            }
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
            data.Ray = Camera.ScreenPointToRay(position);
            data.Triangle = RaycastController.Instance.Raycast(PaintManager, data.Ray.Value, fingerId, position);
            if (data.Triangle != null)
            {
                base.OnHoverSuccess(fingerId, position, data.Triangle);
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
            if (data.Ray == null || IsShouldRaycast)
            {
                data.Ray = Camera.ScreenPointToRay(position);
            }
            
            if (data.Triangle == null || IsShouldRaycast)
            {
                data.Triangle = RaycastController.Instance.Raycast(PaintManager, data.Ray.Value, fingerId, position);
            }
            
            IsShouldRaycast = false;
            OnDownSuccessInvoke(fingerId, position, pressure, data.Triangle);
        }

        public override void OnPress(int fingerId, Vector3 position, float pressure = 1.0f)
        {
            if (!PaintManager.PaintObject.ProcessInput || !PaintManager.enabled)
                return;

            if (IsOnDownSuccess)
            {
                var data = inputData[fingerId];
                if (data.Ray == null || IsShouldRaycast)
                {
                    data.Ray = Camera.ScreenPointToRay(position);
                }
                
                if (data.Triangle == null || IsShouldRaycast)
                {
                    data.Triangle = RaycastController.Instance.Raycast(PaintManager, data.Ray.Value, fingerId, position);
                }
                
                IsShouldRaycast = false;
                OnPressInvoke(fingerId, position, pressure, data.Triangle);
            }
        }
    }
}