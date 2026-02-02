using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    public static TimerUI Instance { get; private set; }

    [SerializeField] private Image timerFillImage;
    [SerializeField] private GameObject activeTimerContainer;
    [SerializeField] private GameObject inactiveTimerPlaceholder;

    private float totalTime = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EnableTimer(bool enable)
    {
        if (activeTimerContainer != null)
            activeTimerContainer.SetActive(enable);

        if (inactiveTimerPlaceholder != null)
            inactiveTimerPlaceholder.SetActive(!enable);
    }

    /// <summary>
    /// Call this ONCE when the timer starts
    /// </summary>
    public void SetTotalTime(float time)
    {
        totalTime = Mathf.Max(1f, time);
        UpdateTime(time);
    }

    /// <summary>
    /// Call this every frame / tick with remaining time
    /// </summary>
    public void UpdateTime(float remainingTime)
    {
        if (timerFillImage == null) return;

        remainingTime = Mathf.Clamp(remainingTime, 0f, totalTime);
        timerFillImage.fillAmount = remainingTime / totalTime;
    }
}
