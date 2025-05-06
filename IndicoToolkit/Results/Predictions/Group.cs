using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace IndicoToolkit.Results;


public class Group : PrettyPrint
{
    public int Id { get; init; }
    public string Name { get; init; }
    public int Index { get; init; }

    public static Group FromJson(JToken json)
    {
        return new Group
        {
            Id = int.Parse(Utils.Get<string>(json, "group_id").Split(":")[0]),
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
