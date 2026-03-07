using UnityEngine;
using UnityEditor;
using CJUtils;

[CustomEditor(typeof(SwappableStateManager))]
public class SwappableStateManagerEditor : Editor
{
    private SwappableStateManager Target => target as SwappableStateManager;

    private Texture2D iconInfo;

    void OnEnable() {
        EditorUtils.LoadIcon(ref iconInfo, "_Help");
    }

    void OnDisable() {
        Resources.UnloadAsset(iconInfo);
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        EditorUtils.DrawSeparatorLine();

        using (new EditorGUILayout.HorizontalScope()) {
            if (GUILayout.Button("Get Swappable Children")) {
                Swappable[] swappableObjects = Target.GetComponentsInChildren<Swappable>(true);
                Target.EDITOR_SetSwappableObjects(swappableObjects);
            }

            string tooltip = "Populate the swappable objects list with all swappable children of this object;\n\n" +
                             "Inactive children are included in this operation;";
            GUIContent infoContent = new(iconInfo, tooltip);
            GUILayout.Label(infoContent, GUILayout.Width(20));
            GUILayout.Space(4);
        }
    }
}
