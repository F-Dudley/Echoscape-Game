using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

namespace TerrainGeneration
{
    using ComputeStructs;

    [System.Serializable]
    [BurstCompile]
    public struct ChunkAttributes
    {
        public int3 id;
        public float3 centre;
        public float size;

        public int3 GetChunkCoord(int numCubesPerAxis) => id * numCubesPerAxis;
    }

    [System.Serializable]
    [BurstCompile]
    public struct Chunk
    {
        public ChunkAttributes attributes;
        public GameObject meshHolder;

        [Header("Mesh")]
        [SerializeField] private Mesh mesh;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshCollider meshCollider;
        [SerializeField] private VisualEffect meshVFX;

        public Chunk(int3 _id, float3 _centre, float _size, Material terrainMat, VisualEffectAsset terrainVFX, GameObject holder)
        {
            attributes = new ChunkAttributes
            {
                id = _id,
                centre = _centre,
                size = _size,
            };

            meshHolder = holder;

            // Create Mesh Objects
            mesh = new Mesh();
            mesh.name = $"Chunk ({_id.x}-{_id.y}-{_id.z})'s Mesh";
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            meshFilter = meshHolder.AddComponent<MeshFilter>();
            meshRenderer = meshHolder.AddComponent<MeshRenderer>();
            meshRenderer.material = terrainMat;
            meshVFX = meshHolder.AddComponent<VisualEffect>();
            meshVFX.visualEffectAsset = terrainVFX;

            meshFilter.mesh = mesh;
            meshCollider = meshRenderer.gameObject.AddComponent<MeshCollider>();
        }

        public void CreateMesh(Triangle[] triangles, bool useFlatShading)
        {
            Dictionary<int2, int> vertexIndexMap = new Dictionary<int2, int>();
            List<Vector3> processedVertices = new List<Vector3>();
            List<Vector3> processedNormals = new List<Vector3>();
            List<int> processedTriangles = new List<int>();

            int vertexNum = triangles.Length * 3;
            int triIndex = 0;
            for (int i = 0; i < triangles.Length; i++)
            {
                Triangle triangle = triangles[i];

                Vertex v0 = triangle.vertexA;
                Vertex v1 = triangle.vertexB;
                Vertex v2 = triangle.vertexC;

                // v0
                int sharedVertexIndex;
                if (!useFlatShading && vertexIndexMap.TryGetValue(v0.id, out sharedVertexIndex))
                {
                    processedTriangles.Add(sharedVertexIndex);
                }
                else
                {
                    if (!useFlatShading)
                    {
                        vertexIndexMap.Add(v0.id, triIndex);
                    }
                    processedVertices.Add(v0.position);
                    processedNormals.Add(v0.normal);
                    processedTriangles.Add(triIndex);
                    triIndex++;
                }

                // v1
                if (!useFlatShading && vertexIndexMap.TryGetValue(v1.id, out sharedVertexIndex))
                {
                    processedTriangles.Add(sharedVertexIndex);
                }
                else
                {
                    if (!useFlatShading)
                    {
                        vertexIndexMap.Add(v1.id, triIndex);
                    }
                    processedVertices.Add(v1.position);
                    processedNormals.Add(v1.normal);
                    processedTriangles.Add(triIndex);
                    triIndex++;
                }

                // v2
                if (!useFlatShading && vertexIndexMap.TryGetValue(v2.id, out sharedVertexIndex))
                {
                    processedTriangles.Add(sharedVertexIndex);
                }
                else
                {
                    if (!useFlatShading)
                    {
                        vertexIndexMap.Add(v2.id, triIndex);
                    }
                    processedVertices.Add(v2.position);
                    processedNormals.Add(v2.normal);
                    processedTriangles.Add(triIndex);
                    triIndex++;
                }
            }

            mesh.Clear();
            mesh.SetVertices(processedVertices);
            mesh.SetTriangles(processedTriangles, 0, true);

            if (useFlatShading) mesh.RecalculateNormals();
            else mesh.SetNormals(processedNormals);

            meshCollider.sharedMesh = mesh;
        }

        public void CreateMesh(NativeList<Triangle> triangles, bool useFlatShading)
        {
            if (triangles.Length == 0)
            {
                Debug.Log("No Triangles Submitted", this.meshHolder);
                return;
            }

            NativeHashMap<int2, int> vertexIndexMap = new NativeHashMap<int2, int>(triangles.Length * 3, Allocator.TempJob);
            NativeList<float3> processedVertices = new NativeList<float3>(triangles.Length * 3, Allocator.TempJob);
            NativeList<float3> processedNormals = new NativeList<float3>(triangles.Length * 3, Allocator.TempJob);
            NativeList<int> processedTriangles = new NativeList<int>(triangles.Length * 3, Allocator.TempJob);

            FilterChunkData triangleFilterJob = new FilterChunkData(triangles,
                                                                              processedVertices, processedNormals, processedTriangles,
                                                                              vertexIndexMap, useFlatShading);
            JobHandle handler = triangleFilterJob.Schedule();
            handler.Complete();

            //Debug.Log($"Triangle Amount: {processedTriangles.Length}");
            //Debug.Log($"Vertex Amount: {processedVertices.Length}");

            mesh.Clear();
            mesh.SetVertices(processedVertices.AsArray());
            mesh.SetTriangles(processedTriangles.ToArray(), 0, true);

            if (useFlatShading) mesh.RecalculateNormals();
            else mesh.SetNormals(processedNormals.AsArray());

            meshCollider.sharedMesh = mesh;

            vertexIndexMap.Dispose(handler);
            processedVertices.Dispose(handler);
            processedNormals.Dispose(handler);
            processedTriangles.Dispose(handler);
        }
    }
}