using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public class Group : PrettyPrint
{
    public int Id { get; init; }
    public string Name { get; init; }
    public int Index { get; init; }

    public static Group FromJson(JToken json)
    {
        var idAndName = Utils.Get<string>(json, "group_id");
        var idString = idAndName.Split(":").First();
        var id = int.Parse(idString);

        return new Group
        {
            Id = id,
            Name = Utils.Get<string>(json, "group_name"),
            Index = Utils.Get<int>(json, "group_index"),
        };
    }

    public JObject ToJson()
    {
        return new JObject
        {
            ["group_id"] = $"{Id}:{Name}",
            ["group_name"] = Name,
            ["group_index"] = Index,
        };
    }
}
