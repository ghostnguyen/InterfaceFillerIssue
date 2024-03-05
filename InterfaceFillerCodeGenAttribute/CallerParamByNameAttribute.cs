namespace InterfaceFillerCodeGen;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
public sealed class CallerParamByNameAttribute : Attribute
{
    public CallerParamByNameAttribute()
    {
    }

    public CallerParamByNameAttribute(string paramName)
    {
        ParamName = paramName;
    }

    public string ParamName { get; }
}