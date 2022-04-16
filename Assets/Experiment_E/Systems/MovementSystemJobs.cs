using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[AlwaysUpdateSystem]
public partial class MovementSystemJobs : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        float maxZ = StressTestManager.globalManager.HeightRange;

        Entities.
            WithoutBurst().
            ForEach((ref Translation trans, ref MovementComponent move) =>
            {
                trans.Value += new float3(0f, 0f, move.MoveSpeed * deltaTime);

                if (trans.Value.z > maxZ)
                    trans.Value.z = 0;
            }).Schedule();
    }
}
