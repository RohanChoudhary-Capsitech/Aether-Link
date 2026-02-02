using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private List<LevelData> levels;
    // [SerializeField] private GameObject TimerUIObject;
    [SerializeField] private GameObject LevelsButton;

    private int currentLevelIndex = 0;
    private LevelData currentLevelData;
    private float levelTimer;
    private bool isLevelActive;
    private bool timerStarted;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // 🔥 AUTO LOAD FIRST LEVEL
        // LoadLevel(0);
    }
    
    void Update()
    {
        if (!isLevelActive) return;

        if (currentLevelData != null && currentLevelData.HasTimer())
        {
            if (!timerStarted) return;

            levelTimer -= Time.deltaTime;
            // Debug.Log($"⏳ Time: {levelTimer:F1}"); // Verified mechanic

            if (TimerUI.Instance != null)
            {
                TimerUI.Instance.UpdateTime(levelTimer);
            }

            if (levelTimer <= 0)
            {
                Debug.Log("⏰ TIME UP!");
                OnGameOver();
            }
        }
    }

    public void LoadLevel(int index)
    {
        LevelsButton.SetActive(true);
        if (index >= levels.Count)
        {
            Debug.Log("Finished all levels!");
            return;
        }

        currentLevelIndex = index;
        currentLevelData = levels[index];

        GridManager.Instance.LoadLevel(currentLevelData);
        SelectionManager.Instance.ResetForNewLevel(currentLevelData);
        
        // Timer Setup
        bool hasTimer = currentLevelData.HasTimer();
        timerStarted = false; // Wait for first click

        if (TimerUI.Instance != null)
        {
            TimerUI.Instance.EnableTimer(hasTimer);
            if (hasTimer)
            {
                TimerUI.Instance.SetTotalTime(currentLevelData.GetTimeLimit());
            }
        }

        if (hasTimer)
        {
            levelTimer = currentLevelData.GetTimeLimit();
            //TimerUIObject.SetActive(true);
            Debug.Log($"⏳ Timer Setup: {levelTimer}s (Waiting for click)");
        }
        else
        {
            Debug.Log("⏳ No Timer for this level");
        }
        isLevelActive = true;
    }

    public void StartTimer()
    {
        if (isLevelActive && !timerStarted)
        {
            timerStarted = true;
            Debug.Log("⏳ Timer Started!");
        }
    }


    // ---------------- RESTART SAME LEVEL ----------------
    public void RestartCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
        // UILineDrawer.Instance.StopAllLines(); // LoadLevel handles reset logic usually, but let's keep previous logic if needed? 
        // Actually LoadLevel calls GridManager which rebuilds.
        // The original RestartCurrentLevel did:
        // SelectionManager.Instance.ResetForNewLevel(levels[currentLevelIndex]);
        // GridManager.Instance.LoadLevel(levels[currentLevelIndex]);
        // UILineDrawer.Instance.StopAllLines();
        
        // My LoadLevel does the first two. taking care of UILineDrawer might be needed if not handled.
        // But LoadLevel calls GridManager which destroys cells.
        UILineDrawer.Instance.StopAllLines();
        
        Debug.Log("Restart Level: " + currentLevelIndex);
    }

    // ---------------- NEXT LEVEL ----------------
    public void OnLevelWin()
    {
        isLevelActive = false;
        //TimerUIObject.SetActive(false);
        LevelsButton.SetActive(false);
        int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 0);
        if (currentLevelIndex >= unlocked)
        {
            unlocked = currentLevelIndex + 1;
            PlayerPrefs.SetInt("UnlockedLevel", unlocked);
        }

        // Check if next level exists to auto-load or show menu?
        // Original code showed LevelSelectPanel.
        LevelSelectPanel.Instance.Show();
        // 🔥 Refresh all level buttons
        foreach (LevelButton btn in FindObjectsOfType<LevelButton>())
        {
             if(btn != null) btn.Refresh();
        }
    }

    public void HomeButtonPressed()
    {
        isLevelActive = false;
        LevelsButton.SetActive(false);
        //TimerUIObject.SetActive(false);
        LevelSelectPanel.Instance.Show();
        // 🔥 Refresh all level buttons
        foreach (LevelButton btn in FindObjectsOfType<LevelButton>())
        {
             if(btn != null) btn.Refresh();
        }
    }
    // ---------------- GAME OVER ----------------
    public void OnGameOver()
    {
        isLevelActive = false;
        LevelsButton.SetActive(false);
        // TimerUIObject.SetActive(false);
        Debug.Log("Game Over");
        if (GameOverPanel.gameOverPanel != null) 
            GameOverPanel.gameOverPanel.Show();
    }
   

}
