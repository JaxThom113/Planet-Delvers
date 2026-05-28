using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Demo_StructureMap : MonoBehaviour
{
    [Header("Settings")]
    [Range(0, 1)]
    [SerializeField] private float delay;
    [SerializeField] private bool seededRun;
    [SerializeField] private int seed;

    [Header("References")]
    [SerializeField] private Button runButton;
    [SerializeField] private Tilemap regionTilemap;
    [SerializeField] private Tile[] regionTiles;
    [SerializeField] private Tilemap structureTilemap;
    [SerializeField] private Tile[] structureTiles;

    // grid size must be defined here because MapGen isn't run in the demo scene
    private const int GRID_SIZE = 16;
    private int attemptNum;
    private Vector2Int startingRoom;

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
        regionTilemap.ClearAllTiles();
        structureTilemap.ClearAllTiles();

        attemptNum = 1;

        // make sure to save the random seed number for the player's reference
        if (!seededRun)
            seed = Environment.TickCount;

        UnityEngine.Random.InitState(seed); // set the seed

        StartCoroutine(GenerateMap());
    }

    private IEnumerator GenerateMap()
    {
        yield return StartCoroutine(FloodFillRegions());
        yield return StartCoroutine(CreateStructures());
    }

    private IEnumerator FloodFillRegions()
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

            regionTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), regionTiles[region]);
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
                        if (regionTilemap.GetTile(new Vector3Int(next.x, next.y, 0)) != null)
                            continue;

                        // add to the region in the grid, queue this position to frontier
                        regionTilemap.SetTile(new Vector3Int(next.x, next.y, 0), regionTiles[region]);
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
            startingRoom = region1Tiles[UnityEngine.Random.Range(0, region1Tiles.Count)];
            regionTilemap.SetTile(new Vector3Int(startingRoom.x, startingRoom.y, 0), regionTiles[5]);
        }

        // if regions aren't adjacent in the correct way, try again
        if (!EnsureProgression())
        {
            Debug.Log("Failed Attempt #" + attemptNum + " - Regions do not touch");
            attemptNum++;
            regionTilemap.ClearAllTiles();
            StartCoroutine(FloodFillRegions());
        }

        runButton.interactable = true;
    }

    private IEnumerator CreateStructures()
    {
        List<Vector2Int> regionTiles;

        // use a randomly picked algorithm to generate unique structures in each region
        for (int region = 1; region <= 4; region++)
        {
            regionTiles = GetTilesOfRegion(region);

            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                if (region == 1)
                    StartCoroutine(DfsGenerate(regionTiles, region, startingRoom.y, startingRoom.x));
                    
                Vector2Int seed = regionTiles[UnityEngine.Random.Range(0, regionTiles.Count)];
                StartCoroutine(DfsGenerate(regionTiles, region, seed.y, seed.x));
            }
            else
            {
                if (region == 1)
                    StartCoroutine(RandomWalkGenerate(regionTiles, region, startingRoom));
                
                Vector2Int seed = regionTiles[UnityEngine.Random.Range(0, regionTiles.Count)];
                StartCoroutine(RandomWalkGenerate(regionTiles, region, seed));
            }
        }

        // remember to add starting room location
        structureTilemap.SetTile(new Vector3Int(startingRoom.x, startingRoom.y, 0), structureTiles[5]);

        yield return null;
    }

    private IEnumerator DfsGenerate(List<Vector2Int> regionTiles, int region, int y = 0, int x = 0)
    {
        yield return new WaitForSeconds(delay);

        Vector2Int current = new Vector2Int(x, y);

        Vector2Int[] doubledDirections =
        {
            new Vector2Int(0, -2), // up
            new Vector2Int(0, 2),  // down
            new Vector2Int(-2, 0), // left
            new Vector2Int(2, 0),  // right
        };

        // randomize directions
        List<Vector2Int> dirs = new List<Vector2Int>(doubledDirections);
        Shuffle(dirs);

        foreach (Vector2Int dir in dirs)
        {
            Vector2Int next = current + dir;

            // check bounds
            if (InBounds(next) && regionTiles.Contains(next))
            {
                if (structureTilemap.GetTile(new Vector3Int(next.x, next.y, 0)) == null) // unvisited
                {
                    // carve path between current and neighbor
                    structureTilemap.SetTile(new Vector3Int(x + dir.x / 2, y + dir.y / 2, 0), structureTiles[region]);
                    structureTilemap.SetTile(new Vector3Int(next.x, next.y, 0), structureTiles[region]);

                    StartCoroutine(DfsGenerate(regionTiles, region, next.y, next.x));
                }
            }
        }
    }

    private IEnumerator RandomWalkGenerate(List<Vector2Int> regionTiles, int region, Vector2Int seed)
    {
        // random walk could fill a 4th of the region at the low end, full region at high end
        int pathLength = UnityEngine.Random.Range(regionTiles.Count / 2, regionTiles.Count);

        Vector2Int current = new Vector2Int(seed.x, seed.y);

        for (int i = 0; i < pathLength; i++)
        {
            yield return new WaitForSeconds(delay);

            List<Vector2Int> dirs = new List<Vector2Int>(directions);
            Shuffle(dirs);

            Vector2Int next = current;

            foreach (Vector2Int dir in dirs)
            {
                // try all 4 directions, take the first one that works (they are randomized by Shuffle)
                next = current + dir;

                if (InBounds(next) && regionTiles.Contains(next))
                {
                    // tiles that have already been visited can be visited again
                    structureTilemap.SetTile(new Vector3Int(next.x, next.y, 0), structureTiles[region]);
                    break;
                }
            }

            current = next;
        }
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

    private Vector2Int[] GenerateSeeds()
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
                if (regionTilemap.GetTile(new Vector3Int(x, y, 0)) == targetTile)
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
                if (regionTilemap.GetTile(new Vector3Int(neighbor.x, neighbor.y, 0)) == targetTileB)
                    return true;
            }
        }

        // if no tiles touch, regions are not adjacent
        return false;
    }
}
