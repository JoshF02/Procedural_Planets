using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    PlanetFace[] planetFaces;

    public Vector3 noiseCentre; // centre of noise, change to offset noise
    [Range(0f, 5f)]
    public float strength = 1f;
    [Range(2, 8)]
    public int numLayers = 2;   // number of layers of noise
    [Range(0f, 5f)]
    public float baseRoughness = 1f;    // base frequency of noise for the first layer
    [Range(0f, 5f)]
    public float roughness = 0f;    // roughness value, acts as frequency multiplier for sampling noise each layer  
    [Range(0f, 5f)]
    public float persistence = 0.5f;    // amplitude multiplier each noise layer
    [Range(0f, 5f)]
    public float subtractionValue; // value to subtract from noise elevation, bringing terrain inwards
    
    [Range(1, 1000)]
    public float planetSize = 10;
    [Range(2, 20)]
    public int chunkResolution = 8;  // resolution of each chunk

    public static Transform player;
    [HideInInspector]
    public Vector3 previousPlayerPos;
    [HideInInspector]
    public bool hasStarted = false;

    [HideInInspector]
    public Material planetMaterial;
    public MinMax elevationMinMax;
    public Gradient gradient;
    Texture2D texture;
    const int textureResolution = 50;

    public OpenSimplexNoise OpenSimplexNoise;

    public static float[] detailIncrements = new float[]
    {
        Mathf.Infinity,
        6f,
        2.5f,
        1f,
        0.4f,
        0.15f,
        0.07f,
        0.03f,
        0.01f//,
        //0.01f   // extra LOD causes gaps in face
    };


    private void Start()
    {
        OpenSimplexNoise = new OpenSimplexNoise();

        Initialize();
        UpdateMesh();

        StartCoroutine(PlanetUpdateLoop());
    }

    private void OnValidate()   // only generates new mesh when inspector settings changed
    {
        if (hasStarted)
        {
            UpdateMesh();
        }
    }

    private IEnumerator PlanetUpdateLoop()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.2f);
            if (player.position != previousPlayerPos)   // only generates new mesh if player moved
            {
                previousPlayerPos = player.position;
                UpdateMesh();
            }
        }
    }

    private void Initialize()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        previousPlayerPos = player.position;
        hasStarted = true;

        meshFilters = new MeshFilter[6];
        planetFaces = new PlanetFace[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("Face");
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = planetMaterial;
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            planetFaces[i] = new PlanetFace(meshFilters[i].sharedMesh, directions[i], planetSize);
        }
    }

    private void UpdateMesh()
    {
        elevationMinMax = new MinMax();
        if (texture == null)
        {
            texture = new Texture2D(textureResolution, 1);
        }

        foreach (PlanetFace face in planetFaces)
        {
            face.UpdateFace(this);
        }
        UpdateColours(elevationMinMax);
    }

    private void UpdateColours(MinMax elevationMinMax)
    {
        planetMaterial.SetVector("_elevationMinMax", new Vector4(elevationMinMax.Min, elevationMinMax.Max));

        Color[] colours = new Color[textureResolution];
        for (int i = 0; i < textureResolution; i++)
        {
            colours[i] = gradient.Evaluate(i / (textureResolution - 1f));
        }

        texture.SetPixels(colours);
        texture.Apply();
        planetMaterial.SetTexture("_texture", texture);
    }
}
