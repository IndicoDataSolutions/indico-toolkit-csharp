using IndicoToolkit.Results;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace IndicoToolkit.EtlOutputs;


public record Range
(
    int Row,
    int Column,
    int RowSpan,
    int ColumnSpan,
    ImmutableList<int> Rows,
    ImmutableList<int> Columns
) : IComparable<Range>
{
    public int CompareTo(Range other)
    {
        if (this.Row == other.Row && this.Column == other.Column && this.RowSpan == other.RowSpan)
            return this.ColumnSpan.CompareTo(other.ColumnSpan);
        else if (this.Row == other.Row && this.Column == other.Column)
            return this.RowSpan.CompareTo(other.RowSpan);
        else if (this.Row == other.Row)
            return this.Column.CompareTo(other.Column);
        else
            return this.Row.CompareTo(other.Row);
    }

    public static Range FromJson(JToken json)
    {
        var rows = Utils.Get<ImmutableList<int>>(json, "rows");
        var columns = Utils.Get<ImmutableList<int>>(json, "columns");

        return new
        (
            rows.First(),
            columns.First(),
            rows.Count,
            columns.Count,
            rows,
            columns
        );
    }

    public override string ToString()
    {
        return Utils.PrettyPrint(
            GetType(),
            this,
            "Row",
            "Column",
            "RowSpan",
            "ColumnSpan"
        );
    }
}
