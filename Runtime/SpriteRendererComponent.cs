using System;

using UnityEngine;
using Unity.Entities;

namespace Xedrial.Rendering
{
    [Serializable]
    public struct SpriteRendererComponent : ISharedComponentData, IEquatable<SpriteRendererComponent>
    {
        public Sprite Sprite;
        public Material Material;
        public Color Color;
        public Mesh Mesh;

        public static Mesh CreateMesh(Sprite sprite)
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

        public bool Equals(SpriteRendererComponent other)
        {
            return Equals(Sprite, other.Sprite) && Equals(Material, other.Material) && Color.Equals(other.Color);
        }

        public override bool Equals(object obj)
        {
            return obj is SpriteRendererComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Sprite, Material, Color);
        }
    }
}
