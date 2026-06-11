using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CarveDoors
{
    private static readonly Vector2Int[] directions =
    {
        new Vector2Int(0, -1), // up
        new Vector2Int(0, 1),  // down
        new Vector2Int(-1, 0), // left
        new Vector2Int(1, 0),  // right
    };

    public static List<List<MapTile>> CarveDoorsGenerate(List<List<MapTile>> grid)
    {
        for (int region = 1; region <= 4; region++)
        {
            // get the rooms created in RoomPick
            List<List<MapTile>> shuffledRooms = RoomGen.GetRooms();
            MapGenUtility.Shuffle(shuffledRooms);

            foreach (List<MapTile> room in shuffledRooms)
            {
                MapTile current = new MapTile();
                List<int> validNeighbors = new List<int>();

                // keep rerolling the random tile in a room until you have one with neighbors
                int doorAttempts = 0;
                while (validNeighbors.Count == 0)
                {
                    doorAttempts++;
                    if (doorAttempts > 100)
                    {
                        Debug.LogWarning("Could not find valid neighbor for room - skipping door carving for this room");
                        break;
                    }

                    current = room[UnityEngine.Random.Range(0, room.Count)];

                    // { up, down, left, right }
                    bool[] neighbors = new bool[4] { false, false, false, false };
                    
                    for (int i = 0; i < directions.Length; i++)
                    {
                        Vector2Int neighbor = current.position + directions[i];

                        if (MapGenUtility.InBounds(neighbor))
                        {
                            // if the neighbor is valid and not part of the current room, add a door
                            if (grid[neighbor.y][neighbor.x].region != 0 && !room.Contains(grid[neighbor.y][neighbor.x]))
                                neighbors[i] = true;
                        }
                    }

                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        if (neighbors[i])
                            validNeighbors.Add(i);
                    }
                }

                // skip this room if no valid neighbors were found
                if (validNeighbors.Count == 0)
                    continue;

                // pick a random neighbor tile to carve a door to, 0 = up, 1 = down, 2 = left, 3 = right
                int neighborDirection = validNeighbors[UnityEngine.Random.Range(0, validNeighbors.Count)];
                bool[] doors = new bool[4] { false, false, false, false };
                doors[neighborDirection] = true;
                
                Vector2Int currentTilePos = current.position;
                grid[currentTilePos.y][currentTilePos.x].SetDoors(doors);

                // this also means you need to get that neighbor MapTile and place a door in the opposite direction
                int oppositeDirection = neighborDirection ^ 1; // this makes 0->1, 1->0, 2->3, 3->2
                bool[] neighborDoors = new bool[4] { false, false, false, false };
                neighborDoors[oppositeDirection] = true;

                Vector2Int neighborTilePos = current.position + directions[neighborDirection];
                grid[neighborTilePos.y][neighborTilePos.x].SetDoors(neighborDoors);
            }
        }

        // add doors to connect regions
        List<int[]> region2ConnectsRegion1 = FindAdjacentMapTiles(grid, 2, 1);
        foreach (int[] c in region2ConnectsRegion1)
        {
            // region 2 tile
            bool[] a = new bool[4] { false, false, false, false };
            a[c[4]] = true;
            grid[c[0]][c[1]].SetDoors(a);

            // region 1 tile
            bool[] b = new bool[4] { false, false, false, false };
            b[c[5]] = true;
            grid[c[2]][c[3]].SetDoors(b);
        }

        List<int[]> region3ConnectsRegion1Or2 = FindAdjacentMapTiles(grid, 3, 1);
        region3ConnectsRegion1Or2.AddRange(FindAdjacentMapTiles(grid, 3, 2));
        foreach (int[] c in region3ConnectsRegion1Or2)
        {
            // region 3 tile
            bool[] a = new bool[4] { false, false, false, false };
            a[c[4]] = true;
            grid[c[0]][c[1]].SetDoors(a);

            // region 1 or 2 tile
            bool[] b = new bool[4] { false, false, false, false };
            b[c[5]] = true;
            grid[c[2]][c[3]].SetDoors(b);
        }

        List<int[]> region4ConnectsRegion1Or2Or3 = FindAdjacentMapTiles(grid, 4, 1);
        region4ConnectsRegion1Or2Or3.AddRange(FindAdjacentMapTiles(grid, 4, 2));
        region4ConnectsRegion1Or2Or3.AddRange(FindAdjacentMapTiles(grid, 4, 3));
        foreach (int[] c in region4ConnectsRegion1Or2Or3)
        {
            // region 4 tile
            bool[] a = new bool[4] { false, false, false, false };
            a[c[4]] = true;
            grid[c[0]][c[1]].SetDoors(a);

            // region 1 or 2 or 3tile
            bool[] b = new bool[4] { false, false, false, false };
            b[c[5]] = true;
            grid[c[2]][c[3]].SetDoors(b);
        }

        return grid;
    }

    private static List<int[]> FindAdjacentMapTiles(List<List<MapTile>> grid, int regionA, int regionB)
    {
        int gridSize = MapGen.GridSize;

        // the int[] contains the index for regionA tile, index of regionB tile, direction of door for A, direction of door for B
        List<int[]> positions = new List<int[]>();

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                if (grid[y][x].region == regionA)
                {
                    for (int k = 0; k < directions.Length; k++)
                    {
                        int nx = x + directions[k].x;
                        int ny = y + directions[k].y;

                        if(!MapGenUtility.InBounds(new Vector2Int(nx, ny)))
                            continue;

                        MapTile neighbor = grid[ny][nx];
                        
                        if (neighbor.region == 0)
                            continue;
                        
                        // if valid neighbor tile
                        if (neighbor.region == regionB)
                        {
                            positions.Add(new int[6]{ y, x, ny, nx, k, k^1 });
                        }
                    }
                }
            }
        }

        return positions;
    }
}
