/* using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace {

    Mesh mesh;
    int resolution;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    Vector3 position;

    public List<Vector3> faceVertices = new List<Vector3>();
    public List<int> faceTriangles = new List<int>();

    public TerrainFace(Mesh mesh, int resolution, Vector3 localUp)  // constructor for terrain face
    {
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;
        this.position = localUp.normalized * 1f; //1 = planet size

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void ConstructMesh() // creates terrain face mesh
    {
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution-1) * (resolution-1) * 6];
        int triIndex = 0;

        int i = 0;
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                Vector2 percent = new Vector2(x, y) / (resolution-1);
                Vector3 pointOnUnitCube = position + (percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                vertices[i] = pointOnUnitSphere;

                if (x != (resolution-1) && y != (resolution-1))
                {
                    triangles[triIndex + 0] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;

                    triIndex += 6;
                }
                i++;
            }
        }

        Debug.Log(Vector3.Distance(position, Planet.player.position)); // logs position from face to player

        faceVertices.AddRange(vertices);
        faceTriangles.AddRange(triangles);

        mesh.Clear();
        mesh.vertices = faceVertices.ToArray();
        mesh.triangles = faceTriangles.ToArray();
        mesh.RecalculateNormals();
    }


    /* public void ConstructTree() // called to construct quadtree for the face each update
    {
        int oldVerticesNumber = vertices.ToArray().Length;  // saves num verts to see if it changes later

        vertices.Clear();   // clears verts + tris lists
        triangles.Clear();

        Chunk parentChunk = new Chunk(null, null, position, radius, 0, localUp, axisA, axisB);  // creates root node of tree
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

        if (mesh.vertices.Length != oldVerticesNumber)  // if num vertices has changed then log it
        {
            Debug.Log("vertices number changed from " + oldVerticesNumber + " to " + mesh.vertices.Length);
        }
    } 
}
 */