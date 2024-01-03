using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct Waypoint : IBufferElementData
{
    public Entity Value;
}

[UpdateAfter(typeof(MovingSystemBase))]
[UpdateBefore(typeof(AttackSystem))]
[BurstCompile]
public partial struct FocusEnemySystem : ISystem
{
    private EntityCommandBuffer entityCommandBuffer;
    
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<AttackEffectSpawnerComponent>())
        {
            return;
        }
        
        EntityQuery redQuery = SystemAPI.QueryBuilder().WithAll<CachePosition, TargetPosition, RedTag>().Build();
        var cachePositions = redQuery.ToComponentDataArray<CachePosition>(Allocator.TempJob);
        
        AttackEffectSpawnerComponent attackEffectSpawnerComponent = SystemAPI.GetSingleton<AttackEffectSpawnerComponent>();
        float time = SystemAPI.Time.DeltaTime;
        entityCommandBuffer =
            SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        FocusEnemyJob focusEnemyJob = new FocusEnemyJob
        {
            Time = time,
            CachePositionArray = cachePositions,
            AttackEffectSpawnerComponent = attackEffectSpawnerComponent,
            EntityCommandBuffer = entityCommandBuffer.AsParallelWriter(),
            entityManager = state.EntityManager
        };
        
        JobHandle focusJobHandle = focusEnemyJob.ScheduleParallel(state.Dependency);
        state.Dependency = focusJobHandle;
        focusJobHandle.Complete();
    }
}

[WithAll(typeof(PlayerTag))]
[BurstCompile]
public partial struct FocusEnemyJob : IJobEntity
{
    public float Time;
    [ReadOnly]
    public NativeArray<CachePosition> CachePositionArray;

    public AttackEffectSpawnerComponent AttackEffectSpawnerComponent;

    public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
    [ReadOnly]
    public EntityManager entityManager;

    [BurstCompile]
    public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity ,RefRW<TargetPosition> playerTargetPosition, RefRW<LocalTransform> transform, RefRW<Hp> hp)
    {
        for (int i = 0; i < CachePositionArray.Length; i++)
        {
            var position = CachePositionArray[i].Position;
            if (math.distancesq(position, transform.ValueRW.Position) < 2.25f)
            {
                if (playerTargetPosition.ValueRW.TestGetRandom)
                {
                    if (playerTargetPosition.ValueRW.AttackEffectTmpEntity == Entity.Null)
                    {
                        // 此时为该寻找行为中 首次进入对方范围
                        // todo 创建effect entity 并绑定 为移除实体时做准备
                        playerTargetPosition.ValueRW.AttackEffectTmpEntity = DoCreateAttackEffect(chunkIndex, EntityCommandBuffer, AttackEffectSpawnerComponent,
                            transform.ValueRW.Position, playerTargetPosition.ValueRW.value, entity);
                    }
                }
                playerTargetPosition.ValueRW.value = new float3(position.x,
                    0f, position.z) ;
                playerTargetPosition.ValueRW.TestGetRandom = false;
                
                hp.ValueRW.Value -= 5f * Time;
                break;
            }
            else
            {
                playerTargetPosition.ValueRW.TestGetRandom = true;
            }
        }
    }

    private Entity DoCreateAttackEffect(int index, EntityCommandBuffer.ParallelWriter entityCommandBuffer, 
        AttackEffectSpawnerComponent attackEffectSpawnerComponent, float3 position, float3 targetPosition, Entity entity)
    {
        Entity spawnedEntity = entityCommandBuffer.Instantiate(index, attackEffectSpawnerComponent.EffectPrefab);
        
        // 动态设置组件属性数值
        entityCommandBuffer.SetComponent(index, spawnedEntity, new LocalTransform()
        {
            Position = new float3(position.x, 0.171f, position.z),
            Rotation = quaternion.EulerXYZ(-90f,0f,0f),
            Scale = 4f
        });

        entityCommandBuffer.SetComponent(index, spawnedEntity, new TargetPosition()
        {
            value = new float3(targetPosition.x, 0f, targetPosition.z),
            TestGetRandom = false,
            AttackEffectTmpEntity = Entity.Null
        });
        
        entityCommandBuffer.SetComponent(index, spawnedEntity, new AttackEffect()
        {
            Target = entity
        });
        return spawnedEntity;
    }

    private void UpdateAttackEffectTargetPosition(int index, EntityCommandBuffer.ParallelWriter entityCommandBuffer,  Entity spawnedEntity, float3 targetPosition)
    {
        // TargetPosition target = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<TargetPosition>(spawnedEntity);
        // target.value = new float3(targetPosition.x, 0f, targetPosition.z);
        
        entityCommandBuffer.SetComponent(index, spawnedEntity, new TargetPosition()
        {
            value = new float3(targetPosition.x, 0f, targetPosition.z),
            TestGetRandom = false,
            AttackEffectTmpEntity = Entity.Null
        });
    }
}
