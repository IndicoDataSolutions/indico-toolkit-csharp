using Newtonsoft.Json.Linq;
using System.Collections.Immutable;

namespace IndicoToolkit.Results;


public class PredictionList<PredictionType> : List<PredictionType> where PredictionType : Prediction
{
    public PredictionList<Classification> Classifications => OfType<Classification>();
    public PredictionList<DocumentExtraction> DocumentExtractions => OfType<DocumentExtraction>();
    public PredictionList<Extraction> Extractions => OfType<Extraction>();
    public PredictionList<FormExtraction> FormExtractions => OfType<FormExtraction>();
    public PredictionList<Summarization> Summarizations => OfType<Summarization>();
    public PredictionList<Unbundling> Unbundlings => OfType<Unbundling>();

    public PredictionList() : base() { }
    public PredictionList(IEnumerable<PredictionType> collection) : base(collection) { }

    /*
    Apply `function` to all predictions.
    */
    public PredictionList<PredictionType> Apply(Action<PredictionType> function)
    {
        foreach (var prediction in this)
            function(prediction);

        return this;
    }

    /*
    Group predictions into a dictionary using `key`.
    */
    public Dictionary<KeyType, PredictionList<PredictionType>> GroupBy<KeyType>(Func<PredictionType, KeyType> key)
    {
        var groupedPredictions = new Dictionary<KeyType, PredictionList<PredictionType>>();

        foreach (var prediction in this)
        {
            KeyType groupKey = key(prediction);

            if (!groupedPredictions.ContainsKey(groupKey))
                groupedPredictions[groupKey] = new PredictionList<PredictionType>();

            groupedPredictions[groupKey].Add(prediction);
        }

        return groupedPredictions;
    }

    /*
    Group predictions into a dictionary using `keys`.
    Each prediction is associated with every key in the iterable individually.
    If the iterable is empty, the prediction is not included in any group.
    */
    public Dictionary<KeyType, PredictionList<PredictionType>> GroupByIter<KeyType>(Func<PredictionType, IEnumerable<KeyType>> keys)
    {
        var groupedPredictions = new Dictionary<KeyType, PredictionList<PredictionType>>();

        foreach (var prediction in this)
        {
            foreach (var groupKey in keys(prediction))
            {
                if (!groupedPredictions.ContainsKey(groupKey))
                    groupedPredictions[groupKey] = new PredictionList<PredictionType>();

                groupedPredictions[groupKey].Add(prediction);
            }
        }

        return groupedPredictions;
    }

    /*
    Return a new prediction list containing predictions of type `Subtype`.
    */
    public PredictionList<Subtype> OfType<Subtype>() where Subtype : Prediction
    {
        return new PredictionList<Subtype>(Enumerable.OfType<Subtype>(this));
    }

    /*
    Return a new prediction list with predictions sorted by `key`.
    */
    public PredictionList<PredictionType> OrderBy(Func<PredictionType, IComparable> key, bool reverse = false)
    {
        if (reverse)
            return new PredictionList<PredictionType>(this.OrderByDescending(key));
        else
            return new PredictionList<PredictionType>(Enumerable.OrderBy(this, key));
    }

