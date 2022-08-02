using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using System.Diagnostics;
using System;

namespace Experiment_B1
{

    public class MemoryAccessTest : MonoBehaviour
    {
        [SerializeField] private int numberOfEntities = 1000;
        [SerializeField] private bool readOnly = false;
        [SerializeField] private int totalCount = 10000;

        SystemBase system;
        StressTestStatistics testStats = new StressTestStatistics(1);
        Stopwatch stopWatch = new Stopwatch();
        int currentCount;

        void Start()
        {
            Type type;
            if (readOnly)
                type = typeof(UpdateSystem_RunReader);
            else
                type = typeof(UpdateSystem_Run);

            system = (SystemBase)Activator.CreateInstance(type);

            var world = World.DefaultGameObjectInjectionWorld;
            var testArchetype = world.EntityManager.CreateArchetype(typeof(TestComponent));
            var entityArray = world.EntityManager.CreateEntity(testArchetype, numberOfEntities, Allocator.Temp);

            entityArray.Dispose();

            world.AddSystem(system);
        }

        void Update()
        {
            ++currentCount;

            stopWatch.Reset();
            stopWatch.Start();

            system.Update();

            stopWatch.Stop();

            if (currentCount <= totalCount && currentCount >= 0)
            {
                testStats.AddValue((float)stopWatch.Elapsed.TotalMilliseconds);

                if ((testStats.Count % 1000) == 0)
                    UnityEngine.Debug.Log($"{testStats.Count} calls. Average read & write time is {testStats.MeanTime}ms +/- {testStats.SigmaDeviation}ms");
            }

            if (testStats.Count == totalCount)
            {
                UnityEngine.Debug.Log($"{testStats.Count} calls. Average read & write time is {testStats.MeanTime}ms +/- {testStats.SigmaDeviation}ms");
                Destroy(this.gameObject);
            }
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [DisableAutoCreation]
    [AlwaysUpdateSystem]
    partial class UpdateSystem_Run : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref TestComponent t) =>
            {
                t.Value++;
            }).Run();
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [DisableAutoCreation]
    [AlwaysUpdateSystem]
    partial class UpdateSystem_RunReader : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((in TestComponent t) => { }).Run();
        }
    }
}