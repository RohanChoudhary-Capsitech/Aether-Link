using UnityEngine;
using System.Collections.Generic;

public static class PathChecker
{
    public static bool TryGetPath(
        Cell[,] grid,
        Cell a,
        Cell b,
        out List<Vector2Int> path)
    {
        path = new();

        // must be same sprite
        if (a == b || a.spriteIndex != b.spriteIndex)
            return false;

        // I
        if (Straight(grid, a, b))
        {
            path.Add(Pos(a));
            path.Add(Pos(b));
            return true;
        }

        // L
        if (LShape(grid, a, b, out path))
            return true;

        // U (shortest outside)
        if (UShape(grid, a, b, out path))
            return true;

        return false;
    }

    // --------------------------------------------------
    // I SHAPE
    static bool Straight(Cell[,] g, Cell a, Cell b)
    {
        if (a.row == b.row)
            return ClearRow(g, a.row, a.col, b.col);

        if (a.col == b.col)
            return ClearCol(g, a.col, a.row, b.row);

        return false;
    }

    // --------------------------------------------------
    // L SHAPE
    static bool LShape(Cell[,] g, Cell a, Cell b, out List<Vector2Int> path)
    {
        path = new();

        Vector2Int c1 = new(a.row, b.col);
        if (Empty(g, c1) &&
            ClearRow(g, a.row, a.col, c1.y) &&
            ClearCol(g, c1.y, c1.x, b.row))
        {
            path.Add(Pos(a));
            path.Add(c1);
            path.Add(Pos(b));
            return true;
        }

        Vector2Int c2 = new(b.row, a.col);
        if (Empty(g, c2) &&
            ClearCol(g, a.col, a.row, c2.x) &&
            ClearRow(g, c2.x, c2.y, b.col))
        {
            path.Add(Pos(a));
            path.Add(c2);
            path.Add(Pos(b));
            return true;
        }

        return false;
    }

    // --------------------------------------------------
    // U SHAPE (EXTEND FROM A, THEN L)
    static bool UShape(Cell[,] g, Cell a, Cell b, out List<Vector2Int> path)
    {
        path = new();

        int rows = g.GetLength(0);
        int cols = g.GetLength(1);

        foreach (Vector2Int dir in Dirs)
        {
            int r = a.row;
            int c = a.col;

            while (true)
            {
                r += dir.x;
                c += dir.y;

                // outside reached → virtual empty
                if (!Inside(rows, cols, r, c))
                {
                    Vector2Int v = new(r, c);
                    if (LFromVirtual(g, a, b, v, out path))
                        return true;
                    break;
                }

                // blocked
                if (!g[r, c].IsEmpty())
                    break;

                // try L from empty extension
                Vector2Int mid = new(r, c);
                if (LFromVirtual(g, a, b, mid, out path))
                    return true;
            }
        }

        return false;
    }

    // --------------------------------------------------
    // L FROM VIRTUAL / EXTENSION POINT
    static bool LFromVirtual(
        Cell[,] g,
        Cell a,
        Cell b,
        Vector2Int v,
        out List<Vector2Int> path)
    {
        path = new();

        Vector2Int c1 = new(v.x, b.col);
        if (Empty(g, c1) &&
            ClearRowVirtual(g, v.x, v.y, c1.y) &&
            ClearCol(g, c1.y, c1.x, b.row))
        {
            path.Add(Pos(a));
            path.Add(v);
            path.Add(c1);
            path.Add(Pos(b));
            return true;
        }

        Vector2Int c2 = new(b.row, v.y);
        if (Empty(g, c2) &&
            ClearColVirtual(g, v.y, v.x, c2.x) &&
            ClearRow(g, c2.x, c2.y, b.col))
        {
            path.Add(Pos(a));
            path.Add(v);
            path.Add(c2);
            path.Add(Pos(b));
            return true;
        }

        return false;
    }

    // --------------------------------------------------
    // CLEAR CHECKS

    // --------------------------------------------------
    // CLEAR CHECKS

    static bool ClearRow(Cell[,] g, int row, int c1, int c2)
    {
        int min = Mathf.Min(c1, c2) + 1;
        int max = Mathf.Max(c1, c2);

        for (int c = min; c < max; c++)
            if (!g[row, c].IsEmpty())
                return false;

        return true;
    }

    static bool ClearCol(Cell[,] g, int col, int r1, int r2)
    {
        int min = Mathf.Min(r1, r2) + 1;
        int max = Mathf.Max(r1, r2);

        for (int r = min; r < max; r++)
            if (!g[r, col].IsEmpty())
                return false;

        return true;
    }

    static bool ClearRowVirtual(Cell[,] g, int row, int c1, int c2)
    {
        if (row < 0 || row >= g.GetLength(0))
            return true;

        return ClearRow(g, row, c1, c2);
    }

    static bool ClearColVirtual(Cell[,] g, int col, int r1, int r2)
    {
        if (col < 0 || col >= g.GetLength(1))
            return true;

        return ClearCol(g, col, r1, r2);
    }

    // --------------------------------------------------
    // HELPERS

    static bool Empty(Cell[,] g, Vector2Int p)
    {
        if (!Inside(g.GetLength(0), g.GetLength(1), p.x, p.y))
            return true;

        return g[p.x, p.y].IsEmpty();
    }

    static bool Inside(int r, int c, int x, int y)
        => x >= 0 && x < r && y >= 0 && y < c;

    static Vector2Int Pos(Cell c) => new(c.row, c.col);

    static readonly Vector2Int[] Dirs =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };
}
