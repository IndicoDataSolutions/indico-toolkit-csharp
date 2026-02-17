using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace IndicoToolkit.EtlOutputs;


public record Range
(
    int Row,
    int Column,
    int RowSpan,
    int ColumnSpan,
    ImmutableArray<int> Rows,
    ImmutableArray<int> Columns
) : IComparable<Range>
{
    public int CompareTo(Range? other)
    {
        if (other == null)
            return 1;
        else if (this.Row == other.Row && this.Column == other.Column && this.RowSpan == other.RowSpan)
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
        var rows = Utils.Get<ImmutableArray<int>>(json, "rows");
        var columns = Utils.Get<ImmutableArray<int>>(json, "columns");

        return new
        (
            rows.Min(),
            columns.Min(),
            rows.Length,
            columns.Length,
            rows,
            columns
        );
    }

    public override string ToString()
    {
        return IsNull
            ? "NULL_RANGE"
            : Utils.PrettyPrint(
                GetType(),
                this,
                "Row",
                "Column",
                "RowSpan",
                "ColumnSpan"
            );
    }

    /*
    It's more ergonomic to represent the lack of ranges with a special null range object
    rather than using `null` or raising an error. This lets you e.g. sort by the
    `Range` property without having to constantly check for `null`, while still
    allowing you do a "null check" with `Cell.Range.IsNull`.
    */
    public static readonly Range NULL_RANGE = new(0, 0, 0, 0, [], []);
    public bool IsNull => this == NULL_RANGE;
}
