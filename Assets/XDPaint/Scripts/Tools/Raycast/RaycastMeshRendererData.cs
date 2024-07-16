using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using XDPaint.Controllers;
using XDPaint.Core;
using XDPaint.Tools.Raycast.Base;
using XDPaint.Tools.Raycast.Data;

namespace XDPaint.Tools.Raycast
{
    public class RaycastMeshRendererData : BaseRaycastMeshData
    {
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        
        public override void Init(Component paintComponent, Component rendererComponent)
        {
            base.Init(paintComponent, rendererComponent);
            meshRenderer = rendererComponent as MeshRenderer;
            meshFilter = paintComponent as MeshFilter;
        }

        public override void AddPaintManager(IPaintManager paintManager)
        {
            base.AddPaintManager(paintManager);
            var mesh = meshFilter.sharedMesh;
            InitUVs(paintManager, mesh);
            InitTriangles(paintManager, mesh);
        }

        public override IEnumerable<Triangle> GetNeighborsRaycasts(IPaintManager sender, Triangle currentTriangle, Ray ray)
        {
            var results = new List<Triangle>();
            foreach (var paintManager in PaintManagers)
            {
                if (paintManager != sender)
                    continue;

                var intersects = new List<Triangle>();
                if (Settings.Instance.UseJobsForRaycasts)
                {
                    var paintManagerComponent = (PaintManager)paintManager;
                    var trianglesArray = new TriangleData[currentTriangle.N.Count];
                    if (paintManagerComponent.gameObject.activeInHierarchy && paintManagerComponent.enabled)
                    {
                        for (var i = 0; i < currentTriangle.N.Count; i++)
                        {
                            var index = currentTriangle.N[i];
                            var triangle = paintManager.Triangles[index];
                            var simplified = new TriangleData
                            {
                                Id = triangle.Id,
                                Position0 = triangle.Position0,
                                Position1 = triangle.Position1,
                                Position2 = triangle.Position2,
                                UV0 = triangle.UV0,
                                UV1 = triangle.UV1,
                                UV2 = triangle.UV2
                            };
                            trianglesArray[i] = simplified;
                        }
                    }

                    var triangles = new NativeArray<TriangleData>(trianglesArray, Allocator.TempJob);
                    var hits = new NativeArray<RaycastTriangleHit>(trianglesArray.Length, Allocator.TempJob);
                    var hitsCount = new NativeArray<int>(1, Allocator.TempJob);
                    hitsCount[0] = 0;

                    var job = new RaycastJob
                    {
                        Triangles = triangles,
                        Hits = hits,
                        HitsCount = hitsCount,
                        RayOrigin = ray.origin,
                        RayDirection = ray.direction
                    };
                    job.Run();

                    var raycastsCount = job.HitsCount[0];
                    for (var i = 0; i < raycastsCount; i++)
                    {
                        var hit = job.Hits[i];
                        var triangleId = hit.Id;
                        var triangle = paintManagerComponent.Triangles[triangleId];
                        triangle.UVHit = hit.UV;
                        intersects.Add(triangle);
                    }

                    triangles.Dispose();
                    hits.Dispose();
                    hitsCount.Dispose();
                }
                else
                {
                    foreach (var triangleId in currentTriangle.N)
                    {
                        var triangle = paintManager.Triangles[triangleId];
                        var isIntersected = IsIntersected(triangle, ray, false);
                        if (isIntersected)
                        {
                            intersects.Add(triangle);
                        }
                    }
                }

                if (intersects.Count > 0)
                {
                    var sortedIntersect = SortIntersects(intersects);
                    results.Add(sortedIntersect);
                }
            }
            return results;
        }

        public override Triangle GetRaycast(IPaintManager sender, Ray ray, int fingerId, Vector3? screenPosition = null, bool useWorld = true, bool useCache = true)
        {
            var frameIntersectionData = FrameIntersectionData[fingerId];
            if (useCache && frameIntersectionData.FrameId == Time.frameCount && 
                frameIntersectionData.Ray.origin == ray.origin && frameIntersectionData.Ray.direction == ray.direction)
            {
                if (frameIntersectionData.PaintManager == sender)
                {
                    return frameIntersectionData.Triangle;
                }
                return null;
            }

            var rayTransformed = new Ray(ray.origin, ray.direction);
            if (useWorld)
            {
                var worldBounds = meshRenderer.bounds;
                worldBounds.Expand(0.0001f);
                var boundsIntersect = worldBounds.IntersectRay(ray);
                if (!boundsIntersect || !IsBoundsInDepth(worldBounds, screenPosition))
                    return null;
                
                var origin = Transform.InverseTransformPoint(ray.origin);
                var direction = Transform.InverseTransformVector(ray.direction);
                rayTransformed = new Ray(origin, direction);
            }
            
            var raycasts = new Dictionary<IPaintManager, Triangle>();
            foreach (var paintManager in PaintManagers)
            {
                var paintManagerComponent = (PaintManager)paintManager;
                if (paintManagerComponent.gameObject.activeInHierarchy && paintManagerComponent.enabled)
                {
                    var intersects = new List<Triangle>();
                    if (Settings.Instance.UseJobsForRaycasts)
                    {
                        var verticesData = GetTrianglesData(paintManager.SubMesh);
                        var triangles = new NativeArray<TriangleData>(verticesData.TrianglesData, Allocator.TempJob);
                        var hits = new NativeArray<RaycastTriangleHit>(verticesData.TrianglesData.Length, Allocator.TempJob);
                        var hitsCount = new NativeArray<int>(1, Allocator.TempJob);
                        hitsCount[0] = 0;

                        var job = new RaycastJob
                        {
                            Triangles = triangles,
                            Hits = hits,
                            HitsCount = hitsCount,
                            RayOrigin = rayTransformed.origin,
                            RayDirection = rayTransformed.direction
                        };
                        job.Run();

                        var raycastsCount = job.HitsCount[0];
                        for (var i = 0; i < raycastsCount; i++)
                        {
                            var hit = job.Hits[i];
                            var triangleId = hit.Id;
                            var triangle = paintManagerComponent.Triangles[triangleId];
                            triangle.Hit = hit.Position;
                            triangle.UVHit = hit.UV;
                            intersects.Add(triangle);
                        }

                        triangles.Dispose();
                        hits.Dispose();
                        hitsCount.Dispose();
                    }
                    else
                    {
                        foreach (var triangle in paintManager.Triangles)
                        {
                            var isIntersected = IsIntersected(triangle, rayTransformed, useWorld);
                            if (isIntersected)
                            {
                                intersects.Add(triangle);
                            }
                        }
                    }

                    if (intersects.Count > 0)
                    {
                        var sortedIntersect = SortIntersects(intersects);
                        raycasts.Add(paintManager, sortedIntersect);
                    }
                }
            }
            
            KeyValuePair<IPaintManager, Triangle> closestRaycast = default;
            var minDistance = float.MaxValue;
            foreach (var raycast in raycasts)
            {
                var triangle = raycast.Value;
                var distance = Vector3.Distance(PaintController.Instance.Camera.transform.position, triangle.WorldHit);
                if (distance < minDistance)
                {
                    closestRaycast = raycast;
                    minDistance = distance;
                }
            }

            if (useCache)
            {
                FrameIntersectionData[fingerId] = new FrameIntersectionData
                {
                    PaintManager = closestRaycast.Key, Triangle = closestRaycast.Value, Ray = ray, FrameId = Time.frameCount
                };
            }

            if (closestRaycast.Key != sender)
            {
                return null;
            }
            return closestRaycast.Value;
        }
    }
}