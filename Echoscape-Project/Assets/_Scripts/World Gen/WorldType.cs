using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

namespace TerrainGeneration
{
    [CreateAssetMenu(fileName = "WorldType", menuName = "ScriptableObjects/World Type", order = 1)]
    public class WorldType : ScriptableObject
    {
        public string worldName = "";

        public PlanetAttributes planetAttributes;

        public bool useFlatShading;

        [Header("VFX Assets")]
        public Gradient meshGradient;
        public Material meshMaterial;
        public Shader meshShader;
        public VisualEffectAsset meshVFXAsset;

        [Header("Placeable Assets")]
        public List<GameObject> placeables = new List<GameObject>();
    }
}