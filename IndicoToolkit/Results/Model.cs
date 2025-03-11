using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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


public class ModelGroup : PrettyPrint
{
    public int? Id { get; init; }  // v1 result files don't include Model Group IDs.
    public string Name { get; init; }
    public ModelGroupType Type { get; init; }

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

    // Determine the task type of a model using a heuristic on its pre-review structure,
    // (whether it's an object or an array) and its first prediction (the keys it has).
    public static ModelGroupType ModelGroupTypeFromV1Heuristic(JToken preReviewJson)
    {
        if (preReviewJson.HasValues)  // Extraction model sections may be empty.
        {
            var prediction = preReviewJson.First;

            if (Utils.Has<string>(prediction, "type"))
                return ModelGroupType.FORM_EXTRACTION;
            else if (Utils.Has<string>(prediction, "text"))
                return ModelGroupType.DOCUMENT_EXTRACTION;
            else
                return ModelGroupType.CLASSIFICATION;
        }
        else
        {
            return ModelGroupType.DOCUMENT_EXTRACTION;
        }
    }

    // Create a `ModelGroup` from a Model/Predictions key/value pair of a
    // `["results"]["document"]["results"]` section of a v1 result file.
    // Use a heuristic on the first prediction of the model to determine its type.
    public static ModelGroup FromV1Json(KeyValuePair<string, JToken> modelPredictions)
    {
        return new ModelGroup
        {
            Id = null,
            Name = modelPredictions.Key,
            Type = ModelGroupTypeFromV1Heuristic(
                Utils.Get<JToken>(modelPredictions.Value, "pre_review")
            ),
        };
    }

    // Create a ModelGroup from a v3 `modelgroup_metadata` list item.
    public static ModelGroup FromV3Json(JToken json)
    {
        return new ModelGroup
        {
            Id = Utils.Get<int>(json, "id"),
            Name = Utils.Get<string>(json, "name"),
            Type = ModelGroupTypeFromString(Utils.Get<string>(json, "task_type")),
        };
    }
}
