# Unity Localization Tool

A lightweight, automated localization system for Unity that supports both Legacy Text and TextMeshPro (TMP). This tool streamlines the localization process by automating key generation and providing a workflow optimized for AI-assisted translation (e.g., ChatGPT).

## Features

*   **Dual Support**: Works with `Text` (Legacy) and `TextMeshProUGUI`.
*   **Auto-Setup**: One-click setup to find all text components in your scene and attach localization scripts.
*   **Auto-Keying**: Automatically generates keys for your text.
    *   Single words -> Readable keys (e.g., "START")
    *   Phrases -> Numeric keys (e.g., "1", "2") to avoid long, messy keys.
*   **ChatGPT Integration**: Exports all your scene text into a formatted prompt ready to be pasted into ChatGPT for instant translation into multiple languages.
*   **Runtime Switching**: efficient language switching at runtime with event-driven updates.

## Quick Start Guide

### 1. Setup the Manager
1.  Locate `LocalizationManager.cs` (in `Assets/Localization/Scripts`).
2.  Create a GameObject in your first scene (e.g., "LocalizationManager").
3.  Attach the `LocalizationManager` script to it.
4.  (Optional) The manager will Persist (`DontDestroyOnLoad`).

### 2. Automating Keys & Components
*Note: You don't need to manually attach `LocalizedLegacyText` or `LocalizedTMPText` components.*

1.  Open the scene you want to localize.
2.  Go to the top menu: **Tools > Localization > Run Full Localization Pipeline**.
3.  The tool will:
    *   Find all Text/TMP objects.
    *   Attach the appropriate component script (`LocalizedLegacyText` or `LocalizedTMPText`).
    *   Generate a key based on the current text.
    *   **Export** all text to a file for translation.

### 3. Getting Translations (The Magic Part) 🪄
After running the pipeline, a file is created at:
`Assets/Localization/Data/SceneTextsForGPT.txt`

1.  Open `SceneTextsForGPT.txt`.
2.  Copy the **entire content**.
3.  Paste it into ChatGPT (or Claude/Gemini).
4.  The AI will generate a strict JSON response with translations for:
    *   English (`en`)
    *   Portuguese (`pt-BR`)
    *   Russian (`ru`)
    *   Spanish (`sp`)
    *   French (`fr`)
    *   Arabic (`ar`)

### 4. Import Translations
1.  Copy the JSON output from the AI.
2.  Paste it into your project's JSON file: `Assets/Localization/Data/localization.json`.
3.  **Important**: Assign this `localization.json` TextAsset to the `Localization Json` field on your `LocalizationManager` GameObject in the inspector.

## Runtime Usage

### Switching Languages
Call the `SetLanguage` method on the instance.

```csharp
// Helper methods
LocalizationManager.Instance.SetEnglish();
LocalizationManager.Instance.SetSpanish();
LocalizationManager.Instance.SetRussian();

// Or by code
LocalizationManager.Instance.SetLanguage("fr");
```

### Getting Text in Code
If you need to localize strings dynamically in scripts:

```csharp
string localizedString = LocalizationManager.Instance.GetText("KEY_NAME");
```

## Folder Structure

*   `Scripts/`: Core logic (`LocalizationManager`, `LocalizedText` wrappers).
*   `Editor/`: The automation tools (`LocalizationAllInOneTool`).
*   `Data/`: Stores the `localization.json` and the GPT prompt export (`SceneTextsForGPT.txt`).

## Technical Details

*   **Storage**: Translations are stored in a single JSON dictionary `Dictionary<string, Dictionary<string, string>>`.
*   **Persistence**: Language preference is saved in `PlayerPrefs` under the key `"language"`.
*   **Events**: Components subscribe to `OnLanguageChanged` to update text immediately without polling.
