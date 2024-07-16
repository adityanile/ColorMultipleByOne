using System;
using System.Linq;
using UnityEngine;
using XDPaint.Controllers;
using XDPaint.Core.Materials;
using XDPaint.Scripts.Core.PaintObject.Base;
using XDPaint.Scripts.Core.PaintObject.Data;
using XDPaint.States;
using XDPaint.Tools.Raycast;

namespace XDPaint.Core.PaintObject.Base
{
    public abstract class BasePaintObject : BasePaintObjectRenderer
    {
        #region Events

        /// <summary>
        /// Mouse hover event
        /// </summary>
        public event Action<PointerData> OnPointerHover;
        
        /// <summary>
        /// Mouse down event
        /// </summary>
        public event Action<PointerData> OnPointerDown;
        
        /// <summary>
        /// Mouse press event
        /// </summary>
        public event Action<PointerData> OnPointerPress;
        
        /// <summary>
        /// Mouse up event
        /// </summary>
        public event Action<PointerUpData> OnPointerUp;
        
        /// <summary>
        /// Draw point event, can be used by the developer to obtain data about painting
        /// </summary>
        public event Action<DrawPointData> OnDrawPoint;
        
        /// <summary>
        /// Draw line event, can be used by the developer to obtain data about painting
        /// </summary>
        public event Action<DrawLineData> OnDrawLine;
        
        #endregion

        #region Obsolete Events

        /// <summary>
        /// Mouse hover event, used by ToolsManager.
        /// Arguments: paint object local position, screen position, UV position, texture position, pressure
        /// </summary>
        [Obsolete("This event has been deprecated and will be removed in future updates. Please use OnHover instead.")] 
        public event Action<Vector3, Vector2, Vector2, Vector2, float> OnMouseHoverHandler;
        
        /// <summary>
        /// Mouse down event, used by ToolsManager.
        /// Arguments: paint object local position, screen position, UV position, texture position, pressure
        /// </summary>
        [Obsolete("This event has been deprecated and will be removed in future updates. Please use OnDown instead.")] 
        public event Action<Vector3, Vector2, Vector2, Vector2, float> OnMouseDownHandler;
        
        /// <summary>
        /// Mouse press event, used by ToolsManager.
        /// Arguments: paint object local position, screen position, UV position, texture position, pressure
        /// </summary>
        [Obsolete("This event has been deprecated and will be removed in future updates. Please use OnPress instead.")] 
        public event Action<Vector3, Vector2, Vector2, Vector2, float> OnMouseHandler;
        
        /// <summary>
        /// Mouse up event, used by ToolsManager.
        /// Arguments: screen position, is mouse in object bounds
        /// </summary>
        [Obsolete("This event has been deprecated and will be removed in future updates. Please use OnUp instead.")] 
        public event Action<Vector2, bool> OnMouseUpHandler;
        
        /// <summary>
        /// Draw point event, can be used by the developer to obtain data about painting.
        /// Arguments: texture position, pressure
        /// </summary>
        [Obsolete("This event has been deprecated and will be removed in future updates. Please use OnDrawPoint instead.")] 
        public event Action<Vector2, float> OnDrawPointHandler;
        
        /// <summary>
        /// Draw line event, can be used by the developer to obtain data about painting.
        /// Arguments: line start texture position, line end texture position, line start pressure, line end pressure
        /// </summary>
        [Obsolete("This event has been deprecated and will be removed in future updates. Please use OnDrawLine instead.")] 
        public event Action<Vector2, Vector2, float, float> OnDrawLineHandler;

        #endregion

        #region Properties and variables

        public bool InBounds => PaintObjectData.Any(x => x.InBounds);
        public bool IsPainting => PaintObjectData.Any(x => x.IsPainting);
        public bool IsPainted { get; private set; }
        public bool ProcessInput = true;

        private Camera thisCamera;
        public new Camera Camera
        {
            protected get => thisCamera;
            set
            {
                thisCamera = value;
                base.Camera = thisCamera;
            }
        }

        protected BasePaintObjectData[] PaintObjectData;
        protected Transform ObjectTransform { get; private set; }
        protected IPaintManager PaintManager;

