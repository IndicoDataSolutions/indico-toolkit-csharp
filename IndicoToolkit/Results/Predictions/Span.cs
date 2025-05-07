using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public record Span
(
    int Page,
    int Start,
    int End
)
{
    public static Span FromJson(JToken json)
    {
        return new
        (
            Utils.Get<int>(json, "page_num"),
            Utils.Get<int>(json, "start"),
            Utils.Get<int>(json, "end")
        );
    }

    public JObject ToJson()
    {
        return new()
        {
            ["page_num"] = Page,
            ["start"] = Start,
            ["end"] = End,
        };
    }
}
