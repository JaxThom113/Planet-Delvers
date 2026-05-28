using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Demo_RegionMap : MonoBehaviour
{
    [Header("Settings")]
    [Range(0, 5)]
    [SerializeField] private float delay;
    [SerializeField] private bool seededRun;
    [SerializeField] private int seed;

    [Header("References")]
    [SerializeField] private Button runButton;
    [SerializeField] private Tilemap mapTilemap;
    [SerializeField] private Tile[] regionTiles;

    // grid size must be defined here because MapGen isn't run in the demo scene
    private const int GRID_SIZE = 16;
    private int attemptNum;

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
                UnityEngine.Random.Range(xMin, xMax),
                UnityEngine.Random.Range(yMin, yMax)
            );
        }
    }

    private readonly Vector2Int[] directions =
    {
        new Vector2Int(0, -1), // up
        new Vector2Int(0, 1),  // down
        new Vector2Int(-1, 0), // left
        new Vector2Int(1, 0),  // right
    };

    public void OnRunClicked()
    {
        runButton.interactable = false;
        mapTilemap.ClearAllTiles();

        attemptNum = 1;

        // make sure to save the random seed number for the player's reference
        if (!seededRun)
            seed = Environment.TickCount;

        UnityEngine.Random.InitState(seed); // set the seed

        StartCoroutine(FloodFillRegions());
    }

    public IEnumerator FloodFillRegions()
    {
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

            mapTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), regionTiles[region]);
            frontiers[region].Enqueue(pos);
        }
       
        // flood fill expansion
        bool expanded = true;
        while (expanded)
        {
            yield return new WaitForSeconds(delay);

            expanded = false;

            // add to each region in a random order every iteration
            List<int> regionOrder = new List<int>(){ 1, 2, 3, 4 };
            Shuffle(regionOrder);

            foreach (int region in regionOrder)
            {
                // process all frontier items for this region
                int frontierCount = frontiers[region].Count;

                for (int i = 0; i < frontierCount; i++)
                {
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
                        if (mapTilemap.GetTile(new Vector3Int(next.x, next.y, 0)) != null)
                            continue;

                        // add to the region in the grid, queue this position to frontier
                        mapTilemap.SetTile(new Vector3Int(next.x, next.y, 0), regionTiles[region]);
                        frontiers[region].Enqueue(next);

                        expanded = true;
                    }
                }
            }
        }

        // set start room location somewhere in region 1
        List<Vector2Int> region1Tiles = GetTilesOfRegion(1);
        if (region1Tiles.Count > 0)
        {
            Vector2Int startingRoom = region1Tiles[UnityEngine.Random.Range(0, region1Tiles.Count)];
            mapTilemap.SetTile(new Vector3Int(startingRoom.x, startingRoom.y, 0), regionTiles[5]);
        }

        // if regions aren't adjacent in the correct way, try again
        if (!EnsureProgression())
        {
            Debug.Log("Failed Attempt #" + attemptNum + " - Regions do not touch");
            attemptNum++;
            mapTilemap.ClearAllTiles();
            StartCoroutine(FloodFillRegions());
        }

        runButton.interactable = true;
    }

    private bool InBounds(Vector2Int pos)
    {
        bool inBounds = (
            pos.x >= 0 && 
            pos.x < GRID_SIZE && 
            pos.y >= 0 &&
            pos.y < GRID_SIZE
        );

        return inBounds;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    public Vector2Int[] GenerateSeeds()
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
        int startQuadrant = UnityEngine.Random.Range(0, 4);

        // adjacent quadrants for seeds 2/3
        int adjacent1 = (startQuadrant + 1) % 4;
        int adjacent2 = (startQuadrant + 3) % 4;

        // opposite quadrant for seed 4
        int opposite = (startQuadrant + 2) % 4;

        Vector2Int[] seeds = new Vector2Int[4];

        seeds[0] = quadrants[startQuadrant].GetRandomPoint();

        // randomize which adjacent quadrant gets seed 2 or 3
        if (UnityEngine.Random.Range(0, 2) == 0)
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

    private List<Vector2Int> GetTilesOfRegion(int region)
    {
        // add all tile of region to a list and return
        List<Vector2Int> tiles = new List<Vector2Int>();
        Tile targetTile = regionTiles[region];

        for (int x = 0; x < GRID_SIZE; x++)
        {
            for (int y = 0; y < GRID_SIZE; y++)
            {
                if (mapTilemap.GetTile(new Vector3Int(x, y, 0)) == targetTile)
                    tiles.Add(new Vector2Int(x, y));
            }
        }

        return tiles;
    }

    private bool EnsureProgression()
    {
        bool region1TouchesRegion2 = AreRegionsAdjacent(1, 2);
        bool region3TouchesRegion1Or2 = AreRegionsAdjacent(3, 1) || AreRegionsAdjacent(3, 2);
        
        return region1TouchesRegion2 && region3TouchesRegion1Or2;
    }

    private bool AreRegionsAdjacent(int regionA, int regionB)
    {
        // check all tiles of a region against a target region
        List<Vector2Int> tilesA = GetTilesOfRegion(regionA);
        Tile targetTileB = regionTiles[regionB];

        foreach (Vector2Int tileA in tilesA)
        {
            // check all 4 neighbors of this tile
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = tileA + dir;

                if (!InBounds(neighbor))
                    continue;

                // if the regions touch at a single point,, return true
                if (mapTilemap.GetTile(new Vector3Int(neighbor.x, neighbor.y, 0)) == targetTileB)
                    return true;
            }
        }

        // if no tiles touch, regions are not adjacent
        return false;
    }
}
