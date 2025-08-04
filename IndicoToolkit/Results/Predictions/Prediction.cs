using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public abstract record Prediction
{
    public Document Document { get; set; }
    public Results.Tasks.Task Task { get; set; }
    public Review? Review { get; set; }  // Pre-review predictions do not have an associated Review.

    public string Label { get; set; }
    public Dictionary<string, double> Confidences { get; set; }
    public JObject Extras { get; set; }

    public double Confidence
    {
        get => Confidences[Label];
        set => Confidences[Label] = value;
    }

    /*
    Create a `Prediction` subtype appropriate for `task.Type` from a prediction JSON.
    */
    public static Prediction FromJson(Document document, Results.Tasks.Task task, Review? review, JToken json)
    {
        Normalization.NormalizePredictionJson(task.Type, json);

        if (task.Type == TaskType.CLASSIFICATION || task.Type == TaskType.GENAI_CLASSIFICATION)
            return Classification.FromJson(document, task, review, json);
        else if (task.Type == TaskType.DOCUMENT_EXTRACTION || task.Type == TaskType.GENAI_EXTRACTION)
            return DocumentExtraction.FromJson(document, task, review, json);
        else if (task.Type == TaskType.FORM_EXTRACTION)
            return FormExtraction.FromJson(document, task, review, json);
        else if (task.Type == TaskType.GENAI_SUMMARIZATION)
            return Summarization.FromJson(document, task, review, json);
        else if (task.Type == TaskType.UNBUNDLING)
            return Unbundling.FromJson(document, task, review, json);
        else
            throw new ResultException($"unsupported task type `{task.Type}`");
    }

    /*
    Create JSON for auto review changes.
    */
    public abstract JObject ToJson();
}
