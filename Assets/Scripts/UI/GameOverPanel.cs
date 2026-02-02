using UnityEngine;

public class GameOverPanel : MonoBehaviour
{
    public static GameOverPanel gameOverPanel;
    private void Awake()
    {
        if (gameOverPanel == null)
        {
            gameOverPanel = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        gameObject.SetActive(false);
    }
    public void Show()
        {
        this.gameObject.SetActive(true);
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
