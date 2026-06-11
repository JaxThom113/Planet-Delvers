using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Wrapper classes for JSON serialization (Unity's JsonUtility requires non-generic lists)
[System.Serializable]
public class SerializableMapTile
{
    public Vector2Int position;
    public int region;
    public bool visited;
    public bool[] doors; // { up, down, left, right }
    public bool[] connections; // { up, down, left, right }

    public SerializableMapTile() { }

    public SerializableMapTile(MapTile tile)
    {
        position = tile.position;
        region = tile.region;
        visited = tile.visited;
        doors = tile.doors;
        connections = tile.connections;
    }

    public MapTile ToMapTile()
    {
        MapTile tile = new MapTile();
        tile.SetPosition(position);
        tile.SetRegion(region);
        tile.SetVisited(visited);
        tile.SetDoors(doors);
        tile.SetConnections(connections);
        return tile;
    }
}

[System.Serializable]
public class SerializableMapTileRow
{
    public SerializableMapTile[] tiles;

    public SerializableMapTileRow() { }

    public SerializableMapTileRow(List<MapTile> row)
    {
        tiles = new SerializableMapTile[row.Count];
        for (int i = 0; i < row.Count; i++)
        {
            tiles[i] = new SerializableMapTile(row[i]);
        }
    }
}

[System.Serializable]
public class SerializableMapTileGrid
{
    public SerializableMapTileRow[] rows;

    public SerializableMapTileGrid() { }

    public SerializableMapTileGrid(List<List<MapTile>> grid)
    {
        rows = new SerializableMapTileRow[grid.Count];
        for (int i = 0; i < grid.Count; i++)
        {
            rows[i] = new SerializableMapTileRow(grid[i]);
        }
    }

    public List<List<MapTile>> ToMapTileGrid()
    {
        List<List<MapTile>> grid = new List<List<MapTile>>();
        foreach (SerializableMapTileRow row in rows)
        {
            List<MapTile> tileRow = new List<MapTile>();
            foreach (SerializableMapTile tile in row.tiles)
            {
                tileRow.Add(tile.ToMapTile());
            }
            grid.Add(tileRow);
        }
        return grid;
    }
}

public static class JsonUtility
{
    public static void SaveGridToJson(List<List<MapTile>> grid, string fileName)
    {
        SerializableMapTileGrid serializableGrid = new SerializableMapTileGrid(grid);
        string json = UnityEngine.JsonUtility.ToJson(serializableGrid, true); // true for pretty print

        string currentTime = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        string directory = Path.Combine(Application.dataPath, "Data", "Cache", currentTime);
        Directory.CreateDirectory(directory);

        string path = Path.Combine(directory, fileName);
        File.WriteAllText(path, json);
    }

    public static List<List<MapTile>> LoadGridFromJson(string fileName)
    {
        string path = Path.Combine(Application.dataPath, fileName);

        string json = File.ReadAllText(path);
        SerializableMapTileGrid serializableGrid = UnityEngine.JsonUtility.FromJson<SerializableMapTileGrid>(json);

        return serializableGrid.ToMapTileGrid();
    }
}
