using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Quadrant
{
    public int xMin, xMax, yMin, yMax;

    public Quadrant(int xMin, int xMax, int yMin, int yMax)
    {
        this.xMin = xMin;
        this.xMax = xMax;
        this.yMin = yMin;
        this.yMax = yMax;
    }

    // get a random point within a quadrant
    public Vector2Int GetRandomPoint()
    {
        return new Vector2Int(
            Random.Range(xMin, xMax),
            Random.Range(yMin, yMax)
        );
    }
}