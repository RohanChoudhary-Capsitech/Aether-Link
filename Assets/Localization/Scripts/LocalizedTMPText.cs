using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedTMPText : MonoBehaviour
{
    [SerializeField] string localizationKey;
    TextMeshProUGUI text;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        if (LocalizationManager.Instance == null)
        {
            Debug.LogError(
                $"LocalizationManager not found for {gameObject.name}",
                this
            );
            return;
        }

        LocalizationManager.Instance.OnLanguageChanged += UpdateText;
        UpdateText();
    }

    void OnDisable()
    {
        if (LocalizationManager.Instance == null)
            return;

        LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
    }


    void UpdateText()
    {
        text.text = LocalizationManager.Instance.GetText(localizationKey);
    }
}
