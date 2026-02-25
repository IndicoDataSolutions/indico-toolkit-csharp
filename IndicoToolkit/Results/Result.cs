using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace IndicoToolkit.Results;


public record Result
(
    int SubmissionId,
    ImmutableArray<Document> Documents,
    ImmutableArray<Results.Tasks.Task> Tasks,
    ImmutableArray<Review> Reviews,
    PredictionList<Prediction> Predictions
) : IComparable<Result>
{
    public bool Rejected => Reviews.Any() && Reviews.Last().Rejected;
    public PredictionList<Prediction> PreReview => Predictions.Where(pred => pred.Review == null);
    public PredictionList<Prediction> AutoReview => Predictions.Where(reviewType: ReviewType.AUTO);
    public PredictionList<Prediction> ManualReview => Predictions.Where(reviewType: ReviewType.MANUAL);
    public PredictionList<Prediction> AdminReview => Predictions.Where(reviewType: ReviewType.ADMIN);
    public PredictionList<Prediction> Final => Predictions.Where(pred => pred.Review == (Reviews.Any() ? Reviews.Last() : null));

    public int CompareTo(Result? other) => (other == null) ? 1 : this.SubmissionId.CompareTo(other.SubmissionId);
    public virtual bool Equals(Result? other) => other != null && this.SubmissionId == other.SubmissionId;
    public override int GetHashCode() => this.SubmissionId.GetHashCode();

    /*
    Load `resultUri` as a `Result` record. A `reader` function must be supplied to read
    JSON from disk, storage API, or Indico client.
    */
    public static Result Load(string resultUri, Func<string, string> reader)
    {
        var resultJson = JObject.Parse(reader(resultUri));
        return FromJson(resultJson);
    }

    /*
    Load `resultUri` as a `Result` record. A `reader` coroutine must be supplied to read
    JSON from disk, storage API, or Indico client.
    */
    public static async Task<Result> LoadAsync(string resultUri, Func<string, Task<string>> reader)
    {
        var resultJson = JObject.Parse(await reader(resultUri));
        return FromJson(resultJson);
    }

    /*
    Create a `Result` from the root object of a result file.
    */
    public static Result FromJson(JObject json)
    {
        var fileVersion = Utils.Get<int>(json, "file_version");

        if (fileVersion != 3)
            throw new ResultException($"unsupported file version `{fileVersion}`");

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
            .ToImmutableArray();
        var tasks = modelgroupMetadata.PropertyValues()
            .Concat(staticModelComponents)
            .Select(Results.Tasks.Task.FromJson)
            .Order()
            .ToImmutableArray();
        var reviews = reviewMetadata
            .PropertyValues()
            .Select(Review.FromJson)
            .Order()
            .ToImmutableArray();

        var predictions = new PredictionList<Prediction>();

        foreach (var documentJson in submissionResults)
        {
            var documentId = Utils.Get<int>(documentJson, "submissionfile_id");
            var document = documents.Where(document => document.Id == documentId).First();
            var modelResultsJson = Utils.Get<JObject>(documentJson, "model_results");
            var componentResultsJson = Utils.Get<JObject>(documentJson, "component_results");
            var originalResultsJson = Utils.Get<JObject>(modelResultsJson, "ORIGINAL").Properties()
                .Concat(Utils.Get<JObject>(componentResultsJson, "ORIGINAL").Properties());

            // Parse original predictions (which don't have an associated review).
            foreach (var taskJson in originalResultsJson)
            {
                var taskId = int.Parse(taskJson.Name);
                var task = tasks.Where(task => task.Id == taskId).First();

                foreach (var taskPredictions in (JArray)taskJson.Value)
                    predictions.Add(Prediction.FromJson(
                        document, task, review: null, taskPredictions
                    ));
            }

            // Parse final predictions (associated with the most recent review).
            if (reviews.Any())
            {
                var review = reviews.Last();
                var finalResultsJson = Utils.Get<JObject>(modelResultsJson, "FINAL").Properties()
                    .Concat(Utils.Get<JObject>(componentResultsJson, "FINAL").Properties());

                foreach (var taskJson in finalResultsJson)
                {
                    var taskId = int.Parse(taskJson.Name);
                    var task = tasks.Where(task => task.Id == taskId).First();

                    foreach (var taskPredictions in (JArray)taskJson.Value)
                        predictions.Add(Prediction.FromJson(
                            document, task, review, taskPredictions
                        ));
                }
            }
        }

        return new
        (
            submissionId,
            documents,
            tasks,
            reviews,
            predictions
        );
    }

    public override string ToString()
    {
        return Utils.PrettyPrint(
            GetType(),
            this,
            "SubmissionId",
            "SubmissionId",
            "Documents",
            "Tasks",
            "Reviews",
            "Predictions",
            "Rejected"
        );
    }
}
