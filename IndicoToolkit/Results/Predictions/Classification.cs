using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public class Classification : Prediction
{
    // Create a `Classification` from a prediction JSON.
    public static new Classification FromJson(Document document, ModelGroup model, Review? review, JToken json)
    {
        return new Classification
        {
            Document = document,
            Model = model,
            Review = review,
            Label = Utils.Get<string>(json, "label"),
            Confidences = Utils.Get<Dictionary<string, double>>(json, "confidence"),
            Extras = json as JObject,
        };
    }

    // Create JSON for auto review changes.
    public override JObject ToJson()
    {
        Extras["label"] = Label;
        Extras["confidence"] = JObject.FromObject(Confidences);

        return Extras;
    }
}
