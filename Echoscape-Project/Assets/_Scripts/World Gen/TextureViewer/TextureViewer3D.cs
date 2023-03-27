using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainGeneration
{
	public class TextureViewer3D : MonoBehaviour
	{

		[Range(0,1)]
		[SerializeField] private float sliceDepth;
		[SerializeField] private Material material;

		[SerializeField] private TerrainGenerator generator;

		void Update()
		{
			material.SetFloat("sliceDepth", sliceDepth);
			material.SetTexture("DisplayTexture", generator.getDensityTexture());

			sliceDepth += 0.05f * Time.deltaTime;
			sliceDepth %= 1;
		}
	}
}