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
        [SerializeField] [ReadOnly] private int textureSize;
        [SerializeField] private RenderTexture densityTexture;
        [SerializeField] private ComputeShader densityShader;
        private int densityKernel;

        [Header("Mesh Settings")]
        [SerializeField] private Material meshMaterial;

        #if UNITY_EDITOR

        [Header("Debug")]
        [SerializeField] private bool drawDebug;

        #endif

        #region Unity Functions
        private void Start()
        {
            float startFullProcessTime = Time.realtimeSinceStartup;

            // Density Texture Generation
            float startTextureTime = Time.realtimeSinceStartup;
            InitTextures();
            Debug.Log("Texture Gen Time: " + (Time.realtimeSinceStartup - startTextureTime));

            // Terrain Generation w/ Cube Marching
            float startChunkTime = Time.realtimeSinceStartup;
            CreateChunks();
            GenerateChunks();
            Debug.Log("Chunk Gen Time: " + (Time.realtimeSinceStartup - startChunkTime));

            // Prop Gen
            float startPropTime = Time.realtimeSinceStartup;
            GenerateSceneProps();
            Debug.Log("Prop Gen Time: " + (Time.realtimeSinceStartup - startPropTime));

            Debug.Log("Full Terrain Gen Time: " + (Time.realtimeSinceStartup - startFullProcessTime));
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

                        chunks[i] = new Chunk 
                        {
                            attributes = new ChunkAttributes 
                            {
                                id = new int3(x, y, z),
                                centre = new float3(centreX, centreY, centreZ),
                                size = chunkSize,
                            },

                            meshHolder = chunkGameObject,
                        };

                        i++;
                    }
                }
            }
        }

        private void GenerateChunks()
        {
            NativeArray<int> triangulationTable = new NativeArray<int>(256 * 16, Allocator.TempJob);
            for (int format = 0; format < 256; format++)
            {
                for (int i = 0; i < 16; i++)
                {
                    triangulationTable[(format * 256) + i] = CubeMarchTables.triangulation[format][i];
                }
            }

            NativeArray<ChunkAttributes> chunkArray = new NativeArray<ChunkAttributes>(chunks.Length, Allocator.TempJob);
            for (int i = 0; i < chunks.Length; i++)
            {
                chunkArray[i] = chunks[i].attributes;
            }

            NativeHashMap<int, ListBuffer<ComputeStructs.Triangle>> triangles = new NativeHashMap<int, ListBuffer<ComputeStructs.Triangle>>(chunks.Length, Allocator.TempJob);
            Debug.Log(triangles[0][0]);

            NativeArray<float> densityArray = new NativeArray<float>(textureSize * textureSize * textureSize, Allocator.TempJob);
            AsyncGPUReadback.RequestIntoNativeArray(ref densityArray, densityTexture, 0, (req) => {
                if (req.hasError)
                {
                    Debug.LogError("Error:");
                    return;
                }
                else Debug.Log("Density Req Successful");

                // Perform Cube Marching;
                MarchJob marchingJob = new MarchJob(planetAttributes, chunkArray, densityArray, req.layerDataSize, textureSize, triangulationTable, triangles);

                JobHandle marchingJobHandle = marchingJob.Schedule(chunks.Length, chunks.Length / 10);
                marchingJobHandle.Complete();

                triangles.Dispose();
                triangulationTable.Dispose();
                chunkArray.Dispose();
                densityArray.Dispose();
            });
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