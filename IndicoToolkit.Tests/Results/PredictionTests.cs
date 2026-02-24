using IndicoToolkit.EtlOutputs;
using IndicoToolkit.Results;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using Xunit;

namespace IndicoToolkit.Tests.Results;

public class PredictionTests
{
    private static Document document => new(
        0, "", "", false, "", "", ImmutableHashSet<string>.Empty, ImmutableHashSet<string>.Empty
    );

    private static DocumentExtraction documentExtraction => DocumentExtraction.FromJson(
        document,
        new IndicoToolkit.Results.Tasks.Task(0, "", TaskType.DOCUMENT_EXTRACTION),
        null,
        JObject.Parse(@"
            {
                ""label"": ""Agency"",
                ""confidence"": {""Agency"": 0.99},
                ""text"": ""ORIGINAL_OCR"",
                ""spans"": [{""page_num"": 0, ""start"": 35, ""end"": 61}],
                ""groupings"": [],
                ""normalized"": {
                    ""text"": ""ORIGINAL_GENAI"",
                    ""formatted"": ""NORMALIZED"",
                    ""structured"": null
                }
            }
        ")
    );

    private static FormExtraction formExtraction => FormExtraction.FromJson(
        document,
        new IndicoToolkit.Results.Tasks.Task(0, "", TaskType.FORM_EXTRACTION),
        null,
        JObject.Parse(@"
            {
                ""type"": ""text"",
                ""label"": ""Agency"",
                ""confidence"": {""Agency"": 1.0},
                ""text"": ""ORIGINAL_OCR"",
                ""page_num"": 0,
                ""top"": 201,
                ""left"": 73,
                ""right"": 1266,
                ""bottom"": 448,
                ""normalized"": {
                    ""text"": ""ORIGINAL_GENAI"",
                    ""formatted"": ""NORMALIZED"",
                    ""structured"": null
                }
            }
        ")
    );

    private static Summarization summarization => Summarization.FromJson(
        document,
        new IndicoToolkit.Results.Tasks.Task(0, "", TaskType.GENAI_SUMMARIZATION),
        null,
        JObject.Parse(@"
            {
                ""label"": ""Accounting Summary"",
                ""confidence"": {""Accounting Summary"": 1.0},
                ""text"": ""Vendor: HubSpot, Inc.\\nDate: 06/21/2016\\nNumber: 579266\\nTotal: $1,301.56\\nBilling Address:\\n186 SOUTH STREET\\nSUITE 400\\nBoston MA 02111\\nUS\\nLine Items:\\n- HubSpot Enterprise (1): $1,200.00\\n- Included Contacts (10): $0.00\\n- Enterprise Contacts - Per 1000 (5): $25.00 [1]"",
                ""citations"": [
                    {
                        ""document"": {""page_num"": 0, ""start"": 0, ""end"": 758},
                        ""response"": {""start"": 285, ""end"": 288}
                    }
                ]
            }
        ")
    );

    private static Unbundling unbundling => Unbundling.FromJson(
        document,
        new IndicoToolkit.Results.Tasks.Task(0, "", TaskType.UNBUNDLING),
        null,
        JObject.Parse(@"
            {
                ""label"": ""Invoice"",
                ""confidence"": {
                    ""Invoice"": 0.975,
                    ""Purchase Order"": 0.0245,
                },
                ""spans"": [{""page_num"": 0, ""start"": 0, ""end"": 762}]
            }
        ")
    );

    [Fact]
    public void TestNextGroup()
    {
        var group = new Group(123, "Linked Label", 0);
        var nextGroup = new Group(123, "Linked Label", 1);

        Assert.Equal(nextGroup, group.Next());
    }

    [Fact]
    public void TestPage()
    {
        Assert.Equal(0, documentExtraction.Page);
        Assert.Equal(0, formExtraction.Page);
        Assert.Equal(0, summarization.Page);
        Assert.Equal(ImmutableList.Create(0), unbundling.Pages);
    }

    [Theory]
    [InlineData("documentExtraction")]
    [InlineData("formExtraction")]
    [InlineData("summarization")]
    [InlineData("unbundling")]
    public void TestConfidence(string predictionType)
    {
        Prediction prediction = predictionType switch
        {
            "documentExtraction" => documentExtraction,
            "formExtraction" => formExtraction,
            "summarization" => summarization,
            "unbundling" => unbundling,
            _ => throw new System.ArgumentException($"Unknown prediction type: {predictionType}")
        };

        prediction.Confidence = 0.5;
        Assert.Equal(0.5, prediction.Confidence);
        Assert.Equal(0.5, Utils.Get<double>(prediction.ToJson(), "confidence", prediction.Label));
    }

    [Theory]
    [InlineData("documentExtraction")]
    [InlineData("formExtraction")]
    [InlineData("summarization")]
    public void TestAccept(string extractionType)
    {
        Extraction extraction = extractionType switch
        {
            "documentExtraction" => documentExtraction,
            "formExtraction" => formExtraction,
            "summarization" => summarization,
            _ => throw new System.ArgumentException($"Unknown extraction type: {extractionType}")
        };

        var changes = extraction.ToJson();
        Assert.False(changes.ContainsKey("accepted"));
        Assert.False(changes.ContainsKey("rejected"));

        extraction.Reject();
        extraction.Accept();
        Assert.True(extraction.Accepted);
        Assert.False(extraction.Rejected);

        changes = extraction.ToJson();
        Assert.True(changes.ContainsKey("accepted"));
        Assert.False(changes.ContainsKey("rejected"));
        Assert.True(changes.Value<bool>("accepted"));

        extraction.Unaccept();
        Assert.False(extraction.Accepted);
    }

    [Theory]
    [InlineData("documentExtraction")]
    [InlineData("formExtraction")]
    [InlineData("summarization")]
    public void TestReject(string extractionType)
    {
        Extraction extraction = extractionType switch
        {
            "documentExtraction" => documentExtraction,
            "formExtraction" => formExtraction,
            "summarization" => summarization,
            _ => throw new System.ArgumentException($"Unknown extraction type: {extractionType}")
        };

        var changes = extraction.ToJson();
        Assert.False(changes.ContainsKey("accepted"));
        Assert.False(changes.ContainsKey("rejected"));

        extraction.Accept();
        extraction.Reject();
        Assert.True(extraction.Rejected);
        Assert.False(extraction.Accepted);

        changes = extraction.ToJson();
        Assert.True(changes.ContainsKey("rejected"));
        Assert.False(changes.ContainsKey("accepted"));
        Assert.True(changes.Value<bool>("rejected"));

        extraction.Unreject();
        Assert.False(extraction.Rejected);
    }

    [Theory]
    [InlineData("documentExtraction")]
    [InlineData("formExtraction")]
    public void TestText(string extractionType)
    {
        Extraction extraction = extractionType switch
        {
            "documentExtraction" => documentExtraction,
            "formExtraction" => formExtraction,
            _ => throw new System.ArgumentException($"Unknown extraction type: {extractionType}")
        };

        var changes = extraction.ToJson();
        Assert.Equal("ORIGINAL_OCR", Utils.Get<string>(changes, "text"));
        Assert.Equal("ORIGINAL_GENAI", Utils.Get<string>(changes, "normalized", "text"));
        Assert.Equal("NORMALIZED", Utils.Get<string>(changes, "normalized", "formatted"));

        extraction.Text = "UPDATED";
        changes = extraction.ToJson();
        Assert.Equal("UPDATED", changes["text"]);
        Assert.Equal("UPDATED", Utils.Get<string>(changes, "normalized", "text"));
        Assert.Equal("UPDATED", Utils.Get<string>(changes, "normalized", "formatted"));
    }

    [Fact]
    public void TestTextCheckbox()
    {
        var checkbox = formExtraction;
        checkbox.Type = FormExtractionType.CHECKBOX;
        checkbox.Checked = false;

        var changes = checkbox.ToJson();
        Assert.Equal("Unchecked", Utils.Get<string>(changes, "text"));
        Assert.Equal("Unchecked", Utils.Get<string>(changes, "normalized", "text"));
        Assert.Equal("Unchecked", Utils.Get<string>(changes, "normalized", "formatted"));
        Assert.False(Utils.Get<bool>(changes, "normalized", "structured", "checked"));

        checkbox.Checked = true;
        changes = checkbox.ToJson();
        Assert.Equal("Checked", Utils.Get<string>(changes, "text"));
        Assert.Equal("Checked", Utils.Get<string>(changes, "normalized", "text"));
        Assert.Equal("Checked", Utils.Get<string>(changes, "normalized", "formatted"));
        Assert.True(Utils.Get<bool>(changes, "normalized", "structured", "checked"));
    }

    [Fact]
    public void TestTextSignature()
    {
        var signature = formExtraction;
        signature.Type = FormExtractionType.SIGNATURE;
        signature.Signed = false;

        var changes = signature.ToJson();
        Assert.Equal("ORIGINAL_OCR", Utils.Get<string>(changes, "text"));
        Assert.Equal("ORIGINAL_GENAI", Utils.Get<string>(changes, "normalized", "text"));
        Assert.Equal("Unsigned", Utils.Get<string>(changes, "normalized", "formatted"));
        Assert.False(Utils.Get<bool>(changes, "normalized", "structured", "signed"));

        signature.Signed = true;
        changes = signature.ToJson();
        Assert.Equal("ORIGINAL_OCR", Utils.Get<string>(changes, "text"));
        Assert.Equal("ORIGINAL_GENAI", Utils.Get<string>(changes, "normalized", "text"));
        Assert.Equal("Signed", Utils.Get<string>(changes, "normalized", "formatted"));
        Assert.True(Utils.Get<bool>(changes, "normalized", "structured", "signed"));
    }

    [Fact]
    public void TestSpans()
    {
        var extraction = documentExtraction;
        var oldSpan = extraction.Span;
        var newSpan = oldSpan with { Page = 1 };

        extraction.Spans.Add(newSpan);
        Assert.Equal(oldSpan, extraction.Span);
        Assert.Equal(new List<Span> { oldSpan, newSpan }, extraction.Spans);
        Assert.Equal(2, Utils.Get<JArray>(extraction.ToJson(), "spans").Count);

        extraction.Span = newSpan;
        Assert.Equal(newSpan, extraction.Span);
        Assert.Equal(new List<Span> { newSpan }, extraction.Spans);
        Assert.Single(extraction.ToJson().Value<JArray>("spans"));

        extraction.Spans = new List<Span>();
        Assert.True(extraction.Span.IsNull);
        Assert.Equal(Span.NULL_SPAN, extraction.Span);
        Assert.Empty(extraction.ToJson().Value<JArray>("spans"));

        extraction.Span = Span.NULL_SPAN;
        Assert.Empty(extraction.Spans);
        Assert.True(extraction.Span.IsNull);
        Assert.Equal(Span.NULL_SPAN, extraction.Span);
        Assert.Empty(extraction.ToJson().Value<JArray>("spans"));
    }

    [Fact]
    public void TestCitations()
    {
        var extraction = summarization;
        var oldCitation = extraction.Citation;
        var oldSpan = extraction.Span;
        Assert.Equal(oldSpan, oldCitation.Span);

        var newSpan = oldSpan with { Page = 1 };
        var newCitation = oldCitation with { Start = 0, Span = newSpan };
        var oldCitationNewSpan = oldCitation with { Span = newSpan };

        extraction.Citations.Add(newCitation);
        Assert.Equal(new List<Citation> { oldCitation, newCitation }, extraction.Citations);
        Assert.Equal(oldCitation, extraction.Citation);
        Assert.Equal(new List<Span> { oldSpan, newSpan }, extraction.Spans);
        Assert.Equal(oldSpan, extraction.Span);
        Assert.Equal(2, Utils.Get<JArray>(extraction.ToJson(), "citations").Count);

        extraction.Span = newSpan;
        Assert.Equal(new List<Citation> { oldCitationNewSpan }, extraction.Citations);
        Assert.Equal(new List<Span> { newSpan }, extraction.Spans);
        Assert.Equal(newSpan, extraction.Span);
        Assert.Single(Utils.Get<JArray>(extraction.ToJson(), "citations"));

        extraction.Citations = new List<Citation> { oldCitation, newCitation };
        extraction.Citation = newCitation;
        Assert.Equal(new List<Citation> { newCitation }, extraction.Citations);
        Assert.Equal(new List<Span> { newSpan }, extraction.Spans);
        Assert.Equal(newSpan, extraction.Span);
        Assert.Single(Utils.Get<JArray>(extraction.ToJson(), "citations"));

        extraction.Citations = new List<Citation>();
        Assert.True(extraction.Citation.IsNull);
        Assert.Equal(Citation.NULL_CITATION, extraction.Citation);
        Assert.True(extraction.Span.IsNull);
        Assert.Equal(Span.NULL_SPAN, extraction.Span);
        Assert.Empty(Utils.Get<JArray>(extraction.ToJson(), "citations"));

        extraction.Citation = Citation.NULL_CITATION;
        Assert.Empty(extraction.Citations);
        Assert.True(extraction.Citation.IsNull);
        Assert.Equal(Citation.NULL_CITATION, extraction.Citation);
        Assert.Empty(Utils.Get<JArray>(extraction.ToJson(), "citations"));
    }
}
