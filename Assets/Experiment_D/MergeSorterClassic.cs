using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class MergeSorterClassic : MonoBehaviour
{
    [SerializeField] public int arraySize;
    [SerializeField] public bool multithreaded = false;
    [SerializeField] public bool outputSortedArray = true;

    int[] toSortArray;

    Thread[] threads;
    int numberOfThreads = 4;

    Stopwatch stopWatch = new Stopwatch();
    int part = 0;

    public void StartSort()
    {
        toSortArray = new int[arraySize];

        for (int i = 0; i < arraySize; i++)
            toSortArray[i] = Random.Range(0, arraySize * 10);

        if (multithreaded)
            createThreads();

        stopWatch.Reset();
        stopWatch.Start();

        if (multithreaded)
            multiThreadedStart();
        else
            mergeSort(0, arraySize - 1);

        stopWatch.Stop();
        SortExperiment.testStats.AddValue(stopWatch.Elapsed.TotalMilliseconds);

        UnityEngine.Debug.Log("Total time to sort: " + stopWatch.Elapsed.TotalMilliseconds + " ms");
        UnityEngine.Debug.Log("Total time to sort: " + stopWatch.Elapsed.TotalMilliseconds * 1000000 + " ns");
        var threaded = multithreaded ? "multithreadeing!" : "no multithreading!";
        UnityEngine.Debug.Log("This merge sort was conducted in classic unity and uses " + threaded);

        if (!outputSortedArray)
        {
            Destroy(this);
            return;
        }

        UnityEngine.Debug.Log("Sorted Array:  ");
        for (int i = 0; i < arraySize; i++)
            UnityEngine.Debug.Log(toSortArray[i] + " ");
    }

    void createThreads()
    {
        threads = new Thread[numberOfThreads];

        for (int i = 0; i < numberOfThreads; i++)
            threads[i] = new Thread(mergeSortMultiThread);

    }

    void multiThreadedStart()
    {
        for (int i = 0; i < numberOfThreads; i++)
            threads[i].Start();

        for (int i = 0; i < numberOfThreads; i++)
            threads[i].Join();

        merge(0, (arraySize / 2 - 1) / 2, arraySize / 2 - 1);
        merge(arraySize / 2, arraySize / 2 + ((arraySize - 1) - (arraySize / 2)) / 2, arraySize - 1);
        merge(0, (arraySize - 1) / 2, arraySize - 1);
    }

    void mergeSort(int low, int high)
    {
        int mid = low + (high - low) / 2;

        if (low < high)
        {
            mergeSort(low, mid);
            mergeSort(mid + 1, high);

            merge(low, mid, high);
        }
    }

    void mergeSortMultiThread()
    {
        int currentThread = part++;

        int low = currentThread * (arraySize / 4);
        int high = (currentThread + 1) * (arraySize / 4) - 1;
        int mid = low + (high - low) / 2;

        if (low < high)
        {
            mergeSort(low, mid);
            mergeSort(mid + 1, high);

            merge(low, mid, high);
        }
    }

    // merge function for merging two parts
    void merge(int low, int mid, int high)
    {
        int[] left = new int[mid - low + 1];
        int[] right = new int[high - mid];

        int leftSize = mid - low + 1;
        int rightSize = high - mid;

        for (int i = 0; i < leftSize; i++)
            left[i] = toSortArray[i + low];
        for (int j = 0; j < rightSize; j++)
            right[j] = toSortArray[j + mid + 1];

        int arrayIndex = low;
        int leftCount = 0;
        int rightCount = 0;

        // sort values and write back to main array
        while (leftCount < leftSize && rightCount < rightSize)
        {
            if (left[leftCount] <= right[rightCount])
                toSortArray[arrayIndex++] = left[leftCount++];
            else
                toSortArray[arrayIndex++] = right[rightCount++];
        }

        // insert remaining values from left
        while (leftCount < leftSize)
            toSortArray[arrayIndex++] = left[leftCount++];

        // insert remaining values from right
        while (rightCount < rightSize)
            toSortArray[arrayIndex++] = right[rightCount++];

    }
}
