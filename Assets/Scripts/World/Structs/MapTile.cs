using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MapTile
{
    public Vector2Int position;
    public int region;
    public bool visited;
    public bool[] doors; // { up, down, left, right }
    public bool[] connections; // { up, down, left, right }

    public void SetPosition(Vector2Int position)
    {
        this.position = position;
    }

    public void SetRegion(int region)
    {
        this.region = region;
    }

    public void SetVisited(bool visited)
    {
        this.visited = visited;
    }

    public void SetDoors(bool[] doors)
    {
        this.doors = doors;
    }

    public void SetConnections(bool[] connections)
    {
        this.connections = connections;
    }
}