        private IStatesController statesController;
        private bool shouldClearTexture = true;
        private bool writeClear;
        private bool wasRendered;
        private const float HalfTextureRatio = 0.5f;
        
        #endregion

        #region Abstract methods
        
        protected abstract void Init();
        protected abstract void CalculatePaintPosition(int fingerId, Vector3 position, Vector2? uv = null, bool usePostPaint = true, Triangle hitTriangle = null);
        protected abstract bool IsInBounds(Vector3 position);
        protected abstract bool IsInBounds(Vector3 position, Triangle triangle);

        #endregion

        public void Init(IPaintManager paintManagerInstance, Camera camera, Transform objectTransform, Paint paint, 
            IRenderTextureHelper renderTextureHelper, IStatesController states)
        {
            PaintManager = paintManagerInstance;
            thisCamera = camera;
            ObjectTransform = objectTransform;
            PaintMaterial = paint;
            RenderTextureHelper = renderTextureHelper;
            statesController = states;
            InitRenderer(PaintManager, Camera, PaintMaterial);
            PaintObjectData = new BasePaintObjectData[InputController.Instance.MaxTouchesCount + 1];
            for (var i = 0; i < PaintObjectData.Length; i++)
            {
                PaintObjectData[i] = new BasePaintObjectData();
            }
            InitStatesController();
            Init();
        }

        public override void DoDispose()
        {
            if (statesController != null)
            {
                statesController.OnRenderTextureAction -= OnExtraDraw;
                statesController.OnClearTextureAction -= OnClearTexture;
                statesController.OnResetState -= OnResetState;
            }
            base.DoDispose();
        }

        private void InitStatesController()
        {
            if (statesController == null)
                return;
            
            statesController.OnRenderTextureAction += OnExtraDraw;
            statesController.OnClearTextureAction += OnClearTexture;
            statesController.OnResetState += OnResetState;
        }

        private void OnResetState()
        {
            shouldClearTexture = true;
        }

        #region Input

        public void OnMouseHover(int fingerId, Vector3 position, Triangle triangle = null)
        {
            if (ObjectTransform == null)
            {
                Debug.LogError("ObjectForPainting has been destroyed!");
                return;
            }
            
            if (!ProcessInput || !ObjectTransform.gameObject.activeInHierarchy)
                return;
            
            if (!IsPainting)
            {
                var paintObjectData = PaintObjectData[fingerId];
                if (triangle != null)
                {
                    CalculatePaintPosition(fingerId, position, triangle.UVHit, false, triangle);
                    paintObjectData.LocalPosition = triangle.Hit;
                    if (OnPointerHover != null && paintObjectData.LocalPosition != null && paintObjectData.PaintPosition != null)
                    {
                        var data = new PointerData(paintObjectData.LocalPosition.Value, position, triangle.UVHit, paintObjectData.PaintPosition.Value, 1f, fingerId);
                        OnPointerHover(data);
                    }
                    
                    if (OnMouseHoverHandler != null && paintObjectData.LocalPosition != null && paintObjectData.PaintPosition != null)
                    {
                        OnMouseHoverHandler(paintObjectData.LocalPosition.Value, position, triangle.UVHit, paintObjectData.PaintPosition.Value, 1f);
                    }
                }
                else 
                {
                    CalculatePaintPosition(fingerId, position, null, false);
                    if (OnPointerHover != null && paintObjectData.LocalPosition != null && paintObjectData.PaintPosition != null)
                    {
                        var uv = new Vector2(
                            paintObjectData.PaintPosition.Value.x / PaintMaterial.SourceTexture.width, 
                            paintObjectData.PaintPosition.Value.y / PaintMaterial.SourceTexture.height);
                        var data = new PointerData(paintObjectData.LocalPosition.Value, position, uv, paintObjectData.PaintPosition.Value, 1f, fingerId);
                        OnPointerHover(data);
                    }
                    
                    if (OnMouseHoverHandler != null && paintObjectData.LocalPosition != null && paintObjectData.PaintPosition != null)
                    {
                        var uv = new Vector2(
                            paintObjectData.PaintPosition.Value.x / PaintMaterial.SourceTexture.width, 
                            paintObjectData.PaintPosition.Value.y / PaintMaterial.SourceTexture.height);
                        OnMouseHoverHandler(paintObjectData.LocalPosition.Value, position, uv, paintObjectData.PaintPosition.Value, 1f);
                    }
                }
            }
        }

