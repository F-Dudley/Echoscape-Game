using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

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
        [SerializeField] private bool drawDebug;

        #endif

        #region Unity Functions
        private void Start()
        {
            // Create Needed Textures
            InitTextures();

            // Terrain Generation w/ Cube Marching
            CreateChunks();

            AsyncGPUReadbackRequest request = AsyncGPUReadback.Request(densityTexture);

            meshCreation = StartCoroutine(GenerateTerrain(request));
        }

        #if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            foreach (Chunk chunk in chunks)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(chunk.attributes.centre, 3f);

                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(chunk.attributes.centre, (Vector3.one * chunk.attributes.size));
            
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(Vector3.zero, (Vector3.one * planetAttributes.terrainSize));
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
            Debug.Log("Density Texture Size: " + (textureSize * textureSize * textureSize));

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

            for (int x = 0; x < planetAttributes.numChunks; x++)
            {
                for (int y = 0; y < planetAttributes.numChunks; y++)
                {
                    for (int z = 0; z < planetAttributes.numChunks; z++)
                    {
                        centreX = (-(planetAttributes.numChunks - 1f) / 2 + x) * planetAttributes.terrainSize;
                        centreY = (-(planetAttributes.numChunks - 1f) / 2 + y) * planetAttributes.terrainSize;
                        centreZ = (-(planetAttributes.numChunks - 1f) / 2 + z) * planetAttributes.terrainSize;
                        
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

            NativeArray<float> textureData = req.GetData<float>();

            NativeArray<int> triangulationTable = new NativeArray<int>(CubeMarchTables.GetFlatTriangulationTable(), Allocator.TempJob);
            NativeArray<int> cornerIndexATable = new NativeArray<int>(CubeMarchTables.cornerIndexAFromEdge, Allocator.TempJob);
            NativeArray<int> cornerIndexBTable = new NativeArray<int>(CubeMarchTables.cornerIndexBFromEdge, Allocator.TempJob);

            int numCubePerAxis = planetAttributes.pointsPerAxis - 1;
            NativeArray<int3> ids = new NativeArray<int3>(numCubePerAxis * numCubePerAxis * numCubePerAxis, Allocator.TempJob);
            for (int x = 0; x < numCubePerAxis; x++)
            {
                for (int y = 0; y < numCubePerAxis; y++)
                {
                    for (int z = 0; z < numCubePerAxis; z++)
                    {
                        ids[(z * numCubePerAxis) + (y * numCubePerAxis) + x] = new int3(x, y, z);
                    }
                }
            }

            NativeList<ComputeStructs.Triangle> triangles = new NativeList<ComputeStructs.Triangle>(50, Allocator.TempJob);
            Debug.Log($"Triangles Capacity: {triangles.Capacity}");

            Debug.Log($"Texture Data Size: {textureData.Length}\n Texture Data Initial Size: {textureSize}\n Triangulation Table Size: {triangulationTable.Length}\n Ids Table Size: {ids.Length}");

            float startTime = Time.time;
            foreach (Chunk chunk in chunks)
            {
                MarchChunk marchJob = new MarchChunk(planetAttributes, chunk.attributes, ids,
                                                     triangulationTable, cornerIndexATable, cornerIndexBTable,
                                                     textureData, textureSize, req.layerDataSize,
                                                     triangles.AsParallelWriter());
                JobHandle handler = marchJob.Schedule(ids.Length, 1);
                handler.Complete();

                chunk.CreateMesh(triangles.ToArray(), useFlatShading);
                triangles.Clear();
            }
            Debug.Log($"Time Taken: {Time.time - startTime}s");

            textureData.Dispose();
            triangulationTable.Dispose();
            cornerIndexATable.Dispose();
            cornerIndexBTable.Dispose();
            ids.Dispose();
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