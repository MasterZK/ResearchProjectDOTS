using System.Diagnostics;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;

public struct SortRequest
{
    public NativeArray<int> toSort;
}

public struct MergeRequest
{
    public NativeArray<int> mergeSectionOne;
    public NativeArray<int> mergeSectionTwo;
}

public class MergeSorterECS : MonoBehaviour
{
    [SerializeField] public int arraySize;
    [SerializeField] bool multithreaded = true;
    [SerializeField] public bool outputSortedArray = true;

    NativeArray<int> toSortArray;

    NativeArray<JobHandle> jobHandles;

    MergeSortJob[] sortJobs;
    SortRequest[] sortRequests;

    MergeJob[] mergeJobs;
    MergeRequest[] mergeRequests;

    int numberOfJobs = 4;

    Stopwatch stopWatch = new Stopwatch();

    public void StartSort()
    {
        toSortArray = new NativeArray<int>(arraySize, Allocator.Persistent);

        for (int i = 0; i < arraySize; i++)
            toSortArray[i] = UnityEngine.Random.Range(0, arraySize * 10);

        createJobs();

        stopWatch.Reset();
        stopWatch.Start();

        completeRequests();

        stopWatch.Stop();
        SortExperiment.testStats.AddValue(stopWatch.Elapsed.TotalMilliseconds);

        UnityEngine.Debug.Log("Total time to sort: " + stopWatch.Elapsed.TotalMilliseconds + " ms");
        UnityEngine.Debug.Log("Total time to sort: " + stopWatch.Elapsed.TotalMilliseconds * 1000000 + " ns");
        UnityEngine.Debug.Log("This merge sort was conducted in ECS and uses multithreadeing!");

        disposeAfterSort();

        if (!outputSortedArray)
        {
            Destroy(this);
            return;
        }

        UnityEngine.Debug.Log("Sorted Array:  ");
        for (int i = 0; i < arraySize; i++)
            UnityEngine.Debug.Log(toSortArray[i] + " ");
    }

    void createJobs()
    {
        jobHandles = new NativeArray<JobHandle>(numberOfJobs + 3, Allocator.Temp);
        sortRequests = new SortRequest[numberOfJobs];
        sortJobs = new MergeSortJob[numberOfJobs];

        for (int i = 0; i < numberOfJobs; i++)
        {
            int low = i * (arraySize / 4);
            int high = (i + 1) * (arraySize / 4) - 1;

            sortRequests[i] = new SortRequest();

            sortRequests[i].toSort = new NativeArray<int>(high - low + 1, Allocator.TempJob);
            int k = 0;
            for (int j = low; j < high; j++)
                sortRequests[i].toSort[k++] = toSortArray[j];

            sortJobs[i] = new MergeSortJob()
            {
                sortRequest = sortRequests[i],
                result = new NativeArray<int>(high - low + 1, Allocator.TempJob),
            };
            jobHandles[i] = sortJobs[i].Schedule();
        }


        mergeRequests = new MergeRequest[3];
        mergeJobs = new MergeJob[3];

        var length = createMergeRequests(0, 0, 1);
        mergeJobs[0] = new MergeJob()
        {
            mergeRequest = mergeRequests[0],
            result = new NativeArray<int>(length, Allocator.TempJob),
        };
        jobHandles[4] = mergeJobs[0].Schedule(JobHandle.CombineDependencies(jobHandles[0], jobHandles[1]));

        length = createMergeRequests(1, 2, 3);
        mergeJobs[1] = new MergeJob()
        {
            mergeRequest = mergeRequests[1],
            result = new NativeArray<int>(length, Allocator.TempJob),
        };
        jobHandles[5] = mergeJobs[1].Schedule(JobHandle.CombineDependencies(jobHandles[2], jobHandles[3]));


        mergeRequests[2] = new MergeRequest()
        {
            mergeSectionOne = mergeJobs[0].result,
            mergeSectionTwo = mergeJobs[1].result
        };
        mergeJobs[2] = new MergeJob()
        {
            mergeRequest = mergeRequests[2],
            result = new NativeArray<int>(mergeJobs[0].result.Length + mergeJobs[1].result.Length, Allocator.TempJob),
        };
        jobHandles[6] = mergeJobs[2].Schedule(JobHandle.CombineDependencies(jobHandles[4], jobHandles[5]));
    }

