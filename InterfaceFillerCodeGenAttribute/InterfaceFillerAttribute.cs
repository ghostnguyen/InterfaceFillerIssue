namespace InterfaceFillerCodeGen;

[AttributeUsage(AttributeTargets.Field)]
public class InterfaceFillerAttribute : Attribute
{
    public InterfaceFillerAttribute()
    {
    }

    public InterfaceFillerAttribute(params string[] wrappers)
    {
        WrapperNames = wrappers;
    }

    public string[] WrapperNames { get; } = Array.Empty<string>();
}