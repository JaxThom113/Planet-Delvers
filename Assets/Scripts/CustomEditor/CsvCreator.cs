using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CsvCreator : MonoBehaviour
{
    [Header("Room Sizes")]
    [SerializeField] private RoomTilemap[] l1; // length 1 (1x1, 1x2, 1x3, 1x4, 1x5)
    [SerializeField] private RoomTilemap[] l2; // length 2 (2x1, 2x2, 2x3, 2x4, 2x5)
    [SerializeField] private RoomTilemap[] l3; // length 3 (3x1, 3x2, 3x3, 3x4, 3x5)
    [SerializeField] private RoomTilemap[] l4; // length 4 (4x1, 4x2, 4x3, 4x4, 4x5)
    [SerializeField] private RoomTilemap[] l5; // length 5 (5x1, 5x2, 5x3, 5x4, 5x5)

    [Header("Tile References")]
    [SerializeField] private Tile[] fgTiles;     // foreground, tiles player can collide with
    [SerializeField] private Tile[] bgTiles;     // background, tiles in the back for style
    [SerializeField] private Tile[] entityTiles; // entities, specifies what entities (enemies, items, etc.) will spawn and where

    void Start()
    {
        SaveTilemapsToCSV(l1);
        SaveTilemapsToCSV(l2);
        SaveTilemapsToCSV(l3);
        SaveTilemapsToCSV(l4);
        SaveTilemapsToCSV(l5);
    }

    public void SaveTilemapsToCSV(RoomTilemap[] rtArray)
    {
        foreach (RoomTilemap rt in rtArray)
        {
            // only save desired room sizes
            if (!rt.write)
                continue;

            Debug.Log($"Writing new {rt.dims.x}x{rt.dims.y} size room...");

            List<List<List<int>>> fgGrids = new List<List<List<int>>>();
            List<List<List<int>>> bgGrids = new List<List<List<int>>>();
            List<List<List<int>>> entityGrids = new List<List<List<int>>>();

            // read from top left -> bottom right
            for (int y = rt.dims.y - 1; y >= 0; y--)
            {
                for (int x = 0; x < rt.dims.x; x++)
                {
                    int py = y * 18;
                    int px = x * 32;

                    // get the grids to save for fg, bg, and entity tilemaps of this cell
                    List<List<int>> fgGrid = ReadTilemap(1, rt, px, py);
                    List<List<int>> bgGrid = ReadTilemap(2, rt, px, py);
                    List<List<int>> entityGrid = ReadTilemap(3, rt, px, py);

                    fgGrids.Add(fgGrid);
                    bgGrids.Add(bgGrid);
                    entityGrids.Add(entityGrid);
                }
            }

            // get the next number (i.e. you're creating a new 3x4, but folders 3x4_1
            // and 3x4_2 already exist, so make a new folder 3x4_3)
            int nextNum = GetNextNumber(
                $"Data/Rooms/{rt.dims.x}/{rt.dims.y}/", 
                $"{rt.dims.x}x{rt.dims.y}"
            );

            for (int i = 0; i < fgGrids.Count; i++)
            {
                CsvUtility.SaveGridToCSV(
                    fgGrids[i], 
                    $"Data/Rooms/{rt.dims.x}/{rt.dims.y}/",
                    $"{rt.dims.x}x{rt.dims.y}_{nextNum}/",
                    $"Fg/",
                    $"{i+1}.csv"
                );
            }

            for (int i = 0; i < bgGrids.Count; i++)
            {
                CsvUtility.SaveGridToCSV(
                    bgGrids[i], 
                    $"Data/Rooms/{rt.dims.x}/{rt.dims.y}/",
                    $"{rt.dims.x}x{rt.dims.y}_{nextNum}/",
                    $"Bg/",
                    $"{i+1}.csv"
                );
            }

            for (int i = 0; i < entityGrids.Count; i++)
            {
                CsvUtility.SaveGridToCSV(
                    entityGrids[i], 
                    $"Data/Rooms/{rt.dims.x}/{rt.dims.y}/",
                    $"{rt.dims.x}x{rt.dims.y}_{nextNum}/",
                    $"Entity/",
                    $"{i+1}.csv"
                );
            }
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
                    case 3: // entity
                        thisTile = rt.entity.GetTile(new Vector3Int(dx, dy, 0));
                        currentTiles = entityTiles;
                        break; 
                }

                if (thisTile == currentTiles[0])
                    row.Add(1);
                else if (thisTile == currentTiles[1])
                    row.Add(2);
                else if (thisTile == currentTiles[2])
                    row.Add(3);
                else if (thisTile == currentTiles[3])
                    row.Add(4);
                else
                    row.Add(0);
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
