#if UNITY_EDITOR
using System.Reflection;
using Installers;
using UnityEditor;
using UnityEngine;

namespace Services.Cheats
{
    public class CheatPanelEditor : EditorWindow
    {
        private CheatService _service;
        private Vector2 _scrollPos;

        [MenuItem("Tools/Show Cheats Panel")]
        public static void ShowWindow()
        {
            GetWindow<CheatPanelEditor>("Cheat Panel");
        }

        private void OnGUI()
        {
            // Safety check for Zenject container
            if (Application.isPlaying == false || ContainerHolder.CurrentContainer == null)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use Cheats.", MessageType.Info);
                return;
            }
            
            if (_service == null)
            {
                _service = ContainerHolder.CurrentContainer.TryResolve<CheatService>();
                if (_service == null) return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            // --- SECTION 1: PROPERTIES (Settings) ---
            GUILayout.Label("Cheat Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Get public properties
            var props = _service.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props)
            {
                if (!prop.CanWrite) continue; // Skip read-only properties

                if (prop.PropertyType == typeof(int))
                {
                    int val = (int)prop.GetValue(_service);
                    int newVal = EditorGUILayout.IntField(prop.Name, val);
                    if (val != newVal) prop.SetValue(_service, newVal);
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    bool val = (bool)prop.GetValue(_service);
                    bool newVal = EditorGUILayout.Toggle(prop.Name, val);
                    if (val != newVal) prop.SetValue(_service, newVal);
                }
                // Add more types here (float, string, etc.) if needed
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // --- SECTION 2: METHODS (Actions) ---
            GUILayout.Label("Actions", EditorStyles.boldLabel);
            
            MethodInfo[] methods = _service.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (MethodInfo method in methods)
            {
                // Ignore property getters/setters (they start with get_ or set_)
                if (method.IsSpecialName) continue;
                
                ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length == 0)
                {
                    if (GUILayout.Button($"{method.Name}", GUILayout.Height(25)))
                    {
                        method.Invoke(_service, null);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
#endif