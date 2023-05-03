using Unity.Entities;
using UnityEngine;

namespace Xedrial.Graphics
{
    public class SpriteRendererComponent : IComponentData
    {
        public Sprite Sprite;
        public Material Material;
        public Color Color;
        public Mesh Mesh;
        
    }
}
