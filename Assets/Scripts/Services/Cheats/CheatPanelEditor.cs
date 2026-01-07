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
            if (ContainerHolder.CurrentContainer == null)
            {
                return;
            }
            
            GUILayout.Label("Service Debug Actions", EditorStyles.boldLabel);
            
            // 1. Find the service in the scene if not already set
            if (_service == null)
            {
                _service = ContainerHolder.CurrentContainer.Resolve<CheatService>();
                if (_service == null)
                {
                    EditorGUILayout.HelpBox("GameService not found in scene!", MessageType.Warning);
                    return;
                }
            }

            EditorGUILayout.Space();
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            // 2. Use Reflection to get all public methods
            // BindingFlags.DeclaredOnly prevents showing inherited methods like ToString() or GetHashCode()
            MethodInfo[] methods = _service.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (MethodInfo method in methods)
            {
                // We only want to show buttons for methods that don't require complex parameters
                ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length == 0)
                {
                    if (GUILayout.Button($"Invoke {method.Name}"))
                    {
                        method.Invoke(_service, null);
                    }
                }
                else
                {
                    // Optional: Handle simple parameters or just label them as "Unsupported"
                    EditorGUILayout.LabelField($"{method.Name} (Requires {parameters.Length} params)",
                        EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
#endif