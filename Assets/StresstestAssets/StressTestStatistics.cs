using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System;

public struct StressTestStatistics
{
    public float MeanTime { get; private set; }
    public float SigmaDeviation { get; private set; }
    public int Count { get; private set; }

    private List<double> values;

    private double sum;
    private double sumSquared;

    //constructor parameter is required in C# 8.0 and has no meaning
    //parameterless constructor are not supported in C# 8.0
    public StressTestStatistics(float number = 0) 
    {
        MeanTime = 0;
        SigmaDeviation = 0;
        Count = 0;
        sum = 0;
        sumSquared = 0;

        values = new List<double>();
    }

    public void AddValue(double value)
    {
        values.Add(value);

        sum += value;
        sumSquared += value * value;
        Count++;

        MeanTime = (float)(sum / Count);
        var sigmaSq = (float)(sumSquared / Count - (MeanTime * MeanTime));
        var sigma = sigmaSq;
        //if (sigmaSq > Mathf.Epsilon)
        //    sigma = Mathf.Sqrt(sigmaSq);

        SigmaDeviation = sigma;
    }

    public void outputStatistic(string experimentName)
    {
        File.WriteAllText(@"./Assets/StresstestAssets/stresstestResult-" + experimentName + ".txt", JsonConvert.SerializeObject(values));

        if (File.Exists(@"./Assets/StresstestAssets/stresstestResult-" + experimentName + ".txt"))
        {
            File.Delete(@"./Assets/StresstestAssets/stresstestResult-" + experimentName + ".txt");
        }

        using (StreamWriter file = File.CreateText(@"./Assets/StresstestAssets/stresstestResult-" + experimentName + ".txt"))
        {;
            file.Write(JsonConvert.SerializeObject(values, new DecimalFormatConverter()));
            file.Close();
        }
    }
}

public class DecimalFormatConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(decimal));
    }

    public override void WriteJson(JsonWriter writer, object value,
                                   JsonSerializer serializer)
    {
        writer.WriteValue(string.Format("{0:0.00}", value));
    }

    public override bool CanRead
    {
        get { return false; }
    }

    public override object ReadJson(JsonReader reader, System.Type objectType,object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
