using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FloodFill
{
    private static readonly int GRID_SIZE = MapGen.GridSize;
    private static int attemptNum;

    private struct Quadrant
    {
        public int xMin, xMax, yMin, yMax;

        public Quadrant(int xMin, int xMax, int yMin, int yMax)
        {
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
        }

        // get a random point within a quadrant
        public Vector2Int GetRandomPoint()
        {
            return new Vector2Int(
                Random.Range(xMin, xMax),
                Random.Range(yMin, yMax)
            );
        }
    }

    private static readonly Vector2Int[] directions =
    {
        new Vector2Int(0, -1), // up
        new Vector2Int(0, 1),  // down
        new Vector2Int(-1, 0), // left
        new Vector2Int(1, 0),  // right
    };

    public static List<List<int>> FloodFillRegions()
    {
        attemptNum++;
        List<List<int>> grid = InitializeGrid();

        // frontiers for each region
        Dictionary<int, Queue<Vector2Int>> frontiers = new Dictionary<int, Queue<Vector2Int>>();
        for (int i = 1; i <= 4; i++)
        {
            frontiers[i] = new Queue<Vector2Int>();
        }

        // initial seeds where regions will "grow" from, make sure to be in different quadrants of the grid
        Vector2Int[] seeds = GenerateSeeds();

        for (int i = 0; i < seeds.Length; i++)
        {
            // set starting locations for regions, add the region number there
            int region = i + 1;

            Vector2Int pos = seeds[i];
            if (!InBounds(pos))
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
        }

        // set start room location somewhere in region 1
        List<Vector2Int> region1Tiles = GetTilesOfRegion(grid, 1);
        if (region1Tiles.Count > 0)
        {
            Vector2Int startingRoom = region1Tiles[Random.Range(0, region1Tiles.Count)];
            grid[startingRoom.y][startingRoom.x] = 5;
        }

        // if regions aren't adjacent in the correct way, recurse and try again
        if (!EnsureProgression(grid))
        {
            Debug.Log("Failed Attempt #" + attemptNum + " - Regions do not touch");
            return FloodFillRegions();
        }

        return grid;
    }

    private static bool InBounds(Vector2Int pos)
    {
        bool inBounds = (
            pos.x >= 0 && 
            pos.x < GRID_SIZE && 
            pos.y >= 0 &&
            pos.y < GRID_SIZE
        );

        return inBounds;
    }

    private static List<List<int>> InitializeGrid()
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

    private static Vector2Int[] GenerateSeeds()
    {
        // define all 4 quadrants
        List<Quadrant> quadrants = new List<Quadrant>
        {
            new Quadrant(GRID_SIZE/2, GRID_SIZE, GRID_SIZE/2, GRID_SIZE), // Q1 (top right)
            new Quadrant(0, GRID_SIZE/2, GRID_SIZE/2, GRID_SIZE),         // Q2 (top left)
            new Quadrant(0, GRID_SIZE/2, 0, GRID_SIZE/2),                 // Q3 (bottom left)
            new Quadrant(GRID_SIZE/2, GRID_SIZE, 0, GRID_SIZE/2),         // Q4 (bottom right)
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

    private static List<Vector2Int> GetTilesOfRegion(List<List<int>> grid, int region)
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

    private static bool EnsureProgression(List<List<int>> grid)
    {
        bool region1TouchesRegion2 = AreRegionsAdjacent(grid, 1, 2);
        bool region3TouchesRegion1Or2 = AreRegionsAdjacent(grid, 3, 1) || AreRegionsAdjacent(grid, 3, 2);
        
        return region1TouchesRegion2 && region3TouchesRegion1Or2;
    }

    private static bool AreRegionsAdjacent(List<List<int>> grid, int regionA, int regionB)
    {
        // check all tiles of a region against a target region
        List<Vector2Int> tilesA = GetTilesOfRegion(grid, regionA);

        foreach (Vector2Int tileA in tilesA)
        {
            // check all 4 neighbors of this tile
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = tileA + dir;

                if (!InBounds(neighbor))
                    continue;

                // if the regions touch at a single point,, return true
                if (grid[neighbor.x][neighbor.y] == regionB)
                    return true;
            }
        }

        // if no tiles touch, regions are not adjacent
        return false;
    }
}
