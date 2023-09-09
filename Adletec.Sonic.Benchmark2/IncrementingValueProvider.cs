namespace Adletec.Sonic.Benchmark2;

public class IncrementingValueProvider : IValueProvider
{
    
    private double _value = 0;
    
    public double GetNextValue()
    {
        return _value++;
    }
}