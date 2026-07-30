using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FloodFill
{
    private static readonly Vector2Int[] directions =
    {
        new Vector2Int(0, -1), // up
        new Vector2Int(0, 1),  // down
        new Vector2Int(-1, 0), // left
        new Vector2Int(1, 0),  // right
    };

    public static List<List<int>> FloodFillRegions(List<List<int>> grid, Vector2Int[] seeds)
    {
        // frontiers for each region
        Dictionary<int, Queue<Vector2Int>> frontiers = new Dictionary<int, Queue<Vector2Int>>();
        for (int i = 1; i <= 4; i++)
        {
            frontiers[i] = new Queue<Vector2Int>();
        }

        for (int i = 0; i < seeds.Length; i++)
        {
            // set starting locations for regions, add the region number there
            int region = i + 1;

            Vector2Int pos = seeds[i];
            if (!MapGenUtility.InBounds(pos))
                continue;

            grid[pos.y][pos.x] = region;
            frontiers[region].Enqueue(pos);
        }
       
        // flood fill expansion
        bool expanded = true;
        while (expanded)
        {
            expanded = false;

            // add to each region in a random order every iteration
            List<int> regionOrder = new List<int>(){ 1, 2, 3, 4 };
            MapGenUtility.Shuffle(regionOrder);

            foreach (int region in regionOrder)
            {
                // process all frontier items for this region
                int frontierCount = frontiers[region].Count;

                for (int i = 0; i < frontierCount; i++)
                {
                    Vector2Int current = frontiers[region].Dequeue();

                    // check each direction out from the previously added region location
                    List<Vector2Int> shuffledDirs = new List<Vector2Int>(directions);
                    MapGenUtility.Shuffle(shuffledDirs);

                    foreach (Vector2Int dir in shuffledDirs)
                    {
                        Vector2Int next = current + dir;
                        if (!MapGenUtility.InBounds(next))
                            continue;

                        // already filled
                        if (grid[next.y][next.x] != 0)
                            continue;

                        // add to the region in the grid, queue this position to frontier
                        grid[next.y][next.x] = region;
                        frontiers[region].Enqueue(next);

                        expanded = true;
                    }
                }
            }
        }

        return grid;
    }
}
