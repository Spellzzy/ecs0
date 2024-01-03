using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class AttackEffectAuthoring : MonoBehaviour
{
    
}

public class AttackEffectBaker : Baker<AttackEffectAuthoring>
{
    public override void Bake(AttackEffectAuthoring authoring)
    {
        AddComponent<AttackEffect>(new AttackEffect());
    }
}

