using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public class Box : PrettyPrint
{
    public int Page { get; init; }
    public int Top { get; init; }
    public int Left { get; init; }
    public int Right { get; init; }
    public int Bottom { get; init; }

    public static Box FromJson(JToken json)
    {
        return new Box
        {
            Page = Utils.Get<int>(json, "page_num"),
            Top = Utils.Get<int>(json, "top"),
            Left = Utils.Get<int>(json, "left"),
            Right = Utils.Get<int>(json, "right"),
            Bottom = Utils.Get<int>(json, "bottom"),
        };
    }

    public JObject ToJson()
    {
        return new JObject
        {
            ["page_num"] = Page,
            ["top"] = Top,
            ["left"] = Left,
            ["right"] = Right,
            ["bottom"] = Bottom,
        };
    }
}
