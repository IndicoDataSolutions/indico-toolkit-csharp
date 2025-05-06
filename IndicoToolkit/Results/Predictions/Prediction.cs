using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace IndicoToolkit.Results;


public class Prediction : PrettyPrint
{
    public Document Document { get; init; }
    public ModelGroup Model { get; init; }
    public Review? Review { get; init; }  // Pre-review predictions do not have an associated Review.

    public string Label { get; set; }
    [NoPrint]
    public Dictionary<string, double> Confidences { get; init; }
    [NoPrint]
    public JObject Extras { get; init; }

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
    public virtual JObject ToJson()
    {
        throw new NotImplementedException();
    }
}
