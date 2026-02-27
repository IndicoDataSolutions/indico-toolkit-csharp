using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace IndicoToolkit.EtlOutputs;


public record Cell
(
    CellType Type,
    string Text,
    Box Box,
    Range Range,
    ImmutableArray<Span> Spans
)
{
    public Span Span => Spans.FirstOrDefault(Span.NULL_SPAN);

    /*
    Uniquely identify cells by hashing their type, text, box, and range.

    This is small speedup for `.GroupBy(e => e.Cell)` compared to
    the default GetHashCode implementation.
    */
    public virtual bool Equals(Cell? other) => (
        other != null
        && this.Type == other.Type
        && this.Text == other.Text
        && this.Box == other.Box
        && this.Range == other.Range
    );
    public override int GetHashCode() => HashCode.Combine(Type, Text, Box, Range);

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
                .ToImmutableArray()
        );
    }

    public override string ToString()
    {
        return IsNull
            ? "NULL_CELL"
            : Utils.PrettyPrint(
                GetType(),
                this,
                "Type",
                "Text",
                "Box",
                "Range",
                "Spans"
            );
    }

    /*
    It's more ergonomic to represent the lack of cells with a special null cell object
    rather than using `null` or raising an error. This lets you e.g. sort by the `Cell`
    property without having to constantly check for `null`, while still allowing you do
    a "null check" with `Extraction.Cell.IsNull`.
    */
    public static readonly Cell NULL_CELL = new(CellType.CONTENT, "", Box.NULL_BOX, Range.NULL_RANGE, []);
    public bool IsNull => this == NULL_CELL;
}
