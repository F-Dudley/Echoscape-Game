using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using System.Linq;

namespace TerrainGeneration
{
    public class TerrainGenerator : MonoBehaviour
    {

        [Header("Terrain Settings")]
        [SerializeField] private PlanetAttributes planetAttributes;
        [SerializeField] private Chunk[] chunks;
        [SerializeField] private Transform chunkHolder;

        [Header("Density Texture")]
        [SerializeField] private int textureSize;
        [SerializeField] private RenderTexture densityTexture;
        [SerializeField] private ComputeShader densityShader;



        private int densityKernel;

        [Header("Mesh Settings")]
        [SerializeField] private Material meshMaterial;
        [SerializeField] private bool useFlatShading = true;

        private Coroutine meshCreation;

#if UNITY_EDITOR

        [Header("Debug")]
        [SerializeField] private bool drawDebug = false;

        [Header("Gizmo Colours")]
        [SerializeField] private Color terrainBounds_Col;
        [SerializeField] private Color chunkBounds_Col;
        [SerializeField] private Color chunkCentre_Col;


        #endif

        #region Unity Functions
        private void Start()
        {
            // Create Needed Textures
            InitTextures();

            // Terrain Generation w/ Cube Marching
            CreateChunks();

            // Request Density Texture From GPU
            AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(densityTexture);

            // Terrain Generation For Each Chunk, Contained in Coroutine.
            meshCreation = StartCoroutine(GenerateTerrain(request));
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (!drawDebug) return;

            Gizmos.color = terrainBounds_Col;
            Gizmos.DrawWireCube(Vector3.zero, (Vector3.one * planetAttributes.terrainSize));
            
            foreach (Chunk chunk in chunks)
            {
                Gizmos.color = chunkCentre_Col;
                Gizmos.DrawSphere(chunk.attributes.centre, 3f);

                Gizmos.color = chunkBounds_Col;
                Gizmos.DrawWireCube(chunk.attributes.centre, (Vector3.one * chunk.attributes.size));
            }
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
            Debug.Log("Density RenderTexture Size: " + (textureSize * textureSize * textureSize));

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

                        chunks[i] = new Chunk(id, centre, chunkSize, meshMaterial, chunkGameObject);

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
                        cubeIds[(z * numCubePerAxis) + (y * numCubePerAxis) + x] = new float3(x, y, z);
                    }
                }
            }

            NativeList<ComputeStructs.Triangle> triangles = new NativeList<ComputeStructs.Triangle>(numCubesPerChunk * 5, Allocator.TempJob);

            Debug.Log($"=== TEXTURE DEBUG INFO ===\n Texture Raw float Size: {textureData.Length}\n Texture Width/Height: {textureSize}\n Texture Byte Layer Offset {req.layerDataSize}");
            Debug.Log($"=== TABLE DEBUG INFO ===\n Triangulation Table Size: {triangulationTable.Length}\n Ids Table Size: {cubeIds.Length}\n Triangles Capacity{triangles.Capacity}");
            
            float startTime = Time.realtimeSinceStartup;
            foreach (Chunk chunk in chunks)
            {
                MarchChunk marchJob = new MarchChunk(planetAttributes, chunk.attributes, cubeIds,
                                                     triangulationTable, cornerIndexATable, cornerIndexBTable,
                                                     textureData, textureSize,
                                                     triangles.AsParallelWriter());
                JobHandle handler = marchJob.Schedule(cubeIds.Length, 1);
                handler.Complete();

                chunk.CreateMesh(triangles, useFlatShading);
                triangles.Clear();
            }
            Debug.Log($"Time Taken: {Time.realtimeSinceStartup - startTime}s");

            textureData.Dispose();
            triangulationTable.Dispose();
            cornerIndexATable.Dispose();
            cornerIndexBTable.Dispose();
            cubeIds.Dispose();
            triangles.Dispose();

            GenerateSceneProps();
        }
        #endregion

        #region Prop Generation
        private void GenerateSceneProps()
        {

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
        #endregion
    }
}