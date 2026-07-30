using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Dfs
{
    private static readonly Vector2Int[] directions =
    {
        new Vector2Int(0, -2), // up
        new Vector2Int(0, 2),  // down
        new Vector2Int(-2, 0), // left
        new Vector2Int(2, 0),  // right
    };

    public static List<List<int>> DfsGenerate(List<List<int>> grid, int region, Vector2Int seed)
    {
        HashSet<Vector2Int> regionTiles = MapGenUtility.GetTiles(MapGen.RegionGrid, region);
        Vector2Int current = seed;

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
                    grid[seed.y + dir.y / 2][seed.x + dir.x / 2] = region;
                    grid[next.y][next.x] = region;

                    DfsGenerate(grid, region, next);
                }
            }
        }

        return grid;
    }
}
