using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Xedrial.Utility;

namespace Xedrial.Graphics.Tilemaps.Systems
{
    public partial class TilemapGroupRendererSystem : SystemBase
    {
        private struct RenderData
        {
            public float4x4 Matrix;
            public float4 Uv;
        }

        private Mesh m_Mesh;
        private Camera m_Camera;
        private readonly List<Material> m_Materials = new();
        private static readonly int s_MainTexUV = Shader.PropertyToID("_MainTex_UV");

        [BurstCompile]
        private struct CullAndSortJob : IJobParallelFor
        {
            public float2 Min;
            public float2 Max;

            public NativeQueue<RenderData>.ParallelWriter NativeQueue;
            [ReadOnly] public NativeArray<Tile> NativeArray;

            public void Execute(int index)
            {
                Tile tile = NativeArray[index];
                if (!tile) return;

                float4x4 matrix = tile.Transform;
                if (matrix[3][0] > Max.x || matrix[3][0] < Min.x ||
                    matrix[3][1] > Max.y || matrix[3][1] < Min.y)
                    return;

                var renderData = new RenderData
                {
                    Matrix = tile.Transform,
                    Uv = tile.Uv
                };

                NativeQueue.Enqueue(renderData);
            }
        }

        [BurstCompile]
        private struct FillArraysParallelJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<RenderData> NativeArray;
            [NativeDisableContainerSafetyRestriction] public NativeArray<Matrix4x4> MatrixArray;
            [NativeDisableContainerSafetyRestriction] public NativeArray<Vector4> UvsArray;
            public int StartingIndex;

            public void Execute(int index)
            {
                RenderData renderData = NativeArray[index];
                MatrixArray[StartingIndex + index] = renderData.Matrix;
                UvsArray[StartingIndex + index] = renderData.Uv;
            }
        }

        protected override void OnCreate()
        {
            m_Materials.Clear();
            m_Camera = Camera.main;
            m_Mesh = MeshUtils.CreateMesh(Vector3.zero, 0, Vector3.one, Vector2.zero, Vector2.one);
        }

        protected override void OnStartRunning()
        {
        }

        protected override void OnUpdate()
        {
            Entities.WithoutBurst().ForEach((in TilemapGroupComponent tilemapGroup) =>
            {
                MaterialPropertyBlock block = new();
                NativeQueue<RenderData> nativeQueue = new(Allocator.TempJob);

                const float offset = 1f;
                float3 screenMax = m_Camera.ViewportToWorldPoint(new Vector3(1, 1));
                float3 screenMin = m_Camera.ViewportToWorldPoint(new Vector3(0, 0));
                screenMax += offset;
                screenMin -= offset;

                foreach (KeyValue<int2,Tilemap> tilemap in tilemapGroup.Tilemaps)
                {
                    NativeArray<Tile> nativeArray = tilemap.Value.Grid.GetDataArray(Allocator.TempJob);

                    new CullAndSortJob
                    {
                        Max = screenMax.xy,
                        Min = screenMin.xy,
                        NativeArray = nativeArray,
                        NativeQueue = nativeQueue.AsParallelWriter()
                    }.Schedule(nativeArray.Length, 128).Complete();

                    nativeArray.Dispose();
                }

                if (nativeQueue.Count <= 0)
                {
                    nativeQueue.Dispose();
                    return;
                }

                NativeArray<RenderData> renderData = new(nativeQueue.Count, Allocator.TempJob);

                new NativeQueueToArrayJob<RenderData>
                {
                    NativeQueue = nativeQueue,
                    NativeArray = renderData
                }.Schedule().Complete();

                var uvArray = new NativeArray<Vector4>(renderData.Length, Allocator.TempJob);
                var matrixArray = new NativeArray<Matrix4x4>(renderData.Length, Allocator.TempJob);

                new FillArraysParallelJob
                {
                    NativeArray = renderData,
                    MatrixArray = matrixArray,
                    UvsArray = uvArray,
                    StartingIndex = 0
                }.Schedule(renderData.Length, 128).Complete();

                const int sliceCount = 1023;
                var matrixInstancedArray = new Matrix4x4[sliceCount];
                var uvInstancedArray = new Vector4[sliceCount];

                for (int i = 0; i < renderData.Length; i += sliceCount)
                {
                    int sliceSize = math.min(renderData.Length - i, sliceCount);

                    NativeArray<Matrix4x4>.Copy(matrixArray, i, matrixInstancedArray, 0, sliceSize);
                    NativeArray<Vector4>.Copy(uvArray, i, uvInstancedArray, 0, sliceSize);

                    block.SetVectorArray(s_MainTexUV, uvInstancedArray);

                    UnityEngine.Graphics.DrawMeshInstanced(
                        m_Mesh,
                        0,
                        tilemapGroup.Material,
                        matrixInstancedArray,
                        sliceSize,
                        block,
                        ShadowCastingMode.Off,
                        false,
                        1,
                        m_Camera
                    );
                }

                matrixArray.Dispose();
                uvArray.Dispose();
                renderData.Dispose();
                nativeQueue.Dispose();
            }).Run();
        }


        public int GetMaterialIndex(Material material)
        {
            if (!m_Materials.Contains(material))
                m_Materials.Add(material);

            return m_Materials.IndexOf(material);
        }
    }
}
