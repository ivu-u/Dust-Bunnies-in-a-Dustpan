using UnityEngine;
using UnityEditor;

namespace CJUtils {

    /// Took these helpers from my utils, it's a bit chunky so I didn't include the
    /// whole thing. If you'd like a bunch of cool editor helpers though, lmk and
    /// I'll bring them in o7
    /// - Carlos;

    public static class EditorUtils {
        public static void DrawSeparatorLine() {
            GUILayout.Space(4);
            Rect rect = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true));
            rect.height = 1;
            rect.xMin = 0;
            rect.xMax = EditorGUIUtility.currentViewWidth;
            EditorGUI.DrawRect(rect, Color.gray);
            GUILayout.Space(4);
        }

        public static Texture2D FetchIcon(string iconName) {
            return (Texture2D) EditorGUIUtility.IconContent(iconName).image;
        }

        public static void LoadIcon(ref Texture2D icon, string identifier) {
            icon = icon != null ? icon : FetchIcon(identifier);
        }
    }
}
