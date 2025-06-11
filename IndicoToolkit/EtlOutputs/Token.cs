using IndicoToolkit.Results;
using Newtonsoft.Json.Linq;

namespace IndicoToolkit.EtlOutputs;


public record Token
(
    string Text,
    Box Box,
    Span Span
)
{
    public static Token FromJson(JToken json)
    {
        Utils.Get<JObject>(json, "position")["page_num"] = Utils.Get<int>(json, "page_num");
        Utils.Get<JObject>(json, "doc_offset")["page_num"] = Utils.Get<int>(json, "page_num");

        return new
        (
            Utils.Get<string>(json, "text"),
            Box.FromJson(Utils.Get<JObject>(json, "position")),
            Span.FromJson(Utils.Get<JObject>(json, "doc_offset"))
        );
    }
}

