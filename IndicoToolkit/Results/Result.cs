using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace IndicoToolkit.Results;


public record Result
(
    int Version,
    int SubmissionId,
    List<Document> Documents,
    List<ModelGroup> Models,
    PredictionList<Prediction> Predictions,
    List<Review> Reviews
)
{
    public bool Rejected => Reviews.Any() && Reviews.Last().Rejected;
    public PredictionList<Prediction> PreReview => Predictions.Where(pred => pred.Review == null);
    public PredictionList<Prediction> AutoReview => Predictions.Where(reviewType: ReviewType.AUTO);
    public PredictionList<Prediction> ManualReview => Predictions.Where(reviewType: ReviewType.MANUAL);
    public PredictionList<Prediction> AdminReview => Predictions.Where(reviewType: ReviewType.ADMIN);
    public PredictionList<Prediction> Final => Predictions.Where(pred => pred.Review == (Reviews.Any() ? Reviews.Last() : null));

    // Create a `Result` from the root object of a result file.
    public static Result FromJson(JObject json)
    {
        var version = Utils.Get<int>(json, "file_version");

        if (version != 3)
            throw new ResultException($"unsupported file version `{version}`");

        NormalizeJson(json);

        var submissionId = Utils.Get<int>(json, "submission_id");
        var documents = new List<Document>();
        var models = Utils.Get<JObject>(json, "modelgroup_metadata")
            .PropertyValues()
            .Select(value => ModelGroup.FromJson(value))
            .OrderBy(model => model.Id)
            .ToList();
        var predictions = new PredictionList<Prediction>();
        var reviews = Utils.Get<JObject>(json, "reviews")
            .PropertyValues()
            .Select(value => Review.FromJson(value))
            .OrderBy(review => review.Id)
            .ToList();

        foreach (var documentJson in Utils.Get<JArray>(json, "submission_results"))
        {
            var document = Document.FromJson(documentJson);
            documents.Add(document);

            var modelResultsJson = Utils.Get<JObject>(documentJson, "model_results");
            var originalJson = Utils.Get<JObject>(modelResultsJson, "ORIGINAL");
            // Unreviewed results do not have a `FINAL` section.

            // Parse pre-review predictions (which don't have an associated review).
            foreach (var modelJson in originalJson)
            {
                var modelId = int.Parse(modelJson.Key);
                var model = models.Where(model => model.Id == modelId).First();

                foreach (var predictionJson in modelJson.Value as JArray)
                    predictions.Add(Prediction.FromJson(
                        document, model, review: null, predictionJson
                    ));

                // Track model sections so empty ones can be reproduced in auto review changes.
                document.ModelSections.Add(modelJson.Key);
            }

            // Parse final predictions (which don't have an associated review).
            if (reviews.Any())
            {
                var review = reviews.Last();
                var finalJson = Utils.Get<JObject>(modelResultsJson, "FINAL");

                foreach (var modelJson in finalJson)
                {
                    var modelId = int.Parse(modelJson.Key);
                    var model = models.Where(model => model.Id == modelId).First();

                    foreach (var predictionJson in modelJson.Value as JArray)
                        predictions.Add(Prediction.FromJson(
                            document, model, review, predictionJson
                        ));
                }
            }
        }

        foreach (var erroredFileJson in Utils.Get<JObject>(json, "errored_files"))
            documents.Add(Document.FromErroredFileJson(erroredFileJson.Value));

        documents.Sort((left, right) => left.Id.CompareTo(right.Id));

        return new
        (
            version,
            submissionId,
            documents,
            models,
            predictions,
            reviews
        );
    }

    // Fix inconsistencies observed in result files.
    private static void NormalizeJson(JObject json)
    {
        var predictions = (json["submission_results"] as JArray).OfType<JObject>()
            .SelectMany(submissionResult => (submissionResult["model_results"] as JObject).Properties()
                .SelectMany(modelResult => (modelResult.Value as JObject).Properties()
                    .SelectMany(reviewResult => (reviewResult.Value as JArray).OfType<JObject>()
                    )
                )
            );

        foreach (JObject prediction in predictions)
        {
            // Predictions added in review lack a `confidence` section.
            if (!Utils.Has<JObject>(prediction, "confidence"))
            {
                prediction["confidence"] = new JObject { [Utils.Get<string>(prediction, "label")] = 0 };
            }

            // Document Extractions added in review may lack spans.
            if (
                Utils.Has<string>(prediction, "text")
                && !Utils.Has<string>(prediction, "type")
                && !Utils.Has<JArray>(prediction, "spans")
            )
            {
                prediction["spans"] = new JArray
                {
                    new JObject
                    {
                        ["page_num"] = prediction["page_num"],
                        ["start"] = 0,
                        ["end"] = 0,
                    },
                };
            }

            // Form Extractions added in review may lack bounding boxes.
            if (
                Utils.Has<string>(prediction, "type")
                && !Utils.Has<int>(prediction, "top")
            )
            {
                prediction["top"] = 0;
                prediction["left"] = 0;
                prediction["right"] = 0;
                prediction["bottom"] = 0;
            }

            // Prior to 6.11, some Extractions lack a `normalized` section after review.
            if (
                Utils.Has<string>(prediction, "text")
                && !Utils.Has<JObject>(prediction, "normalized")
            )
            {
                prediction["normalized"] = new JObject { ["formatted"] = prediction["text"] };
            }

            // Document Extractions that didn't go through a linked labels transformer
            // lack a `groupings` section.
            if (
                Utils.Has<string>(prediction, "text")
                && !Utils.Has<string>(prediction, "type")
                && !Utils.Has<JArray>(prediction, "groupings")
            )
            {
                prediction["groupings"] = new JArray();
            }
        }

        // Parse filenames for errored files.
        foreach (var erroredFile in json["errored_files"] as JObject)
        {
            var file = erroredFile.Value;

            if (!Utils.Has<string>(file, "input_filename"))
            {
                var reason = Utils.Get<string>(file, "reason");
                var match = Regex.Match(reason, @"file '([^']*)' with id");
                file["input_filename"] = match.Success ? match.Groups[1].Value : "";
            }
        }

        // Prior to 6.8, v3 result files don't include a `reviews` section.
        if (!Utils.Has<JObject>(json, "reviews"))
            json["reviews"] = new JObject();

        // Review notes are `null` unless the reviewer enters a reason for rejection.
        foreach (var review in json["reviews"] as JObject)
            if (!Utils.Has<string>(review.Value, "review_notes"))
                review.Value["review_notes"] = "";
    }
}
