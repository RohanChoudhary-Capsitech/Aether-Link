using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(GridLayoutGroup))]
public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    [Header("Board Offset")]
    [SerializeField] private Vector2 manualOffset = Vector2.zero;


    [Header("References")]
    [SerializeField] private LevelData levelData;
    [SerializeField] private RectTransform boardRect;   // 🔥 Board Image Rec
    [SerializeField] private RectTransform gridRoot;
    [SerializeField] private Cell cellPrefab;
    

    private GridLayoutGroup gridLayout;
    private Cell[,] grid;
    public int CurrentLevelData = 0;
    public Cell[,] Grid => grid;

    void Awake()
    {
        Instance = this;
        gridLayout = gridRoot.GetComponent<GridLayoutGroup>();
    }

    void Start()
    {
        CreateGrid();
    }

   
    public void LoadLevel(LevelData data)
    {
        if (data == null)
        {
            Debug.LogError("LevelData is NULL");
            return;
        }

        // Clear old grid if exists
        if (grid != null)
        {
            foreach (var cell in grid)
            {
                if (cell != null)
                    Destroy(cell.gameObject);
            }
        }

        levelData = data;
        CreateGrid();
    }


    void CreateGrid()
    {
        Vector2Int size = levelData.GetGridSize();
        int rows = size.y;
        int cols = size.x;

        AutoResizeCells(rows, cols);

        // 1. Identify Locked and Free coordinates
        List<Vector2Int> lockedCoords = new();
        List<Vector2Int> freeCoords = new();

        var locks = levelData.GetLockedCells();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // Check if this cell is playable (Normal). If Block/Empty, ignore.
                // We don't have the cell properties yet, so check LevelData.
                CellLayoutType type = levelData.GetCellType(r, c);
                if (type == CellLayoutType.Normal)
                {
                    bool isLocked = locks.Exists(x => x.row == r && x.col == c);
                    if (isLocked)
                        lockedCoords.Add(new Vector2Int(r, c));
                    else
                        freeCoords.Add(new Vector2Int(r, c));
                }
            }
        }

        int totalPlayable = lockedCoords.Count + freeCoords.Count;

        if (totalPlayable <= 0 || totalPlayable % 2 != 0)
        {
            Debug.LogError("Total playable cells must be EVEN.");
            return;
        }

        if (lockedCoords.Count % 2 != 0)
        {
            Debug.LogError($"Locked cells count ({lockedCoords.Count}) must be EVEN to ensure pairs inside locks.");
            // We could fallback or just return. Let's return to enforce fixing data.
            return;
        }

        // 2. Prepare Sprite IDs for each group
        // We need (lockedCoords.Count / 2) pairs for the locked group.
        // We need (freeCoords.Count / 2) pairs for the free group.
        
        List<int> allSpriteIds = BuildSpriteIdList(levelData.GetTilePairs());
        
        if (allSpriteIds.Count != totalPlayable)
        {
            Debug.LogError($"TilePairs mismatch. Playable={totalPlayable}, Sprites={allSpriteIds.Count}");
            return;
        }

        // Shuffle all pairs first to randomize which sprites go to locked vs free
        // BUT we must keep pairs together. BuildSpriteIdList returns [0,0, 1,1, ...]
        // Let's effectively shuffle PAIRS.
        List<int> pairIndices = new(); 
        for(int i=0; i<allSpriteIds.Count; i+=2) pairIndices.Add(allSpriteIds[i]); // Get unique pair IDs
        Shuffle(pairIndices); // Shuffle the TYPES of pairs

        List<int> lockedSprites = new();
        List<int> freeSprites = new();

        int lockedPairsNeeded = lockedCoords.Count / 2;
        int freePairsNeeded = freeCoords.Count / 2;

        // Distribute pairs
        int head = 0;
        for (int i = 0; i < lockedPairsNeeded; i++)
        {
            int pid = pairIndices[head++];
            lockedSprites.Add(pid);
            lockedSprites.Add(pid);
        }
        for (int i = 0; i < freePairsNeeded; i++)
        {
            int pid = pairIndices[head++];
            freeSprites.Add(pid);
            freeSprites.Add(pid);
        }

        // Shuffle positions within the groups
        Shuffle(lockedSprites);
        Shuffle(freeSprites);


        // 3. Spawn Grid
        grid = new Cell[rows, cols];
        
        // Clear old grid
        foreach (Transform child in gridRoot)
            Destroy(child.gameObject);

        // Map for fast lookup during spawn
        // Or just iterate coordinates?
        // Let's spawn empty grid first, then fill.

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Cell cell = Instantiate(cellPrefab, gridRoot);
                cell.row = r;
                cell.col = c;
                grid[r, c] = cell;
                
                CellLayoutType type = levelData.GetCellType(r, c);

                switch (type)
                {
                    case CellLayoutType.Empty:
                        cell.SetupEmpty();
                        break;
                    case CellLayoutType.Block:
                        cell.SetupBlock();
                        break;
                    case CellLayoutType.Normal:
                        // Will fill in next pass
                        break;
                }
            }
        }

        // Fill Locked Cells
        int lIdx = 0;
        foreach (var coord in lockedCoords)
        {
            Cell cell = grid[coord.x, coord.y];
            int tileId = lockedSprites[lIdx++];
            TilePairData pair = levelData.GetTilePairs()[tileId];
            cell.SetupNormal(tileId, pair.sprite);
        }

        // Fill Free Cells
        int fIdx = 0;
        foreach (var coord in freeCoords)
        {
            Cell cell = grid[coord.x, coord.y];
            int tileId = freeSprites[fIdx++];
            TilePairData pair = levelData.GetTilePairs()[tileId];
            cell.SetupNormal(tileId, pair.sprite);
        }

        // Apply Locking
        pairsCleared = 0;
        foreach (var lockData in levelData.GetLockedCells())
        {
            if (lockData.row >= 0 && lockData.row < rows && lockData.col >= 0 && lockData.col < cols)
            {
               Cell c = grid[lockData.row, lockData.col];
               // Check if it's a playable cell before locking? Usually yes.
               if (c.IsPlayable())
               {
                   c.SetLocked(true);
               }
            }
        }

        if (levelData.ShowAds())
        {
            Debug.Log("💰 SHOW ADS BANNER");
        }

        Debug.Log("✅ Grid created successfully (sprite-safe)");
        
        CheckForDeadlock();
    }

    private int pairsCleared = 0;

    public void OnPairCleared()
    {
        pairsCleared++;
        Debug.Log($"Pairs Cleared: {pairsCleared}");

        // Clear hints if they were highlighted? 
        // Logic inside Cell.cs PlayHighlightAnimation handles simple pulse, so no persistence cleanup needed usually.

        // Check for Unlocks
        foreach (var lockData in levelData.GetLockedCells())
        {
             if (lockData.unlockAtPairCount <= pairsCleared)
             {
                 if (lockData.row >= 0 && lockData.row < grid.GetLength(0) 
                     && lockData.col >= 0 && lockData.col < grid.GetLength(1))
                 {
                     Cell c = grid[lockData.row, lockData.col];
                     if (c.IsLocked)
                     {
                         c.SetLocked(false);
                         Debug.Log($"🔓 Unlocked Cell at [{lockData.row},{lockData.col}]");
                     }
                 }
             }
        }
        
        CheckForDeadlock();
    }


    // =================================================
    // AUTO RESIZE (🔥 MAIN FIX 🔥)
    // =================================================
    [SerializeField] private float topPadding = 200f;
    [SerializeField] private float bottomPadding = 200f;
    [SerializeField] private float sidePadding = 40f;

    void AutoResizeCells(int rows, int cols)
    {
        // 🔥 Use boardRect.parent (Canvas/Panel) to get full screen size
        RectTransform parentRect = boardRect.parent as RectTransform;
        
        if (parentRect == null)
            return;
   // Detect tablet (phones remain unchanged)
bool isTablet = Mathf.Min(Screen.width, Screen.height) >= 900;

// Tablet-only extra spacing (defaults = 0 for phones)
float tabletExtraTop = 0f;
float tabletExtraBottom = 0f;
float tabletExtraSide = 0f;

if (isTablet)
{
    tabletExtraTop = parentRect.rect.height * 0.08f;    // extra top space
    tabletExtraBottom = parentRect.rect.height * 0.08f; // extra bottom space
    tabletExtraSide = parentRect.rect.width * 0.05f;    // extra side spacing
}

// Original phone logic (UNCHANGED)
float availableWidth =
    parentRect.rect.width - sidePadding - tabletExtraSide;

float availableHeight =
    parentRect.rect.height
    - topPadding
    - bottomPadding
    - tabletExtraTop
    - tabletExtraBottom;

// Safety clamp (recommended)
availableWidth = Mathf.Max(0, availableWidth);
availableHeight = Mathf.Max(0, availableHeight);



        if (availableWidth <= 0 || availableHeight <= 0)
        {
            Debug.LogWarning("⚠️ Screen size too small for margins!");
            return;
        }

        // 2. Calculate Cell Size based on available space
        float spacingX = gridLayout.spacing.x;
        float spacingY = gridLayout.spacing.y;

        float maxCellWidth = (availableWidth - (spacingX * (cols - 1))) / cols;
        float maxCellHeight = (availableHeight - (spacingY * (rows - 1))) / rows;

        // Use smallest dimension to keep cells square
        float cellSize = Mathf.Floor(Mathf.Min(maxCellWidth, maxCellHeight));

        // 3. Apply to Grid Layout
        gridLayout.cellSize = new Vector2(cellSize, cellSize);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = cols;
        // 🔥 Force Center Alignment for cells
        gridLayout.childAlignment = TextAnchor.MiddleCenter; 

        // 4. Resize Board Container to fit snugly
        float totalBoardWidth = (cellSize * cols) + (spacingX * (cols - 1));
        float totalBoardHeight = (cellSize * rows) + (spacingY * (rows - 1));

        boardRect.sizeDelta = new Vector2(totalBoardWidth + 40, totalBoardHeight + 40); // +40 for internal padding
        gridRoot.sizeDelta = new Vector2(totalBoardWidth, totalBoardHeight);

        // 5. 🔥 CENTER THE BOARD and apply OFFSET
        // Reset anchors to center
        boardRect.anchorMin = new Vector2(0.5f, 0.5f);
        boardRect.anchorMax = new Vector2(0.5f, 0.5f);
        boardRect.pivot = new Vector2(0.5f, 0.5f);
        
        // Calculate offset if top/bottom padding is different
        // If Top is larger, we push down (negative Y). If Bottom is larger, we push up (positive Y).
        // Wait, formula: (Bottom - Top) / 2
        // Example: Top 200, Bottom 0 -> (0 - 200)/2 = -100 (Push down). Correct.
        // Example: Top 0, Bottom 200 -> (200 - 0)/2 = +100 (Push up). Correct.
        float offsetY = (bottomPadding - topPadding) / 2f;
        
        //boardRect.anchoredPosition = new Vector2(0, offsetY);
        boardRect.anchoredPosition =
    new Vector2(manualOffset.x, offsetY + manualOffset.y);


        // Debug.Log($"📱 Resized Grid: CellSize={cellSize}, Board={totalBoardWidth}x{totalBoardHeight}, Parent={availableWidth}x{availableHeight}, OffsetY={offsetY}");
    }

    // =================================================
    // HELPERS
    // =================================================
    List<int> BuildSpriteIdList(List<TilePairData> pairs)
    {
        List<int> list = new();
        for (int i = 0; i < pairs.Count; i++)
        {
            for (int p = 0; p < pairs[i].pairCount; p++)
            {
                list.Add(i);
                list.Add(i);
            }
        }
        return list;
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    public bool IsLevelComplete()
    {
        foreach (var cell in grid)
        {
            if (cell != null && cell.IsPlayable())
                return false;
        }
        return true;
    }
   
     public LevelData GetCurrentLevelData()
    {
        return levelData;
    }
    public void SetGridInteractable(bool value)
    {
        foreach (Cell cell in grid)
        {
            cell.SetInteractable(value);
        }
    }



    // =================================================
    // DEADLOCK & SHUFFLE
    // =================================================
    public void CheckForDeadlock()
    {
        if (IsLevelComplete()) return;

        if (TryGetValidMove(out _, out _))
            return;

        // No moves found -> Shuffle
        Debug.Log("⚠️ No moves remaining! Shuffling...");
        ShuffleBoard();
    }

    public void ShowHint()
    {
        if (TryGetValidMove(out Cell a, out Cell b))
        {
            a.PlayHighlightAnimation();
            b.PlayHighlightAnimation();
            Debug.Log($"💡 Hint: {a.row},{a.col} -> {b.row},{b.col}");
        }
        else
        {
            Debug.Log("🤷 No hint available (should be impossible/shuffling)");
        }
    }

    public void RemovePairs(int count)
    {
        for (int k = 0; k < count; k++)
        {
            if (TryGetValidMove(out Cell a, out Cell b))
            {
                // Visual effect? Lightning? For now just clear.
                // We need to trigger the line drawer or some effect ideally, but request says "Remove".
                // Let's clear them directly.
                
                // Add particles or effect here if needed
                
                a.Clear();
                b.Clear();
                
                // Trigger pair cleared logic (score, unlock, deadlock check)
                OnPairCleared();
                
                Debug.Log($"💥 PowerUp Removed Pair: {a.row},{a.col} & {b.row},{b.col}");
            }
            else
            {
                Debug.Log("🤷 No more pairs to remove!");
                break;
            }
        }
    }

    public bool TryGetValidMove(out Cell start, out Cell end)
    {
        start = null;
        end = null;

        // Group playable cells by sprite index
        Dictionary<int, List<Cell>> groups = new();
        
        foreach (var cell in grid)
        {
            if (cell != null && cell.IsPlayable())
            {
                if (!groups.ContainsKey(cell.spriteIndex))
                    groups[cell.spriteIndex] = new List<Cell>();
                
                groups[cell.spriteIndex].Add(cell);
            }
        }

        // Check for any valid move
        foreach (var kvp in groups)
        {
            List<Cell> cells = kvp.Value;
            for (int i = 0; i < cells.Count; i++)
            {
                for (int j = i + 1; j < cells.Count; j++)
                {
                    if (PathChecker.TryGetPath(grid, cells[i], cells[j], out _))
                    {
                        start = cells[i];
                        end = cells[j];
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void ShuffleBoard()
    {
        // Gather all playable cells and their data
        List<Cell> playables = new();
        List<int> indices = new();
        List<Sprite> sprites = new(); // We need to grab the current sprite from the cell

        foreach (var cell in grid)
        {
            if (cell != null && cell.IsPlayable())
            {
                playables.Add(cell);
                indices.Add(cell.spriteIndex);
                sprites.Add(cell.GetComponent<Image>().sprite); // Assuming Image has the sprite
            }
        }

        if (playables.Count == 0) return;

        // Shuffle the indices/sprites (coupled)
        // Actually, let's just create a struct or tuple to keep them together, 
        // OR just shuffle the list of indices and re-assign (since sprite depends on index usually, but let's be safe and swap the visual too).
        
        // Let's rely on the fact that LevelData *has* the sprites, but it's easier to just swap the data between the cells.
        
        // Fisher-Yates shuffle on the LIST OF DATA pairs
        for (int i = 0; i < playables.Count; i++)
        {
            int r = Random.Range(i, playables.Count);
            
            // Swap data in the Lists
            (indices[i], indices[r]) = (indices[r], indices[i]);
            (sprites[i], sprites[r]) = (sprites[r], sprites[i]);
        }

        // Re-assign to cells
        for (int i = 0; i < playables.Count; i++)
        {
            // We need a way to set sprite/index without "SetupNormal" fully resetting state if we want to preserve other things?
            // SetupNormal is fine, it just sets sprite/index/color.
            // But wait, SetupNormal sets IsLocked=false.
            // playables only contains IsPlayable() which means Not Locked. So safe.
            playables[i].SetupNormal(indices[i], sprites[i]);
        }

        // Re-check for deadlock (recursive, but safe enough if randomness works)
        // Add a small delay or check to avoid infinite recursion if 0 moves are mathematically impossible (e.g. 1 pair blocked by itself? unlikely with pathing).
        CheckForDeadlock(); 
    }

   


}
