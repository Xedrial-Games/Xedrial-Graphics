using Unity.Entities;
using UnityEngine;

namespace Xedrial.Graphics
{
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    public class SpriteRendererAuthoring : MonoBehaviour
    {
        private class SpriteRendererBaker : Baker<SpriteRenderer>
        {
            public override void Bake(SpriteRenderer authoring)
            {
                var spriteRenderer = GetComponent<SpriteRenderer>();

                Entity entity = GetEntity(TransformUsageFlags.NonUniformScale);
                AddComponentObject(entity, new SpriteRendererComponent
                {
                    Sprite = spriteRenderer.sprite,
                    Material = spriteRenderer.sharedMaterial,
                    Color = spriteRenderer.color,
                    Mesh = CreateMesh(spriteRenderer.sprite)
                });
            }

            private static Mesh CreateMesh(Sprite sprite)
            {
                var vertices = new Vector3[sprite.vertices.Length];
                Vector2[] uv = sprite.uv;
                int[] triangles = new int[sprite.triangles.Length];

                for (int i = 0; i < sprite.vertices.Length; i++)
                    vertices[i] = sprite.vertices[i];

                for (int i = 0; i < sprite.triangles.Length; i++)
                    triangles[i] = sprite.triangles[i];

                return new Mesh
                {
                    vertices = vertices,
                    uv = uv,
                    triangles = triangles,
                    name = sprite.name
                };
            }
        }
    }
}
