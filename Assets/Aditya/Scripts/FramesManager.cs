using IDZBase.Core.GameTemplates.Coloring;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using XDPaint;
using XDPaint.Controllers;
using XDPaint.Core;
using XDPaint.Tools.Image;
using XDPaint.Tools.Layers;

public class FramesManager : MonoBehaviour
{
    List<GameObject> paintables = new();

    public UnityEvent<PaintManager> OnPointerDown = new();
    public UnityEvent<PaintManager> OnPointerUp = new();
    public BrushData CurrentBrushData;

    public RenderTexture currentTex;
    public LayerData[] layerData;

    public PaintManager currPM;
    public List<PaintManager> regPM;

    public float rate = 0.5f;

    private bool painting = false;

    private void Start()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            paintables.Add(transform.GetChild(i).gameObject);
        }

        StartCoroutine(Initailise());
    }

    private void Update()
    {
        if (painting)
        {
            foreach(var pm in regPM)
            {
                pm.LayersController.SetLayerTexture(0, currentTex);
                pm.PaintObject.Render();
            }
        } 
    }

    IEnumerator Initailise()
    {
        foreach (var part in paintables)
        {
            var paintManager = part.GetComponent<PaintManager>();
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
                InitBrush(CurrentBrushData, paintManager);

                currentTex = paintManager.GetPaintTexture();

                currPM = paintManager;
                regPM.Remove(currPM);
                painting = true;

                StartCoroutine(PaintContinuously(paintManager));
            });

            eventTrigger.triggers.Add(pointerDownEntry);

            var pointerUpEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp
            };

            pointerUpEntry.callback.AddListener(_ =>
            {
                OnPointerUp.Invoke(paintManager);

                regPM.Add(currPM);
                painting = false;

                StopCoroutine(PaintContinuously(paintManager));

                paintManager.PaintObject.ProcessInput = false;
                paintManager.PaintObject.FinishPainting();
            });

            eventTrigger.triggers.Add(pointerUpEntry);

            yield return new WaitUntil(() => paintManager.Initialized);
            regPM.Add(paintManager);
            paintManager.PaintObject.ProcessInput = false;
        }
    }

    IEnumerator PaintContinuously(PaintManager paintManager)
    {
        while(painting)
        {
            yield return new WaitForEndOfFrame();
            paintManager.PaintObject.FinishPainting();
        }
    }

    public void InitBrush(BrushData brushData, PaintManager paintManager)
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
            PaintController.Instance.UseSharedSettings = true;

            PaintController.Instance.Brush.SetColor(brushData.BrushColor);
            PaintController.Instance.Brush.Size = brushData.BrushSize;

            paintManager.SetPaintMode(PaintMode.Default);
            var settings = ((BrushTool)paintManager.ToolsManager.CurrentTool).Settings;
            settings.UsePattern = false;
        }
    }

}
