using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using System.Linq;

namespace TerrainGeneration
{
    #if DEBUG
    [System.Serializable]
    public struct GenTimes
    {
        public float textureGenTime;
        public float effectInitTime;
        public float chunkCreationTime;
        public float cubeMarchTime;
        public float propGenTime;

        public float wholeGenTime;

        public void DebugToConsole()
        {
            Debug.Log($"=== PROCESS TIMES ===\nTexture Time: {textureGenTime}s\nEffect Init: {effectInitTime}s\nChunk Creation: {chunkCreationTime}s\nCube Marching: {cubeMarchTime}s\nWhole Process: {wholeGenTime}s");
        }
    }
    #endif

    public class TerrainGenerator : MonoBehaviour, ISceneLoadProcess
    {
        [Header("Terrain Settings")]
        [SerializeField] private bool processFinished = false;
        private PlanetAttributes planetAttributes;
        private bool useFlatShading = false;
        [SerializeField] private Chunk[] chunks;
        [SerializeField] private Transform chunkHolder;
        [SerializeField] private Vector3 planetCentre;

        [SerializeField] private GameObject spawnPoint;
        [SerializeField] private GameObject escapePoint;

        [SerializeField] private List<GameObject> placeables = new List<GameObject>();

        [Header("Density Texture")]
        private int textureSize;
        [SerializeField] private RenderTexture densityTexture;
        [SerializeField] private ComputeShader densityShader;

        private int densityKernel;

        [Header("Mesh Settings")]
        [SerializeField] private Gradient meshGradient;
        [SerializeField] private Material meshMaterial;
        [SerializeField] private Shader meshShader;
        [SerializeField] private VisualEffectAsset meshVFXAsset;

        private Coroutine meshCreation;

        [Header("Scene Prop Settings")]
        [SerializeField] private LayerMask placeableMask;

        #if DEBUG
        [Header("Debug")]
        [SerializeField] private bool drawDebug = false;

        [SerializeField] private GenTimes genTimes;

        [Header("Gizmo Colours")]
        [SerializeField] private Color terrainBounds_Col;
        [SerializeField] private Color chunkBounds_Col;
        [SerializeField] private Color chunkCentre_Col;

        private float wholeProcessStartTime = 0.0f;
#endif

        #region Unity Functions

#if DEBUG
        private void Awake()
        {
            processFinished = false;            
            LoadPlanetOptions();

            wholeProcessStartTime = Time.realtimeSinceStartup;

            // Create Needed Textures
            float individualProcessTime = Time.realtimeSinceStartup;
            InitTextures();
            genTimes.textureGenTime = Time.realtimeSinceStartup - individualProcessTime;

            // Initialize Shader
            individualProcessTime = Time.realtimeSinceStartup;
            InitializeEffects();
            genTimes.effectInitTime = Time.realtimeSinceStartup - individualProcessTime;

            // Terrain Generation w/ Cube Marching
            individualProcessTime = Time.realtimeSinceStartup;
            CreateChunks();
            genTimes.chunkCreationTime = Time.realtimeSinceStartup - individualProcessTime;

            // Request Density Texture From GPU
            AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(densityTexture);

            // Terrain Generation For Each Chunk, Contained in Coroutine.
            meshCreation = StartCoroutine(GenerateTerrain(request));

        }

        private void OnDrawGizmos()
        {
            if (!drawDebug) return;

            Gizmos.color = terrainBounds_Col;
            Gizmos.DrawWireCube(planetCentre, (Vector3.one * planetAttributes.terrainSize));

            foreach (Chunk chunk in chunks)
            {
                Gizmos.color = chunkCentre_Col;
                Gizmos.DrawSphere(chunk.attributes.centre, 3f);

                Gizmos.color = chunkBounds_Col;
                Gizmos.DrawWireCube(chunk.attributes.centre, (Vector3.one * chunk.attributes.size));
            }

            Gizmos.DrawCube(UnityEngine.Random.onUnitSphere * planetAttributes.terrainSize, Vector3.one * 10f);
        }
#else
        private void Start()
        {
            InitTextures();

            // Initialize Shader
            InitializeEffects();

            // Terrain Generation w/ Cube Marching
            CreateChunks();

            // Request Density Texture From GPU
            AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(densityTexture);

            // Terrain Generation For Each Chunk, Contained in Coroutine.
            meshCreation = StartCoroutine(GenerateTerrain(request));
        }
#endif

        #endregion

        #region Texture Generation
        public RenderTexture getDensityTexture() => densityTexture;

        private void InitTextures()
        {
            densityKernel = densityShader.FindKernel("CSMain");

            textureSize = planetAttributes.numChunks * (planetAttributes.pointsPerAxis - 1) + 1;

            CreateTexture("Density Texture", textureSize, ref densityTexture);

            densityShader.SetTexture(densityKernel, "DensityTexture", densityTexture);

            ComputeDensity();
        }

