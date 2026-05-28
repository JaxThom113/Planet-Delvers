using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Dfs
{
    private static readonly int GRID_SIZE = MapGen.GridSize;

    private static readonly Vector2Int[] directions =
    {
        new Vector2Int(0, -2), // up
        new Vector2Int(0, 2),  // down
        new Vector2Int(-2, 0), // left
        new Vector2Int(2, 0),  // right
    };

    public static List<List<int>> DfsGenerate(List<List<int>> grid, List<Vector2Int> regionTiles, int region, int y = 0, int x = 0)
    {
        Vector2Int current = new Vector2Int(x, y);

        // randomize directions
        List<Vector2Int> dirs = new List<Vector2Int>(directions);
        MapGenUtility.Shuffle(dirs);

        foreach (Vector2Int dir in dirs)
        {
            Vector2Int next = current + dir;

            // check bounds
            if (MapGenUtility.InBounds(next) && regionTiles.Contains(next))
            {
                if (grid[next.y][next.x] == 0) // unvisited
                {
                    // carve path between current and neighbor
                    grid[y + dir.y / 2][x + dir.x / 2] = region;
                    grid[next.y][next.x] = region;

                    DfsGenerate(grid, regionTiles, next.y, next.x);
                }
            }
        }

        return grid;
    }
}
