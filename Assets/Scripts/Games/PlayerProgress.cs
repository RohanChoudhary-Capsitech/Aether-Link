using UnityEngine;

public class PlayerProgress : MonoBehaviour
{
    public static PlayerProgress Instance;

    [SerializeField] private int startKeys = 2;
    private int currentKeys;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentKeys = startKeys;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool UseKey()
    {
        currentKeys--;
        Debug.Log("Keys left: " + currentKeys);
        return currentKeys > 0;
    }

    public int GetKeys() => currentKeys;
}
