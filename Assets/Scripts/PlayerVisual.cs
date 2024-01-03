using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerVisual : MonoBehaviour
{
    private Entity _targetEntity;

    private Entity GetRandomEntity()
    {
        EntityQuery playerTagEntityQuery =
            World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(PlayerTag));
        NativeArray<Entity> entityNativeArray =
            playerTagEntityQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
        if (entityNativeArray.Length > 0)
        {
            return entityNativeArray[Random.Range(0, entityNativeArray.Length)];
        }
        return Entity.Null;
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _targetEntity = GetRandomEntity();
        }
        if (_targetEntity != Entity.Null)
        {
            Vector3 followPos =
                World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<LocalTransform>(_targetEntity).Position;
            transform.position = followPos;
            Debug.Log("aaa ---> " + _targetEntity);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(GetRandomEntity());
        }
    }
}
