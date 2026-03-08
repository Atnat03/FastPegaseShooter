using GunDecorator;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(GunModule), true)]
public class GunModuleCustomInspector : Editor
{
        private SerializedProperty moduleNameProp;
        private SerializedProperty moduleColorProp;

        private bool foldout = false;

        private void OnEnable()
        {
            moduleNameProp = serializedObject.FindProperty("moduleName");
            moduleColorProp = serializedObject.FindProperty("moduleColor");
            
            Texture2D moduleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Scripts/Gun_Main/ModuleGun_icon.png");
            if (moduleIcon != null)
            {
                EditorGUIUtility.SetIconForObject(target, moduleIcon);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GunModule module = (GunModule)target;
            
            Rect rect = EditorGUILayout.GetControlRect(false, 26);

            EditorGUI.DrawRect(rect, moduleColorProp.colorValue * 0.6f);
            
            if (string.IsNullOrEmpty(moduleNameProp.stringValue))
            {
                moduleNameProp.stringValue = target.GetType().Name;
            }

            if(moduleColorProp.colorValue == Color.clear)
            { 
                ChangeColor(module);
            }

            Event e = Event.current;
            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                foldout = !foldout;
                e.Use();
            }

            GUIStyle centeredHeader = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };

            GUI.Label(rect, moduleNameProp.stringValue, centeredHeader);

            EditorGUILayout.Space(4);

            if (foldout)
            {
                EditorGUILayout.PropertyField(moduleNameProp);
                EditorGUILayout.PropertyField(moduleColorProp);

                EditorGUILayout.Space(8);
            }

            DrawModuleWarnings(module);
            
            DrawPropertiesExcluding(serializedObject, "m_Script", "moduleName", "moduleColor");

            serializedObject.ApplyModifiedProperties();
        }

        private void ChangeColor(GunModule module)
        {
            if (module is IShootModule)
            {
                moduleColorProp.colorValue = Color.dodgerBlue;
            }
            else if (module is IReloadModule)
            {
                moduleColorProp.colorValue = Color.crimson;
            }
            else if (module is IRecoilModule)
            {
                moduleColorProp.colorValue = Color.wheat;
            }
            else if (module is ISecondModule)
            {
                moduleColorProp.colorValue = Color.darkGreen;
            }
            else if (module is IAmmoModule)
            {
                moduleColorProp.colorValue = Color.yellow;
            }
            else if (module is IHitMarkerModule)
            {
                moduleColorProp.colorValue = Color.magenta;
            }
        }

        private void DrawModuleWarnings(GunModule module)
        {
            if (module == null) return;

            var go = module.gameObject;

            if (module is ISecondModule)
            {
                IShootModule[] shootModules = go.GetComponents<IShootModule>();
                if (shootModules.Length == 0)
                {
                    EditorGUILayout.HelpBox(
                        "Attention : ce module secondaire ne fonctionnera pas sans module de tir !",
                        MessageType.Warning
                    );
                }
            }

            if (module is IShootModule)
            {
                IReloadModule reloadModules = go.GetComponent<IReloadModule>();
                if (reloadModules == null)
                {
                    EditorGUILayout.HelpBox(
                        "Attention : ce module de tir n'a pas de module de rechargement associé !",
                        MessageType.Warning
                    );
                }
            }
            
            if (module is IRecoilModule)
            {
                IShootModule[] shootModules = go.GetComponents<IShootModule>();
                
                if (shootModules.Length == 0)
                {
                    EditorGUILayout.HelpBox(
                        "Attention : ce module de noise ne fonctionnera pas sans module de tir !",
                        MessageType.Warning
                    );
                }
            }
            
            if (module is IReloadModule)
            {
                IShootModule[] shootModules = go.GetComponents<IShootModule>();
                
                if (shootModules.Length == 0)
                {
                    EditorGUILayout.HelpBox(
                        "Attention : ce module de recharge ne fonctionnera pas sans module de tir !",
                        MessageType.Warning
                    );
                }
            }
        }
    }