        public void OnMouseHoverFailed(int fingerId, Vector3 position, Triangle triangle = null)
        {
            PaintObjectData[fingerId].InBounds = false;
        }

        public void OnMouseDown(int fingerId, Vector3 position, float pressure = 1f, Triangle triangle = null)
        {
            if (ObjectTransform == null)
            {
                Debug.LogError("ObjectForPainting has been destroyed!");
                return;
            }
            
            if (!ProcessInput || !ObjectTransform.gameObject.activeInHierarchy)
                return;

            var paintObjectData = PaintObjectData[fingerId];
            paintObjectData.IsPaintingDone = false;
            paintObjectData.InBounds = false;
            paintObjectData.Pressure = pressure;
            wasRendered = false;

            if (!IsPainting && paintObjectData.PaintPosition == null)
            {
                if (triangle != null)
                {
                    CalculatePaintPosition(fingerId, position, triangle.UVHit, true, triangle);
                    paintObjectData.LocalPosition = triangle.Hit;
                }
                else
                {
                    CalculatePaintPosition(fingerId, position);
                }
            }

            if (paintObjectData.PaintPosition != null && paintObjectData.LocalPosition != null)
            {
                if (triangle == null)
                {
                    if (IsInBounds(position))
                    {
                        var uv = new Vector2(
                            paintObjectData.PaintPosition.Value.x / PaintMaterial.SourceTexture.width,
                            paintObjectData.PaintPosition.Value.y / PaintMaterial.SourceTexture.height);

                        if (OnPointerDown != null)
                        {
                            var data = new PointerData(paintObjectData.LocalPosition.Value, position, uv,
                                paintObjectData.PaintPosition.Value, paintObjectData.Pressure, fingerId);
                            OnPointerDown(data);
                        }

                        OnMouseDownHandler?.Invoke(paintObjectData.LocalPosition.Value, position, uv,
                            paintObjectData.PaintPosition.Value, paintObjectData.Pressure);
                    }
                }
                else
                {
                    if (OnPointerDown != null)
                    {
                        var data = new PointerData(paintObjectData.LocalPosition.Value, position, triangle.UVHit,
                            paintObjectData.PaintPosition.Value, paintObjectData.Pressure, fingerId);
                        OnPointerDown(data);
                    }

                    OnMouseDownHandler?.Invoke(paintObjectData.LocalPosition.Value, position, triangle.UVHit,
                        paintObjectData.PaintPosition.Value, paintObjectData.Pressure);
                }
            }
        }

        public void OnMouseButton(int fingerId, Vector3 position, float brushPressure = 1f, Triangle triangle = null)
        {
            if (ObjectTransform == null)
            {
                Debug.LogError("ObjectForPainting has been destroyed!");
                return;
            }
            
            if (!ProcessInput || !ObjectTransform.gameObject.activeInHierarchy)
                return;
            
            wasRendered = false;
            var paintObjectData = PaintObjectData[fingerId];
            
            if (triangle == null)
            {
                paintObjectData.IsPainting = true;
                paintObjectData.LineData.AddBrush(brushPressure * Brush.Size);
                CalculatePaintPosition(fingerId, position);
                paintObjectData.Pressure = brushPressure;
                if (paintObjectData.PaintPosition != null)
                {
                    paintObjectData.IsPainting = true;
                }
                
                if (paintObjectData.InBounds && paintObjectData.LocalPosition != null && paintObjectData.PaintPosition != null)
                {
                    var uv = new Vector2(paintObjectData.PaintPosition.Value.x / PaintMaterial.SourceTexture.width, paintObjectData.PaintPosition.Value.y / PaintMaterial.SourceTexture.height);
                    var data = new PointerData(paintObjectData.LocalPosition.Value, position, uv, paintObjectData.PaintPosition.Value, paintObjectData.Pressure, fingerId);
                    OnPointerPress?.Invoke(data);
                    OnMouseHandler?.Invoke(paintObjectData.LocalPosition.Value, position, uv, paintObjectData.PaintPosition.Value, paintObjectData.Pressure);
                }
            }
            else if (triangle.Transform == ObjectTransform)
            {
                paintObjectData.IsPainting = true;
                paintObjectData.LineData.AddTriangleBrush(triangle, brushPressure * Brush.Size);
                paintObjectData.Pressure = brushPressure;
                CalculatePaintPosition(fingerId, position, triangle.UVHit, true, triangle);
                paintObjectData.LocalPosition = triangle.Hit;
                if (paintObjectData.LocalPosition != null && paintObjectData.PaintPosition != null)
                {
                    var data = new PointerData(paintObjectData.LocalPosition.Value, position, triangle.UVHit, paintObjectData.PaintPosition.Value, paintObjectData.Pressure, fingerId);
                    OnPointerPress?.Invoke(data);
                    OnMouseHandler?.Invoke(paintObjectData.LocalPosition.Value, position, triangle.UVHit, paintObjectData.PaintPosition.Value, paintObjectData.Pressure);
                }
            }
            else
            {
                paintObjectData.LocalPosition = null;
                paintObjectData.PaintPosition = null;
                paintObjectData.LineData.Clear();
            }
        }

