using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateAfter(typeof(AttackSystem))]
public partial struct AttackEffectSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityManager em = state.EntityManager;
        
        foreach ((RefRW<AttackEffect> attackEffect, RefRW<TargetPosition> targetPosition) in SystemAPI.Query<RefRW<AttackEffect>, RefRW<TargetPosition>>())
        {
            Entity targetEntity = attackEffect.ValueRW.Target;
            if (!SystemAPI.Exists(targetEntity))
            {
                attackEffect.ValueRW.Target = Entity.Null;
                continue;
            }

            TargetPosition playerTarget = em.GetComponentData<TargetPosition>(targetEntity);
            if (playerTarget.AttackEffectTmpEntity != Entity.Null)
            {
                // 存在 攻击实体
                if (playerTarget.TestGetRandom)
                {
                    // 人物脱战 随机寻路
                    
                    playerTarget.AttackEffectTmpEntity = Entity.Null;
                    em.SetComponentData(targetEntity, playerTarget);
                    
                    attackEffect.ValueRW.Target = Entity.Null;
                }
                else
                {
                    // 更新目标坐标
                    targetPosition.ValueRW.value = playerTarget.value;
                }
            }
        }
        
        EntityQuery attackEffectEntityQuery =
            World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(AttackEffect));
        NativeArray<Entity> attackEffectNativeArray =
            attackEffectEntityQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
        if (attackEffectNativeArray.Length > 0)
        {
            for (int i = 0; i < attackEffectNativeArray.Length; i++)
            {
                Entity effectEntity = attackEffectNativeArray[i];
                AttackEffect attackEffect = em.GetComponentData<AttackEffect>(effectEntity);
                if (attackEffect.Target == Entity.Null || !SystemAPI.Exists(attackEffect.Target))
                {
                    em.DestroyEntity(effectEntity);
                }
                else
                {
                    if (!SystemAPI.Exists(attackEffect.Target))
                    {
                        attackEffect.Target = Entity.Null;
                        continue;
                    }

                    TargetPosition playerTargetPos = em.GetComponentData<TargetPosition>(attackEffect.Target);
                    playerTargetPos.AttackEffectTmpEntity = effectEntity;
                    em.SetComponentData(attackEffect.Target, playerTargetPos);
                }
            }
        }
    }
}
