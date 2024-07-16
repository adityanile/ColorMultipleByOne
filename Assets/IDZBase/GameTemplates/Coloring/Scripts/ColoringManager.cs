using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using XDPaint;
using XDPaint.Controllers;
using XDPaint.Core;
using XDPaint.Tools.Image;

namespace IDZBase.Core.GameTemplates.Coloring
{
    public class ColoringManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool UseFixedPoints;
        public List<Vector2> Points;

        [SerializeField] private bool _isBucketColoring;
        [ShowIf("_isBucketColoring")] public bool UseSpeed;
        [ShowIf("@_isBucketColoring && UseSpeed")] [Range(0.01f, 10f)] public float FillSpeed = 1f;
        [ShowIf("@_isBucketColoring && !UseSpeed")] public float FillDuration;
        [ShowIf("_isBucketColoring")] public int MaxConcurrentFillLimit = 4;

        public BrushData CurrentBrushData;

        [FoldoutGroup("Events")] public UnityEvent OnPointerDownEvent = new();
        [FoldoutGroup("Events")] public UnityEvent OnPointerUpEvent = new();
        [FoldoutGroup("Events")] public UnityEvent OnBucketFillComplete = new();

        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer _paintBoardSpriteRenderer;
        private Texture2D _currentTexture;
        private Texture2D _originalTexture;
        private Texture2D _maskTexture;
        private Camera _mainCamera;
        private PaintManager _paintManager;

        private int _width;
        private int _height;
        private MaskData _currentMaskData;
        private readonly List<MaskData> _maskDataList = new();
        private readonly List<int> _activeBucketFillIndices = new();

        private static readonly int MaskTex = Shader.PropertyToID("_MaskTex");

        public bool IsBucketColoring {
            get {
                return _isBucketColoring;
            }
            set {
                _isBucketColoring = value;
                InputController.Instance.enabled = !_isBucketColoring;
            }
        }


        private void InitBrush(BrushData brushData, PaintManager paintManager)
        {
            if (paintManager is null) return;
            if (paintManager.ToolsManager.CurrentTool.Type != PaintTool.Brush) return;
            if (brushData.UsePattern)
            {
                PaintController.Instance.UseSharedSettings = false;
                paintManager.SetPaintMode(PaintMode.Additive);
                paintManager.Brush.SetColor(brushData.BrushColor);
                paintManager.Brush.Size = brushData.BrushSize;
                var settings = ((BrushTool)paintManager.ToolsManager.CurrentTool).Settings;
                settings.UsePattern = true;
                settings.PatternTexture = brushData.PatternTexture;
                settings.PatternScale = brushData.PatternScale;
            }
            else
            {
                PaintController.Instance.UseSharedSettings = !_isBucketColoring;
                if (_isBucketColoring)
                {
                    paintManager.Brush.SetColor(brushData.BrushColor);
                }
                else
                {
                    PaintController.Instance.Brush.SetColor(brushData.BrushColor);
                    PaintController.Instance.Brush.Size = brushData.BrushSize;
                }

                paintManager.SetPaintMode(PaintMode.Default);
                var settings = ((BrushTool)paintManager.ToolsManager.CurrentTool).Settings;
                settings.UsePattern = false;
            }
        }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            var texture = _spriteRenderer.sprite.texture;
            _width = texture.width;
            _height = texture.height;
            _currentTexture = new Texture2D(_width, _height, texture.format, false)
                { hideFlags = HideFlags.HideAndDontSave };
            _originalTexture = new Texture2D(_width, _height, texture.format, false)
                { hideFlags = HideFlags.HideAndDontSave };
            _maskTexture = new Texture2D(_width, _height, texture.format, false)
                { hideFlags = HideFlags.HideAndDontSave };

