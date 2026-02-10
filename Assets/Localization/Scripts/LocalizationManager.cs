using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;
    [SerializeField] private TextAsset localizationJson;


    Dictionary<string, Dictionary<string, string>> localizedData;
    string currentLangCode = "en";

    public event Action OnLanguageChanged;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
             Instance = this;
       DontDestroyOnLoad(gameObject);
        }
       
       Debug.Log("TEST");

        if (localizationJson == null)
        {
            Debug.LogError("Localization JSON not assigned!");
            return;
        }

        localizedData =
            JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(
                localizationJson.text
            );

        currentLangCode = PlayerPrefs.GetString("language");
        Debug.Log("Saved Language: " + currentLangCode);
        if (currentLangCode!="")
           SetLanguage(currentLangCode);
        else
           SetLanguage("en");
    }
    public void SetEnglish() => SetLanguage("en");
    public void SetPortuguese() => SetLanguage("pt-BR");
    public void SetRussian() => SetLanguage("ru");
    public void SetSpanish() => SetLanguage("sp");
    public void SetFrench() => SetLanguage("fr");
    public void SetArabic() => SetLanguage("ar");


    void LoadJson()
    {
        TextAsset json = Resources.Load<TextAsset>("Localization/Data/localization.json");

        if (json == null)
        {
            Debug.LogError("Localization JSON not found!");
            return;
        }

        localizedData =
            JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json.text);
    }

    public void SetLanguage(string langCode)
    {
        if (currentLangCode == langCode)
            return;

        currentLangCode = langCode;
        OnLanguageChanged?.Invoke();

        if(langCode == "en")
            PlayerPrefs.SetString("language", "en");
        else if(langCode == "pt-BR")
            PlayerPrefs.SetString("language", "pt-BR");
        else if(langCode == "ru")
            PlayerPrefs.SetString("language", "ru");
        else if(langCode == "sp")
            PlayerPrefs.SetString("language", "sp");
        else if(langCode == "fr")
            PlayerPrefs.SetString("language", "fr");
        else if(langCode == "ar")
            PlayerPrefs.SetString("language", "ar");



       // SoundManager.Instance.PlaySfx("Btn");
    }

    public string GetText(string key)
    {
        if (!localizedData.TryGetValue(key, out var langMap))
            return $"#{key}";

        if (!langMap.TryGetValue(currentLangCode, out var value))
            return $"#{key}_{currentLangCode}";

        return value;
    }
}
