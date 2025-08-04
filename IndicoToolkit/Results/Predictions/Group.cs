using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public record Group
(
    int Id,
    string Name,
    int Index
) : IComparable<Group>
{
    public int CompareTo(Group other) => this.Id == other.Id
        ? this.Index.CompareTo(other.Index)
        : this.Id.CompareTo(other.Id);

    public Group Next() => this with { Index = Index + 1 };

    public static Group FromJson(JToken json)
    {
        var idAndName = Utils.Get<string>(json, "group_id");
        var idString = idAndName.Split(":").First();
        var id = int.Parse(idString);

        return new
        (
            id,
            Utils.Get<string>(json, "group_name"),
            Utils.Get<int>(json, "group_index")
        );
    }

    public JObject ToJson()
    {
        return new()
        {
            ["group_id"] = $"{Id}:{Name}",
            ["group_name"] = Name,
            ["group_index"] = Index,
        };
    }

    public override string ToString()
    {
        return Utils.PrettyPrint(
            GetType(),
            this,
            "Id",
            "Name",
            "Index"
        );
    }
}
