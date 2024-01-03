using UnityEngine;
using Unity.Entities;
using UnityEditor.Search;

public class RedSpawnerAuthoring : MonoBehaviour
{
    public GameObject PlayerPrefab;
    public int Num;
    public int Tag;
}

public class RedSpawnerBaker : Baker<RedSpawnerAuthoring>
{
    public override void Bake(RedSpawnerAuthoring authoring)
    {
        AddComponent<RedSpawnerComponent>(new RedSpawnerComponent
        {
            PlayerPrefab = GetEntity(authoring.PlayerPrefab),
            Num = authoring.Num,
            Tag = authoring.Tag
        });
    }
}
