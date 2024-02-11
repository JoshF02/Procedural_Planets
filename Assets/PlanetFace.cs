using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetFace {

    Mesh mesh;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    float radius;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<int> smallerTriangles = new List<int>();    // smaller tris list that doesn't include the edge tris

    public PlanetFace(Mesh mesh, Vector3 localUp, float radius)  // constructor for terrain face
    {
        this.mesh = mesh;
        this.localUp = localUp;
        this.radius = radius;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void UpdateFace(Planet planet) // called to create new mesh data for the face each update
    {
        vertices.Clear();   // clears verts + tris lists
        triangles.Clear();
        smallerTriangles.Clear();

        BuildQuadtree(planet);

        mesh.Clear();                                     // clears all mesh data
        mesh.vertices = vertices.ToArray();              // creates mesh with new v + t
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();                      // calculates normals using larger mesh data
        mesh.triangles = smallerTriangles.ToArray();   // assigns the smaller tris array to the mesh after normal calculations
    }

    private void BuildQuadtree(Planet planet)    // builds quadtree for the face
    {
        Chunk rootChunk = new Chunk(null, null, localUp.normalized * radius, radius, 0, localUp, axisA, axisB, planet);  // creates root node of tree
        Chunk[] leafChunks = rootChunk.FindLeafChunks(); // creates the tree by recursively generating all children for all nodes
        //Chunk[] leafChunks = rootChunk.GetLeafChildren();
        
        int triOffset = 0; // offset for the vertex index numbers used for triangles
        foreach (Chunk child in leafChunks)   // for all leaf nodes (got recursively by checking if each node has children)
        {
            (Vector3[], int[], int[]) verticesAndTriangles = child.GetVsAndTs(triOffset);  // calculates v + t for the node (aka chunk)
            vertices.AddRange(verticesAndTriangles.Item1);  // adds node v+t to face v+t lists
            triangles.AddRange(verticesAndTriangles.Item2);
            smallerTriangles.AddRange(verticesAndTriangles.Item3);
            triOffset += verticesAndTriangles.Item1.Length; // increases triangle offset by number of verts
        }
    }
}
