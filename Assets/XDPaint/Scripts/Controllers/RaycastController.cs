using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XDPaint.Core;
using XDPaint.Tools.Raycast;
using XDPaint.Tools.Raycast.Base;
using XDPaint.Tools.Raycast.Data;
using XDPaint.Utils;

namespace XDPaint.Controllers
{
    public class RaycastController : Singleton<RaycastController>
    {
        [SerializeField] private bool useDepthTexture = true;
        public bool UseDepthTexture
        {
            get => useDepthTexture;
            set
            {
                useDepthTexture = value;
                if (depthToWorldConverter != null)
                {
                    depthToWorldConverter.IsEnabled = useDepthTexture;
                }
            }
        }

        private DepthToWorldConverter depthToWorldConverter;
        private readonly List<IRaycastMeshData> meshesData = new List<IRaycastMeshData>();
        private readonly Dictionary<IPaintManager, List<Triangle>> raycastResults = new Dictionary<IPaintManager, List<Triangle>>();
        private readonly Dictionary<IPaintManager, FrameIntersectionData[]> frameIntersectionData = new Dictionary<IPaintManager, FrameIntersectionData[]>();
        private readonly List<Triangle> raycastsTriangles = new List<Triangle>();

        private void Start()
        {
            InitDepthToWorldConverter();
        }

        private void OnDestroy()
        {
            depthToWorldConverter?.DoDispose();
        }

        public void InitObject(IPaintManager paintManager, Component paintComponent, Component renderComponent)
        {
            DestroyMeshData(paintManager);
            
            var raycastMeshData = meshesData.FirstOrDefault(x => x.Transform == paintComponent.transform);
            if (raycastMeshData == null)
            {
                if (renderComponent is SkinnedMeshRenderer)
                {
                    raycastMeshData = new RaycastSkinnedMeshRendererData();
                }
                else if (renderComponent is MeshRenderer)
                {
                    raycastMeshData = new RaycastMeshRendererData();
                }

                if (raycastMeshData != null)
                {
                    raycastMeshData.Init(paintComponent, renderComponent);
                    raycastMeshData.SetDepthToWorldConverter(depthToWorldConverter);
                    meshesData.Add(raycastMeshData);
                }
                else
                {
                    Debug.LogError("RaycastMeshData is null!");
                    return;
                }
            }
            
            if (paintManager.Triangles != null)
            {
                foreach (var triangle in paintManager.Triangles)
                {
                    triangle.SetRaycastMeshData(raycastMeshData, paintManager.UVChannel);
                }
            }
            raycastMeshData.AddPaintManager(paintManager);
            frameIntersectionData.Add(paintManager, new FrameIntersectionData[InputController.Instance.MaxTouchesCount]);
        }

        public Mesh GetMesh(IPaintManager paintManager)
        {
            return meshesData.Find(x => x.PaintManagers.Contains(paintManager)).Mesh;
        }

        public void DestroyMeshData(IPaintManager paintManager)
        {
            if (frameIntersectionData.ContainsKey(paintManager))
            {
                frameIntersectionData.Remove(paintManager);
            }
            
            for (var i = meshesData.Count - 1; i >= 0; i--)
            {
                if (meshesData[i].PaintManagers.Count == 1 && meshesData[i].PaintManagers.ElementAt(0) == paintManager)
                {
                    meshesData[i].DoDispose();
                    meshesData.RemoveAt(i);
                    break;
                }

                if (meshesData[i].PaintManagers.Count > 1 && meshesData[i].PaintManagers.Contains(paintManager))
                {
                    meshesData[i].RemovePaintManager(paintManager);
                    break;
                }
            }
        }
        