        public void OnMouseUp(int fingerId, Vector3 position)
        {
            if (!wasRendered && PaintObjectData[fingerId].IsPaintingDone)
            {
                OnRender();
                Render();
            }
            
            if (ObjectTransform == null)
            {
                Debug.LogError("ObjectForPainting has been destroyed!");
                return;
            }
            
            if (!ProcessInput || !ObjectTransform.gameObject.activeInHierarchy)
                return;
            
            FinishPainting(fingerId);
            var data = new PointerUpData(position, IsInBounds(position), fingerId);
            OnPointerUp?.Invoke(data);
            OnMouseUpHandler?.Invoke(position, IsInBounds(position));
        }

        public Vector2? GetPaintPosition(int fingerId, Vector3 position, Triangle triangle = null)
        {
            if (ObjectTransform == null)
            {
                Debug.LogError("ObjectForPainting has been destroyed!");
                return null;
            }

            if (!ProcessInput || !ObjectTransform.gameObject.activeInHierarchy)
                return null;
            
            if (triangle != null)
            {
                CalculatePaintPosition(fingerId, position, triangle.UVHit, false);
            }
            else
            {
                CalculatePaintPosition(fingerId, position, null, false);
            }

            var paintObjectData = PaintObjectData[fingerId];
            var paintPosition = paintObjectData.PaintPosition;
            if (paintObjectData.InBounds && paintPosition != null)
            {
                return paintPosition.Value;
            }
            return null;
        }

        #endregion
        
        #region DrawFromCode

        public void DrawPoint(DrawPointData drawPointData)
        {
            DrawPoint(drawPointData.TexturePosition, drawPointData.Pressure, drawPointData.FingerId);
        }

        /// <summary>
        /// Draws point with pressure
        /// </summary>
        /// <param name="position"></param>
        /// <param name="brushPressure"></param>
        /// <param name="fingerId"></param>
        public void DrawPoint(Vector2 position, float brushPressure = 1f, int fingerId = 0)
        {
            var paintObjectData = PaintObjectData[fingerId];
            var pressureBefore = paintObjectData.Pressure;
            var previousPaintPositionBefore = paintObjectData.PreviousPaintPosition;
            var paintPositionBefore = paintObjectData.PaintPosition;
            var isPaintingBefore = IsPainting;
            var isPaintingDoneBefore = paintObjectData.IsPaintingDone;
            var lineDataBefore = new LineData();
            lineDataBefore.Triangles.AddRange(paintObjectData.LineData.Triangles);
            lineDataBefore.PaintPositions.AddRange(paintObjectData.LineData.PaintPositions);
            lineDataBefore.BrushSizes.AddRange(paintObjectData.LineData.BrushSizes);
            
            paintObjectData.Pressure = brushPressure;
            paintObjectData.PaintPosition = position;
            paintObjectData.IsPainting = true;
            paintObjectData.IsPaintingDone = true;
            var data = new DrawPointData(position, brushPressure, fingerId);
            OnDrawPoint?.Invoke(data);
            OnDrawPointHandler?.Invoke(position, brushPressure);
            paintObjectData.LineData.Clear();
            paintObjectData.LineData.AddPosition(position);
            OnRender();
            Render();
         
            paintObjectData.Pressure = pressureBefore;
            paintObjectData.PreviousPaintPosition = previousPaintPositionBefore;
            paintObjectData.PaintPosition = paintPositionBefore;
            paintObjectData.IsPainting = isPaintingBefore;
            paintObjectData.IsPaintingDone = isPaintingDoneBefore;
            paintObjectData.LineData.Clear();
            paintObjectData.LineData.Triangles.AddRange(lineDataBefore.Triangles);
            paintObjectData.LineData.PaintPositions.AddRange(lineDataBefore.PaintPositions);
            paintObjectData.LineData.BrushSizes.AddRange(lineDataBefore.BrushSizes);
            
            // FinishPainting(fingerId);
        }
        
