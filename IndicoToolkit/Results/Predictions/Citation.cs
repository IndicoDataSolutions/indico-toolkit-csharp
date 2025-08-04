using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public record Citation
(
    int Start,
    int End,
    Span Span
) : IComparable<Citation>
{
    public Range Range => Start..End;

    public int CompareTo(Citation other)
    {
        if (this.Start == other.Start && this.End == other.End)
            return this.Span.CompareTo(other.Span);
        else if (this.Start == other.Start)
            return this.End.CompareTo(other.End);
        else
            return this.Start.CompareTo(other.Start);
    }

    public static Citation FromJson(JToken json)
    {
        return new
        (
            Utils.Get<int>(json, "response", "start"),
            Utils.Get<int>(json, "response", "end"),
            Span.FromJson(Utils.Get<JObject>(json, "document"))
        );
    }

    public JObject ToJson()
    {
        return new()
        {
            ["response"] = new JObject {
                ["start"] = Start,
                ["end"] = End,
            },
            ["document"] = Span.ToJson(),
        };
    }

    public override string ToString()
    {
        return IsNull
            ? "NULL_CITATION"
            : Utils.PrettyPrint(
                GetType(),
                this,
                "Start",
                "End",
                "Span"
            );
    }

    /*
    It's more ergonomic to represent the lack of citations with a special null citation
    object rather than using `null` or raising an error. This lets you e.g. sort by the
    `citation` property without having to constantly check for `null`, while still
    allowing you do a "null check" with `summarization.citation.IsNull`.
    */
    public static readonly Citation NULL_CITATION = new(0, 0, Span.NULL_SPAN);
    public bool IsNull => this == NULL_CITATION;
}
