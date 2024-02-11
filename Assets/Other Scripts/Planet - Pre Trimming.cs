/* using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    //[Range(2, 256)]
    //public int resolution = 10;

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;


    public static float size = 10;
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


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        //Debug.Log(player);

        Initialize();
        GenerateMesh();

        StartCoroutine(PlanetGenerationLoop());
    }

    private IEnumerator PlanetGenerationLoop()
    {
        //int c = 0;
        while(true)
        {
            yield return new WaitForSeconds(0.1f);
            GenerateMesh();
            //Debug.Log("planet updated" + c);
            //c++;
        }
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
                // meshFilters[i].sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;   // allows more vertices per mesh, not needed?
            }

            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, 8, directions[i], 10);
        }
    }

    void GenerateMesh()
    {
        foreach (TerrainFace face in terrainFaces)
        {
            face.ConstructTree();
        }
    }
}
 */