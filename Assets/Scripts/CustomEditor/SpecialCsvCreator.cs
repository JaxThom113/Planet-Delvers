using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpecialCsvCreator : MonoBehaviour
{
    [Header("Rooms")]
    [SerializeField] private SpecialRoomTilemap[] specialRooms; // all 1x1 rooms

    [Header("Tile References")]
    [SerializeField] private Tile[] fgTiles;     // foreground
    [SerializeField] private Tile[] bgTiles;     // background
    [SerializeField] private Tile[] hazardTiles; // hazards (spikes)
    [SerializeField] private Tile[] entityTiles; // entities (index 0-15 enemies, 16-32 bosses, 33-48 items)

    void Start()
    {
        SaveTilemapsToCSV(specialRooms);
    }

    public void SaveTilemapsToCSV(SpecialRoomTilemap[] rtArray)
    {
        for (int i = 0; i < rtArray.Length; i++)
        {
            SpecialRoomTilemap rt = specialRooms[i];

            // only save desired room numbers
            if (!rt.write)
                continue;

            Debug.Log($"Writing new special room from slot {i}...");

            List<List<int>> fgGrid = new List<List<int>>();
            List<List<int>> bgGrid = new List<List<int>>();
            List<List<int>> hazardGrid = new List<List<int>>();
            List<List<int>> entityGrid = new List<List<int>>();

            // read from top left -> bottom right
            for (int y = rt.dims.y - 1; y >= 0; y--)
            {
                for (int x = 0; x < rt.dims.x; x++)
                {
                    int py = y * 18;
                    int px = x * 32;

                    // get the grids to save for fg, bg, and entity tilemaps of this cell
                    fgGrid = ReadTilemap(1, rt, px, py);
                    bgGrid = ReadTilemap(2, rt, px, py);
                    hazardGrid = ReadTilemap(3, rt, px, py);
                    entityGrid = ReadTilemap(4, rt, px, py);
                }
            }

            // get the next number for a unnamed room
            int nextNum = GetNextNumber(
                $"Data/Special/", 
                $"Room"
            );

            string newPath = Path.Combine(Application.streamingAssetsPath, $"Data/Special/{rt.roomName}");

            // default to the room name "Room" if the name field is empty or room of this name already exists
            if (rt.roomName == null)
            {
                Debug.LogWarning($"No room name given, assigning name \"Room{nextNum}\"...");
                rt.roomName = $"Room{nextNum}";
            }
            else if (Directory.Exists(newPath))
            {
                Debug.LogWarning($"The room \"{rt.roomName}\" already exists, assigning name \"Room{nextNum}\"...");
                rt.roomName = $"Room{nextNum}";
            }

            CsvUtility.SaveGridToCSV(
                fgGrid, 
                $"Data/Special/",
                $"{rt.roomName}/",
                $"Fg/",
                $"1.csv"
            );

            CsvUtility.SaveGridToCSV(
                bgGrid, 
                $"Data/Special/",
                $"{rt.roomName}/",
                $"Bg/",
                $"1.csv"
            );

            CsvUtility.SaveGridToCSV(
                hazardGrid, 
                $"Data/Special/",
                $"{rt.roomName}/",
                $"Hazard/",
                $"1.csv"
            );

            CsvUtility.SaveGridToCSV(
                entityGrid, 
                $"Data/Special/",
                $"{rt.roomName}/",
                $"Entity/",
                $"1.csv"
            );
        }
    }

    private List<List<int>> ReadTilemap(int layer, RoomTilemap rt, int px, int py)
    {
        List<List<int>> grid = new List<List<int>>();

        for (int dy = py; dy < py + 18; dy++)
        {
            List<int> row = new List<int>();

            for (int dx = px; dx < px + 32; dx++)
            {
                TileBase thisTile = rt.fg.GetTile(new Vector3Int(dx, dy, 0));
                Tile[] currentTiles = fgTiles;

                switch (layer)
                {
                    case 1: // foreground
                        thisTile = rt.fg.GetTile(new Vector3Int(dx, dy, 0));
                        currentTiles = fgTiles;
                        break; 
                    case 2: // background
                        thisTile = rt.bg.GetTile(new Vector3Int(dx, dy, 0));
                        currentTiles = bgTiles;
                        break; 
                    case 3: // hazard
                        thisTile = rt.hazard.GetTile(new Vector3Int(dx, dy, 0));
                        currentTiles = hazardTiles;
                        break;
                    case 4: // entity
                        thisTile = rt.entity.GetTile(new Vector3Int(dx, dy, 0));
                        currentTiles = entityTiles;
                        break; 
                }

                // read the tiles in the tilemaps and add numbers to the csv accordingly
                for (int i = 0; i < currentTiles.Length; i++)
                {
                    if (thisTile == currentTiles[i])
                    {
                        row.Add(i+1);
                        break;
                    }
                    else if (i == currentTiles.Length - 1)
                    {
                        row.Add(0);
                        break;
                    }
                }
            }

            grid.Add(row);
        }

        // reverse so csv data is not inverted
        grid.Reverse();

        return grid;
    }

    public int GetNextNumber(string parentPath, string baseName)
    {
        string newPath = Path.Combine(Application.streamingAssetsPath, parentPath);

        if (!Directory.Exists(newPath))
            return 1;

        var existingNumbers = Directory.GetDirectories(newPath)
            .Select(path => Path.GetFileName(path))
            .Select(name => Regex.Match(name, $@"^{Regex.Escape(baseName)}_(\d+)$"))
            .Where(match => match.Success)
            .Select(match => int.Parse(match.Groups[1].Value))
            .ToList();

        if (!existingNumbers.Any())
            return 1;

        return existingNumbers.Max() + 1;
    }
}