    /*
    Return a new prediction list containing predictions that match
    all of the specified filters.

    predicate: predictions for which this function returns true,
    document: predictions from this document,
    documentIn: predictions from any of these documents,
    task: predictions from this task,
    taskIn: predictions from any of these tasks,
    taskName: predictions with this task name,
    taskNameIn: predictions with any of these task names,
    taskType: predictions with this task type,
    taskTypeIn: predictions with any of these task types,
    review: predictions from this review,
    reviewIn: predictions from any of these reviews,
    reviewType: predictions from this review type,
    reviewTypeIn: predictions from any of these review types,
    label: predictions with this label,
    labelIn: predictions with any of these labels,
    page: extractions on this page,
    pageIn: extractions on any of these pages,
    minConfidence: predictions with confidence >= this threshold,
    maxConfidence: predictions with confidence <= this threshold,
    accepted: extractions that are accepted (or not),
    rejected: extractions that are rejected (or not),
    checked_: form extractions that are checked (or not),
    signed: form extractions that are signed (or not).
    */
    public PredictionList<PredictionType> Where(
        Func<PredictionType, bool>? predicate = null,
        Document? document = null,
        ICollection<Document>? documentIn = null,
        Results.Tasks.Task? task = null,
        ICollection<Results.Tasks.Task>? taskIn = null,
        string? taskName = null,
        ICollection<string>? taskNameIn = null,
        TaskType? taskType = null,
        ICollection<TaskType>? taskTypeIn = null,
        Review? review = null,
        ICollection<Review>? reviewIn = null,
        ReviewType? reviewType = null,
        ICollection<ReviewType>? reviewTypeIn = null,
        string? label = null,
        ICollection<string>? labelIn = null,
        int? page = null,
        ICollection<int>? pageIn = null,
        double? minConfidence = null,
        double? maxConfidence = null,
        bool? accepted = null,
        bool? rejected = null,
        bool? checked_ = null,
        bool? signed = null
    )
    {
        List<Func<PredictionType, bool>> predicates = new List<Func<PredictionType, bool>>();

        if (predicate != null)
            predicates.Add(predicate);

        if (document != null)
            predicates.Add(pred => pred.Document == document);

        if (documentIn != null)
            predicates.Add(pred => documentIn.Contains(pred.Document));

        if (task != null)
            predicates.Add(pred => pred.Task == task);

        if (taskIn != null)
            predicates.Add(pred => taskIn.Contains(pred.Task));

        if (taskName != null)
            predicates.Add(pred => pred.Task.Name == taskName);

        if (taskNameIn != null)
            predicates.Add(pred => taskNameIn.Contains(pred.Task.Name));

        if (taskType != null)
            predicates.Add(pred => pred.Task.Type == taskType);

        if (taskTypeIn != null)
            predicates.Add(pred => taskTypeIn.Contains(pred.Task.Type));

        if (review != null)
            predicates.Add(pred => pred.Review == review);

        if (reviewIn != null)
            predicates.Add(pred => reviewIn.Contains(pred.Review));

        if (reviewType != null)
            predicates.Add(pred => pred.Review != null && pred.Review.Type == reviewType);

        if (reviewTypeIn != null)
            predicates.Add(pred => pred.Review != null && reviewTypeIn.Contains(pred.Review.Type));

        if (label != null)
            predicates.Add(pred => pred.Label == label);

        if (labelIn != null)
            predicates.Add(pred => labelIn.Contains(pred.Label));

        if (page != null)
            predicates.Add(pred =>
                pred is Extraction && (pred as Extraction).Page == page
                ||
                pred is Unbundling && (pred as Unbundling).Pages.Contains((int)page)
            );

        if (pageIn != null)
            predicates.Add(pred =>
                pred is Extraction && pageIn.Contains((pred as Extraction).Page)
                ||
                pred is Unbundling && pageIn.ToImmutableHashSet().Intersect((pred as Unbundling).Pages).Any()
            );

        if (minConfidence != null)
            predicates.Add(pred => pred.Confidence >= minConfidence);

        if (maxConfidence != null)
            predicates.Add(pred => pred.Confidence <= maxConfidence);

        if (accepted != null)
            predicates.Add(pred => pred is Extraction && (pred as Extraction).Accepted == accepted);

        if (rejected != null)
            predicates.Add(pred => pred is Extraction && (pred as Extraction).Rejected == rejected);

        if (checked_ != null)
            predicates.Add(pred =>
                pred is FormExtraction
                && (pred as FormExtraction).Type == FormExtractionType.CHECKBOX
                && (pred as FormExtraction).Checked == checked_
            );

        if (signed != null)
            predicates.Add(pred =>
                pred is FormExtraction
                && (pred as FormExtraction).Type == FormExtractionType.CHECKBOX
                && (pred as FormExtraction).Signed == signed
            );

        return new PredictionList<PredictionType>(
            Enumerable.Where(
                this,
                prediction => predicates.All(predicate => predicate(prediction))
            )
        );
    }

    /*
    Mark extractions as accepted for auto review.
    */
    public PredictionList<PredictionType> Accept()
    {
        Extractions.Apply(prediction => prediction.Accept());
        return this;
    }

    /*
    Mark extractions as not accepted for auto review.
    */
    public PredictionList<PredictionType> Unaccept()
    {
        Extractions.Apply(prediction => prediction.Unaccept());
        return this;
    }

    /*
    Mark extractions as rejected for auto review.
    */
    public PredictionList<PredictionType> Reject()
    {
        Extractions.Apply(prediction => prediction.Reject());
        return this;
    }

    /*
    Mark extractions as not rejected for auto review.
    */
    public PredictionList<PredictionType> Unreject()
    {
        Extractions.Apply(prediction => prediction.Unreject());
        return this;
    }

    /*
    Create a JArray for the `changes` argument of `Reviews().SubmitReviewAsync()`
    based on the predictions in this prediction list and the documents in `result`.
    */
    public JArray ToChanges(Result result)
    {
        var changes = new JArray();

        foreach (var document in result.Documents)
        {
            if (document.Failed) continue;

            var modelResults = new JObject();
            var componentResults = new JObject();

            var predictionsByTask = this.Where(
                document: document
            ).GroupBy<Results.Tasks.Task>(
                prediction => prediction.Task
            );

            foreach (var taskItem in predictionsByTask)
            {
                var taskId = taskItem.Key.Id.ToString();
                var predictions = new JArray(
                    taskItem.Value.Select(prediction => prediction.ToJson())
                );

                if (document.ModelIds.Contains(taskId))
                    modelResults[taskId] = predictions;
                else if (document.ComponentIds.Contains(taskId))
                    componentResults[taskId] = predictions;
            }

            foreach (var modelId in document.ModelIds)
                if (!modelResults.ContainsKey(modelId))
                    modelResults[modelId] = new JArray();

            foreach (var componentId in document.ComponentIds)
                if (!componentResults.ContainsKey(componentId))
                    componentResults[componentId] = new JArray();

            changes.Add(
                new JObject
                {
                    ["submissionfile_id"] = document.Id,
                    ["model_results"] = modelResults,
                    ["component_results"] = componentResults,
                }
            );
        }

        return changes;
    }
}
