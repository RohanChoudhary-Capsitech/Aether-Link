using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    private const int CELL_SIZE = 28;

    public override void OnInspectorGUI()
    {
        LevelData data = (LevelData)target;

        DrawDefaultInspector();
        GUILayout.Space(10);

        DrawGridEditor(data);
        GUILayout.Space(10);

        DrawValidation(data);

        if (GUI.changed)
            EditorUtility.SetDirty(data);
    }

    // =================================================
    // GRID EDITOR
    // =================================================
    void DrawGridEditor(LevelData data)
    {
        Vector2Int size = data.GetGridSize();

        GUILayout.Label("Grid Layout (Click to Change)", EditorStyles.boldLabel);

        for (int r = 0; r < size.y; r++)
        {
            GUILayout.BeginHorizontal();

            for (int c = 0; c < size.x; c++)
            {
                CellLayoutType type = data.GetCellType(r, c);
                bool isLocked = IsLocked(data, r, c);

                Color old = GUI.backgroundColor;
                
                // Color logic: Locked gets a tint or specific color if normal
                if (isLocked) 
                    GUI.backgroundColor = new Color(1f, 0.5f, 0.5f); // Reddish for locked
                else
                    GUI.backgroundColor = GetColor(type);

                string label = GetLabel(type);
                if (isLocked) label = string.IsNullOrEmpty(label) ? "🔒" : label + "🔒";

                if (GUILayout.Button(
                    label,
                    GUILayout.Width(CELL_SIZE),
                    GUILayout.Height(CELL_SIZE)))
                {
                    // Check for Shift Click -> Toggle Lock
                    if (Event.current.shift)
                    {
                        ToggleLock(data, r, c);
                    }
                    else
                    {
                        // Normal cycle
                        data.SetCellType(r, c, Next(type));
                    }
                }

                GUI.backgroundColor = old;
            }

            GUILayout.EndHorizontal();
        }

        EditorGUILayout.HelpBox(
            "Click = Cycle Type (White/Gray/Black)\n" +
            "Shift + Click = Toggle Lock (🔒)\n",
            MessageType.Info
        );
    }

    bool IsLocked(LevelData data, int r, int c)
    {
        var list = data.GetLockedCells();
        if (list == null) return false;
        return list.Exists(x => x.row == r && x.col == c);
    }

    void ToggleLock(LevelData data, int r, int c)
    {
        var list = data.GetLockedCells();
        if (list == null) return;

        var existing = list.Find(x => x.row == r && x.col == c);
        if (existing != null)
        {
            list.Remove(existing);
        }
        else
        {
            list.Add(new LockedCellData { row = r, col = c, unlockAtPairCount = 2 }); // Default 2
        }
        EditorUtility.SetDirty(data);
    }

    // =================================================
    // VALIDATION PANEL (NEW)
    // =================================================


    void DrawValidation(LevelData data)
    {
        if (data == null)
            return;

        GUILayout.Space(10);
        GUILayout.Label("Level Validation", EditorStyles.boldLabel);

        Vector2Int size = data.GetGridSize();
        int totalCells = size.x * size.y;
        int playable = data.GetPlayableCellCount();

        int pairCells = 0;
        var pairs = data.GetTilePairs();

        if (pairs != null)
        {
            foreach (var p in pairs)
            {
                if (p != null)
                    pairCells += p.pairCount * 2;
            }
        }

        bool valid = true;

        // ================= RULES =================

        if (playable <= 0)
        {
            EditorGUILayout.HelpBox(
                "❌ No playable cells in the grid.",
                MessageType.Error);
            valid = false;
        }

        int lockedCount = data.GetLockedCells() != null ? data.GetLockedCells().Count : 0;
        if (lockedCount % 2 != 0)
        {
            EditorGUILayout.HelpBox(
                $"❌ Locked cells count ({lockedCount}) must be EVEN to ensure pairs.",
                MessageType.Error);
            valid = false;
        }

        if (playable % 2 != 0)
        {
            EditorGUILayout.HelpBox(
                "❌ Playable cells must be EVEN (pairs required).",
                MessageType.Error);
            valid = false;
        }

        if (pairCells != playable)
        {
            EditorGUILayout.HelpBox(
                $"❌ TilePairs mismatch.\n" +
                $"Pairs = {pairCells}, Playable = {playable}",
                MessageType.Error);
            valid = false;
        }

        // ================= RESULT =================

        if (valid)
        {
            EditorGUILayout.HelpBox(
                "✅ LEVEL VALID – Ready to Play",
                MessageType.Info);
        }

        // ================= INFO =================

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField(
            $"Grid Size: {size.x} × {size.y}\n" +
            $"Total Cells: {totalCells}\n" +
            $"Playable Cells: {playable}\n" +
            $"Pair Cells (from TilePairs): {pairCells}\n",
            EditorStyles.helpBox
        );
    }


    // =================================================
    // HELPERS
    // =================================================
    CellLayoutType Next(CellLayoutType t)
    {
        return t switch
        {
            CellLayoutType.Normal => CellLayoutType.Block,
            CellLayoutType.Block => CellLayoutType.Empty,
            _ => CellLayoutType.Normal
        };
    }

    Color GetColor(CellLayoutType t)
    {
        return t switch
        {
            CellLayoutType.Block => Color.gray,
            CellLayoutType.Empty => Color.black,
            _ => Color.white
        };
    }

    string GetLabel(CellLayoutType t)
    {
        return t switch
        {
            CellLayoutType.Block => "B",
            CellLayoutType.Empty => "E",
            _ => ""
        };
    }
}