        public void DrawLine(DrawLineData drawLineData)
        {
            DrawLine(drawLineData.LineStartPosition, drawLineData.LineEndPosition, 
                drawLineData.StartPressure, drawLineData.EndPressure, drawLineData.FingerId);
        }

        /// <summary>
        /// Draws line with pressure
        /// </summary>
        /// <param name="positionStart"></param>
        /// <param name="positionEnd"></param>
        /// <param name="pressureStart"></param>
        /// <param name="pressureEnd"></param>
        /// <param name="fingerId"></param>
        public void DrawLine(Vector2 positionStart, Vector2 positionEnd, float pressureStart = 1f, float pressureEnd = 1f, int fingerId = 0)
        {
            var paintObjectData = PaintObjectData[fingerId];
            var pressureBefore = paintObjectData.Pressure;
            var previousPaintPositionBefore = paintObjectData.PreviousPaintPosition;
            var paintPositionBefore = paintObjectData.PaintPosition;
            var isPaintingBefore = IsPainting;
            var isPaintingDoneBefore = paintObjectData.IsPaintingDone;
            var lineDataBefore = new LineData();
            lineDataBefore.Triangles.AddRange(paintObjectData.LineData.Triangles);
            lineDataBefore.PaintPositions.AddRange(paintObjectData.LineData.PaintPositions);
            lineDataBefore.BrushSizes.AddRange(paintObjectData.LineData.BrushSizes);

            paintObjectData.Pressure = pressureEnd;
            paintObjectData.PaintPosition = positionEnd;
            paintObjectData.IsPainting = true;
            paintObjectData.IsPaintingDone = true;
            var data = new DrawLineData(positionStart, positionEnd, pressureStart, pressureEnd, fingerId);
            OnDrawLine?.Invoke(data);
            OnDrawLineHandler?.Invoke(positionStart, positionEnd, pressureStart, pressureEnd);
            paintObjectData.LineData.Clear();
            paintObjectData.LineData.AddBrush(pressureStart * Brush.Size);
            paintObjectData.LineData.AddBrush(paintObjectData.Pressure * Brush.Size);
            paintObjectData.LineData.AddPosition(positionStart);
            paintObjectData.LineData.AddPosition(positionEnd);
            OnRender();
            Render();

            paintObjectData.Pressure = pressureBefore;
            paintObjectData.PreviousPaintPosition = previousPaintPositionBefore;
            paintObjectData.PaintPosition = paintPositionBefore;
            paintObjectData.IsPainting = isPaintingBefore;
            paintObjectData.IsPaintingDone = isPaintingDoneBefore;
            paintObjectData.LineData.Clear();
            paintObjectData.LineData.Triangles.AddRange(lineDataBefore.Triangles);
            paintObjectData.LineData.PaintPositions.AddRange(lineDataBefore.PaintPositions);
            paintObjectData.LineData.BrushSizes.AddRange(lineDataBefore.BrushSizes);
            
            // FinishPainting(fingerId);
        }
        
        #endregion

