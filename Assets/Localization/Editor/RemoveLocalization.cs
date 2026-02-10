using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class RemoveLocalizationFromScene
{
    [MenuItem("Tools/Localization/Remove Localization From Entire Scene")]
    public static void RemoveLocalization()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();

        int removedCount = 0;

        foreach (GameObject root in roots)
        {
            // Remove LocalizedLegacyText
            foreach (var loc in root.GetComponentsInChildren<LocalizedLegacyText>(true))
            {
                Undo.DestroyObjectImmediate(loc);
                removedCount++;
            }

            // Remove LocalizedTMPText
            foreach (var loc in root.GetComponentsInChildren<LocalizedTMPText>(true))
            {
                Undo.DestroyObjectImmediate(loc);
                removedCount++;
            }
        }

        Debug.Log($"Removed {removedCount} localization components from scene '{scene.name}'.");
    }
}
