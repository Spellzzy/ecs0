using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

[UpdateAfter(typeof(MovingSystemBase))]
[BurstCompile]
public partial struct AttackSystem : ISystem  
{
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<RandomComponent>())
        {
            return;
        }
        float time = SystemAPI.Time.DeltaTime;
        // RefRO<>
        // RefRW<>
        RefRW<RandomComponent> randomComponent = SystemAPI.GetSingletonRW<RandomComponent>();

        EntityQuery playerQuery = SystemAPI.QueryBuilder().WithAll<CachePosition, TargetPosition, PlayerTag>().Build();
        var cachePositions = playerQuery.ToComponentDataArray<CachePosition>(Allocator.TempJob);
        
        
        // todo position sort排序
        NativeArray<float3> cachePositionArray = new NativeArray<float3>(cachePositions.Length, Allocator.TempJob);
        for (int i = 0; i < cachePositionArray.Length; i++)
        {
            cachePositionArray[i] = cachePositions[i].Position;
        }

        SortJob<float3, AxisXComparer> sortJob = cachePositionArray.SortJob(new AxisXComparer {});
        JobHandle sortHandle = sortJob.Schedule(state.Dependency);
        state.Dependency = sortHandle;
        sortHandle.Complete();
        
        AttackJob attackJob = new AttackJob
        {
            Time = time,
            RandomComponent = randomComponent,
            CachePositionArray = cachePositionArray,
        };
        JobHandle attackJobHandle = attackJob.ScheduleParallel(state.Dependency);
        state.Dependency = attackJobHandle;
        attackJobHandle.Complete();
        
        EntityQuery redTagEntityQuery =
            World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(RedTag));
        if (redTagEntityQuery.IsEmpty)
            return;
        
        // SystemBase写法
        // EntityCommandBuffer entityCommandBuffer =
        //     SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
        //         .CreateCommandBuffer(World.Unmanaged);
        // ISystem写法        
        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
        
        // NativeArray<ArchetypeChunk> chunks = redTagEntityQuery.ToArchetypeChunkArray(Allocator.TempJob);
        ComponentTypeHandle<Hp> hpHandle = SystemAPI.GetComponentTypeHandle<Hp>();
        EntityTypeHandle entityHandle = SystemAPI.GetEntityTypeHandle();

        CheckDestroyEntityJob destroyRedJob = new CheckDestroyEntityJob
        {
            entityCommandBuffer = entityCommandBuffer.AsParallelWriter(),
            hpHandle = hpHandle,
            entityHandle = entityHandle
        };
        state.Dependency = destroyRedJob.Schedule(redTagEntityQuery, state.Dependency);
        state.Dependency.Complete();
        
        
        CheckDestroyEntityJob destroyGreenJob = new CheckDestroyEntityJob
        {
            entityCommandBuffer = entityCommandBuffer.AsParallelWriter(),
            hpHandle = hpHandle,
            entityHandle = entityHandle
        };
        state.Dependency = destroyGreenJob.Schedule(playerQuery, state.Dependency);
        state.Dependency.Complete();
    }
}

[WithAll(typeof(RedTag))]
[BurstCompile]
public partial struct AttackJob : IJobEntity
{
    public float Time;
    [NativeDisableUnsafePtrRestriction]
    public RefRW<RandomComponent> RandomComponent;

    [ReadOnly] public NativeArray<float3> CachePositionArray;
    
    [BurstCompile]
    public void Execute(RefRW<LocalTransform> transform, RefRW<Hp> hp)
    {
        // if (CachePositionArray.Length > 0)
        // {
        //     Debug.Log("CachePositionArray[0] ==> " + CachePositionArray[0]);
        //     if (math.distancesq(CachePositionArray[0], transform.ValueRW.Position) > 2.25f)
        //     {
        //         return;
        //     }
        // }
        for (int i = 0; i < CachePositionArray.Length; i++)
        {
            var position = CachePositionArray[i];
            if (math.distancesq(position, transform.ValueRW.Position) < 2.25f)
            {
                hp.ValueRW.Value -= Time * RandomComponent.ValueRW.Random.NextFloat(2f, 50f);
                break;
            }
        }
    }
}

[BurstCompile]
public partial struct CheckDestroyEntityJob : IJobChunk
{
    public EntityCommandBuffer.ParallelWriter entityCommandBuffer;
    [ReadOnly]
    public ComponentTypeHandle<Hp> hpHandle;
    
    public EntityTypeHandle entityHandle;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        NativeArray<Hp> Hps = chunk.GetNativeArray(ref hpHandle);
        NativeArray<Entity> entities = chunk.GetNativeArray(entityHandle);
        for (int j = 0, entityCount = entities.Length; j < entityCount; j++)
        {
            Entity entity = entities[j];
            if (entity != Entity.Null)
            {
                Hp hp = Hps[j];
                if (hp.Value <= 0)
                {
                    entityCommandBuffer.DestroyEntity(unfilteredChunkIndex, entity);
                }
            }
        }
    }
}

public struct AxisXComparer : IComparer<float3>
{
    public int Compare(float3 a, float3 b)
    {
        return (b.x + b.y).CompareTo(a.x + a.y);
    }
}
