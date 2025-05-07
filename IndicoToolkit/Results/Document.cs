using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace IndicoToolkit.Results;


public record Document
(
    int Id,
    string Name,
    string EtlOutputUri,
    bool Failed,
    string Error,
    string Traceback,
    // Auto review changes must reproduce all model and component sections that were
    // present in the original result file. This may not be possible from the
    // predictions alone--if a model or component had an empty section because it didn't
    // produce predictions or if all of the predictions for that section were dropped.
    // As such, the models and components seen when parsing a result file are tracked
    // per-document so that the empty sections can be reproduced later.
    ImmutableHashSet<string> ModelSections,
    ImmutableHashSet<string> ComponentSections
)
{
    // Create a `Document` from a `submission_results` list item.
    public static Document FromJson(JToken json)
    {
        var modelResults = Utils.Get<JObject>(json, "model_results", "ORIGINAL");
        var componentResults = Utils.Get<JObject>(json, "component_results", "ORIGINAL");
        var modelIds = modelResults.Properties().Select(p => p.Name);
        var componentIds = componentResults.Properties().Select(p => p.Name);

        return new
        (
            Utils.Get<int>(json, "submissionfile_id"),
            Utils.Get<string>(json, "input_filename"),
            Utils.Get<string>(json, "etl_output"),
            false,
            "",
            "",
            modelIds.ToImmutableHashSet(),
            componentIds.ToImmutableHashSet()
        );
    }

    // Create a `Document` from an `errored_files` list item.
    public static Document FromErroredFileJson(JToken json)
    {
        var traceback = Utils.Get<string>(json, "error");
        var error = traceback.Split("\n").Last().Trim();

        return new
        (
            Utils.Get<int>(json, "submissionfile_id"),
            Utils.Get<string>(json, "input_filename"),
            "",
            true,
            error,
            traceback,
            ImmutableHashSet<string>.Empty,
            ImmutableHashSet<string>.Empty
        );
    }
}
