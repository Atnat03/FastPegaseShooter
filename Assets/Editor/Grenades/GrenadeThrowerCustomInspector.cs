using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrenadeThrower))]
public class GrenadeThrowerCustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty enumProp = serializedObject.FindProperty("magneticCharge");

        //Enum
        DrawEnum(enumProp);
        
        //Other Variables
        EditorUtilities.Draw("_elementaryGrenadePrefab", serializedObject);
        EditorUtilities.Draw("_bridgeAnimation", serializedObject);
        EditorUtilities.Draw("_spawnPoint", serializedObject);
        EditorUtilities.Draw("_currentGun", serializedObject);
        EditorUtilities.Draw("_cooldown", serializedObject);
        EditorUtilities.Draw("_throwForce", serializedObject);
        EditorUtilities.Draw("_explosionRadius", serializedObject);
        EditorUtilities.Draw("_showGizmoOnSpawnPoint", serializedObject);
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawEnum(SerializedProperty enumProp)
    {
        MagneticCharge magneticCharge = (MagneticCharge)enumProp.enumValueIndex;

        Color GetColor(MagneticCharge e)
        {
            return e switch
            {
                MagneticCharge.Positive => Color.softRed,
                MagneticCharge.Negative => Color.softBlue,
                _ => Color.white
            };
        }

        EditorGUILayout.LabelField("Text Animation Type", EditorStyles.boldLabel);

        float width = EditorGUIUtility.currentViewWidth;

        float buttonWidth = 110f;
        float buttonHeight = 28f;
        float spacing = 6f;

        int columns = Mathf.Max(1, Mathf.FloorToInt(width / (buttonWidth + spacing)));

        int count = 0;

        GUILayout.BeginVertical();

        foreach (MagneticCharge type in System.Enum.GetValues(typeof(MagneticCharge)))
        {
            if (count % columns == 0)
                GUILayout.BeginHorizontal();

            bool isSelected = type == magneticCharge;

            Rect rect = GUILayoutUtility.GetRect(buttonWidth, buttonHeight);

            EditorGUI.DrawRect(rect, GetColor(type));

            if (rect.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rect, new Color(1,1,1,0.05f));
            }

            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState { textColor = Color.black }
            };

            GUI.Label(rect, type.ToString(), labelStyle);

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                enumProp.enumValueIndex = (int)type;
                GUI.changed = true;
                Event.current.Use();
            }

            if (isSelected)
            {
                EditorUtilities.DrawOutline(rect, Color.black, 2f);
            }
            
            count++;

            if (count % columns == 0)
                GUILayout.EndHorizontal();
        }

        if (count % columns != 0)
            GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }
}