        public Triangle Raycast(IPaintManager sender, Ray ray, int fingerId, Vector3 screenPosition)
        {
            raycastResults.Clear();
            foreach (var meshData in meshesData)
            {
                if (meshData == null)
                    continue;
                
                foreach (var paintManager in meshData.PaintManagers)
                {
                    if (paintManager == null)
                        continue;
                    
                    var paintManagerComponent = (PaintManager)paintManager;
                    if (paintManagerComponent.gameObject.activeInHierarchy && paintManagerComponent.enabled)
                    {
                        Triangle triangle;
                        if (frameIntersectionData[paintManager][fingerId].FrameId == Time.frameCount && 
                            frameIntersectionData[paintManager][fingerId].Ray.origin == ray.origin && 
                            frameIntersectionData[paintManager][fingerId].Ray.direction == ray.direction)
                        {
                            var intersectionData = frameIntersectionData[paintManager];
                            triangle = intersectionData[fingerId].Triangle;
                        }
                        else
                        {
                            triangle = meshData.GetRaycast(paintManager, ray, fingerId, screenPosition);
                            if (triangle != null)
                            {
                                frameIntersectionData[paintManager][fingerId] = new FrameIntersectionData
                                {
                                    PaintManager = paintManager, Triangle = triangle, FrameId = Time.frameCount, Ray = ray
                                };
                            }
                        }
                        
                        if (triangle != null)
                        {
                            if (raycastResults.ContainsKey(paintManager))
                            {
                                raycastResults[paintManager].Add(triangle);
                            }
                            else
                            {
                                raycastResults.Add(paintManager, new List<Triangle> {triangle});
                            }
                        }
                    }
                }
            }
            
            return SortIntersects(sender, raycastResults);
        }

        public Triangle RaycastLocal(IPaintManager paintManager, Ray ray, int fingerId)
        {
            raycastsTriangles.Clear();
            foreach (var meshData in meshesData)
            {
                var raycast = meshData?.GetRaycast(paintManager, ray, fingerId, null, true, false);
                if (raycast != null)
                {
                    raycastsTriangles.Add(raycast);
                }
            }
            return SortIntersects(raycastsTriangles);
        }

        public Triangle NeighborsRaycast(IPaintManager sender, Triangle triangle, Ray ray)
        {
            raycastsTriangles.Clear();
            foreach (var meshData in meshesData)
            {
                var raycasts = meshData.GetNeighborsRaycasts(sender, triangle, ray);
                if (raycasts != null)
                {
                    raycastsTriangles.AddRange(raycasts);
                }
            }
            return SortIntersects(raycastsTriangles);
        }
        
        private Triangle SortIntersects(IPaintManager sender, Dictionary<IPaintManager, List<Triangle>> data)
        {
            IPaintManager paintManager = null;
            Triangle triangle = null;
            var cameraPosition = PaintController.Instance.Camera.transform.position;
            var currentDistance = float.MaxValue;

            foreach (var pair in data)
            {
                var key = pair.Key;
                var triangles = data[key];

                if (triangles.Count == 0)
                    continue;

                foreach (var t in triangles)
                {
                    var distance = Vector3.Distance(cameraPosition, t.WorldHit);
                    if (distance < currentDistance)
                    {
                        currentDistance = distance;
                        paintManager = key;
                        triangle = t;
                    }
                }
            }

            return paintManager == sender ? triangle : null;
        }

        private Triangle SortIntersects(IList<Triangle> triangles)
        {
            if (triangles.Count == 0)
                return null;
            
            if (triangles.Count == 1)
                return triangles[0];
            
            var result = triangles[0];
            var cameraPosition = PaintController.Instance.Camera.transform.position;
            var currentDistance = Vector3.Distance(cameraPosition, result.WorldHit);
            for (var i = 1; i < triangles.Count; i++)
            {
                var distance = Vector3.Distance(cameraPosition, triangles[i].WorldHit);
                if (distance < currentDistance)
                {
                    currentDistance = distance;
                    result = triangles[i];
                }
            }
            return result;
        }

        private void InitDepthToWorldConverter()
        {
            if (useDepthTexture)
            {
                if (PaintController.Instance.Camera.orthographic)
                {
                    Debug.LogWarning("Camera is orthographic, 'useDepthTexture' flag will be ignored.");
                    return;
                }
                
                var textureFloatSupports = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat);
                var renderTextureFloatSupports = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat);
                if (textureFloatSupports && renderTextureFloatSupports)
                {
                    if (PaintController.Instance.Camera.depthTextureMode == DepthTextureMode.None)
                    {
                        PaintController.Instance.Camera.depthTextureMode |= DepthTextureMode.Depth;
                    }
                    depthToWorldConverter = new DepthToWorldConverter();
                    depthToWorldConverter.Init();
                }
                else
                {
                    Debug.LogWarning("Float texture format is not supported! Set UseDepthTexture to false.");
                    useDepthTexture = false;
                }
            }
        }
    }
}