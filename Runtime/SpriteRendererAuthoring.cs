using System;
using Unity.Entities;
using UnityEngine;

namespace Xedrial.Rendering
{
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    public class SpriteRendererAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] private Mesh m_Mesh;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem _)
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            
            dstManager.AddSharedComponentData(entity, new SpriteRendererComponent
            {
                Sprite = spriteRenderer.sprite,
                Material = spriteRenderer.sharedMaterial,
                Color = spriteRenderer.color,
                Mesh = m_Mesh == null ? SpriteRendererComponent.CreateMesh(spriteRenderer.sprite) : m_Mesh
            });

            if (dstManager.HasComponent<SpriteRenderer>(entity))
                dstManager.RemoveComponent<SpriteRenderer>(entity);
        }
    }
}
