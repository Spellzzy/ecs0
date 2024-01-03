using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
public struct TargetPosition : IComponentData
{
    public float3 value;
    public bool TestGetRandom;
    public Entity AttackEffectTmpEntity;
}
