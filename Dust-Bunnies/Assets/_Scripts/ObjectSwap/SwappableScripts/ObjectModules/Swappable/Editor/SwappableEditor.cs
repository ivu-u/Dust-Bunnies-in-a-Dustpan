using UnityEngine;
using UnityEditor;
using CJUtils;

[CustomEditor(typeof(Swappable))]
public class SwappableEditor : Editor
{
    private Swappable Swappable => target as Swappable;

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
            if (GUILayout.Button("Prewarm States")) {
                Swappable.EDITOR_PrewarmStates();
            }
            string tooltip = "Instantiate any asset-bound swap bodies associated with this prefab into the scene, under " +
                             "the designated anchor transform;\n\n" +
                             "In this context, asset-bound references are merely prefab references which are not scene-bound " +
                             "(that is, they are not comprised of scene instances but the prefab asset itself);\n\n" +
                             "If not prewarmed, this component will instantiate said items at runtime. Consider prewarming " +
                             "for performance reasons;\n\n" +
                             "<b>Note:</b> If you are wondering what this button does, you may click on any field with a prefab reference in this component. " +
                             "If it pointed to an asset before, this button will generate a new prefab instance, and the aforementioned field will " +
                             "reference said instance instead!";
            GUIContent infoContent = new(iconInfo, tooltip);
            GUILayout.Label(infoContent, GUILayout.Width(20));
            GUILayout.Space(4);
        }
    }
}
