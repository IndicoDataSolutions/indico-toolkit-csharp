using System.Reflection;

namespace IndicoToolkit.Results;


[AttributeUsage(AttributeTargets.Property)]
public class NoPrintAttribute : Attribute { }


public class PrettyPrint
{
    public override string ToString()
    {
        var properties = GetType().GetProperties()
            .Where(prop => prop.GetCustomAttribute<NoPrintAttribute>() == null)
            .Select(prop => $"    {prop.Name} = {prop.GetValue(this)?.ToString()?.Replace("\n", "\n    ") ?? "null"}");

        return $"{GetType().Name} {{\n{string.Join(",\n", properties)}\n}}";
    }
}


public class PrettyPrintList<T> : List<T>
{
    public PrettyPrintList() : base() { }
    public PrettyPrintList(IEnumerable<T> collection) : base(collection) { }

    public override string ToString()
    {
        var items = this.Select(item => $"    {item?.ToString()?.Replace("\n", "\n    ") ?? "null"}");
        return $"{GetType().Name} {{\n{string.Join(",\n", items)}\n}}";
    }
}
