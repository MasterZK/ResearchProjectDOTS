using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Experiment_A1 : MonoBehaviour
{
    void Start()
    {
        UnsafeSizeOf<Vector2>();
        UnsafeSizeOf<float2>();

        UnsafeSizeOf<Vector2Int>();
        UnsafeSizeOf<int2>();

        UnsafeSizeOf<Matrix4x4>();
        UnsafeSizeOf<float4x4>();

        UnsafeSizeOf<Quaternion>();
        UnsafeSizeOf<quaternion>();
    }

    //approach 2 -> success 
    private unsafe void UnsafeSizeOf<T>() where T : unmanaged
    {
        Debug.Log($"Size of {typeof(T)} is {sizeof(T)} bytes");
    }

    //aproach 1 -> failed
    private long SizeOf<T>(T type)
    {
        long size = 0;
        using (Stream s = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(s, type);
            size = s.Length;
        }
        return size;
    }
}
