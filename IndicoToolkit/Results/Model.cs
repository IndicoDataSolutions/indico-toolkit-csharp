using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public enum ModelGroupType
{
    CLASSIFICATION,
    DOCUMENT_EXTRACTION,
    FORM_EXTRACTION,
    GENAI_CLASSIFICATION,
    GENAI_EXTRACTION,
    GENAI_SUMMARIZATION,
    UNBUNDLING
}


public record ModelGroup
(
    int Id,
    string Name,
    ModelGroupType Type
)
{
    // Determine the task type of a model from its string representation.
    public static ModelGroupType ModelGroupTypeFromString(string taskType)
    {
        if (taskType == "classification")
            return ModelGroupType.CLASSIFICATION;
        else if (taskType == "annotation")
            return ModelGroupType.DOCUMENT_EXTRACTION;
        else if (taskType == "form_extraction")
            return ModelGroupType.FORM_EXTRACTION;
        else if (taskType == "genai_classification")
            return ModelGroupType.GENAI_CLASSIFICATION;
        else if (taskType == "genai_annotation")
            return ModelGroupType.GENAI_EXTRACTION;
        else if (taskType == "summarization")
            return ModelGroupType.GENAI_SUMMARIZATION;
        else if (taskType == "classification_unbundling")
            return ModelGroupType.UNBUNDLING;
        else
            throw new ResultException($"unsupported task type `{taskType}`");
    }

    // Create a `ModelGroup` from a `modelgroup_metadata` list item.
    public static ModelGroup FromJson(JToken json)
    {
        return new
        (
            Utils.Get<int>(json, "id"),
            Utils.Get<string>(json, "name"),
            ModelGroupTypeFromString(Utils.Get<string>(json, "task_type"))
        );
    }
}