    private int createMergeRequests(int mergeRequestIndex, int resultOne, int resultTwo)
    {
        mergeRequests[mergeRequestIndex] = new MergeRequest()
        {
            mergeSectionOne = sortJobs[resultOne].result,
            mergeSectionTwo = sortJobs[resultTwo].result
        };
        return sortJobs[resultOne].result.Length + sortJobs[resultTwo].result.Length;
    }

    private void completeRequests()
    {
        JobHandle.CompleteAll(jobHandles);

        toSortArray.CopyFrom(mergeJobs[2].result);
    }

    private void disposeAfterSort()
    {
        if (jobHandles.IsCreated)
            jobHandles.Dispose();

        foreach (MergeSortJob job in sortJobs)
        {
            if (job.result.IsCreated)
                job.result.Dispose();
        }

        foreach (SortRequest request in sortRequests)
        {
            if (request.toSort.IsCreated)
                request.toSort.Dispose();
        }

        foreach (MergeJob job in mergeJobs)
        {
            if (job.result.IsCreated)
                job.result.Dispose();
        }
    }

    private void OnDestroy()
    {
        if (toSortArray.IsCreated)
            toSortArray.Dispose();
    }
}

struct MergeSortJob : IJob
{
    public SortRequest sortRequest;
    public NativeArray<int> result;

    public void Execute()
    {
        mergeSort(0, sortRequest.toSort.Length - 1);

        result.CopyFrom(sortRequest.toSort);
    }

    void mergeSort(int low, int high)
    {
        int mid = low + ((high - low) / 2);

        if (low < high)
        {
            mergeSort(low, mid);
            mergeSort(mid + 1, high);

            merge(low, mid, high);
        }
    }

    void merge(int low, int mid, int high)
    {
        int[] left = new int[mid - low + 1];
        int[] right = new int[high - mid];

        int leftSize = mid - low + 1;
        int rightSize = high - mid;

        for (int i = 0; i < leftSize; i++)
            left[i] = sortRequest.toSort[i + low];
        for (int j = 0; j < rightSize; j++)
            right[j] = sortRequest.toSort[j + mid + 1];

        int arrayIndex = low;
        int leftCount = 0;
        int rightCount = 0;

        while (leftCount < leftSize && rightCount < rightSize)
        {
            if (left[leftCount] <= right[rightCount])
                sortRequest.toSort[arrayIndex++] = left[leftCount++];
            else
                sortRequest.toSort[arrayIndex++] = right[rightCount++];
        }

        while (leftCount < leftSize)
            sortRequest.toSort[arrayIndex++] = left[leftCount++];

        while (rightCount < rightSize)
            sortRequest.toSort[arrayIndex++] = right[rightCount++];
    }
}

struct MergeJob : IJob
{
    public MergeRequest mergeRequest;
    public NativeArray<int> result;

    public void Execute()
    {
        merge();
    }

    void merge()
    {
        int[] left = mergeRequest.mergeSectionOne.ToArray();
        int[] right = mergeRequest.mergeSectionTwo.ToArray();

        int leftSize = left.Length;
        int rightSize = right.Length;

        int arrayIndex = 0;
        int leftCount = 0;
        int rightCount = 0;

        while (leftCount < leftSize && rightCount < rightSize)
        {
            if (left[leftCount] <= right[rightCount])
                result[arrayIndex++] = left[leftCount++];
            else
                result[arrayIndex++] = right[rightCount++];
        }

        while (leftCount < leftSize)
            result[arrayIndex++] = left[leftCount++];

        while (rightCount < rightSize)
            result[arrayIndex++] = right[rightCount++];
    }
}
