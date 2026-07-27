using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[InitializeOnLoad]
internal static class TextMeshProGuiMigration
{
    private const string ScriptPath = "Assets/_Project/Script/Editor/TextMeshProGuiMigration.cs";
    private const string MarkerPath = "Library/TextMeshProGuiMigration.done";

    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/SubGame.unity",
        "Assets/Scenes/BounceCollectionGameplaySubScene.unity",
        "Assets/Scenes/BounceCollectionLevel 1.unity"
    };

    static TextMeshProGuiMigration()
    {
        EditorApplication.delayCall += Run;
    }

    private static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
        {
            EditorApplication.delayCall += Run;
            return;
        }

        try
        {
            foreach (Scene loadedScene in GetLoadedScenes())
            {
                if (loadedScene.isDirty)
                    throw new InvalidOperationException($"Scene has unsaved changes: {loadedScene.path}");
            }

            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            int convertedCount = 0;

            try
            {
                foreach (string scenePath in ScenePaths)
                {
                    Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    convertedCount += ConvertScene(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(setup);
            }

            File.WriteAllText(MarkerPath, convertedCount.ToString());
            Debug.Log($"Converted {convertedCount} TextMesh components to TextMeshProUGUI.");
            AssetDatabase.DeleteAsset(ScriptPath);
        }
        catch (Exception exception)
        {
            File.WriteAllText(MarkerPath, $"ERROR{Environment.NewLine}{exception}");
            Debug.LogException(exception);
        }
    }

    private static int ConvertScene(Scene scene)
    {
        int convertedCount = 0;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            TextMesh[] legacyTexts = root.GetComponentsInChildren<TextMesh>(true);
            foreach (TextMesh legacyText in legacyTexts)
            {
                Convert(legacyText);
                convertedCount++;
            }
        }

        return convertedCount;
    }

    private static void Convert(TextMesh legacyText)
    {
        Transform legacyTransform = legacyText.transform;
        Transform parent = legacyTransform.parent;
        int siblingIndex = legacyTransform.GetSiblingIndex();
        Vector3 localPosition = legacyTransform.localPosition;
        Quaternion localRotation = legacyTransform.localRotation;
        Vector3 localScale = legacyTransform.localScale;
        bool active = legacyText.gameObject.activeSelf;
        string objectName = legacyText.gameObject.name;
        Renderer legacyRenderer = legacyText.GetComponent<Renderer>();

        var canvasObject = new GameObject($"{objectName}Canvas", typeof(RectTransform), typeof(Canvas));
        RectTransform canvasRect = (RectTransform)canvasObject.transform;
        canvasRect.SetParent(parent, false);
        canvasRect.SetSiblingIndex(siblingIndex);
        canvasRect.localPosition = localPosition;
        canvasRect.localRotation = localRotation;

        float scale = legacyText.fontSize > 0
            ? legacyText.characterSize / legacyText.fontSize
            : legacyText.characterSize;
        canvasRect.localScale = localScale * scale;
        canvasRect.sizeDelta = new Vector2(1000f, 200f);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        if (legacyRenderer != null)
        {
            canvas.sortingLayerID = legacyRenderer.sortingLayerID;
            canvas.sortingOrder = legacyRenderer.sortingOrder;
        }

        var textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        RectTransform textRect = (RectTransform)textObject.transform;
        textRect.SetParent(canvasRect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = legacyText.text;
        text.fontSize = legacyText.fontSize;
        text.color = legacyText.color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.richText = legacyText.richText;
        text.raycastTarget = false;

        canvasObject.SetActive(active);
        Object.DestroyImmediate(legacyText.gameObject);
    }

    private static Scene[] GetLoadedScenes()
    {
        var scenes = new Scene[SceneManager.sceneCount];
        for (int i = 0; i < scenes.Length; i++)
            scenes[i] = SceneManager.GetSceneAt(i);

        return scenes;
    }
}
