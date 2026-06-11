using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomWalk
{
    private static readonly Vector2Int[] directions =
    {
        new Vector2Int(0, -1), // up
        new Vector2Int(0, 1),  // down
        new Vector2Int(-1, 0), // left
        new Vector2Int(1, 0),  // right
    };

    public static List<List<int>> RandomWalkGenerate(List<List<int>> grid, int region, Vector2Int seed)
    {
        HashSet<Vector2Int> regionTiles = MapGenUtility.GetTiles(MapGen.RegionGrid, region);
        Vector2Int current = seed;
        List<Vector2Int> dirs = new List<Vector2Int>(directions);

        // random walk could fill a 4th of the region at the low end, full region at high end
        int pathLength = UnityEngine.Random.Range(regionTiles.Count / 4, regionTiles.Count);

        for (int i = 0; i < pathLength; i++)
        {
            MapGenUtility.Shuffle(dirs);

            Vector2Int next = current;            

            foreach (Vector2Int dir in dirs)
            {
                // try all 4 directions, take the first one that works
                next = current + dir;

                if (MapGenUtility.InBounds(next) && regionTiles.Contains(next))
                {
                    // Can revisit tiles, so accept any valid tile (filled or empty)
                    // structureTilemap.SetTile(new Vector3Int(next.x, next.y, 0), structureTiles[region]);
                    // break;

                    // Cannot revisit tiles, only accept empty tiles
                    if (grid[next.y][next.x] == 0) // unvisited
                    {
                        grid[next.y][next.x] = region;
                        break;
                    }
                }
            }

            current = next;
        }

        return grid;
    }
}
