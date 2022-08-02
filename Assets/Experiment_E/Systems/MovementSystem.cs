using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[AlwaysUpdateSystem]
[DisableAutoCreation]
public partial class MovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.
            WithoutBurst().
            ForEach((ref Translation trans, ref MovementComponent move) =>
            {
                float deltaTime = Time.DeltaTime;
                trans.Value += new float3(0f, 0f, move.MoveSpeed * deltaTime);

                if (trans.Value.z > StressTestManager.globalManager.HeightRange)
                    trans.Value.z = 0;
            }).Run();
    }
}

