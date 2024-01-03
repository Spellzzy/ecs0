using UnityEngine;
using Unity.Entities;

public class RedTagAuthoring : MonoBehaviour
{
}

public class RedTagBaker : Baker<RedTagAuthoring>
{
    public override void Bake(RedTagAuthoring authoring)
    {
        AddComponent<RedTag>(new RedTag());
    }
}