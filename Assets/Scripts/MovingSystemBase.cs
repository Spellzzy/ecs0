using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

[BurstCompile]
public partial struct MovingSystemBase : ISystem
{
    private float time;
    private RefRW<RandomComponent> randomComponent;
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<RandomComponent>())
        {
            return;
        }
        time = SystemAPI.Time.DeltaTime;
        randomComponent = SystemAPI.GetSingletonRW<RandomComponent>();
        
        MoveJob job = new MoveJob
        {
            DeltaTime = time,
        };
        JobHandle jobHandle = job.ScheduleParallel(state.Dependency);
        jobHandle.Complete();

        TestReachedTargetPositionJob newPosJob = new TestReachedTargetPositionJob
        {
            RandomComponent = randomComponent
        };
        JobHandle newPosJobHandle = newPosJob.ScheduleParallel(state.Dependency);
        newPosJobHandle.Complete();
        // newPosJob.Run(); 主线程运行
    }
}

public partial struct MoveJob : IJobEntity
{
    public float DeltaTime;
    public void Execute(MoveToPositionAspect moveToPositionAspect)
    {
        moveToPositionAspect.Move(DeltaTime);
    }
}

public partial struct TestReachedTargetPositionJob : IJobEntity
{
    [NativeDisableUnsafePtrRestriction]
    public RefRW<RandomComponent> RandomComponent; 
    public void Execute(MoveToPositionAspect moveToPositionAspect)
    {
        moveToPositionAspect.TestReachedTargetPosition(RandomComponent);
    }
}