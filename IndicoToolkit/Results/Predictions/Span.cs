using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public class Span : PrettyPrint
{
    public int Page { get; init; }
    public int Start { get; init; }
    public int End { get; init; }

    public static Span FromJson(JToken json)
    {
        return new Span
        {
            Page = Utils.Get<int>(json, "page_num"),
            Start = Utils.Get<int>(json, "start"),
            End = Utils.Get<int>(json, "end"),
        };
    }

    public JObject ToJson()
    {
        return new JObject
        {
            ["page_num"] = Page,
            ["start"] = Start,
            ["end"] = End,
        };
    }
}
