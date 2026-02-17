using Newtonsoft.Json.Linq;

namespace IndicoToolkit.EtlOutputs;


public record Span
(
    int Page,
    int Start,
    int End
) : IComparable<Span>
{
    public Range Range => Start..End;

    public int CompareTo(Span other)
    {
        if (this.Page == other.Page && this.Start == other.Start)
            return this.End.CompareTo(other.End);
        else if (this.Page == other.Page)
            return this.Start.CompareTo(other.Start);
        else
            return this.Page.CompareTo(other.Page);
    }

    /*
    Return a new `Span` for the overlap between `this` and `other`
    or `NULL_SPAN` if they don't overlap.
    */
    public Span Intersect(Span other)
    {
        if (
            this.Page != other.Page
            || this.End <= other.Start  // `this` is to the left of `other`
            || this.Start >= other.End  // `this` is to the right of `other`
        )
            return NULL_SPAN;
        else
            return this with
            {
                Start = Math.Max(this.Start, other.Start),
                End = Math.Min(this.End, other.End),
            };
    }

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

    public override string ToString()
    {
        return IsNull
            ? "NULL_SPAN"
            : Utils.PrettyPrint(
                GetType(),
                this,
                "Page",
                "Start",
                "End"
            );
    }

    /*
    It's more ergonomic to represent the lack of spans with a special null span
    object rather than using `null` or throwing an exception. This lets you e.g.
    sort by the `Span` attribute without having to constantly check for `null`,
    while still allowing you do a "null check" with `Extraction.Span.IsNull`.
    */
    public static readonly Span NULL_SPAN = new(0, 0, 0);
    public bool IsNull => this == NULL_SPAN;
}
