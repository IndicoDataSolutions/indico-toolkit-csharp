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
}
