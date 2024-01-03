using Unity.Entities;

public struct PlayerSpawnerComponent : IComponentData
{
    public Entity PlayerPrefab;
    public int Num;
    public int Tag;
}

public struct RedSpawnerComponent : IComponentData
{
    public Entity PlayerPrefab;
    public int Num;
    public int Tag;
}

public struct AttackEffectSpawnerComponent : IComponentData
{
    public Entity EffectPrefab;
}