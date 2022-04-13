using UnityEngine;

public class MovementClassic : MonoBehaviour
{
    void Update()
    {
        Vector3 pos = transform.position;
        pos += transform.forward * StressTestManager.globalManager.MovementSpeed * Time.deltaTime;

        if (pos.z > StressTestManager.globalManager.HeightRange)       
            pos.z = 0;

        transform.position = pos;
    }
}
