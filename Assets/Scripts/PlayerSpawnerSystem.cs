using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class PlayerSpawnerSystem : SystemBase
{
    private EntityQuery entityQuery;
    private EntityQuery redEntityQuery;
    private bool isFullPlayer;
    private bool isFullRed;
    protected override void OnCreate()
    {
        entityQuery = EntityManager.CreateEntityQuery(typeof(PlayerTag));
        redEntityQuery = EntityManager.CreateEntityQuery(typeof(RedTag));
        isFullPlayer = false;
        isFullRed = false;
    }

    protected override void OnUpdate()
    {
        // int count = entityQuery.CalculateChunkCount();
        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        DoCreatePlayerEntity(entityCommandBuffer);
        DoCreateRedEntity(entityCommandBuffer);
    }

    private void DoCreatePlayerEntity(EntityCommandBuffer entityCommandBuffer)
    {
        if (isFullPlayer)
        {
            return;
        }
        if (!SystemAPI.HasSingleton<PlayerSpawnerComponent>())
        {
            return;
        }
        PlayerSpawnerComponent playerSpawnerComponent = SystemAPI.GetSingleton<PlayerSpawnerComponent>();
        int count = entityQuery.CalculateEntityCount();
        RefRW<RandomComponent> randomComponent = SystemAPI.GetSingletonRW<RandomComponent>();
        
        if (count < playerSpawnerComponent.Num)
        {
            Entity spawnedEntity = entityCommandBuffer.Instantiate(playerSpawnerComponent.PlayerPrefab);
            // 动态设置组件属性数值
            entityCommandBuffer.SetComponent(spawnedEntity, new Speed
            {
                value = randomComponent.ValueRW.Random.NextFloat(1f, 5f)
            });
        }
        else
        {
            isFullPlayer = true;
        }
    }
    
    private void DoCreateRedEntity(EntityCommandBuffer entityCommandBuffer)
    {
        if (isFullRed)
        {
            return;
        }
        if (!SystemAPI.HasSingleton<RedSpawnerComponent>())
        {
            return;
        }
        RedSpawnerComponent redSpawnerComponent = SystemAPI.GetSingleton<RedSpawnerComponent>();
        int count = redEntityQuery.CalculateEntityCount();
        RefRW<RandomComponent> randomComponent = SystemAPI.GetSingletonRW<RandomComponent>();
            
        if (count < redSpawnerComponent.Num)
        {
            Entity spawnedEntity = entityCommandBuffer.Instantiate(redSpawnerComponent.PlayerPrefab);
            // 动态设置组件属性数值
            entityCommandBuffer.SetComponent(spawnedEntity, new Speed
            {
                value = randomComponent.ValueRW.Random.NextFloat(1f, 5f)
            });
            entityCommandBuffer.SetComponent(spawnedEntity, new LocalTransform()
            {
                Position = new float3(50f, 0.171f, 50f),
                Rotation = new quaternion(0f,0f,0f,0f),
                Scale = 1f
            });
        }
        else
        {
            isFullRed = true;
        }
    }
}
