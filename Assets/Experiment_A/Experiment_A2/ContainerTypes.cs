using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerTypes : MonoBehaviour
{
    [Header("Container parameters")]
    public int containerSize = 10;
    
    [Header("Containers")]
    public NativeArray<int> testArrayNative;
    public int[] testArrayClassic;

    public NativeList<int> testListNative;
    public List<int> testListClassic;

    private void Start()
    {
        testArrayNative = new NativeArray<int>(containerSize,Allocator.Temp);
        testArrayClassic = new int[containerSize];

        testListNative = new NativeList<int>(containerSize, Allocator.Temp);
        testListClassic = new List<int>(containerSize);
    }

    private void OnDestroy()
    {
        testArrayNative.Dispose();
        testListNative.Dispose();
    }
}
