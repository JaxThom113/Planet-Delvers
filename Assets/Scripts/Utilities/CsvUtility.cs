using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class CsvUtility
{
    public static void SaveGridToCSV(List<List<int>> grid, string fileName)
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
        string[] lines = File.ReadAllLines(path);

        foreach (string line in lines)
        {
            List<int> row = line.Split(',').Select(int.Parse).ToList();
            grid.Add(row);
        }

        return grid;
    }
}
