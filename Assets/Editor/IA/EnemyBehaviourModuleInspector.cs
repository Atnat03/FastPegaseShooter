using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Internal;

[CustomEditor(typeof(EnemyBehaviourModule))]
public class EnemyBehaviourModuleInspector : Editor
{
    protected GUIStyle _centeredTitleStyle;
    protected GUIStyle _titleStyle;
    
    private SerializedProperty scriptProperty;
    protected virtual void OnEnable()
    {
        _centeredTitleStyle = new GUIStyle
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
        };
        _centeredTitleStyle.normal.textColor = Color.white;
        _titleStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
            fontSize = 18
        };
        _titleStyle.normal.textColor = Color.white;
        
        scriptProperty = serializedObject.FindProperty("m_Script");
        
        SetIcon();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(scriptProperty);
        EditorGUI.EndDisabledGroup();
        
        DrawCustomFields();

        if (GetDrawnPropertyAmount(serializedObject, "m_Script") > 0)
        {
            GUILayout.Space(10);
            Rect lineRect = EditorUtilities.GetRectWithSize(height: 4f);
            EditorUtilities.DrawDashedRect(lineRect, true, Color.white);
            GUILayout.Space(10);
            GUILayout.Label("Script Specific Variables", _titleStyle);
            GUILayout.Space(10);
            
            DrawPropertiesExcluding(serializedObject, "m_Script");
        }
        
        serializedObject.ApplyModifiedProperties();
    }

    protected virtual void SetIcon()
    {
        Texture2D moduleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/AI/Behaviour.png");
        if (moduleIcon != null)
        {
            EditorGUIUtility.SetIconForObject(target, moduleIcon);
        }
    }

    protected void ShowModuleTitle(string title, Color color, DashedBorderParameters border)
    {
        GUILayout.Space(10);
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(60));
        EditorGUI.DrawRect(rect, color);
        GUI.Label(rect, title, _centeredTitleStyle);
        GUILayout.Space(10);
        if (border.p_activeSides != DashedBorderParameters.NONE_SIDE)
        {
            EditorUtilities.DrawDashedBorders(rect, border);
        }
    }

    protected virtual void DrawCustomFields() {}

    int GetDrawnPropertyAmount(SerializedObject obj, params string[] excluded)
    {
        int amount = 0;
        
        SerializedProperty property = obj.GetIterator();
        bool enterChildren = true;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;
            if(System.Array.Exists(excluded, e => e == property.name))
                continue;
            
            amount++;
        }
        return amount;
    }
    
    //Only to give icone to abstract class in project
    [InitializeOnLoad]
    public static class EnemyBehaviourModuleIconSetter
    {
        static EnemyBehaviourModuleIconSetter()
        {
            MonoScript monoScript = GetMonoScriptFromType(typeof(EnemyBehaviourModule));
            if (monoScript == null) return;

            Texture2D moduleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/AI/Behaviour.png");
            if (moduleIcon != null)
            {
                EditorGUIUtility.SetIconForObject(monoScript, moduleIcon);
            }
            
            monoScript = GetMonoScriptFromType(typeof(EnemyCore));
            if (monoScript == null) return;

            moduleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/AI/core.png");
            if (moduleIcon != null)
            {
                EditorGUIUtility.SetIconForObject(monoScript, moduleIcon);
            }

            foreach (Type t in GetDerivedTypes(typeof(EnemyMovingModule)))
            {
                monoScript = GetMonoScriptFromType(t);
                if (monoScript == null) return;

                moduleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/AI/Moving.png");
                if (moduleIcon != null)
                {
                    EditorGUIUtility.SetIconForObject(monoScript, moduleIcon);
                }
            }
            foreach (Type t in GetDerivedTypes(typeof(EnemyTargetModule)))
            {
                monoScript = GetMonoScriptFromType(t);
                if (monoScript == null) return;

                moduleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/AI/Target.png");
                if (moduleIcon != null)
                {
                    EditorGUIUtility.SetIconForObject(monoScript, moduleIcon);
                }
            }
            foreach (Type t in GetDerivedTypes(typeof(EnemyAttackModule)))
            {
                monoScript = GetMonoScriptFromType(t);
                if (monoScript == null) return;

                moduleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/AI/Attack.png");
                if (moduleIcon != null)
                {
                    EditorGUIUtility.SetIconForObject(monoScript, moduleIcon);
                }
            }
            foreach (Type t in GetDerivedTypes(typeof(EnemyLifeModule)))
            {
                monoScript = GetMonoScriptFromType(t);
                if (monoScript == null) return;

                moduleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/AI/Life.png");
                if (moduleIcon != null)
                {
                    EditorGUIUtility.SetIconForObject(monoScript, moduleIcon);
                }
            }
        }

        private static MonoScript GetMonoScriptFromType(System.Type type)
        {
            string[] guids = AssetDatabase.FindAssets("t:MonoScript");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == type)
                {
                    return script;
                }
            }
            return null;
        }
        
        public static List<Type> GetDerivedTypes(Type baseType)
        {
            return AppDomain.CurrentDomain.GetAssemblies() // toutes les assemblies chargées
                .SelectMany(a => a.GetTypes())            // tous les types dans chaque assembly
                .Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t)) // classe concrète héritant de baseType
                .ToList();
        }
    }
}

public enum dashType
{
    Interior,
    Middle,
    Exterior
};

public struct DashedBorderParameters
{
    public const byte ALL_SIDES = 0b1111;
    public const byte TOP_DOWN_SIDES = 0b1010;
    public const byte LEFT_RIGHT_SIDES = 0b0101;
    public const byte NONE_SIDE = 0b0000;

    
    
    public float p_width;
    public Color p_color;
    public float p_segmentLenght;
    public float p_gapLenght;

    public byte p_activeSides;
    public dashType p_type;

    public DashedBorderParameters(float width,
        Color? color = null,
        byte sides = ALL_SIDES,
        dashType type = dashType.Interior,
        float segmentLenght = 4,
        float gapLenght = 2)
    {
        p_width = width;
        p_color = color ?? Color.white;
        p_activeSides = sides;
        p_type = type;
        p_segmentLenght = segmentLenght;
        p_gapLenght = gapLenght;
    }

    public bool IsSideActive(int side) => (p_activeSides & (1 << side)) != 0;
}
