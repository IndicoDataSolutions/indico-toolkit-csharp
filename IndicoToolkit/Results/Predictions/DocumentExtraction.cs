using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public record DocumentExtraction : Extraction
{
    public HashSet<Group> Groups { get; set; }
    public List<Span> Spans { get; set; }

    public Span Span
    {
        get => Spans.First();
        set => Spans = new List<Span> { value };
    }

    public override int Page => Span.Page;

    // Create an `DocumentExtraction` from a prediction object.
    public static new DocumentExtraction FromJson(Document document, Model model, Review? review, JToken json)
    {
        return new()
        {
            Document = document,
            Model = model,
            Review = review,
            Label = Utils.Get<string>(json, "label"),
            Confidences = Utils.Get<Dictionary<string, double>>(json, "confidence"),
            Text = Utils.Get<string>(json, "normalized", "formatted"),
            Accepted = Utils.Has<bool>(json, "accepted") && Utils.Get<bool>(json, "accepted"),
            Rejected = Utils.Has<bool>(json, "rejected") && Utils.Get<bool>(json, "rejected"),
            Groups = Utils.Get<JArray>(json, "groupings").Select(Group.FromJson).ToHashSet(),
            Spans = Utils.Get<JArray>(json, "spans").Select(Span.FromJson).ToList(),
            Extras = json as JObject,
        };
    }

    // Create JSON for auto review changes.
    public override JObject ToJson()
    {
        Extras["label"] = Label;
        Extras["confidence"] = JObject.FromObject(Confidences);
        Extras["normalized"]["formatted"] = Text;
        Extras["groupings"] = new JArray(Groups.Select(group => group.ToJson()));
        Extras["spans"] = new JArray(Spans.Select(span => span.ToJson()));

        if (Accepted)
            Extras["accepted"] = true;
        else if (Rejected)
            Extras["rejected"] = true;

        return Extras;
    }
}
