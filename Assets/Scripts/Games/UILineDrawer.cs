using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILineDrawer : MonoBehaviour
{
    public static UILineDrawer Instance;

    [Header("References")]
    [SerializeField] private Image linePrefab;
    [SerializeField] private RectTransform boardRect;

    [Header("Visual")]
    [SerializeField] private float thickness = 8f;
    [SerializeField] private float segmentDelay = 0.12f;

    private readonly List<Image> active = new();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
   

    // =================================================
    // PUBLIC ENTRY
    // =================================================
    public void DrawPath(
        List<Vector2Int> path,
        Cell[,] grid,
        System.Action onDone)
    {
        StopAllLines(); // 🔥 important
        StartCoroutine(DrawRoutine(path, grid, onDone));
    }

    // =================================================
    // DRAW ROUTINE
    // =================================================
    IEnumerator DrawRoutine(
        List<Vector2Int> path,
        Cell[,] grid,
        System.Action onDone)
    {
        ClearLinesSafe();

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector2 a = GetPos(path[i], grid);
            Vector2 b = GetPos(path[i + 1], grid);

            Image seg = Instantiate(linePrefab, boardRect);
            seg.gameObject.SetActive(true);
            active.Add(seg);

            RectTransform rt = seg.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localRotation = Quaternion.identity;

            // Horizontal or Vertical only
            if (Mathf.Abs(a.x - b.x) >= Mathf.Abs(a.y - b.y))
            {
                rt.sizeDelta = new Vector2(Mathf.Abs(b.x - a.x), thickness);
                rt.anchoredPosition = new Vector2((a.x + b.x) * 0.5f, a.y);
            }
            else
            {
                rt.sizeDelta = new Vector2(thickness, Mathf.Abs(b.y - a.y));
                rt.anchoredPosition = new Vector2(a.x, (a.y + b.y) * 0.5f);
            }

            yield return new WaitForSeconds(segmentDelay);
        }

        yield return new WaitForSeconds(0.1f);
        ClearLinesSafe();
        onDone?.Invoke();
    }

    // =================================================
    // GRID → UI POSITION
    // =================================================
    Vector2 GetPos(Vector2Int p, Cell[,] grid)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        int rx = Mathf.Clamp(p.x, 0, rows - 1);
        int ry = Mathf.Clamp(p.y, 0, cols - 1);

        RectTransform cellRT =
            grid[rx, ry].GetComponent<RectTransform>();

        Vector2 screenPos =
            RectTransformUtility.WorldToScreenPoint(null, cellRT.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            boardRect, screenPos, null, out Vector2 localPos);

        float cellSize = cellRT.rect.width * 0.5f;

        if (p.y < 0) localPos.x -= cellSize;
        else if (p.y >= cols) localPos.x += cellSize;

        if (p.x < 0) localPos.y += cellSize;
        else if (p.x >= rows) localPos.y -= cellSize;

        return localPos;
    }

    // =================================================
    // CLEANUP
    // =================================================
    public void StopAllLines()
    {
        StopAllCoroutines();
        ClearLinesSafe();
    }

    void ClearLinesSafe()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            if (active[i] != null)
            {
                Destroy(active[i].gameObject);
            }
        }
        active.Clear();
    }

}
