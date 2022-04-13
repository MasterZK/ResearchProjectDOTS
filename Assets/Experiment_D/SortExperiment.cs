using UnityEngine;

public class SortExperiment : MonoBehaviour
{
    [SerializeField] public int arraySize = 10;
    [Space]
    [SerializeField] public bool ecs = false;
    [SerializeField] public bool multithreaded = false;

    private void Start()
    {
        if (ecs)
        {
            var tempECS = this.gameObject.AddComponent<MergeSorterECS>();

            if (!multithreaded)
                Debug.Log("ECS is only available with mulitthreading!");
            multithreaded = true;
            tempECS.arraySize = this.arraySize;
            tempECS.StartSort();

            return;
        }

        var temp = this.gameObject.AddComponent<MergeSorterClassic>();

        temp.arraySize = this.arraySize;
        temp.multithreaded = this.multithreaded;
        temp.StartSort();
    }
}
