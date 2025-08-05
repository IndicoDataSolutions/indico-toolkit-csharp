using IndicoToolkit.Results;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace IndicoToolkit.EtlOutputs;


public record Cell
(
    CellType Type,
    string Text,
    Box Box,
    Range Range,
    ImmutableList<Span> Spans
)
{
    public Span Span => Spans.FirstOrDefault(Span.NULL_SPAN);

    public static CellType CellTypeFromString(string cellType)
    {
        if (cellType == "header")
            return CellType.HEADER;
        else if (cellType == "content")
            return CellType.CONTENT;
        else
            throw new EtlOutputException($"unsupported cell type `{cellType}`");
    }

    public static Cell FromJson(JToken json, int page)
    {
        Utils.Get<JObject>(json, "position")["page_num"] = page;

        foreach (var docOffset in Utils.Get<JArray>(json, "doc_offsets"))
            docOffset["page_num"] = page;

        return new
        (
            CellTypeFromString(Utils.Get<string>(json, "cell_type")),
            Utils.Get<string>(json, "text"),
            Box.FromJson(Utils.Get<JObject>(json, "position")),
            Range.FromJson(json),
            Utils.Get<JArray>(json, "doc_offsets")
                .Select(Span.FromJson)
                .ToImmutableList()
        );
    }

    public override string ToString()
    {
        return Utils.PrettyPrint(
            GetType(),
            this,
            "Type",
            "Text",
            "Box",
            "Range",
            "Spans"
        );
    }
}

