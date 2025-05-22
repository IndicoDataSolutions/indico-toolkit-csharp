using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace IndicoToolkit.Results;


public static class Normalization
{
    public static void NormalizeResultJson(JToken json)
    {
        foreach (var erroredFile in Utils.Get<JObject>(json, "errored_files").PropertyValues())
        {
            // Parse filenames for errored files.
            if (!Utils.Has<string>(erroredFile, "input_filename"))
            {
                var reason = Utils.Get<string>(erroredFile, "reason");
                var match = Regex.Match(reason, @"file '([^']*)' with id");
                erroredFile["input_filename"] = match.Success ? match.Groups[1].Value : "";
            }

            // Parse error out of traceback.
            if (!Utils.Has<string>(erroredFile, "traceback"))
            {
                var traceback = Utils.Get<string>(erroredFile, "error");
                var error = traceback.Split("\n").Last().Trim();
                erroredFile["traceback"] = traceback;
                erroredFile["error"] = error;
            }
        }

        // Convert `null` review notes to "".
        foreach (var review in Utils.Get<JObject>(json, "reviews").PropertyValues())
        {
            if (!Utils.Has<string>(review, "review_notes"))
            {
                review["review_notes"] = "";
            }
        }
    }

    public static void NormalizePredictionJson(TaskType taskType, JToken json)
    {
        // Predictions added in review lack a `confidence` section.
        if (!Utils.Has<JObject>(json, "confidence"))
        {
            json["confidence"] = new JObject { [Utils.Get<string>(json, "label")] = 0 };
        }

        // Extractions added in review may lack a `normalized` section.
        if (
            (
                taskType == TaskType.DOCUMENT_EXTRACTION
                || taskType == TaskType.GENAI_EXTRACTION
                || taskType == TaskType.FORM_EXTRACTION
            )
            && !Utils.Has<JObject>(json, "normalized")
        )
        {
            json["normalized"] = new JObject { ["formatted"] = Utils.Get<string>(json, "text") };
        }

        // Document Extractions added in review may lack a `spans` section.
        if (
            (
                taskType == TaskType.DOCUMENT_EXTRACTION
                || taskType == TaskType.GENAI_EXTRACTION
            )
            && !Utils.Has<JArray>(json, "spans")
        )
        {
            json["spans"] = new JArray();
        }

        // Form Extractions added in review may lack bounding box information.
        // These values will match `Box.NULL_BOX`.
        if (
            taskType == TaskType.FORM_EXTRACTION
            && !Utils.Has<int>(json, "top")
        )
        {
            json["page_num"] = 0;
            json["top"] = 0;
            json["left"] = 0;
            json["right"] = 0;
            json["bottom"] = 0;
        }

        // Document Extractions that didn't go through a linked labels transformer
        // lack a `groupings` section.
        if (
            (
                taskType == TaskType.DOCUMENT_EXTRACTION
                || taskType == TaskType.GENAI_EXTRACTION
            )
            && !Utils.Has<JArray>(json, "groupings")
        )
        {
            json["groupings"] = new JArray();
        }
    }
}
