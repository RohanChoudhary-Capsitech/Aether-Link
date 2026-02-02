using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private int levelIndex;
    [SerializeField] private Button button;
    [SerializeField] private GameObject LockedSprite;
    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
        LockedSprite.SetActive(true);
    }

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 0);
        bool isLocked = levelIndex > unlocked;
        
        if (LockedSprite != null)
            LockedSprite.SetActive(isLocked);
            
        button.interactable = !isLocked;
    }

    public void OnClick()
    {
        LevelManager.Instance.LoadLevel(levelIndex);
    }
}
