/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace {

    Mesh mesh;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    float radius;

    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();

    public TerrainFace(Mesh mesh, Vector3 localUp, float radius)  // constructor for terrain face, removed resolution
    {
        this.mesh = mesh;
        this.localUp = localUp;
        this.radius = radius;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void ConstructTree() // called to construct quadtree for the face each update
    {
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
        Planet planet = GameObject.FindGameObjectWithTag("Planet").GetComponent<Planet>();  // for the elevation, only find planet once

        int resolution = planet.resolution; // changed from set value of 8 to getting it from planet (variable)
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution-1) * (resolution-1) * 6];
        int triIndex = 0;

        int i = 0;
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                Vector2 percent = new Vector2(x, y) / (resolution-1);
                Vector3 pointOnUnitCube = position + ((percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB) * radius;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                // float elevation = planetScript.noiseFilter.Evaluate(pointOnUnitSphere);  // elevation using noise

                //float elevation = PerlinNoise3D(pointOnUnitSphere.x, pointOnUnitSphere.y, pointOnUnitSphere.z); // + 1f;

                float elevation = EvaluateNoise(pointOnUnitSphere, planet);

                //vertices[i] = pointOnUnitSphere * Planet.size * ((elevation * 0.5f) + 0.5f); //Mathf.Max(elevation, 1f);
                //vertices[i] = pointOnUnitSphere * Planet.size * (1 + elevation); // the way seblague did it

                elevation = Planet.size * (1 + elevation);
                planet.elevationMinMax.AddValue(elevation);
                vertices[i] = pointOnUnitSphere * elevation;    // modified for shader stuff

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

        return (vertices, triangles);
    }

    public float EvaluateNoise(Vector3 point, Planet planet)
    {
        float noiseValue = 0;
        float frequency = planet.baseRoughness;
        float amplitude = 1f;

        for (int i = 0; i < planet.numLayers; i++)
        {
            float v = PerlinNoise3D(point * frequency + planet.centre);
            noiseValue += (v + 1) * 0.5f * amplitude;
            frequency *= planet.roughness;
            amplitude *= planet.persistence;
        }

        noiseValue = Mathf.Max(0, noiseValue - planet.minValue);
        return noiseValue * planet.strength;
    }


    public static float PerlinNoise3D(Vector3 point)
    {
        float x = point.x;
        float y = point.y;
        float z = point.z;

        y += 1;
        z += 2;

        float xy = _perlin3DFixed(x, y);
        float xz = _perlin3DFixed(x, z);
        float yz = _perlin3DFixed(y, z);
        float yx = _perlin3DFixed(y, x);
        float zx = _perlin3DFixed(z, x);
        float zy = _perlin3DFixed(z, y);
        return xy * xz * yz * yx * zx * zy;
    }

    static float _perlin3DFixed(float a, float b)
    {
        return Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(a, b));
    }
}*/

// THIS FILE IS A COPY OF THE CODE BEFORE THE NORMALS FIX
