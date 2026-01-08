using UnityEditor;
using UnityEngine;

namespace Plugins.RotaryHeart.Editor
{
    public class SupportWindow : EditorWindow
    {
        int ToolBarIndex;

        private GUIContent assetName;
        private GUIContent support;
        private GUIContent contact;
        private GUIContent review;

        private GUIStyle labelStyle;
        private GUIStyle PublisherNameStyle;
        private GUIStyle ToolBarStyle;
        private GUIStyle GreyText;
        private GUIStyle ReviewBanner;

        void OnGUI()
        {
            maxSize = minSize = new Vector2(300, 400);

            EditorGUILayout.Space();
            GUILayout.Label(assetName, PublisherNameStyle);
            EditorGUILayout.Space();

            GUIContent[] toolbarOptions = new GUIContent[2];
            toolbarOptions[0] = support;
            toolbarOptions[1] = contact;

            ToolBarIndex = GUILayout.Toolbar(ToolBarIndex, toolbarOptions, ToolBarStyle, GUILayout.Height(50));

            switch (ToolBarIndex)
            {
                case 0:
                    EditorGUILayout.Space();

                    if (GUILayout.Button("Support Forum"))
                        Application.OpenURL("https://forum.unity.com/threads/released-serializable-dictionary.518178/");

                    EditorGUILayout.LabelField("Talk with others.", GreyText);

                    EditorGUILayout.Space();

                    if (GUILayout.Button("Wiki"))
                        Application.OpenURL("https://www.rotaryheart.com/Wiki.html");

                    EditorGUILayout.LabelField("Detailed code documentation.", GreyText);
                    break;

                case 1:
                    EditorGUILayout.Space();

                    if (GUILayout.Button("Email"))
                        Application.OpenURL("mailto:ma.rotaryheart@gmail.com?");

                    EditorGUILayout.LabelField("Get in touch.", GreyText);
                    break;
                default: break;
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(review, ReviewBanner, GUILayout.Height(30)))
                Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/account/downloads/search=Serialized%20Dictionary");
        }

        GUIContent IconContent(string text, string icon, string tooltip)
        {
            GUIContent content;

            if (string.IsNullOrEmpty(icon))
            {
                content = new GUIContent();
            }
            else
            {
                content = EditorGUIUtility.IconContent(icon);
            }

            content.text = text;
            content.tooltip = tooltip;
            return content;
        }

    }
}