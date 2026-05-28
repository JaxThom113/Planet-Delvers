using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapGenUtility
{
    private static readonly int GRID_SIZE = MapGen.GridSize;

    public static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            T tmp = list[i];
            list[i] = list[rand];
            list[rand] = tmp;
        }
    }

    public static bool InBounds(Vector2Int pos)
    {
        bool inBounds = (
            pos.x >= 0 && 
            pos.x < GRID_SIZE && 
            pos.y >= 0 &&
            pos.y < GRID_SIZE
        );

        return inBounds;
    }

    public static List<List<int>> InitializeGrid()
    {
        List<List<int>> grid = new List<List<int>>();
        for (int y = 0; y < GRID_SIZE; y++)
        {
            grid.Add(new List<int>());
            for (int x = 0; x < GRID_SIZE; x++)
            {
                grid[y].Add(0);
            }
        }

        return grid;
    }

    public static List<Vector2Int> GetTilesOfRegion(List<List<int>> grid, int region)
    {
        // add all tile of region to a list and return
        List<Vector2Int> tiles = new List<Vector2Int>();

        for (int x = 0; x < GRID_SIZE; x++)
        {
            for (int y = 0; y < GRID_SIZE; y++)
            {
                if (grid[y][x] == region)
                    tiles.Add(new Vector2Int(x, y));
            }
        }

        return tiles;
    }
}
