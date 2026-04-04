using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class EditorUtilities : Editor
{
    //for direct list access
    public static void DrawList<T>(List<T> list, string listTitle, EditorListDrawerStyle listStyle, ref bool isOpened)
    {
        Color oldColor = GUI.color;
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

            GUI.color = listStyle.p_removeColor;
            if (GUILayout.Button("-",  GUILayout.Width(50)))
            {
                elementToRemove = i;
            }
            
            GUILayout.BeginVertical();
            GUI.color = i == 0 ? oldColor * 0.8f : oldColor;
            if (GUILayout.Button("↑", GUILayout.Height(10), GUILayout.Width(50)))
            {
                if(i!=0)
                {
                    Swap(list, i, i-1);
                    //list.MoveArrayElement(i, i - 1);
                }
            }
            GUI.color = i == list.Count-1 ? oldColor * 0.8f : oldColor;
            if (GUILayout.Button("↓", GUILayout.Height(10), GUILayout.Width(50)))
            {
                if(i!=list.Count-1)
                {
                    Swap(list, i, i+1);
                    //list.MoveArrayElement(i, i + 1);
                }
            }
            GUILayout.EndVertical();
            GUI.color = oldColor;
            GUI.color = oldColor;
            EditorGUILayout.EndHorizontal();

            list[i] = (T)value;
        }
        GUILayout.BeginHorizontal();
        
        GUI.color = listStyle.p_addColor;
        if (GUILayout.Button("+"))
        {
            list.Add(default(T));
        }
        GUI.color = listStyle.p_removeColor;
        if (GUILayout.Button("-"))
        {
            if(list.Count > 0)
            {
                list.RemoveAt(list.Count - 1);
            }
        }
        GUI.color = oldColor;
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
        Color oldColor = GUI.color;
        
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
            
            GUI.color = listStyle.p_removeColor;
            if (GUILayout.Button("-",  GUILayout.Width(50)))
            {
                elementToRemove = i;
            }
            GUI.color = oldColor;
            
            GUILayout.BeginVertical();
            GUI.color = i == 0 ? oldColor * 0.8f : oldColor;
            if (GUILayout.Button("↑", GUILayout.Height(10), GUILayout.Width(50)))
            {
                if(i!=0)
                {
                    list.MoveArrayElement(i, i - 1);
                }
            }
            GUI.color = i == list.arraySize-1 ? oldColor * 0.8f : oldColor;
            if (GUILayout.Button("↓", GUILayout.Height(10), GUILayout.Width(50)))
            {
                if(i!=list.arraySize-1)
                {
                    list.MoveArrayElement(i, i + 1);
                }
            }
            GUILayout.EndVertical();
            GUI.color = oldColor;
            
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3);

            //list[i] = (T)value;
        }
        GUILayout.BeginHorizontal();
        
        GUI.color = listStyle.p_addColor;
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
        GUI.color = listStyle.p_removeColor;
        if (list.arraySize > 0 && GUILayout.Button("-"))
        {
            list.DeleteArrayElementAtIndex(list.arraySize - 1);
        }
        GUI.color = oldColor;
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Space(listStyle.p_verticalMargin);

        if (elementToRemove >= 0)
        {
            list.DeleteArrayElementAtIndex(elementToRemove);
            elementToRemove = -1;
        }
    }
    
    static void Swap<T>(List<T> list, int i, int j)
    {
        if (i == j) return;

        (list[i], list[j]) = (list[j], list[i]);
    }
    
    //Dessiner une properties directement
    public static void Draw(string propertyName, SerializedObject s)
    {
        SerializedProperty prop = s.FindProperty(propertyName);
        if (prop != null)
        {
            EditorGUILayout.PropertyField(prop);
            EditorGUILayout.Space(2);
        }
        else
        {
            EditorGUILayout.HelpBox($"Missing field: {propertyName}", MessageType.Warning);
        }
    }
    
    //Dessiner un outline autour d'un rect
    public static void DrawOutline(Rect rect, Color color, float thickness)
    {
        Color old = GUI.color; 
        GUI.color = color;

        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        GUI.color = old;
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
