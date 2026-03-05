using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnergyManager), true)]
public class EnergyManagerCustomInpector : Editor
{
	private SerializedProperty energyMax;
	private SerializedProperty valueOneBar;
	
	private void OnEnable()
	{
		energyMax = serializedObject.FindProperty("_energyMax");
		valueOneBar = serializedObject.FindProperty("_valueOneBar");
            
		Texture2D moduleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/Energy.png");
		if (moduleIcon != null)
		{
			EditorGUIUtility.SetIconForObject(target, moduleIcon);
		}
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();
    
		EditorGUILayout.PropertyField(energyMax);
		EditorGUILayout.PropertyField(valueOneBar);
    
		GUILayout.Space(4);
    
		DrawCalculWarnings();
    
		GUILayout.Space(8);
    
		DrawPropertiesExcluding(serializedObject, "m_Script", "_energyMax", "_valueOneBar");
    
		serializedObject.ApplyModifiedProperties();
	}

	private void DrawCalculWarnings()
	{
		if (energyMax.floatValue % valueOneBar.floatValue != 0)
		{
			EditorGUILayout.HelpBox(
				"Attention : energyMax doit être divisible par valueOneBar !",
				MessageType.Warning
			);
		}
		else
		{
			GUILayout.Label("Nombre de sous barres d'energies : " + (int)energyMax.floatValue / valueOneBar.floatValue);
		}
	}
}
