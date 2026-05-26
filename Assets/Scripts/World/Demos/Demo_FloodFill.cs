using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Demo_FloodFill : MonoBehaviour
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

        // make sure to save the random seed number for the player's reference
        if (!seededRun)
            seed = Environment.TickCount;

        UnityEngine.Random.InitState(seed); // set the seed

        StartCoroutine(FloodFillRegions());
    }

    public IEnumerator FloodFillRegions()
    {
        // set start room location
        Vector2Int start = new Vector2Int(
            UnityEngine.Random.Range(0, GRID_SIZE), 
            UnityEngine.Random.Range(0, GRID_SIZE)
        );
        mapTilemap.SetTile(new Vector3Int(start.x, start.y, 0), regionTiles[5]);

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
                    if (mapTilemap.GetTile(new Vector3Int(next.x, next.y, 0)) != null)
                        continue;

                    // add to the region in the grid, queue this position to frontier
                    mapTilemap.SetTile(new Vector3Int(next.x, next.y, 0), regionTiles[region]);
                    frontiers[region].Enqueue(next);

                    expanded = true;
                }
            }
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
}
