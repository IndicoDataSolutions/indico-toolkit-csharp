using IndicoToolkit.EtlOutputs;
using IndicoToolkit.Results;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using Xunit;

namespace IndicoToolkit.Tests.Results;

public class PredictionListTests
{
    private static Document document => new(
        2922,
        "1040_filled.tiff",
        "indico-file:///storage/submission/2922/etl_output.json",
        false,
        "",
        "",
        ImmutableHashSet.Create("124", "123", "122", "121"),
        ImmutableHashSet<string>.Empty
    );

    public static IndicoToolkit.Results.Tasks.Task classificationTask => new(
        121, "Tax Classification", TaskType.CLASSIFICATION
    );
    public static IndicoToolkit.Results.Tasks.Task extractionTask => new(
        122, "1040 Document Extraction", TaskType.DOCUMENT_EXTRACTION
    );

    public static Review autoReview => new(1306, 5, "", false, ReviewType.AUTO);
    public static Review manualReview => new(1308, 5, "", false, ReviewType.MANUAL);

    public static Group groupAlpha => new(12345, "Alpha", 0);
    public static Group groupBravo => new(12345, "Bravo", 0);

    public static PredictionList<Prediction> Predictions = new()
    {
        new Classification()
        {
            Document = document,
            Task = classificationTask,
            Review = null,
            Label = "1040",
            Confidences = new() { {"1040", 0.7} },
            Extras = new(),
        },
        new DocumentExtraction()
        {
            Document = document,
            Task = extractionTask,
            Review = autoReview,
            Label = "First Name",
            Confidences = new() { {"First Name", 0.8} },
            Text = "John",
            Groups = new() { groupAlpha },
            Spans = new() { new(0, 352, 356) },
            Tokens = new(),
            Tables = new(),
            Cells = new(),
            Extras = new(),
        },
        new DocumentExtraction()
        {
            Document = document,
            Task = extractionTask,
            Review = manualReview,
            Label = "Last Name",
            Confidences = new() { {"Last Name", 0.9} },
            Text = "Doe",
            Groups = new() { groupAlpha, groupBravo },
            Spans = new() { new(1, 357, 360) },
            Tokens = new(),
            Tables = new(),
            Cells = new(),
            Extras = new(),
        },
    };

    [Fact]
    public void TestClassifications()
    {
        var classification = Predictions.Classifications.Single();
        Assert.IsType<Classification>(classification);
    }

    [Fact]
    public void TestExtractions()
    {
        var extractions = Predictions.Extractions;
        var firstExtraction = extractions.First();
        var secondExtraction = extractions.Last();
        Assert.IsType<DocumentExtraction>(firstExtraction);
        Assert.IsType<DocumentExtraction>(secondExtraction);
    }

    [Fact]
    public void TestSliceIsPredictionList()
    {
        var predictions = Predictions;

        var prediction = predictions[0];
        Assert.IsType<Classification>(prediction);

        var sliced = predictions[1..3];
        Assert.Equal(2, sliced.Count);
        Assert.IsType<PredictionList<Prediction>>(sliced);
    }

    [Fact]
    public void TestGroupby()
    {
        var extractions = Predictions.DocumentExtractions;
        var firstName = extractions.First();
        var lastName = extractions.Last();

        var predictionsByGroups = extractions.GroupBy<HashSet<Group>>(e => e.Groups);
        Assert.Equal(
            new()
            {
                { firstName.Groups, new() { firstName } },
                { lastName.Groups, new() { lastName } },
            },
            predictionsByGroups
        );

        var predictionsBySpans = extractions.GroupBy<List<Span>>(e => e.Spans);
        Assert.Equal(
            new()
            {
                { firstName.Spans, new() { firstName } },
                { lastName.Spans, new() { lastName } },
            },
            predictionsBySpans
        );
    }

    [Fact]
    public void TestGroupbyiter()
    {
        var extractions = Predictions.DocumentExtractions;
        var firstName = extractions.First();
        var lastName = extractions.Last();
        var predictionsByGroup = extractions.GroupByIter<Group>(e => e.Groups);
        Assert.Equal(
            new()
            {
                { groupAlpha, new() { firstName, lastName } },
                { groupBravo, new() { lastName } },
            },
            predictionsByGroup
        );
    }

    [Fact]
    public void TestOrderby()
    {
        var predictions = Predictions;
        var classification = predictions.First();
        var firstName = predictions.Skip(1).First();
        var lastName = predictions.Skip(2).First();
        var ordered = predictions.OrderBy(p => p.Confidence, reverse: true);
        Assert.Equal(new() { lastName, firstName, classification }, ordered);
    }

    [Fact]
    public void TestWhereDocument()
    {
        Assert.Equal(Predictions, Predictions.Where(document: document));
    }

    [Fact]
    public void TestWhereDocumentIn()
    {
        Assert.Equal(Predictions, Predictions.Where(documentIn: new[] { document }));
        Assert.Empty(Predictions.Where(documentIn: new List<Document> { }));
    }

    [Fact]
    public void TestWhereTask()
    {
        var predictions = Predictions;
        var classification = predictions.Classifications.Single();
        Assert.Equal(new() { classification }, predictions.Where(task: classificationTask));
        Assert.Equal(new() { classification }, predictions.Where(taskType: TaskType.CLASSIFICATION));
        Assert.Equal(new() { classification }, predictions.Where(taskName: "Tax Classification"));
    }

