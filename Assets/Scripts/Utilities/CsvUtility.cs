using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class CsvUtility
{
    public static void SaveGridToCSV(List<List<int>> grid, string parentPath, string directoryName, string layerName, string fileName)
    {
        List<string> lines = new List<string>();
        foreach (List<int> row in grid)
        {
            lines.Add(string.Join(",", row));
        }

        // create parent directory (i.e. 3x4_4)
        string directory = Path.Combine(Application.dataPath, parentPath, directoryName);
        Directory.CreateDirectory(directory);

        // create desired layer directory (foreground, background, entity)
        string layerDirectory = Path.Combine(directory, layerName);
        Directory.CreateDirectory(layerDirectory);

        // create csv data for that layer (1.csv -> x.csv, where x is the number of connected rooms)
        string path = Path.Combine(layerDirectory, fileName);
        File.WriteAllLines(path, lines);
    }

    public static void SaveGridToCache(List<List<int>> grid, string fileName)
    {
        List<string> lines = new List<string>();
        foreach (List<int> row in grid)
        {
            lines.Add(string.Join(",", row));
        }

        string currentTime = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        string directory = Path.Combine(Application.dataPath, "Data", "Cache", currentTime);
        Directory.CreateDirectory(directory);

        string path = Path.Combine(directory, fileName);
        File.WriteAllLines(path, lines);
    }

    public static List<List<int>> LoadGridFromCSV(string fileName)
    {
        List<List<int>> grid = new List<List<int>>();

        string path = Path.Combine(Application.dataPath, fileName);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"File not found: {path}");
            return null;
        }

        string[] lines = File.ReadAllLines(path);

        foreach (string line in lines)
        {
            List<int> row = line.Split(',').Select(int.Parse).ToList();
            grid.Add(row);
        }

        return grid;
    }
}
