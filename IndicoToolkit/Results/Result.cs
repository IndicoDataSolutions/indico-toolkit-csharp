using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public record Result
(
    int Version,
    int SubmissionId,
    List<Document> Documents,
    List<Model> Models,
    PredictionList<Prediction> Predictions,
    List<Review> Reviews
) : IComparable<Result>
{
    public bool Rejected => Reviews.Any() && Reviews.Last().Rejected;
    public PredictionList<Prediction> PreReview => Predictions.Where(pred => pred.Review == null);
    public PredictionList<Prediction> AutoReview => Predictions.Where(reviewType: ReviewType.AUTO);
    public PredictionList<Prediction> ManualReview => Predictions.Where(reviewType: ReviewType.MANUAL);
    public PredictionList<Prediction> AdminReview => Predictions.Where(reviewType: ReviewType.ADMIN);
    public PredictionList<Prediction> Final => Predictions.Where(pred => pred.Review == (Reviews.Any() ? Reviews.Last() : null));

    public int CompareTo(Result other) => this.SubmissionId.CompareTo(other.SubmissionId);

    // Create a `Result` from the root object of a result file.
    public static Result FromJson(JObject json)
    {
        var version = Utils.Get<int>(json, "file_version");

        if (version != 3)
            throw new ResultException($"unsupported file version `{version}`");

        Normalization.NormalizeResultJson(json);

        var submissionId = Utils.Get<int>(json, "submission_id");
        var submissionResults = Utils.Get<JArray>(json, "submission_results");
        var modelgroupMetadata = Utils.Get<JObject>(json, "modelgroup_metadata");
        var componentMetadata = Utils.Get<JObject>(json, "component_metadata");
        var reviewMetadata = Utils.Get<JObject>(json, "reviews");
        var erroredFiles = Utils.Get<JObject>(json, "errored_files");

        var staticModelComponents = componentMetadata.PropertyValues().Where(
            component => Utils.Get<string>(component, "component_type").ToLower() == "static_model"
        );

        var documents = submissionResults.Select(Document.FromJson)
            .Concat(erroredFiles.PropertyValues().Select(Document.FromErroredFileJson))
            .Order()
            .ToList();
        var models = modelgroupMetadata.PropertyValues()
            .Concat(staticModelComponents)
            .Select(Model.FromJson)
            .Order()
            .ToList();
        var reviews = reviewMetadata
            .PropertyValues()
            .Select(Review.FromJson)
            .Order()
            .ToList();

        var predictions = new PredictionList<Prediction>();

        foreach (var documentJson in Utils.Get<JArray>(json, "submission_results"))
        {
            var documentId = Utils.Get<int>(documentJson, "submissionfile_id");
            var document = documents.Where(document => document.Id == documentId).First();
            var modelResultsJson = Utils.Get<JObject>(documentJson, "model_results");
            var componentResultsJson = Utils.Get<JObject>(documentJson, "component_results");
            var originalJson = Utils.Get<JObject>(modelResultsJson, "ORIGINAL").Properties()
                .Concat(Utils.Get<JObject>(componentResultsJson, "ORIGINAL").Properties());

            // Parse pre-review predictions (which don't have an associated review).
            foreach (var modelJson in originalJson)
            {
                var modelId = int.Parse(modelJson.Name);
                var model = models.Where(model => model.Id == modelId).First();

                foreach (var predictionJson in modelJson.Value as JArray)
                    predictions.Add(Prediction.FromJson(
                        document, model, review: null, predictionJson
                    ));
            }

            // Parse final predictions (which are associated with the most recent review).
            if (reviews.Any())
            {
                var review = reviews.Last();
                var finalJson = Utils.Get<JObject>(modelResultsJson, "FINAL").Properties()
                    .Concat(Utils.Get<JObject>(componentResultsJson, "FINAL").Properties());

                foreach (var modelJson in finalJson)
                {
                    var modelId = int.Parse(modelJson.Name);
                    var model = models.Where(model => model.Id == modelId).First();

                    foreach (var predictionJson in modelJson.Value as JArray)
                        predictions.Add(Prediction.FromJson(
                            document, model, review, predictionJson
                        ));
                }
            }
        }

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
}