            Assert.IsNotNull(Camera.main);
            _mainCamera = Camera.main;
        }

        private void Start()
        {
            var paintBoard = transform.GetChild(0);
            _paintManager = paintBoard.GetComponent<PaintManager>();
            _paintBoardSpriteRenderer = paintBoard.GetComponent<SpriteRenderer>();

            InputController.Instance.enabled = !_isBucketColoring;

            CopyTexture(_spriteRenderer.sprite.texture, _currentTexture);
            CreateSprite(_currentTexture);
            CopyTexture(_currentTexture, _originalTexture);

            _originalTexture.filterMode = FilterMode.Point;
            _originalTexture.Apply();

            List<IEnumerable<int>> whitePixelsGroup = new();

            if (UseFixedPoints)
            {
                var imageProcessor = new ImageProcessor(_originalTexture);
                foreach (var point in Points)
                {
                    var texturePos = WorldPositionToTexturePosition(point);
                    whitePixelsGroup.Add(imageProcessor.FindContinuousPixels(texturePos.x, texturePos.y));
                }
            }
            else
            {
                _ = new ImageProcessor(_originalTexture, out whitePixelsGroup);
            }

            for (var i = 0; i < whitePixelsGroup.Count; i++)
            {
                var maskData = new MaskData
                {
                    MaskIndex = i,
                    WhitePixelsHashSet = whitePixelsGroup[i].ToHashSet(),
                    WhitePixelsList = whitePixelsGroup[i].ToList()
                };

                var left = int.MaxValue;
                var right = int.MinValue;
                var top = int.MinValue;
                var bottom = int.MaxValue;

                foreach (var whitePixel in maskData.WhitePixelsList)
                {
                    var x = whitePixel % _width;
                    var y = whitePixel / _height;
                    left = Mathf.Min(left, x);
                    right = Mathf.Max(right, x);
                    top = Mathf.Max(top, y);
                    bottom = Mathf.Min(bottom, y);
                }

                maskData.BottomLeft = new Vector2Int(left, bottom);
                maskData.TopRight = new Vector2Int(right, top);

                _maskDataList.Add(maskData);
            }
        }

        private void OnDestroy()
        {
            Destroy(_currentTexture);
            Destroy(_originalTexture);
            Destroy(_maskTexture);
        }

        private void CopyTexture(Texture2D inTexture, Texture2D outTexture)
        {
            Graphics.CopyTexture(inTexture, 0, 0, 0, 0, _width, _height, outTexture, 0, 0, 0, 0);
        }

        private void CreateSprite(Texture2D texture2D)
        {
            var sprite = Sprite.Create(texture2D, new Rect(0, 0, _width, _height), new Vector2(0.5f, 0.5f));
            _spriteRenderer.sprite = sprite;
        }

        private bool CreateNewMask(int x, int y, out bool outOfBounds)
        {
            if (!_currentMaskData.Equals(default(MaskData)) && _currentMaskData.WhitePixelsHashSet.Contains(y * _width + x))
            {
                outOfBounds = false;
                return false;
            }

            try
            {
                var filteredMasks = _maskDataList.FindAll(maskData =>
                    x >= maskData.BottomLeft.x && x <= maskData.TopRight.x &&
                    y >= maskData.BottomLeft.y && y <= maskData.TopRight.y);

                var index = y * _width + x;
                _currentMaskData = filteredMasks.Single(maskData => maskData.WhitePixelsHashSet.Contains(index));
            }
            catch
            {
                Debug.Log("No white pixels found");
                outOfBounds = true;
                return false;
            }

            var maskColors = _maskTexture.GetPixels32();
            for (var i = 0; i < maskColors.Length; i++)
            {
                maskColors[i] = Color.clear;
            }

            foreach (var whitePixel in _currentMaskData.WhitePixelsList)
            {
                maskColors[whitePixel] = Color.white;
            }

            _maskTexture.SetPixels32(maskColors);
            _maskTexture.Apply();
            outOfBounds = false;
            return true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var mousePosition = eventData.position;
            var worldPosition = _mainCamera.ScreenToWorldPoint(mousePosition);
            worldPosition.z = transform.position.z;

            var texCoord = WorldPositionToTexturePosition(worldPosition);

            if (_isBucketColoring)
            {
                StartCoroutine(BucketColor(texCoord));
            }
            else
            {
                if (CreateNewMask(texCoord.x, texCoord.y, out _))
                {
                    _paintManager.Material.SourceMaterial.SetTexture(MaskTex, _maskTexture);
                }

                _paintManager.Init();
                InitBrush(CurrentBrushData, _paintManager);
            }

            OnPointerDownEvent.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpEvent.Invoke();
            if (!_isBucketColoring) SaveColorToTexture(_paintManager, _maskTexture);
        }

        private IEnumerator BucketColor(Vector2Int position)
        {
            CreateNewMask(position.x, position.y, out var outOfBounds);

            if (outOfBounds) yield break;

            var maskIndex = _currentMaskData.MaskIndex;
            if (_activeBucketFillIndices.Contains(maskIndex) || _activeBucketFillIndices.Count >= MaxConcurrentFillLimit) yield break;
            _activeBucketFillIndices.Add(maskIndex);

            var maskCopy = new Texture2D(_maskTexture.width, _maskTexture.height, _maskTexture.format, false);
            CopyTexture(_maskTexture, maskCopy);

            var paintBoardCopy = new GameObject("PaintBoard Copy");
            paintBoardCopy.transform.SetParent(transform, false);

            var spriteRenderer = paintBoardCopy.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = _paintBoardSpriteRenderer.sprite;
            spriteRenderer.sortingOrder = _paintBoardSpriteRenderer.sortingOrder + 1;

            var paintManager = paintBoardCopy.AddComponent<PaintManager>();
            paintManager.ObjectForPainting = paintManager.gameObject;
            var material = new Material(Shader.Find("XD Paint/Alpha Mask"));
            material.SetTexture(MaskTex, maskCopy);
            paintManager.Material.SourceMaterial = material;
            paintManager.Material.ShaderTextureName = "_MainTex";

            yield return new WaitUntil(() => paintManager.Initialized);

            InitBrush(CurrentBrushData, paintManager);

            var width = Mathf.Abs(_currentMaskData.TopRight.x - _currentMaskData.BottomLeft.x);
            var height = Mathf.Abs(_currentMaskData.TopRight.y - _currentMaskData.BottomLeft.y);
            var center = new Vector2Int(_currentMaskData.BottomLeft.x + width / 2,
                _currentMaskData.BottomLeft.y + height / 2);
            var offset = new Vector2Int(Mathf.Abs(position.x - center.x), Mathf.Abs(position.y - center.y));
            var targetBrushSize = (Mathf.Max(width + offset.x, height + offset.y) / _spriteRenderer.sprite.pixelsPerUnit) * 1.414f;
            var targetBrushSizeWithPadding = targetBrushSize + 0.5f;

            if (UseSpeed) FillDuration = targetBrushSizeWithPadding / FillSpeed;
            
            var brushSize = 0.0141844f;

            while (brushSize < targetBrushSizeWithPadding)
            {
                brushSize += targetBrushSize * Time.deltaTime / FillDuration;
                paintManager.Brush.Size = brushSize;
                paintManager.PaintObject.DrawPoint(position);
                paintManager.PaintObject.FinishPainting();
                
                if (CurrentBrushData is { UseGradient: true, UsePattern: false })
                {
                    paintManager.Brush.Size = brushSize < CurrentBrushData.GradientSize ? brushSize : CurrentBrushData.GradientSize;
                    var defaultBrush = paintManager.PaintObject.Brush.SourceTexture;
                    paintManager.Brush.SetTexture(CurrentBrushData.GradientBrushTexture);
                    paintManager.Brush.SetColor(CurrentBrushData.GradientColor);
                    paintManager.PaintObject.DrawPoint(position);
                    paintManager.PaintObject.FinishPainting();
                    paintManager.Brush.SetTexture(defaultBrush);
                    paintManager.Brush.SetColor(CurrentBrushData.BrushColor);
                    paintManager.Brush.Size = brushSize;
                }

                yield return null;
            }

            SaveColorToTexture(paintManager, maskCopy);
            OnBucketFillComplete.Invoke();

            Destroy(material);
            Destroy(maskCopy);
            Destroy(paintBoardCopy);

            _activeBucketFillIndices.Remove(maskIndex);
        }

        private Vector2Int WorldPositionToTexturePosition(Vector2 worldPosition)
        {
            var unitsToPixels = _width / _spriteRenderer.bounds.size.x;
            var position = transform.position;
            var xCoord = Mathf.RoundToInt((worldPosition.x - position.x) * unitsToPixels) + _width / 2;
            var yCoord = Mathf.RoundToInt((worldPosition.y - position.y) * unitsToPixels) + _width / 2;

            return new Vector2Int(xCoord, yCoord);
        }

        private void SaveColorToTexture(PaintManager paintManager, Texture2D maskTexture)
        {
            var currentPixels = _currentTexture.GetPixels32();
            var maskPixels = maskTexture.GetPixels32();
            var coloredPixels = paintManager.GetResultTexture().GetPixels32();

            for (var i = 0; i < currentPixels.Length; i++)
            {
                currentPixels[i] = maskPixels[i].r > 50 ? coloredPixels[i] : currentPixels[i];
            }

            _currentTexture.SetPixels32(currentPixels);
            _currentTexture.Apply();

            _paintBoardSpriteRenderer.sprite = _spriteRenderer.sprite;
            _paintManager.Init();
        }

        public int GetMaskIndex()
        {
            return _currentMaskData.Equals(default(MaskData)) ? -1 : _currentMaskData.MaskIndex;
        }
    }

    [Serializable]
    public struct BrushData
    {
        public Color BrushColor;
        public float BrushSize;
        public bool UsePattern;
        public Texture2D PatternTexture;
        public Vector2 PatternScale;
        public bool UseGradient;
        public float GradientSize;
        public Color GradientColor;
        public Texture2D GradientBrushTexture;
    }

    public struct MaskData
    {
        public int MaskIndex;
        public Vector2Int BottomLeft;
        public Vector2Int TopRight;
        public List<int> WhitePixelsList;
        public HashSet<int> WhitePixelsHashSet;
    }
}