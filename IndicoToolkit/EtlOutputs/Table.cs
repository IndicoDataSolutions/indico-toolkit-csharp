using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace IndicoToolkit.EtlOutputs;


public record Table
(
    Box Box,
    ImmutableArray<Span> Spans,
    ImmutableArray<Cell> Cells,
    ImmutableArray<ImmutableArray<Cell>> Rows,
    ImmutableArray<ImmutableArray<Cell>> Columns
)
{
    public Span Span => Spans.FirstOrDefault(Span.NULL_SPAN);

    /*
    Uniquely identify tables by hashing their bounding box and spans.

    This is an order of magnitude speedup for `.GroupBy(e => e.Table)`
    compared to the default GetHashCode implementation.
    */
    public override int GetHashCode() => HashCode.Combine(Box, Spans);

    public static Table FromJson(JToken json)
    {
        var page = Utils.Get<int>(json, "page_num");
        Utils.Get<JObject>(json, "position")["page_num"] = page;

        foreach (var docOffset in Utils.Get<JArray>(json, "doc_offsets"))
            docOffset["page_num"] = page;

        var spans = Utils.Get<JArray>(json, "doc_offsets")
            .Select(Span.FromJson)
            .ToImmutableArray();

        var cells = Utils.Get<JArray>(json, "cells")
            .Select(cell => Cell.FromJson(cell, page))
            .OrderBy(cell => cell.Range)
            .ToImmutableArray();

        var rows = Enumerable.Range(0, Utils.Get<int>(json, "num_rows"))
            .Select(row => cells
                .Where(cell => cell.Range.Rows.Contains(row))
                .ToImmutableArray())
            .ToImmutableArray();

        var columns = Enumerable.Range(0, Utils.Get<int>(json, "num_columns"))
            .Select(column => cells
                .Where(cell => cell.Range.Columns.Contains(column))
                .ToImmutableArray())
            .ToImmutableArray();

        return new
        (
            Box.FromJson(Utils.Get<JObject>(json, "position")),
            spans,
            cells,
            rows,
            columns
        );
    }

    public override string ToString()
    {
        return IsNull
            ? "NULL_TABLE"
            : Utils.PrettyPrint(
                GetType(),
                this,
                "Box",
                "Spans",
                "Cells"
            );
    }

    /*
    It's more ergonomic to represent the lack of tables with a special null table
    object rather than using `null` or throwing an exception. This lets you e.g.
    sort by the `Table` attribute without having to constantly check for `null`,
    while still allowing you do a "null check" with `Extraction.Table.IsNull`.
    */
    public static readonly Table NULL_TABLE = new(Box.NULL_BOX, [], [], [], []);
    public bool IsNull => this == NULL_TABLE;
}