        private void ComputeDensity()
        {
            densityShader.SetInt("textureSize", densityTexture.width);
            densityShader.SetFloat("planetSize", planetAttributes.terrainSize);
            densityShader.SetFloat("noiseScale", planetAttributes.noiseScale);
            densityShader.SetFloat("noiseHeightMultiplier", planetAttributes.noiseHeightMultiplier);
            densityShader.SetTexture(densityKernel, "DensityTexture", densityTexture);
        
            DispatchShader(densityShader, densityTexture.width, densityTexture.width, densityTexture.width, densityKernel);
        }

        private void CreateTexture(string name, int size, ref RenderTexture texture)
        {
            var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat;
            if (texture == null || !texture.IsCreated() || texture.width != size || texture.height != size || texture.volumeDepth != size || texture.graphicsFormat != format)
            {
                if (texture != null)
                {
                    texture.Release();
                }

                const int numBitsInDepthBuffer = 0;
                texture = new RenderTexture(size, size, numBitsInDepthBuffer);
                texture.graphicsFormat = format;
                texture.volumeDepth = size;
                texture.enableRandomWrite = true;
                texture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;

                texture.Create();
            }
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;
            texture.name = name;
        }
#endregion

        #region Terrain Generation
        private void LoadPlanetOptions()
        {
            WorldType chosenWorld;
            {
                WorldType[] worlds = Resources.LoadAll<WorldType>("Planets");
                chosenWorld = worlds[UnityEngine.Random.Range(0, worlds.Length - 1)];                
            }

            Debug.Log("Chosen World: " + chosenWorld.worldName);

            // Assign Planet Options
            planetAttributes = chosenWorld.planetAttributes;
            useFlatShading = chosenWorld.useFlatShading;
            
            // Load VFX Assets
            meshMaterial = chosenWorld.meshMaterial;
            meshShader = chosenWorld.meshShader;
            meshVFXAsset = chosenWorld.meshVFXAsset;
        }

        private void InitializeEffects()
        {
            meshMaterial.SetVector("PlanetCentre", chunkHolder.position);
        }

        private void CreateChunks()
        {
            chunks = new Chunk[planetAttributes.numChunks * planetAttributes.numChunks * planetAttributes.numChunks];
            float chunkSize = planetAttributes.terrainSize / planetAttributes.numChunks;
            
            int i = 0;
            float centreX, centreY, centreZ;

            float terrainOriginOffset = planetAttributes.terrainSize / 2;

            for (int x = 0; x < planetAttributes.numChunks; x++)
            {
                for (int y = 0; y < planetAttributes.numChunks; y++)
                {
                    for (int z = 0; z < planetAttributes.numChunks; z++)
                    {
                        centreX = (x * chunkSize) + (chunkSize / 2) - terrainOriginOffset;
                        centreY = (y * chunkSize) + (chunkSize / 2) - terrainOriginOffset;
                        centreZ = (z * chunkSize) + (chunkSize / 2) - terrainOriginOffset;
                        
                        GameObject chunkGameObject = new GameObject($"Chunk ({x}-{y}-{z})");
                        chunkGameObject.layer = chunkHolder.gameObject.layer;
                        chunkGameObject.transform.parent = chunkHolder;

                        int3 id = new int3(x, y, z);
                        float3 centre = new float3(centreX, centreY, centreZ);

                        chunks[i] = new Chunk(id, centre, chunkSize, meshMaterial, meshVFXAsset, chunkGameObject);

                        i++;
                    }
                }
            }
        }
        