        /// <summary>
        /// Resets all states, bake paint result into PaintTexture, save paint result to TextureKeeper
        /// </summary>
        public void FinishPainting(int fingerId = 0)
        {
            var paintObjectData = PaintObjectData[fingerId];
            if (IsPainting)
            {
                paintObjectData.Pressure = 1f;
                if (PaintMode.UsePaintInput)
                {
                    BakeInputToPaint();
                    ClearTexture(RenderTarget.Input);
                }

                paintObjectData.IsPainting = false;
                if (paintObjectData.IsPaintingDone && Tool.ProcessingFinished)
                {
                    SaveUndoTexture();
                }
                paintObjectData.LineData.Clear();
                if (!PaintMode.UsePaintInput)
                {
                    ClearTexture(RenderTarget.Input);
                    Render();
                }
            }
            
            PaintMaterial.SetPaintPreviewVector(Vector4.zero);
            paintObjectData.LocalPosition = null;
            paintObjectData.PaintPosition = null;
            paintObjectData.IsPaintingDone = false;
            paintObjectData.InBounds = false;
            paintObjectData.PreviousPaintPosition = default;
        }

        /// <summary>
        /// Renders Points and Lines, restoring textures when Undo/Redo invoking
        /// </summary>
        public void OnRender()
        {
            if (shouldClearTexture)
            {
                ClearTexture(RenderTarget.Input);
                shouldClearTexture = false;
                if (writeClear && Tool.RenderToTextures)
                {
                    SaveUndoTexture();
                    writeClear = false;
                }
            }

            var painted = false;
            IsPainted = false;
            for (var i = 0; i < PaintObjectData.Length; i++)
            {
                var paintObjectData = PaintObjectData[i];
                if (IsPainting && paintObjectData.PaintPosition != null && (!Tool.ConsiderPreviousPosition ||
                     paintObjectData.PreviousPaintPosition != paintObjectData.PaintPosition.Value) && Tool.AllowRender)
                {
                    painted = true;
                    IsPainted = true;
                    if (paintObjectData.LineData.HasOnePosition())
                    {
                        DrawPoint(i);
                        paintObjectData.PreviousPaintPosition = paintObjectData.PaintPosition.Value;
                    }
                    else if (Tool.BaseSettings.CanPaintLines)
                    {
                        DrawLine(!paintObjectData.LineData.HasNotSameTriangles(), i);
                        paintObjectData.PreviousPaintPosition = paintObjectData.PaintPosition.Value;
                    }
                }
                IsPainted = painted;
            }
        }

        /// <summary>
        /// Combines textures, render preview
        /// </summary>
        public void Render()
        {
            DrawPreProcess();
            ClearTexture(RenderTarget.Combined);
            DrawProcess();
            wasRendered = true;
        }

        public void RenderToTextureWithoutPreview(RenderTexture resultTexture)
        {
            DrawPreProcess();
            ClearTexture(RenderTarget.Combined);
            //disable preview
            var inBounds = PaintObjectData.Select(x => x.InBounds).ToArray();
            foreach (var paintObjectData in PaintObjectData)
            {
                paintObjectData.InBounds = false;
            }
            DrawProcess();
            for (var i = 0; i < PaintObjectData.Length; i++)
            {
                PaintObjectData[i].InBounds = inBounds[i];
            }
            Graphics.Blit(RenderTextureHelper.GetTexture(RenderTarget.Combined), resultTexture);
        }

        public void SaveUndoTexture()
        {
            ActiveLayer().SaveState();
        }
        
        /// <summary>
        /// Restores texture when Undo/Redo invoking
        /// </summary>
        private void OnExtraDraw()
        {
            if (!PaintMode.UsePaintInput)
            {
                ClearTexture(RenderTarget.Input);
            }
            Render();
        }

        private void OnClearTexture(RenderTexture renderTexture)
        {
            ClearTexture(renderTexture, Color.clear);
            Render();
        }

        /// <summary>
        /// Gets position for draw point
        /// </summary>
        /// <param name="holePosition"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        private Rect GetPosition(Vector2 holePosition, float scale)
        {
            return new Rect(
                (holePosition.x - (HalfTextureRatio - Brush.RenderOffset.x) * Brush.RenderTexture.width * scale) / PaintMaterial.SourceTexture.width,
                (holePosition.y - (HalfTextureRatio - Brush.RenderOffset.y) * Brush.RenderTexture.height * scale) / PaintMaterial.SourceTexture.height,
                Brush.RenderTexture.width / (float)PaintMaterial.SourceTexture.width * scale,
                Brush.RenderTexture.height / (float)PaintMaterial.SourceTexture.height * scale);
        }
        
