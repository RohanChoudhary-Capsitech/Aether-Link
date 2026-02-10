using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class LocalizationAllInOneTool
{
    private static readonly string[] PossibleKeyFieldNames =
    {
        "localizationKey",
        "key",
        "localizationID",
        "localizationId",
        "entryKey"
    };

    private const string OutputFolder = "Assets/Localization/Data";
    private const string OutputFileName = "SceneTextsForGPT.txt";

    private const string PromptHeader =
@"Generate ONLY valid JSON for a game localization file.

Constraints:
- No markdown
- No comments
- No explanations
- No trailing commas
- Preserve key order
- Use uppercase keys

Structure:
{
  ""KEY"": {
    ""en"": ""English"",
    ""pt-BR"": ""Portuguese (Brazil)"",
    ""ru"": ""Russian"",
    ""sp"": ""Spanish"",
    ""fr"": ""French"",
    ""ar"": ""Arabic""
  }
}

Entries:
";

    private static int numericKeyCounter;

    // ================= MENU =================

    [MenuItem("Tools/Localization/Run Full Localization Pipeline")]
    public static void RunAll()
    {
        numericKeyCounter = 1;

        Scene scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();

        HashSet<string> seenTexts = new HashSet<string>();
        List<string> exportLines = new List<string>();

        int added = 0;
        int keyed = 0;

       foreach (GameObject root in roots)
{
    // ---------- LEGACY TEXT ----------
    foreach (Text text in root.GetComponentsInChildren<Text>(true))
    {
        if (IsNumericOnly(text.text))
            continue;

        AddLocalizationIfMissing(text.gameObject, ref added);
        keyed += ProcessText(
            text.text,
            text.GetComponent<LocalizedLegacyText>(),
            seenTexts,
            exportLines
        );
    }

    // ---------- TMP ----------
    foreach (TextMeshProUGUI tmp in root.GetComponentsInChildren<TextMeshProUGUI>(true))
    {
        if (IsNumericOnly(tmp.text))
            continue;

        AddLocalizationIfMissing(tmp.gameObject, ref added);
        keyed += ProcessText(
            tmp.text,
            tmp.GetComponent<LocalizedTMPText>(),
            seenTexts,
            exportLines
        );
    }
}


        WriteExportFile(exportLines);

        Debug.Log(
            $"Localization pipeline complete for scene '{scene.name}'.\n" +
            $"Added components: {added}\n" +
            $"Keys assigned: {keyed}\n" +
            $"Exported entries: {exportLines.Count}"
        );
    }

    // ================= CORE LOGIC =================

    private static void AddLocalizationIfMissing(GameObject go, ref int count)
    {
        if (go.GetComponent<Text>() && !go.GetComponent<LocalizedLegacyText>())
        {
            Undo.AddComponent<LocalizedLegacyText>(go);
            count++;
        }

        if (go.GetComponent<TextMeshProUGUI>() && !go.GetComponent<LocalizedTMPText>())
        {
            Undo.AddComponent<LocalizedTMPText>(go);
            count++;
        }
    }
    private static bool IsNumericOnly(string text)
{
    if (string.IsNullOrWhiteSpace(text))
        return true;

    text = Regex.Replace(text, "<.*?>", "").Trim();
    return Regex.IsMatch(text, @"^\d+$");
}


    private static int ProcessText(string source, Object localizationComponent, HashSet<string> seen, List<string> export)
    {
        if (localizationComponent == null || string.IsNullOrWhiteSpace(source))
            return 0;

        source = Regex.Replace(source, "<.*?>", "").Trim();

        // Skip numeric-only
        

        SerializedObject so = new SerializedObject(localizationComponent);

        foreach (string field in PossibleKeyFieldNames)
        {
            SerializedProperty prop = so.FindProperty(field);
            if (prop == null || prop.propertyType != SerializedPropertyType.String)
                continue;

            if (!string.IsNullOrEmpty(prop.stringValue))
                return 0;

            string key;

            if (source.Split(' ').Length > 1)
            {
                key = numericKeyCounter.ToString();
                numericKeyCounter++;
            }
            else
            {
                key = GenerateKey(source);
            }

            prop.stringValue = key;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(localizationComponent);

            if (seen.Add(source))
                export.Add($"{key} = {source}");

            return 1;
        }

        return 0;
    }

    private static void WriteExportFile(List<string> lines)
    {
        if (!Directory.Exists(OutputFolder))
            Directory.CreateDirectory(OutputFolder);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine(PromptHeader);

        foreach (string line in lines)
            sb.AppendLine(line);

        string path = Path.Combine(OutputFolder, OutputFileName);
        File.WriteAllText(path, sb.ToString());

        AssetDatabase.Refresh();
    }

    private static string GenerateKey(string source)
    {
        source = Regex.Replace(source, @"[^a-zA-Z0-9]+", "_");
        source = source.Trim('_');
        return source.ToUpperInvariant();
    }
}
