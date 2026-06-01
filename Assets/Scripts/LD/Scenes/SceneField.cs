using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LD.Scenes
{
    [System.Serializable]
    public class SceneField
    {
#if UNITY_EDITOR
        [SerializeField]
        private UnityEditor.SceneAsset m_SceneAsset;
#endif

        [SerializeField]
        private string m_SceneName = "";

        public string SceneName => m_SceneName;

        public static implicit operator string(SceneField sceneField)
        {
            return sceneField.SceneName;
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);

            SerializedProperty sceneAsset = property.FindPropertyRelative("m_SceneAsset");
            SerializedProperty sceneName = property.FindPropertyRelative("m_SceneName");

            position = EditorGUI.PrefixLabel(
                position,
                GUIUtility.GetControlID(FocusType.Passive),
                label
            );

            sceneAsset.objectReferenceValue = EditorGUI.ObjectField(
                position,
                sceneAsset.objectReferenceValue,
                typeof(SceneAsset),
                false
            );

            if (sceneAsset.objectReferenceValue != null)
            {
                sceneName.stringValue =
                    ((SceneAsset)sceneAsset.objectReferenceValue).name;
            }

            EditorGUI.EndProperty();
        }
    }

#endif
}