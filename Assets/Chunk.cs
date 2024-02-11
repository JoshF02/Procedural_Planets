using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    Chunk[] children;
    Chunk parent;
    Vector3 position;
    float radius;
    int detailLevel;
    Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    Planet planet;

    public Chunk(Chunk[] children, Chunk parent, Vector3 position, float radius, int detailLevel, Vector3 localUp, Vector3 axisA, Vector3 axisB, Planet planet)
    {
        this.children = children;
        this.parent = parent;
        this.position = position;
        this.radius = radius;
        this.detailLevel = detailLevel;
        this.localUp = localUp;
        this.axisA = axisA;
        this.axisB = axisB;
        this.planet = planet;
    }

    public Chunk[] FindLeafChunks()
    {
        List<Chunk> leafChunks = new List<Chunk>();

        if (detailLevel == 8) 
        {
            leafChunks.Add(this);
        }
        else if (Vector3.Distance(Planet.player.position, position.normalized * planet.planetSize) > (Planet.detailIncrements[detailLevel] * planet.planetSize))
        {
            leafChunks.Add(this);
        } 
        else
        {
            SubdivideChunk();

            foreach (Chunk child in children)
            {
                leafChunks.AddRange(child.FindLeafChunks());
            }
        }

        return leafChunks.ToArray();
    }

    public void SubdivideChunk()
    {
        children = new Chunk[4];
        children[0] = new Chunk(new Chunk[0], this, position + axisA * radius / 2 + axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB, planet);
        children[1] = new Chunk(new Chunk[0], this, position + axisA * radius / 2 - axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB, planet);
        children[2] = new Chunk(new Chunk[0], this, position - axisA * radius / 2 + axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB, planet);
        children[3] = new Chunk(new Chunk[0], this, position - axisA * radius / 2 - axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB, planet);
    }

    /*public Chunk[] GetLeafChildren()
    {
        List<Chunk> leafChildren = new List<Chunk>();

        if (children.Length == 0)
        {
            leafChildren.Add(this);
        } else
        {
            foreach (Chunk child in children)
            {
                leafChildren.AddRange(child.GetLeafChildren());
            }
        }

        return leafChildren.ToArray();
    }*/

    public (Vector3[], int[], int[]) GetVsAndTs(int triOffset)
    {
        int overlapResolution = planet.chunkResolution + 2; // the resolution of a chunk including overlap of 1 on each edge
        Vector3[] vertices = new Vector3[overlapResolution * overlapResolution];
        int[] triangles = new int[(overlapResolution-1) * (overlapResolution-1) * 6];
        int triIndex = 0;

        int smallerResolution = overlapResolution - 2; // the actual resolution of a chunk
        int[] smallerTriangles = new int[(smallerResolution-1) * (smallerResolution-1) * 6];
        int smallerTriIndex = 0;

        int i = 0;
        for (int y = 0; y < overlapResolution; y++)
        {
            for (int x = 0; x < overlapResolution; x++)
            {
                Vector2 percent = new Vector2(x-1, y-1) / (smallerResolution-1);
                Vector3 pointOnUnitCube = position + ((percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB) * radius;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                float elevation = EvaluateNoise(pointOnUnitSphere); // applies noise to each vertex
                elevation = planet.planetSize * (1 + elevation);
                planet.elevationMinMax.AddValue(elevation);
                vertices[i] = pointOnUnitSphere * elevation;

                if (x != (overlapResolution-1) && y != (overlapResolution-1))
                {
                    triangles[triIndex + 0] = i + triOffset;
                    triangles[triIndex + 1] = i + overlapResolution + 1 + triOffset;
                    triangles[triIndex + 2] = i + overlapResolution + triOffset;

                    triangles[triIndex + 3] = i + triOffset;
                    triangles[triIndex + 4] = i + 1 + triOffset;
                    triangles[triIndex + 5] = i + overlapResolution + 1 + triOffset;

                    triIndex += 6;

                    if (x != 0 && x != smallerResolution && y != 0 && y != smallerResolution) // adds non-overlap triangles to smallerTriangles array
                    {
                        smallerTriangles[smallerTriIndex + 0] = i + triOffset;
                        smallerTriangles[smallerTriIndex + 1] = i + overlapResolution + 1 + triOffset;
                        smallerTriangles[smallerTriIndex + 2] = i + overlapResolution + triOffset;

                        smallerTriangles[smallerTriIndex + 3] = i + triOffset;
                        smallerTriangles[smallerTriIndex + 4] = i + 1 + triOffset;
                        smallerTriangles[smallerTriIndex + 5] = i + overlapResolution + 1 + triOffset;

                        smallerTriIndex += 6;
                    }
                }
                i++;
            }
        }

        return (vertices, triangles, smallerTriangles);
    }

    private float EvaluateNoise(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = planet.baseRoughness;
        float amplitude = 1f;

        for (int i = 0; i < planet.numLayers; i++)
        {
            Vector3 samplePoint = point * frequency + planet.noiseCentre;

            // PERLIN NOISE
            float v = PerlinNoise3D(samplePoint.x, samplePoint.y, samplePoint.z);

            // SIMPLEX NOISE
            //float v = (float)planet.OpenSimplexNoise.Evaluate((double)samplePoint.x, (double)samplePoint.y, (double)samplePoint.z);

            noiseValue += (v + 1) * 0.5f * amplitude;
            frequency *= planet.roughness;
            amplitude *= planet.persistence;
        }

        noiseValue = Mathf.Max(0, noiseValue - planet.subtractionValue);
        return noiseValue * planet.strength;
    }


    private static float PerlinNoise3D(float x, float y, float z)
    {
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

    private static float _perlin3DFixed(float a, float b)
    {
        return Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(a, b));
    }
}

