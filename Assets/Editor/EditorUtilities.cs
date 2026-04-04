using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class EditorUtilities
{
    #region List Drawing
    //for direct list access
    public static void DrawList<T>(List<T> list, string listTitle, EditorListDrawerStyle listStyle, ref bool isOpened)
    {
        Color oldColor = GUI.backgroundColor;
        int elementToRemove = -1;
        
        GUILayout.Space(listStyle.p_verticalMargin);
        
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        GUILayout.Label(listTitle, listStyle.p_titleStyle);
        if (GUILayout.Button(isOpened ? "⇑" : "⇓", GUILayout.Width(50)))
        {
            isOpened = !isOpened;
        }
        GUILayout.EndHorizontal();
        
        if (!isOpened)
        {
            GUILayout.EndVertical();
            GUILayout.Space(listStyle.p_verticalMargin);
            return;
        }
        
        for (int i = 0; i < list.Count; i++)
        {
            object value = list[i];
            EditorGUILayout.BeginHorizontal();
            if(typeof(T) == typeof(Vector2))
            {
                GUILayout.Label(listStyle.p_label?.Invoke(i), GUILayout.Width(80));
                value = EditorGUILayout.Vector2Field("", (Vector2)value);
            }
            else if (typeof(Object).IsAssignableFrom(typeof(T)))
            {
                GUILayout.Label(listStyle.p_label?.Invoke(i), GUILayout.Width(80));
                value = EditorGUILayout.ObjectField("", (Object)value, typeof(T), true);
            }
            
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = listStyle.p_removeColor;
            if (GUILayout.Button("-",  GUILayout.Width(50)))
            {
                elementToRemove = i;
            }
            
            GUILayout.BeginVertical();
            GUI.backgroundColor = i == 0 ? oldColor * 0.8f : oldColor;
            if (GUILayout.Button("↑", GUILayout.Height(10), GUILayout.Width(50)))
            {
                if(i!=0)
                {
                    Swap(list, i, i-1);
                    //list.MoveArrayElement(i, i - 1);
                }
            }
            GUI.backgroundColor = i == list.Count-1 ? oldColor * 0.8f : oldColor;
            if (GUILayout.Button("↓", GUILayout.Height(10), GUILayout.Width(50)))
            {
                if(i!=list.Count-1)
                {
                    Swap(list, i, i+1);
                    //list.MoveArrayElement(i, i + 1);
                }
            }
            GUILayout.EndVertical();
            GUI.backgroundColor = oldColor;
            GUI.backgroundColor = oldColor;
            EditorGUILayout.EndHorizontal();

            list[i] = (T)value;
        }
        GUILayout.BeginHorizontal();

        GUI.backgroundColor = listStyle.p_addColor;
        if (GUILayout.Button("+"))
        {
            list.Add(default(T));
        }

        GUI.backgroundColor = listStyle.p_removeColor;
        if (GUILayout.Button("-"))
        {
            if(list.Count > 0)
            {
                list.RemoveAt(list.Count - 1);
            }
        }
        GUI.backgroundColor = oldColor;
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Space(listStyle.p_verticalMargin);
        
        if (elementToRemove >= 0)
        {
            list.RemoveAt(elementToRemove);
            elementToRemove = -1;
        }
    }
    //for serializedObject access
    public static void DrawList(SerializedProperty list, string listTitle, EditorListDrawerStyle listStyle, ref bool isOpened)
    {
        Color oldColor = GUI.backgroundColor;
        
        GUILayout.Space(listStyle.p_verticalMargin);
        
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        GUILayout.Label(listTitle, listStyle.p_titleStyle);
        if (GUILayout.Button(isOpened ? "⇑" : "⇓", GUILayout.Width(50)))
        {
            isOpened = !isOpened;
        }
        GUILayout.EndHorizontal();
        
        if (!isOpened)
        {
            GUILayout.EndVertical();
            GUILayout.Space(listStyle.p_verticalMargin);
            return;
        }
        int elementToRemove = -1;
        
        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty element = list.GetArrayElementAtIndex(i);
            //object value = list[i];
            
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label(listStyle.p_label?.Invoke(i), GUILayout.Width(80));
            EditorGUILayout.PropertyField(element, GUIContent.none);

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = listStyle.p_removeColor;
            if (GUILayout.Button("-", GUILayout.Width(50)))
            {
                elementToRemove = i;
            }
            GUI.backgroundColor = oldColor;
            
            GUILayout.BeginVertical();
            GUI.backgroundColor = i == 0 ? oldColor * 0.8f : oldColor;
            if (GUILayout.Button("↑", GUILayout.Height(10), GUILayout.Width(50)))
            {
                if(i!=0)
                {
                    list.MoveArrayElement(i, i - 1);
                }
            }
            GUI.backgroundColor = i == list.arraySize-1 ? oldColor * 0.8f : oldColor;
            if (GUILayout.Button("↓", GUILayout.Height(10), GUILayout.Width(50)))
            {
                if(i!=list.arraySize-1)
                {
                    list.MoveArrayElement(i, i + 1);
                }
            }
            GUILayout.EndVertical();
            GUI.backgroundColor = oldColor;
            
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3);

            //list[i] = (T)value;
        }
        GUILayout.BeginHorizontal();
        GUI.backgroundColor = listStyle.p_addColor;
        if (GUILayout.Button("+"))
        {
            //list.Add(default(T));
            list.InsertArrayElementAtIndex(list.arraySize);
            SerializedProperty element = list.GetArrayElementAtIndex(list.arraySize-1);
            if (element.propertyType == SerializedPropertyType.ObjectReference)
            {
                element.objectReferenceValue = null;
            }
        }

        GUI.backgroundColor = listStyle.p_removeColor;
        if (list.arraySize > 0 && GUILayout.Button("-"))
        {
            list.DeleteArrayElementAtIndex(list.arraySize - 1);
        }
        GUI.backgroundColor = oldColor;
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Space(listStyle.p_verticalMargin);

        if (elementToRemove >= 0)
        {
            list.DeleteArrayElementAtIndex(elementToRemove);
            elementToRemove = -1;
        }
    }
    #endregion

    public static void DrawDashedLine(Vector3 start, Vector3 end, Color color, float dashLength = 4f, float gap = 2f)
    {
        Handles.color = color;
        
        Vector3 dir = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        float drawn = 0f;
        while (drawn < distance)
        {
            float segment = Mathf.Min(dashLength, distance - drawn);
            Handles.DrawLine(start+dir*drawn, start +dir* (drawn + segment));
            drawn += dashLength + gap;
        }
        Handles.color = Color.white;
    }

    public static Rect GetRectWithSize(float height, float width = 0)
    {
        if (height <= 0 && width <= 0)
            return EditorGUILayout.GetControlRect();
        else if (width <= 0)
            return GUILayoutUtility.GetRect(
                0, height,
                GUILayout.Height(height));
        else if (height <= 0)
            return GUILayoutUtility.GetRect(
                width, 0,
                GUILayout.Width(width));
        else
            return GUILayoutUtility.GetRect(
            width, height,
            GUILayout.Width(width),
            GUILayout.Height(height));
    }
    public static void DrawDashedRect(Rect rect, bool isHorizontal, Color color, float dashLength = 4f, float gap = 2f)
    {
        if(gap == 0)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, rect.height), color);
            return;
        }
        
        float drawn = 0f;
        if (isHorizontal)
        {
            while (drawn < rect.width)
            {
                float segmentLenght = Mathf.Min(dashLength, rect.width - drawn);
                EditorGUI.DrawRect(new Rect(rect.x + drawn, rect.y, segmentLenght, rect.height), color);
                drawn += segmentLenght + gap;
            }
        }
        else
        {
            while (drawn < rect.height)
            {
                float segmentLenght = Mathf.Min(dashLength, rect.height - drawn);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y + drawn, rect.width, segmentLenght), color);
                drawn += segmentLenght + gap;
            }
        }
    }

    public static void DrawDashedBorders(Rect rect, DashedBorderParameters border)
    {
        float offset =
                border.p_type == dashType.Interior ? 0 :
                border.p_type == dashType.Middle ? border.p_width / 2 :
                border.p_width;
            float inverseOffset = border.p_width - offset;
            
            //Horizontal Lines
            if(border.IsSideActive(0)) //top
            {
                float startX = border.IsSideActive(3) ? rect.x - offset : rect.x;
                float width = border.IsSideActive(3) && border.IsSideActive(1) ? rect.width +offset*2 :
                    border.IsSideActive(3) || border.IsSideActive(1) ? rect.width + offset:
                    rect.width;
                EditorUtilities.DrawDashedRect(
                    new Rect(startX, rect.y - offset, width, border.p_width),
                    true, border.p_color, border.p_segmentLenght, border.p_gapLenght);
            }
            if(border.IsSideActive(2)) //bottom
            {
                float startX = border.IsSideActive(3) ? rect.x - offset : rect.x;
                float width = border.IsSideActive(3) && border.IsSideActive(1) ? rect.width +offset*2 :
                    border.IsSideActive(3) || border.IsSideActive(1) ? rect.width + offset:
                    rect.width;
                EditorUtilities.DrawDashedRect(
                    new Rect(startX, rect.y + rect.height - border.p_width + offset, width, border.p_width),
                    true, border.p_color, border.p_segmentLenght, border.p_gapLenght);
            }
            
            //Vertical Lines
            if(border.IsSideActive(3)) //left
            {
                float startY = border.IsSideActive(0) ? rect.y + inverseOffset : rect.y;
                float height = border.IsSideActive(0) && border.IsSideActive(2) ? rect.height -inverseOffset*2 :
                    border.IsSideActive(0) || border.IsSideActive(2) ? rect.height - inverseOffset:
                    rect.height;
                EditorUtilities.DrawDashedRect(
                    new Rect(rect.x - offset, startY, border.p_width, height),
                    false, border.p_color, border.p_segmentLenght, border.p_gapLenght);
            }
            if(border.IsSideActive(1)) //right
            {
                float startY = border.IsSideActive(0) ? rect.y + inverseOffset : rect.y;
                float height = border.IsSideActive(0) && border.IsSideActive(2) ? rect.height -inverseOffset*2 :
                    border.IsSideActive(0) || border.IsSideActive(2) ? rect.height - inverseOffset:
                    rect.height;
                EditorUtilities.DrawDashedRect(
                    new Rect(rect.x + rect.width - border.p_width + offset, startY, border.p_width, height),
                    false, border.p_color, border.p_segmentLenght, border.p_gapLenght);
            }
    }

    public static Rect WrapInBlock(Action elementsToWrap, Color? backgroundColor = null, int hPadding = 8, int vPadding = 6)
    {
        Color color = backgroundColor ?? new Color(0.2f, 0.2f, 0.2f);

        Rect rect = EditorGUILayout.BeginVertical();
        if (Event.current.type == EventType.Repaint && rect.width > 0)
        {
            EditorGUI.DrawRect(rect, color);
        }

        GUILayout.Space(vPadding);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(hPadding);
        EditorGUILayout.BeginVertical();

        elementsToWrap?.Invoke();

        EditorGUILayout.EndVertical();
        GUILayout.Space(hPadding);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(vPadding);

        EditorGUILayout.EndVertical();

        return rect;
    }
    public static string SplitPascalCase(string input)
    {
        // Remplace chaque majuscule précédée d'une lettre par un espace + majuscule
        return Regex.Replace(input, "(?<!^)([A-Z])", " $1");
    }
    static void Swap<T>(List<T> list, int i, int j)
    {
        if (i == j) return;

        (list[i], list[j]) = (list[j], list[i]);
    }
}

public class EditorListDrawerStyle
{
    private GUIStyle _titleStyle;
    public GUIStyle p_titleStyle
    {
        get
        {
            if (_titleStyle == null)
                _titleStyle = new GUIStyle(EditorStyles.boldLabel);
            return _titleStyle;
        }
        set => _titleStyle = value;
    }

    public Func<int, string> p_label = (i) => "";

    public Color p_removeColor = new Color(0.8f, 0.3f, 0.3f);
    public Color p_addColor = new Color(0.3f, 0.8f, 0.3f);

    public float p_verticalMargin = 15;
}
