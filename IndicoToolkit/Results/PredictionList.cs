using Newtonsoft.Json.Linq;

namespace IndicoToolkit.Results;


public class PredictionList<PredictionType> : List<PredictionType> where PredictionType : Prediction
{
    public PredictionList<Classification> Classifications => OfType<Classification>();
    public PredictionList<DocumentExtraction> DocumentExtractions => OfType<DocumentExtraction>();
    public PredictionList<Extraction> Extractions => OfType<Extraction>();
    public PredictionList<FormExtraction> FormExtractions => OfType<FormExtraction>();
    public PredictionList<Unbundling> Unbundlings => OfType<Unbundling>();

    public PredictionList() : base() { }
    public PredictionList(IEnumerable<PredictionType> collection) : base(collection) { }

    // Apply `function` to all predictions.
    public PredictionList<PredictionType> Apply(Action<PredictionType> function)
    {
        foreach (var prediction in this)
            function(prediction);

        return this;
    }

    // Group predictions into a dictionary using `key`.
    public Dictionary<KeyType, PredictionList<PredictionType>> GroupBy<KeyType>(Func<PredictionType, KeyType> key)
    {
        var grouped = new Dictionary<KeyType, PredictionList<PredictionType>>();

        foreach (var prediction in this)
        {
            KeyType groupKey = key(prediction);

            if (!grouped.ContainsKey(groupKey))
                grouped[groupKey] = new PredictionList<PredictionType>();

            grouped[groupKey].Add(prediction);
        }

        return grouped;
    }

    // Return a new prediction list containing predictions of type `Subtype`.
    public PredictionList<Subtype> OfType<Subtype>() where Subtype : Prediction
    {
        return new PredictionList<Subtype>(Enumerable.OfType<Subtype>(this));
    }

    // Return a new prediction list with predictions sorted by `key`.
    public PredictionList<PredictionType> OrderBy(Func<PredictionType, IComparable> key, bool reverse = false)
    {
        if (reverse)
            return new PredictionList<PredictionType>(this.OrderByDescending(key));
        else
            return new PredictionList<PredictionType>(Enumerable.OrderBy(this, key));
    }

    // Return a new prediction list containing predictions that match
    // all of the specified filters.
    //
    // predicate: predictions for which this function returns True.
    // document: predictions from this document,
    // model: predictions from this model,
    // review: predictions from this review,
    // reviewType: predictions from this review type,
    // label: predictions with this label,
    // min_confidence: predictions with confidence >= this threshold,
    // max_confidence: predictions with confidence <= this threshold,
    public PredictionList<PredictionType> Where(
        Func<PredictionType, bool>? predicate = null,
        Document? document = null,
        Model? model = null,
        string? modelName = null,
        ModelType? modelType = null,
        Review? review = null,
        ReviewType? reviewType = null,
        string? label = null,
        ICollection<string>? labelIn = null,
        double? minConfidence = null,
        double? maxConfidence = null,
        int? page = null,
        ICollection<int>? pageIn = null,
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

        if (model != null)
            predicates.Add(pred => pred.Model == model);

        if (modelName != null)
            predicates.Add(pred => pred.Model.Name == modelName);

        if (modelType != null)
            predicates.Add(pred => pred.Model.Type == modelType);

        if (review != null)
            predicates.Add(pred => pred.Review == review);

        if (reviewType != null)
            predicates.Add(pred => pred.Review != null && pred.Review.Type == reviewType);

        if (label != null)
            predicates.Add(pred => pred.Label == label);

        if (labelIn != null)
            predicates.Add(pred => labelIn.Contains(pred.Label));

        if (minConfidence != null)
            predicates.Add(pred => pred.Confidence >= minConfidence);

        if (maxConfidence != null)
            predicates.Add(pred => pred.Confidence <= maxConfidence);

        if (page != null)
            predicates.Add(pred => pred is Extraction && (pred as Extraction).Page == page);

        if (pageIn != null)
            predicates.Add(pred => pred is Extraction && pageIn.Contains((pred as Extraction).Page));

        if (accepted != null)
            predicates.Add(pred => pred is Extraction && (pred as Extraction).Accepted == accepted);

        if (rejected != null)
            predicates.Add(pred => pred is Extraction && (pred as Extraction).Rejected == rejected);

        if (checked_ != null)
            predicates.Add(pred => pred is FormExtraction && (pred as FormExtraction).Checked == checked_);

        if (signed != null)
            predicates.Add(pred => pred is FormExtraction && (pred as FormExtraction).Signed == signed);

        return new PredictionList<PredictionType>(
            Enumerable.Where(
                this,
                prediction => predicates.All(predicate => predicate(prediction))
            )
        );
    }

    // Accept all extractions in the list.
    public PredictionList<PredictionType> Accept()
    {
        Extractions.Apply(prediction => prediction.Accept());
        return this;
    }

    // Unaccept all extractions in the list.
    public PredictionList<PredictionType> Unaccept()
    {
        Extractions.Apply(prediction => prediction.Unaccept());
        return this;
    }

    // Reject all extractions in the list.
    public PredictionList<PredictionType> Reject()
    {
        Extractions.Apply(prediction => prediction.Reject());
        return this;
    }

    // Unreject all extractions in the list.
    public PredictionList<PredictionType> Unreject()
    {
        Extractions.Apply(prediction => prediction.Unreject());
        return this;
    }

    // Create a JArray for the `changes` argument of `Reviews().SubmitReviewAsync()`
    // based on the predictions in this prediction list and the documents of `result`.
    public JArray ToChanges(Result result)
    {
        var changes = new JArray();

        foreach (var document in result.Documents)
        {
            if (document.Failed) continue;

            var modelResults = new JObject();
            var componentResults = new JObject();

            var predictionsByModel = this.Where(
                document: document
            ).GroupBy<Model>(
                prediction => prediction.Model
            );

            foreach (var modelPair in predictionsByModel)
            {
                var id = modelPair.Key.Id.ToString();
                var predictions = new JArray(
                    modelPair.Value.Select(prediction => prediction.ToJson())
                );

                if (document.ModelIds.Contains(id))
                    modelResults[id] = predictions;
                else
                    componentResults[id] = predictions;
            }

            foreach (var modelId in document.ModelIds)
                if (!modelResults.ContainsKey(modelId))
                    modelResults[modelId] = new JArray();

            foreach (var componentId in document.ComponentIds)
                if (!modelResults.ContainsKey(componentId))
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

    public override string ToString()
    {
        var items = this.Select(item => $"    {item?.ToString()?.Replace("\n", "\n    ") ?? "null"}");
        return $"{GetType().Name} {{\n{string.Join(",\n", items)}\n}}";
    }
}
