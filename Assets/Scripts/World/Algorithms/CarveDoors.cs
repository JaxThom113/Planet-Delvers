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
                // only process doors in the current region 
                // ensures new regions connected by 1 door each
                if (room.Count > 0 && room[0].region != region)
                    continue;

                MapTile current = new MapTile();
                List<int> validNeighbors = new List<int>();

                // keep rerolling the random tile in a room until you have one with neighbors
                List<MapTile> temp = new List<MapTile>(room);
                while (temp.Count > 0)
                {
                    validNeighbors.Clear();
                    current = temp[UnityEngine.Random.Range(0, temp.Count)];
                    temp.Remove(current);

                    // { up, down, left, right }
                    bool[] neighbors = new bool[4] { false, false, false, false };
                    
                    for (int i = 0; i < directions.Length; i++)
                    {
                        Vector2Int neighbor = current.position + directions[i];

                        if (!MapGenUtility.InBounds(neighbor))
                            continue;

                        // if the neighbor is part of the current region and not part of the current room, add a door
                        if (grid[neighbor.y][neighbor.x].region == region && !room.Contains(grid[neighbor.y][neighbor.x]))
                        {
                            neighbors[i] = true;
                        }
                    }

                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        if (neighbors[i])
                            validNeighbors.Add(i);
                    }

                    // take the first random tile in the room that has valid neighbors
                    if (validNeighbors.Count != 0)
                        break;
                }

                if (validNeighbors.Count == 0)
                    continue;

                // pick a random neighbor tile to carve a door to, 0 = up, 1 = down, 2 = left, 3 = right
                int neighborDirection = validNeighbors[UnityEngine.Random.Range(0, validNeighbors.Count)];
                
                // update current tile
                MapTile currentTile = grid[current.position.y][current.position.x];
                bool[] newDoors = currentTile.doors;
                newDoors[neighborDirection] = true;
                currentTile.SetDoors(newDoors);
                grid[current.position.y][current.position.x] = currentTile;

                // this also means you need to get that neighbor MapTile and place a door in the opposite direction
                int oppositeDirection = neighborDirection ^ 1; // this makes 0->1, 1->0, 2->3, 3->2

                // update neighbor tile
                MapTile neighborTile = grid[current.position.y + directions[neighborDirection].y][current.position.x + directions[neighborDirection].x];
                bool[] neighborDoors = neighborTile.doors;
                neighborDoors[oppositeDirection] = true;
                neighborTile.SetDoors(neighborDoors);
                grid[current.position.y + directions[neighborDirection].y][current.position.x + directions[neighborDirection].x] = neighborTile;
            }

            // foreach (List<MapTile> room in GetDisconnectedRooms())
            // {

            // }  
        }

        // connect region 2 with 1 at one of their touching points
        List<int[]> region2ConnectsRegion1 = FindAdjacentMapTiles(grid, 2, 1);
        ConnectRegions(grid, region2ConnectsRegion1[UnityEngine.Random.Range(0, region2ConnectsRegion1.Count)]);

        // connect region 3 with 1 or 2 at one of their touching points
        List<int[]> region3ConnectsRegion1Or2 = FindAdjacentMapTiles(grid, 3, 1);
        region3ConnectsRegion1Or2.AddRange(FindAdjacentMapTiles(grid, 3, 2));
        ConnectRegions(grid, region3ConnectsRegion1Or2[UnityEngine.Random.Range(0, region3ConnectsRegion1Or2.Count)]);

        // connect region 4 with 1 or 2 or 3 at one of their touching points
        List<int[]> region4ConnectsRegion1Or2Or3 = FindAdjacentMapTiles(grid, 4, 1);
        region4ConnectsRegion1Or2Or3.AddRange(FindAdjacentMapTiles(grid, 4, 2));
        region4ConnectsRegion1Or2Or3.AddRange(FindAdjacentMapTiles(grid, 4, 3));
        ConnectRegions(grid, region4ConnectsRegion1Or2Or3[UnityEngine.Random.Range(0, region4ConnectsRegion1Or2Or3.Count)]);

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

    private static void ConnectRegions(List<List<MapTile>> grid, int[] c)
    {
        // set the region A door
        MapTile currentTile = grid[c[0]][c[1]];
        bool[] currentDoors = currentTile.doors;
        currentDoors[c[4]] = true;
        currentTile.SetDoors(currentDoors);
        grid[c[0]][c[1]] = currentTile;

        // set the region B neighbor door
        MapTile neighborTile = grid[c[2]][c[3]];
        bool[] neighborDoors = neighborTile.doors;
        neighborDoors[c[5]] = true;
        neighborTile.SetDoors(neighborDoors);
        grid[c[2]][c[3]] = neighborTile;
    }

    private static List<MapTile> GetDisconnectedRooms()
    {
        // find disconnected rooms or groups of rooms
        // and add a door to connect them to the rest of the region
        return null;
    }

    private static bool AreAllRoomsConnected(List<List<MapTile>> rooms, List<List<MapTile>> grid, int region)
    {
        if (rooms.Count <= 1) return true;
        
        // Build an adjacency graph between rooms based on doors
        var roomGraph = new Dictionary<List<MapTile>, HashSet<List<MapTile>>>();
        
        // For each room, find which other rooms it's connected to via doors
        foreach (var room in rooms)
        {
            if (room.Count == 0 || room[0].region != region) continue;
            roomGraph[room] = new HashSet<List<MapTile>>();
            
            foreach (var tile in room)
            {
                // Check each door and see if it connects to another room
                for (int i = 0; i < 4; i++)
                {
                    if (tile.doors[i])
                    {
                        Vector2Int neighbor = tile.position + directions[i];
                        var neighborTile = grid[neighbor.y][neighbor.x];
                        
                        // Find which room this tile belongs to
                        foreach (var otherRoom in rooms)
                        {
                            if (otherRoom != room && otherRoom.Contains(neighborTile))
                                roomGraph[room].Add(otherRoom);
                        }
                    }
                }
            }
        }
        
        // BFS from first room to see if all rooms are reachable
        var visited = new HashSet<List<MapTile>>();
        var queue = new Queue<List<MapTile>>();
        queue.Enqueue(rooms[0]);
        visited.Add(rooms[0]);
        
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var neighbor in roomGraph[current])
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        
        return visited.Count == rooms.Count;
    }
}
