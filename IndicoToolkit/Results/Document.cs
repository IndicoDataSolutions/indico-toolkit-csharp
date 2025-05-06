using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace IndicoToolkit.Results;


public class Document : PrettyPrint
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string EtlOutputUri { get; init; }
    public bool Failed { get; init; }
    public string Error { get; init; }
    public string Traceback { get; init; }


    // Auto review changes must reproduce all model sections that were present in the
    // original result file. This may not be possible from the predictions alone--if a
    // model had an empty section because it didn't produce predictions or if all of
    // the predictions were removed to reject them. As such, the models seen when
    // parsing result files are tracked per-document so that the empty sections can be
    // reproduced later.
    [NoPrint]
    public HashSet<string> ModelSections { get; init; }

    // Create a `Document` from a `submission_results` list item.
    public static Document FromJson(JToken json)
    {
        return new Document
        {
            Id = Utils.Get<int>(json, "submissionfile_id"),
            Name = Utils.Get<string>(json, "input_filename"),
            EtlOutputUri = Utils.Get<string>(json, "etl_output"),
            Failed = false,
            Error = "",
            Traceback = "",
            ModelSections = new HashSet<string>(),
        };
    }

    // Create a `Document` from an `errored_files` list item.
    public static Document FromErroredFileJson(JToken json)
    {
        var traceback = Utils.Get<string>(json, "error");
        var error = traceback.Split("\n").Last().Trim();

        return new Document
        {
            Id = Utils.Get<int>(json, "submissionfile_id"),
            Name = Utils.Get<string>(json, "input_filename"),
            EtlOutputUri = "",
            Failed = true,
            Error = error,
            Traceback = traceback,
            ModelSections = new HashSet<string>(),
        };
    }
}
