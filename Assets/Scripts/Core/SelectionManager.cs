using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;

    [SerializeField] private float secondClickTime = 3f;

    private Coroutine waitCoroutine;
    private Cell firstCell;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // ==========================================
    // RESET FOR LEVEL
    // ==========================================
    public void ResetForNewLevel(LevelData data)
    {
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        if (firstCell != null)
            firstCell.SetInteractable(true);

        firstCell = null;
        
        Debug.Log("Reset selection.");
    }

    // ==========================================
    // CELL CLICK
    // ==========================================
    public void Select(Cell cell)
    {
        if (!cell.IsPlayable())
            return;

        // 🔥 START TIMER ON FIRST CLICK
        LevelManager.Instance.StartTimer();

        // ---------- FIRST CLICK ----------
        if (firstCell == null)
        {
            firstCell = cell;
            firstCell.SetInteractable(false);

            waitCoroutine = StartCoroutine(WaitForSecondClick());
            return;
        }

        // ---------- SAME CELL ----------
        if (cell == firstCell)
            return;

        // Stop timeout
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        firstCell.SetInteractable(true);

        // ---------- SPRITE MISMATCH ----------
        if (cell.spriteIndex != firstCell.spriteIndex)
        {
            //RegisterWrong();
            firstCell = null;
            return;
        }

        // Cache references
        Cell a = firstCell;
        Cell b = cell;
        firstCell = null;

        // ---------- PATH CHECK ----------
        if (PathChecker.TryGetPath(
            GridManager.Instance.Grid,
            a,
            b,
            out List<Vector2Int> path))
        {
            UILineDrawer.Instance.DrawPath(
                path,
                GridManager.Instance.Grid,
                () =>
                {
                    if (a != null) a.Clear();
                    if (b != null) b.Clear();

                    GridManager.Instance.OnPairCleared(); // 🔥 Trigger Unlock
                    CheckGameWin();
                }
            );
        }
        // else
        // {
        //     RegisterWrong();
        // }
    }

    // ==========================================
    // WRONG TRY
    // ==========================================


    // ==========================================
    // GAME WIN CHECK
    // ==========================================
    void CheckGameWin()
    {
        Cell[,] grid = GridManager.Instance.Grid;

        for (int r = 0; r < grid.GetLength(0); r++)
        {
            for (int c = 0; c < grid.GetLength(1); c++)
            {
                if (grid[r, c].IsPlayable())
                    return;
            }
        }

        StartCoroutine(WinDelay());
    }

    IEnumerator WinDelay()
    {
        yield return new WaitForSeconds(0.5f);
        LevelManager.Instance.OnLevelWin();
    }

    // ==========================================
    // SECOND CLICK TIMEOUT
    // ==========================================
    IEnumerator WaitForSecondClick()
    {
        yield return new WaitForSeconds(secondClickTime);

        if (firstCell != null)
        {
            firstCell.SetInteractable(true);
            firstCell = null;
        }
    }
    




    bool IsInsideGrid(Cell[,] grid, int r, int c)
    {
        return r >= 0 &&
               c >= 0 &&
               r < grid.GetLength(0) &&
               c < grid.GetLength(1);
    }

#region Not Required Methods
      
#endregion
}
