using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Aquiris.ClassTypeReference.Editor
{
    [CustomPropertyDrawer(typeof(ClassTypeRef)), CustomPropertyDrawer(typeof(ClassTypeConstraintAttribute), true)]
    public sealed class ClassTypeReferencePropertyDrawer : PropertyDrawer
    {
        private static int _selectionControlId;
        private static string _selectedClassRef;
        public static Func<ICollection<Type>> ExcludedTypeCollectionGetter { get; set; }
        private static Dictionary<string, Type> _typeMap = new Dictionary<string, Type>();
        private static GUIContent _tempContent = new GUIContent();

        private static readonly int _controlHint = typeof(ClassTypeReferencePropertyDrawer).GetHashCode();
        private static readonly GenericMenu.MenuFunction2 _onSelectedTypeName = OnSelectedTypeName;

        private const string TYPE_REFERENCE_UPDATED = "TypeReferenceUpdated";

        private static List<Type> GetFilteredTypes(ClassTypeConstraintAttribute filter)
        {
            var types = new List<Type>();
            ICollection<Type> excludedTypes = ExcludedTypeCollectionGetter != null
                ? ExcludedTypeCollectionGetter()
                : null;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                FilterTypes(assembly, filter, excludedTypes, types);
            }

            types.Sort(SortNames);
            int SortNames(Type x, Type y)
            {
                return x.FullName.CompareTo(y.FullName);
            }

            return types;
        }

        private static void FilterTypes(Assembly assembly, ClassTypeConstraintAttribute filter, ICollection<Type> excludedTypes, List<Type> output)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsPublic || !type.IsClass)
                {
                    continue;
                }

                if (filter != null && !filter.IsConstraintSatisfied(type))
                {
                    continue;
                }

                if (excludedTypes != null && excludedTypes.Contains(type))
                {
                    continue;
                }

                output.Add(type);
            }
        }

        private static Type ResolveType(string classRef)
        {
            if (!_typeMap.TryGetValue(classRef, out Type type))
            {
                type = !string.IsNullOrEmpty(classRef)
                    ? Type.GetType(classRef)
                    : null;

                _typeMap[classRef] = type;
            }

            return type;
        }

        private static string DrawTypeSelectionControl(Rect position, GUIContent label, string classRef, ClassTypeConstraintAttribute filter)
        {
            if (label != null && label != GUIContent.none)
            {
                position = EditorGUI.PrefixLabel(position, label);
            }

            bool triggerDropDown = false;
            int controlId = GUIUtility.GetControlID(_controlHint, FocusType.Keyboard, position);
            switch (Event.current.GetTypeForControl(controlId))
            {
                case EventType.ExecuteCommand:
                    if (Event.current.commandName == TYPE_REFERENCE_UPDATED)
                    {
                        if (_selectionControlId == controlId)
                        {
                            if (classRef != _selectedClassRef)
                            {
                                classRef = _selectedClassRef;
                                GUI.changed = true;
                            }

                            _selectionControlId = 0;
                            _selectedClassRef = null;
                        }
                    }
                    break;
                case EventType.MouseDown:
                    if (GUI.enabled && position.Contains(Event.current.mousePosition))
                    {
                        GUIUtility.keyboardControl = controlId;
                        triggerDropDown = true;
                        Event.current.Use();
                    }
                    break;
                case EventType.KeyDown:
                    if (GUI.enabled && GUIUtility.keyboardControl == controlId)
                    {
                        if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Space)
                        {
                            triggerDropDown = true;
                            Event.current.Use();
                        }
                    }
                    break;
                case EventType.Repaint:
                    string[] classRefParts = classRef.Split(',');
                    _tempContent.text = classRefParts[0].Trim();
                    if (_tempContent.text == "")
                    {
                        _tempContent.text = "(None)";
                    }
                    else if (ResolveType(classRef) == null)
                    {
                        _tempContent.text += " {Missing}";
                    }

                    string[] parts = _tempContent.text.Split('.');
                    string lastWord = parts[parts.Length - 1];
                    EditorStyles.popup.Draw(position, new GUIContent(lastWord), controlId);
                    break;
            }

            if (triggerDropDown)
            {
                _selectionControlId = controlId;
                _selectedClassRef = classRef;
                List<Type> filteredTypes = GetFilteredTypes(filter);
                DisplayDropDown(position, filteredTypes, ResolveType(classRef), filter.Grouping);
            }

            return classRef;
        }

        private static void DrawTypeSelectionControl(Rect position, SerializedProperty property, GUIContent label, ClassTypeConstraintAttribute filter)
        {
            try
            {
                bool restoreShowMixedValue = EditorGUI.showMixedValue;
                EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
                property.stringValue = DrawTypeSelectionControl(position, label, property.stringValue, filter);
                EditorGUI.showMixedValue = restoreShowMixedValue;
            }
            finally
            {
                ExcludedTypeCollectionGetter = null;
            }
        }

        private static void DisplayDropDown(Rect position, List<Type> types, Type selectedType, EClassGrouping grouping)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("(None)"), selectedType == null, _onSelectedTypeName, null);
            menu.AddSeparator("");

            for (int i = 0; i < types.Count; ++i)
            {
                Type type = types[i];
                string menuLabel = FormatGroupedTypeName(type, grouping);
                if (string.IsNullOrEmpty(menuLabel))
                {
                    continue;
                }

                var content = new GUIContent(menuLabel);
                menu.AddItem(content, type == selectedType, _onSelectedTypeName, type);
            }

            menu.DropDown(position);
        }

        private static string FormatGroupedTypeName(Type type, EClassGrouping grouping)
        {
            string fullName = type.FullName;
            switch (grouping)
            {
                default:
                case EClassGrouping.None:
                    return fullName;
                case EClassGrouping.ByNamespace:
                    return fullName.Replace('.', '/');
                case EClassGrouping.ByNamespaceFlat:
                    int lastPeriodIndex = fullName.LastIndexOf('.');
                    if (lastPeriodIndex != -1)
                    {
                        fullName = $"{fullName.Substring(0, lastPeriodIndex)}/{fullName.Substring(lastPeriodIndex + 1)}";
                    }

                    return fullName;
                case EClassGrouping.ByAddComponentMenu:
                    object[] addComponentMenuAttributes = type.GetCustomAttributes(typeof(AddComponentMenu), false);
                    if (addComponentMenuAttributes.Length == 1)
                    {
                        return ((AddComponentMenu) addComponentMenuAttributes[0]).componentMenu;
                    }

                    return $"Scripts/{type.FullName.Replace('.', '/')}";
            }
        }

        private static void OnSelectedTypeName(object userData)
        {
            var selectedType = userData as Type;
            _selectedClassRef = ClassTypeRef.GetClassRef(selectedType);
            Event typeReferenceUpdatedEvent = EditorGUIUtility.CommandEvent(TYPE_REFERENCE_UPDATED);
            EditorWindow.focusedWindow.SendEvent(typeReferenceUpdatedEvent);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorStyles.popup.CalcHeight(GUIContent.none, 0);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawTypeSelectionControl(position, property.FindPropertyRelative("_classRef"), label, attribute as ClassTypeConstraintAttribute);
        }
    }
}
