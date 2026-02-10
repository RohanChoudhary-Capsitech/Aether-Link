using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Reflection;

public class LocalizationKeyFinder : EditorWindow
{
    private string searchKey = "";
    private Vector2 scroll;
    private readonly List<GameObject> results = new();

    [MenuItem("Tools/Localization/Find Localization Key")]
    public static void Open()
    {
        GetWindow<LocalizationKeyFinder>("Localization Key Finder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Find Localization Key Usage", EditorStyles.boldLabel);

        searchKey = EditorGUILayout.TextField("Localization Key", searchKey);

        if (GUILayout.Button("Search"))
            Search();

        EditorGUILayout.Space();

        if (results.Count == 0)
        {
            GUILayout.Label("No results.");
            return;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (GameObject go in results)
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(go.name, GUILayout.Width(200)))
            {
                Selection.activeGameObject = go;
                EditorGUIUtility.PingObject(go);
            }

            GUILayout.Label(GetFullPath(go));

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void Search()
    {
        results.Clear();

        if (string.IsNullOrWhiteSpace(searchKey))
            return;

        Scene scene = SceneManager.GetActiveScene();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (MonoBehaviour comp in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (comp == null) // missing script
                    continue;

                // Only inspect localization-related components
                if (!IsLocalizationComponent(comp))
                    continue;

                string key = SafeExtractKey(comp);
                if (key == searchKey)
                {
                    results.Add(comp.gameObject);
                }
            }
        }
    }

    private static bool IsLocalizationComponent(MonoBehaviour comp)
    {
        string typeName = comp.GetType().Name;
        return typeName.Contains("Localized");
    }

    private static string SafeExtractKey(MonoBehaviour comp)
    {
        var type = comp.GetType();

        // ---- Fields ----
        foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (field.FieldType != typeof(string))
                continue;

            try
            {
                var value = field.GetValue(comp) as string;
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            catch
            {
                // Ignore broken Unity fields
            }
        }

        // ---- Properties ----
        foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (prop.PropertyType != typeof(string) || !prop.CanRead)
                continue;

            if (prop.GetIndexParameters().Length > 0)
                continue; // skip indexers

            try
            {
                var value = prop.GetValue(comp, null) as string;
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            catch
            {
                // Ignore Unity-internal exceptions
            }
        }

        return null;
    }

    private static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform t = go.transform.parent;

        while (t != null)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }

        return path;
    }
}
