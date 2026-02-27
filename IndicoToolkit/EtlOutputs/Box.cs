using Newtonsoft.Json.Linq;

namespace IndicoToolkit.EtlOutputs;


public record Box
(
    int Page,
    int Top,
    int Left,
    int Right,
    int Bottom
) : IComparable<Box>
{
    public int CompareTo(Box? other)
    {
        /*
        Bounding boxes are sorted with vertical hysteresis. Those on the same line are
        sorted left-to-right, even when later tokens are higher than earlier ones,
        as long as they overlap vertically.

        ┌──────────────────┐ ┌───────────────────┐
        │        1         │ │         2         │
        └──────────────────┘ │                   │
                             └───────────────────┘
                        ┌────────────────┐
        ┌─────────────┐ │        4       │ ┌─────┐
        │      3      │ └────────────────┘ │  5  │
        └─────────────┘                    └─────┘
        */
        if (other == null)
            return 1;
        else if (
            this.Page < other.Page
            || (this.Page == other.Page && this.Bottom < other.Top)
            || (this.Page == other.Page && this.Top < other.Bottom && this.Left < other.Left)
        )
            return -1;
        else if (this == other)
            return 0;
        else
            return 1;
    }

    /*
    Return a new `Box` for the overlap between `this` and `other`
    or `NULL_BOX` if they don't overlap.
    */
    public Box Intersect(Box other)
    {
        if (
            this.Page != other.Page
            || this.Bottom <= other.Top  // `this` is above `other`
            || this.Top >= other.Bottom  // `this` is below `other`
            || this.Right <= other.Left  // `this` is to the left of `other`
            || this.Left >= other.Right  // `this` is to the right of `other`
        )
            return NULL_BOX;
        else
            return this with
            {
                Top = Math.Max(this.Top, other.Top),
                Left = Math.Max(this.Left, other.Left),
                Right = Math.Min(this.Right, other.Right),
                Bottom = Math.Min(this.Bottom, other.Bottom),
            };
    }

    public static Box FromJson(JToken json)
    {
        return new
        (
            Utils.Get<int>(json, "page_num"),
            Utils.Get<int>(json, "top"),
            Utils.Get<int>(json, "left"),
            Utils.Get<int>(json, "right"),
            Utils.Get<int>(json, "bottom")
        );
    }

    public override string ToString()
    {
        return IsNull
            ? "NULL_BOX"
            : Utils.PrettyPrint(
                GetType(),
                this,
                "Page",
                "Top",
                "Left",
                "Right",
                "Bottom"
            );
    }

    /*
    It's more ergonomic to represent the lack of a bounding box with a special null
    box object rather than using `null` or raising an error. This lets you e.g. sort
    by the `Box` property without having to constantly check for `null`, while still
    allowing you do a "null check" with `Extraction.Box.IsNull`.
    */
    public static readonly Box NULL_BOX = new(0, 0, 0, 0, 0);
    public bool IsNull => this == NULL_BOX;
}
