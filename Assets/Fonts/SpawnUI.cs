using UnityEngine;
using UnityEngine.UI;

public class SpawnUI : MonoBehaviour
{
    [Header("UI Prefab (must be UI element)")]
    public RectTransform uiPrefab;

    [Header("Optional: Target Canvas")]
    public Canvas targetCanvas;

    void Start()
    {
       
    }

    public void Spawn()
    {
        if (uiPrefab == null)
        {
            Debug.LogError("UI Prefab not assigned!");
            return;
        }

        // Auto-find canvas if not assigned
        if (targetCanvas == null)
        {
            targetCanvas = FindObjectOfType<Canvas>();
            if (targetCanvas == null)
            {
                Debug.LogError("No Canvas found in scene!");
                return;
            }
        }

        // Instantiate UI
        RectTransform instance =
            Instantiate(uiPrefab, targetCanvas.transform);

        // IMPORTANT: reset transform correctly
        instance.anchorMin = new Vector2(0.5f, 0.5f);
        instance.anchorMax = new Vector2(0.5f, 0.5f);
        instance.pivot = new Vector2(0.5f, 0.5f);

        instance.anchoredPosition = Vector2.zero;
        instance.localScale = Vector3.one;
    }
}
