using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class RenameFilesTool : EditorWindow
    {
        private string _phraseToReplace;
        private string _patternText;
        
        [MenuItem("Custom Tools/Rename Tool")]
        private static void OpenCreateVillagePopup()
        {
            RenameFilesTool window = ScriptableObject.CreateInstance<RenameFilesTool>();
            window.position = new Rect(UnityEngine.Device.Screen.width / 2f, UnityEngine.Device.Screen.height / 2f, 250, 250);
            window.ShowAuxWindow();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Phrase to be replaced");
            _phraseToReplace = EditorGUILayout.TextField(_phraseToReplace);

            GUILayout.Label("Pattern to replace phrase");
            _patternText = EditorGUILayout.TextField(_patternText);
            var parentPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (GUILayout.Button("Rename"))
            {
                ReplacePhrases(parentPath, _phraseToReplace, _patternText);
                this.Close();
            }
        }

        private static void ReplacePhrases(string path, string phrase, string pattern)
        {
            Regex pref = new Regex($@".*{phrase}.*(?<!\\.meta)$");
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (FileInfo f in files)
            {
                if (pref.IsMatch(f.Name))
                {
                    if (f.Directory != null)
                    {
                        string fullPath = f.Directory.ToString().Replace('\\', '/');
                        string updPath = fullPath.Substring(fullPath.IndexOf("Asset", StringComparison.Ordinal));
                        string newName = f.Name.Replace(phrase, pattern);
                        AssetDatabase.RenameAsset(updPath + "/" + f.Name, newName);
                    }

                    AssetDatabase.Refresh();
                }
            }
            AssetDatabase.SaveAssets();
        }
    }
}