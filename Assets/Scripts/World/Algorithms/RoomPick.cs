using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoomPick
{
    private static List<List<MapTile>> allRooms;

    private static readonly Vector2Int[] directions =
    {
        new Vector2Int(0, -1), // up
        new Vector2Int(0, 1),  // down
        new Vector2Int(-1, 0), // left
        new Vector2Int(1, 0),  // right
    };

    // length x height
    private static readonly Vector2Int[] roomSizes =
    {
        new(1,1), new(1,2), new(1,3), new(1,4), new(1,5),
        new(2,1), new(2,2), new(2,3), new(2,4), new(2,5),
        new(3,1), new(3,2), new(3,3), new(3,4), new(3,5),
        new(4,1), new(4,2), new(4,3), new(4,4), new(4,5),
        new(5,1), new(5,2), new(5,3), new(5,4), new(5,5),
    };

    public static List<List<MapTile>> RoomPickGenerate(List<List<MapTile>> grid)
    {
        allRooms = new List<List<MapTile>>();

        for (int region = 1; region <= 4; region++)
        {
            HashSet<Vector2Int> targetStructureTiles = MapGenUtility.GetTiles(MapGen.StructureGrid, region);
            
            foreach (Vector2Int tile in targetStructureTiles)
            {
                // skip tiles already occupied by a room
                if (grid[tile.y][tile.x].region != 0)
                    continue;

                Vector2Int current = new Vector2Int(tile.x, tile.y);

                List<Vector2Int> shuffledRooms = new List<Vector2Int>(roomSizes);
                MapGenUtility.Shuffle(shuffledRooms);

                foreach (Vector2Int room in shuffledRooms)
                {
                    bool roomFits = true;
                    HashSet<Vector2Int> currentRoomTiles = new HashSet<Vector2Int>();

                    // for the currently selected room size, check if all tiles are part of the structure tilemap
                    for (int dx = 0; dx < room.x; dx++)
                    {
                        for (int dy = 0; dy < room.y; dy++)
                        {
                            Vector2Int loc = new Vector2Int(current.x + dx, current.y + dy);

                            if (!targetStructureTiles.Contains(loc) || grid[loc.y][loc.x].region != 0)
                            {
                                // if any one of the tiles is not in the current region's structure tiles 
                                // or on top of an existing room, this room size can't be placed
                                roomFits = false;
                                break;
                            }
                            else
                            {
                                currentRoomTiles.Add(new Vector2Int(loc.x, loc.y));
                            }
                        }

                        if (!roomFits)
                            break;
                    }

                    if (!roomFits)
                        continue;

                    // length is the size of the room about to be added
                    List<MapTile> roomToAdd = new List<MapTile>();

                    // if the room fits within structure and region maps, add it
                    for (int dx = 0; dx < room.x; dx++)
                    {
                        for (int dy = 0; dy < room.y; dy++)
                        {
                            Vector2Int loc = new Vector2Int(current.x + dx, current.y + dy);

                            // grid[loc.y][loc.x].SetPosition(loc);
                            // grid[loc.y][loc.x].SetRegion(region);

                            // go through each direction (up->down->left->right) and mark true if another tile of the room is there
                            bool[] connections = new bool[4] { false, false, false, false };
                            for (int i = 0; i < directions.Length; i++)
                            {
                                Vector2Int neighbor = loc + directions[i];
                                if (currentRoomTiles.Contains(neighbor))
                                    connections[i] = true;
                            }

                            // grid[loc.y][loc.x].SetConnections(connections);

                            // update this tile
                            MapTile thisTile = grid[loc.y][loc.x]; // grab copy
                            thisTile.SetPosition(loc);
                            thisTile.SetRegion(region);
                            thisTile.SetConnections(connections);
                            grid[loc.y][loc.x] = thisTile; // write back

                            roomToAdd.Add(grid[loc.y][loc.x]);
                        }
                    }

                    allRooms.Add(roomToAdd);

                    // if the process gets to this point, a room has been added, so skip over remaining shuffledRooms
                    break;
                }
            }
        }

        return grid;
    }

    public static List<List<MapTile>> GetRooms()
    {
        return allRooms;
    }
}
