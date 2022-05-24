using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace TerrainGeneration
{

    public class TerrainManager : MonoBehaviour
    {
        [Header("Terrain Settings")]
        [SerializeField] private int _terrainSize = 10;
        [SerializeField] private int _numChunks = 10;        
        [SerializeField] private int _pointsPerChunk = 100;

        [Space]

        [SerializeField] private float _noiseHeightMultiplier = 1f;
        [SerializeField] private float _noiseScale = 1f;

        [Header("Density Texture")]
        [SerializeField] private RenderTexture densityTexture;
        [SerializeField] private ComputeShader densityShader;
        private int densityKernel;

        // Start is called before the first frame update
        void Start()
        {
            densityKernel = densityShader.FindKernel("CSMain");
            CreateTextures();

            // Await Texture Creation

            // Generate Meshes using Job System

            // Await Terrain Generation

            // Place Scene Objects
                // Spawn
                // Trees
                // Rocks

        }

        #region Texture Generation
        private void CreateTextures()
        {
            int size = _numChunks * (_pointsPerChunk - 1) + 1;

            GenerateTexture("DensityTexture", size, ref densityTexture);

            densityShader.SetTexture(densityKernel, "DensityTexture", densityTexture);

            GenerateTerrainDensity();
        }

        private void GenerateTerrainDensity()
        {
            int textureSize = densityTexture.width;

            densityShader.SetInt("textureSize", textureSize);
            densityShader.SetInt("planetSize", _numChunks);
            densityShader.SetFloat("noiseScale", _noiseScale);
            densityShader.SetFloat("noiseHeightMultiplier", _noiseHeightMultiplier);
            
            densityShader.Dispatch(densityKernel, textureSize, textureSize, 2);
        }

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
    }    
}