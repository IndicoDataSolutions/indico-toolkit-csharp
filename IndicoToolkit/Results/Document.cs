using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace IndicoToolkit.Results;


public class Document : PrettyPrint
{
    public int? Id { get; init; }  // v1 result files don't include Document IDs.
    public string? Name { get; init; }  // v1 result files don't include Document Names.
    public string EtlOutputUri { get; init; }

    // Auto review changes must reproduce all model sections that were present in the
    // original result file. This may not be possible from the predictions alone--if a
    // model had an empty section because it didn't produce predictions or if all of
    // the predictions were removed to reject them. As such, the models seen when
    // parsing result files are tracked per-document so that the empty sections can be
    // reproduced later.
    [NoPrint]
    public HashSet<string> ModelSections { get; init; }

    // Create a Document from the root structure of a v1 result file.
    public static Document FromV1Json(JToken json)
    {
        var etlOutputUri = Utils.Get<string>(json, "etl_output");

        return new Document
        {
            Id = null,
            Name = null,
            EtlOutputUri = etlOutputUri,
            ModelSections = new HashSet<string>(),
        };
    }

    // Create a Document from a v3 `submission_results` list item.
    public static Document FromV3Json(JToken json)
    {
        var etlOutputUri = Utils.Get<string>(json, "etl_output");

        return new Document
        {
            Id = Utils.Get<int>(json, "submissionfile_id"),
            Name = Utils.Get<string>(json, "input_filename"),
            EtlOutputUri = etlOutputUri,
            ModelSections = new HashSet<string>(),
        };
    }
}
