using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyCoreSO))]
public class EnemyCoreSOInspector : Editor
{
    private GUIStyle _titleStyle;
    
    private SerializedProperty p_pinataType;
    
    private void OnEnable()
    {
        p_pinataType = serializedObject.FindProperty("p_pinataType");
        _titleStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
            fontSize = 18
        };
        _titleStyle.normal.textColor = Color.white;
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        GUILayout.Label("Energy Drop", _titleStyle);
        EditorUtilities.Draw("p_dropXpOrb", serializedObject);
        EditorUtilities.Draw("p_baseEnergyDropValue", serializedObject);

        EditorGUILayout.PropertyField(p_pinataType);
        if ((ChargeType)p_pinataType.enumValueFlag != ChargeType.None)
        {
            EditorUtilities.Draw("p_pinataEnergyDropValue", serializedObject);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}
