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

            // find groups of rooms that are disconnected and connect them
            ConnectDisconnectedRoomGroups(grid, RoomGen.GetRooms(), region);
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

    private static void ConnectDisconnectedRoomGroups(List<List<MapTile>> grid, List<List<MapTile>> allRooms, int region)
    {
        List<List<List<MapTile>>> roomGroups = GetConnectedRoomGroups(grid, allRooms, region);

        while (roomGroups.Count > 1)
        {
            // get groups of connected rooms repeatedly until there is only 1 room group
            roomGroups = GetConnectedRoomGroups(grid, allRooms, region);

            if (roomGroups.Count <= 1)
                return;

            for (int i = 1; i < roomGroups.Count; i++)
            {
                // check for connections between current groups to connection them
                int[] connection = FindRoomGroupConnections(grid, roomGroups[0], roomGroups[i], region);

                if (connection == null)
                    continue;
                    
                // there is a possible connection, so connect the room groups here
                ConnectRegions(grid, connection);
                break;
            }
        }
    }

    private static List<List<List<MapTile>>> GetConnectedRoomGroups(List<List<MapTile>> grid, List<List<MapTile>> allRooms, int region)
    {
        List<List<MapTile>> regionRooms = new List<List<MapTile>>();
        foreach (List<MapTile> room in allRooms)
        {
            if (room.Count > 0 && room[0].region == region)
                regionRooms.Add(room);
        }

        Dictionary<Vector2Int, List<MapTile>> roomByPosition = new Dictionary<Vector2Int, List<MapTile>>();
        foreach (List<MapTile> room in regionRooms)
        {
            foreach (MapTile tile in room)
            {
                roomByPosition[tile.position] = room;
            }
        }

        Dictionary<List<MapTile>, HashSet<List<MapTile>>> graph = new Dictionary<List<MapTile>, HashSet<List<MapTile>>>();
        foreach (List<MapTile> room in regionRooms)
        {
            graph[room] = new HashSet<List<MapTile>>();
        }

        foreach (List<MapTile> room in regionRooms)
        {
            foreach (MapTile mapTile in room)
            {
                MapTile tile = grid[mapTile.position.y][mapTile.position.x];

                for (int i = 0; i < directions.Length; i++)
                {
                    if (!tile.doors[i])
                        continue;

                    Vector2Int neighbor = tile.position + directions[i];

                    if (!MapGenUtility.InBounds(neighbor))
                        continue;

                    if (roomByPosition.TryGetValue(neighbor, out List<MapTile> neighborRoom) && neighborRoom != room)
                    {
                        graph[room].Add(neighborRoom);
                        graph[neighborRoom].Add(room);
                    }
                }
            }
        }

        List<List<List<MapTile>>> groups = new List<List<List<MapTile>>>();
        HashSet<List<MapTile>> visited = new HashSet<List<MapTile>>();

        foreach (List<MapTile> room in regionRooms)
        {
            if (visited.Contains(room))
                continue;

            List<List<MapTile>> group = new List<List<MapTile>>();
            Queue<List<MapTile>> queue = new Queue<List<MapTile>>();

            visited.Add(room);
            queue.Enqueue(room);

            while (queue.Count > 0)
            {
                List<MapTile> current = queue.Dequeue();
                group.Add(current);

                foreach (List<MapTile> neighbor in graph[current])
                {
                    if (visited.Add(neighbor))
                        queue.Enqueue(neighbor);
                }
            }

            groups.Add(group);
        }

        return groups;
    }

    private static int[] FindRoomGroupConnections(List<List<MapTile>> grid, List<List<MapTile>> roomGroupA, List<List<MapTile>> roomGroupB, int region)
    {
        HashSet<Vector2Int> groupBPositions = new HashSet<Vector2Int>();
        foreach (List<MapTile> room in roomGroupB)
        {
            foreach (MapTile tile in room)
            {
                groupBPositions.Add(tile.position);
            }
        }

        // check if a MapTile in group A neighbors a MapTile of group B
        List<int[]> connections = new List<int[]>();

        foreach (List<MapTile> room in roomGroupA)
        {
            foreach (MapTile tile in room)
            {
                for (int i = 0; i < directions.Length; i++)
                {
                    Vector2Int neighbor = tile.position + directions[i];

                    if (!MapGenUtility.InBounds(neighbor))
                        continue;

                    if (grid[neighbor.y][neighbor.x].region != region)
                        continue;

                    if (!groupBPositions.Contains(neighbor))
                        continue;

                    connections.Add(new int[6]{ tile.position.y, tile.position.x, neighbor.y, neighbor.x, i, i ^ 1});
                }
            }
        }

        if (connections.Count == 0)
            return null;

        return connections[UnityEngine.Random.Range(0, connections.Count)];
    }
}
