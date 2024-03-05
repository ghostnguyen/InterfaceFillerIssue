namespace InterfaceFillerCodeGen;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
public sealed class CallerParamByIndexAttribute : Attribute
{
    public CallerParamByIndexAttribute()
    {
    }

    public CallerParamByIndexAttribute(int value, bool fromEnd = false)
    {
        Value = value;
        FromEnd = fromEnd;
    }

    public int Value { get; }
    public bool FromEnd { get; }
}