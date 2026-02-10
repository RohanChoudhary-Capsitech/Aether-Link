using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    public static TimerUI Instance { get; private set; }

    [SerializeField] private Image timerFillImage;
    [SerializeField] private GameObject activeTimerContainer;
    [SerializeField] private GameObject inactiveTimerPlaceholder;

    private float totalTime = 1f;
    private float remainingTime;
    

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void EnableTimer(bool enable)
    {
        activeTimerContainer?.SetActive(enable);
        inactiveTimerPlaceholder?.SetActive(!enable);
    }

    public void SetTotalTime(float time)
    {
        totalTime = Mathf.Max(1f, time);
        remainingTime = totalTime;
        UpdateFill();
    }

    public void UpdateTime(float timeLeft)
    {
        remainingTime = Mathf.Clamp(timeLeft, 0f, totalTime);
        UpdateFill();
    }

    /// <summary>
    /// Power-up: adds time instantly
    /// Can be called anytime
    /// </summary>
    public void AddTime(float extraTime)
    {
        remainingTime += extraTime;
        remainingTime = Mathf.Clamp(remainingTime, 0f, totalTime);
        UpdateFill();
    }

    private void UpdateFill()
    {
        if (timerFillImage == null) return;
        timerFillImage.fillAmount = remainingTime / totalTime;
    }
}
