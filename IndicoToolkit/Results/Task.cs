using Newtonsoft.Json.Linq;

/*
Results are most often used in async contexts where `Task` refers to an awaitable.

To avoid a name collision that would require explicit namespace prefixing,
Results' `Task` type is put in its own namespace.

In practice, you'll rarely if ever need to reference the `Task` type itself.
Instead using the `TaskType` enum, which doesn't collide and is in the Results namespace.
*/
namespace IndicoToolkit.Results.Tasks;


public record Task
(
    int Id,
    string Name,
    TaskType Type
) : IComparable<Task>
{
    public int CompareTo(Task other) => this.Id.CompareTo(other.Id);

    // Determine the task type of a task from its string representation.
    public static TaskType TaskTypeFromString(string taskType)
    {
        if (taskType == "classification")
            return TaskType.CLASSIFICATION;
        else if (taskType == "annotation")
            return TaskType.DOCUMENT_EXTRACTION;
        else if (taskType == "form_extraction")
            return TaskType.FORM_EXTRACTION;
        else if (taskType == "genai_classification")
            return TaskType.GENAI_CLASSIFICATION;
        else if (taskType == "genai_annotation")
            return TaskType.GENAI_EXTRACTION;
        else if (taskType == "summarization")
            return TaskType.GENAI_SUMMARIZATION;
        else if (taskType == "classification_unbundling")
            return TaskType.UNBUNDLING;
        else
            throw new ResultException($"unsupported task type `{taskType}`");
    }

    // Create a `Task` from a `modelgroup_metadata` list item.
    public static Task FromJson(JToken json)
    {
        return new
        (
            Utils.Get<int>(json, "id"),
            Utils.Get<string>(json, "name"),
            TaskTypeFromString(Utils.Get<string>(json, "task_type"))
        );
    }
}
