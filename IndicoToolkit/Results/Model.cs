using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public enum ModelType
{
    CLASSIFICATION,
    DOCUMENT_EXTRACTION,
    FORM_EXTRACTION,
    GENAI_CLASSIFICATION,
    GENAI_EXTRACTION,
    GENAI_SUMMARIZATION,
    UNBUNDLING
}


public record Model
(
    int Id,
    string Name,
    ModelType Type
)
{
    // Determine the task type of a model from its string representation.
    public static ModelType ModelTypeFromString(string taskType)
    {
        if (taskType == "classification")
            return ModelType.CLASSIFICATION;
        else if (taskType == "annotation")
            return ModelType.DOCUMENT_EXTRACTION;
        else if (taskType == "form_extraction")
            return ModelType.FORM_EXTRACTION;
        else if (taskType == "genai_classification")
            return ModelType.GENAI_CLASSIFICATION;
        else if (taskType == "genai_annotation")
            return ModelType.GENAI_EXTRACTION;
        else if (taskType == "summarization")
            return ModelType.GENAI_SUMMARIZATION;
        else if (taskType == "classification_unbundling")
            return ModelType.UNBUNDLING;
        else
            throw new ResultException($"unsupported task type `{taskType}`");
    }

    // Create a `Model` from a `modelgroup_metadata` list item.
    public static Model FromJson(JToken json)
    {
        return new
        (
            Utils.Get<int>(json, "id"),
            Utils.Get<string>(json, "name"),
            ModelTypeFromString(Utils.Get<string>(json, "task_type"))
        );
    }
}
