using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public static class Utils
{
    // Return the value of type `ValueType` obtained by traversing `json` using `keys`.
    // Throw `ResultException` if a key doesn't exist or the value has the wrong type.
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

    // Check if `json` can be traversed using `keys` to a value of type `ValueType`.
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
}
