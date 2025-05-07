using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public static class Utils
{
    // Return the value obtained by traversing `json` using `keys` as indices if that
    // value has type `ValueType`. Throw a `ResultException` otherwise.
    public static ValueType Get<ValueType>(JToken? json, params object[] keys)
    {
        foreach (var key in keys)
        {
            if (key is string strKey && json?.Type == JTokenType.Object)
                json = json[key];
            else if (key is int intKey && json?.Type == JTokenType.Array)
                json = json[key];
            else
                throw new ResultException(
                    $"JSON object `{json}` does not contain key `{key}`"
                );
        }

        if (json == null || json.Type == JTokenType.Null)
        {
            var type = typeof(ValueType);
            var isNullableValueType = Nullable.GetUnderlyingType(type) != null;

            if (isNullableValueType)
                return default;
            else
                throw new ResultException(
                    $"value `{json}` for key `{keys.Last()}` is not of type `{typeof(ValueType)}`"
                );
        }

        try
        {
            return json.ToObject<ValueType>();
        }
        catch (System.Exception)
        {
            throw new ResultException(
                $"value `{json}` for key `{keys.Last()}` is not of type `{typeof(ValueType)}`"
            );
        }
    }

    // Determine if `json` can be traversed using `keys` to a value of type `ValueType`.
    public static bool Has<ValueType>(JToken? json, params object[] keys)
    {
        try
        {
            Get<ValueType>(json, keys);
            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }
}
