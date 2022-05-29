using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGeneration
{
    public class TerrainManager : MonoBehaviour
    {
        [Header("Main Containers")]
        [SerializeField] private Chunk[] chunks;

        [Header("Terrain Settings")]
        [SerializeField] private PlanetAttributes planetAttributes;

        [Header("Density Texture")]
        [SerializeField] private RenderTexture densityTexture;
        [SerializeField] private ComputeShader densityShader;
        private int densityKernel;

        [Header("References")]
        [SerializeField] private Transform chunkHolder;

        #if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool drawDebug;
        #endif

        #region Unity Functions
        private void Start()
        {
            GetProcessHashes();

            float startTime = Time.realtimeSinceStartup;

            // Density Texture Generation
            CreateTextures();
            CreateChunks();
            // Await and Get Texture Data

            // Terrain Generation w/ Cube Marching
            GenerateTerrain();

            // Prop Generation
            PlaceSceneProps();
            // End of Prop Generation

            float endTime = Time.realtimeSinceStartup;
            Debug.Log("Whole Terrain Gen Time: " + (endTime - startTime));
        }

        private void OnDestroy()
        {
            densityTexture.Release();
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawDebug) return;

            // Draw Boundaries
            for (int i = 0; i < chunks.Length; i++)
            {
                if (i % 2 == 0) Gizmos.color = Color.white;
                else Gizmos.color = Color.yellow;

                Gizmos.DrawSphere(chunks[i].attributes.centre, 1f);
                float chunkBounds = (planetAttributes.terrainSize / planetAttributes.numChunks) * 0.8f;
                Gizmos.DrawWireCube(chunks[i].attributes.centre, new Vector3(chunkBounds, chunkBounds, chunkBounds));
            }
        }
        #endif
        #endregion

        private void GetProcessHashes()
        {
            densityKernel = densityShader.FindKernel("CSMain");
        }

        #region Density Texture Generation
        private void CreateTextures()
        {
            float textureGen_startTime = Time.realtimeSinceStartup;

            int size = planetAttributes.numChunks * (planetAttributes.pointsPerChunk - 1) + 1;
            GenerateTexture("DensityTexture", size, ref densityTexture);

            densityShader.SetTexture(densityKernel, "DensityTexture", densityTexture);
            GenerateTerrainDensity();

            Debug.Log("Texture Generation Time: " + (Time.realtimeSinceStartup - textureGen_startTime));
        }

        private void GenerateTerrainDensity()
        {
            int textureSize = densityTexture.width;

            densityShader.SetInt("textureSize", textureSize);
            densityShader.SetInt("planetSize", planetAttributes.numChunks);
            densityShader.SetFloat("noiseScale", planetAttributes.noiseScale);
            densityShader.SetFloat("noiseHeightMultiplier", planetAttributes.noiseHeightMultiplier);
            
            densityShader.Dispatch(densityKernel, textureSize, textureSize, 2);
        }

        /// <summary>
        /// Creates 3D RenderTexture with Correct Formatting
        /// </summary>
        /// <param name="name">Name of the Outputted RenderTexture</param>
        /// <param name="size">Size of the Render Texture</param>
        /// <param name="texture">The RenderTexture to apply to</param>
        private void GenerateTexture(string name, int size, ref RenderTexture texture)
        {
            if (texture == null || !texture.IsCreated() || texture.width != size || texture.height != size)
            {
                if (texture != null)
                {
                    texture.Release();
                }

                var textureFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat;

                texture = new RenderTexture(size, size, 0, textureFormat);
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
        private void GenerateTerrain()
        {
            NativeArray<ComputeStructs.Triangle> triangles = new NativeArray<ComputeStructs.Triangle>(256, Allocator.TempJob);

            PerformCubeMarch(ref triangles);
            GenerateMeshes(ref triangles);

            triangles.Dispose();
        }

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
                        centreX = (x * chunkSize) + (chunkSize / 2);
                        centreY = (y * chunkSize) + (chunkSize / 2);
                        centreZ = (z * chunkSize) + (chunkSize / 2);

                        GameObject chunk = new GameObject($"Chunk ({x}-{y}-{z})");
                        chunk.transform.parent = chunkHolder;
                        chunk.layer = chunkHolder.gameObject.layer;

                        chunks[i] = new Chunk
                        {
                            attributes = new ChunkAttributes 
                            {
                                id = new int3(x, y, z),
                                centre = new float3(centreX, centreY, centreZ),
                                size = chunkSize,
                            },
                            meshHolder = chunk,
                        };

                        i++;
                    }
                }
            }
        }

        private void PerformCubeMarch(ref NativeArray<ComputeStructs.Triangle> triangles)
        {
            float chunkGen_startTime = Time.realtimeSinceStartup;

            NativeArray<int> triangulationTable = new NativeArray<int>(256, Allocator.TempJob);

            NativeArray<ChunkAttributes> chunkArray = new NativeArray<ChunkAttributes>(chunks.Length, Allocator.TempJob);
            for (int i = 0; i < chunks.Length; i++)
            {
                chunkArray[i] = chunks[i].attributes;
            }

            MarchJob marchJob = new MarchJob(chunkArray, planetAttributes, densityTexture.width, triangulationTable, triangles);

            JobHandle jobHandle = marchJob.Schedule(chunks.Length, chunks.Length / 5);
            jobHandle.Complete();

            triangulationTable.Dispose();
            chunkArray.Dispose();
            
            Debug.Log("Chunk Generation Time: " + (Time.realtimeSinceStartup - chunkGen_startTime));
        }

        private void GenerateMeshes(ref NativeArray<ComputeStructs.Triangle> triangles)
        {
            foreach (Chunk chunk in chunks)
            {
                chunk.CreateMesh();
            }
        }
        #endregion

        #region Scene Placement
        /// <summary>
        /// Places the Required Scene Props on the Terrain.
        /// </summary>
        private void PlaceSceneProps()
        {
            float propGen_startTime = Time.realtimeSinceStartup;
            // Place Scene Objects
                // Spawn
                // Trees
                // Rocks

            float propGen_endTime = Time.realtimeSinceStartup;
            Debug.Log("Prop Generation Time: " + (propGen_endTime - propGen_startTime));
        }

        private Vector3 FindPlacementPosition()
        {
            return Vector3.zero;
        }
        #endregion
    }    
}