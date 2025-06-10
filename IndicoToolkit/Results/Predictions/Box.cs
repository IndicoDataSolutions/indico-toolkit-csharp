using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public record Box
(
    int Page,
    int Top,
    int Left,
    int Right,
    int Bottom
) : IComparable<Box>
{
    public int CompareTo(Box other)
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
        if (
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

    // It's more ergonomic to represent the lack of a bounding box with a special null
    // box object rather than using `null` or raising an error. This lets you e.g. sort
    // by the `Box` property without having to constantly check for `null`, while still
    // allowing you do a "null check" with `Extraction.Box.IsNull`.
    public static readonly Box NULL_BOX = new(0, 0, 0, 0, 0);
    public bool IsNull => this == NULL_BOX;
}
