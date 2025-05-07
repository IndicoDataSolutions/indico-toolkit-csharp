using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public abstract record Prediction
{
    public Document Document { get; set; }
    public ModelGroup Model { get; set; }
    public Review? Review { get; set; }  // Pre-review predictions do not have an associated Review.

    public string Label { get; set; }
    public Dictionary<string, double> Confidences { get; set; }
    public JObject Extras { get; set; }

    public double Confidence
    {
        get => Confidences[Label];
        set => Confidences[Label] = value;
    }

    // Create a `Prediction` subtype appropriate for `model.Type` from a prediction JSON.
    public static Prediction FromJson(Document document, ModelGroup model, Review? review, JToken json)
    {
        if (model.Type == ModelGroupType.CLASSIFICATION)
            return Classification.FromJson(document, model, review, json);
        else if (model.Type == ModelGroupType.DOCUMENT_EXTRACTION)
            return DocumentExtraction.FromJson(document, model, review, json);
        else if (model.Type == ModelGroupType.FORM_EXTRACTION)
            return FormExtraction.FromJson(document, model, review, json);
        else if (model.Type == ModelGroupType.UNBUNDLING)
            return Unbundling.FromJson(document, model, review, json);
        else
            throw new ResultException($"unsupported task type `{model.Type}`");
    }

    // Create JSON for auto review changes.
    public abstract JObject ToJson();
}
