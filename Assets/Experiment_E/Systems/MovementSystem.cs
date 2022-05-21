using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[AlwaysUpdateSystem]
[DisableAutoCreation]
public class MovementSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.
            ForEach((ref Translation trans, ref MovementComponent move) =>
            {
                float deltaTime = Time.DeltaTime;
                trans.Value += new float3(0f, 0f, move.MoveSpeed * deltaTime);

                if (trans.Value.z > StressTestManager.globalManager.HeightRange)
                    trans.Value.z = 0;
            });
    }
}

