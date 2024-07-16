using System.Collections;
using System.Collections.Generic;
using System.IO;
using Library.Extensions;
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
    [RequireComponent(typeof(SpriteRenderer))]
    public class ColoringMultipleSprites : MonoBehaviour
    {
        public string UniqueId;
        public bool InitializeOnStart = true;
        public bool UseFixedPoints;
        public List<Vector2> Points;
        public Texture2D BorderTexture;
        [SerializeField] private bool _isBucketColoring;
        [ShowIf("_isBucketColoring")] public bool UseSpeed;
        [ShowIf("@_isBucketColoring && UseSpeed")] [Range(0.01f, 10f)] public float FillSpeed = 1f;
        [ShowIf("@_isBucketColoring && !UseSpeed")] public float FillDuration;

        public BrushData CurrentBrushData;

        public UnityEvent<PaintManager> OnPointerDown = new();
        public UnityEvent<PaintManager> OnPointerUp = new();
        public UnityEvent OnLoadingComplete = new();

        private SpriteRenderer _spriteRenderer;
        private Sprite _sprite;
        private FilterTexture _filterTexture;
        private Texture2D _loadedTexture;
        private Camera _mainCamera;
        private readonly List<Texture2D> _maskTextureReferences = new();
        private readonly List<PaintManager> _paintManagerReferences = new();
        private readonly List<GameObject> _bucketFillQueue = new();
        private readonly List<Vector2Int> _paddings = new();
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

        public bool Loading { get; private set; }
        
        public bool Loaded { get; private set; }

        private void Awake()
        {
            Assert.IsNotNull(Camera.main);
            _mainCamera = Camera.main;
        }

        private void Start()
        {
            if (InitializeOnStart) StartCoroutine(InitializeCoroutine());
        }

        public void Initialize()
        {
            StartCoroutine(InitializeCoroutine());
        }
        
        private IEnumerator InitializeCoroutine()
        {
            if (Loading) yield break;
            Loading = true;
            Loaded = false;
            
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _sprite = _spriteRenderer.sprite;
            var originalTexture = _sprite.texture;

            var whitePixelsGroup = new List<IEnumerable<int>>();
            var textureFilter = new FilterTexture(originalTexture, Color.white, 200);
            var filteredTexture = textureFilter.GetTexture();
            var textureSplitter = new SplitTexture(filteredTexture);

            if (UseFixedPoints)
            {
                foreach (var point in Points)
                {
                    var texturePos = WorldToTexture(point, _spriteRenderer);
                    whitePixelsGroup.Add(textureSplitter.Split(texturePos.x, texturePos.y));
                    yield return null;
                }
            }
            else
            {
                var autoSplitAsync = textureSplitter.AutoSplitAsync();
                yield return new WaitUntil(() => autoSplitAsync.IsCompleted);
                whitePixelsGroup = autoSplitAsync.Result;
            }


            LoadTexture(originalTexture.width, originalTexture.height);
            var loadedPixels = _loadedTexture != null ? _loadedTexture.GetPixels32() : new Color32[] { };

            for (var i = 0; i < whitePixelsGroup.Count; i++)
            {
                var left = int.MaxValue;
                var right = int.MinValue;
                var top = int.MinValue;
                var bottom = int.MaxValue;

                foreach (var whitePixel in whitePixelsGroup[i])
                {
                    var x = whitePixel % originalTexture.width;
                    var y = whitePixel / originalTexture.height;
                    left = Mathf.Min(left, x);
                    right = Mathf.Max(right, x);
                    top = Mathf.Max(top, y);
                    bottom = Mathf.Min(bottom, y);
                }

                var maskTextureSize = Mathf.Max(Mathf.NextPowerOfTwo(right - left), Mathf.NextPowerOfTwo(top - bottom));
                var center = new Vector2Int(left + (right - left) / 2, bottom + (top - bottom) / 2);
                var dWidth = center.x - maskTextureSize / 2;
                var dHeight = center.y - maskTextureSize / 2;
                _paddings.Add(new Vector2Int(dWidth, dHeight));

                var maskTexture = new Texture2D(maskTextureSize, maskTextureSize, TextureFormat.RGBA32, false);
                var maskColors = maskTexture.GetPixels32();
                for (var j = 0; j < maskColors.Length; j++)
                {
                    maskColors[j] = Color.clear;
                }

                foreach (var whitePixel in whitePixelsGroup[i])
                {
                    var x = whitePixel % originalTexture.width;
                    var y = whitePixel / originalTexture.height;
                    var dx = x - dWidth;
                    var dy = y - dHeight;
                    var maskIndex = dy * maskTextureSize + dx;
                    if (maskIndex < 0 || maskIndex >= maskColors.Length) continue;
                    maskColors[maskIndex] = loadedPixels.Length > 0
                        ? loadedPixels[y * originalTexture.width + x]
                        : Color.white;
                }

                maskTexture.SetPixels32(maskColors);
                maskTexture.Apply();
                _maskTextureReferences.Add(maskTexture);

                var part = new GameObject("Part_" + i);
                part.transform.SetParent(transform, false);
                part.transform.localPosition = TextureToLocal(center, _spriteRenderer);

                var sprite = Sprite.Create(maskTexture, new Rect(0, 0, maskTextureSize, maskTextureSize),
                    new Vector2(0.5f, 0.5f), _sprite.pixelsPerUnit);

                var spriteRenderer = part.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite;
                spriteRenderer.sortingOrder = i + 1;

                var paintManager = part.AddComponent<PaintManager>();
                paintManager.ObjectForPainting = part;
                var material = new Material(Shader.Find("XD Paint/Alpha Mask"));
                material.SetTexture(MaskTex, maskTexture);
                paintManager.Material.SourceMaterial = material;
                paintManager.Material.ShaderTextureName = "_MainTex";

                part.AddComponent<PolygonCollider2D>();
                var eventTrigger = part.AddComponent<EventTrigger>();
                var pointerDownEntry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerDown
                };

                pointerDownEntry.callback.AddListener(eventData =>
                {
                    OnPointerDown.Invoke(paintManager);

                    paintManager.PaintObject.ProcessInput = true;

                    if (_isBucketColoring)
                    {
                        if (_bucketFillQueue.Contains(part)) return;
                        _bucketFillQueue.Add(part);

                        var screenPos = eventData.currentInputModule.input.mousePosition;
                        StartCoroutine(BucketFill(screenPos, spriteRenderer, paintManager));
                    }
                    else
                    {
                        InitBrush(CurrentBrushData, paintManager);
                    }
                });

                eventTrigger.triggers.Add(pointerDownEntry);

                var pointerUpEntry = new EventTrigger.Entry
                {
                    eventID = EventTriggerType.PointerUp
                };

                pointerUpEntry.callback.AddListener(_ =>
                {
                    OnPointerUp.Invoke(paintManager);

                    if (_isBucketColoring) return;
                    paintManager.PaintObject.ProcessInput = false;
                    paintManager.PaintObject.FinishPainting();
                });

                eventTrigger.triggers.Add(pointerUpEntry);

                yield return new WaitUntil(() => paintManager.Initialized);
                paintManager.PaintObject.ProcessInput = false;
                _paintManagerReferences.Add(paintManager);
            }

            if (BorderTexture == null)
            {
                _filterTexture = new FilterTexture(originalTexture, Color.black, 100);
                BorderTexture = _filterTexture.GetTexture();
            }

            var border = new GameObject("Border");
            border.transform.SetParent(transform, false);

            var borderSprite = Sprite.Create(
                BorderTexture,
                new Rect(0, 0, BorderTexture.width, BorderTexture.height),
                new Vector2(0.5f, 0.5f));

            var borderSpriteRenderer = border.AddComponent<SpriteRenderer>();
            borderSpriteRenderer.sprite = borderSprite;
            borderSpriteRenderer.sortingOrder = _maskTextureReferences.Count + 1;

            textureFilter.Dispose();
            Destroy(filteredTexture);

            Loading = false;
            Loaded = true;
            OnLoadingComplete.Invoke();
        }

        private void OnDestroy()
        {
            foreach (var maskTexture in _maskTextureReferences)
            {
                Destroy(maskTexture);
            }

            if (_filterTexture != null)
            {
                _filterTexture.Dispose();
                Destroy(BorderTexture);
            }

            Destroy(_loadedTexture);
        }

        private IEnumerator BucketFill(Vector2 screenPosition, SpriteRenderer spriteRenderer, PaintManager paintManager)
        {
            InitBrush(CurrentBrushData, paintManager);

            var worldPosition = _mainCamera.ScreenToWorldPoint(screenPosition);
            var texturePosition = WorldToTexture(worldPosition, spriteRenderer);
            var texture = spriteRenderer.sprite.texture;
            var xSize = Mathf.Max(texturePosition.x, texture.width - texturePosition.x);
            var ySize = Mathf.Max(texturePosition.y, texture.height - texturePosition.y);
            var targetBrushSize = (Mathf.Max(xSize, ySize) / spriteRenderer.sprite.pixelsPerUnit) * 1.414f + 0.5f;

            if (UseSpeed) FillDuration = targetBrushSize / FillSpeed;

            paintManager.StatesController.Disable();
            var brushSize = 0.0141844f;

            while (brushSize < targetBrushSize)
            {
                brushSize += targetBrushSize * Time.deltaTime / FillDuration;
                DrawPoint(brushSize, false);
                yield return null;
            }

            paintManager.StatesController.Enable();
            DrawPoint(targetBrushSize, true);

            paintManager.PaintObject.ProcessInput = false;
            paintManager.PaintObject.FinishPainting();
            _bucketFillQueue.Remove(spriteRenderer.gameObject);

            void DrawPoint(float bSize, bool enableStateControllerOnComplete)
            {
                paintManager.Brush.Size = bSize;
                paintManager.PaintObject.OnMouseDown(1, screenPosition);
                paintManager.PaintObject.OnMouseButton(1, screenPosition);
                paintManager.Render();
                paintManager.PaintObject.OnMouseUp(1, screenPosition);

                if (CurrentBrushData is not { UseGradient: true, UsePattern: false }) return;
                paintManager.StatesController.Disable();
                paintManager.Brush.Size = bSize < CurrentBrushData.GradientSize ? bSize : CurrentBrushData.GradientSize;
                var defaultBrush = paintManager.PaintObject.Brush.SourceTexture;
                paintManager.Brush.SetTexture(CurrentBrushData.GradientBrushTexture);
                paintManager.Brush.SetColor(CurrentBrushData.GradientColor);

                paintManager.PaintObject.OnMouseDown(1, screenPosition);
                paintManager.PaintObject.OnMouseButton(1, screenPosition);
                paintManager.Render();
                paintManager.PaintObject.OnMouseUp(1, screenPosition);

                paintManager.Brush.SetTexture(defaultBrush);
                paintManager.Brush.SetColor(CurrentBrushData.BrushColor);
                paintManager.Brush.Size = bSize;
                if (enableStateControllerOnComplete) paintManager.StatesController.Enable();
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

        private static Vector2Int WorldToTexture(Vector2 worldPosition, SpriteRenderer spriteRenderer)
        {
            var texture = spriteRenderer.sprite.texture;
            var width = texture.width;
            var height = texture.height;
            var unitsToPixels = width / spriteRenderer.bounds.size.x;
            var position = spriteRenderer.transform.position;
            var xCoord = Mathf.RoundToInt((worldPosition.x - position.x) * unitsToPixels) + width / 2;
            var yCoord = Mathf.RoundToInt((worldPosition.y - position.y) * unitsToPixels) + height / 2;

            return new Vector2Int(xCoord, yCoord);
        }

        // Inaccurate
        private static Vector2 TextureToWorld(Vector2Int texturePosition, SpriteRenderer spriteRenderer)
        {
            var transform = spriteRenderer.transform;
            var position = transform.position;
            var sprite = spriteRenderer.sprite;
            var texture = sprite.texture;
            var ppu = sprite.pixelsPerUnit;

            var scale = transform.localScale;
            var parentTransform = transform.parent;
            while (parentTransform != transform.root)
            {
                scale.MultiplyMagnitude(parentTransform.localScale);
                parentTransform = parentTransform.parent;
            }

            scale.MultiplyMagnitude(parentTransform.localScale);

            var x = (position.x + (texturePosition.x - texture.width / 2.0f) / ppu) * scale.x;
            var y = (position.y + (texturePosition.y - texture.height / 2.0f) / ppu) * scale.y;
            return new Vector2(x, y);
        }

        private static Vector2 TextureToLocal(Vector2Int texturePosition, SpriteRenderer spriteRenderer)
        {
            var sprite = spriteRenderer.sprite;
            var texture = sprite.texture;
            var ppu = sprite.pixelsPerUnit;
            var x = (texturePosition.x - texture.width / 2.0f) / ppu;
            var y = (texturePosition.y - texture.height / 2.0f) / ppu;
            return new Vector2(x, y);
        }

        [Button("SaveTexture")]
        public void SaveTexture()
        {
            StartCoroutine(SaveTextureToFile());
        }

        private IEnumerator SaveTextureToFile()
        {
            if (UniqueId.Equals(string.Empty))
            {
                this.LogInEditor("Unique id cannot be null", LogType.Error);
                yield break;
            }

            var originalTexture = _sprite.texture;
            var textureToSave =
                new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
            var pixels = textureToSave.GetPixels32();
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            for (var i = 0; i < _paintManagerReferences.Count; i++)
            {
                var paintManager = _paintManagerReferences[i];
                var resultTexture = paintManager.GetResultTexture();
                var resultPixels = resultTexture.GetPixels32();
                var dWidth = _paddings[i].x;
                var dHeight = _paddings[i].y;
                var maskTexture = _maskTextureReferences[i];
                var maskPixels = maskTexture.GetPixels32();
                for (var j = 0; j < resultPixels.Length; j++)
                {
                    if (maskPixels[j] == Color.clear) continue;
                    var rx = j % resultTexture.width;
                    var ry = j / resultTexture.height;
                    var index = (ry + dHeight) * originalTexture.width + (rx + dWidth);
                    if (pixels[index] != Color.clear) continue;
                    pixels[index] = resultPixels[j];
                }

                yield return null;
            }

            var borderPixels = BorderTexture.GetPixels32();
            for (var i = 0; i < pixels.Length; i++)
            {
                if (borderPixels[i] == Color.clear) continue;
                pixels[i] = borderPixels[i];
            }

            textureToSave.SetPixels32(pixels);
            textureToSave.Apply();
            var byteArray = textureToSave.EncodeToPNG();
            var dirPath = Path.Combine(Application.persistentDataPath, "SaveFiles", _sprite.name);
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
            File.WriteAllBytes(Path.Combine(dirPath, $"{UniqueId}.png"), byteArray);
            Debug.Log("File saved at: " + Path.Combine(dirPath, $"{UniqueId}.png"));
            Destroy(textureToSave);
        }

        private void LoadTexture(int width, int height)
        {
            var filePath = Path.Combine(Application.persistentDataPath, "SaveFiles", _sprite.name,
                $"{UniqueId}.png");

            if (File.Exists(filePath))
            {
                Debug.Log("Save file found at " + filePath);
                var byteArray = File.ReadAllBytes(filePath);
                _loadedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                _loadedTexture.LoadImage(byteArray);
                _loadedTexture.Apply();
            }
            else
            {
                Debug.Log("Save file not found");
            }
        }
    }
}