    [Fact]
    public void TestWhereTaskIn()
    {
        var predictions = Predictions;
        var classification = predictions.First();
        var firstName = predictions.Skip(1).First();
        var lastName = predictions.Skip(2).First();
        Assert.Equal(new() { classification }, predictions.Where(taskIn: new[] { classificationTask }));
        Assert.Equal(new() { classification }, predictions.Where(taskTypeIn: new[] { TaskType.CLASSIFICATION }));
        Assert.Equal(new() { classification, firstName, lastName }, predictions.Where(taskTypeIn: new[] { TaskType.CLASSIFICATION, TaskType.DOCUMENT_EXTRACTION }));
        Assert.Equal(new() { classification }, predictions.Where(taskNameIn: new[] { "Tax Classification" }));
        Assert.Equal(new() { classification, firstName, lastName }, predictions.Where(taskNameIn: new[] { "Tax Classification", "1040 Document Extraction" }));
        Assert.Empty(predictions.Where(taskIn: new List<IndicoToolkit.Results.Tasks.Task> { }));
        Assert.Empty(predictions.Where(taskTypeIn: new List<TaskType> { }));
        Assert.Empty(predictions.Where(taskNameIn: new List<string> { }));
    }

    [Fact]
    public void TestWhereReview()
    {
        var predictions = Predictions;
        var classification = predictions.First();
        var firstName = predictions.Skip(1).First();
        var lastName = predictions.Skip(2).First();
        Assert.Equal(predictions, predictions.Where(review: null));
        Assert.Equal(new() { classification }, predictions.Where(p => p.Review == null));
        Assert.Equal(new() { firstName }, predictions.Where(review: autoReview));
        Assert.Equal(new() { lastName }, predictions.Where(reviewType: ReviewType.MANUAL));
    }

    [Fact]
    public void TestWhereReviewIn()
    {
        var predictions = Predictions;
        var classification = predictions.First();
        var firstName = predictions.Skip(1).First();
        var lastName = predictions.Skip(2).First();
        Assert.Equal(new() { classification }, predictions.Where(reviewIn: new List<Review?> { null }));
        Assert.Equal(new() { classification, firstName }, predictions.Where(reviewIn: new List<Review?> { null, autoReview }));
        Assert.Equal(new() { firstName, lastName }, predictions.Where(reviewTypeIn: new[] { ReviewType.AUTO, ReviewType.MANUAL }));
        Assert.Empty(predictions.Where(reviewIn: new List<Review?> { }));
        Assert.Empty(predictions.Where(reviewTypeIn: new List<ReviewType> { }));
    }

    [Fact]
    public void TestWhereLabel()
    {
        var predictions = Predictions;
        var firstName = predictions.Extractions.First();
        Assert.Equal(new() { firstName }, predictions.Where(label: "First Name"));
    }

    [Fact]
    public void TestWhereLabelIn()
    {
        var predictions = Predictions;
        var firstName = predictions.Extractions.First();
        var lastName = predictions.Extractions.Last();
        Assert.Equal(new() { firstName, lastName }, predictions.Where(labelIn: new[] { "First Name", "Last Name" }));
    }

    [Fact]
    public void TestWhereConfidence()
    {
        var predictions = Predictions;
        var conf70 = predictions.First();
        var conf80 = predictions.Skip(1).First();
        var conf90 = predictions.Skip(2).First();
        Assert.Equal(new() { conf90 }, predictions.Where(minConfidence: 0.9));
        Assert.Equal(new() { conf80 }, predictions.Where(minConfidence: 0.75, maxConfidence: 0.85));
        Assert.Equal(new() { conf70 }, predictions.Where(maxConfidence: 0.7));
    }

    [Fact]
    public void TestWherePage()
    {
        var predictions = Predictions;
        var firstName = predictions.Extractions.First();
        Assert.Equal(new() { firstName }, predictions.Where(page: 0));
    }

    [Fact]
    public void TestWherePageIn()
    {
        var predictions = Predictions;
        var firstName = predictions.Extractions.First();
        var lastName = predictions.Extractions.Last();
        Assert.Equal(new() { firstName, lastName }, predictions.Where(pageIn: new[] { 0, 1 }));
    }

    [Fact]
    public void TestWhereAccepted()
    {
        var predictions = Predictions;
        var firstName = predictions.Extractions.First();
        var lastName = predictions.Extractions.Last();
        predictions.Unaccept();

        Assert.Empty(predictions.Where(accepted: true));
        Assert.Equal(new() { firstName, lastName }, predictions.Where(accepted: false));

        predictions.Accept();

        Assert.Empty(predictions.Where(accepted: false));
        Assert.Equal(new() { firstName, lastName }, predictions.Where(accepted: true));
    }

    [Fact]
    public void TestWhereRejected()
    {
        var predictions = Predictions;
        var firstName = predictions.Extractions.First();
        var lastName = predictions.Extractions.Last();
        predictions.Unreject();

        Assert.Empty(predictions.Where(rejected: true));
        Assert.Equal(new() { firstName, lastName }, predictions.Where(rejected: false));

        predictions.Reject();

        Assert.Empty(predictions.Where(rejected: false));
        Assert.Equal(new() { firstName, lastName }, predictions.Where(rejected: true));
    }


}
