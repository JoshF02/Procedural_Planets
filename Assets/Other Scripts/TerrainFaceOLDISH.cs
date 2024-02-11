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

    public List<Vector3> borderVertices = new List<Vector3>();
    public List<int> borderTriangles = new List<int>();

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
        borderVertices.Clear();
        borderTriangles.Clear();

        Chunk parentChunk = new Chunk(null, null, localUp.normalized * Planet.size, radius, 0, localUp, axisA, axisB);  // creates root node of tree
        parentChunk.GenerateChildren(); // creates the tree by recursively generating all children for all nodes

        int triangleOffset = 0; // offset for the vertex index numbers used for triangles (NOT USED)
        foreach (Chunk child in parentChunk.GetVisibleChildren())   // for all leaf nodes (got recursively by checking if each node has children)
        {
            (Vector3[], int[], Vector3[], int[]) verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset);  // calculates v + t for the node (aka chunk), code from above
            vertices.AddRange(verticesAndTriangles.Item1);  // adds node v+t to face v+t lists
            triangles.AddRange(verticesAndTriangles.Item2);
            borderVertices.AddRange(verticesAndTriangles.Item3);
            borderTriangles.AddRange(verticesAndTriangles.Item4);
            triangleOffset += verticesAndTriangles.Item1.Length;    // increases offset by number of verts
        }

        mesh.Clear();   // clears all mesh data
        mesh.vertices = vertices.ToArray(); // creates mesh with new v + t
        mesh.triangles = triangles.ToArray();
        //mesh.RecalculateNormals();                      // will need to change and have normals returned like v+t
        mesh.normals = CalculateNormals();
    }

    Vector3[] CalculateNormals()    // not using
    {
        Vector3[] vertexNormals = new Vector3[vertices.Count];  // count instead of length because lists not arrays
        int triangleCount = triangles.Count / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        int borderTriangleCount = borderTriangles.Count / 3;    // border triangle version of above
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0) {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0) {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0) {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? borderVertices[-indexA-1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? borderVertices[-indexB-1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVertices[-indexC-1] : vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
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

    public (Vector3[], int[], Vector3[], int[]) CalculateVerticesAndTriangles(int triangleOffset)
    {
        Planet planet = GameObject.FindGameObjectWithTag("Planet").GetComponent<Planet>();  // for the elevation, only find planet once

        // TRYING TO FIX SPACING - NEED TO IMPLEMENT MESHSIMPLIFICATIONINCREMENT INTO WHOLE OF THE CODE LIKE SEBLAGUE
        int meshSimplificationIncrement = (detailLevel == 0) ? 1 : detailLevel * 2;
    
        int borderedSize = planet.resolution + 2; // changed from set val of 8 to getting from planet variable, RES = BORDEREDSIZE = 8+2, TAKE AWAY THE +2?
        int meshSize = borderedSize - 2;// * meshSimplificationIncrement;
        int meshSizeUnsimplified = borderedSize - 2;


        Vector3[] vertices = new Vector3[meshSize * meshSize];
        int[] triangles = new int[(meshSize-1) * (meshSize-1) * 6]; // was breaking less when all 4 meshSize were borderedSize but seems fine now
        int triIndex = 0;

        Vector3[] borderVertices = new Vector3[meshSize * 4 + 4];
        int[] borderTriangles = new int[meshSize * 4 * 6];
        int borderTriIndex = 0;

        //int vertexIndex = 0;

        int[,] vertexIndicesMap = new int[borderedSize,borderedSize];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;

        for (int y = 0; y < borderedSize; y++)
        {
            for (int x = 0; x < borderedSize; x++)
            {
                bool isBorderVertex = y == 0 || y == borderedSize-1 || x == 0 || x == borderedSize-1;

                if (isBorderVertex)
                {
                    vertexIndicesMap[x,y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x,y] = meshVertexIndex;
                    meshVertexIndex++;
                }
                //vertexIndicesMap[x,y] = meshVertexIndex;    // temporary - no negative index numbers (meaning no border vs)
                //meshVertexIndex++;                          // shows that the stuff below is right, just need to implement border stuff
            }
        }

        for (int y = 0; y < borderedSize; y++)
        {
            for (int x = 0; x < borderedSize; x++)
            {
                int vertexIndex = vertexIndicesMap[x,y];

                // THIS IS WHERE I GOT TO (CHUNKS SPACED OUT)
                Vector2 percent = new Vector2(x, y) / (borderedSize-1);
                //Vector2 percent = new Vector2(x-meshSimplificationIncrement, y-meshSimplificationIncrement) / (float)(borderedSize - 2 * meshSimplificationIncrement);

                Vector3 pointOnUnitCube = position + ((percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB) * radius;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                float elevation = EvaluateNoise(pointOnUnitSphere, planet);
                elevation = Planet.size * (1 + elevation);
                planet.elevationMinMax.AddValue(elevation);


                // VERTICES STUFF
                //vertices[vertexIndex] = pointOnUnitSphere * elevation;
                Vector3 vertexPosition = pointOnUnitSphere * elevation;

                if (vertexIndex < 0)
                {
                    borderVertices[-vertexIndex-1] = vertexPosition;
                }
                else
                {
                    vertices[vertexIndex] = vertexPosition;
                    //uvs[vertexIndex] = uv;
                }


                // TRIANGLES STUFF
                if (x != (borderedSize-1) && y != (borderedSize-1))
                {
                    int a = vertexIndicesMap[x,y];
                    int b = vertexIndicesMap[x+1,y];
                    int c = vertexIndicesMap[x,y+1];
                    int d = vertexIndicesMap[x+1,y+1];

                    if (a < 0 || b < 0 || c < 0)
                    {
                        borderTriangles[borderTriIndex + 0] = a + triangleOffset;
                        borderTriangles[borderTriIndex + 1] = d + triangleOffset;
                        borderTriangles[borderTriIndex + 2] = c + triangleOffset;

                        borderTriangles[borderTriIndex + 3] = a + triangleOffset;
                        borderTriangles[borderTriIndex + 4] = b + triangleOffset;
                        borderTriangles[borderTriIndex + 5] = d + triangleOffset;
                        borderTriIndex += 6;
                    }
                    else
                    {
                        triangles[triIndex + 0] = a + triangleOffset;
                        triangles[triIndex + 1] = d + triangleOffset;
                        triangles[triIndex + 2] = c + triangleOffset;

                        triangles[triIndex + 3] = a + triangleOffset;
                        triangles[triIndex + 4] = b + triangleOffset;
                        triangles[triIndex + 5] = d + triangleOffset;
                        triIndex += 6;
                    }
                }
                //vertexIndex++;
            }
        }

        return (vertices, triangles, borderVertices, borderTriangles);
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


// THINK THIS FILE HAS ALL THE REALLY OLD UNDO/REDO EDIT HISTORY














































//  1ST ATTEMPT AT TRYING TO FOLLOW SEBLAGUE NORMALS VID

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
        mesh.RecalculateNormals();                      // will need to change and have normals returned like v+t
        //mesh.normals = CalculateNormals();
    }

    Vector3[] CalculateNormals()    // not using
    {
        Vector3[] vertexNormals = new Vector3[vertices.Count];  // count instead of length because lists not arrays
        int triangleCount = triangles.Count / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = vertices[indexA];
        Vector3 pointB = vertices[indexB];
        Vector3 pointC = vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
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
        int borderedSize = resolution + 2;
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





        
        Vector3[] borderVertices = new Vector3[borderedSize * borderedSize];

        int b = 0;
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
            //DVector3 pointOnCube = GetPointOnCube((DVector3) Presets.quadTemplateBorderVertices[quadIndex][i], scale, position, rotationMatrix);
            //Vector3 pointOnUnitSphere = (Vector3) pointOnCube.normalized;
            //float elevation = TerrainFace.GetElevation(planetScript.HighDefElevationConfig, pointOnUnitSphere) + 
            //    TerrainFace.GetElevation(planetScript.LowDefElevationConfig, pointOnUnitSphere);
            //borderVertices[i] = pointOnUnitSphere * ((1f + elevation) * planetScript.Size);

            Vector2 percent = new Vector2(x, y) / (resolution-1);
            Vector3 pointOnUnitCube = position + ((percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB) * radius;
            Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
            float elevation = EvaluateNoise(pointOnUnitSphere, planet);
            elevation = Planet.size * (1 + elevation);
            planet.elevationMinMax.AddValue(elevation);
            borderVertices[b] = pointOnUnitSphere * elevation;    // modified for shader stuff

            b++;
            }
        }

        int[] borderTriangles = new int[(borderedSize-1) * (borderedSize-1) * 6];
        // calculate the normals
        Vector3[] normals = new Vector3[vertices.Length];

        int triangleCount = triangles.Length / 3;

        int vertexIndexA;
        int vertexIndexB;
        int vertexIndexC;

        Vector3 triangleNormal;

        //int[] edgefansIndices = Presets.quadTemplateEdgeIndices[quadIndex];

        //for (int j = 0; j < triangleCount; j++)
        //{
        //    int normalTriangleIndex = j * 3;
        //    vertexIndexA = triangles[normalTriangleIndex];
        //    vertexIndexB = triangles[normalTriangleIndex + 1];
        //    vertexIndexC = triangles[normalTriangleIndex + 2];

        //    triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

            // dont calculate normals on the edge edgefans here, they are only calculated using the border vertices
        //    if (edgefansIndices[vertexIndexA] == 0)
        //    {
        //        normals[vertexIndexA] += triangleNormal;
        //    }
        //    if (edgefansIndices[vertexIndexB] == 0)
        //    {
        //        normals[vertexIndexB] += triangleNormal;
        //    }
        //    if (edgefansIndices[vertexIndexC] == 0)
        //    {
        //        normals[vertexIndexC] += triangleNormal;
        //    }
        //}

        int borderTriangleCount = borderTriangles.Length / 3;

        for (int j = 0; j < borderTriangleCount; j++)
        {
            int normalTriangleIndex = j * 3;
            vertexIndexA = triangles[normalTriangleIndex];
            vertexIndexB = triangles[normalTriangleIndex + 1];
            vertexIndexC = triangles[normalTriangleIndex + 2];


            //triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            Vector3 pointA = (vertexIndexA < 0) ? borderVertices[-vertexIndexA - 1] : vertices[vertexIndexA];
            Vector3 pointB = (vertexIndexB < 0) ? borderVertices[-vertexIndexB - 1] : vertices[vertexIndexB];
            Vector3 pointC = (vertexIndexC < 0) ? borderVertices[-vertexIndexC - 1] : vertices[vertexIndexC];

            // Get an aproximation of the vertex normal using two other vertices that share the same triangle
            Vector3 sideAB = pointB - pointA;
            Vector3 sideAC = pointC - pointA;
            triangleNormal = Vector3.Cross(sideAB, sideAC).normalized;
            

            // apply the normal if the vertex is on the visible edge of the quad
            if (vertexIndexA >= 0 && (vertexIndexA % resolution == 0 ||
                vertexIndexA % resolution == (resolution-1) ||
                (vertexIndexA >= 0 && vertexIndexA <= (resolution-1)) ||
                (vertexIndexA >= resolution * (resolution-1) && vertexIndexA < resolution * resolution)))
            {
                normals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0 && (vertexIndexB % resolution == 0 ||
                vertexIndexB % resolution == (resolution-1) ||
                (vertexIndexB >= 0 && vertexIndexB <= (resolution-1)) ||
                (vertexIndexB >= resolution * (resolution-1) && vertexIndexB < resolution * resolution)))
            {
                normals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0 && (vertexIndexC % resolution == 0 ||
                vertexIndexC % resolution == (resolution-1) ||
                (vertexIndexC >= 0 && vertexIndexC <= (resolution-1)) ||
                (vertexIndexC >= resolution * (resolution-1) && vertexIndexC < resolution * resolution)))
            {
                normals[vertexIndexC] += triangleNormal;
            }
        }

        // Normalise the result to combine the approximations into one
        for (int j = 0; j < normals.Length; j++)
        {
            normals[j].Normalize();
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