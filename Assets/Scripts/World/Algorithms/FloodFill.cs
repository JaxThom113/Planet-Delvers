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

    public static List<List<int>> FloodFillRegions()
    {
        List<List<int>> grid = new List<List<int>>();

        // set start room location
        Vector2Int start = new Vector2Int(
            Random.Range(0, MapGen.GridSize + 1), 
            Random.Range(0, MapGen.GridSize + 1)
        );     
        grid[start.y][start.x] = 5;

        // frontiers for each region
        Dictionary<int, Queue<Vector2Int>> frontiers = new Dictionary<int, Queue<Vector2Int>>();
        for (int i = 1; i <= 4; i++)
        {
            frontiers[i] = new Queue<Vector2Int>();
        }

        // initial seeds where regions will "grow" from
        Vector2Int[] seeds =
        {
            new Vector2Int(start.x, start.y - 1), // region 4 (up)
            new Vector2Int(start.x, start.y + 1), // region 2 (down)
            new Vector2Int(start.x - 1, start.y), // region 1 (left)
            new Vector2Int(start.x + 1, start.y), // region 3 (right)
        };

        for (int i = 1; i <= seeds.Length; i++)
        {
            // set starting locations for regions, add the region number there
            Vector2Int pos = seeds[i];
            if (!InBounds(pos))
                continue;

            grid[pos.y][pos.x] = i;
            frontiers[i].Enqueue(pos);
        }
       
        // flood fill expansion
        bool expanded = true;
        while (expanded)
        {
            expanded = false;

            // add to each region in a random order every iteration
            List<int> regionOrder = new List<int>(){ 1, 2, 3, 4 };
            Shuffle(regionOrder);

            foreach (int region in regionOrder)
            {
                if (frontiers[region].Count == 0)
                    continue;

                Vector2Int current = frontiers[region].Dequeue();

                // check each direction out from the previously added region location
                List<Vector2Int> shuffledDirs = new List<Vector2Int>(directions);
                Shuffle(shuffledDirs);

                foreach (Vector2Int dir in shuffledDirs)
                {
                    Vector2Int next = current + dir;
                    if (!InBounds(next))
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

        return grid;
    }

    private static bool InBounds(Vector2Int pos)
    {
        bool inBounds = (
            pos.x >= 0 && 
            pos.x < MapGen.GridSize && 
            pos.y >= 0 &&
            pos.y < MapGen.GridSize
        );

        return inBounds;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
