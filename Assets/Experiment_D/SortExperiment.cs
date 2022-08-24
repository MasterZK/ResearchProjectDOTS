using UnityEngine;

public class SortExperiment : MonoBehaviour
{
    [SerializeField] public int arraySize = 10;
    [SerializeField] public int numberOfRuns = 100;
    [Space]
    [SerializeField] public bool ecs = false;
    [SerializeField] public bool multithreaded = false;
    [SerializeField] public bool outputResult = false;

    private int currentRuns;

    public static StressTestStatistics testStats;

    private void Start()
    {
        testStats = new StressTestStatistics(1);
    }

    private void Update()
    {
        if (currentRuns >= numberOfRuns)
            return;

        if (!(gameObject.TryGetComponent<MergeSorterECS>(out var a) == false &&
            gameObject.TryGetComponent<MergeSorterClassic>(out var b) == false))
            return;

        currentRuns++;

        if (ecs)
        {
            var tempECS = this.gameObject.AddComponent<MergeSorterECS>();

            if (!multithreaded)
                Debug.Log("ECS is only available with mulitthreading!");
            multithreaded = true;
            tempECS.arraySize = this.arraySize;
            tempECS.outputSortedArray = this.outputResult;
            tempECS.StartSort();

            testStats.outputStatistic("D");
            return;
        }

        var temp = this.gameObject.AddComponent<MergeSorterClassic>();

        temp.arraySize = this.arraySize;
        temp.multithreaded = this.multithreaded;
        temp.outputSortedArray = this.outputResult;
        temp.StartSort();

        testStats.outputStatistic("D");
    }
}
