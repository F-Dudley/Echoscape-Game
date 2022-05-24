using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace TerrainGeneration
{

    [BurstCompile]
    public struct MarchJob : IJobParallelFor
    {
        public void Execute(int index)
        {

        }
    }
    
}