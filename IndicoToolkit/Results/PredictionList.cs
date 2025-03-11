using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IndicoToolkit.Results;


public class PredictionList<PredictionType> : PrettyPrintList<PredictionType> where PredictionType : Prediction
{
    [NoPrint]
    public PredictionList<Classification> Classifications => OfType<Classification>();
    [NoPrint]
    public PredictionList<DocumentExtraction> DocumentExtractions => OfType<DocumentExtraction>();
    [NoPrint]
    public PredictionList<Extraction> Extractions => OfType<Extraction>();
    [NoPrint]
    public PredictionList<FormExtraction> FormExtractions => OfType<FormExtraction>();
    [NoPrint]
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
        ModelGroup? model = null,
        string? modelName = null,
        ModelGroupType? modelType = null,
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

    // Create a JObject or JArray for the `changes` argument of `SubmitReview` based on
    // the predictions in this prediction list and the documents and version of `result`.
    public dynamic ToChanges(Result result)
    {
        if (result.Version == 1)
            return ToV1Changes(result.Documents.Single());
        else if (result.Version == 3)
            return ToV3Changes(result.Documents);
        else
            throw new ResultException($"unsupported file version `{result.Version}`");
    }

    // Create a v1 JObject for the `changes` argument of `SubmitReview`.
    private JObject ToV1Changes(Document document)
    {
        var changes = new JObject();

        foreach (var pair in this.GroupBy<ModelGroup>(prediction => prediction.Model))
        {
            var model = pair.Key;
            var predictions = pair.Value;

            if (model.Type == ModelGroupType.CLASSIFICATION)
                changes[model.Name] = predictions.Single().ToV1Json();
            else
                changes[model.Name] = new JArray(
                    predictions.Select(prediction => prediction.ToV1Json())
                );
        }

        // Reproduce empty models sections from the original result file.
        foreach (var modelName in document.ModelSections)
            if (!changes.ContainsKey(modelName))
                changes[modelName] = new JArray();

        return changes;
    }

    // Create a v3 JArray for the `changes` argument of `SubmitReview`.
    private JArray ToV3Changes(List<Document> documents)
    {
        var changes = new JArray();

        foreach (var document in documents)
        {
            var modelResults = new JObject();
            var predictionsByModel = this.Where(
                document: document
            ).GroupBy<ModelGroup>(
                prediction => prediction.Model
            );

            foreach (var modelPair in predictionsByModel)
            {
                var model = modelPair.Key;
                var modelPredictions = modelPair.Value;

                modelResults[model.Id.ToString()] = new JArray(
                    modelPredictions.Select(prediction => prediction.ToV3Json())
                );
            }

            // Reproduce empty model sections from the original result file.
            foreach (var modelId in document.ModelSections)
                if (!modelResults.ContainsKey(modelId))
                    modelResults[modelId] = new JArray();

            changes.Add(
                new JObject
                {
                    ["submissionfile_id"] = document.Id,
                    ["model_results"] = modelResults,
                    ["component_results"] = new JObject(),
                }
            );
        }

        return changes;
    }
}
