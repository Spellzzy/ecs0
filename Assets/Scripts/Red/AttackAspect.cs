using Unity.Entities;
using Unity.Transforms;

public readonly partial struct AttackAspect : IAspect
{
    public readonly RefRW<Hp> HpComponent;
    public readonly RefRO<RedTag> RedTag;
    public readonly RefRW<LocalTransform> RedTrans;
}
