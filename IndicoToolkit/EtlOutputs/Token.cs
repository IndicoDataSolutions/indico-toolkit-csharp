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

    public override string ToString()
    {
        return IsNull
            ? "NULL_TOKEN"
            : Utils.PrettyPrint(
                GetType(),
                this,
                "Text",
                "Box",
                "Span"
            );
    }

    /*
    It's more ergonomic to represent the lack of tokens with a special null token
    object rather than using `null` or throwing an exception. This lets you e.g.
    sort by the `Token` attribute without having to constantly check for `null`,
    while still allowing you do a "null check" with `Extraction.Token.IsNull`.
    */
    public static readonly Token NULL_TOKEN = new("", Box.NULL_BOX, Span.NULL_SPAN);
    public bool IsNull => this == NULL_TOKEN;
}
