/* using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace {

    Mesh mesh;
    int resolution;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    float radius;

    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();

    public TerrainFace(Mesh mesh, int resolution, Vector3 localUp, float radius)  // constructor for terrain face
    {
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;
        this.radius = radius;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);

        //Debug.Log(mesh.indexFormat);
    }

    public void ConstructTree() // called to construct quadtree for the face each update
    {
        //int oldVerticesNumber = vertices.ToArray().Length;
        //int oldTrianglesNumber = triangles.ToArray().Length;
        vertices.Clear();   // clears verts + tris lists
        triangles.Clear();

        Chunk parentChunk = new Chunk(null, null, localUp.normalized * Planet.size, radius, 0, localUp, axisA, axisB);  // creates root node of tree
        parentChunk.GenerateChildren(); // creates the tree by recursively generating all children for all nodes

        int triangleOffset = 0; // offset for the vertex index numbers used for triangles (NOT USED)
        foreach (Chunk child in parentChunk.GetVisibleChildren())   // for all leaf nodes (got recursively by checking if each node has children)
        {
            (Vector3[], int[]) verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset);  // calculates v + t for the node (aka chunk), code from above
            vertices.AddRange(verticesAndTriangles.Item1);  // adds node v+t to face v+t lists
            triangles.AddRange(verticesAndTriangles.Item2);
            triangleOffset += verticesAndTriangles.Item1.Length;    // increases offset by number of verts
        }

        mesh.Clear();   // clears all mesh data
        mesh.vertices = vertices.ToArray(); // creates mesh with new v + t
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        //if (mesh.vertices.Length != oldVerticesNumber)  // if num vertices has changed then log it
        //{
        //    Debug.Log("vertices number changed from " + oldVerticesNumber + " to " + mesh.vertices.Length);
        //    Debug.Log("triangles number changed from " + oldTrianglesNumber + " to " + mesh.triangles.Length);
        //}
    }
}

public class Chunk
{
    public Chunk[] children;
    public Chunk parent;
    public Vector3 position;
    public float radius;
    public int detailLevel;
    public Vector3 localUp;
    public Vector3 axisA;
    public Vector3 axisB;

    public Chunk(Chunk[] children, Chunk parent, Vector3 position, float radius, int detailLevel, Vector3 localUp, Vector3 axisA, Vector3 axisB)
    {
        this.children = children;
        this.parent = parent;
        this.position = position;
        this.radius = radius;
        this.detailLevel = detailLevel;
        this.localUp = localUp;
        this.axisA = axisA;
        this.axisB = axisB;
    }

    public void GenerateChildren()
    {
        if (detailLevel <= 8 && detailLevel >= 0)
        {
            if (Vector3.Distance(position.normalized * Planet.size, Planet.player.position) <= Planet.detailLevelDistances[detailLevel])
            {
                children = new Chunk[4];
                children[0] = new Chunk(new Chunk[0], this, position + axisA * radius / 2 + axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB);
                children[1] = new Chunk(new Chunk[0], this, position + axisA * radius / 2 - axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB);
                children[2] = new Chunk(new Chunk[0], this, position - axisA * radius / 2 + axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB);
                children[3] = new Chunk(new Chunk[0], this, position - axisA * radius / 2 - axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB);

                foreach (Chunk child in children)
                {
                    child.GenerateChildren();
                }
            }
        }
    }

    public Chunk[] GetVisibleChildren()
    {
        List<Chunk> toBeRendered = new List<Chunk>();

        if (children.Length > 0)
        {
            foreach (Chunk child in children)
            {
                toBeRendered.AddRange(child.GetVisibleChildren());
            }
        } else
        {
            toBeRendered.Add(this);
        }

        return toBeRendered.ToArray();
    }

    public (Vector3[], int[]) CalculateVerticesAndTriangles(int triangleOffset)
    {
        int resolution = 8;
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution-1) * (resolution-1) * 6];
        int triIndex = 0;

        int i = 0;
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                Vector2 percent = new Vector2(x, y) / (resolution-1);
                //Vector3 pointOnUnitCube = localUp + (percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB;
                
                Vector3 pointOnUnitCube = position + ((percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB) * radius;

                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized * Planet.size;
                vertices[i] = pointOnUnitSphere;

                if (x != (resolution-1) && y != (resolution-1))
                {
                    triangles[triIndex + 0] = i + triangleOffset;
                    triangles[triIndex + 1] = i + resolution + 1 + triangleOffset;
                    triangles[triIndex + 2] = i + resolution + triangleOffset;

                    triangles[triIndex + 3] = i + triangleOffset;
                    triangles[triIndex + 4] = i + 1 + triangleOffset;
                    triangles[triIndex + 5] = i + resolution + 1 + triangleOffset;

                    triIndex += 6;
                }
                i++;
            }
        }
        //mesh.Clear();
        //mesh.vertices = vertices;
        //mesh.triangles = triangles;
        //mesh.RecalculateNormals();
        return (vertices, triangles);
    }
}
 */