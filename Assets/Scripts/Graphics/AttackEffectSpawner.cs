using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class AttackEffectSpawnerAuthoring : MonoBehaviour
{
    public GameObject EffectPrefab;
}

public class AttackEffectSpawnerBaker : Baker<AttackEffectSpawnerAuthoring>
{
    public override void Bake(AttackEffectSpawnerAuthoring authoring)
    {
        AddComponent<AttackEffectSpawnerComponent>(new AttackEffectSpawnerComponent
        {
            EffectPrefab = GetEntity(authoring.EffectPrefab)
        });
    }
}
