using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public record Box
(
    int Page,
    int Top,
    int Left,
    int Right,
    int Bottom
)
{
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

    public JObject ToJson()
    {
        return new()
        {
            ["page_num"] = Page,
            ["top"] = Top,
            ["left"] = Left,
            ["right"] = Right,
            ["bottom"] = Bottom,
        };
    }

    // It's more ergonomic to represent the lack of a bounding box with a special null
    // box object rather than using `null` or raising an error. This lets you e.g. sort
    // by the `box` attribute without having to constantly check for `null`, while
    // still allowing you do a "null check" with `Extraction.Box.IsNull`.
    public static readonly Box NULL_BOX = new(0, 0, 0, 0, 0);
    public bool IsNull => this == NULL_BOX;
}
