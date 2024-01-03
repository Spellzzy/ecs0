using UnityEngine;
using Unity.Entities;
public class HpAuthoring : MonoBehaviour
{
    public float Value;
}

public class HpBaker : Baker<HpAuthoring>
{
    public override void Bake(HpAuthoring authoring)
    {
        AddComponent<Hp>(new Hp
        {
            Value = authoring.Value
        });
    }
}
