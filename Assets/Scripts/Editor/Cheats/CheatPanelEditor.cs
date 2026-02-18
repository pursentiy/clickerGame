#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Installers;
using Services.Cheats;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor.Cheats
{
    public class CheatPanelEditor : EditorWindow
    {
        private CheatService _service;
        private Vector2 _scrollPos;

        private const string DefaultGroupName = "General";

        [MenuItem("Tools/Show Cheats Panel")]
        public static void ShowWindow()
        {
            GetWindow<CheatPanelEditor>("Cheat Panel");
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Only close when changing scene in edit mode; keep panel open when entering Play mode
            if (!Application.isPlaying)
                Close();
        }

        private static string GetGroupName(MemberInfo member)
        {
            var attr = Attribute.GetCustomAttribute(member, typeof(CheatGroupAttribute)) as CheatGroupAttribute;
            return attr != null ? attr.GroupName : DefaultGroupName;
        }

        private void OnGUI()
        {
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

            var type = _service.GetType();

            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanWrite)
                .Cast<MemberInfo>()
                .ToList();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName && m.GetParameters().Length == 0)
                .Cast<MemberInfo>()
                .ToList();

            var allMembers = props.Concat(methods).ToList();
            var groupNames = allMembers.Select(GetGroupName).Distinct().OrderBy(s => s).ToList();

            foreach (var groupName in groupNames)
            {
                GUILayout.Label(groupName, EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                foreach (var member in allMembers.Where(m => GetGroupName(m) == groupName))
                {
                    if (member is PropertyInfo prop)
                        DrawProperty(prop);
                    else if (member is MethodInfo method)
                        DrawMethod(method);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(8);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawProperty(PropertyInfo prop)
        {
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
            else if (prop.PropertyType == typeof(float))
            {
                var val = (float)prop.GetValue(_service);
                var newVal = EditorGUILayout.FloatField(prop.Name, val);
                if (!Mathf.Approximately(val, newVal)) prop.SetValue(_service, newVal);
            }
            else if (prop.PropertyType.IsEnum)
            {
                var val = (Enum)prop.GetValue(_service);
                var newVal = EditorGUILayout.EnumPopup(prop.Name, val);
                if (!Equals(val, newVal)) prop.SetValue(_service, newVal);
            }
        }

        private void DrawMethod(MethodInfo method)
        {
            if (GUILayout.Button(method.Name, GUILayout.Height(25)))
                method.Invoke(_service, null);
        }
    }
}
#endif