        private IEnumerator GenerateTerrain(AsyncGPUReadbackRequest req)
        {
            req.WaitForCompletion();
            if (req.hasError)
            {
                Debug.LogError($"Error Occured in Texture Data Readback");
                yield break;
            }

            Debug.Log($"=== TEXTURE DEBUG INFO ===\n Texture Raw float Size: {textureSize * textureSize * textureSize}\n Texture Width/Height: {textureSize}\n Texture Byte Layer Offset {req.layerDataSize}");

            NativeArray<float> textureData = new NativeArray<float>(textureSize * textureSize * textureSize, Allocator.TempJob);
            for (int z = 0; z < req.layerCount; z++)
            {
                NativeArray<float> data = req.GetData<float>(z);

                for (int y = 0; y < req.height; y++)
                {
                    for (int x = 0; x < req.width; x++)
                    {
                        textureData[(z * (textureSize * textureSize)) + (y * textureSize) + x] = data[(y * textureSize) + x];
                    }
                }
            }

            NativeArray<int> triangulationTable = new NativeArray<int>(CubeMarchTables.GetFlatTriangulationTable(), Allocator.TempJob);
            NativeArray<int> cornerIndexATable = new NativeArray<int>(CubeMarchTables.cornerIndexAFromEdge, Allocator.TempJob);
            NativeArray<int> cornerIndexBTable = new NativeArray<int>(CubeMarchTables.cornerIndexBFromEdge, Allocator.TempJob);

            int numCubePerAxis = planetAttributes.pointsPerAxis - 1;
            int numCubesPerChunk = numCubePerAxis * numCubePerAxis * numCubePerAxis;
            NativeArray<float3> cubeIds = new NativeArray<float3>(numCubesPerChunk, Allocator.TempJob);
            for (int x = 0; x < numCubePerAxis; x++)
            {
                for (int y = 0; y < numCubePerAxis; y++)
                {
                    for (int z = 0; z < numCubePerAxis; z++)
                    {
                        cubeIds[(z * (numCubePerAxis* numCubePerAxis)) + (y * numCubePerAxis) + x] = new float3(x, y, z);
                    }
                }
            }

            NativeList<ComputeStructs.Triangle> triangles = new NativeList<ComputeStructs.Triangle>(numCubesPerChunk * 5, Allocator.TempJob);

#if DEBUG
            float individualProcessTime = Time.realtimeSinceStartup;
#endif

            foreach (Chunk chunk in chunks)
            {
                MarchChunk marchJob = new MarchChunk(planetAttributes, chunk.attributes, cubeIds,
                                                     triangulationTable, cornerIndexATable, cornerIndexBTable,
                                                     textureData, textureSize,
                                                     triangles.AsParallelWriter());
                JobHandle handler = marchJob.Schedule(cubeIds.Length, 1);
                handler.Complete();

                chunk.CreateMesh(triangles.ToArray(), useFlatShading);
                triangles.Clear();
            }

#if DEBUG
            genTimes.cubeMarchTime = Time.realtimeSinceStartup - individualProcessTime;
#endif

            textureData.Dispose();
            triangulationTable.Dispose();
            cornerIndexATable.Dispose();
            cornerIndexBTable.Dispose();
            cubeIds.Dispose();
            triangles.Dispose();

#if DEBUG
            individualProcessTime = Time.realtimeSinceStartup;
            GenerateSceneProps();
            genTimes.effectInitTime = Time.realtimeSinceStartup - individualProcessTime;

            genTimes.wholeGenTime = Time.realtimeSinceStartup - wholeProcessStartTime;

            genTimes.DebugToConsole();
#else
            GenerateSceneProps();
#endif

            processFinished = true;
        }
        #endregion

        #region Prop Generation
        private void GenerateSceneProps()
        {
            // Spawn Main Scene Props
            {
                Debug.Log("Placing Main Scene Props");

                SpawnPlaceable(ref spawnPoint);

                SpawnPlaceable(ref escapePoint);
            }

            // Spawn Additional Scene Props
            {
                
            }
        }

        private void SpawnPlaceable(ref GameObject gameObject)
        {
            Vector3 positionAroundTerrain = GetRandomPointAroundPlanet();
            
            RaycastHit rayhit;

            Ray ray = new Ray(positionAroundTerrain, (planetCentre - positionAroundTerrain).normalized);

            if (Physics.Raycast(ray, out rayhit, planetAttributes.terrainSize))
            {
                if (!rayhit.collider.gameObject.layer.Equals(placeableMask))
                {
                    GameObject instantiatedObject = Instantiate<GameObject>(gameObject, rayhit.point, Quaternion.identity);
                    
                    instantiatedObject.transform.localRotation = Quaternion.FromToRotation(Vector3.up, rayhit.normal);

                    gameObject = instantiatedObject;
                }
            }
            else
            {
                Debug.LogError($"Cannot Place Spawnable Object\nObject Name: {gameObject.name}");
            }
        }

        private Vector3 GetRandomPointAroundPlanet()
        {
            return UnityEngine.Random.onUnitSphere * (planetAttributes.terrainSize / 2);
        }
#endregion

        #region Helpers
        private void DispatchShader(ComputeShader shader, int iterationsX, int iterationsY = 1, int iterationsZ = 1, int kernel = 0)
        {
            uint x, y, z;
            shader.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
            Vector3Int threadGroupSizes = new Vector3Int((int) x, (int) y, (int) z);
            
            int numGroupsX = Mathf.CeilToInt(iterationsX / (float) threadGroupSizes.x);
            int numGroupsY = Mathf.CeilToInt(iterationsY / (float) threadGroupSizes.y);
            int numGroupsZ = Mathf.CeilToInt(iterationsZ / (float) threadGroupSizes.z);

            shader.Dispatch(kernel, numGroupsX, numGroupsY, numGroupsZ);
        }

        public bool FinishedProcess() => processFinished;
        #endregion
    }
}