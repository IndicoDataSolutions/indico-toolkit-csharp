using IndicoToolkit.Results;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace IndicoToolkit.EtlOutputs;


public record Table
(
    Box Box,
    ImmutableList<Cell> Cells,
    ImmutableList<ImmutableList<Cell>> Rows,
    ImmutableList<ImmutableList<Cell>> Columns
)
{
    public static Table FromJson(JToken json)
    {
        var page = Utils.Get<int>(json, "page_num");
        Utils.Get<JObject>(json, "position")["page_num"] = page;

        var cells = Utils.Get<JArray>(json, "cells")
            .Select(cell => Cell.FromJson(cell, page))
            .OrderBy(cell => cell.Range)
            .ToImmutableList();

        var rows = Enumerable.Range(0, Utils.Get<int>(json, "num_rows"))
            .Select(row => cells
                .Where(cell => cell.Range.Rows.Contains(row))
                .ToImmutableList())
            .ToImmutableList();

        var columns = Enumerable.Range(0, Utils.Get<int>(json, "num_columns"))
            .Select(column => cells
                .Where(cell => cell.Range.Columns.Contains(column))
                .ToImmutableList())
            .ToImmutableList();

        return new
        (
            Box.FromJson(Utils.Get<JObject>(json, "position")),
            cells,
            rows,
            columns
        );
    }

    public override string ToString()
    {
        return Utils.PrettyPrint(
            GetType(),
            this,
            "Box",
            "Cells"
        );
    }
}
