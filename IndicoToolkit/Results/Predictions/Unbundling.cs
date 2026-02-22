using IndicoToolkit.EtlOutputs;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace IndicoToolkit.Results;


public record Unbundling : Prediction
{
    public required List<Span> Spans { get; set; }

    public ImmutableList<int> Pages => Spans.Select(span => span.Page).ToImmutableList();

    /*
    Create an `Unbundling` from a prediction JSON.
    */
    public static new Unbundling FromJson(Document document, Results.Tasks.Task task, Review? review, JToken json)
    {
        return new()
        {
            Document = document,
            Task = task,
            Review = review,
            Label = Utils.Get<string>(json, "label"),
            Confidences = Utils.Get<Dictionary<string, double>>(json, "confidence"),
            Spans = Utils.Get<JArray>(json, "spans").Select(Span.FromJson).ToList(),
            Extras = json as JObject,
        };
    }

    /*
    Create JSON for auto review changes.
    */
    public override JObject ToJson()
    {
        Extras["label"] = Label;
        Extras["confidence"] = JObject.FromObject(Confidences);
        Extras["spans"] = new JArray(Spans.Select(span => span.ToJson()));

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
            "Spans"
        );
    }
}
