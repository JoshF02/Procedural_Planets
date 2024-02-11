/* using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [Range(2, 256)]
    public int resolution = 2;
    public int detailLevel = 0;

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;

    public static Transform player;

    public static Dictionary<int, float> detailLevelDistances = new Dictionary<int, float>()
    {
        {0, Mathf.Infinity},
        {1, 60f},
        {2, 25f},
        {3, 10f},
        {4, 4f},
        {5, 1.5f},
        {6, 0.7f},
        {7, 0.3f},
        {8, 0.1f}
    };



    //private void OnValidate()
    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        /* float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= detailLevelDistances[detailLevel])
        {
            Debug.Log("Detail Level increased from " + detailLevel + " to " + (detailLevel + 1) + " new resolution = " + (resolution + 1));
            detailLevel++;
            resolution++;
        } else if (dist > detailLevelDistances[detailLevel - 1])
        {
            Debug.Log("Detail Level decreased from " + detailLevel + " to " + (detailLevel - 1) + " new resolution = " + (resolution - 1));
            detailLevel--;
            resolution--;
        } 

        Initialize();
        GenerateMesh();
    }

    void Initialize()
    {
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new TerrainFace[6];

        Vector3[] directions = {Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, resolution, directions[i]);
        }
    }

    void GenerateMesh()
    {
        foreach (TerrainFace face in terrainFaces)
        {
            face.ConstructMesh();
        }
    }
} */
