using IndicoToolkit.EtlOutputs;
using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public record DocumentExtraction : Extraction
{
    public required HashSet<Group> Groups { get; set; }
    public required List<Span> Spans { get; set; }

    public required List<Token> Tokens { get; set; }
    public required List<Table> Tables { get; set; }
    public required List<Cell> Cells { get; set; }

    public Span Span
    {
        get => Spans.FirstOrDefault(Span.NULL_SPAN);
        set => Spans = value.IsNull ? new() : new() { value };
    }

    public Token Token
    {
        get => Tokens.FirstOrDefault(Token.NULL_TOKEN);
        set => Tokens = value.IsNull ? new() : new() { value };
    }

    public Table Table
    {
        get => Tables.FirstOrDefault(Table.NULL_TABLE);
        set => Tables = value.IsNull ? new() : new() { value };
    }

    public Cell Cell
    {
        get => Cells.FirstOrDefault(Cell.NULL_CELL);
        set => Cells = value.IsNull ? new() : new() { value };
    }

    public IEnumerable<(Table Table, Cell Cell)> TableCells
    {
        get => Tables.Zip(Cells);
        set
        {
            Tables = new();
            Cells = new();

            foreach (var (table, cell) in value)
            {
                if (!Cells.Contains(cell))
                {
                    Tables.Add(table);
                    Cells.Add(cell);
                }
            }
        }
    }

    public override int Page => Span.Page;

    /*
    Create an `DocumentExtraction` from a prediction JSON.
    */
    public static new DocumentExtraction FromJson(Document document, Results.Tasks.Task task, Review? review, JToken json)
    {
        return new()
        {
            Document = document,
            Task = task,
            Review = review,
            Label = Utils.Get<string>(json, "label"),
            Confidences = Utils.Get<Dictionary<string, double>>(json, "confidence"),
            Text = Utils.Get<string>(json, "normalized", "formatted"),
            Accepted = Utils.Has<bool>(json, "accepted") && Utils.Get<bool>(json, "accepted"),
            Rejected = Utils.Has<bool>(json, "rejected") && Utils.Get<bool>(json, "rejected"),
            Groups = Utils.Get<JArray>(json, "groupings").Select(Group.FromJson).ToHashSet(),
            Spans = Utils.Get<JArray>(json, "spans").Select(Span.FromJson).Order().ToList(),
            Tokens = new(),
            Tables = new(),
            Cells = new(),
            Extras = (JObject)json,
        };
    }

    /*
    Create JSON for auto review changes.
    */
    public override JObject ToJson()
    {
        Extras["label"] = Label;
        Extras["confidence"] = JObject.FromObject(Confidences);
        Extras["groupings"] = new JArray(Groups.Select(group => group.ToJson()));
        Extras["spans"] = new JArray(Spans.Select(span => span.ToJson()));

        if (Text != Utils.Get<string>(Extras, "normalized", "formatted"))
        {
            var normalized = Utils.Get<JObject>(Extras, "normalized");
            normalized["formatted"] = Text;
            normalized["text"] = Text;
            Extras["text"] = Text;
        }

        if (Accepted)
            Extras["accepted"] = true;
        else if (Rejected)
            Extras["rejected"] = true;

        return Extras;
    }

    public override string ToString()
    {
        return Utils.PrettyPrint(
            GetType(),
            this,
            "Document",
            "Task",
            "Review",
            "Label",
            "Confidence",
            "Text",
            "Accepted",
            "Rejected",
            "Groups",
            "Spans"
        );
    }
}
