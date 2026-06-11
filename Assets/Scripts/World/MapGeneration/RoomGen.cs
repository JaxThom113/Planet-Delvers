using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoomGen
{
    private static List<List<MapTile>> mapGrid;

    public static List<List<MapTile>> CreateRooms()
    {
        mapGrid = MapGenUtility.InitializeMapTileGrid();

        // connect MapTiles and make Rooms
        mapGrid = RoomPick.RoomPickGenerate(mapGrid);

        // connect Rooms by making doors
        mapGrid = CarveDoors.CarveDoorsGenerate(mapGrid);

        return mapGrid;
    }

    public static List<List<MapTile>> GetRooms()
    {
        return RoomPick.GetRooms();
    }
}
