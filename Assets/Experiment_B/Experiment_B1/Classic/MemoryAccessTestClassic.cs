using System.Diagnostics;
using UnityEngine;

enum DataStructure
{
    AoS,
    SoA
}

public class MemoryAccessTestClassic : MonoBehaviour
{
    [SerializeField] private int numberOfObjects = 1000;
    [SerializeField] private bool readOnly = false;
    [SerializeField] private int totalCount = 10000;
    [SerializeField] private DataStructure dataStructure = DataStructure.AoS;

    [SerializeField] GameObject testObjectAoS;
    TestObjectAoS[] testObjects;
    [SerializeField] GameObject testObjectSoA;
    TestObjectSoA SoATestObject;

    StressTestStatistics testStats = new StressTestStatistics();
    Stopwatch stopWatch = new Stopwatch();
    int currentCount;

    void Start()
    {
        if (dataStructure == DataStructure.AoS)
            StartAoS();
        else
            StartSoA();
    }

    void StartAoS()
    {
        for (int i = 0; i < numberOfObjects; i++)
            Instantiate(testObjectAoS, this.transform).name = "TestObject_" + i;

        testObjects = gameObject.GetComponentsInChildren<TestObjectAoS>();
    }

    void StartSoA()
    {
        SoATestObject = Instantiate(testObjectSoA, this.transform).GetComponent<TestObjectSoA>();

        SoATestObject.Values = new float[numberOfObjects];
    }

    void Update()
    {
        currentCount++; 

        if (dataStructure == DataStructure.AoS)
            UpdateAoS();
        else
            UpdateSoA();

        outputTestData();
    }

    void UpdateAoS()
    {
        if (readOnly)
        {
            stopWatch.Reset();
            stopWatch.Start();

            foreach (TestObjectAoS testObject in testObjects)
            {
                var test = testObject.Value;
            }

            stopWatch.Stop();

            testStats.AddValue((float)stopWatch.Elapsed.TotalMilliseconds);
            return;
        }

        stopWatch.Reset();
        stopWatch.Start();

        foreach (TestObjectAoS testObject in testObjects)
            testObject.Value++;

        stopWatch.Stop();

        testStats.AddValue((float)stopWatch.Elapsed.TotalMilliseconds);
    }

    void UpdateSoA()
    {
        if (readOnly)
        {
            stopWatch.Reset();
            stopWatch.Start();

            foreach (float value in SoATestObject.Values)
            {
                var test = value;
            }

            stopWatch.Stop();

            testStats.AddValue((float)stopWatch.Elapsed.TotalMilliseconds);
            return;
        }

        stopWatch.Reset();
        stopWatch.Start();

        for (int i = 0; i < numberOfObjects; i++)
        {
            SoATestObject.Values[i]++;
        }

        stopWatch.Stop();

        testStats.AddValue((float)stopWatch.Elapsed.TotalMilliseconds);
    }

    void outputTestData()
    {
        if (currentCount <= totalCount && currentCount >= 0)
        {
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
