using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[AlwaysUpdateSystem]
public class MovementSystemJobs : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;
        float maxZ = StressTestManager.globalManager.HeightRange;

        JobHandle jobHandle = Entities.
            WithoutBurst().
            ForEach((ref Translation trans, ref MovementComponent move) =>
            {
                trans.Value += new float3(0f, 0f, move.MoveSpeed * deltaTime);

                if (trans.Value.z > maxZ)
                    trans.Value.z = 0;
            }).Schedule(inputDeps);

        return jobHandle;
    }
}
