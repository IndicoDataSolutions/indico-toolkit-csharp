using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public abstract record Prediction
{
    public Document Document { get; set; }
    public Model Model { get; set; }
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
    public static Prediction FromJson(Document document, Model model, Review? review, JToken json)
    {
        if (model.Type == ModelType.CLASSIFICATION || model.Type == ModelType.GENAI_CLASSIFICATION)
            return Classification.FromJson(document, model, review, json);
        else if (model.Type == ModelType.DOCUMENT_EXTRACTION || model.Type == ModelType.GENAI_EXTRACTION)
            return DocumentExtraction.FromJson(document, model, review, json);
        else if (model.Type == ModelType.FORM_EXTRACTION)
            return FormExtraction.FromJson(document, model, review, json);
        else if (model.Type == ModelType.UNBUNDLING)
            return Unbundling.FromJson(document, model, review, json);
        else
            throw new ResultException($"unsupported model type `{model.Type}`");
    }

    // Create JSON for auto review changes.
    public abstract JObject ToJson();
}
