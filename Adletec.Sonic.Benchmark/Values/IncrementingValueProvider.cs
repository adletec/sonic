namespace Adletec.Sonic.Benchmark.Values;

public class IncrementingValueProvider : IValueProvider
{
    private double value = 0;

    public double GetNextValue()
    {
        return value++;
    }

    public List<double> GetValues(long amount)
    {
        var returnList = new List<double>();
        for (var i = 0L; i < amount; i++)
        {
            returnList.Add(GetNextValue());
        }

        return returnList;
    }
}