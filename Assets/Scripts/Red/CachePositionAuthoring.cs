using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
public class CachePositionAuthoring : MonoBehaviour
{
    public float3 Position;
}

public class CachePositionBaker : Baker<CachePositionAuthoring>
{
    public override void Bake(CachePositionAuthoring authoring)
    {
        AddComponent<CachePosition>(new CachePosition
        {
            Position = authoring.Position
        });
    }
}
