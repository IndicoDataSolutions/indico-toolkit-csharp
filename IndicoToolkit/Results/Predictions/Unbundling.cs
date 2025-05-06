using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace IndicoToolkit.Results;


public class Unbundling : Prediction
{
    public List<int> Pages { get; set; }

    // Create an `Unbundling` from a prediction JSON.
    public static Unbundling FromJson(Document document, ModelGroup model, Review? review, JToken json)
    {
        var spans = Utils.Get<JArray>(json, "spans");

        return new Unbundling
        {
            Document = document,
            Model = model,
            Review = review,
            Label = Utils.Get<string>(json, "label"),
            Confidences = Utils.Get<Dictionary<string, double>>(json, "confidence"),
            Pages = spans.Select(span => Utils.Get<int>(span, "page_num")).ToList(),
            Extras = json as JObject,
        };
    }

    // Create JSON for auto review changes.
    public override JObject ToJson()
    {
        Extras["label"] = Label;
        Extras["confidence"] = JObject.FromObject(Confidences);
        Extras["spans"] = new JArray(Pages.Select(page => new JObject { ["page_num"] = page }));

        return Extras;
    }
}
