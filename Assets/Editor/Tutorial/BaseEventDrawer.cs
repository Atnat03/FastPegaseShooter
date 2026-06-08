#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Tuto.Triggers;
using UnityEditor;
using UnityEngine;

namespace Tuto.Editor
{
    public abstract class PolymorphicDrawer<TBase> : PropertyDrawer
    {
        private static readonly Dictionary<Type, List<Type>>  _typesCache = new();
        private static readonly Dictionary<Type, string[]>    _namesCache = new();

        protected abstract Color ColorForType(Type t);

        private List<Type> Types
        {
            get
            {
                Type key = GetType();
                if (_typesCache.TryGetValue(key, out var cached)) return cached;

                List<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                    .Where(t => t.IsSubclassOf(typeof(TBase)) && !t.IsAbstract)
                    .OrderBy(t => t.Name)
                    .ToList();

                string[] names = new[] { "— Choisir un type —" }
                    .Concat(types.Select(t =>
                    {
                        try
                        {
                            var inst = Activator.CreateInstance(t);
                            var prop = t.GetProperty("DisplayName");
                            return prop?.GetValue(inst) as string ?? t.Name;
                        }
                        catch { return t.Name; }
                    }))
                    .ToArray();

                _typesCache[key] = types;
                _namesCache[key] = names;
                return types;
            }
        }

        private string[] Names
        {
            get
            {
                _ = Types;
                return _namesCache[GetType()];
            }
        }

        private static bool IsManagedRef(SerializedProperty p)
            => p.propertyType == SerializedPropertyType.ManagedReference;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!IsManagedRef(property))
                return EditorGUI.GetPropertyHeight(property, label, true);

            if (property.managedReferenceValue == null)
                return EditorGUIUtility.singleLineHeight + 4f;

            float h = EditorGUIUtility.singleLineHeight + 6f;
            var child = property.Copy();
            var end   = property.GetEndProperty();
            if (child.NextVisible(true))
                while (!SerializedProperty.EqualContents(child, end))
                {
                    h += EditorGUI.GetPropertyHeight(child, true) + 2f;
                    if (!child.NextVisible(false)) break;
                }
            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsManagedRef(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            
            var dropRect = new Rect(position.x, position.y + 2f, position.width, EditorGUIUtility.singleLineHeight);

            int currentIdx = property.managedReferenceValue == null
                ? 0
                : Types.IndexOf(property.managedReferenceValue.GetType()) + 1;

            Color prev = GUI.backgroundColor;
            if (property.managedReferenceValue != null)
                GUI.backgroundColor = ColorForType(property.managedReferenceValue.GetType());

            int selected = EditorGUI.Popup(dropRect, currentIdx, Names);
            GUI.backgroundColor = prev;

            if (selected != currentIdx)
            {
                property.managedReferenceValue = selected == 0
                    ? null
                    : Activator.CreateInstance(Types[selected - 1]);
                property.serializedObject.ApplyModifiedProperties();
            }

            if (property.managedReferenceValue == null) { EditorGUI.EndProperty(); return; }

            float y = position.y + EditorGUIUtility.singleLineHeight + 6f;
            EditorGUI.indentLevel++;
            SerializedProperty child = property.Copy();
            SerializedProperty end   = property.GetEndProperty();
            if (child.NextVisible(true))
                while (!SerializedProperty.EqualContents(child, end))
                {
                    float fh = EditorGUI.GetPropertyHeight(child, true);
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, fh), child, true);
                    y += fh + 2f;
                    if (!child.NextVisible(false)) break;
                }
            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(BaseEvent), true)]
    public class BaseEventDrawer : PolymorphicDrawer<BaseEvent>
    {
        protected override Color ColorForType(Type t)
        {
            if (t == typeof(Event_Wait))     return Color.gray6;
            if (t == typeof(Event_Dialogue)) return Color.pink;
            if (t == typeof(Event_Notification)) return Color.cornflowerBlue;
            if (t == typeof(Event_OpenDoor)) return Color.lightGreen;
            if (t == typeof(Event_UnlockCapacity)) return Color.bisque;
            if (t == typeof(Event_ChangeFillAmount)) return Color.aquamarine;
            if (t == typeof(Event_StartSpawn)) return Color.darkSalmon;
            if (t == typeof(Event_TakeDamage)) return Color.indianRed;
            return Color.white;
        }
    }

    [CustomPropertyDrawer(typeof(BaseTrigger), true)]
    public class BaseTriggerDrawer : PolymorphicDrawer<BaseTrigger>
    {
        protected override Color ColorForType(Type t)
        {
            if (t == typeof(Trigger_BoxCollider))    return Color.green;
            if (t == typeof(Trigger_50PercentDap))    return Color.blue;
            if (t == typeof(Trigger_AllMobsDead))    return Color.yellow;
            if (t == typeof(Trigger_Heal))    return Color.red;
            
            return Color.white;
        }
    }
}
#endif