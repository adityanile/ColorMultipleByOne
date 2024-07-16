using System;
using System.Collections;
using System.Collections.Generic;
using IDZBase.Core.GameTemplates.Coloring;
using UnityEngine;

public class ColoringTest : MonoBehaviour
{
    public ColoringManager ColoringManager;

    public void ChangeColor(int color)
    {
        ColoringManager.CurrentBrushData.BrushSize = 0.5f;
        ColoringManager.CurrentBrushData.BrushColor = color switch
        {
            0 => Color.red,
            1 => Color.green,
            2 => Color.blue,
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, null)
        };

        ColoringManager.CurrentBrushData.UsePattern = false;
    }

    public void ChangeBrush(int brush)
    {
        ColoringManager.IsBucketColoring = brush == 1;
    }

    public void UseTexture()
    {
        ColoringManager.CurrentBrushData.BrushColor = Color.white;
        ColoringManager.CurrentBrushData.UsePattern = true;
    }
}
