using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
public class TargetPositionAuthoring : MonoBehaviour
{
    public float3 value;
    public bool TestGetRandom = true;
}

public class TargetPositionBaker : Baker<TargetPositionAuthoring>
{
    public override void Bake(TargetPositionAuthoring authoring)
    {
        AddComponent(new TargetPosition{ value = authoring.value, TestGetRandom = authoring.TestGetRandom});
    }
}
