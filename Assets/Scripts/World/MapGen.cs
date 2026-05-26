using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

This script generates a 16x16 grid to define a world map. Each number corresponds
to a region type, 

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
    /*
        Region types:
        0 = empty
        1 = first region
        2 = second region
        3 = third region
        4 = fourth region
        5 = starting room
    */

    // main map grid
    public static List<List<MapTile>> MapGrid { get; private set; }
    public static int GridSize { get; private set; }
    
    // used for room generation
    public static List<List<int>> structureGrid;

    // used to divide map into different regions to set bounds for generation
    public static List<List<int>> regionGrid;

    public static void GenerateMap()
    {
        // Step #1: Create empty world grid
        CreateWorld();

        // Step #2: Set start point and establish regions
        CreateRegions();

        // Step #3: Generate structures for each region within their bounds
        GenerateStructures();

        // Step #4: Connect cells to create rooms
        AddConnections();

        // Step #5: Add doors to connect rooms
        AddDoors();
    }

    private static void CreateWorld()
    {
        GridSize = 16;

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

    private static void CreateRegions()
    {
        structureGrid = FloodFill.FloodFillRegions();
    }

    private static void GenerateStructures()
    {
        return;
    }

    private static void AddConnections()
    {
        return;
    }

    private static void AddDoors()
    {
        return;
    }
}
