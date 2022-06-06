using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
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

        public Chunk(int3 _id, float3 _centre, float _size, Material terrainMat, GameObject holder)
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

            meshFilter.mesh = mesh;
            meshCollider = meshRenderer.gameObject.AddComponent<MeshCollider>();
        }

        public void CreateMesh(ListBuffer<ComputeStructs.Triangle> triangles, bool useFlatShading)
        {
            Dictionary<int2, int> vertexMap = new Dictionary<int2, int>();
            List<Vector3> verticies = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> finalTris = new List<int>();

            int triangleIndex = 0;

            Debug.Log($"Triangle Capacity: {triangles.Capacity} / Triangles Length: {triangles.Count}");
            for (int i = 0; i < triangles.Capacity; i++)
            {
                AddVertex(triangles[i].vertexA, ref verticies, ref normals, ref finalTris, ref vertexMap, triangleIndex, useFlatShading);
                triangleIndex++;

                AddVertex(triangles[i].vertexB, ref verticies, ref normals, ref finalTris, ref vertexMap, triangleIndex, useFlatShading);
                triangleIndex++;

                AddVertex(triangles[i].vertexC, ref verticies, ref normals, ref finalTris, ref vertexMap, triangleIndex, useFlatShading);
                triangleIndex++;
            }

            Debug.Log("Processed Verticies Count: " + verticies.Count);

            meshCollider.sharedMesh = null;

            mesh.Clear();
            mesh.SetVertices(verticies);
            mesh.SetTriangles(finalTris, 0, true);

            if (useFlatShading) mesh.RecalculateNormals();
            else mesh.SetNormals(normals);

            meshCollider.sharedMesh = mesh;

            Debug.Log("Settings Mesh Data");
        }

        public void CreateMesh(Triangle[] triangles, bool useFlatShading)
        {
            Dictionary<int2, int> vertexMap = new Dictionary<int2, int>();
            List<Vector3> verticies = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> finalTris = new List<int>();

            int triangleIndex = 0;

            Debug.Log($"Vertex Count: {triangles.Length * 3} / Triangles Length: {triangles.Length}");
            for (int i = 0; i < triangles.Length; i++)
            {
                AddVertex(triangles[i].vertexA, ref verticies, ref normals, ref finalTris, ref vertexMap, triangleIndex, useFlatShading);
                triangleIndex++;

                AddVertex(triangles[i].vertexB, ref verticies, ref normals, ref finalTris, ref vertexMap, triangleIndex, useFlatShading);
                triangleIndex++;

                AddVertex(triangles[i].vertexC, ref verticies, ref normals, ref finalTris, ref vertexMap, triangleIndex, useFlatShading);
                triangleIndex++;
            }

            meshCollider.sharedMesh = null;

            mesh.Clear();
            mesh.SetVertices(verticies);
            mesh.SetTriangles(finalTris, 0, true);

            if (useFlatShading) mesh.RecalculateNormals();
            else mesh.SetNormals(normals);

            meshCollider.sharedMesh = mesh;
        }

        private void AddVertex(Vertex vertex, ref List<Vector3> vertexList, ref List<Vector3> normalList, ref List<int> triangleList, 
                               ref Dictionary<int2, int> vertexDupeMap, 
                               int triangleIndex, bool useflatShade)
        {
            if (!useflatShade && vertexDupeMap.TryGetValue(vertex.id, out int sharedVertexIndex))
            {
                triangleList.Add(sharedVertexIndex);
            }
            else
            {
                if (!useflatShade)
                {
                    vertexDupeMap.Add(vertex.id, triangleIndex);
                }

                vertexList.Add(vertex.position);
                normalList.Add(vertex.normal);
                triangleList.Add(triangleIndex);
            }
        }
    }
}