using UnityEngine;

public static class CustomVerticalSlider
{
    public static float Draw(
        Rect rect,
        float value,
        float min,
        float max,
        Texture2D thumbTex
    )
    {
        Rect barRect = new Rect(
            rect.x + rect.width * 0.5f - 3f,
            rect.y,
            6f,
            rect.height
        );
        
        GUIStyle bgStyle = new GUIStyle(GUI.skin.box);
        bgStyle.normal.background = Texture2D.whiteTexture;

        GUI.Box(barRect, GUIContent.none, bgStyle);

        GUIStyle thumbStyle = new GUIStyle(GUI.skin.button);

        if (thumbTex != null)
        {
            thumbStyle.normal.background = thumbTex;

            thumbStyle.fixedWidth = thumbTex.width;
            thumbStyle.fixedHeight = thumbTex.height;

            thumbStyle.stretchWidth = false;
            thumbStyle.stretchHeight = false;

            thumbStyle.alignment = TextAnchor.MiddleCenter;

            thumbStyle.padding = new RectOffset(0, 0, 0, 0);
            thumbStyle.margin = new RectOffset(0, 0, 0, 0);
            thumbStyle.border = new RectOffset(0, 0, 0, 0);
        }

        GUIStyle emptyStyle = GUIStyle.none;

        barRect.x -= 60f;
        
        value = GUI.VerticalSlider(
            barRect,
            value,
            max,
            min,
            emptyStyle,
            thumbStyle
        );

        return Mathf.Clamp(value, min, max);
    }
}