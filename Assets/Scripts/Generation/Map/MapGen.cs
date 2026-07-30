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

    // all of the distinct rooms in MapGrid
    public static List<List<MapTile>> MapRooms { get; private set; }

    public static void GenerateMap(int gridSize)
    {
        GridSize = gridSize;

        // Step #1: Establish regions and set start point
        CreateRegions();

        // Step #2: Generate structures for each region within their bounds
        CreateStructures();

        // Step #3: Connect tiles to make rooms and connect rooms with doors
        CreateRooms();

        // Step #4: Save map data to cache
        //CacheMapData();
    }

    private static void CreateRegions()
    {
        RegionGrid = RegionGen.CreateRegions();
    }

    private static void CreateStructures()
    {
        StructureGrid = StructureGen.CreateStructures();
    }

    private static void CreateRooms()
    {
        MapGrid = RoomGen.CreateRooms();
        MapRooms = RoomGen.GetRooms();
    }

    private static void CacheMapData()
    {
        CsvUtility.SaveGridToCache(RegionGrid, "region_grid.csv");
        CsvUtility.SaveGridToCache(StructureGrid, "structure_grid.csv");
        JsonUtility.SaveGridToJson(MapGrid, "map_grid.json");
    }
}
