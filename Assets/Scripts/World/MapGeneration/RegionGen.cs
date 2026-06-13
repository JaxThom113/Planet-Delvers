using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RegionGen
{
    private static List<List<int>> regionGrid;
    private static int attempt;
    
    private static Vector2Int startRoomLocation;

    /*
        Main functions
    */

    public static List<List<int>> CreateRegions()
    {
        regionGrid = MapGenUtility.InitializeGrid();
        attempt = 1;

        // initial seeds where regions will "grow" from, make sure to be in different quadrants of the grid
        Vector2Int[] seeds = GenerateSeeds();

        // use flood fill to create 4 regions
        regionGrid = FloodFill.FloodFillRegions(regionGrid, seeds);

        // if regions aren't adjacent in the correct way, recurse and try again
        if (!MapGenUtility.EnsureProgression(regionGrid))
        {
            Debug.Log("Failed Attempt #" + attempt + " - Regions do not touch");
            return CreateRegions();
        }

        // set start room location somewhere in region 1
        HashSet<Vector2Int> region1Tiles = MapGenUtility.GetTiles(regionGrid, 1);
        startRoomLocation = region1Tiles.ElementAt(Random.Range(0, region1Tiles.Count));
        regionGrid[startRoomLocation.y][startRoomLocation.x] = 5;

        return regionGrid;
    }

    public static Vector2Int GetStartPoint()
    {
        return startRoomLocation;
    }

    /*
        Helper functions
    */

    private static Vector2Int[] GenerateSeeds()
    {
        int gridSize = MapGen.GridSize;

        // define all 4 quadrants
        List<Quadrant> quadrants = new List<Quadrant>
        {
            new Quadrant(gridSize/2, gridSize, gridSize/2, gridSize), // Q1 (top right)
            new Quadrant(0, gridSize/2, gridSize/2, gridSize),         // Q2 (top left)
            new Quadrant(0, gridSize/2, 0, gridSize/2),                // Q3 (bottom left)
            new Quadrant(gridSize/2, gridSize, 0, gridSize/2),         // Q4 (bottom right)
        };

        // starting quadrant for seed 1
        int startQuadrant = Random.Range(0, 4);

        // adjacent quadrants for seeds 2/3
        int adjacent1 = (startQuadrant + 1) % 4;
        int adjacent2 = (startQuadrant + 3) % 4;

        // opposite quadrant for seed 4
        int opposite = (startQuadrant + 2) % 4;

        Vector2Int[] seeds = new Vector2Int[4];

        seeds[0] = quadrants[startQuadrant].GetRandomPoint();

        // randomize which adjacent quadrant gets seed 2 or 3
        if (Random.Range(0, 2) == 0)
        {
            seeds[1] = quadrants[adjacent1].GetRandomPoint();
            seeds[2] = quadrants[adjacent2].GetRandomPoint();
        }
        else
        {
            seeds[1] = quadrants[adjacent2].GetRandomPoint();
            seeds[2] = quadrants[adjacent1].GetRandomPoint();
        }

        seeds[3] = quadrants[opposite].GetRandomPoint();

        return seeds;
    }
}
