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
    // Auto review changes must reproduce all model sections that were present in the
    // original result file. This may not be possible from the predictions alone--if a
    // model had an empty section because it didn't produce predictions or if all of
    // the predictions were removed to reject them. As such, the models seen when
    // parsing result files are tracked per-document so that the empty sections can be
    // reproduced later.
    ImmutableHashSet<string> ModelSections
)
{
    // Create a `Document` from a `submission_results` list item.
    public static Document FromJson(JToken json)
    {
        return new
        (
            Utils.Get<int>(json, "submissionfile_id"),
            Utils.Get<string>(json, "input_filename"),
            Utils.Get<string>(json, "etl_output"),
            false,
            "",
            "",
            ImmutableHashSet<string>.Empty
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
            ImmutableHashSet<string>.Empty
        );
    }
}
