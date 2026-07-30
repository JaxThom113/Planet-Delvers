using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LightingMesh
{
    private const int MAX_DEPTH = 4;

    private static readonly Vector2Int[] directions =
    {
        new Vector2Int(0, 1), // up
        new Vector2Int(0, -1),  // down
        new Vector2Int(-1, 0), // left
        new Vector2Int(1, 0),  // right
    };

    public static void CreateLightingMesh(GameObject lighting, List<List<float>> vertexDepthMap, Material lightingMaterial, float tileSize = 1f)
    {
        // add a filter and renderer to the gameobject
        MeshFilter filter = lighting.AddComponent<MeshFilter>();
        MeshRenderer renderer = lighting.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        filter.mesh = mesh;

        int vHeight = vertexDepthMap.Count;
        int vWidth = vertexDepthMap[0].Count;

        Vector3[] vertices = new Vector3[vWidth * vHeight];
        Color32[] colors = new Color32[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];

        int i = 0;

        for (int y = 0; y < vHeight; y++)
        {
            for (int x = 0; x < vWidth; x++)
            {
                vertices[i] = new Vector3(
                    x * tileSize,
                    y * tileSize, // flip y
                    0
                );

                float depthRatio = Mathf.Clamp01(vertexDepthMap[y][x] / MAX_DEPTH);

                byte alpha = (byte)(255 * depthRatio);
                colors[i] = new Color32(0, 0 ,0, alpha);

                // uv[i] = Vector2.zero;
                uv[i] = new Vector2(x / (float)(vWidth - 1), y / (float)(vHeight - 1));

                i++;
            }
        }

        int[] triangles = new int[(vWidth-1)*(vHeight-1)*6];

        int t = 0;

        for (int y = 0; y < vHeight-1; y++)
        {
            for (int x = 0; x < vWidth-1; x++)
            {
                int a = y*vWidth + x;
                int b = a + 1;
                int c = a + vWidth;
                int d = c + 1;

                triangles[t++] = a;
                triangles[t++] = c;
                triangles[t++] = b;

                triangles[t++] = b;
                triangles[t++] = c;
                triangles[t++] = d;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors32 = colors;
        mesh.uv = uv;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        renderer.material = lightingMaterial;

        renderer.sortingLayerName = "Environment";
        renderer.sortingOrder = 3;
    }

    /*
        Depth map helper functions
    */

    public static List<List<float>> GenerateDepthMap(List<List<int>> roomData)
    {
        // convert int list to float
        List<List<float>> depthMap = roomData.Select(innerList => innerList.Select(i => (float)i).ToList()).ToList();

        int height = depthMap.Count;
        int width = depthMap[0].Count;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (depthMap[y][x] == 0)
                    queue.Enqueue(new Vector2Int(x, y));
                else
                    depthMap[y][x] = -1;
            }
        }

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            float currentDepth = depthMap[current.y][current.x];

            // don't expand any farther
            if (currentDepth >= MAX_DEPTH)
                continue;

            foreach (Vector2Int dir in directions)
            {
                int nx = current.x + dir.x;
                int ny = current.y + dir.y;

                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;

                if (depthMap[ny][nx] != -1f)
                    continue;

                depthMap[ny][nx] = currentDepth + 1f;
                queue.Enqueue(new Vector2Int(nx, ny));
            }
        }

        // anything still -1 is farther than MAX_DEPTH
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (depthMap[y][x] == -1)
                    depthMap[y][x] = MAX_DEPTH;
            }
        }

        return depthMap;
    }

    public static List<List<float>> GenerateVertexDepthMap(List<List<float>> depthMap)
    {
        int height = depthMap.Count;
        int width = depthMap[0].Count;

        // increase height and width by 1 to account for the new vertices
        List<List<float>> vertexDepthMap = MapGenUtility.InitializeFloatGrid(width + 1, height + 1);

        for (int vy = 0; vy <= height; vy++)
        {
            for (int vx = 0; vx <= width; vx++)
            {
                float sum = 0;
                int count = 0;

                // top-left tile
                if (vx > 0 && vy > 0)
                {
                    sum += depthMap[vy - 1][vx - 1];
                    count++;
                }

                // top-right tile
                if (vx < width && vy > 0)
                {
                    sum += depthMap[vy - 1][vx];
                    count++;
                }

                // bottom-left tile
                if (vx > 0 && vy < height)
                {
                    sum += depthMap[vy][vx - 1];
                    count++;
                }

                // bottom-right tile
                if (vx < width && vy < height)
                {
                    sum += depthMap[vy][vx];
                    count++;
                }

                vertexDepthMap[vy][vx] = sum / count;
            }
        }

        return vertexDepthMap;
    }
}