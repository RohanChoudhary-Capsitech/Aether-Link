using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image), typeof(Button))]
public class Cell : MonoBehaviour
{
    public int row;
    public int col;

    public int spriteIndex = -1;
    public CellLayoutType cellType;   // ✅ correct enum

    private Image image;
    private Button button;

    void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    // ================= SETUP =================
    // ================= SETUP =================
    // ================= SETUP =================
    public bool IsLocked { get; private set; }

   
    public void SetupNormal(int index, Sprite sprite)
    {
        cellType = CellLayoutType.Normal;   // ✅ FIXED
        spriteIndex = index;
        IsLocked = false;

        image.sprite = sprite;
        image.color = Color.white;
        image.enabled = true;

        button.interactable = true;
    }

    public void SetLocked(bool locked)
    {
        IsLocked = locked;
        if (IsLocked)
        {
            // Visual for locked
            image.color = new Color(0.8f, 0.8f, 0.8f); // Dimmed
            // Add outline or overlay if possible, for now just dim
             if (GetComponent<Outline>() == null)
                gameObject.AddComponent<Outline>().effectColor = Color.red;
             
             GetComponent<Outline>().enabled = true;
        }
        else
        {
             image.color = Color.white;
             if (GetComponent<Outline>() != null)
                GetComponent<Outline>().enabled = false;
        }
        
        // Locked cells are NOT interactable for selection, but are visible
        // Actually, if we want to click it to see "Locked", we might keep it interactable but fail isValid?
        // User says "require clearing ... to unlock". So they are likely passive obstacles.
        // Let's make them not interactable.
        button.interactable = !locked; 
    }

    public void SetupBlock()
    {
        cellType = CellLayoutType.Block;
        spriteIndex = -1;
        IsLocked = false;

        image.sprite = null;
        image.color = Color.black;
        image.enabled = true;

        button.interactable = false;
    }

    public void SetupEmpty()
    {
        cellType = CellLayoutType.Empty;
        spriteIndex = -1;
        IsLocked = false;

        image.sprite = null;
        image.enabled = false;

        button.interactable = false;
    }




    // ================= HELPERS =================

    public bool IsPlayable()
    {
        if (IsLocked) return false;

        return cellType == CellLayoutType.Normal;
    }

    public bool IsEmpty()
    {
        return cellType == CellLayoutType.Empty;
    }

    public bool IsBlock()
    {
        return cellType == CellLayoutType.Block;
    }


    // ================= CLICK =================
    void OnClick()
    {


        SelectionManager.Instance.Select(this);
    }


    // ================= CLEAR =================

    public void Clear()
    {
        SetupEmpty();
    }

    public void SetInteractable(bool value)
    {
        if (button != null)
            button.interactable = value;
    }



    // ================= HIGHLIGHT =================
    private Coroutine highlightRoutine;

    public void PlayHighlightAnimation()
    {
        if (highlightRoutine != null) StopCoroutine(highlightRoutine);
        highlightRoutine = StartCoroutine(HighlightRoutine());
    }

    private IEnumerator HighlightRoutine()
    {
        Color original = image.color;
        Color highlight = Color.yellow; // Or any distinct color
        
        float duration = 0.5f;
        float elapsed = 0f;

        // Pulse 5 times (0.5s * 5 = 2.5s)
        for(int i=0; i<5; i++)
        {
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.PingPong(elapsed * 2, 1f);
                image.color = Color.Lerp(original, highlight, t);
                yield return null;
            }
        }
        
        image.color = original;
    }
}
