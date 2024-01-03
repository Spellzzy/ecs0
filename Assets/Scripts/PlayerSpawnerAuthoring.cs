using Unity.Entities;
using UnityEngine;

public class PlayerSpawnerAuthoring : MonoBehaviour
{
    public GameObject PlayerPrefab;
    public int Num;
    public int Tag;
}

public class PlayerSpawnerBaker : Baker<PlayerSpawnerAuthoring>
{
    public override void Bake(PlayerSpawnerAuthoring authoring)
    {
        AddComponent<PlayerSpawnerComponent>(new PlayerSpawnerComponent
        {
            PlayerPrefab = GetEntity(authoring.PlayerPrefab),
            Num = authoring.Num,
            Tag = authoring.Tag
        });
    }
}
