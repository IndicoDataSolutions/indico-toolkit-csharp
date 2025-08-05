using Newtonsoft.Json.Linq;
using System.Reflection;

namespace IndicoToolkit.Results;


public static class Utils
{
    /*
    Return the value of type `ValueType` obtained by traversing `json` using `keys`.
    Throw `ResultException` if a key doesn't exist or the value has the wrong type.
    */
    public static ValueType Get<ValueType>(JToken? json, params object[] keys)
    {
        foreach (var key in keys)
        {
            if (json is JObject jsonObject)
            {
                if (key is string keyString && jsonObject.ContainsKey(keyString))
                {
                    json = jsonObject[keyString];
                }
                else
                {
                    var properties = jsonObject.Properties().Select(property => property.Name);
                    throw new ResultException(
                        $"key '{key}' not in object keys ('{string.Join("', '", properties)}')"
                    );
                }
            }
            else if (json is JArray jsonArray)
            {
                if (key is int keyInteger)
                {
                    if (0 <= keyInteger && keyInteger < jsonArray.Count)
                        json = jsonArray[keyInteger];
                    else
                        throw new ResultException(
                            $"index {keyInteger} out of range [0,{jsonArray.Count})"
                        );
                }
                else
                {
                    throw new ResultException($"array can't be indexed with `{key}`");
                }
            }
            else
            {
                throw new ResultException($"{json?.GetType()} can't be traversed");
            }
        }

        try
        {
            ValueType value;

            // Return a reference to JSON types so they can be modified in-place.
            if (typeof(JToken).IsAssignableFrom(typeof(ValueType)))
                value = (ValueType)(object)json;
            // Parse a scalar value (string, int, bool, etc) otherwise.
            else
                value = json.ToObject<ValueType>();

            // Guarantee the returned value isn't null.
            if (value == null)
                throw new ResultException("value is null");

            return value;
        }
        catch
        {
            throw new ResultException(
                $"value `{json}` doesn't have type {typeof(ValueType)}"
            );
        }
    }

    /*
    Check if `json` can be traversed using `keys` to a value of type `ValueType`.
    */
    public static bool Has<ValueType>(JToken? json, params object[] keys)
    {
        foreach (var key in keys)
        {
            if (
                json is JObject jsonObject
                && key is string keyString
                && jsonObject.ContainsKey(keyString)
            )
                json = jsonObject[keyString];
            else if (
                json is JArray jsonArray
                && key is int keyInteger
                && 0 <= keyInteger && keyInteger < jsonArray.Count
            )
                json = jsonArray[keyInteger];
            else
                return false;
        }

        try
        {
            return json.ToObject<ValueType>() != null;
        }
        catch
        {
            return false;
        }
    }

    public static string PrettyPrint(Type type, object instance, params string[] propertyNames)
    {
        var properties = PrettyPrintProperties(type.GetProperties(), instance, propertyNames);
        return $"{type.Name}(\n{properties}\n)";
    }

    private static string PrettyPrintProperties(PropertyInfo[] properties, object instance, params string[] propertyNames)
    {
        return "    " + string.Join(
            ",\n",
            properties
                .Where(property => propertyNames.Contains(property.Name))
                .Select(property => $"{property.Name} = {PrettyPrintProperty(property.GetValue(instance))}")
        ).Replace("\n", "\n    ");
    }

    private static string PrettyPrintProperty(object instance)
    {
        if (instance == null)
            return "null";
        else if (instance is string)
            return PrettyPrintStringProperty(instance as string);
        else if (instance is IEnumerable<object>)
            return PrettyPrintEnumerableProperty(instance as IEnumerable<object>);
        else
            return instance.ToString();
    }

    private static string PrettyPrintStringProperty(string instance)
    {
        return $"\"{instance
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\"", "\\\"")
                .Replace("\\", "\\\\")}\"";
    }

    private static string PrettyPrintEnumerableProperty(IEnumerable<object> instance)
    {
        if (!instance.Any()) return "[]";
        var items = string.Join(
            ",\n",
            instance.Select(item => $"    {item?.ToString().Replace("\n", "\n    ")}")
        );
        return $"[\n{items}\n]";
    }
}
