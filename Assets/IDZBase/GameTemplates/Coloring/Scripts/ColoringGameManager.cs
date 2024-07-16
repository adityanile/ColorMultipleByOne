using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XDPaint;

namespace IDZBase.Core.GameTemplates.Coloring
{
    public class ColoringGameManager : MonoBehaviour
    {
        [SerializeField] private ColoringMultipleSprites _coloringManger;
        [SerializeField] private Image _currentSelected;
        [SerializeField] private Transform _colorPalette;
        [SerializeField] private Transform _texturePalette;
        [SerializeField] private Button _brushTool;
        [SerializeField] private Button _bucketTool;
        [SerializeField] private Button _undoButton;
        [SerializeField] private Button _redoButton;

        private readonly List<PaintManager> _undoStack = new();
        private readonly List<PaintManager> _redoStack = new();

        private void Start()
        {
            foreach (Transform child in _colorPalette)
            {
                var button = child.GetComponent<Button>();
                var color = child.GetComponent<Image>().color;
                button.onClick.AddListener(() =>
                {
                    _coloringManger.CurrentBrushData.UsePattern = false;
                    _coloringManger.CurrentBrushData.BrushColor = color;
                    _currentSelected.color = color;
                    _currentSelected.sprite = null;
                });
            }

            foreach (Transform child in _texturePalette)
            {
                var button = child.GetComponent<Button>();
                var sprite = child.GetComponent<Image>().sprite;
                button.onClick.AddListener(() =>
                {
                    _coloringManger.CurrentBrushData.UsePattern = true;
                    _coloringManger.CurrentBrushData.BrushColor = Color.white;
                    _coloringManger.CurrentBrushData.PatternTexture = sprite.texture;
                    _currentSelected.color = Color.white;
                    _currentSelected.sprite = sprite;
                });
            }

            _brushTool.onClick.AddListener(() => _coloringManger.IsBucketColoring = false);
            _bucketTool.onClick.AddListener(() => _coloringManger.IsBucketColoring = true);

            _coloringManger.OnPointerDown.AddListener(pm =>
            {
                _undoStack.Add(pm);
                if (_undoStack.Count >= 10) _undoStack.RemoveAt(0);
            });

            _undoButton.onClick.AddListener(OnUndoButtonClick);
            _redoButton.onClick.AddListener(OnRedoButtonClick);
        }

        private void OnUndoButtonClick()
        {
            if (_undoStack.Count == 0) return;
            var paintManager = _undoStack[^1];
            _undoStack.RemoveAt(_undoStack.Count - 1);
            if (paintManager.StatesController.CanUndo()) paintManager.StatesController.Undo();
            if (paintManager.StatesController.CanRedo()) _redoStack.Add(paintManager);
        }

        private void OnRedoButtonClick()
        {
            if (_redoStack.Count == 0) return;
            var paintManager = _redoStack[^1];
            _redoStack.RemoveAt(_redoStack.Count - 1);
            if (paintManager.StatesController.CanRedo()) paintManager.StatesController.Redo();
            if (paintManager.StatesController.CanUndo()) _undoStack.Add(paintManager);
        }

        public void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}