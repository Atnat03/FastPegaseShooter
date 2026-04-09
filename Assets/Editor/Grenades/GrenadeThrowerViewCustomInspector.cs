using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrenadeThrowerView))]
public class GrenadeThrowerViewCustomInspector : Editor
{
    private GrenadeThrowerView view;
    private GrenadeThrower thrower;

    void OnEnable()
    {
        view = (GrenadeThrowerView)target;
        thrower = view.GetComponent<GrenadeThrower>();
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorUtilities.Draw("_imageCooldown", serializedObject);

        MagneticCharge magneticCharge = thrower.MagneticCharge;
        
        string propertyName = magneticCharge switch
        {
            MagneticCharge.Positive => "_impactParticlesPositive",
            MagneticCharge.Negative => "_impactParticlesNegative",
            _ => null
        };

        BoxGUI bgColor = magneticCharge switch
        {
            MagneticCharge.Positive => new BoxGUI("Positive", Color.red),
            MagneticCharge.Negative => new BoxGUI("Negative", Color.softBlue),
            _ => new BoxGUI("NULL", Color.white)
        };

        //Box de l'element
        
        Rect rect = EditorGUILayout.GetControlRect(false, 26);
        EditorGUI.DrawRect(rect, bgColor.color * 0.6f);

        GUIStyle centeredHeader = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter
        };
        
        GUI.Label(rect, bgColor.name, centeredHeader);
        
        if (propertyName != null) 
        {
            EditorGUILayout.Space(4);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Impact Particles", GUILayout.Width(120));

            SerializedProperty prop = serializedObject.FindProperty(propertyName);
            EditorGUILayout.PropertyField(prop, GUIContent.none);

            EditorGUILayout.EndHorizontal();
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}

public struct BoxGUI
{
    public string name;
    public Color color;
    
    public BoxGUI(string name, Color color)
    {
        this.name = name;
        this.color = color;
    }
}
