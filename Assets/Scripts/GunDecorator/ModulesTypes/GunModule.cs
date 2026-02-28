using UnityEditor;
using UnityEngine;

namespace  GunDecorator
{
    public abstract class GunModule : MonoBehaviour
    {
        protected GunController _gunController;

        [SerializeField] private string moduleName = "New Module";
        [SerializeField] private Color moduleColor = Color.white;

        public string ModuleName => moduleName;
        public Color ModuleColor => moduleColor;

        public virtual void Initialize(GunController gun)
        {
            _gunController = gun;
        }
    }

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
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Rect rect = EditorGUILayout.GetControlRect(false, 26);

            EditorGUI.DrawRect(rect, moduleColorProp.colorValue * 0.6f);

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

            DrawPropertiesExcluding(serializedObject, "m_Script", "moduleName", "moduleColor");

            serializedObject.ApplyModifiedProperties();
        }    
    }
}
