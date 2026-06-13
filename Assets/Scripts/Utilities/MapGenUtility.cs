using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapGenUtility
{
    private static int GridSize => MapGen.GridSize;

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
            pos.x < GridSize && 
            pos.y >= 0 &&
            pos.y < GridSize
        );

        return inBounds;
    }

    public static List<List<int>> InitializeGrid()
    {
        List<List<int>> grid = new List<List<int>>();
        for (int y = 0; y < GridSize; y++)
        {
            grid.Add(new List<int>());
            for (int x = 0; x < GridSize; x++)
            {
                grid[y].Add(0);
            }
        }

        return grid;
    }

    public static List<List<MapTile>> InitializeMapTileGrid()
    {
        List<List<MapTile>> grid = new List<List<MapTile>>();
        for (int y = 0; y < GridSize; y++)
        {
            grid.Add(new List<MapTile>());
            for (int x = 0; x < GridSize; x++)
            {
                grid[y].Add(new MapTile
                {
                    position = new Vector2Int(x, y),
                    region = 0,
                    visited = false,
                    doors = new bool[4] { false, false, false, false },
                    connections = new bool[4] { false, false, false, false }
                });
            }
        }

        return grid;
    }

    public static HashSet<Vector2Int> GetTiles(List<List<int>> grid, int region)
    {
        // add all tiles of a region or a structure in a return to a list and return
        HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();

        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (grid[y][x] == region)
                    tiles.Add(new Vector2Int(x, y));
            }
        }

        return tiles;
    }

    public static bool EnsureProgression(List<List<int>> grid)
    {
        bool region2TouchesRegion1 = AreTilesAdjacent(grid, 1, 2);
        bool region3TouchesRegion1Or2 = AreTilesAdjacent(grid, 3, 1) || AreTilesAdjacent(grid, 3, 2);
        bool region4TouchesRegion1Or2Or3 = AreTilesAdjacent(grid, 4, 1) || AreTilesAdjacent(grid, 4, 2) || AreTilesAdjacent(grid, 4, 3);
        
        return (
            region2TouchesRegion1 && 
            region3TouchesRegion1Or2 && 
            region4TouchesRegion1Or2Or3
        );
    }

    public static bool AreTilesAdjacent(List<List<int>> grid, int regionA, int regionB)
    {
        // check all tiles of a region against a target region
        HashSet<Vector2Int> tilesA = GetTiles(grid, regionA);

        Vector2Int[] directions =
        {
            new Vector2Int(0, -1), // up
            new Vector2Int(0, 1),  // down
            new Vector2Int(-1, 0), // left
            new Vector2Int(1, 0),  // right
        };

        foreach (Vector2Int tileA in tilesA)
        {
            // check all 4 neighbors of this tile
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = tileA + dir;

                if (InBounds(neighbor))
                {
                    // if the regions touch at a single point, return true
                    if (grid[neighbor.y][neighbor.x] == regionB)
                        return true;
                }
            }
        }

        // if no tiles touch, regions are not adjacent
        return false;
    }

    public static Vector2Int GetStartLocation()
    {
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                if (MapGen.RegionGrid[y][x] == 5)
                    return new Vector2Int(x, y);
            }
        }

        return new Vector2Int(0, 0);
    }
}
