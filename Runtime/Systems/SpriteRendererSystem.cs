using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Xedrial.Rendering.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class SpriteRendererSystem : SystemBase
    {
        private static readonly int s_MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int s_Color = Shader.PropertyToID("_Color");

        private Camera m_Camera;
        
        protected override void OnCreate() => m_Camera = Camera.main;

        protected override void OnUpdate()
        {
            Entities.WithoutBurst().WithNone<Prefab>().ForEach((in LocalToWorld matrix, in SpriteRendererComponent spriteRenderer) =>
            {
                MaterialPropertyBlock materialPropertyBlock = new();

                materialPropertyBlock.SetTexture(s_MainTex, spriteRenderer.Sprite.texture);
                materialPropertyBlock.SetColor(s_Color, spriteRenderer.Color);
                Graphics.DrawMesh(
                    spriteRenderer.Mesh,
                    matrix.Value,
                    spriteRenderer.Material,
                    0,
                    null,
                    0,
                    materialPropertyBlock
                );
            }).Run();
        }
    }
}
