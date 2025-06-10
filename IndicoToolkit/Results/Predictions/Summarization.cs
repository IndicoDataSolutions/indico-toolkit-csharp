using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace IndicoToolkit.Results;


public record Summarization : Extraction
{
    public List<Citation> Citations { get; set; }

    public Citation Citation
    {
        get => Citations.FirstOrDefault(Citation.NULL_CITATION);
        set => Citations = value.IsNull ? new List<Citation>() : new List<Citation> { value };
    }

    public ImmutableList<Span> Spans => Citations.Select(citation => citation.Span).ToImmutableList();

    public Span Span
    {
        get => Citation.Span;
        set => Citation = Citation with { Span = value };
    }

    public override int Page => Span.Page;

    // Create an `Summarization` from a prediction JSON.
    public static new Summarization FromJson(Document document, Results.Tasks.Task task, Review? review, JToken json)
    {
        return new()
        {
            Document = document,
            Task = task,
            Review = review,
            Label = Utils.Get<string>(json, "label"),
            Confidences = Utils.Get<Dictionary<string, double>>(json, "confidence"),
            Text = Utils.Get<string>(json, "text"),
            Accepted = Utils.Has<bool>(json, "accepted") && Utils.Get<bool>(json, "accepted"),
            Rejected = Utils.Has<bool>(json, "rejected") && Utils.Get<bool>(json, "rejected"),
            Citations = Utils.Get<JArray>(json, "citations").Select(Citation.FromJson).Order().ToList(),
            Extras = json as JObject,
        };
    }

    // Create JSON for auto review changes.
    public override JObject ToJson()
    {
        Extras["label"] = Label;
        Extras["confidence"] = JObject.FromObject(Confidences);
        Extras["text"] = Text;
        Extras["citations"] = new JArray(Citations.Select(citation => citation.ToJson()));

        if (Accepted)
            Extras["accepted"] = true;
        else if (Rejected)
            Extras["rejected"] = true;

        return Extras;
    }
}
