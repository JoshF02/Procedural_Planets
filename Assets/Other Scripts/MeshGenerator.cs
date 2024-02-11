using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    public int xSize = 20;
    public int zSize = 20;

    [Range(0f, 100f)]
    public float xOffset;
    [Range(0f, 100f)]
    public float zOffset;

    [Range(1f, 100f)]
    public int numHorizontalSegments;
    [Range(1f, 100f)]
    public int numVerticalSegments;

    [Range(1f, 20f)]
    public int resolution;

    public bool hasStarted = false;


    void Start()
    {
        hasStarted = true;
    }

    void Update()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateGridShape();
        UpdateMesh();
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

    void OnDrawGizmos()
    {
        if (hasStarted)
        {
            for (int i = 0; i < vertices.Length; i++) {
                Gizmos.DrawSphere(vertices[i], 0.02f);
            }
		}
    }




    // grid based terrain

    void CreateGridShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z = 0; z <= zSize; z++) {
            for (int x = 0; x <= xSize; x++) {

                float y = Mathf.PerlinNoise(x * .3f + xOffset, z * .3f + zOffset) * 2f; // uses perlin noise function for terrain height
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++) {
            for (int x = 0; x < xSize; x++) {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }



    // uv sphere

    void CreateUVSphere()
    {
        int vertexArraySize = 2 + numHorizontalSegments * (numVerticalSegments + 1);
        vertices = new Vector3[vertexArraySize];

        vertices[0] = Vector3.up * 2;   // top vertex

        int i = 1;  // vertex index starts at 1 as v[0] already assigned
        for (int h = 0; h < numHorizontalSegments; h++) {
            float phi = (h + 1) * Mathf.PI / (numHorizontalSegments + 1);

            for (int v = 0; v <= numVerticalSegments; v++) {
                float theta = v * (Mathf.PI * 2) / numVerticalSegments;

                float x = Mathf.Sin(phi) * Mathf.Cos(theta);
                float y = Mathf.Cos(phi);
                float z = Mathf.Sin(phi) * Mathf.Sin(theta);
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        vertices[vertexArraySize - 1] = Vector3.down;   // bottom vertex
    }

    


    // cube sphere

    void CreateCubeSphere()
    {
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        List<Vector3> verticesList = new List<Vector3>();

        for (int d = 0; d < 6; d++)
        {
            Vector3 localUp = directions[d];
            Vector3 localRight = new Vector3(localUp.y, localUp.z, localUp.x);
            Vector3 localBack = Vector3.Cross(localUp, localRight);

            Vector3[] faceVertices = new Vector3[resolution * resolution];

            int i = 0;
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    Vector2 percent = new Vector2(x, y) / (resolution - 1);
                    Vector3 pointOnUnitCube = localUp + (percent.x - 0.5f) * 2 * localRight + (percent.y - 0.5f) * 2 * localBack;
                    Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                    faceVertices[i] = pointOnUnitSphere;
                    i++;
                }
            }

            verticesList.AddRange(faceVertices);
        }

        vertices = verticesList.ToArray();
    }




    // icosphere

    // same as normalising?
    void ProjectToUnitSphere(Vector3[] vertices)    // scales vertex position vectors so length = radius (1)
    {
        for (int i = 0; i < vertices.Length; i++) {
            Vector3 vert = vertices[i]; // assumes sphere centre at origin - subtract sphere centre coords from vertices[i]
            float mag = vert.magnitude;  // magnitude is slow - sqrt

            vertices[i] = (1 / mag) * vert; // 1 = radius
        }
    }

    void CreateIcosahedron()
    {
        float phi = (1.0f + Mathf.Sqrt(5.0f)) * 0.5f; // golden ratio
        float a = 1.0f;
        float b = 1.0f / phi;

        vertices = new Vector3[12];

        vertices[0] = new Vector3(0, b, -a).normalized;
        vertices[1] = new Vector3(b, a, 0).normalized;
        vertices[2] = new Vector3(-b, a, 0).normalized;
        vertices[3] = new Vector3(0, b, a).normalized;
        vertices[4] = new Vector3(0, -b, a).normalized;
        vertices[5] = new Vector3(-a, 0, b).normalized;
        vertices[6] = new Vector3(0, -b, -a).normalized;
        vertices[7] = new Vector3(a, 0, -b).normalized;
        vertices[8] = new Vector3(a, 0, b).normalized;
        vertices[9] = new Vector3(-a, 0, -b).normalized;
        vertices[10] = new Vector3(b, -a, 0).normalized;
        vertices[11] = new Vector3(-b, -a, 0).normalized;

        //ProjectToUnitSphere(vertices);

        triangles = new int[60] // 20 sided shape, each side = triangle = 3 points
        {
            2, 1, 0,
            1, 2, 3,
            5, 4, 3,
            4, 8, 3,
            7, 6, 0,
            6, 9, 0,
            11, 10, 4,
            10, 11, 6,
            9, 5, 2,
            5, 9, 11,
            8, 7, 1,
            7, 8, 10,
            2, 5, 3,
            8, 1, 3,
            9, 2, 0,
            1, 7, 0,
            11, 9, 6,
            7, 10, 6,
            5, 11, 4,
            10, 8, 4
        };
    }


    /*public void SubdivideTriangles(int recursions)
    {
        var midPointCache = new Dictionary<int, int>();

        for (int i = 0; i < recursions; i++)
        {
            List<int> newTris = new List<int>();
            for (int t = 0; t < 60; t += 3)
            {
                int a = vertices[t + 0];
                int b = vertices[t + 1];
                int c = vertices[t + 2];
                // Use GetMidPointIndex to either create a
                // new vertex between two old vertices, or
                // find the one that was already created.
                int ab = GetMidPointIndex(midPointCache, a, b);
                int bc = GetMidPointIndex(midPointCache, b, c);
                int ca = GetMidPointIndex(midPointCache, c, a);
                // Create the four new polygons using our original
                // three vertices, and the three new midpoints.

                int[] one = new int[3] {a, ab, ca};
                int[] two = new int[3] {b, bc, ab};
                int[] three = new int[3] {c, ca, bc};
                int[] four = new int[3] {ab, bc, ca};

                newTris.AddRange(one);
                newTris.AddRange(two);
                newTris.AddRange(three);
                newTris.AddRange(four);
            }
            // Replace all our old polygons with the new set of
            // subdivided ones.
            triangles = newTris.ToArray();
        }
    }

    public int GetMidPointIndex(Dictionary<int, int> cache, int indexA, int indexB)
    {
        // We create a key out of the two original indices
        // by storing the smaller index in the upper two bytes
        // of an integer, and the larger index in the lower two
        // bytes. By sorting them according to whichever is smaller
        // we ensure that this function returns the same result
        // whether you call
        // GetMidPointIndex(cache, 5, 9)
        // or...
        // GetMidPointIndex(cache, 9, 5)
        int smallerIndex = Mathf.Min(indexA, indexB);
        int greaterIndex = Mathf.Max(indexA, indexB);
        int key = (smallerIndex << 16) + greaterIndex;
        // If a midpoint is already defined, just return it.
        int ret;
        if (cache.TryGetValue(key, out ret))
            return ret;
        // If we're here, it's because a midpoint for these two
        // vertices hasn't been created yet. Let's do that now!
        Vector3 p1 = vertices[indexA];
        Vector3 p2 = vertices[indexB];
        Vector3 middle = Vector3.Lerp(p1, p2, 0.5f).normalized;

        ret = vertices.Count;
        vertices.Add(middle);

        cache.Add(key, ret);
        return ret;
    }*/

    /*void SubdivideTriangles()
    {
        int recursionLevel = 2;

        for (int i = 0; i < recursionLevel; i++)
        {
            var faces = new List<int>();
            for (int t = 0; t < 60; t += 3)
            {   
                int a = getMiddlePoint(triangles[t], triangles[t+1]);
                int b = getMiddlePoint(triangles[t+1], triangles[t+2]);
                int c = getMiddlePoint(triangles[t+2], triangles[t]);

                faces.AddRange([triangles[t], a, c]);
                faces.AddRange([triangles[t+1], b, a]);
                faces.AddRange([triangles[t+2], c, b]);
                faces.AddRange([a, b, c]);
            }
            triangles = faces.ToArray();
        }
    }

    int getMiddlePoint(int p1, int p2)
    {
        Vector3 point1 = vertices[p1];
        Vector3 point2 = vertices[p2];
        Vector3 middle = new Vector3((point1.x + point2.x) / 2, (point1.y + point2.y) / 2, (point1.z + point2.z) / 2);

        return middle;
    }*/

    void CreateIcoSphere()
    {
        // subdivide icosahedron n times, projecting to unit sphere each time
        CreateIcosahedron();
        //SubdivideTriangles();
    }


}









public class Polygon
{
    public readonly List<int> vertices;

    public Polygon (int a, int b, int c)
    {
        vertices = new List<int>() { a, b, c };
    }
}

public class IcosahedronGenerator
{
    private List<Polygon> polygons;
    private List<Vector3> vertices;

    public List<Polygon> Polygons { get => polygons; private set => polygons = value; }
    public List<Vector3> Vertices { get => vertices; private set => vertices = value; }

    public void Initialize()
    {
        polygons = new List<Polygon>();
        vertices = new List<Vector3>();

        // An icosahedron has 12 vertices, and
        // since it's completely symmetrical the
        // formula for calculating them is kind of
        // symmetrical too:

        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        vertices.Add(new Vector3(-1, t, 0).normalized);
        vertices.Add(new Vector3(1, t, 0).normalized);
        vertices.Add(new Vector3(-1, -t, 0).normalized);
        vertices.Add(new Vector3(1, -t, 0).normalized);
        vertices.Add(new Vector3(0, -1, t).normalized);
        vertices.Add(new Vector3(0, 1, t).normalized);
        vertices.Add(new Vector3(0, -1, -t).normalized);
        vertices.Add(new Vector3(0, 1, -t).normalized);
        vertices.Add(new Vector3(t, 0, -1).normalized);
        vertices.Add(new Vector3(t, 0, 1).normalized);
        vertices.Add(new Vector3(-t, 0, -1).normalized);
        vertices.Add(new Vector3(-t, 0, 1).normalized);

        // And here's the formula for the 20 sides,
        // referencing the 12 vertices we just created.
        polygons.Add(new Polygon(0, 11, 5));
        polygons.Add(new Polygon(0, 5, 1));
        polygons.Add(new Polygon(0, 1, 7));
        polygons.Add(new Polygon(0, 7, 10));
        polygons.Add(new Polygon(0, 10, 11));
        polygons.Add(new Polygon(1, 5, 9));
        polygons.Add(new Polygon(5, 11, 4));
        polygons.Add(new Polygon(11, 10, 2));
        polygons.Add(new Polygon(10, 7, 6));
        polygons.Add(new Polygon(7, 1, 8));
        polygons.Add(new Polygon(3, 9, 4));
        polygons.Add(new Polygon(3, 4, 2));
        polygons.Add(new Polygon(3, 2, 6));
        polygons.Add(new Polygon(3, 6, 8));
        polygons.Add(new Polygon(3, 8, 9));
        polygons.Add(new Polygon(4, 9, 5));
        polygons.Add(new Polygon(2, 4, 11));
        polygons.Add(new Polygon(6, 2, 10));
        polygons.Add(new Polygon(8, 6, 7));
        polygons.Add(new Polygon(9, 8, 1));
    }

    public void Subdivide(int recursions)
    {
        var midPointCache = new Dictionary<int, int>();

        for (int i = 0; i < recursions; i++)
        {
            var newPolys = new List<Polygon>();
            foreach (var poly in polygons)
            {
                int a = poly.vertices[0];
                int b = poly.vertices[1];
                int c = poly.vertices[2];
                // Use GetMidPointIndex to either create a
                // new vertex between two old vertices, or
                // find the one that was already created.
                int ab = GetMidPointIndex(midPointCache, a, b);
                int bc = GetMidPointIndex(midPointCache, b, c);
                int ca = GetMidPointIndex(midPointCache, c, a);
                // Create the four new polygons using our original
                // three vertices, and the three new midpoints.
                newPolys.Add(new Polygon(a, ab, ca));
                newPolys.Add(new Polygon(b, bc, ab));
                newPolys.Add(new Polygon(c, ca, bc));
                newPolys.Add(new Polygon(ab, bc, ca));
            }
            // Replace all our old polygons with the new set of
            // subdivided ones.
            polygons = newPolys;
        }
    }

    public int GetMidPointIndex(Dictionary<int, int> cache, int indexA, int indexB)
    {
        // We create a key out of the two original indices
        // by storing the smaller index in the upper two bytes
        // of an integer, and the larger index in the lower two
        // bytes. By sorting them according to whichever is smaller
        // we ensure that this function returns the same result
        // whether you call
        // GetMidPointIndex(cache, 5, 9)
        // or...
        // GetMidPointIndex(cache, 9, 5)
        int smallerIndex = Mathf.Min(indexA, indexB);
        int greaterIndex = Mathf.Max(indexA, indexB);
        int key = (smallerIndex << 16) + greaterIndex;
        // If a midpoint is already defined, just return it.
        int ret;
        if (cache.TryGetValue(key, out ret))
            return ret;
        // If we're here, it's because a midpoint for these two
        // vertices hasn't been created yet. Let's do that now!
        Vector3 p1 = vertices[indexA];
        Vector3 p2 = vertices[indexB];
        Vector3 middle = Vector3.Lerp(p1, p2, 0.5f).normalized;

        ret = vertices.Count;
        vertices.Add(middle);

        cache.Add(key, ret);
        return ret;
    }
}