        /// <summary>
        /// Renders quad(point)
        /// </summary>
        private void DrawPoint(int fingerId = 0)
        {
            var paintObjectData = PaintObjectData[fingerId];
            var data = new DrawPointData(paintObjectData.PaintPosition.Value, Brush.Size * paintObjectData.Pressure, fingerId);
            OnDrawPoint?.Invoke(data);
            OnDrawPointHandler?.Invoke(paintObjectData.PaintPosition.Value, Brush.Size * paintObjectData.Pressure);
            var positionRect = GetPosition(paintObjectData.PaintPosition.Value, Brush.Size * paintObjectData.Pressure);
            RenderQuad(positionRect);
        }

        /// <summary>
        /// Renders a few quads (line)
        /// </summary>
        /// <param name="interpolate"></param>
        /// <param name="fingerId"></param>
        private void DrawLine(bool interpolate, int fingerId = 0)
        {
            Vector2[] positions;
            Vector2[] paintPositions;
            var paintObjectData = PaintObjectData[fingerId];
            if (interpolate)
            {
                paintPositions = paintObjectData.LineData.GetPositions();
                positions = paintPositions;
            }
            else
            {
                paintPositions = paintObjectData.LineData.GetPositions();
                var triangles = paintObjectData.LineData.GetTriangles();
                positions = GetLinePositions(paintPositions[0], paintPositions[1], triangles[0], triangles[1], fingerId);
            }
            
            if (positions.Length > 0)
            {
                var brushes = paintObjectData.LineData.GetBrushes();
                if (brushes.Length != 2)
                {
                    Debug.LogWarning("Incorrect length of the brushes array!");
                }
                else
                {
                    var data = new DrawLineData(paintPositions[0], paintPositions[1], brushes[0], brushes[1], fingerId);
                    OnDrawLine?.Invoke(data);
                    OnDrawLineHandler?.Invoke(paintPositions[0], paintPositions[1], brushes[0], brushes[1]);
                    RenderLine(positions, Brush.RenderOffset, Brush.RenderTexture, Brush.Size, brushes);
                }
            }
        }

        /// <summary>
        /// Post paint method, used by CalculatePaintPosition method
        /// </summary>
        protected void OnPostPaint(int fingerId = 0)
        {
            var paintObjectData = PaintObjectData[fingerId];
            if (paintObjectData.PaintPosition != null && IsPainting)
            {
                paintObjectData.LineData.AddPosition(paintObjectData.PaintPosition.Value);
            }
            else if (paintObjectData.PaintPosition == null)
            {
                paintObjectData.LineData.Clear();
            }
        }

        protected void UpdateBrushPreview(int fingerId = 0)
        {
            var paintObjectData = PaintObjectData[fingerId];
            if (Brush.Preview && paintObjectData.InBounds)
            {
                if (paintObjectData.PaintPosition != null)
                {
                    var previewVector = GetPreviewVector(fingerId);
                    PaintMaterial.SetPaintPreviewVector(previewVector);
                }
                else
                {
                    PaintMaterial.SetPaintPreviewVector(Vector4.zero);
                }
            }
        }

        /// <summary>
        /// Returns Vector4 for brush preview
        /// </summary>
        /// <returns></returns>
        private Vector4 GetPreviewVector(int fingerId = 0)
        {
            var paintObjectData = PaintObjectData[fingerId];
            var brushRatio = new Vector2(
                PaintMaterial.SourceTexture.width / (float)Brush.RenderTexture.width,
                PaintMaterial.SourceTexture.height / (float)Brush.RenderTexture.height) / Brush.Size / paintObjectData.Pressure;
            var brushOffset = new Vector4(
                paintObjectData.PaintPosition.Value.x / PaintMaterial.SourceTexture.width * brushRatio.x + Brush.RenderOffset.x,
                paintObjectData.PaintPosition.Value.y / PaintMaterial.SourceTexture.height * brushRatio.y + Brush.RenderOffset.y,
                brushRatio.x, brushRatio.y);
            return brushOffset;
        }
    }
}