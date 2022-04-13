using UnityEngine;

public struct StressTestStatistics
{
    public float MeanTime { get; private set; }
    public float SigmaDeviation { get; private set; }
    public int Count { get; private set; }

    private double sum;
    private double sumSquared;

    public void AddValue(float value)
    {
        sum += value;
        sumSquared += value * value;
        Count++;

        MeanTime = (float)(sum / Count);
        var sigmaSq = (float)(sumSquared / Count - (MeanTime * MeanTime));
        var sigma = sigmaSq;
        if (sigmaSq > Mathf.Epsilon)
            sigma = Mathf.Sqrt(sigmaSq);

        SigmaDeviation = sigma;
    }
}
