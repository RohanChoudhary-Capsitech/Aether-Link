using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    // ================= GRID =================
    [Header("Grid Settings")]
    [SerializeField] private GridSize gridSize;

    // ================= RULES =================
    [Header("Level Rules")]
    [SerializeField] private bool hasTimer;
    [SerializeField] private float timeLimit = 60f;
    [SerializeField] private bool showAds;

    // ================= LOCKING =================
    [Header("Locked Cells")]
    [SerializeField] private List<LockedCellData> lockedCells = new();
    
    // ================= LAYOUT =================
    [Header("Manual Cell Layout")]
    [SerializeField] private List<CellLayoutData> specialCells = new();

    // ================= TILES =================
    [Header("Tile Pairs (Sprite + Count)")]
    [SerializeField] private List<TilePairData> tilePairs;
  

    // =================================================
    // GRID SIZE
    // =================================================
    public Vector2Int GetGridSize()
    {
        return gridSize switch
        {
            GridSize.Small => new Vector2Int(4, 4),
            GridSize.Medium => new Vector2Int(6, 6),
            GridSize.Large => new Vector2Int(8, 8),
            GridSize.ExtraLarge => new Vector2Int(10, 10),
            GridSize.ExtraLargeExtra => new Vector2Int(20, 20),
            _ => Vector2Int.zero
        };
    }

    // =================================================
    // RULES
    // =================================================
    
    // =================================================
    // CELL TYPE
    // =================================================
    public CellLayoutType GetCellType(int row, int col)
    {
        foreach (var c in specialCells)
        {
            if (c.row == row && c.col == col)
                return c.type;
        }
        return CellLayoutType.Normal;
    }

    public void SetCellType(int row, int col, CellLayoutType type)
    {
        specialCells.RemoveAll(c => c.row == row && c.col == col);

        if (type != CellLayoutType.Normal)
        {
            specialCells.Add(new CellLayoutData
            {
                row = row,
                col = col,
                type = type
            });
        }
    }

    // =================================================
    // TILE PAIRS
    // =================================================
    public List<TilePairData> GetTilePairs() => tilePairs;

    // =================================================
    // VALIDATION
    // =================================================
    public int GetPlayableCellCount()
    {
        Vector2Int size = GetGridSize();
        int total = size.x * size.y;

        foreach (var c in specialCells)
        {
            if (c.type != CellLayoutType.Normal)
                total--;
        }

        return total;
    }

    // =================================================
    // NEW MECHANICS GETTERS
    // =================================================
    public bool HasTimer() => hasTimer;
    public float GetTimeLimit() => timeLimit;
    public bool ShowAds() => showAds;
    public List<LockedCellData> GetLockedCells() => lockedCells;
}

[System.Serializable]
public class LockedCellData
{
    public int row;
    public int col;
    public int unlockAtPairCount; // Unlock when this many pairs have been cleared globally
}
