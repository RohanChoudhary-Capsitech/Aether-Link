using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LocalizedLegacyText : MonoBehaviour
{
    [SerializeField] string localizationKey;
    Text text;

    void Awake()
    {
        text = GetComponent<Text>();
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
        Debug.Log("Called");
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
        Debug.Log("updated");
        text.text = LocalizationManager.Instance.GetText(localizationKey);
    }
}
