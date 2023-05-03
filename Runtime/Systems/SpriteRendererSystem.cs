using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Xedrial.Graphics.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public partial class SpriteRendererSystem : SystemBase
    {
        private static readonly int s_MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int s_Color = Shader.PropertyToID("_Color");

        protected override void OnUpdate()
        {
            Entities
                .WithoutBurst()
                .ForEach((in LocalToWorld matrix, in SpriteRendererComponent spriteRenderer) =>
                {
                    MaterialPropertyBlock materialPropertyBlock = new();

                    materialPropertyBlock.SetTexture(s_MainTex, spriteRenderer.Sprite.texture);
                    materialPropertyBlock.SetColor(s_Color, spriteRenderer.Color);
                    
                    UnityEngine.Graphics.DrawMesh(
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
