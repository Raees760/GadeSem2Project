using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;
using Unity.AI.Navigation;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Dimensions")]
    [SerializeField] private int width = 256; // Controls Vertex DENSITY, not size
    [SerializeField] private int depth = 256;
    [SerializeField] private float terrainScale = 0.25f; // Controls Size

    [Header("Perlin Noise Parameters")]
    [SerializeField] private float scale = 20f;
    [SerializeField] private float amplitude = 10f;
    [SerializeField] private int octaves = 4;
    [SerializeField] private float lacunarity = 2f;
    [SerializeField] private float persistence = 0.5f;

    [Header("Level Design")]
    [SerializeField] private Texture2D[] levelDesigns;
    [SerializeField] private float pathHeight = 1f;
    [SerializeField] private float placementHeightOffset = 0.2f; //How much higher placement is than the path
    [SerializeField] private Material terrainMaterial; // Custom shader material
    [SerializeField] private int numberOfSpawners = 4;

    [Header("Game Object Prefabs")]
    [SerializeField] private GameObject mainTowerPrefab;
    [SerializeField] private GameObject enemySpawnerPrefab;
    [SerializeField] private GameObject towerPlacementNodePrefab;


    private Mesh mesh;
    private Vector3[] vertices;
    private Color[] vertexColors; // Array to hold vertex color data
    private NavMeshSurface navMeshSurface;
    private Texture2D selectedLevelDesign;

    void Start()
    {
        navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
        selectedLevelDesign = levelDesigns[Random.Range(0, levelDesigns.Length)];

        GenerateTerrain();
        BakeNavMesh();
    }

    private void GenerateTerrain()
    {
        GetComponent<MeshRenderer>().material = terrainMaterial;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateMeshShape();
        ApplyHeightsAndFeatures();
        UpdateMesh();
    }

    private void CreateMeshShape()
    {
        vertices = new Vector3[(width + 1) * (depth + 1)];
        vertexColors = new Color[vertices.Length]; 
        
        int[] triangles = new int[width * depth * 6];
        Vector2[] uvs = new Vector2[vertices.Length]; 

        for (int i = 0, z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                float scaledX = (x - width / 2f) * terrainScale;
                float scaledZ = (z - depth / 2f) * terrainScale;
                vertices[i] = new Vector3(scaledX, 0, scaledZ);
                uvs[i] = new Vector2((float)x / width, (float)z / depth); // Assign UV coordinates
                i++;
            }
        }
        
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + width + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + width + 1;
                triangles[tris + 5] = vert + width + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs; // Assign UVs to the mesh
    }

    private void ApplyHeightsAndFeatures()
    {
        float offsetX = Random.Range(0f, 9999f);
        float offsetY = Random.Range(0f, 9999f);

        List<Vector3> towerNodeLocations = new List<Vector3>();
        List<Vector3> spawnerLocations = new List<Vector3>();

        for (int i = 0; i < vertices.Length; i++)
        {
            // Sample the texture
            int texX = Mathf.RoundToInt(((vertices[i].x / terrainScale + width / 2f) / width) * selectedLevelDesign.width);
            int texY = Mathf.RoundToInt(((vertices[i].z / terrainScale + depth / 2f) / depth) * selectedLevelDesign.height);
            Color pixelColor = selectedLevelDesign.GetPixel(texX, texY);

            // Terracing logic
            if (pixelColor.r > 0.1f) // Red: Paths
            {
                vertices[i].y = pathHeight;
                vertexColors[i] = Color.red; // Assign red vertex color (for shader logic)
            }
            else if (pixelColor.g > 0.1f) // Green: Tower Placement
            {
                vertices[i].y = pathHeight + placementHeightOffset; // Set to slightly higher
                vertexColors[i] = Color.green; // Assign green vertex color
                towerNodeLocations.Add(new Vector3(vertices[i].x, vertices[i].y + 0.1f, vertices[i].z));
            }
            else if (pixelColor.b > 0.1f) // Blue: Enemy Spawners
            {
                vertices[i].y = pathHeight; // Spawners are on the path
                vertexColors[i] = Color.blue; // Assign blue vertex color
                spawnerLocations.Add(new Vector3(vertices[i].x, vertices[i].y + 0.5f, vertices[i].z));
            }
            else // Black: Perlin Terrain
            {
                float noiseHeight = 0f;
                float frequency = 1f;
                float currentAmplitude = 1f;
                for (int j = 0; j < octaves; j++)
                {
                    float sampleX = (vertices[i].x + offsetX) / scale * frequency;
                    float sampleZ = (vertices[i].z + offsetY) / scale * frequency;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    noiseHeight += perlinValue * currentAmplitude;
                    currentAmplitude *= persistence;
                    frequency *= lacunarity;
                }
                vertices[i].y = noiseHeight * amplitude;
                vertexColors[i] = Color.black; // Assign black vertex color 
            }
        }

        // Instantiate a controlled number of objects
        foreach (var pos in towerNodeLocations) { Instantiate(towerPlacementNodePrefab, pos, Quaternion.identity, transform); }
        
        for (int i = 0; i < numberOfSpawners && spawnerLocations.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, spawnerLocations.Count);
        
            // Instantiate the spawner prefab and keep a reference to it.
            GameObject spawnerGO = Instantiate(enemySpawnerPrefab, spawnerLocations[randomIndex], Quaternion.identity, transform);
        
            // Use the WaveManager's Singleton instance to register the new spawner's transform.
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.RegisterSpawnPoint(spawnerGO.transform);
            }
            else
            {
                Debug.LogError("WaveManager instance not found! Cannot register spawn point.");
            }
        
            spawnerLocations.RemoveAt(randomIndex);
        }
        
        Instantiate(mainTowerPrefab, new Vector3(0, pathHeight + 0.5f, 0), Quaternion.identity);
    }
    
    private void UpdateMesh()
    {
        mesh.vertices = vertices;
        mesh.colors = vertexColors; // Assign the vertex color data to the mesh
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        if (meshCollider == null) meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }

    private void BakeNavMesh()
    {
        if (navMeshSurface != null) navMeshSurface.BuildNavMesh();
    }
}