using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public readonly partial struct MoveToPositionAspect : IAspect
{
    private readonly Entity _entity;

    private readonly RefRW<LocalTransform> _localTransform;
    private readonly RefRO<Speed> _speed;
    private readonly RefRW<TargetPosition> _targetPosition;
    private readonly RefRW<CachePosition> _cachePosition;

    public void Move(float deltaTime)
    {
        float3 dir = math.normalize(_targetPosition.ValueRW.value - _localTransform.ValueRW.Position);
        _localTransform.ValueRW.Position += dir * deltaTime * _speed.ValueRO.value;
        _cachePosition.ValueRW.Position = _localTransform.ValueRW.Position;
    }

    public void TestReachedTargetPosition(RefRW<RandomComponent> randomComponent)
    {
        if (!_targetPosition.ValueRW.TestGetRandom)
            return;
        float _reachedTargetDistance = 0.5f;
        if (math.distancesq(_localTransform.ValueRW.Position, _targetPosition.ValueRW.value) < _reachedTargetDistance * _reachedTargetDistance)
        {
            _targetPosition.ValueRW.value = GetRandomPosition(randomComponent);
        }
    }

    private float3 GetRandomPosition(RefRW<RandomComponent> randomComponent)
    {
        // Unity.Mathematics.Random random = new Random(1);
        return new float3(
            randomComponent.ValueRW.Random.NextFloat(0f, 50f), 0, randomComponent.ValueRW.Random.NextFloat(0f, 50f));
    }

}
