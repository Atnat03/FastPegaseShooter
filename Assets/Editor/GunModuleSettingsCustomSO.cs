using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

[CustomEditor(typeof(GunModuleSettingsSO))]
public class GunModuleSettingsCustomSO : Editor
{
    private Type[] availableTypes;

    private void OnEnable()
    {
        availableTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(GunSetting)) && !t.IsAbstract)
            .ToArray();
        
        Texture2D moduleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/Gun.png");
        if (moduleIcon != null)
        {
            EditorGUIUtility.SetIconForObject(target, moduleIcon);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        SerializedProperty listProp = serializedObject.FindProperty("modulesList");
        
        Color oldColor = GUI.color;
        
        GUI.backgroundColor = Color.green;

        if (GUILayout.Button("Add Module"))
        {
            GenericMenu menu = new GenericMenu();

            foreach (Type type in availableTypes)
            {
                menu.AddItem(new GUIContent(type.Name), false, () =>
                {
                    AddModule(type, listProp);
                });
            }

            menu.ShowAsContext();
        }

        GUI.backgroundColor = oldColor;
        
        EditorGUILayout.Space();

        for (int i = 0; i < listProp.arraySize; i++)
        {
            SerializedProperty element = listProp.GetArrayElementAtIndex(i);

            SerializedProperty nameProp = element.FindPropertyRelative("displayName");
            SerializedProperty colorProp = element.FindPropertyRelative("headerColor");

            EditorGUILayout.BeginVertical("box");

            Rect rect = GUILayoutUtility.GetRect(0, 25, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, colorProp.colorValue);

            GUIStyle centeredStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.black }
            };

            EditorGUI.LabelField(rect, nameProp.stringValue, centeredStyle);
            
            EditorGUILayout.PropertyField(element, true);
            
            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();
            
            oldColor = GUI.color;
            
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(24), GUILayout.Height(16)))
            {
                listProp.DeleteArrayElementAtIndex(i);
            }
            
            GUI.backgroundColor = oldColor;

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void AddModule(Type type, SerializedProperty listProp)
    {
        serializedObject.Update();

        int index = listProp.arraySize;
        listProp.InsertArrayElementAtIndex(index);

        SerializedProperty element = listProp.GetArrayElementAtIndex(index);
        object instance = Activator.CreateInstance(type);

        if (instance is ReloadSetting reload)
        {
            reload.displayName = "ReloadModule";
            reload.headerColor = Color.crimson;
        }
        
        if (instance is RecoilSetting recoil)
        {
            recoil.displayName = "RecoilModule";
            recoil.headerColor = Color.wheat;
        }
        
        if (instance is TemplateShootSetting template)
        {
            template.displayName = "TemplateShootModule";
            template.headerColor = Color.dodgerBlue;
        }
        
        if (instance is RaycastAmmoSetting raycast)
        {
            raycast.displayName = "RaycastAmmoModule";
            raycast.headerColor = Color.yellow;
        }

        
        if (instance is PhysicAmmoSetting physic)
        {
            physic.displayName = "PhysicAmmoModule";
            physic.headerColor = Color.yellow;
        }
        
        if (instance is S_ExplosifSetting s_explosif)
        {
            s_explosif.displayName = "ExplosifAmmoModule";
            s_explosif.headerColor = Color.darkGreen;
        }
        
        if (instance is S_NoiseSetting s_noise)
        {
            s_noise.displayName = "NoiseModule";
            s_noise.headerColor = Color.darkGreen;
        }
        
        if (instance is S_SalveSetting s_Salve)
        {
            s_Salve.displayName = "SalveShootModule";
            s_Salve.headerColor = Color.darkGreen;
        }
                
        if (instance is ChargedSalveSetting chargeS)
        {
            chargeS.displayName = "SalveChargedModule";
            chargeS.headerColor = Color.cyan;
        }
        
        if (instance is ChargedIncreaseNoiseSetting chargeI)
        {
            chargeI.displayName = "IncreaseNoiseChargedModule";
            chargeI.headerColor = Color.cyan;
        }
        
        if (instance is ChargedDecreaseNoiseSetting chargeD)
        {
            chargeD.displayName = "DecreaseNoiseChargedModule";
            chargeD.headerColor = Color.cyan;
        }

        element.managedReferenceValue = instance;

        serializedObject.ApplyModifiedProperties();
    }
}