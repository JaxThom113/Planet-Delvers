using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

This script generates a map grid to define a world map. Each number corresponds
to a region type. There are 3 layers to generation: MapGrid, RegionGrind, and 
StructureGrid.

Region types:
0 = empty
1 = first region
2 = second region
3 = third region
4 = fourth region
5 = starting room

Example Region Grid:

4 4 4 4 4 4 4 4 3 3 3 3 3 3 3 3
4 4 4 4 4 4 4 4 3 3 3 3 3 3 3 3
4 4 4 4 4 4 4 4 3 3 3 3 3 3 3 3
4 4 4 4 4 4 4 4 3 3 3 3 3 3 3 3
4 4 4 4 4 4 4 3 3 3 3 3 3 3 3 3
4 4 4 4 4 4 4 3 3 3 3 3 3 3 3 3
4 4 4 4 4 4 3 3 3 3 3 3 3 3 3 3
1 1 1 1 1 5 3 3 3 3 3 3 3 3 3 3
1 1 1 1 1 2 2 2 3 3 3 3 3 3 3 3
1 1 1 1 1 2 2 2 2 2 2 3 3 3 3 3
1 1 1 1 1 1 2 2 2 2 2 2 2 2 2 2
1 1 1 1 1 1 1 2 2 2 2 2 2 2 2 2
1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2
1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2
1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2
1 1 1 1 1 1 1 1 2 2 2 2 2 2 2 2

Example Structure Grid:

0 4 4 4 0 0 0 0 0 0 0 0 3 0 0 0
0 0 0 4 0 0 0 0 0 3 3 3 3 3 3 3
0 0 0 4 4 4 4 0 0 3 0 0 0 0 0 3
0 0 0 4 4 0 0 0 0 3 3 3 3 0 0 3
0 0 0 0 4 4 0 0 0 0 0 0 3 3 3 3
0 0 0 0 4 4 0 0 3 3 3 3 3 0 0 3
0 0 0 0 0 4 0 0 3 0 0 0 3 0 0 3
1 1 1 1 1 5 3 3 3 3 3 3 3 3 3 3
1 0 0 0 0 2 2 2 0 0 0 0 0 3 0 0
1 0 0 0 0 0 0 2 2 2 0 0 0 3 3 3
1 1 1 1 0 0 0 2 2 2 0 0 0 0 0 0
0 0 0 1 0 0 0 0 0 2 0 0 0 0 0 0
0 1 1 1 1 1 0 0 0 2 2 2 2 2 2 0
0 1 0 1 0 1 0 0 0 2 2 2 0 0 0 0
0 1 0 0 0 1 1 1 2 2 0 0 0 0 0 0
1 1 1 0 0 0 0 0 0 0 0 0 0 0 0 0

*/

public struct MapTile
{
    public int regionType;
    public bool visited;
    public bool[] doors; // {N, S, E, W}
    public bool[] connections; // {N, S, E, W}
}

public static class MapGen
{
    // dimensions of map grid, square
    public static int GridSize { get; private set; }

    // used to divide map into different regions to set bounds for generation
    public static List<List<int>> RegionGrid { get; private set; }

    // used for room generation
    public static List<List<int>> StructureGrid { get; private set; }

    // main map grid
    public static List<List<MapTile>> MapGrid { get; private set; }

    public static void GenerateMap()
    {
        GridSize = 16;

        // Step #1: Establish regions and set start point
        CreateRegions();

        // Step #2: Generate structures for each region within their bounds
        CreateStructures();

        // Step #3: Create MapGrid
        CreateMap();

        // Step #4: Connect cells to create rooms
        AddConnections();

        // Step #5: Add doors to connect rooms
        AddDoors();

        // Step #6: Save map data to cache
        CacheMapData();
    }

    private static void CreateRegions()
    {
        // use flood fill to create 4 regions
        RegionGrid = FloodFill.FloodFillRegions();
    }

    private static void CreateStructures()
    {
        List<Vector2Int> regionTiles;

        // use a randomly picked algorithm to generate unique structures in each region
        for (int region = 1; region <= 4; region++)
        {
            regionTiles = MapGenUtility.GetTilesOfRegion(RegionGrid, region);

            if (Random.Range(0, 2) == 0)
                StructureGrid = Dfs.DfsGenerate(RegionGrid, regionTiles, region);
            else
                StructureGrid = RandomWalk.RandomWalkGenerate(RegionGrid, regionTiles, region);
        }
    }

    private static void CreateMap()
    {
        // set up empty map grid
        MapGrid = new List<List<MapTile>>();
        for (int y = 0; y < GridSize; y++)
        {
            MapGrid.Add(new List<MapTile>());
            for (int x = 0; x < GridSize; x++)
            {
                MapGrid[y].Add(new MapTile
                {
                    regionType = 0,
                    visited = false,
                    doors = new bool[4] { false, false, false, false },
                    connections = new bool[4] { false, false, false, false }
                });
            }
        }
    }

    private static void AddConnections()
    {
        return;
    }

    private static void AddDoors()
    {
        return;
    }

    private static void CacheMapData()
    {
        CsvUtility.SaveGridToCSV(RegionGrid, "region_grid.csv");
    }
}
