using UnityEngine;
using XDPaint.Controllers;
using XDPaint.Core;
using XDPaint.Utils;

namespace XDPaint.Tools.Raycast
{
    public class DepthToWorldConverter : IDisposable
    {
        public bool IsEnabled = true;
        
        private CommandBufferBuilder commandBuffer;
        private RenderTexture renderTexture;
        private Mesh quadMesh;
        private Material material;
        private Texture2D texture;
        private int frameId;
        private Vector4 position;

        public void Init()
        {
            DoDispose();
            commandBuffer = new CommandBufferBuilder();
            quadMesh = MeshGenerator.GenerateQuad(Vector3.one, Vector3.zero);
            material = new Material(Settings.Instance.DepthToWorldPositionShader);
            renderTexture = RenderTextureFactory.CreateRenderTexture(1, 1, 0, RenderTextureFormat.ARGBFloat);
            texture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false, true);
        }
        
        public void DoDispose()
        {
            commandBuffer?.Release();
            if (quadMesh != null)
            {
                Object.Destroy(quadMesh);
            }
            if (material != null)
            {
                Object.Destroy(material);
            }
            if (renderTexture != null)
            {
                renderTexture.ReleaseTexture();
            }
            if (texture != null)
            {
                Object.Destroy(texture);
            }
        }
        
        public Vector4 GetPosition(Vector2 screenPosition)
        {
            if (frameId == Time.frameCount)
                return position;

            frameId = Time.frameCount;
            var mainCamera = PaintController.Instance.Camera;
            var uv = mainCamera.ScreenToViewportPoint(screenPosition);
            material.SetVector(Constants.DepthToWorldPositionShader.ScreenVector, uv);
            var projectionMatrix = GL.GetGPUProjectionMatrix(mainCamera.projectionMatrix, false);
            var inverseViewProjectionMatrix = (projectionMatrix * mainCamera.worldToCameraMatrix).inverse;
            material.SetMatrix(Constants.DepthToWorldPositionShader.InverseViewProjectionMatrix, inverseViewProjectionMatrix);
            commandBuffer.LoadOrtho().Clear().SetRenderTarget(renderTexture).DrawMesh(quadMesh, material).Execute();
            var prevRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            texture.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
            texture.Apply();
            RenderTexture.active = prevRenderTexture;
            position = texture.GetPixel(0, 0);
            return position;
        }
    }
}