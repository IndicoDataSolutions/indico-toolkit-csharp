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

    // It's more ergonomic to represent the lack of spans with a special null span
    // object rather than using `null` or throwing an exception. This lets you e.g.
    // sort by the `Span` attribute without having to constantly check for `null`,
    // while still allowing you do a "null check" with `Extraction.Span.IsNull`.
    public static readonly Span NULL_SPAN = new(0, 0, 0);
    public bool IsNull => this == NULL_SPAN;
}
